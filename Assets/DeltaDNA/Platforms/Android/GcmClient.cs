#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android
{

	internal class GcmClient {
	
		private AndroidJavaObject gcmClient;
		
		public GcmClient() {
			gcmClient = new AndroidJavaObject(Utils.GcmClientClassName);
		}
		
		public void Register(string senderId) {
			gcmClient.Call("register", senderId);
		}
		
		public void Unregister() {
			gcmClient.Call("unregister");
		}
	
	}

}

#endif