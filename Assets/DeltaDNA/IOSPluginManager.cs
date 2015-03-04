using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeltaDNA
{

public class IOSPluginManager : MonoBehaviour
{
    #if UNITY_IPHONE

    [DllImport("__Internal")]
    private static extern void _registerForPushNotifications();

    public static void RegisterForPushNotifications()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            _registerForPushNotifications();
        }
    }

    [DllImport("__Internal")]
    private static extern void _unregisterForPushNotifications();

    public static void UnregisterForPushNotifications()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            _unregisterForPushNotifications();
        }
    }
    
    // Notification events
    public static event Action<string> didLaunchWithPushNotificationEvent;
    public static event Action<string> didReceivePushNotificationEvent;
    public static event Action<string> didRegisterForPushNotificationsEvent;
    public static event Action<string> didFailToRegisterForPushNotificationsEvent;

    void Awake()
    {
        gameObject.name = this.GetType().ToString();
        DontDestroyOnLoad(this);
    }
    
    public void didLaunchWithPushNotification(string notification)
    {
    	Debug.Log ("DidLaunchWithPushNotification: "+notification);
    	
    	var payload = DeltaDNA.MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
    	DDNA.Instance.RecordPushNotification(payload);
    	
    	if (didLaunchWithPushNotificationEvent != null) {
    		didLaunchWithPushNotificationEvent(notification);
    	}
    }
    
    public void didReceivePushNotification(string notification)
    {
    	Debug.Log ("DidReceivePushNotification: "+notification);
    	
		var payload = DeltaDNA.MiniJSON.Json.Deserialize(notification) as Dictionary<string, object>;
		DDNA.Instance.RecordPushNotification(payload);
    	
    	if (didReceivePushNotificationEvent != null) {
    		didReceivePushNotificationEvent(notification);
    	}
    }

    public void didRegisterForPushNotifications(string deviceToken)
    {
        DDNA.Instance.PushNotificationToken = deviceToken;

        if (didRegisterForPushNotificationsEvent != null) {
            didRegisterForPushNotificationsEvent(deviceToken);
        }
    }

    public void didFailToRegisterForPushNotifications(string error)
    {
        if (didFailToRegisterForPushNotificationsEvent != null) {
            didFailToRegisterForPushNotificationsEvent(error);
        }
    }

    #endif // UNITY_IPHONE
}

} // namespace DeltaDNA
