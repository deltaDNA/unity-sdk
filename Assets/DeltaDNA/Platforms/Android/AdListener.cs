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
			Logger.LogDebug("Registered for ads");
			ads.DidRegisterForAds();
		}
		
		void onFailedToRegisterForAds(string reason) {
			Logger.LogDebug("Failed to register for ads "+reason);
			ads.DidFailToRegisterForAds(reason);
		}
		
		void onInterstitialAdReady() {
			Logger.LogDebug("Interstitial ad ready");
			ads.InterstitialAdReady();
		}
		
		void onInterstitialAdClosed() {
			Logger.LogDebug("Interstitial ad closed");
			ads.InterstitialAdClosed();
		}
		
		void onVideoAdReady() {
			Logger.LogDebug("Video ad ready");
			ads.VideoAdReady();
		}
		
		void onVideoAdClosed() {
			Logger.LogDebug("Video ad closed");
			ads.VideoAdClosed();
		}
		
		string toString() {
			return "AdListener";
		}
		
	}
	
}

#endif