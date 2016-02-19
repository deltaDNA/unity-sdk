using System;
using UnityEngine;

namespace DeltaDNA.Notifications
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
