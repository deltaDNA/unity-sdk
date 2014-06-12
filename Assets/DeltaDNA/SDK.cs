using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	public sealed class SDK : Singleton<SDK>
	{
		static readonly string PF_KEY_USER_ID = "DDSDK_USER_ID";
		static readonly string PF_KEY_FIRST_RUN = "DDSDK_FIRST_RUN";
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
		
		private bool initialised = false;
		private bool userIdRequestInProgress = false;
		
		private EventStore eventStore = null;
		private EngageArchive engageArchive = null;
		
		private SDK() 
		{
			this.Settings = new Settings();	// default configuration
		}
		
		#region Client Interface
		
		public void Init(string envKey, string collectURL, string secret, string userID=null, string engageURL=null)
		{
			if (Settings.ResetTest) ClearPersistentData();
		
			this.EnvironmentKey = envKey;
			this.Secret = secret;
			this.UserID = userID;
			this.CollectURL = collectURL;	// TODO: warn if no http is present, prepend it, although we support both
			this.EngageURL = engageURL;
			this.Platform = ClientInfo.Platform;
			this.SessionID = GetSessionID();
			
			this.eventStore = new EventStore(
				Settings.EVENT_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath), 
				Settings.ResetTest, 
				Settings.DebugMode
			);
			
			if (engageURL != null)
			{
				this.engageArchive = new EngageArchive(
					Settings.ENGAGE_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath)
				);
			}
			
			this.initialised = true;
			
			// must do this once we're initialised
			TriggerDefaultEvents();
			
			// Setup automated event uploads
			if (Settings.BackgroundEventUpload)
			{
				InvokeRepeating("Upload", Settings.BackgroundEventUploadStartDelaySeconds, Settings.BackgroundEventUploadRepeatRateSeconds);
			}
		}
		
		public override void OnDestroy()
		{
			if (this.eventStore != null) this.eventStore.Dispose();
			if (this.engageArchive != null) this.engageArchive.Save();
			base.OnDestroy();
		}
		
		public void TriggerEvent(string eventName)
		{
			TriggerEvent(eventName, new Dictionary<string, object>());
		}
		
		public void TriggerEvent(string eventName, EventBuilder eventParams)
		{
			TriggerEvent(eventName, eventParams == null ? new Dictionary<string, object>() : eventParams.ToDictionary());
		}
		
		public void TriggerEvent(string eventName, Dictionary<string, object> eventParams)
		{
			if (!this.initialised) 
			{
				throw new NotInitialisedException("You must first initialise our SDK via the Init method");
			}
			
			// the header for every event is eventName, userID, sessionID and timestamp
			//var eventRecord = new Event(eventName, this.UserID, this.SessionID, DateTime.UtcNow);
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
		
		public void RequestEngagement(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
		{
			if (!this.initialised)
			{
				throw new NotInitialisedException("You must first initialise out SDK via the Init method");
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
			
			if (this.IsRequestingEngagement)
			{
				// TODO: should we abort the current request if it is for a different descison point?
				// Implication is that the games moved on, so previous request is no longer valid.
				LogWarning("Only one engage request at a time is currently supported.");
				return;
			}
			
			StartCoroutine(EngageCoroutine(decisionPoint, engageParams, callback));
		}
		
		public void Upload()
		{
			if (!this.initialised) 
			{
				throw new NotInitialisedException("You must first initialise our SDK via the Init method");
			}
			
			if (this.IsUploading)
			{
				LogWarning("Event upload already in progress, aborting");
				return;
			}
			
			StartCoroutine(UploadCoroutine());
		}
		
		#endregion
		
		#region Properties
		
		public string EnvironmentKey { get; private set; }
		public string Secret { get; private set; }
		public string CollectURL { get; private set; }
		public string EngageURL { get; private set; }
		public string SessionID { get; private set; }
		public string Platform { get; private set; }
		
		public string UserID 
		{
			get { return PlayerPrefs.GetString(PF_KEY_USER_ID, null); }
			set 
			{
				if (!String.IsNullOrEmpty(value))
				{
					PlayerPrefs.SetString(PF_KEY_USER_ID, value);
					PlayerPrefs.Save();
				}
			}
		}
		
		public bool IsInitialised { get { return this.initialised; }} 
		public bool IsUploading { get; private set; }
		public bool IsRequestingEngagement { get; private set; }
		
		#endregion
		
		#region Client Configuration
		
		/// <summary>
		/// A version string for your game that will be reported to us.
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
					PlayerPrefs.SetString(PF_KEY_CLIENT_VERSION, value);
					PlayerPrefs.Save();				
				}
			}
		}
		
		/// <summary>
		/// The push notification token from Apple that is associated with this device if
		/// it's running on the iOS platform.
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
		/// on the Android platform.
		/// </summary>
		/// <value>The android registration I.</value>
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
		
		/// <summary>
		/// Controls default behaviour of the SDK.  Set prior to intialisation.
		/// </summary>
		public Settings Settings { get; set; }
		
		#region Private Helpers
		
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
		
		private void ClearPersistentData()
		{
			// PlayerPrefs
			PlayerPrefs.DeleteKey(PF_KEY_USER_ID);
			PlayerPrefs.DeleteKey(PF_KEY_FIRST_RUN);
			PlayerPrefs.DeleteKey(PF_KEY_CLIENT_VERSION);
			PlayerPrefs.DeleteKey(PF_KEY_PUSH_NOTIFICATION_TOKEN);
			PlayerPrefs.DeleteKey(PF_KEY_ANDROID_REGISTRATION_ID);		
		}
		
		private string GetSessionID()
		{
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
				LogDebug("Starting event upload");
				
				// If there's no user id set by the client, ask our server for one.
				if (String.IsNullOrEmpty(UserID)) {
					LogDebug("Upload has no user id set, requesting an user id");
					yield return StartCoroutine(RequestUserID());
				}
				
				// Did we eventually get a user id?
				if (String.IsNullOrEmpty(UserID)) {
					LogDebug("Upload failed to get an user id, can not continue");
					yield break;
				}
				
				// Swap over event queue.
				this.eventStore.Swap();	
				
				// Create bulk event message to post.
				List<string> events = eventStore.Read();
				
				if (events.Count > 0)
				{
					yield return StartCoroutine(PostEvents(events.ToArray(), (succeeded) =>
					{
						if (succeeded)
						{
							LogDebug("Upload successful");
							this.eventStore.Clear();
						}
						else
						{
							LogWarning("Upload failed - try again later");
						}
					}));
				}
			}
			finally
			{
				this.IsUploading = false;
			}
		}
		
		private IEnumerator EngageCoroutine(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
		{
			try
			{
				this.IsRequestingEngagement = true;
				LogDebug("Starting engagement for '"+decisionPoint+"'");	
				
				if (String.IsNullOrEmpty(UserID))	
				{
					LogDebug("Not user ID set, requesting one");
					yield return StartCoroutine(RequestUserID());
					
					if(String.IsNullOrEmpty(UserID))
					{
						LogWarning("Failed to get a user id, can not continue.");
					}
				}	
				
				Dictionary<string, object> engageRequest = new Dictionary<string, object>()
				{
					{ "userID", this.UserID },
					{ "decisionPoint", decisionPoint },
					{ "sessionID", this.SessionID },
					{ "version", Settings.ENGAGE_API_VERSION },
					{ "sdkVersion", Settings.SDK_VERSION },
					{ "platform", this.Platform }
				};
				
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
				
				yield return StartCoroutine(EngageRequest(engageJSON, (response) => 
				{
					if (response != null)
					{
						LogDebug("Using live engagement: "+response);
						this.engageArchive[decisionPoint] = response;
					}
					else
					{
						if (this.engageArchive.Contains(decisionPoint))
						{
							LogWarning("Engage request failed, using cached response.");
							response = this.engageArchive[decisionPoint];
						}
						else
						{
							LogWarning("Engage request failed");
						}
					}
					Dictionary<string, object> result = MiniJSON.Json.Deserialize(response) as Dictionary<string, object>;
					callback(result);
				}));
			}	
			finally
			{
				this.IsRequestingEngagement = false;
			}
		}
		
		private IEnumerator RequestUserID()
		{
			// We can call this from different coroutines so
			// need to make sure we don't make multiple requests
			// which will give us different answers.
			while (this.userIdRequestInProgress)
			{
				LogDebug("User ID request already in progress, waiting...");
				yield return null;
			}
			
			if (!String.IsNullOrEmpty(UserID))
			{
				yield break;
			}
			
			try
			{
				this.userIdRequestInProgress = true;
				
				string url = FormatURI(Settings.USERID_URL_PATTERN, this.CollectURL, this.EnvironmentKey);
				
				// create a new url to turn off any caching
				DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
				TimeSpan span = (DateTime.Now.ToLocalTime() - epoch);
				url += "?"+span.TotalMilliseconds;
				
				int attempts = 0;
				bool succeeded = false;
				
				do
				{
					yield return StartCoroutine(HttpGET(url, (status, response) => {
						if (status == 200)
						{
							var obj = MiniJSON.Json.Deserialize(response) as Dictionary<string, object>;
							UserID = Convert.ToString(obj["userID"]);
							succeeded = true;
						}
						else
						{
							LogDebug("Error requesting User ID, Collect returned: "+status);
						}
					}));
					yield return new WaitForSeconds(Settings.HttpRequestRetryDelaySeconds);
				}
				while (!succeeded && ++attempts < Settings.HttpRequestMaxRetries);
			}
			finally
			{
				this.userIdRequestInProgress = false;
			}
		}
		
		private IEnumerator PostEvents(string[] events, Action<bool> resultCallback)
		{
			string bulkEvent = "{\"eventList\":[" + String.Join(",", events) + "]}";
			string md5Hash = GenerateHash(bulkEvent);
			string url = FormatURI(Settings.COLLECT_URL_PATTERN, this.CollectURL, this.EnvironmentKey, md5Hash);
			
			int attempts = 0;
			bool succeeded = false;
			
			do
			{
				yield return StartCoroutine(HttpPOST(url, bulkEvent, (status, response) =>
				{
					if (status == 200) succeeded = true;
					else LogDebug("Error uploading events, Collect returned: "+status);
				}));
				yield return new WaitForSeconds(Settings.HttpRequestRetryDelaySeconds);
			}
			while (!succeeded && ++attempts < Settings.HttpRequestMaxRetries);
			
			resultCallback(succeeded);
		}
		
		private IEnumerator EngageRequest(string engagement, Action<string> callback)
		{
			string md5Hash = GenerateHash(engagement);
			string url = FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey, md5Hash);
			
			yield return StartCoroutine(HttpPOST(url, engagement, (status, response) => 
			{
				if (status == 200)
				{
					callback(response);
				}
				else
				{
					LogDebug("Error requesting engagement, Engage returned: "+status);
					callback(null);
				}
			}));
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
			Hashtable headers = form.headers;
			headers["Content-Type"] = "application/json";
			
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			
			WWW www = new WWW(url, bytes, headers);
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
		
		private static int ReadWWWResponse(string response)
		{
			System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(response, @"^(\d+).+$");
			if (matches.Count > 0 && matches[0].Groups.Count > 0) 
			{
				return Convert.ToInt32(matches[0].Groups[1].Value);
			}
			return 0;
		}
		
		public static string FormatURI(string uriPattern, string apiHost, string envKey, string hash=null) 
		{
			var uri = uriPattern.Replace("{host}", apiHost);
			uri = uri.Replace("{env_key}", envKey);
			uri = uri.Replace("{hash}", hash);
			return uri;
		}
		
		private string GenerateHash(string data){
			var md5 = System.Security.Cryptography.MD5.Create();
			var inputBytes = Encoding.UTF8.GetBytes(data + this.Secret);
			var hash = md5.ComputeHash(inputBytes);
			
			var sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			
			return sb.ToString();
		}
		
		private void TriggerDefaultEvents()
		{
			if (Settings.OnFirstRunSendNewPlayerEvent && PlayerPrefs.GetInt(PF_KEY_FIRST_RUN, 1) > 0)
			{
				LogDebug("Sending 'newPlayer' event");
			
				var newPlayerParams = new EventBuilder()
					.AddParam("userCountry", ClientInfo.CountryCode);
			
				this.TriggerEvent("newPlayer", newPlayerParams);
				
				PlayerPrefs.SetInt(PF_KEY_FIRST_RUN, 0);
			}
			
			if (Settings.OnInitSendGameStartedEvent)
			{
				LogDebug("Sending 'gameStarted' event");
				
				var gameStartedParams = new EventBuilder()
					.AddParam("clientVersion", this.ClientVersion)
					.AddParam("pushNotificationToken", this.PushNotificationToken)
					.AddParam("androidRegistrationID", this.AndroidRegistrationID);
				
				this.TriggerEvent("gameStarted", gameStartedParams);
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
					.AddParam("timezone", ClientInfo.Timezone)
					.AddParam("userLanguage", ClientInfo.LanguageCode);
					
				this.TriggerEvent("clientDevice", clientDeviceParams);
			}	
		}
		
		#endregion
	}
}

