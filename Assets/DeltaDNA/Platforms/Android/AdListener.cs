#if UNITY_ANDROID

using UnityEngine;
using DeltaDNA;

namespace DeltaDNA.Android
{
	
	internal class AdListener : AndroidJavaProxy {
	
		private Ads ads;
		
		internal AdListener(Ads ads)
			: base(Utils.AdServiceListenerClassName) 
		{
			this.ads = ads;
		}
		
		void onRegisteredForAds() {
			ads.DidRegisterForAds();
		}
		
		void onFailedToRegisterForAds(string reason) {
			ads.DidFailToRegisterForAds(reason);
		}
		
		void onAdReady() {
			ads.AdReady();
		}
		
		void onAdClosed() {
			ads.AdClosed();
		}
		
		string toString() {
			return "AdListener";
		}
		
	}
	
}

#endif