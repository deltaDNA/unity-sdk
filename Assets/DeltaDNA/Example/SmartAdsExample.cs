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

namespace DeltaDNA
{
    public class SmartAdsExample : MonoBehaviour {

        public const string ENVIRONMENT_KEY = "76410301326725846610230818914037";
        public const string COLLECT_URL = "https://collect2470ntysd.deltadna.net/collect/api";
        public const string ENGAGE_URL = "https://engage2470ntysd.deltadna.net";

        private int clickCount = 0;

        [SerializeField]
        private Transform cubeObj;

        // Use this for initialization
        void Start() {
            // Configure the SDK
            DDNA.Instance.SetLoggingLevel(Logger.Level.DEBUG);
            DDNA.Instance.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";
            DDNA.Instance.ClientVersion = "1.0.0";
            
            // Setup Ad notifications
            SmartAds.Instance.OnDidRegisterForInterstitialAds += () => {
                Debug.Log("Registered for interstitial ads.");
            };
            SmartAds.Instance.OnDidFailToRegisterForInterstitialAds += (string reason) => {
                Debug.Log("Failed to register for interstitial ads, " + reason);
            };
            SmartAds.Instance.OnDidRegisterForRewardedAds += () => {
                Debug.Log("Registered for rewarded ads.");
            };
            SmartAds.Instance.OnDidFailToRegisterForRewardedAds += (string reason) => {
                Debug.Log("Failed to register for rewarded ads, " + reason);
            };
            
            // Start collecting data and register for ads
            DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL);
        }

        void FixedUpdate() {
            // Make our cube rotate
            if (DDNA.Instance.HasStarted)
            {
                cubeObj.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
            }
        }

        public void OnInterstitialBtn_Clicked() {
            var interstitialAd = InterstitialAd.Create();
            if (interstitialAd != null) {
                interstitialAd.Show();
            }
        }

        public void OnInterstitialWithEngageBtn_Clicked() {
            var engagement = new Engagement("showInterstitial");

            DDNA.Instance.RequestEngagement(engagement, response => {
                var interstitialAd = InterstitialAd.Create(response);

                if (interstitialAd != null) { // Engagement didn't prevent ad from showing
                    interstitialAd.OnInterstitialAdOpened += () => {
                        Debug.Log("Interstitial Ad opened its ad.");
                    };

                    interstitialAd.OnInterstitialAdFailedToOpen += (reason) => {
                        Debug.Log("Interstitial Ad failed to open its ad: " + reason);
                    };

                    interstitialAd.OnInterstitialAdClosed += () => {
                        Debug.Log("Interstitial Ad closed its ad.");
                    };

                    interstitialAd.Show();
                } else {
                    Debug.Log("Engage disabled the interstitial ad from showing.");
                }
            }, exception => {
                Debug.Log("Engage encountered an error: " + exception.Message);
            });
        }

        public void OnRewardedBtn_Clicked() {
            var rewardedAd = RewardedAd.Create();
            if (rewardedAd != null) {
                rewardedAd.Show();
            }
        }

        public void OnRewardedWithEngageBtn_Clicked() {
            var engagement = new Engagement("showRewardOrImage");
            engagement.AddParam("clickCount", ++clickCount);

            DDNA.Instance.RequestEngagement(engagement, response => {
                // Since ads must be specifically disabled, try to build image message
                // first. If that fails, then see if the ad had been disabled.
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
                Debug.Log("Engage encountered an error: " + exception.Message);
            });
        }

        public void OnNewSessionBtn_Clicked() {
            DDNA.Instance.NewSession();
        }

        public void OnStartSdkBtn_Clicked() {
            DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL);
        }

        public void OnStopSdkBtn_Clicked() {
            DDNA.Instance.StopSDK();
        }
    }
}
