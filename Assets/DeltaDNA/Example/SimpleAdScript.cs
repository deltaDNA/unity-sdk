using UnityEngine;
using System.Collections;

namespace DeltaDNA
{
	public class SimpleAdScript : MonoBehaviour {
	
		// Use this for initialization
		void Start () {
			// Ads			
			DDNA.Instance.Ads.RegisterForAds();
		}
		
		void OnGUI() {
			
			if (GUI.Button(new Rect(250, 20, 200, 80), "Show Interstitial")) {
				
				StartCoroutine(ShowInterstitialWhenReady());
			}
			
			if (GUI.Button(new Rect(250, 120, 200, 80), "Show Video")) {
				
				StartCoroutine(ShowVideoWhenReady());
			}
			
		}
		
		IEnumerator ShowInterstitialWhenReady()
		{
			while (!DDNA.Instance.Ads.IsInterstitialAdReady()) {
				yield return null;
			}
			
			DDNA.Instance.Ads.ShowInterstitialAd();
		}
		
		IEnumerator ShowVideoWhenReady()
		{
			while (!DDNA.Instance.Ads.IsVideoAdReady()) {
				yield return null;
			}
			
			DDNA.Instance.Ads.ShowVideoAd();
		}
	}
}