using UnityEngine;
using System.Collections;

namespace DeltaDNA
{
	public class SimpleAdScript : MonoBehaviour {
	
		// Use this for initialization
		void Start () {
			// Ads
			DDNA.Instance.Ads.OnDidRegisterForAds += () => { Debug.Log("Registered for Ads.");};
			DDNA.Instance.Ads.OnDidFailToRegisterForAds += (string reason) => { Debug.Log ("Problem registering for Ads. "+reason);};
			DDNA.Instance.Ads.OnInterstitialAdReady += () => { Debug.Log("Ad ready to show");};
			DDNA.Instance.Ads.OnInterstitialAdClosed += () => { Debug.Log("Ad closed");};
			
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