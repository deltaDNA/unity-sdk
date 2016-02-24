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
using UnityEngine;

namespace DeltaDNA
{
    
    /// <summary>
    /// Android Notifications enables a game to register with Google's GCM service.  Is uses
    /// our native android plugin to retreive the registration id required to send a push
    /// notification to an Android game.  This id is sent to our platform with each gameStarted event.
    /// </summary>
    public class AndroidNotifications : MonoBehaviour
    {
        #if UNITY_ANDROID
        private Android.DDNANotifications ddnaNotifications;
        #endif
        
        // Called with the registrationId.
        public event Action<string> OnDidRegisterForPushNotifications;
        // Called with the error string.
        public event Action<string> OnDidFailToRegisterForPushNotifications;
        
        void Awake()
        {
            gameObject.name = this.GetType().ToString();
            DontDestroyOnLoad(this);
        }
        
        /// <summary>
        /// Registers for push notifications.
        /// </summary>
        public void RegisterForPushNotifications()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                #if UNITY_ANDROID
                ddnaNotifications = new Android.DDNANotifications();
                ddnaNotifications.Register(
                    new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity"));
                #endif
            }
        }
        
        /// <summary>
        /// Unregisters for push notifications.  Google recommends not calling this,
        /// better to not send messages.
        /// </summary>
        public void UnregisterForPushNotifications()
        {
            if (Application.platform == RuntimePlatform.Android) {
                #if UNITY_ANDROID
                DDNA.Instance.AndroidRegistrationID = null;
                #endif
            }
        }
        
        #region Native Bridge
        
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
            Logger.LogDebug("Did fail to register for push notifications: "+error);
            
            if (OnDidFailToRegisterForPushNotifications != null) {
                OnDidFailToRegisterForPushNotifications(error);
            }
        }
        
        #endregion
        
    }

} // namespace DeltaDNA
