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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace DeltaDNA {

    /// <summary>
    /// DDNA is the deltaDNA SDK.
    /// </summary>
    public class DDNA : Singleton<DDNA> {

        internal const string PF_KEY_USER_ID = "DDSDK_USER_ID";
        internal const string PF_KEY_FIRST_SESSION = "DDSDK_FIRST_SESSION";
        internal const string PF_KEY_LAST_SESSION = "DDSDK_LAST_SESSION";
        internal const string PF_KEY_CROSS_GAME_USER_ID = "DDSDK_CROSS_GAME_USER_ID";
        internal const string PF_KEY_ADVERTISING_ID = "DDSDK_ADVERTISING_ID";
        internal const string PF_KEY_FORGET_ME = "DDSDK_FORGET_ME";
        internal const string PF_KEY_STOP_TRACKING_ME = "DDSKD_STOP_TRACKING_ME";
        internal const string PF_KEY_FORGOTTEN = "DDSK_FORGOTTEN";
        internal const string PF_KEY_ACTIONS_SALT = "DDSDK_ACTIONS_SALT";

        private static object _lock = new object();

        public event Action OnNewSession;
        /// <summary>
        /// Will be called when the session configuration will be successfully
        /// retrieved. The type parameter specifies whether the response was
        /// cached or not.
        /// </summary>
        public event Action<bool> OnSessionConfigured;
        /// <summary>
        /// Will be called when the session configuration request fails.
        /// </summary>
        public event Action OnSessionConfigurationFailed;
        /// <summary>
        /// Will be called when the image cache will be successfully populated.
        /// </summary>
        public event Action OnImageCachePopulated;
        /// <summary>
        /// Will be called when the image cache will fail to be populated. The
        /// reason for the failure will be passed as the type argument.
        /// </summary>
        public event Action<string> OnImageCachingFailed;

        private DDNABase delegated;
        private string collectURL;
        private string engageURL;

        protected DDNA() {
            Settings = new Settings(); // default configuration
        }

        void OnEnable() {
            #if UNITY_5_OR_NEWER
            Application.logMessageReceived += Logger.HandleLog;
            #endif
        }

        void OnDisable() {
            #if UNITY_5_OR_NEWER
            Application.logMessageReceived -= Logger.HandleLog;
            #endif
        }

        internal void Awake() {
            lock (_lock) {
                if (PlayerPrefs.HasKey(PF_KEY_FORGET_ME)
                    || PlayerPrefs.HasKey(PF_KEY_FORGOTTEN)
                    || PlayerPrefs.HasKey(PF_KEY_STOP_TRACKING_ME)
                    ) {
                    delegated = new DDNANonTracking(this);
                } else {
                    delegated = new DDNAImpl(this);
                }

                // attach additional behaviours as children of this gameObject
                #if !DDNA_IOS_PUSH_NOTIFICATIONS_REMOVED
                GameObject iosNotificationsObject = new GameObject();
                IosNotifications = iosNotificationsObject.AddComponent<IosNotifications>();
                iosNotificationsObject.transform.parent = gameObject.transform;
                #endif

                GameObject androidNotificationsObject = new GameObject();
                AndroidNotifications = androidNotificationsObject.AddComponent<AndroidNotifications>();
                androidNotificationsObject.transform.parent = gameObject.transform;
            }
        }

        #region Client Interface

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.  The SDK will
        /// generate a new user id if this is the first run.
        /// </summary>
        public void StartSDK() {
            StartSDK((string) null);
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.
        /// </summary>
        /// <param name="userID">The user id for the player, if set to null we create one for you.</param>
        public void StartSDK(string userID) {
            var configResource = Resources.Load("ddna_configuration", typeof(TextAsset));
            Configuration config;
            if (configResource != null) {
                using (var stringReader = new StringReader((configResource as TextAsset).text)) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        config = (new XmlSerializer(
                            typeof(Configuration), new XmlRootAttribute("configuration")))
                            .Deserialize(xmlReader) as Configuration;
                    }
                }
            } else {
                Logger.LogWarning("Failed to find DDNA SDK configuration");
                config = new Configuration();
            }

            StartSDK(config, userID);
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.  The SDK will
        /// generate a new user id if this is the first run. This method can be used if the
        /// game configuration needs to be provided in the code as opposed to using the
        /// configuration UI.
        /// </summary>
        /// <param name="config">The game configuration for the SDK.</param>
        public void StartSDK(Configuration config) {
            StartSDK(config, null);
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.  This method
        /// can be used if the game configuration needs to be provided in the code as opposed
        /// to using the configuration UI.
        /// </summary>
        /// <param name="config">The game configuration for the SDK.</param>
        /// <param name="userID">The user id for the player, if set to null we create one for you.</param>
        public void StartSDK(Configuration config, string userID) {
            lock (_lock) {
                bool newPlayer = false;
                if (String.IsNullOrEmpty(UserID)) {         // first time!
                    newPlayer = true;
                    if (String.IsNullOrEmpty(userID)) {     // generate a user id
                        userID = GenerateUserID();
                    }
                } else if (!String.IsNullOrEmpty(userID)) { // use offered user id
                    if (UserID != userID) {
                        newPlayer = true;
                    }
                }

                UserID = userID;

                if (newPlayer) {
                    Logger.LogInfo("Starting DDNA SDK with new user " + UserID);
                } else {
                    Logger.LogInfo("Starting DDNA SDK with existing user " + UserID);
                }

                EnvironmentKey = (config.environmentKey == 0)
                    ? config.environmentKeyDev
                    : config.environmentKeyLive;
                CollectURL = config.collectUrl;
                EngageURL = config.engageUrl;
                if (Platform == null) {
                    Platform = ClientInfo.Platform;
                }

                if (!string.IsNullOrEmpty(config.hashSecret)) {
                    HashSecret = config.hashSecret;
                }
                if (config.useApplicationVersion) {
                    ClientVersion = Application.version;
                } else if (!string.IsNullOrEmpty(config.clientVersion)) {
                    ClientVersion = config.clientVersion;
                }

                if (newPlayer) {
                    PlayerPrefs.DeleteKey(PF_KEY_FIRST_SESSION);
                    PlayerPrefs.DeleteKey(PF_KEY_LAST_SESSION);
                    PlayerPrefs.DeleteKey(PF_KEY_CROSS_GAME_USER_ID);

                    if (delegated is DDNANonTracking) {
                        PlayerPrefs.DeleteKey(PF_KEY_FORGET_ME);
                        PlayerPrefs.DeleteKey(PF_KEY_FORGOTTEN);
                        PlayerPrefs.DeleteKey(PF_KEY_STOP_TRACKING_ME);

                        delegated = new DDNAImpl(this);
                    }
                }

                delegated.StartSDK(newPlayer);
            }
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.  The SDK will
        /// generate a new user id if this is the first run.
        /// </summary>
        /// <param name="envKey">The unique environment key for this game environment.</param>
        /// <param name="collectURL">The Collect URL for this game.</param>
        /// <param name="engageURL">The Engage URL for this game.</param>
        [Obsolete("Deprecated as of version 4.8, please use the Editor configuration UI and StartSDK() instead")]
        public void StartSDK(string envKey, string collectURL, string engageURL) {
            StartSDK();
        }

        /// <summary>
        /// Starts the SDK.  Call before sending events or making engagements.
        /// </summary>
        /// <param name="envKey">The unique environment key for this game environment.</param>
        /// <param name="collectURL">The Collect URL for this game.</param>
        /// <param name="engageURL">The Engage URL for this game.</param>
        /// <param name="userID">The user id for the player, if set to null we create one for you.</param>
        [Obsolete("Deprecated as of version 4.8, please use the Editor configuration UI and StartSDK(userID) instead")]
        public void StartSDK(string envKey, string collectURL, string engageURL, string userID) {
            StartSDK(userID);
        }

        /// <summary>
        /// Changes the session ID for the current User.
        /// </summary>
        public void NewSession() {
            string sessionID = GenerateSessionID();
            Logger.LogInfo("Starting new session "+sessionID);
            SessionID = sessionID;

            RequestSessionConfiguration();
            if (!PlayerPrefs.HasKey(PF_KEY_FIRST_SESSION)) {
                PlayerPrefs.SetString(
                    PF_KEY_FIRST_SESSION,
                    DateTime.UtcNow.ToString(Settings.EVENT_TIMESTAMP_FORMAT));
            }
            PlayerPrefs.SetString(
                PF_KEY_LAST_SESSION,
                DateTime.UtcNow.ToString(Settings.EVENT_TIMESTAMP_FORMAT));

            if (OnNewSession != null) OnNewSession();
        }

        /// <summary>
        /// Sends a 'gameEnded' event to Collect, disables background uploads.
        /// </summary>
        public void StopSDK() {
            lock (_lock) {
                delegated.StopSDK();
            }
        }

        /// <summary>
        /// Records an event using the GameEvent class.
        /// </summary>
        /// <param name="gameEvent">Event to record.</param>
        /// <returns><see cref="EventAction"/> for this event</returns>
        /// <exception cref="System.Exception">Thrown if the SDK has not been started.</exception>
        public EventAction RecordEvent<T>(T gameEvent) where T : GameEvent<T> {
            return delegated.RecordEvent(gameEvent);
        }

        /// <summary>
        /// Records an event with no custom parameters.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns><see cref="EventAction"/> for this event</returns>
        /// <exception cref="System.Exception">Thrown if the SDK has not been started.</exception>
        public EventAction RecordEvent(string eventName) {
            return delegated.RecordEvent(eventName);
        }

        /// <summary>
        /// Records an event with a name and a dictionary of event parameters.  The eventParams dictionary
        /// should match the 'eventParams' branch of the event schema.
        /// </summary>
        /// <param name="eventName">Event name.</param>
        /// <param name="eventParams">Event parameters.</param>
        /// <returns><see cref="EventAction"/> for this event</returns>
        /// <exception cref="System.Exception">Thrown if the SDK has not been started.</exception>
        public EventAction RecordEvent(string eventName, Dictionary<string, object> eventParams) {
            return delegated.RecordEvent(eventName, eventParams);
        }

        /// <summary>
        /// Makes an Engage request.  The result of the engagement will be passed as a dictionary object to your callback method. The dictionary
        /// will be empty if engage couldn't be reached on a campaign is not running.
        /// A cache is maintained that will return the last valid response if available.
        /// </summary>
        /// <param name="engagement">The engagement the request is for.</param>
        /// <param name="callback">Method called with the response from Engage.</param>
        /// <exception cref="System.Exception">Thrown if the SDK has not been started, and if the Engage URL has not been set.</exception>
        public void RequestEngagement(Engagement engagement, Action<Dictionary<string, object>> callback) {
            delegated.RequestEngagement(engagement, callback);
        }

        /// <summary>
        /// Requests an Engagement with Engage.  The engagement is populated with the result of the request and
        /// returned in the onCompleted callback.  The engagement's json field can be queried for the returned json.
        /// A cache is maintained that will return the last valid response if available.
        /// </summary>
        /// <param name="engagement">The engagement the request is for.</param>
        /// <param name="onCompleted">Method called with the Engagement populated by Engage.</param>
        /// <exception cref="System.Exception">Thrown if the SDK has not been started, and if the Engage URL has not been set.</exception>
        public void RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError) {
            delegated.RequestEngagement(engagement, onCompleted, onError);
        }

        /// <summary>
        /// Records that the game received a push notification.  It is safe to call this method
        /// before calling StartSDK, the 'notificationOpened' event will be sent at that time.
        /// </summary>
        /// <param name="payload">The notification payload.</param>
        public void RecordPushNotification(Dictionary<string, object> payload) {
            delegated.RecordPushNotification(payload);
        }

        /// <summary>
        /// Makes a session configuration request. This method should be called if
        /// a session configuration request has previously failed.
        ///
        /// The result will be notified via <see cref="OnSessionConfigured"/> or
        /// <see cref="OnSessionConfigurationFailed"/> in case of failure.
        /// </summary>
        public void RequestSessionConfiguration() {
            delegated.RequestSessionConfiguration();
        }

        /// <summary>
        /// Uploads waiting events to our Collect service.  By default this is called automatically in the
        /// background.  If you disable auto uploading via <see cref="Settings.BackgroundEventUpload"/> you
        /// will need to call this method yourself periodically.
        /// </summary>
        public void Upload() {
            delegated.Upload();
        }

        /// <summary>
        /// Downloads image assets from the session configuration.
        ///
        /// This happens automatically whenever a session configuration request
        /// takes place, such as during a new session or when
        /// <see cref="RequestSessionConfiguration"/> is called.
        ///
        /// The success or failure will be notified via
        /// <see cref="OnImageCachePopulated"/> or <see cref="OnImageCachingFailed"/>
        /// respectively.
        /// </summary>
        public void DownloadImageAssets() {
            delegated.DownloadImageAssets();
        }

        /// <summary>
        /// Clears the persistent data, such as user id. The SDK should be stopped
        /// before this method is called.
        ///
        /// Useful for testing purposes.
        /// </summary>
        public void ClearPersistentData() {
            if (HasStarted) {
                Logger.LogWarning("SDK has not been stopped before clearing persistent data");
            }

            PlayerPrefs.DeleteKey(PF_KEY_USER_ID);
            PlayerPrefs.DeleteKey(PF_KEY_FIRST_SESSION);
            PlayerPrefs.DeleteKey(PF_KEY_LAST_SESSION);
            PlayerPrefs.DeleteKey(PF_KEY_CROSS_GAME_USER_ID);
            PlayerPrefs.DeleteKey(PF_KEY_ADVERTISING_ID);
            PlayerPrefs.DeleteKey(PF_KEY_FORGET_ME);
            PlayerPrefs.DeleteKey(PF_KEY_FORGOTTEN);
            PlayerPrefs.DeleteKey(PF_KEY_STOP_TRACKING_ME);
            PlayerPrefs.DeleteKey(PF_KEY_ACTIONS_SALT);

            delegated.ClearPersistentData();

            lock (_lock) {
                if (delegated is DDNANonTracking) {
                    delegated = new DDNAImpl(this);
                }
            }
        }

        /// <summary>
        /// Forgets the current user and stops them from being tracked.
        ///
        /// Any subsequent calls on the SDK will succeed, but not send/request anything to/from
        /// the Platform.
        ///
        /// The status can be cleared by starting the SDK with a new use or clearing the persistent
        /// data.
        /// </summary>
        public void ForgetMe() {
            lock (_lock) {
                if (!PlayerPrefs.HasKey(PF_KEY_FORGET_ME)) {
                    var started = HasStarted;
                    delegated.ForgetMe();

                    delegated = new DDNANonTracking(this);
                    if (started) delegated.StartSDK(false);
                    delegated.ForgetMe();
                }
            }
        }
        
        public void StopTrackingMe() {
            lock (_lock) {
                if (!PlayerPrefs.HasKey(PF_KEY_STOP_TRACKING_ME)) {
                    var started = HasStarted;
                    delegated.StopTrackingMe();

                    delegated = new DDNANonTracking(this);
                    if (started) delegated.StartSDK(false);
                    delegated.StopTrackingMe();
                }
            }
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
            delegated.UseCollectTimestamp(useCollect);
        }

        /// <summary>
        /// If more control is required over the event timestamp source, you can override the default
        /// behaviour with a function that returns a DateTime.
        /// </summary>
        /// <param name="TimestampFunc">Timestamp func.</param>
        public void SetTimestampFunc(Func<DateTime?> TimestampFunc) {
            delegated.SetTimestampFunc(TimestampFunc);
        }

        /// <summary>
        /// Sets the logging level. Choices are ERROR, WARNING, INFO or DEBUG. Default is WARNING.
        /// </summary>
        public void SetLoggingLevel(Logger.Level level) {
            Logger.SetLogLevel(level);
        }

        /// <summary>
        /// Controls default behaviour of the SDK.  Set prior to initialisation.
        /// </summary>
        public Settings Settings { get; set; }

        /// <summary>
        /// Helper for Android push notifications.
        /// </summary>
        public AndroidNotifications AndroidNotifications { get; private set; }

        #if !DDNA_IOS_PUSH_NOTIFICATIONS_REMOVED
        /// <summary>
        /// Helper for iOS push notifications.
        /// </summary>
        public IosNotifications IosNotifications { get; private set; }
        #endif

        /// <summary>
        /// The EngageFactory helps with using the Engage service.
        /// </summary>
        /// <value>The engage factory.</value>
        public EngageFactory EngageFactory {
            get { return delegated.EngageFactory; }
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
            get { return collectURL; }
            private set { collectURL = Utils.FixURL(value); }
        }

        /// <summary>
        /// Gets the engage URL.
        /// </summary>
        public string EngageURL {
            get { return engageURL; }
            private set { engageURL = Utils.FixURL(value); }
        }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public string UserID {
            get {
                string v = PlayerPrefs.GetString(PF_KEY_USER_ID, null);
                if (String.IsNullOrEmpty(v)) {
                    return null;
                }
                return v;
            }
            private set {
                if (!String.IsNullOrEmpty(value)) {
                    PlayerPrefs.SetString(PF_KEY_USER_ID, value);
                    PlayerPrefs.Save();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialised.
        /// </summary>
        public bool HasStarted { get { return delegated.HasStarted; }}

        /// <summary>
        /// Gets a value indicating whether an event upload is in progress.
        /// </summary>
        public bool IsUploading { get { return delegated.IsUploading; }}

        #endregion
        #region Client Configuration

        /// <summary>
        /// To enable hashing of your event and engage data, set this value to your
        /// unique hash secret.  You must also enable hashing for the environment.
        /// To disable hashing set it to null, which is the default.  This must be
        /// set before calling <see cref="Start"/>.
        /// </summary>
        public string HashSecret { get; set; }

        /// <summary>
        /// A version string for your game that will be reported to us.  This must
        /// be set before calling <see cref="Start"/>.
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// By default we detect the platform field for your events.  You can override
        /// this value, make sure to set it before calling <see cref="Start"/>.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// The cross game user ID to be used for cross promotion. May be <code>null</code>
        /// or empty if not set.
        /// </summary>
        public string CrossGameUserID {
            get { return delegated.CrossGameUserID; }
            set { delegated.CrossGameUserID = value; }
        }

        /// <summary>
        /// The Android registration ID that is associated with this device if it's running
        /// on the Android platform.  This must be set before calling <see cref="Start"/>.
        /// </summary>
        public string AndroidRegistrationID {
            get { return delegated.AndroidRegistrationID; }
            set { delegated.AndroidRegistrationID = value; }
        }

        /// <summary>
        /// The push notification token from Apple that is associated with this device if
        /// it's running on the iOS platform.  This must be set before calling <see cref="Start"/>.
        /// </summary>
        public string PushNotificationToken {
            get { return delegated.PushNotificationToken; }
            set { delegated.PushNotificationToken = value; }
        }

        #endregion
        #region Helpers

        public override void OnDestroy() {
            PlayerPrefs.Save();
            delegated.OnDestroy();
            base.OnDestroy();
        }

        private void OnApplicationPause(bool pauseStatus) {
            if (pauseStatus) {
                PlayerPrefs.Save();
            }
            delegated.OnApplicationPause(pauseStatus);
        }

        internal virtual ImageMessageStore GetImageMessageStore() {
            return delegated.ImageMessageStore;
        }

        internal string ResolveEngageURL(string httpBody) {
            string url;
            if (httpBody != null && this.HashSecret != null) {
                string md5Hash = GenerateHash(httpBody, this.HashSecret);
                url = FormatURI(Settings.ENGAGE_HASH_URL_PATTERN, this.EngageURL, this.EnvironmentKey, md5Hash);
            } else {
                url = FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey, null);
            }
            return url;
        }

        internal void NotifyOnSessionConfigured(bool cached) {
            if (OnSessionConfigured != null) OnSessionConfigured(cached);
        }

        internal void NotifyOnSessionConfigurationFailed() {
            if (OnSessionConfigurationFailed != null) OnSessionConfigurationFailed();
        }

        internal void NotifyOnImageCachePopulated() {
            if (OnImageCachePopulated != null) OnImageCachePopulated();
        }

        internal void NotifyOnImageCachingFailed(string cause) {
            if (OnImageCachingFailed != null) OnImageCachingFailed(cause);
        }

        private string GenerateSessionID() {
            return Guid.NewGuid().ToString();
        }

        private string GenerateUserID() {
            return Guid.NewGuid().ToString();
        }

        internal static string GenerateHash(string data, string secret) {
            var inputBytes = Encoding.UTF8.GetBytes(data + secret);
            var hash = Utils.ComputeMD5Hash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        internal static string FormatURI(string uriPattern, string apiHost, string envKey, string hash) {
            var uri = uriPattern.Replace("{host}", apiHost);
            uri = uri.Replace("{env_key}", envKey);
            uri = uri.Replace("{hash}", hash);
            return uri;
        }

        #endregion
    }
}
