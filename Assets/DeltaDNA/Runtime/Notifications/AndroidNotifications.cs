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
        /// <summary>
        /// Called with the registration id when the app registers for push
        /// notifications, or when the registration id is refreshed.
        /// </summary>
        public event Action<string> OnDidRegisterForPushNotifications;
        /// <summary>
        /// Called with the error string when registering for push notifications
        /// fails.
        /// </summary>
        public event Action<string> OnDidFailToRegisterForPushNotifications;

        /// <summary>
        /// Holds whether notifications are present in the project as they can
        /// be removed.
        /// </summary>
        private bool? notificationsPresent;

        void Awake()
        {
            gameObject.name = this.GetType().ToString();
            DontDestroyOnLoad(this);

            #if UNITY_ANDROID && !UNITY_EDITOR
            if (AreNotificationsPresent()) {
                ddnaNotifications = new Android.DDNANotifications();
                ddnaNotifications.MarkUnityLoaded();
            }
            #endif
        }

        /// <summary>
        /// Registers for push notifications.
        /// 
        /// If you have multiple Firebase Cloud Messaging senders in your project
        /// then you can use the deltaDNA sender either as a main one or as a
        /// secondary one by setting the secondary parameter. If you set secondary
        /// to true then the default FCM sender will need to have been initialised
        /// beforehand.
        /// 
        /// In the case when the app is already registered for push notifications
        /// and the registration id is up to date then the callbacks will not be
        /// invoked.
        /// </summary>
        /// <param name="secondary">Whether the Firebase instance used for the
        /// deltaDNA notifications should be registered as a secondary (non-main)
        /// instance.</param>
        public void RegisterForPushNotifications(bool secondary = false) {
            if (Application.platform == RuntimePlatform.Android && AreNotificationsPresent()) {
                #if UNITY_ANDROID && !UNITY_EDITOR
                try {
                    ddnaNotifications.Register(
                        new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                            .GetStatic<AndroidJavaObject>("currentActivity"),
                        secondary);
                } catch (AndroidJavaException e) {
                    Logger.LogWarning("Failed to register for push notifications. Notifications may not be configured correctly. " + e.Message);
                }
                #endif
            }
        }

        /// <summary>
        /// Unregisters for push notifications.
        /// </summary>
        public void UnregisterForPushNotifications()
        {
            if (Application.platform == RuntimePlatform.Android && AreNotificationsPresent()) {
                #if UNITY_ANDROID && !UNITY_EDITOR
                DDNA.Instance.AndroidRegistrationID = null;
                #endif
            }
        }

        private bool AreNotificationsPresent() {
            if (notificationsPresent == null) {
                try {
                    new AndroidJavaClass("com.deltadna.android.sdk.notifications.DDNANotifications");
                    notificationsPresent = true;
                } catch (AndroidJavaException) {
                    notificationsPresent = false;
                }
            }

            return (bool) notificationsPresent;
        }

        #region Native Bridge

        public void DidReceivePushNotification(string notification)
        {
            var payload = MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
            payload["_ddCommunicationSender"] = "GOOGLE_NOTIFICATION";

            if (payload["_ddLaunch"] as bool? ?? false) {
                Logger.LogDebug("Did launch with Android push notification");

                DDNA.Instance.RecordPushNotification(payload);

                if (OnDidLaunchWithPushNotification != null) {
                    OnDidLaunchWithPushNotification(notification);
                }
            } else {
                Logger.LogDebug("Did receive Android push notification");

                DDNA.Instance.RecordPushNotification(payload);

                if (OnDidReceivePushNotification != null) {
                    OnDidReceivePushNotification(notification);
                }
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
            Logger.LogWarning("Did fail to register for Android push notifications: "+error);

            if (OnDidFailToRegisterForPushNotifications != null) {
                OnDidFailToRegisterForPushNotifications(error);
            }
        }

        #endregion
    }

} // namespace DeltaDNA
