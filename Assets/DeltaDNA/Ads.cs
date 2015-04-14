using UnityEngine;
using System;

namespace DeltaDNA
{
	public class Ads : MonoBehaviour {
	
		#if UNITY_ANDROID
		private DeltaDNA.Android.AdService adService;	
		#endif
		
		#region Public interface
		
		public event Action OnDidRegisterForAds;
		public event Action<string> OnDidFailToRegisterForAds;
		public event Action OnInterstitialAdReady;
		public event Action OnInterstitialAdClosed;
	
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
		
		#endregion
		
		#region Native Bridge
		
		public void DidRegisterForAds()
		{
			if (OnDidRegisterForAds != null) {
				OnDidRegisterForAds();
			}			
		}
		
		public void DidFailToRegisterForAds(string reason)
		{
			if (OnDidFailToRegisterForAds != null) {
				OnDidFailToRegisterForAds(reason);
			}
		}
		
		public void InterstitialAdReady()
		{
			if (OnInterstitialAdReady != null) {
				OnInterstitialAdReady();
			}
		}
		
		public void InterstitialAdClosed()
		{
			if (OnInterstitialAdClosed != null) {
				OnInterstitialAdClosed();
			}
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