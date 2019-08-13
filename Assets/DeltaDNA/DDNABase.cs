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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DeltaDNA {

    internal abstract class DDNABase {

        protected static Func<DateTime?> TimestampFunc = new Func<DateTime?>(DefaultTimestampFunc);

        protected readonly DDNA ddna;
        protected readonly GameObject gameObject;

        internal DDNABase(DDNA ddna) {
            this.ddna = ddna;

            gameObject = ddna.gameObject;
        }

        #if UNITY_EDITOR
        internal DDNABase() {
            ddna = null;
            gameObject = null;
        }
        #endif

        #region Unity Lifecycle

        internal abstract void OnApplicationPause(bool pauseStatus);
        internal abstract void OnDestroy();

        #endregion
        #region Client Interface

        internal abstract void StartSDK(bool newPlayer);
        internal abstract void StopSDK();

        internal abstract EventAction RecordEvent<T>(T gameEvent) where T : GameEvent<T>;
        internal abstract EventAction RecordEvent(string eventName);
        internal abstract EventAction RecordEvent(string eventName, Dictionary<string, object> eventParams);

        internal abstract void RequestEngagement(Engagement engagement, Action<Dictionary<string, object>> callback);
        internal abstract void RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError);

        internal abstract void RecordPushNotification(Dictionary<string, object> payload);

        internal abstract void RequestSessionConfiguration();

        internal abstract void Upload();
        internal abstract void DownloadImageAssets();

        internal abstract void ClearPersistentData();
        internal abstract void ForgetMe();
        internal abstract void StopTrackingMe();
        internal ImageMessageStore ImageMessageStore { get; set; }
        internal EngageFactory EngageFactory { get; set; }

        #endregion
        #region Properties

        protected string EnvironmentKey { get { return ddna.EnvironmentKey; }}
        protected string CollectURL { get { return ddna.CollectURL; }}
        protected string EngageURL { get { return ddna.EngageURL; }}
        protected string Platform { get { return ddna.Platform; }}
        protected string HashSecret { get {return ddna.HashSecret; }}
        protected string ClientVersion { get { return ddna.ClientVersion; }}
        protected Settings Settings { get { return ddna.Settings; }}

        protected string UserID { get { return ddna.UserID; }}
        protected string SessionID { get { return ddna.SessionID; }}

        internal abstract bool HasStarted { get; }
        internal abstract bool IsUploading { get; }

        #endregion
        #region Client Configuration

        internal abstract string CrossGameUserID { get; set; }
        internal abstract string AndroidRegistrationID { get; set; }
        internal abstract string PushNotificationToken { get; set; }

        #endregion
        #region Implementation

        protected Coroutine StartCoroutine(IEnumerator routine) {
            return ddna.StartCoroutine(routine);
        }

        protected void InvokeRepeating(string methodName, float time, float repeatRate) {
            ddna.InvokeRepeating(methodName, time, repeatRate);
        }

        protected bool IsInvoking(string methodName) {
            return ddna.IsInvoking(methodName);
        }

        protected void CancelInvoke() {
            ddna.CancelInvoke();
        }

        protected void NewSession() {
          ddna.NewSession();
        }

        internal void UseCollectTimestamp(bool useCollect) {
            if (!useCollect) {
                SetTimestampFunc(DefaultTimestampFunc);
            } else {
                SetTimestampFunc(() => { return null; });
            }
        }

        internal void SetTimestampFunc(Func<DateTime?> TimestampFunc) {
            DDNABase.TimestampFunc = TimestampFunc;
        }

        protected static string GetCurrentTimestamp() {
            DateTime? dt = TimestampFunc();
            if (dt.HasValue) {
                String ts = dt.Value.ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
                // fix for millisecond timestamp format bug seen on Android.
                if (ts.EndsWith(".1000")) {
                    ts = ts.Replace(".1000", ".999");
                }
                return ts;
            }

            return null; // Collect will insert a timestamp for us.
        }

        private static DateTime? DefaultTimestampFunc() {
            return DateTime.UtcNow;
        }

        #endregion
    }
}
