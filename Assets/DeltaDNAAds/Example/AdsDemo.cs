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

        private int clickCount = 0;

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
            DDNASmartAds.Instance.OnDidRegisterForRewardedAds += () => {
                Debug.Log("Registered for rewarded ads.");
            };
            DDNASmartAds.Instance.OnDidFailToRegisterForRewardedAds += (string reason) => {
                Debug.Log("Failed to register for rewarded ads, "+reason);
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

                var interstitialAd = InterstitialAd.Create();
                if (interstitialAd != null) {
                    interstitialAd.Show();
                }
            }

            if (GUI.Button(new Rect(10, 120, 200, 80), "Interstitial with Engage")) {

                var engagement = new Engagement("showInterstitial");

                DDNA.Instance.RequestEngagement(engagement, response =>
                {
                    var interstitialAd = InterstitialAd.Create(response);

                    if (interstitialAd != null) {   // Engagement didn't prevent ad from showing

                        interstitialAd.OnInterstitialAdOpened += () =>  {
                            Debug.Log("Interstitial Ad opened its ad.");
                        };

                        interstitialAd.OnInterstitialAdFailedToOpen += (reason) => {
                            Debug.Log("Interstitial Ad failed to open its ad: "+reason);
                        };

                        interstitialAd.OnInterstitialAdClosed += () => {
                            Debug.Log("Interstitial Ad closed its ad.");
                        };

                        interstitialAd.Show();

                    }
                    else {
                        Debug.Log("Engage disabled the interstitial ad from showing.");
                    }
                }, exception => {
                    Debug.Log("Engage encountered an error: "+exception.Message);
                });
            }

            GUI.enabled = DDNASmartAds.Instance.IsRewardedAdAvailable();

            if (GUI.Button(new Rect(10, 320, 200, 80), "Rewarded Ad")) {

                var rewardedAd = RewardedAd.Create();
                if (rewardedAd != null) {
                    rewardedAd.Show();
                }
            }

            if (GUI.Button(new Rect(10, 420, 200, 80), "Rewarded with Engage")) {

                var engagement = new Engagement("showRewarded");

                DDNA.Instance.RequestEngagement(engagement, response => {

                    var rewardedAd = RewardedAd.Create(response);

                    if (rewardedAd != null) {

                        rewardedAd.OnRewardedAdOpened += () => {
                            Debug.Log("Rewarded Ad opened its ad.");
                        };
                        rewardedAd.OnRewardedAdFailedToOpen += (reason) => {
                            Debug.Log("Rewarded Ad failed to open its ad: "+reason);
                        };
                        rewardedAd.OnRewardedAdClosed += (reward) => {
                            Debug.Log("Rewarded Ad closed its ad with reward: " + (reward ? "YES" : "NO"));
                        };

                        rewardedAd.Show();

                    } else {
                        Debug.Log("Engage disabled the rewarded ad from showing.");
                    }

                }, exception => {
                    Debug.Log("Engage encountered an error: "+exception.Message);
                });
            }

            GUI.enabled = true;

            if (GUI.Button(new Rect(10, 620, 200, 80), "Rewarded or Image")) {

                var engagement = new Engagement("showRewardOrImage");
                engagement.AddParam("clickCount", ++clickCount);

                DDNA.Instance.RequestEngagement(engagement, response => {

                    // Since ads must be specifically disabled, try to build image message
                    // first.  If that fails, then see if the ad had been disabled.

                    var imageMessage = ImageMessage.Create(response);
                    var rewardedAd = RewardedAd.Create(response);

                    if (imageMessage != null) {

                        Debug.Log("Got an image message.");
                        imageMessage.OnDidReceiveResources += () => {
                            imageMessage.Show();
                        };
                        imageMessage.FetchResources();

                    } else if (rewardedAd != null) {

                        rewardedAd.Show();

                    } else {

                        Debug.Log("Engage didn't return an image and prevented the ad from showing.");
                    }

                }, exception => {
                    Debug.Log("Engage encountered an error: "+exception.Message);
                });

            }
        }
    }
}
