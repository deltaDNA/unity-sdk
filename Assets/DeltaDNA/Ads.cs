using UnityEngine;
using System.Collections;

namespace DeltaDNA
{
	public class Ads : MonoBehaviour {
	
		#if UNITY_ANDROID
		private DeltaDNA.Android.AdService adService;	
		#endif
	
		void Awake()
		{
			gameObject.name = this.GetType().ToString();
			DontDestroyOnLoad(this);
		}
		
		void OnApplicationPause(bool pauseStatus)
		{
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
		
		void OnDestroy()
		{
			#if UNITY_ANDROID
			if (adService != null) {
				adService.OnDestroy();
			}			
			#endif
		}
	
		public void RegisterForAds()
		{
			#if UNITY_ANDROID
			adService = new DeltaDNA.Android.AdService(new DeltaDNA.Android.AdListener());	
			adService.RegisterForAds();	
			#endif
		}
		
		public bool IsInterstitialAdReady()
		{
			#if UNITY_ANDROID
			if (adService != null) {
				return adService.IsInterstitialReady();	
			}	
			Logger.LogError("You must first register for ads");
			return false;
			#endif
		}
		
		public void ShowInterstitialAd()
		{
			#if UNITY_ANDROID
			if (adService != null) {
				adService.ShowInterstitialAd();
			} else {
				Logger.LogError("You must first register for ads");
			}
			#endif
		}
	}
}