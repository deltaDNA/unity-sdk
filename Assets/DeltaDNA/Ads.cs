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
			return adService.IsInterstitialReady();		
			#endif
		}
		
		public void ShowInterstitialAd()
		{
			#if UNITY_ANDROID
			adService.ShowInterstitialAd();
			#endif
		}
	}
}