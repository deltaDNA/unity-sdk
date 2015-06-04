#if UNITY_ANDROID

using UnityEngine;
using System.Collections;

namespace DeltaDNA.Android 
{
	internal class AdService 
	{
		private static readonly string DECISION_POINT = "advertising";
	
		private AndroidJavaObject adService;
		
		internal AdService(AdListener listener) {
			AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
			AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");			
			this.adService = new AndroidJavaObject(Utils.AdServiceClassName, activity, listener);
		}
			
		internal void RegisterForAds() {
			Logger.LogDebug("Registering for Ads");
			if (adService != null) {
			
				DDNA ddna = DDNA.Instance;
			
				adService.Call("init", 
					DECISION_POINT,
					ddna.EngageURL, 
					ddna.CollectURL,
				    ddna.EnvironmentKey, 
				    ddna.HashSecret, 
				    ddna.UserID, 
				    ddna.SessionID, 
				    DeltaDNA.Settings.ENGAGE_API_VERSION, 
				    DeltaDNA.Settings.SDK_VERSION, 
				    ddna.Platform, 
				    DeltaDNA.ClientInfo.TimezoneOffset, 
				    DeltaDNA.ClientInfo.Manufacturer, 
				    DeltaDNA.ClientInfo.OperatingSystemVersion
				);
			}
		}
		
		internal bool IsAdReady() {
			if (adService != null) {
				return adService.Call<bool>("isAdReady");
			}
			return false;
		}
		
		internal void ShowAd(string adPoint) {
			Logger.LogDebug("Show Ad "+adPoint);
			if (adService != null) {
				adService.Call("showAd", adPoint);
			}
		}

		internal bool IsInterstitialReady() {
			if (adService != null) {
				return adService.Call<bool>("isInterstitialAdReady");
			}
			return false;
		}
		
		internal void ShowInterstitialAd() {
			Logger.LogDebug("Show Interstitial");
			if (adService != null) {
				adService.Call("showInterstitialAd");
			}
		}
		
		internal bool IsVideoReady() {
			if (adService != null) {
				return adService.Call<bool>("isVideoAdReady");
			}
			return false;
		}
		
		internal void ShowVideoAd() {
			Logger.LogDebug("Show Video");
			if (adService != null) {
				adService.Call("showVideoAd");
			}
		}
		
		internal void OnPause() {
			Logger.LogDebug("Ad Service OnPause");
			if (adService != null) {
				adService.Call("onPause");
			}
		}
		
		internal void OnResume() {
			Logger.LogDebug("Ad Service OnResume");
			if (adService != null) {
				adService.Call("onResume");
			}
		}
		
		internal void OnDestroy() {
			Logger.LogDebug("Ad Service OnDestroy");
			if (adService != null) {
				adService.Call("onDestroy");
			}
		}
	}
}

#endif