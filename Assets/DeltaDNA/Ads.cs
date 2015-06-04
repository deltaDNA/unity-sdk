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
		public event Action OnAdReady;
		public event Action OnAdClosed;
		public event Action OnInterstitialAdReady;
		public event Action OnInterstitialAdClosed;
		public event Action OnVideoAdReady;
		public event Action OnVideoAdClosed;
	
		public void RegisterForAds()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				adService = new DeltaDNA.Android.AdService(new DeltaDNA.Android.AdListener(this));	
				adService.RegisterForAds();	
				#endif
			}
		}
		
		public bool IsAdReady()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					return adService.IsAdReady();	
				}	
				Logger.LogError("You must first register for ads");
				#endif
			}
			
			return false;
		}
		
		public void ShowAd(string adPoint)
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowAd(adPoint);
				} else {
					Logger.LogError("You must first register for ads");
				}
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
		
		public void AdReady()
		{
			if (OnAdReady != null) {
				OnAdReady();
			}
		}
		
		public void AdClosed()
		{
			if (OnAdClosed != null) {
				OnAdClosed();
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
		
		public void VideoAdReady()
		{
			if (OnVideoAdReady != null) {
				OnVideoAdReady();
			}
		}
		
		public void VideoAdClosed()
		{
			if (OnVideoAdClosed != null) {
				OnVideoAdClosed();
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