//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
            DDNASmartAds.Instance.OnDidRegisterForInterstitialAds += () => {
                Debug.Log("Registered for interstitial ads.");
            };
            DDNASmartAds.Instance.OnDidFailToRegisterForInterstitialAds += (string reason) => {
                Debug.Log("Failed to register for interstitial ads, "+reason);
            };
            DDNASmartAds.Instance.OnInterstitialAdOpened += () => {
                Debug.Log("An interstitial ad opened.");
            };
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen += () => {
                Debug.Log("Failed to open an interstitial ad.");
            };
            DDNASmartAds.Instance.OnInterstitialAdClosed += () => {
                Debug.Log("An interstitial ad closed.");
            };
            DDNASmartAds.Instance.OnDidRegisterForRewardedAds += () => {
                Debug.Log("Registered for rewarded ads.");
            };
            DDNASmartAds.Instance.OnDidFailToRegisterForRewardedAds += (string reason) => {
                Debug.Log("Failed to register for rewarded ads, "+reason);
            };
            DDNASmartAds.Instance.OnRewardedAdOpened += () => {
                Debug.Log("A rewarded ad opened.");
            };
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen += () => {
                Debug.Log("Failed to open a rewarded ad.");
            };
            DDNASmartAds.Instance.OnRewardedAdClosed += (bool reward) => {
                Debug.Log("A rewarded ad closed. Should reward="+reward);
            };

            // Start collecting data
            DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL);

            // Register for ads
            DDNASmartAds.Instance.RegisterForAds();
        }

        void FixedUpdate() {
            // Make our cube rotate
            transform.Rotate(new Vector3(-15, -30, -45) * Time.deltaTime);
        }

        void OnGUI() {

            GUI.enabled = DDNASmartAds.Instance.IsInterstitialAdAvailable();

            if (GUI.Button(new Rect(10, 20, 200, 80), "Interstitial")) {
                DDNASmartAds.Instance.ShowInterstitialAd();
            }

            if (GUI.Button(new Rect(10, 120, 200, 80), "Interstitial with Ad Point")) {
                DDNASmartAds.Instance.ShowInterstitialAd("testDecisionPoint");
            }

            GUI.enabled = DDNASmartAds.Instance.IsRewardedAdAvailable();

            if (GUI.Button(new Rect(10, 220, 200, 80), "Rewarded Ad")) {
                DDNASmartAds.Instance.ShowRewardedAd();
            }

            if (GUI.Button(new Rect(10, 320, 200, 80), "Rewarded with Ad Point")) {
                DDNASmartAds.Instance.ShowRewardedAd("testDecisionPoint2");
            }

        }

    }
}
