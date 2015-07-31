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
		
		internal void ShowAd() {
			Logger.LogDebug("Show Ad");
			if (adService != null) {
				adService.Call("showAd");
			}
		}
		
		internal void ShowAd(string adPoint) {
			if (string.IsNullOrEmpty(adPoint)) {
				this.ShowAd();
			} else if (adService != null) {
				Logger.LogDebug("Show Ad "+adPoint);
				adService.Call("showAd", adPoint);
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