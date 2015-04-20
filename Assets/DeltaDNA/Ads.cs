using UnityEngine;
using System;

namespace DeltaDNA
{
	public class Ads : MonoBehaviour {
	
		#if UNITY_ANDROID
		private DeltaDNA.Android.AdService adService;	
		#endif
		
		#region Public interface
	
		public void RegisterForAds()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				adService = new DeltaDNA.Android.AdService(new DeltaDNA.Android.AdListener(this));	
				adService.RegisterForAds();	
				#endif
			}
		}
		
		public bool IsInterstitialAdReady()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					return adService.IsInterstitialReady();	
				}	
				Logger.LogError("You must first register for ads");
				#endif
			}
			
			return false;
		}
		
		public void ShowInterstitialAd()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowInterstitialAd();
				} else {
					Logger.LogError("You must first register for ads");
				}
				#endif
			}
		}
		
		public bool IsVideoAdReady()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					return adService.IsVideoReady();	
				}	
				Logger.LogError("You must first register for ads");
				#endif
			}
			
			return false;
		}
		
		public void ShowVideoAd()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowVideoAd();
				} else {
					Logger.LogError("You must first register for ads");
				}
				#endif
			}
		}
		
		#endregion
		
		#region Native Bridge
		
		public void DidRegisterForAds()
		{
					
		}
		
		public void DidFailToRegisterForAds(string reason)
		{
			
		}
		
		public void InterstitialAdReady()
		{
			
		}
		
		public void InterstitialAdClosed()
		{
			
		}
		
		public void VideoAdReady()
		{
			
		}
		
		public void VideoAdClosed()
		{
		
		}
		
		#endregion
		
		void Awake()
		{
			gameObject.name = this.GetType().ToString();
			DontDestroyOnLoad(this);
		}
		
		void OnApplicationPause(bool pauseStatus)
		{
			if (Application.platform == RuntimePlatform.Android) {
				
				#if UNITY_ANDROID
				if (adService != null) {
					if (pauseStatus) {
						adService.OnPause();
					} else {
						adService.OnResume();
					}
				}
				#endif 	
			}
		}
		
		void OnDestroy()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.OnDestroy();
				}			
				#endif
			}
		}
	}
}