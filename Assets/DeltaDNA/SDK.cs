using System;
using System.Text;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if NETFX_CORE
using UnityEngine.Windows;
#endif
using DeltaDNA.Messaging;

namespace DeltaDNA
{
	public sealed class SDK : Singleton<SDK>
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

		private static object _lock = new object();

		private SDK()
		{
			this.Settings = new Settings();	// default configuration
			this.Transaction = new TransactionBuilder(this);

			//#if UNITY_WEBPLAYER

			//this.eventStore = new WebplayerEventStore();

			//#else

			this.eventStore = new EventStore(
				Settings.EVENT_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath)
			);

			//#endif

			this.engageArchive = new EngageArchive(
				Settings.ENGAGE_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath)
			);
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
				SetUserID(userID);

				this.EnvironmentKey = envKey;
				this.CollectURL = collectURL;	// TODO: warn if no http is present, prepend it, although we support both
				this.EngageURL = engageURL;
				this.Platform = ClientInfo.Platform;
				this.SessionID = GetSessionID();

				this.initialised = true;

				// must do this once we're initialised
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
			LogDebug("Stopping SDK");
			RecordEvent("gameEnded");
			CancelInvoke();
			Upload();
			this.initialised = false;
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
				throw new NotStartedException("You must first start the SDK via the StartSDK method");
			}

			// the header for every event is eventName, userID, sessionID and timestamp
			var eventRecord = new Dictionary<string, object>();
			eventRecord[EV_KEY_NAME] 		= eventName;
			eventRecord[EV_KEY_USER_ID] 	= this.UserID;
			eventRecord[EV_KEY_SESSION_ID] 	= this.SessionID;
			eventRecord[EV_KEY_TIMESTAMP] 	= GetCurrentTimestamp();

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
				LogWarning("Event Store full, unable to handle event");
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
				throw new NotStartedException("You must first start the SDK via the StartSDK method");
			}

			if (String.IsNullOrEmpty(this.EngageURL))
			{
				LogWarning("Engage URL not configured, can not make engagement.");
				return;
			}

			if (String.IsNullOrEmpty(decisionPoint))
			{
				LogWarning("No decision point set, can not make engagement.");
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
		/// <param name="callback">Optionally pass the full engage response back to the caller.</param>
		public void RequestImageMessage(
			string decisionPoint,
			Dictionary<string, object> engageParams,
			IPopup popup,
			Action<Dictionary<string, object>> callback = null)
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
		/// Uploads waiting events to our Collect service.  By default this is called automatically in the
		/// background.  If you disable auto uploading via <see cref="Settings.BackgroundEventUpload"/> you
		/// will need to call this method yourself periodically.
		/// </summary>
		public void Upload()
		{
			if (!this.initialised)
			{
				throw new NotStartedException("You must first start the SDK via the StartSDK method");
			}

			if (this.IsUploading)
			{
				LogWarning("Event upload already in progress, aborting");
				return;
			}

			StartCoroutine(UploadCoroutine());
		}

        public void SetLoggingLevel(Logger.Level level)
        {
            Logger.SetLogLevel(level);
        }

		/// <summary>
		/// Controls default behaviour of the SDK.  Set prior to initialisation.
		/// </summary>
		public Settings Settings { get; set; }

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
					LogDebug("No existing User ID found.");
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
					LogDebug("Event hashing not enabled.");
					return null;
				}
				return v;
			}
			set
			{
				LogDebug("Setting Hash Secret '"+value+"'");
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
					LogWarning("No client game version set.");
					return null;
				}
				return v;
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					LogDebug("Setting ClientVersion '"+value+"'");
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
					if (ClientInfo.Platform.Contains("IOS"))
					{
						LogWarning("No Apple push notification token set, sending push notifications to iOS devices will be unavailable.");
					}
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
					if (ClientInfo.Platform.Contains("ANDROID"))
					{
						LogWarning("No Android registration id set, sending push notifications to Android devices will be unavailable.");
					}
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
			if (this.eventStore != null && this.eventStore.GetType() == typeof(EventStore)) this.eventStore.Dispose();
			if (this.engageArchive != null) this.engageArchive.Save();
			base.OnDestroy();
		}

		private void LogDebug(string message)
		{
			if (Settings.DebugMode)
			{
				Debug.Log("[DDSDK] "+message);
			}
		}

		private void LogWarning(string message)
		{
			Debug.LogWarning("[DDSDK] "+message);
		}

		private string GetSessionID()
		{
			return Guid.NewGuid().ToString();
		}

		private string GetUserID()
		{
			//#if !UNITY_WEBPLAYER
			// see if this game ran with the previous SDK and look for
			// a user id.
			string legacySettingsPath = Settings.LEGACY_SETTINGS_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);
			if (File.Exists(legacySettingsPath))
			{
				LogDebug("Found a legacy file in "+legacySettingsPath);
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
							LogDebug("Found a legacy user id for player");
							return settings["userID"] as string;
						}
					}
					catch (Exception e)
					{
						LogWarning("Problem reading legacy user id: "+e.Message);
					}
				}
			}
			//#endif

			LogDebug("Creating a new user id for player");
			return Guid.NewGuid().ToString();
		}

		private string GetCurrentTimestamp()
		{
			return DateTime.UtcNow.ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
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

				if (events.Count > 0)
				{
					LogDebug("Starting event upload");

                    Action<bool> postCb = (succeeded) =>
                    {
                        if (succeeded)
                        {
                            LogDebug("Event upload successful");
                            this.eventStore.ClearOut();
                        }
                        else
                        {
                            LogWarning("Event upload failed - try again later");
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
			LogDebug("Starting engagement for '"+decisionPoint+"'");

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
				LogWarning("Problem serialising engage request data: "+e.Message);
				yield break;
			}

            Action<string> requestCb = (response) =>
            {
                bool cachedResponse = false;
                if (response != null)
                {
                    LogDebug("Using live engagement: " + response);
                    this.engageArchive[decisionPoint] = response;
                }
                else
                {
                    if (this.engageArchive.Contains(decisionPoint))
                    {
                        LogWarning("Engage request failed, using cached response.");
                        cachedResponse = true;
                        response = this.engageArchive[decisionPoint];
                    }
                    else
                    {
                        LogWarning("Engage request failed");
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

		private IEnumerator PostEvents(string[] events, Action<bool> resultCallback)
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
				url = FormatURI(Settings.COLLECT_URL_PATTERN, this.CollectURL, this.EnvironmentKey);
			}

			int attempts = 0;
			bool succeeded = false;

			do
			{
                Action<int, string> httpCb = (status, response) =>
                {
                    // Unity doesn't handle 100 response correctly, so you can't know
                    // if it succeeded or failed.  We can assume if no response text came back
                    // Collect was happy.
                    if (status == 200 || status == 204) succeeded = true;
                    else if (status == 100 && String.IsNullOrEmpty(response)) succeeded = true;
#if UNITY_WEBPLAYER
					// Unity Webplayer on IE will report the request to Collect as 'failed to download'
					// although Collect receives the data fine.
					else if (status == 0) { LogDebug("Webplayer ignoring bad status code"); succeeded = true; }
#endif
                    else LogDebug("Error uploading events, Collect returned: " + status + " " + response);
                };

				yield return StartCoroutine(HttpPOST(url, bulkEvent, httpCb));
				yield return new WaitForSeconds(Settings.HttpRequestRetryDelaySeconds);
			}
			while (!succeeded && ++attempts < Settings.HttpRequestMaxRetries);

			resultCallback(succeeded);
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
				url = FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey);
			}

            Action<int, string> httpCb = (status, response) =>
			{
				if (status == 200 || status == 100)
				{
					if (callback != null) callback(response);
				}
				else
				{
					LogDebug("Error requesting engagement, Engage returned: "+status);
					if (callback != null) callback(null);
				}
			};

			yield return StartCoroutine(HttpPOST(url, engagement, httpCb));
		}

		private IEnumerator HttpGET(string url, Action<int, string> responseCallback = null)
		{
			LogDebug("HttpGET " + url);

			WWW www = new WWW(url);
			yield return www;

			int statusCode = 0;
			if (www.error == null)
			{
				statusCode = 200;
				if (responseCallback != null) responseCallback(statusCode, www.text);
			}
			else
			{
				statusCode = ReadWWWResponse(www.error);
				if (responseCallback != null) responseCallback(statusCode, null);
			}
		}

		private IEnumerator HttpPOST(string url, string json, Action<int, string> responseCallback = null)
		{
			LogDebug("HttpPOST " + url + " " + json);

			WWWForm form = new WWWForm();
			var headers = form.headers;
			headers["Content-Type"] = "application/json";
			// Annoyingly when posting large amounts of data, a 100-continue
			// response is generated.  This becomes the status code of the
			// response so we can't tell if the request was successull or
			// not. You should be able to prevent this behaviour by removing
			// the expect header, but since Unity 4.3 this header is protected.
			//headers["Expect"] = "";

			byte[] bytes = Encoding.UTF8.GetBytes(json);

			// silence deprecation warning
			# if UNITY_4_5
			WWW www = new WWW(url, bytes, Utils.HashtableToDictionary<string, string>(headers));
			# else
			WWW www = new WWW(url, bytes, headers);
			# endif


			yield return www;

			int statusCode = ReadWWWStatusCode(www);

			if (www.error == null)
			{
				if (responseCallback != null) responseCallback(statusCode, www.text);
			}
			else
			{
				LogDebug("WWW.error: "+www.error);
				if (responseCallback != null) responseCallback(statusCode, null);
			}
		}

		private static int ReadWWWResponse(string response)
		{
			System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(response, @"^.*\s(\d{3})\s.*$");
			if (matches.Count > 0 && matches[0].Groups.Count > 0)
			{
				return Convert.ToInt32(matches[0].Groups[1].Value);
			}
			return 0;
		}

		private int ReadWWWStatusCode(WWW www)
		{
			// As of Unity 4.5 WWW is not great for http requests.  Reading the http status is not offically supported,
			// and although the responseHeaders generally contain the status, not all platforms have implemented this the same way.
			// If it looks like the responseHeader doesn't have a STATUS key I fall back to the official method of testing
			// WWW.error.  If this is empty we can assume success i.e. 200 else the error text might have a status code in it
			// to return.

			int statusCode = 0;
			#if UNITY_ANDROID
			// see http://issuetracker.unity3d.com/issues/www-dot-responseheaders-status-key-is-null-in-android
			string headerKey = "NULL";
			#else
			string headerKey = "STATUS";
			#endif

			if (www.responseHeaders.ContainsKey(headerKey))
			{
				string status = www.responseHeaders[headerKey];
				System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(status, @"^HTTP.*\s(\d{3})\s.*$");
				if (matches.Count > 0 && matches[0].Groups.Count > 0)
				{
					statusCode = Convert.ToInt32(matches[0].Groups[1].Value);
				}
			}
			else
			{
				if (String.IsNullOrEmpty(www.error))
				{
					statusCode = 200;
				}
				else
				{
					statusCode = ReadWWWResponse(www.error);
				}
			}

			return statusCode;
		}

		private static string FormatURI(string uriPattern, string apiHost, string envKey, string hash=null)
		{
			var uri = uriPattern.Replace("{host}", apiHost);
			uri = uri.Replace("{env_key}", envKey);
			uri = uri.Replace("{hash}", hash);
			return uri;
		}

		private static string ValidateURL(string url) {
			if (!url.ToLower().StartsWith("http://")) {
				url = "http://" + url;
			}
			return url;
		}

		private static string GenerateHash(string data, string secret){
			var md5 = MD5.Create();
			var inputBytes = Encoding.UTF8.GetBytes(data + secret);
			var hash = md5.ComputeHash(inputBytes);

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
					this.UserID = GetUserID ();
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
				LogDebug("Sending 'newPlayer' event");

				var newPlayerParams = new EventBuilder()
					.AddParam("userCountry", ClientInfo.CountryCode);

				this.RecordEvent("newPlayer", newPlayerParams);

				PlayerPrefs.SetInt(PF_KEY_FIRST_RUN, 0);
			}

			if (Settings.OnInitSendGameStartedEvent)
			{
				LogDebug("Sending 'gameStarted' event");

				var gameStartedParams = new EventBuilder()
					.AddParam("clientVersion", this.ClientVersion)
					.AddParam("pushNotificationToken", this.PushNotificationToken)
					.AddParam("androidRegistrationID", this.AndroidRegistrationID);

				this.RecordEvent("gameStarted", gameStartedParams);
			}

			if (Settings.OnInitSendClientDeviceEvent)
			{
				LogDebug("Sending 'clientDevice' event");

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
