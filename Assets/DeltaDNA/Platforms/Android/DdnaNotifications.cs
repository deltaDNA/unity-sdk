#if UNITY_ANDROID

using UnityEngine;

namespace DeltaDNA.Android
{

	internal class DdnaNotifications {
	
		private AndroidJavaClass ddnaNotifications;
		
		public DdnaNotifications() {
            ddnaNotifications = new AndroidJavaClass(Utils.DdnaNotificationsClassName);
		}
		
		public void Register(AndroidJavaObject context) {
            ddnaNotifications.CallStatic("register", context);
		}

	}

}

#endif