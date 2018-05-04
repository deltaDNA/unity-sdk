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

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DeltaDNA
{
    public class SmartAdsExample : MonoBehaviour {

        // Ad objects
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd1;
        private RewardedAd rewardedAd2;

        // UI elements
        [SerializeField]
        private Transform cubeObj;
        private Button interstitialBtn;
        private Button rewarded1Btn;
        private Button rewarded2Btn;
        private Text interstitialMsg;
        private Text interstitialStats;
        private Text rewarded1Msg;
        private Text rewarded1Stats;
        private Text rewarded2Msg;
        private Text rewarded2Stats;

        // Use this for initialization
        void Start() {
            var buttons = FindObjectsOfType<Button>().ToList();
            interstitialBtn = buttons.Find(e => e.name == "Interstitial Btn");
            rewarded1Btn = buttons.Find(e => e.name == "Rewarded1 Btn");
            rewarded2Btn = buttons.Find(e => e.name == "Rewarded2 Btn");

            var texts = FindObjectsOfType<Text>().ToList();
            interstitialMsg = texts.Find(e => e.name == "Interstitial Msg");
            interstitialStats = texts.Find(e => e.name == "Interstitial Stats");
            rewarded1Msg = texts.Find(e => e.name == "Rewarded1 Msg");
            rewarded1Stats = texts.Find(e => e.name == "Rewarded1 Stats");
            rewarded2Msg = texts.Find(e => e.name == "Rewarded2 Msg");
            rewarded2Stats = texts.Find(e => e.name == "Rewarded2 Stats");

            // Configure the SDK
            DDNA.Instance.SetLoggingLevel(Logger.Level.DEBUG);

            // Setup Ad notifications
            SmartAds.Instance.OnDidRegisterForInterstitialAds += () => {
                Debug.Log("Registered for interstitial ads.");

                DDNA.Instance.EngageFactory.RequestInterstitialAd(
                    "interstitialAd",
                    (action => {
                        interstitialAd = action;
                        interstitialAd.OnInterstitialAdOpened += OnInterstitialAdOpened;
                        interstitialAd.OnInterstitialAdFailedToOpen += OnInterstitialAdFailedToOpen;
                        interstitialAd.OnInterstitialAdClosed += OnInterstitialAdClosed;

                        interstitialBtn.interactable = true;
                        interstitialMsg.text = "Ready";
                }));
            };
            SmartAds.Instance.OnDidFailToRegisterForInterstitialAds += (string reason) => {
                Debug.Log("Failed to register for interstitial ads, " + reason);

                interstitialMsg.text = "Failed to register for interstitial ads";
                interstitialBtn.interactable = false;
                interstitialAd = null;
            };
            SmartAds.Instance.OnDidRegisterForRewardedAds += () => {
                Debug.Log("Registered for rewarded ads.");

                DDNA.Instance.EngageFactory.RequestRewardedAd(
                    "rewardedAd1",
                    (action => {
                        rewardedAd1 = action;
                        rewardedAd1.OnRewardedAdLoaded += OnRewardedAdLoaded;
                        rewardedAd1.OnRewardedAdExpired += OnRewardedAdExpired;
                        rewardedAd1.OnRewardedAdOpened += OnRewardedAdOpened;
                        rewardedAd1.OnRewardedAdFailedToOpen += OnRewardedAdFailedToOpen;
                        rewardedAd1.OnRewardedAdClosed += OnRewardedAdClosed;

                        rewarded1Btn.interactable = true;
                        rewarded1Msg.text = "Ready";
                    }));
                DDNA.Instance.EngageFactory.RequestRewardedAd(
                    "rewardedAd2",
                    (action => {
                        rewardedAd2 = action;
                        rewardedAd2.OnRewardedAdLoaded += OnRewardedAdLoaded;
                        rewardedAd2.OnRewardedAdExpired += OnRewardedAdExpired;
                        rewardedAd2.OnRewardedAdOpened += OnRewardedAdOpened;
                        rewardedAd2.OnRewardedAdFailedToOpen += OnRewardedAdFailedToOpen;
                        rewardedAd2.OnRewardedAdClosed += OnRewardedAdClosed;

                        rewarded2Btn.interactable = true;
                        rewarded2Msg.text = "Ready";
                    }));
            };
            SmartAds.Instance.OnDidFailToRegisterForRewardedAds += (string reason) => {
                Debug.Log("Failed to register for rewarded ads, " + reason);

                rewarded1Btn.interactable = false;
                rewarded2Btn.interactable = false;
                rewarded1Msg.text = "Failed to register for rewarded ads";
                rewarded2Msg.text = "Failed to register for rewarded ads";

                rewardedAd1 = null;
                rewardedAd2 = null;
            };

            // Start the SDK. We recommend using the configuration UI for setting your game's
            // keys and calling StartSDK() or StartSDK(userID) instead.
            DDNA.Instance.StartSDK(new Configuration() {
                environmentKeyDev = "76410301326725846610230818914037",
                environmentKey = 0,
                collectUrl = "https://collect2470ntysd.deltadna.net/collect/api",
                engageUrl = "https://engage2470ntysd.deltadna.net",
                useApplicationVersion = true
            });
        }

        void FixedUpdate() {
            // Make our cube rotate
            if (DDNA.Instance.HasStarted)
            {
                cubeObj.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
            }

            UpdateStats(interstitialAd, interstitialStats);
            UpdateStats(rewardedAd1, rewarded1Stats);
            UpdateStats(rewardedAd2, rewarded2Stats);
        }

        public void OnInterstitialBtn_Clicked() {
            /*
             * Don't worry about checking if an ad is ready. Trying to show an ad
             * when you want will give a report of your fill rate.
             */
            if (interstitialAd != null) interstitialAd.Show();
        }

        public void OnRewarded1Btn_Clicked() {
            if (rewardedAd1 != null && rewardedAd1.IsReady()) {
                rewardedAd1.Show();
            }
        }

        public void OnRewarded2Btn_Clicked() {
            if (rewardedAd2 != null && rewardedAd2.IsReady()) {
                rewardedAd2.Show();
            }
        }

        public void OnNewSessionBtn_Clicked() {
            interstitialBtn.interactable = false;
            interstitialMsg.text = "";
            interstitialStats.text = "";
            rewarded1Btn.interactable = false;
            rewarded1Msg.text = "";
            rewarded1Stats.text = "";
            rewarded2Btn.interactable = false;
            rewarded2Msg.text = "";
            rewarded2Stats.text = "";

            DDNA.Instance.NewSession();
        }

        public void OnGdprToggle_ValueChanged(bool value) {
            DDNA.Instance.Settings.AdvertiserGdprUserConsent = value;
            OnNewSessionBtn_Clicked();
        }

        private void UpdateStats(Ad action, Text view) {
            if (action == null) return;

            var lastShownText = (!action.LastShown.HasValue)
                ? "N/A"
                : action.LastShown.Value.ToString("HH:mm");
            var adShowElapsedSecs = (!action.LastShown.HasValue)
                ? 0
                : (DateTime.UtcNow - action.LastShown.Value).TotalSeconds;
            var secsText = (adShowElapsedSecs > 0 && adShowElapsedSecs < action.AdShowWaitSecs)
                ? " (" + Math.Max(0, Math.Ceiling(action.AdShowWaitSecs - adShowElapsedSecs)) + " secs)"
                : "";
            view.text = string.Format(
                "Session: {0} ({1}) | Today: {2} ({3}) | Time: {4}{5}",
                action.SessionCount,
                action.SessionLimit,
                action.DailyCount,
                action.DailyLimit,
                lastShownText,
                secsText);
        }

        private void OnInterstitialAdOpened(InterstitialAd ad) {
            interstitialMsg.text = "Fulfilled";
        }

        private void OnInterstitialAdFailedToOpen(InterstitialAd ad, string reason) {
            interstitialMsg.text = "Failed to open, " + reason;
        }

        private void OnInterstitialAdClosed(InterstitialAd ad) {}

        private void OnRewardedAdLoaded(RewardedAd ad) {
            if (ad == rewardedAd1) {
                rewarded1Btn.interactable = true;
                rewarded1Msg.text = "Ready";
            } else if (ad == rewardedAd2) {
                rewarded2Btn.interactable = true;
                rewarded2Msg.text = "Ready";
            }
        }

        private void OnRewardedAdExpired(RewardedAd ad) {
            if (ad == rewardedAd1) {
                rewarded1Btn.interactable = false;
                rewarded1Msg.text = "Expired";
            } else if (ad == rewardedAd2) {
                rewarded2Btn.interactable = false;
                rewarded2Msg.text = "Expired";
            }
        }

        private void OnRewardedAdOpened(RewardedAd ad) {
            if (ad == rewardedAd1) {
                rewarded1Btn.interactable = false;
                rewarded1Msg.text = "Fulfilled";
            } else if (ad == rewardedAd2) {
                rewarded2Btn.interactable = false;
                rewarded2Msg.text = "Fulfilled";
            }
        }

        private void OnRewardedAdFailedToOpen(RewardedAd ad, string reason) {
            if (ad == rewardedAd1) {
                rewarded1Btn.interactable = false;
                rewarded1Msg.text = "Failed to open; " + reason;
            } else if (ad == rewardedAd2) {
                rewarded2Btn.interactable = false;
                rewarded2Msg.text = "Failed to open; " + reason;
            }
        }

        private void OnRewardedAdClosed(RewardedAd ad, bool completed) {
            var msg = completed
                ? string.Format(
                    "Watched, reward player {0} {1}",
                    ad.RewardAmount,
                    ad.RewardType)
                : "Skipped, don't reward player";

            if (ad == rewardedAd1) {
                rewarded1Btn.interactable = false;
                rewarded1Msg.text = msg;
            } else if (ad == rewardedAd2) {
                rewarded2Btn.interactable = false;
                rewarded2Msg.text = msg;
            }
        }
    }
}
