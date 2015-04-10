#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android
{

	public class Utils {
	
		public const string UnityGcmListenerClassName = "com.deltadna.unity.gcm.UnityGcmListener";
		public const string GcmClientClassName = "com.deltadna.unity.gcm.GcmClient";
		
		public const string AdServiceClassName = "com.deltadna.unity.ads.AdService";
		public const string AdServiceListenerClassName = "com.deltadna.unity.ads.AdServiceListener";
				
		public const string UnityActivityClassName = "com.unity3d.player.UnityPlayer";
		
	}

}

#endif
