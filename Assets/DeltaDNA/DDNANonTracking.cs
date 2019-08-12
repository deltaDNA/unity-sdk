//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
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
using UnityEngine;

namespace DeltaDNA {

    using JSONObject = Dictionary<string, object>;

    internal class DDNANonTracking : DDNABase {

        private bool started;
        private bool uploading;

        internal DDNANonTracking(DDNA ddna) : base(ddna) {
            EngageFactory = new EngageFactory(this, null);
        }

        #region Unity Lifecycle

        override internal void OnApplicationPause(bool pauseStatus) {}
        override internal void OnDestroy() {}

        #endregion
        #region Client Interface

        override internal void StartSDK(bool newPlayer) {
            started = true;
            NewSession();

            if (PlayerPrefs.HasKey(DDNA.PF_KEY_FORGET_ME)
                && !PlayerPrefs.HasKey(DDNA.PF_KEY_FORGOTTEN)) {
                ForgetMe();
            }
        }

        override internal void StopSDK() {
            started = false;
        }

        override internal EventAction RecordEvent<T>(T gameEvent) {
            return EventAction.CreateEmpty(gameEvent as GameEvent);
        }

        override internal EventAction RecordEvent(string eventName) {
            return RecordEvent(new GameEvent(eventName));
        }

        override internal EventAction RecordEvent(string eventName, Dictionary<string, object> eventParams) {
            return RecordEvent(new GameEvent(eventName));
        }

        override internal void RequestEngagement(Engagement engagement, Action<Dictionary<string, object>> callback) {
            callback(new JSONObject());
        }

        override internal void RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError) {
            engagement.StatusCode = 200;
            engagement.Raw = "{}";
            engagement.Error = null;

            onCompleted(engagement);
        }

        override internal void RecordPushNotification(Dictionary<string, object> payload) {}

        override internal void RequestSessionConfiguration() {
            ddna.NotifyOnSessionConfigured(false);
        }

        override internal void Upload() {}

        override internal void DownloadImageAssets() {
            ddna.NotifyOnImageCachePopulated();
        }

        override internal void ClearPersistentData() {}

        internal override void ForgetMe() {
            if (PlayerPrefs.HasKey(DDNA.PF_KEY_FORGOTTEN)) {
                Logger.LogDebug("Already forgotten user " + UserID);
                return;
            }

            Logger.LogDebug("Forgetting user " + UserID);
            PlayerPrefs.SetInt(DDNA.PF_KEY_FORGET_ME, 1);

            if (IsUploading) return;

            var advertisingId = PlayerPrefs.GetString(DDNA.PF_KEY_ADVERTISING_ID);
            var dictionary = new Dictionary<string, object>() {
                    { "eventName", "ddnaForgetMe" },
                    { "eventTimestamp", GetCurrentTimestamp() },
                    { "eventUUID", Guid.NewGuid().ToString() },
                    { "sessionID", SessionID },
                    { "userID", UserID },
                    { "eventParams", new Dictionary<string, object>() {
                        { "platform", Platform },
                        { "sdkVersion", Settings.SDK_VERSION },
                        { "ddnaAdvertisingId", advertisingId }
                    }}};
            if (string.IsNullOrEmpty(advertisingId)) {
                (dictionary["eventParams"] as Dictionary<string, object>)
                    .Remove("ddnaAdvertisingId");
            }

            string json;
            try {
                json = MiniJSON.Json.Serialize(dictionary);
            } catch (Exception e) {
                Logger.LogWarning("Unable to generate JSON for 'ddnaForgetMe' event. " + e.Message);
                return;
            }

            var url = (HashSecret != null)
                ? DDNA.FormatURI(
                    Settings.COLLECT_HASH_URL_PATTERN.Replace("/bulk", ""),
                    CollectURL,
                    EnvironmentKey,
                    DDNA.GenerateHash(json, HashSecret))
                : DDNA.FormatURI(
                    Settings.COLLECT_URL_PATTERN.Replace("/bulk", ""),
                    CollectURL,
                    EnvironmentKey,
                    null);

            HttpRequest request = new HttpRequest(url) {
                HTTPMethod = HttpRequest.HTTPMethodType.POST,
                HTTPBody = json
            };
            request.setHeader("Content-Type", "application/json");

            StartCoroutine(Send(
                request,
                () => {
                    Logger.LogDebug("Forgot user " + UserID);
                    PlayerPrefs.SetInt(DDNA.PF_KEY_FORGOTTEN, 1);
                }));
        }

        internal override void StopTrackingMe()
        {
            if (PlayerPrefs.HasKey(DDNA.PF_KEY_FORGOTTEN)) {
                Logger.LogDebug("Already forgotten user " + UserID);
                return;
            }
            if (PlayerPrefs.HasKey(DDNA.PF_KEY_STOP_TRACKING_ME)) {
                Logger.LogDebug("Already stopped tracking user " + UserID);
                return;
            }
            
            Logger.LogDebug(" Stopped Tracking " + UserID);
            PlayerPrefs.SetInt(DDNA.PF_KEY_STOP_TRACKING_ME, 1);
        }

        #endregion
        #region Properties

        override internal bool HasStarted { get { return started; }}
        override internal bool IsUploading { get { return uploading; }}

        #endregion
        #region Client Configuration

        override internal string CrossGameUserID { get; set; }
        override internal string PushNotificationToken { get; set; }
        override internal string AndroidRegistrationID { get; set; }

        #endregion
        #region Implementation

        private System.Collections.IEnumerator Send(HttpRequest request, Action onSuccess) {
            int attempts = 0;
            bool succeeded = false;

            Action<int, string, string> onCompletion = (statusCode, data, error) => {
                if (statusCode > 0 && statusCode < 400) {
                    succeeded = true;
                    onSuccess();
                } else {
                    Logger.LogDebug("Error posting events: " + error + " " + data);
                }
            };

            do {
                uploading = true;
                yield return StartCoroutine(Network.SendRequest(request, onCompletion));

                if (succeeded || ++attempts < Settings.HttpRequestMaxRetries) {
                    uploading = false;
                    break;
                }

                yield return new WaitForSeconds(Settings.HttpRequestRetryDelaySeconds);
            } while (attempts < Settings.HttpRequestMaxRetries);
        }

        #endregion
    }
}
