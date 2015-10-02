using System;
using System.Text;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DeltaDNA.Messaging;
using DeltaDNA.Notifications;

namespace DeltaDNA
{
	public class DDNA : Singleton<DDNA>
	{
		static readonly string PF_KEY_USER_ID = "DDSDK_USER_ID";
		static readonly string PF_KEY_FIRST_RUN = "DDSDK_FIRST_RUN";
		static readonly string PF_KEY_HASH_SECRET = "DDSDK_HASH_SECRET";
		static readonly string PF_KEY_CLIENT_VERSION = "DDSDK_CLIENT_VERSION";
		static readonly string PF_KEY_PUSH_NOTIFICATION_TOKEN = "DDSDK_PUSH_NOTIFICATION_TOKEN";
		static readonly string PF_KEY_ANDROID_REGISTRATION_ID = "DDSDK_ANDROID_REGISTRATION_ID";

		static readonly string EV_KEY_NAME = "eventName";
		static readonly string EV_KEY_USER_ID = "userID";
		static readonly string EV_KEY_SESSION_ID = "sessionID";
		static readonly string EV_KEY_TIMESTAMP = "eventTimestamp";
		static readonly string EV_KEY_PARAMS = "eventParams";

		static readonly string EP_KEY_PLATFORM = "platform";
		static readonly string EP_KEY_SDK_VERSION = "sdkVersion";

		public static readonly string AUTO_GENERATED_USER_ID = null;

		private bool initialised = false;
		private string collectURL;
		private string engageURL;

		private EventStore eventStore = null;
		private EngageArchive engageArchive = null;
		private EventBuilder _launchNotificationEventParams = null;
		
		private static Func<DateTime?> TimestampFunc = new Func<DateTime?>(DefaultTimestampFunc); 

		private static object _lock = new object();

		protected DDNA()
		{
			this.Settings = new Settings();	// default configuration
					
			this.Transaction = new TransactionBuilder(this);

			// WebGL builds in an iFrame have problems accessing Application.persistentDataPath
			string eventStorePath = null;
			string archiveStorePath = null;
			#if !UNITY_WEBPLAYER && !UNITY_WEBGL
			eventStorePath = Settings.EVENT_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);
			archiveStorePath = Settings.ENGAGE_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);			
			#endif

