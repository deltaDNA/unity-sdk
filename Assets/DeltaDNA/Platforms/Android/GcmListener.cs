#if UNITY_ANDROID

using UnityEngine;
using DeltaDNA.Notifications;

namespace DeltaDNA.Android
{

	internal class GcmListener : AndroidJavaProxy {
	
		private AndroidNotifications notifications;
	
		internal GcmListener(AndroidNotifications notifications)
			: base(Utils.UnityGcmListenerClassName) 
		{
			this.notifications = notifications;
		}
		
		void onRegisteredForGcm(string registrationId) {
			notifications.DidRegisterForPushNotifications(registrationId);
		}
		
		void onFailedToRegisterForGcm(string reason) {
			notifications.DidFailToRegisterForPushNotifications(reason);
		}
		
	}

}

#endif