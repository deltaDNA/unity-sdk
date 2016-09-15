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
using UnityEngine;

namespace DeltaDNA
{
    
    /// <summary>
    /// Android Notifications enables a game to register with Google's GCM service.
    /// Is uses our native Android plugin to retreive the registration id required
    /// to send a push notification to a game. This id is sent to our platform as
    /// appropriate.
    /// </summary>
    public class AndroidNotifications : MonoBehaviour
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        private Android.DDNANotifications ddnaNotifications;
        #endif

        /// <summary>
        /// Called with the JSON string of the notification payload when the
        /// user opens the app from the background through an interaction on
        /// the push notification.
        /// </summary>
        public event Action<string> OnDidLaunchWithPushNotification;
        /// <summary>
        /// Called with the JSON string of the notification payload when the
        /// push notification is received while the app is in the foreground.
        /// </summary>
        public event Action<string> OnDidReceivePushNotification;
        // Called with the registrationId.
        public event Action<string> OnDidRegisterForPushNotifications;
        // Called with the error string.
        public event Action<string> OnDidFailToRegisterForPushNotifications;
        
        void Awake()
        {
            gameObject.name = this.GetType().ToString();
            DontDestroyOnLoad(this);

            #if UNITY_ANDROID && !UNITY_EDITOR
            ddnaNotifications = new Android.DDNANotifications();
            ddnaNotifications.MarkUnityLoaded();
            #endif
        }
        
        /// <summary>
        /// Registers for push notifications.
        /// </summary>
        public void RegisterForPushNotifications()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                ddnaNotifications.Register(
                    new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity"));
                #endif
            }
        }
        
        /// <summary>
        /// Unregisters for push notifications.
        /// </summary>
        public void UnregisterForPushNotifications()
        {
            if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID && !UNITY_EDITOR
                DDNA.Instance.AndroidRegistrationID = null;
                #endif
            }
        }

        #region Native Bridge

        public void DidLaunchWithPushNotification(string notification)
        {
            Logger.LogDebug("Did launch with Android push notification");

            var payload = MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
            DDNA.Instance.RecordPushNotification(payload);

            if (OnDidLaunchWithPushNotification != null) {
                OnDidLaunchWithPushNotification(notification);
            }
        }

        public void DidReceivePushNotification(string notification)
        {
            Logger.LogDebug("Did receive Android push notification");

            var payload = MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
            DDNA.Instance.RecordPushNotification(payload);

            if (OnDidReceivePushNotification != null) {
                OnDidReceivePushNotification(notification);
            }
        }
        
        public void DidRegisterForPushNotifications(string registrationId)
        {
            Logger.LogDebug("Did register for Android push notifications: "+registrationId);
            
            DDNA.Instance.AndroidRegistrationID = registrationId;
            
            if (OnDidRegisterForPushNotifications != null) {
                OnDidRegisterForPushNotifications(registrationId);
            }
        }
        
        public void DidFailToRegisterForPushNotifications(string error)
        {
            Logger.LogDebug("Did fail to register for Android push notifications: "+error);
            
            if (OnDidFailToRegisterForPushNotifications != null) {
                OnDidFailToRegisterForPushNotifications(error);
            }
        }
        
        #endregion
        
    }

} // namespace DeltaDNA
