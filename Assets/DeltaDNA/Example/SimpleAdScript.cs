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
			
			if (GUI.Button(new Rect(250, 20, 100, 80), "Show Ad")) {
				
				StartCoroutine(ShowAdWhenReady());
			}
			
		}
		
		IEnumerator ShowAdWhenReady()
		{
			while (!DDNA.Instance.Ads.IsInterstitialAdReady()) {
				yield return null;
			}
			
			DDNA.Instance.Ads.ShowInterstitialAd();
		}
	}
}