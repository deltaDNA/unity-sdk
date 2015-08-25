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
			DDNA.Instance.Ads.OnAdOpened += () => { Logger.LogDebug("An ad opened.");};
			DDNA.Instance.Ads.OnAdFailedToOpen += () => { Logger.LogDebug("Failed to open ad.");};
			DDNA.Instance.Ads.OnAdClosed += () => { Logger.LogDebug("Ad closed.");};

			DDNA.Instance.Ads.RegisterForAds();
		}

		void OnGUI() {

			if (GUI.Button(new Rect(250, 20, 200, 80), "Show Ad")) {

				DDNA.Instance.Ads.ShowAd();
			}

			if (GUI.Button(new Rect(250, 120, 200, 80), "Engage Ad 1")) {

				DDNA.Instance.Ads.ShowAd("testAdPoint");
			}
			
			if (GUI.Button(new Rect(250, 220, 200, 80), "Engage Ad 2")) {
				
				DDNA.Instance.Ads.ShowAd("testAdPoint2");
			}

		}
		
	}
}