			this.eventStore = new EventStore(eventStorePath);
			this.engageArchive = new EngageArchive(archiveStorePath);
		}
		
		void Awake()
		{
			// Attach additional behaviours as children of this gameObject
			GameObject iosNotifications = new GameObject();			
			this.IosNotifications = iosNotifications.AddComponent<IosNotifications>();
			iosNotifications.transform.parent = gameObject.transform;
			
			GameObject androidNotifications = new GameObject();
			this.AndroidNotifications = androidNotifications.AddComponent<AndroidNotifications>();
			androidNotifications.transform.parent = gameObject.transform;
			
			//GameObject ads = new GameObject();
			//this.Ads = ads.AddComponent<Ads>();
			//ads.transform.parent = gameObject.transform;
			this.Ads = new Ads();
		}

		#region Client Interface

		/// <summary>
		/// Initialises the SDK.  Call before sending events or making engagements.
		/// </summary>
		/// <param name="envKey">The unique environment key for this game environment.</param>
		/// <param name="collectURL">The Collect URL for this game.</param>
		/// <param name="engageURL">The Engage URL for this game.</param>
		/// <param name="userID">The user id for the player, if set to AUTO_GENERATED_USER_ID we create one for you.</param>
		[Obsolete("Prefer 'StartSDK' instead, Init will be removed in a future update.")]
		public void Init(string envKey, string collectURL, string engageURL, string userID)
		{
			StartSDK(envKey, collectURL, engageURL, userID);
		}

		/// <summary>
		/// Starts the SDK.  Call before sending events or making engagements.
		/// </summary>
		/// <param name="envKey">The unique environment key for this game environment.</param>
		/// <param name="collectURL">The Collect URL for this game.</param>
		/// <param name="engageURL">The Engage URL for this game.</param>
		/// <param name="userID">The user id for the player, if set to AUTO_GENERATED_USER_ID we create one for you.</param>
		public void StartSDK(string envKey, string collectURL, string engageURL, string userID)
		{
			lock (_lock)
			{
				Logger.LogInfo("Starting DDNA SDK");
								
				if (!PlayerPrefs.HasKey(PF_KEY_USER_ID)) {
					Logger.LogDebug("No UserID key found in PlayerPrefs, starting from fresh.");
				}
			
				SetUserID(userID);

				this.EnvironmentKey = envKey;
				this.CollectURL = collectURL;	// TODO: warn if no http is present, prepend it, although we support both
				this.EngageURL = engageURL;
				this.Platform = ClientInfo.Platform;
				this.SessionID = GetSessionID();

				this.initialised = true;

				if (_launchNotificationEventParams != null) {
					RecordEvent("notificationOpened", _launchNotificationEventParams);
					_launchNotificationEventParams = null;
				}
				
				TriggerDefaultEvents();

				// Setup automated event uploads
				if (Settings.BackgroundEventUpload && !IsInvoking("Upload"))
				{
					InvokeRepeating("Upload", Settings.BackgroundEventUploadStartDelaySeconds, Settings.BackgroundEventUploadRepeatRateSeconds);
				}
			}
		}

		/// <summary>
		/// Changes the session ID for the current User.
		/// </summary>
		public void NewSession()
		{
			this.SessionID = GetSessionID();
		}

		/// <summary>
		/// Sends a 'gameEnded' event to Collect, disables background uploads.
		/// </summary>
		public void StopSDK()
		{
			lock (_lock) 
			{
				if (this.initialised) {
					Logger.LogInfo("Stopping DDNA SDK");
					RecordEvent("gameEnded");
					CancelInvoke();
					Upload();
					this.initialised = false;
				} else {
					Logger.LogDebug("SDK not running");
				}
			}
		}

		/// <summary>
		///	Sends an event to Collect, with no additional event parameters.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		[Obsolete("Prefer 'RecordEvent' instead, Trigger will be removed in a future update.")]
		public void TriggerEvent(string eventName)
		{
			RecordEvent(eventName, new Dictionary<string, object>());
		}

		/// <summary>
		///	Sends an event to Collect, with no additional event parameters.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		public void RecordEvent(string eventName)
		{
			RecordEvent(eventName, new Dictionary<string, object>());
		}

		/// <summary>
		/// Sends an event to Collect.
		/// </summary>
		/// <param name="eventName">Name of the event schema.</param>
		/// <param name="eventParams">An EventBuilder that describes the event params for the event.</param>
		[Obsolete("Prefer 'RecordEvent' instead, Trigger will be removed in a future update.")]
		public void TriggerEvent(string eventName, EventBuilder eventParams)
		{
			RecordEvent(eventName, eventParams == null ? new Dictionary<string, object>() : eventParams.ToDictionary());
		}

		/// <summary>
		/// Sends an event to Collect.
		/// </summary>
		/// <param name="eventName">Name of the event schema.</param>
		/// <param name="eventParams">An EventBuilder that describes the event params for the event.</param>
		public void RecordEvent(string eventName, EventBuilder eventParams)
		{
			RecordEvent(eventName, eventParams == null ? new Dictionary<string, object>() : eventParams.ToDictionary());
		}

		/// <summary>
		/// Sends an event to Collect.
		/// </summary>
		/// <param name="eventName">Name of the event schema.</param>
		/// <param name="eventParams">Event parameters for the event.</param>
		[Obsolete("Prefer 'RecordEvent' instead, Trigger will be removed in a future update.")]
		public void TriggerEvent(string eventName, Dictionary<string, object> eventParams)
		{
			RecordEvent(eventName, eventParams);
		}

		/// <summary>
		/// Sends an event to Collect.
		/// </summary>
		/// <param name="eventName">Name of the event schema.</param>
		/// <param name="eventParams">Event parameters for the event.</param>
		public void RecordEvent(string eventName, Dictionary<string, object> eventParams)
		{
			if (!this.initialised)
			{
				Logger.LogError("You must first start the SDK via the StartSDK method.");
				return;
			}

			// the header for every event is eventName, userID, sessionID and timestamp
			var eventRecord = new Dictionary<string, object>();
			eventRecord[EV_KEY_NAME] 		= eventName;
			eventRecord[EV_KEY_USER_ID] 	= this.UserID;
			eventRecord[EV_KEY_SESSION_ID] 	= this.SessionID;
			
			// Collect will insert it's own timestamp if null is returned by the timestamp function
			string currentTimestamp = GetCurrentTimestamp();
			if (!String.IsNullOrEmpty(currentTimestamp)) {
				eventRecord[EV_KEY_TIMESTAMP] 	= GetCurrentTimestamp();
			}

			// every template should support sdkVersion and platform in it's event params
			if (!eventParams.ContainsKey(EP_KEY_PLATFORM))
			{
				eventParams.Add(EP_KEY_PLATFORM, this.Platform);
			}

			if (!eventParams.ContainsKey(EP_KEY_SDK_VERSION))
			{
				eventParams.Add(EP_KEY_SDK_VERSION, Settings.SDK_VERSION);
			}

			eventRecord[EV_KEY_PARAMS] = eventParams;

			if (String.IsNullOrEmpty(this.UserID))
			{

			}
			else if (!this.eventStore.Push(MiniJSON.Json.Serialize(eventRecord)))
			{
				Logger.LogWarning("Event Store full, unable to handle event.");
			}
		}

		/// <summary>
		/// Makes an Engage request.  The result of the engagement will be passed as a dictionary object to your callback method.
		/// </summary>
		/// <param name="decisionPoint">The decision point the request is for, must match the string in Portal.</param>
		/// <param name="engageParams">Additional parameters for the engagement.</param>
		/// <param name="callback">Method called with the response from our server.</param>
		public void RequestEngagement(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
		{
			if (!this.initialised)
			{
				Logger.LogError("You must first start the SDK via the StartSDK method.");
				return;
			}

			if (String.IsNullOrEmpty(this.EngageURL))
			{
				Logger.LogWarning("Engage URL not configured, can not make engagement.");
				return;
			}

			if (String.IsNullOrEmpty(decisionPoint))
			{
				Logger.LogWarning("No decision point set, can not make engagement.");
				return;
			}

			StartCoroutine(EngageCoroutine(decisionPoint, engageParams, callback));
		}
		
		/// <summary>
		/// Requests an image based Engagement for popping up on screen.  This is a convience around RequestEngagement
		/// that loads the image resource automatically from the original engage request.  Register a function with the
		/// Popup's AfterLoad event to be notified when the image has be been downloaded from our server.
		/// </summary>
		/// <param name="decisionPoint">The decision point the request is for, must match the string in Portal.</param>
		/// <param name="engageParams">Additional parameters for the engagement.</param>
		/// <param name="popup">A Popup object to display the image.</param>
		public void RequestImageMessage(
			string decisionPoint,
			Dictionary<string, object> engageParams,
			IPopup popup)
		{
			this.RequestImageMessage(decisionPoint, engageParams, popup, null);	
		}

		/// <summary>
		/// Requests an image based Engagement for popping up on screen.  This is a convience around RequestEngagement
		/// that loads the image resource automatically from the original engage request.  Register a function with the
		/// Popup's AfterLoad event to be notified when the image has be been downloaded from our server.
		/// </summary>
		/// <param name="decisionPoint">The decision point the request is for, must match the string in Portal.</param>
		/// <param name="engageParams">Additional parameters for the engagement.</param>
		/// <param name="popup">A Popup object to display the image.</param>
		/// <param name="callback">A callback with the engage response as the parameter.</param>
		public void RequestImageMessage(
			string decisionPoint,
			Dictionary<string, object> engageParams,
			IPopup popup,
			Action<Dictionary<string, object>> callback)
		{
            Action<Dictionary<string, object>> imageCallback = (response) =>
            {
                if (response != null)
                {
                    if (response.ContainsKey("image"))
                    {
                        var image = response["image"] as Dictionary<string, object>;
                        popup.Prepare(image);
                    }
                    if (callback != null) callback(response);
                }
            };

			RequestEngagement(decisionPoint, engageParams, imageCallback);
		}
		
		/// <summary>
		/// Records that the game received a push notification.  It is safe to call this method
		/// Before calling StartSDK, the 'notificationOpened' event will be sent at that time. 
		/// </summary>
		/// <param name="payload">Payload.</param>
		public void RecordPushNotification(Dictionary<string, object> payload)
		{
			Logger.LogDebug("Received push notification: "+payload);
			
			EventBuilder eventParams = new EventBuilder();			
			try {
				if (payload.ContainsKey("_ddId")) {
					eventParams.AddParam("notificationId", Convert.ToInt32(payload["_ddId"]));
				}
				if (payload.ContainsKey("_ddName")) {
					eventParams.AddParam("notificationName", payload["_ddName"]);
				}
				if (payload.ContainsKey("_ddLaunch")) {
					eventParams.AddParam("notificationLaunch", Convert.ToBoolean(payload["_ddLaunch"]));
				}
			} catch (Exception ex) {
				Logger.LogError("Error parsing push notification payload: "+ex);
			}
			
			if (this.IsInitialised) {
				this.RecordEvent("notificationOpened", eventParams);
			} else {
				this._launchNotificationEventParams = eventParams;
			}
		}

		/// <summary>
		/// Uploads waiting events to our Collect service.  By default this is called automatically in the
		/// background.  If you disable auto uploading via <see cref="Settings.BackgroundEventUpload"/> you
		/// will need to call this method yourself periodically.
		/// </summary>
		public void Upload()
		{
			if (!this.initialised)
			{
				Logger.LogDebug("You must first start the SDK via the StartSDK method.");
				return;
			}

			if (this.IsUploading)
			{
				Logger.LogWarning("Event upload already in progress, try again later.");
				return;
			}

			StartCoroutine(UploadCoroutine());
		}

		/// <summary>
		/// Sets the logging level. Choices are ERROR, WARNING, INFO or DEBUG. Default is WARNING.
		/// </summary>
        public void SetLoggingLevel(Logger.Level level)
        {
            Logger.SetLogLevel(level);
        }

		/// <summary>
		/// Controls default behaviour of the SDK.  Set prior to initialisation.
		/// </summary>
		public Settings Settings { get; set; }
		
		/// <summary>
		/// Helper for iOS push notifications.
		/// </summary>
		public IosNotifications IosNotifications { get; private set; }
		
		/// <summary>
		/// Helper for Android push notifications.
		/// </summary>
		public AndroidNotifications AndroidNotifications { get; private set; }
		
		/// <summary>
		/// Access the ad behaviour.
		/// </summary>
		public Ads Ads { get; private set; }

		/// <summary>
		/// Helper for building common transaction type events.
		/// </summary>
		/// <value>The transaction.</value>
		public TransactionBuilder Transaction { get; private set; }

		/// <summary>
		/// Clears the persistent data such as user id.  Useful for testing purposes.
		/// </summary>
		public void ClearPersistentData()
		{
			// PlayerPrefs
			PlayerPrefs.DeleteKey(PF_KEY_USER_ID);
			PlayerPrefs.DeleteKey(PF_KEY_FIRST_RUN);
			PlayerPrefs.DeleteKey(PF_KEY_HASH_SECRET);
			PlayerPrefs.DeleteKey(PF_KEY_CLIENT_VERSION);
			PlayerPrefs.DeleteKey(PF_KEY_PUSH_NOTIFICATION_TOKEN);
			PlayerPrefs.DeleteKey(PF_KEY_ANDROID_REGISTRATION_ID);

			this.eventStore.ClearAll();
			this.engageArchive.Clear();
		}
		
		/// <summary>
		/// Controls if the device is used as the event timestamp source or our Collect server.  
		/// Using the device time (the default) ensures the events will have the correct timestamp
		/// while no internet connection is available to upload events.  But the device time relies 
		/// on the user having set their system clock correctly.  If you disable the device 
		/// timestamp, our Collect server will inject the time it received the event.  If you 
		/// require more control over the timestamp, use <see cref="SetTimestampFunc"/>. 
		/// </summary>
		/// <param name="useCollect">If set to <c>true</c> use Collect server for event timestamps.</param>
		public void UseCollectTimestamp(bool useCollect) {
			if (!useCollect) {
				SetTimestampFunc(DefaultTimestampFunc);
			} else {
				SetTimestampFunc(() => { return null; });
			}
		}
		
		/// <summary>
		/// If more control is required over the event timestamp source, you can override the default
		/// behaviour with a function that returns a DateTime. 
		/// </summary>
		/// <param name="TimestampFunc">Timestamp func.</param>
		public void SetTimestampFunc(Func<DateTime?> TimestampFunc) 
		{
			DDNA.TimestampFunc = TimestampFunc;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the environment key.
		/// </summary>
		public string EnvironmentKey { get; private set; }

		/// <summary>
		/// Gets the Collect URL.
		/// </summary>
		public string CollectURL {
			get { return this.collectURL; }
			private set { this.collectURL = ValidateURL(value); }
		}

		/// <summary>
		/// Gets the engage URL.
		/// </summary>
		public string EngageURL {
			get { return this.engageURL; }
			private set { this.engageURL = ValidateURL(value); }
		}

		/// <summary>
		/// Gets the session ID.
		/// </summary>
		public string SessionID { get; private set; }

		/// <summary>
		/// Gets the platform.
		/// </summary>
		public string Platform { get; private set; }

		/// <summary>
		/// Gets the user ID.
		/// </summary>
		public string UserID
		{
			get
			{
				string v = PlayerPrefs.GetString(PF_KEY_USER_ID, null);
				if (String.IsNullOrEmpty(v))
				{
					return null;
				}
				return v;
			}
			private set
			{
				if (!String.IsNullOrEmpty(value))
				{
					PlayerPrefs.SetString(PF_KEY_USER_ID, value);
					PlayerPrefs.Save();
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is initialised.
		/// </summary>
		public bool IsInitialised { get { return this.initialised; }}

		/// <summary>
		/// Gets a value indicating whether an event upload is in progress.
		/// </summary>
		public bool IsUploading { get; private set; }

		#endregion

		#region Client Configuration

		/// <summary>
		/// To enable hashing of your event and engage data, set this value to your
		/// unique hash secret.  You must also enable hashing for the environment.
		/// To disable hashing set it to null, which is the default.  This must be
		/// set before calling <see cref="Init"/>.
		/// </summary>
		public string HashSecret
		{
			get
			{
				string v = PlayerPrefs.GetString(PF_KEY_HASH_SECRET, null);
				if (String.IsNullOrEmpty(v))
				{
					Logger.LogDebug("Event hashing not enabled.");
					return null;
				}
				return v;
			}
			set
			{
				Logger.LogDebug("Setting Hash Secret '"+value+"'");
				PlayerPrefs.SetString(PF_KEY_HASH_SECRET, value);
				PlayerPrefs.Save();
			}
		}

		/// <summary>
		/// A version string for your game that will be reported to us.  This must
		/// be set before calling <see cref="Init"/>.
		/// </summary>
		public string ClientVersion
		{
			get
			{
				string v = PlayerPrefs.GetString(PF_KEY_CLIENT_VERSION, null);
				if (String.IsNullOrEmpty(v))
				{
					Logger.LogWarning("No client game version set.");
					return null;
				}
				return v;
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					Logger.LogDebug("Setting ClientVersion '"+value+"'");
					PlayerPrefs.SetString(PF_KEY_CLIENT_VERSION, value);
					PlayerPrefs.Save();
				}
			}
		}

		/// <summary>
		/// The push notification token from Apple that is associated with this device if
		/// it's running on the iOS platform.  This must be set before calling <see cref="Init"/>.
		/// </summary>
		public string PushNotificationToken
		{
			get
			{
				string v = PlayerPrefs.GetString(PF_KEY_PUSH_NOTIFICATION_TOKEN, null);
				if (String.IsNullOrEmpty(v))
				{
					return null;
				}
				return v;
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					PlayerPrefs.SetString(PF_KEY_PUSH_NOTIFICATION_TOKEN, value);
					PlayerPrefs.Save();
				}
			}
		}

		/// <summary>
		/// The Android registration ID that is associated with this device if it's running
		/// on the Android platform.  This must be set before calling <see cref="Init"/>.
		/// </summary>
		public string AndroidRegistrationID
		{
			get
			{
				string v = PlayerPrefs.GetString(PF_KEY_ANDROID_REGISTRATION_ID, null);
				if (String.IsNullOrEmpty(v))
				{
					return null;
				}
				return v;
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					PlayerPrefs.SetString(PF_KEY_ANDROID_REGISTRATION_ID, value);
					PlayerPrefs.Save();
				}
			}
		}

		#endregion

		#region Private Helpers

		public override void OnDestroy()
		{
			if (this.eventStore != null) this.eventStore.Dispose();
			if (this.engageArchive != null) this.engageArchive.Save();
			PlayerPrefs.Save();
			base.OnDestroy();
		}

		private string GetSessionID()
		{
			return Guid.NewGuid().ToString();
		}

		private string GetUserID()
		{
			// see if this game ran with the previous SDK and look for
			// a user id.
			#if !UNITY_WEBPLAYER && !UNITY_WEBGL
			string legacySettingsPath = Settings.LEGACY_SETTINGS_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);
			if (Utils.FileExists(legacySettingsPath))
			{
				Logger.LogDebug("Found a legacy file in "+legacySettingsPath);
				using (Stream fs = Utils.OpenStream(legacySettingsPath))
				{
					try
					{
						var bytes = new List<byte>();
						byte[] buffer = new byte[1024];
						while (fs.Read(buffer, 0, buffer.Length) > 0)
						{
							bytes.AddRange(buffer);
						}
						byte[] byteArray = bytes.ToArray();
						string json = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
						var settings = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
						if (settings.ContainsKey("userID"))
						{
							Logger.LogDebug("Found a legacy user id for player");
							return settings["userID"] as string;
						}
					}
					catch (Exception e)
					{
						Logger.LogWarning("Problem reading legacy user id: "+e.Message);
					}
				}
			}
			#endif

			Logger.LogDebug("Creating a new user id for player");
			return Guid.NewGuid().ToString();
		}
		
		private static DateTime? DefaultTimestampFunc()
		{
			return DateTime.UtcNow;
		}

		private static string GetCurrentTimestamp()
		{
			DateTime? dt = TimestampFunc();
			if (dt.HasValue) {
				String ts = dt.Value.ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
				// Fix for millisecond timestamp format bug seen on Android.
				if (ts.EndsWith(".1000")) {
					ts.Replace(".1000", ".999");
				}
				return ts;
			}
			return null;	// Collect will insert a timestamp for us.
		}

		private IEnumerator UploadCoroutine()
		{
			this.IsUploading = true;

			try
			{
				// Swap over event queue.
				this.eventStore.Swap();

				// Create bulk event message to post.
				List<string> events = eventStore.Read();

				if (events != null && events.Count > 0)
				{
					Logger.LogDebug("Starting event upload.");

                    Action<bool, int> postCb = (succeeded, statusCode) =>
                    {
                        if (succeeded)
                        {
                            Logger.LogDebug("Event upload successful.");
                            this.eventStore.ClearOut();
                        }
                        else if (statusCode == 400) {
                        	Logger.LogDebug("Collect rejected events, possible corruption.");
                        	this.eventStore.ClearOut();
                        }
                        else {
                            Logger.LogWarning("Event upload failed - try again later.");
                        }
                    };

					yield return StartCoroutine(PostEvents(events.ToArray(), postCb));
				}
			}
			finally
			{
				this.IsUploading = false;
			}
		}

		private IEnumerator EngageCoroutine(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
		{
			Logger.LogDebug("Starting engagement for '"+decisionPoint+"'");

			Dictionary<string, object> engageRequest = new Dictionary<string, object>()
			{
				{ "userID", this.UserID },
				{ "decisionPoint", decisionPoint },
				{ "sessionID", this.SessionID },
				{ "version", Settings.ENGAGE_API_VERSION },
				{ "sdkVersion", Settings.SDK_VERSION },
				{ "platform", this.Platform },
				{ "timezoneOffset", Convert.ToInt32(ClientInfo.TimezoneOffset) }
			};

			if (ClientInfo.Locale != null)
			{
				engageRequest.Add("locale", ClientInfo.Locale);
			}

			if (engageParams != null)
			{
				engageRequest.Add("parameters", engageParams);
			}

			string engageJSON = null;
			try
			{
				engageJSON = MiniJSON.Json.Serialize(engageRequest);
			}
			catch (Exception e)
			{
				Logger.LogWarning("Problem serialising engage request data: "+e.Message);
				yield break;
			}

            Action<string> requestCb = (response) =>
            {
                bool cachedResponse = false;
                if (response != null)
                {
                    Logger.LogDebug("Using live engagement: " + response);
                    this.engageArchive[decisionPoint] = response;
                }
                else
                {
                    if (this.engageArchive.Contains(decisionPoint))
                    {
                        Logger.LogWarning("Engage request failed, using cached response.");
                        cachedResponse = true;
                        response = this.engageArchive[decisionPoint];
                    }
                    else
                    {
                        Logger.LogWarning("Engage request failed");
                    }
                }
                Dictionary<string, object> result = MiniJSON.Json.Deserialize(response) as Dictionary<string, object>;
                if (cachedResponse)
                {
                    result["isCachedResponse"] = cachedResponse;
                }

                if (callback != null) callback(result);
            };

			yield return StartCoroutine(EngageRequest(engageJSON, requestCb));
		}

		private IEnumerator PostEvents(string[] events, Action<bool, int> resultCallback)
		{
			string bulkEvent = "{\"eventList\":[" + String.Join(",", events) + "]}";
			string url;
			if (this.HashSecret != null)
			{
				string md5Hash = GenerateHash(bulkEvent, this.HashSecret);
				url = FormatURI(Settings.COLLECT_HASH_URL_PATTERN, this.CollectURL, this.EnvironmentKey, md5Hash);
			}
			else
			{
				url = FormatURI(Settings.COLLECT_URL_PATTERN, this.CollectURL, this.EnvironmentKey, null);
			}

			int attempts = 0;
			bool succeeded = false;
			int status = 0;
			
			Action<int, string, string> completionHandler = (statusCode, data, error) => {
				if (statusCode < 400) {
					succeeded = true;
				}
				else {
					Logger.LogDebug("Error posting events: "+error+" "+data);
				}
				status = statusCode;
			};
			
			HttpRequest request = new HttpRequest(url);
			request.HTTPMethod = HttpRequest.HTTPMethodType.POST;
			request.HTTPBody = bulkEvent;
			request.setHeader("Content-Type", "application/json");
			
			while (attempts < Settings.HttpRequestMaxRetries) {
				
				yield return StartCoroutine(Network.SendRequest(request, completionHandler));
				
				if (succeeded) break;
				
				yield return new WaitForSeconds(Settings.HttpRequestRetryDelaySeconds);
				
				attempts += 1;
			}
			
			resultCallback(succeeded, status);
		}

		private IEnumerator EngageRequest(string engagement, Action<string> callback)
		{
			string url;
			if (this.HashSecret != null)
			{
				string md5Hash = GenerateHash(engagement, this.HashSecret);
				url = FormatURI(Settings.ENGAGE_HASH_URL_PATTERN, this.EngageURL, this.EnvironmentKey, md5Hash);
			}
			else
			{
				url = FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey, null);
			}
			
			HttpRequest request = new HttpRequest(url);
			request.HTTPMethod = HttpRequest.HTTPMethodType.POST;
			request.HTTPBody = engagement;
			request.setHeader("Content-Type", "application/json");

            Action<int, string, string> completionHandler = (status, response, error) =>
			{
				if (status < 400)
				{
					if (callback != null) callback(response);
				}
				else
				{
					Logger.LogDebug("Error requesting engagement: "+error+" "+response);
					if (callback != null) callback(null);
				}
			};

			yield return StartCoroutine(Network.SendRequest(request, completionHandler));
		}

		private static string FormatURI(string uriPattern, string apiHost, string envKey, string hash)
		{
			var uri = uriPattern.Replace("{host}", apiHost);
			uri = uri.Replace("{env_key}", envKey);
			uri = uri.Replace("{hash}", hash);
			return uri;
		}

		private static string ValidateURL(string url) {
			if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://")) {
				url = "http://" + url;
			}
			return url;
		}

		private static string GenerateHash(string data, string secret){
			var inputBytes = Encoding.UTF8.GetBytes(data + secret);
			var hash = Utils.ComputeMD5Hash(inputBytes);

			var sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}

			return sb.ToString();
		}

		private void SetUserID(string userID)
		{
			if (String.IsNullOrEmpty (userID))
			{
				if (String.IsNullOrEmpty (this.UserID))
				{
					// we have no user id and the we've not been given one so make one up
					string uid = GetUserID();
					Logger.LogDebug("Setting new generated user id "+uid);
					this.UserID = uid;
				}
				else
				{
					// leave the user ID alone
				}
			}
			else
			{
				if (this.UserID != userID)
				{
					// new user ID given or not been sent one yet
					PlayerPrefs.DeleteKey(PF_KEY_FIRST_RUN);
					Logger.LogDebug("Setting new provided user id "+userID);
					this.UserID = userID;
				}
				else
				{
					// leave the user ID alone
				}
			}
		}

		private void TriggerDefaultEvents()
		{
			if (Settings.OnFirstRunSendNewPlayerEvent && PlayerPrefs.GetInt(PF_KEY_FIRST_RUN, 1) > 0)
			{
				Logger.LogDebug("Sending 'newPlayer' event");

				var newPlayerParams = new EventBuilder()
					.AddParam("userCountry", ClientInfo.CountryCode);

				this.RecordEvent("newPlayer", newPlayerParams);

				PlayerPrefs.SetInt(PF_KEY_FIRST_RUN, 0);
				PlayerPrefs.Save();
			}

			if (Settings.OnInitSendGameStartedEvent)
			{
				Logger.LogDebug("Sending 'gameStarted' event");
				
				if (ClientInfo.Platform.Contains("IOS") && String.IsNullOrEmpty(this.PushNotificationToken))
				{
					Logger.LogWarning("No Apple push notification token set, sending push notifications to iOS devices will be unavailable.");
				}
				else if (ClientInfo.Platform.Contains("ANDROID") && String.IsNullOrEmpty(this.AndroidRegistrationID))
				{
					Logger.LogWarning("No Android registration id set, sending push notifications to Android devices will be unavailable.");
				}

				var gameStartedParams = new EventBuilder()
					.AddParam("clientVersion", this.ClientVersion)
					.AddParam("pushNotificationToken", this.PushNotificationToken)
					.AddParam("androidRegistrationID", this.AndroidRegistrationID)
					.AddParam("userLocale", ClientInfo.Locale);

				this.RecordEvent("gameStarted", gameStartedParams);
			}

			if (Settings.OnInitSendClientDeviceEvent)
			{
				Logger.LogDebug("Sending 'clientDevice' event");

				EventBuilder clientDeviceParams = new EventBuilder()
					.AddParam("deviceName", ClientInfo.DeviceName)
					.AddParam("deviceType", ClientInfo.DeviceType)
					.AddParam("hardwareVersion", ClientInfo.DeviceModel)
					.AddParam("operatingSystem", ClientInfo.OperatingSystem)
					.AddParam("operatingSystemVersion", ClientInfo.OperatingSystemVersion)
					.AddParam("manufacturer", ClientInfo.Manufacturer)
					.AddParam("timezoneOffset", ClientInfo.TimezoneOffset)
					.AddParam("userLanguage", ClientInfo.LanguageCode);

				this.RecordEvent("clientDevice", clientDeviceParams);
			}
		}

		#endregion

	}
}
