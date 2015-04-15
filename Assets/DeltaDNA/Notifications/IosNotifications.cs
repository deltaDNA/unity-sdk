using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeltaDNA.Notifications
{

/// <summary>
/// iOS Notifications Plugin enables a game to register with Apple's push notification service.  It provides
/// some additional functionality not easily accessible from Unity.  By using the events, a game can be
/// notified when a game has registered with the service and when push notification has occured.  We use
/// these events to log notifications with the DeltaDNA platform.
/// </summary>
public class IosNotifications : MonoBehaviour
{
	// Called with JSON string of the notification payload.
	public event Action<string> OnDidLaunchWithPushNotification;

	// Called with JSON string of the notification payload.
	public event Action<string> OnDidReceivePushNotification;

	// Called with the deviceToken.
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
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
        
        	#if UNITY_IPHONE    
        	#if UNITY_4_5 || UNITY_4_6		
			NotificationServices.RegisterForRemoteNotificationTypes(
				RemoteNotificationType.Alert |
				RemoteNotificationType.Badge |
				RemoteNotificationType.Sound);
			#else
			UnityEngine.iOS.NotificationServices.RegisterForNotifications(
				UnityEngine.iOS.NotificationType.Alert |
				UnityEngine.iOS.NotificationType.Badge |
				UnityEngine.iOS.NotificationType.Sound);		
			#endif
			#endif
        }
    }

	/// <summary>
	/// Unregisters for push notifications.
	/// </summary>
    public void UnregisterForPushNotifications()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
        	#if UNITY_IPHONE
        	#if UNITY_4_5 || UNITY_4_6 
			NotificationServices.UnregisterForRemoteNotifications();
			#else
			UnityEngine.iOS.NotificationServices.UnregisterForRemoteNotifications();
            #endif
			#endif
        }
    }

	#region Native Bridge

    public void DidLaunchWithPushNotification(string notification)
    {
		Logger.LogDebug("Did launch with iOS push notification");

    	var payload = DeltaDNA.MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
    	DDNA.Instance.RecordPushNotification(payload);

    	if (OnDidLaunchWithPushNotification != null) {
    		OnDidLaunchWithPushNotification(notification);
    	}
    }

    public void DidReceivePushNotification(string notification)
    {
		Logger.LogDebug("Did receive iOS push notification");

		var payload = DeltaDNA.MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
		DDNA.Instance.RecordPushNotification(payload);

    	if (OnDidReceivePushNotification != null) {
    		OnDidReceivePushNotification(notification);
    	}
    }

    public void DidRegisterForPushNotifications(string deviceToken)
    {
		Logger.LogDebug("Did register for iOS push notifications: "+deviceToken);

        DDNA.Instance.PushNotificationToken = deviceToken;

        if (OnDidRegisterForPushNotifications != null) {
            OnDidRegisterForPushNotifications(deviceToken);
        }
    }

    public void DidFailToRegisterForPushNotifications(string error)
    {
		Logger.LogDebug("Did fail to register for iOS push notifications: "+error);

        if (OnDidFailToRegisterForPushNotifications != null) {
            OnDidFailToRegisterForPushNotifications(error);
        }
    }

    #endregion

}

} // namespace DeltaDNA
