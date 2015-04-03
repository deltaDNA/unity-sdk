#if UNITY_ANDROID

using UnityEngine;

namespace DeltaDNA.Android
{

	internal class GcmListener : AndroidJavaProxy {
	
		private NotificationsPlugin notificationsPlugin;
	
		internal GcmListener(NotificationsPlugin notificationsPlugin)
			: base(Utils.UnityGcmListenerClassName) 
		{
			this.notificationsPlugin = notificationsPlugin;
		}
		
		void onRegisteredForGcm(string registrationId) {
			notificationsPlugin.DidRegisterForPushNotifications(registrationId);
		}
		
		void onFailedToRegisterForGcm(string reason) {
			notificationsPlugin.DidFailToRegisterForPushNotifications(reason);
		}
		
	}

}

#endif