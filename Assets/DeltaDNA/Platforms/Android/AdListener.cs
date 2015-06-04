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
		
		void onInterstitialAdReady() {
			ads.InterstitialAdReady();
		}
		
		void onInterstitialAdClosed() {
			ads.InterstitialAdClosed();
		}
		
		void onVideoAdReady() {
			ads.VideoAdReady();
		}
		
		void onVideoAdClosed() {
			ads.VideoAdClosed();
		}
		
		string toString() {
			return "AdListener";
		}
		
	}
	
}

#endif