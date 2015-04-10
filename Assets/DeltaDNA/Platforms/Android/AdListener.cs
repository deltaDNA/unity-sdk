#if UNITY_ANDROID

using UnityEngine;
using DeltaDNA.Notifications;

namespace DeltaDNA.Android
{
	
	internal class AdListener : AndroidJavaProxy {
		
		internal AdListener()
			: base(Utils.AdServiceListenerClassName) 
		{
			
		}
		
		void onRegisteredForAds() {
			Logger.LogDebug("Registered for ads");
		}
		
		void onFailedToRegisterForAds(string reason) {
			Logger.LogDebug("Failed to register for ads "+reason);
		}
		
		void onInterstitialAdReady() {
			Logger.LogDebug("Interstitial ad ready");
		}
		
		void onInterstitalAdClosed() {
			Logger.LogDebug("Interstitial ad closed");
		}
		
		void onVideoAdReady() {
			Logger.LogDebug("Video ad ready");
		}
		
		string toString() {
			return "AdListener";
		}
		
	}
	
}

#endif