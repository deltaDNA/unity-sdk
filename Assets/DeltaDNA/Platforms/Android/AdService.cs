#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android 
{
	internal class AdService 
	{
		private AndroidJavaObject adService;
		
		internal AdService(AdListener listener) {
			AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
			AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");			
			adService = new AndroidJavaObject(Utils.AdServiceClassName, activity, listener);
		}
			
		internal void RegisterForAds() {
			Logger.LogDebug("Registering for Ads");
			if (adService != null) {
				adService.Call("init");
			}
		}

		public bool IsInterstitialReady() {
			if (adService != null) {
				return adService.Call<bool>("isInterstitialAdReady");
			}
			return false;
		}
		
		public void ShowInterstitialAd() {
			Logger.LogDebug("Show Interstitial");
			if (adService != null) {
				adService.Call("showInterstitialAd");
			}
		}
		
		public void OnPause() {
			Logger.LogDebug("Ad Service OnPause");
			if (adService != null) {
				adService.Call("onPause");
			}
		}
		
		public void OnResume() {
			Logger.LogDebug("Ad Service OnResume");
			if (adService != null) {
				adService.Call("onResume");
			}
		}
		
		public void OnDestroy() {
			Logger.LogDebug("Ad Service OnDestroy");
			if (adService != null) {
				adService.Call("onDestroy");
			}
		}
	}
}

#endif