using UnityEngine;
using System.Collections;
using DeltaDNA;

namespace DeltaDNAAds
{
	public class AdsDemo : MonoBehaviour {
	
		public const string ENVIRONMENT_KEY = "76410301326725846610230818914037";
		public const string COLLECT_URL = "http://collect2470ntysd.deltadna.net/collect/api";
		public const string ENGAGE_URL = "http://engage2470ntysd.deltadna.net";
		public const string ENGAGE_TEST_URL = "http://www.deltadna.net/qa/engage";

		// Use this for initialization
		void Start () {
		
			// Configure the SDK
			DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
			DDNA.Instance.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";
			DDNA.Instance.ClientVersion = "1.0.0";
		
			// Setup Ad notifications
			DDNASmartAds.Instance.OnDidRegisterForAds += () => { DeltaDNA.Logger.LogDebug("Registered for ads.");};
			DDNASmartAds.Instance.OnDidFailToRegisterForAds += (string reason) => { DeltaDNA.Logger.LogDebug("Failed to register for ads, "+reason);};
			DDNASmartAds.Instance.OnAdOpened += () => { DeltaDNA.Logger.LogDebug("An ad opened.");};
			DDNASmartAds.Instance.OnAdFailedToOpen += () => { DeltaDNA.Logger.LogDebug("Failed to open ad.");};
			DDNASmartAds.Instance.OnAdClosed += () => { DeltaDNA.Logger.LogDebug("Ad closed.");};
			
			// Start collecting data
			DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL, DDNA.AUTO_GENERATED_USER_ID);

			// Register for ads
			DDNASmartAds.Instance.RegisterForAds();
		}
		
		void FixedUpdate() {
			// Make our cube rotate
			transform.Rotate(new Vector3(-15, -30, -45) * Time.deltaTime);
		}

		void OnGUI() {

			if (GUI.Button(new Rect(10, 20, 200, 80), "Show Ad")) {

				DDNASmartAds.Instance.ShowAd();
			}

			if (GUI.Button(new Rect(10, 120, 200, 80), "Engage Ad 1")) {

				DDNASmartAds.Instance.ShowAd("testAdPoint");
			}
			
			if (GUI.Button(new Rect(10, 220, 200, 80), "Engage Ad 2")) {
				
				DDNASmartAds.Instance.ShowAd("testAdPoint2");
			}

		}
		
	}
}
