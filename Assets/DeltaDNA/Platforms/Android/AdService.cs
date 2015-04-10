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
			adService.Call("init");
		}

		public bool IsInterstitialReady() {
			return adService.Call<bool>("isInterstitialAdReady");
		}
		
		public void ShowInterstitialAd() {
			adService.Call("showInterstitialAd");
		}
	}
}

#endif