#if UNITY_ANDROID

using UnityEngine;

namespace DeltaDNA.Android
{

	internal class DDNANotifications {
	
		private AndroidJavaClass ddnaNotifications;
		
		public DDNANotifications() {
            ddnaNotifications = new AndroidJavaClass(Utils.DdnaNotificationsClassName);
		}
		
		public void Register(AndroidJavaObject context) {
            ddnaNotifications.CallStatic("register", context);
		}

	}

}

#endif