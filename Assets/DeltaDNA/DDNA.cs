//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DeltaDNA
{
    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class DDNA : Singleton<DDNA>
    {
        static readonly string PF_KEY_USER_ID = "DDSDK_USER_ID";

        private bool started = false;
        private string collectURL;
        private string engageURL;

        private EventStore eventStore = null;
        private GameEvent launchNotificationEvent = null;
        private string pushNotificationToken = null;
        private string androidRegistrationId = null;

        private static Func<DateTime?> TimestampFunc = new Func<DateTime?>(DefaultTimestampFunc);

        private static object _lock = new object();

        protected DDNA()
        {
            this.Settings = new Settings(); // default configuration
        }

        void Awake()
        {
            if (this.eventStore == null) {
                string eventStorePath = null;
                if (this.Settings.UseEventStore) {
                    eventStorePath = Settings.EVENT_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);
                }
                this.eventStore = new EventStore(eventStorePath);
            }

            // Attach additional behaviours as children of this gameObject
            GameObject iosNotifications = new GameObject();
            this.IosNotifications = iosNotifications.AddComponent<IosNotifications>();
            iosNotifications.transform.parent = gameObject.transform;

            GameObject androidNotifications = new GameObject();
            this.AndroidNotifications = androidNotifications.AddComponent<AndroidNotifications>();
            androidNotifications.transform.parent = gameObject.transform;
        }

        #region Client Interface

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.  The SDK will
        /// generate a new user id if this is the first run.
        /// </summary>
        /// <param name="envKey">The unique environment key for this game environment.</param>
        /// <param name="collectURL">The Collect URL for this game.</param>
        /// <param name="engageURL">The Engage URL for this game.</param>
        public void StartSDK(string envKey, string collectURL, string engageURL)
        {
            StartSDK(envKey, collectURL, engageURL, null);
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.
        /// </summary>
        /// <param name="envKey">The unique environment key for this game environment.</param>
        /// <param name="collectURL">The Collect URL for this game.</param>
        /// <param name="engageURL">The Engage URL for this game.</param>
        /// <param name="userID">The user id for the player, if set to null we create one for you.</param>
        public void StartSDK(string envKey, string collectURL, string engageURL, string userID)
        {
            lock (_lock)
            {
                bool newPlayer = false;
                if (String.IsNullOrEmpty(this.UserID)) {    // first time!
                    newPlayer = true;
                    if (String.IsNullOrEmpty(userID)) {     // generate a user id
                        userID = GenerateUserID();
                    }
                } else if (!String.IsNullOrEmpty(userID)) { // use offered user id
                    if (this.UserID != userID) {
                        newPlayer = true;
                    }
                }

                this.UserID = userID;

                if (newPlayer) {
                    Logger.LogInfo("Starting DDNA SDK with new user "+UserID);
                } else {
                    Logger.LogInfo("Starting DDNA SDK with existing user "+UserID);
                }

                this.EnvironmentKey = envKey;
                this.CollectURL = collectURL;   // TODO: warn if no http is present, prepend it, although we support both
                this.EngageURL = engageURL;
                this.Platform = ClientInfo.Platform;
                this.NewSession();

                this.started = true;

                if (this.launchNotificationEvent != null) {
                    RecordEvent(this.launchNotificationEvent);
                    this.launchNotificationEvent = null;
                }

                TriggerDefaultEvents(newPlayer);

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
            string sessionID = GenerateSessionID();
            Logger.LogDebug("Starting new session "+sessionID);
            this.SessionID = sessionID;
        }

        /// <summary>
        /// Sends a 'gameEnded' event to Collect, disables background uploads.
        /// </summary>
        public void StopSDK()
        {
            lock (_lock)
            {
                if (this.started) {
                    Logger.LogInfo("Stopping DDNA SDK");
                    RecordEvent("gameEnded");
                    CancelInvoke();
                    Upload();
                    this.started = false;
                } else {
                    Logger.LogDebug("SDK not running");
                }
            }
        }

        /// <summary>
        /// Records an event using the GameEvent class.
        /// </summary>
        /// <param name="gameEvent">Event to record.</param>
        public void RecordEvent<T>(T gameEvent) where T : GameEvent<T>
        {
            if (!this.started) {
                throw new Exception("You must first start the SDK via the StartSDK method");
            }

            gameEvent.AddParam("platform", this.Platform);
            gameEvent.AddParam("sdkVersion", Settings.SDK_VERSION);

            var eventSchema = gameEvent.AsDictionary();
            eventSchema["userID"] = this.UserID;
            eventSchema["sessionID"] = this.SessionID;
            eventSchema["eventUUID"] = Guid.NewGuid().ToString();

            string currentTimestmp = GetCurrentTimestamp();
            if (currentTimestmp != null) {
                eventSchema["eventTimestamp"] = GetCurrentTimestamp();
            }

            try {
                string json = MiniJSON.Json.Serialize(eventSchema);
                if (!this.eventStore.Push(json)) {
                    Logger.LogWarning("Event store full, dropping '"+gameEvent.Name+"' event.");
                }
            } catch (Exception ex) {
                Logger.LogWarning("Unable to generate JSON for '"+gameEvent.Name+"' event. "+ex.Message);
            }
        }

        /// <summary>
        /// Records an event with no custom parameters.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        public void RecordEvent(string eventName)
        {
            var gameEvent = new GameEvent(eventName);
            RecordEvent(gameEvent);
        }

        /// <summary>
        /// Records an event with a name and a dictionary of event parameters.  The eventParams dictionary
        /// should match the 'eventParams' branch of the event schema.
        /// </summary>
        /// <param name="eventName">Event name.</param>
        /// <param name="eventParams">Event parameters.</param>
        public void RecordEvent(string eventName, Dictionary<string, object> eventParams)
        {
            var gameEvent = new GameEvent(eventName);
            foreach (var key in eventParams.Keys) {
                gameEvent.AddParam(key, eventParams[key]);
            }
            RecordEvent(gameEvent);
        }

        /// <summary>
        /// Makes an Engage request.  The result of the engagement will be passed as a dictionary object to your callback method. The response
        /// will be null is no response is available.
        /// </summary>
        /// <param name="decisionPoint">The decision point the request is for, must match the string in Portal.</param>
        /// <param name="engageParams">Additional parameters for the engagement.</param>
        /// <param name="callback">Method called with the response from our server.</param>
        [Obsolete("Prefer 'RequestEngagement' with an 'Engagement' instead.")]
        public void RequestEngagement(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
        {
            var engagement = new Engagement(decisionPoint);
            foreach (var key in engageParams.Keys) {
                engagement.AddParam(key, engageParams[key]);
            }
            RequestEngagement(engagement, callback);
        }

        /// <summary>
        /// Makes an Engage request.  The result of the engagement will be passed as a dictionary object to your callback method. The response
        /// will be null is no response is available.
        /// </summary>
        /// <param name="engagement">The engagement the request is for.</param>
        /// <param name="callback">Method called with the response from Engage.</param>
        public void RequestEngagement(Engagement engagement, Action<Dictionary<string, object>> callback)
        {
            if (!this.started) {
                throw new Exception("You must first start the SDK via the StartSDK method.");
            }

            if (String.IsNullOrEmpty(this.EngageURL)) {
                throw new Exception("Engage URL not configured.");
            }

            try {

                var dict = engagement.AsDictionary();

                var request = new EngageRequest(dict["decisionPoint"] as string);
                request.Flavour = dict["flavour"] as string;
                request.Parameters = dict["parameters"] as Dictionary<string, object>;

                EngageResponse handler = (string response, int statusCode, string error) => {
                    JSONObject responseJSON = null;
                    if (response != null) {
                        try {
                            responseJSON = DeltaDNA.MiniJSON.Json.Deserialize(response) as JSONObject;
                        } catch (Exception exception) {
                            Logger.LogError("Engagement "+engagement.DecisionPoint+" responded with invalid JSON: "+exception.Message);
                        }
                    }
                    callback(responseJSON);
                };

                StartCoroutine(Engage.Request(this, request, handler));

            } catch (Exception ex) {
                Logger.LogWarning("Engagement request failed: "+ex.Message);
            }
        }

        /// <summary>
        /// Requests an image based Engagement for popping up on screen.  This is a convience around RequestEngagement
        /// that loads the image resource automatically from the original engage request.  Register a function with the
        /// Popup's AfterLoad event to be notified when the image has be been downloaded from our server.
        /// </summary>
        /// <param name="decisionPoint">The decision point the request is for, must match the string in Portal.</param>
        /// <param name="engageParams">Additional parameters for the engagement.</param>
        /// <param name="popup">A Popup object to display the image.</param>
        [Obsolete("Prefer 'RequestImageMessage' with an 'Engagement' instead.")]
        public void RequestImageMessage(string decisionPoint, Dictionary<string, object> engageParams, IPopup popup)
        {
            var engagement = new Engagement(decisionPoint);
            foreach (var key in engageParams.Keys) {
                engagement.AddParam(key, engageParams[key]);
            }
            RequestImageMessage(engagement, popup, null);
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
        [Obsolete("Prefer 'RequestImageMessage' with an 'Engagement' instead.")]
        public void RequestImageMessage(string decisionPoint, Dictionary<string, object> engageParams, IPopup popup, Action<Dictionary<string, object>> callback)
        {
            var engagement = new Engagement(decisionPoint);
            foreach (var key in engageParams.Keys) {
                engagement.AddParam(key, engageParams[key]);
            }
            RequestImageMessage(engagement, popup, callback);
        }

        /// <summary>
        /// Requests an image based Engagement for popping up on screen.  This is a convience around RequestEngagement
        /// that loads the image resource automatically from the original engage request.  Register a function with the
        /// Popup's AfterLoad event to be notified when the image has be been downloaded from our server.
        /// </summary>
        /// <param name="engagement">The engagement the request is for.</param>
        /// <param name="popup">A Popup object to display the image.</param>
        public void RequestImageMessage(Engagement engagement, IPopup popup)
        {
            RequestImageMessage(engagement, popup, null);
        }

        /// <summary>
        /// Requests an image based Engagement for popping up on screen.  This is a convience around RequestEngagement
        /// that loads the image resource automatically from the original engage request.  Register a function with the
        /// Popup's AfterLoad event to be notified when the image has be been downloaded from our server.
        /// </summary>
        /// <param name="engagement">The engagement the request is for.</param>
        /// <param name="popup">A Popup object to display the image.</param>
        /// <param name="callback">Method called with the response from Engage.</param>
        public void RequestImageMessage(Engagement engagement, IPopup popup, Action<Dictionary<string, object>> callback)
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

            RequestEngagement(engagement, imageCallback);
        }

        /// <summary>
        /// Records that the game received a push notification.  It is safe to call this method
        /// before calling StartSDK, the 'notificationOpened' event will be sent at that time.
        /// </summary>
        /// <param name="payload">The notification payload.</param>
        public void RecordPushNotification(Dictionary<string, object> payload)
        {
            Logger.LogDebug("Received push notification: "+payload);

            var notificationEvent = new GameEvent("notificationOpened");
            try {
                if (payload.ContainsKey("_ddId")) {
                    notificationEvent.AddParam("notificationId", Convert.ToInt32(payload["_ddId"]));
                }
                if (payload.ContainsKey("_ddName")) {
                    notificationEvent.AddParam("notificationName", payload["_ddName"]);
                }
                if (payload.ContainsKey("_ddLaunch")) {
                    notificationEvent.AddParam("notificationLaunch", Convert.ToBoolean(payload["_ddLaunch"]));
                }
            } catch (Exception ex) {
                Logger.LogError("Error parsing push notification payload. "+ex.Message);
            }

            if (this.started) {
                this.RecordEvent(notificationEvent);
            } else {
                this.launchNotificationEvent = notificationEvent;
            }
        }

        /// <summary>
        /// Uploads waiting events to our Collect service.  By default this is called automatically in the
        /// background.  If you disable auto uploading via <see cref="Settings.BackgroundEventUpload"/> you
        /// will need to call this method yourself periodically.
        /// </summary>
        public void Upload()
        {
            if (!this.started)
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
        /// Clears the persistent data such as user id.  Useful for testing purposes.
        /// </summary>
        public void ClearPersistentData()
        {
            PlayerPrefs.DeleteKey(PF_KEY_USER_ID);

            if (this.eventStore != null) {
                this.eventStore.ClearAll();
            }

            Engage.ClearCache();
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
        public bool HasStarted { get { return this.started; }}

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
        public string HashSecret { get; set; }

        /// <summary>
        /// A version string for your game that will be reported to us.  This must
        /// be set before calling <see cref="Init"/>.
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// The push notification token from Apple that is associated with this device if
        /// it's running on the iOS platform.  This must be set before calling <see cref="Init"/>.
        /// </summary>
        public string PushNotificationToken
        {
            get
            {
                return this.pushNotificationToken;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != this.pushNotificationToken) {
                    var notificationServicesEvent = new GameEvent("notificationServices")
                        .AddParam("pushNotificationToken", value);

                    if (this.started) {
                        RecordEvent(notificationServicesEvent);
                    } // else send with clientDevice event
                    this.pushNotificationToken = value;
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
                return this.androidRegistrationId;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != this.androidRegistrationId) {
                    var notificationServicesEvent = new GameEvent("notificationServices")
                        .AddParam("androidRegistrationID", value);

                    if (this.started) {
                        RecordEvent(notificationServicesEvent);
                    } // else send with clientDevice event
                    this.androidRegistrationId = value;
                }
            }
        }

        #endregion

        #region Private Helpers

        public override void OnDestroy()
        {
            if (this.eventStore != null) this.eventStore.Dispose();
            PlayerPrefs.Save();
            base.OnDestroy();
        }

        private string GenerateSessionID()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateUserID()
        {
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
                    ts = ts.Replace(".1000", ".999");
                }
                return ts;
            }
            return null;    // Collect will insert a timestamp for us.
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

        internal string ResolveEngageURL(string httpBody)
        {
            string url;
            if (httpBody != null && this.HashSecret != null)
            {
                string md5Hash = GenerateHash(httpBody, this.HashSecret);
                url = FormatURI(Settings.ENGAGE_HASH_URL_PATTERN, this.EngageURL, this.EnvironmentKey, md5Hash);
            }
            else
            {
                url = FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey, null);
            }
            return url;
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

        private void TriggerDefaultEvents(bool newPlayer)
        {
            if (Settings.OnFirstRunSendNewPlayerEvent && newPlayer)
            {
                Logger.LogDebug("Sending 'newPlayer' event");

                var newPlayerEvent = new GameEvent("newPlayer");
                if (ClientInfo.CountryCode != null) {
                    newPlayerEvent.AddParam("userCountry", ClientInfo.CountryCode);
                }

                RecordEvent(newPlayerEvent);
            }

            if (Settings.OnInitSendGameStartedEvent)
            {
                Logger.LogDebug("Sending 'gameStarted' event");

                var gameStartedEvent = new GameEvent("gameStarted")
                    .AddParam("clientVersion", this.ClientVersion)
                    .AddParam("userLocale", ClientInfo.Locale);

                if (!String.IsNullOrEmpty(this.PushNotificationToken)) {
                    gameStartedEvent.AddParam("pushNotificationToken", this.PushNotificationToken);
                }

                if (!String.IsNullOrEmpty(this.AndroidRegistrationID)) {
                    gameStartedEvent.AddParam("androidRegistrationID", this.AndroidRegistrationID);
                }

                RecordEvent(gameStartedEvent);
            }

            if (Settings.OnInitSendClientDeviceEvent)
            {
                Logger.LogDebug("Sending 'clientDevice' event");

                var clientDeviceEvent = new GameEvent("clientDevice")
                    .AddParam("deviceName", ClientInfo.DeviceName)
                    .AddParam("deviceType", ClientInfo.DeviceType)
                    .AddParam("hardwareVersion", ClientInfo.DeviceModel)
                    .AddParam("operatingSystem", ClientInfo.OperatingSystem)
                    .AddParam("operatingSystemVersion", ClientInfo.OperatingSystemVersion)
                    .AddParam("timezoneOffset", ClientInfo.TimezoneOffset)
                    .AddParam("userLanguage", ClientInfo.LanguageCode);

                if (ClientInfo.Manufacturer != null) {
                    clientDeviceEvent.AddParam("manufacturer", ClientInfo.Manufacturer);
                }

                RecordEvent(clientDeviceEvent);
            }
        }

        #endregion

    }
}
