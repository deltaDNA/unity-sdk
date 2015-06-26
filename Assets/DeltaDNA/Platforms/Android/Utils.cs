#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android
{

	public class Utils {

		public const string UnityGcmListenerClassName = "com.deltadna.android.sdk.gcm.UnityGcmListener";
		public const string GcmClientClassName = "com.deltadna.android.sdk.gcm.GcmClient";

		public const string AdServiceClassName = "com.deltadna.android.sdk.ads.AdService";
		public const string AdServiceListenerClassName = "com.deltadna.android.sdk.ads.AdServiceListener";

		public const string UnityActivityClassName = "com.unity3d.player.UnityPlayer";

	}

}

#endif
