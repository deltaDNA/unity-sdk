using UnityEngine;
using System.Collections;

namespace DeltaDNA
{
	public class SimpleAdScript : MonoBehaviour {
	
		// Use this for initialization
		void Start () {
			// Ads	
			
			DDNA.Instance.Ads.OnDidRegisterForAds += () => { Logger.LogDebug("Registered for ads.");};
			DDNA.Instance.Ads.OnDidFailToRegisterForAds += (string reason) => { Logger.LogDebug("Failed to register for ads, "+reason);};
			DDNA.Instance.Ads.OnAdReady += () => { Logger.LogDebug("An ad is ready.");};
			DDNA.Instance.Ads.OnAdClosed += () => { Logger.LogDebug("Ad closed.");};
			DDNA.Instance.Ads.OnInterstitialAdReady += () => { Logger.LogDebug("An interstitial ad is ready.");};
			DDNA.Instance.Ads.OnInterstitialAdClosed += () => { Logger.LogDebug("Ad interstitial closed.");};
			DDNA.Instance.Ads.OnVideoAdReady += () => { Logger.LogDebug("A video ad is ready.");};
			DDNA.Instance.Ads.OnVideoAdClosed += () => { Logger.LogDebug("A video closed.");};
									
			DDNA.Instance.Ads.RegisterForAds();
		}
		
		void OnGUI() {
		
			if (GUI.Button(new Rect(250, 20, 200, 80), "Show Ad")) {
				
				DDNA.Instance.Ads.ShowAd("testAdPoint");
			}
			
			if (GUI.Button(new Rect(250, 120, 200, 80), "Show Interstitial")) {
				
				DDNA.Instance.Ads.ShowInterstitialAd();
			}
			
			if (GUI.Button(new Rect(250, 220, 200, 80), "Show Video")) {
				
				DDNA.Instance.Ads.ShowVideoAd();
			}
			
		}
	}
}