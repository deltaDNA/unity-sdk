#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android
{

	internal class GcmClient {
	
		private AndroidJavaObject gcmClient;
		
		public GcmClient(GcmListener listener) {
			
			AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
			AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
			gcmClient = new AndroidJavaObject(Utils.GcmClientClassName, activity, listener);
			
		}
		
		public void RegisterForGcm(string senderId) {
			gcmClient.Call("registerForGcm", senderId);
		}
	
	}

}

#endif