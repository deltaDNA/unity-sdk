//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Ads.UnityPlayer {

    #if UNITY_EDITOR
    internal class AdService : ISmartAdsManager {

        private static readonly int CONFIG_DELAY = 60;

        private string decisionPoint;

        private IDictionary<string, object> config;
        private int maxPerNetwork;
        private int adMinimumInterval;
        private int adMaxPerSession;

        private AdAgent interstitialAgent;
        private AdAgent rewardedAgent;

        public void EngageResponse(string id, string response, int statusCode, string error) {
            throw new NotImplementedException();
        }

        public bool IsInterstitialAdAllowed(Engagement engagement) {
            return IsAdAllowed(interstitialAgent, engagement);
        }

        public bool IsInterstitialAdAvailable() {
            return interstitialAgent != null && interstitialAgent.IsAdLoaded();
        }

        public bool IsRewardedAdAllowed(Engagement engagement) {
            return IsAdAllowed(rewardedAgent, engagement);
        }

        public bool IsRewardedAdAvailable() {
            return rewardedAgent != null && rewardedAgent.IsAdLoaded();
        }

        public void OnDestroy() {}

        public void OnPause() {}

        public void OnResume() {}
        
        public void OnNewSession() {
            RegisterForAds(this.decisionPoint);
        }

        public void RegisterForAds(string decisionPoint) {
            this.decisionPoint = decisionPoint;

            var engagement = new Engagement(decisionPoint);
            engagement.Flavour = "internal";
            engagement.AddParam("adSdkVersion", Settings.SDK_VERSION);

            DDNA.Instance.RequestEngagement(engagement, (response) => {
                if (!response.ContainsKey("parameters")) {
                    DDNASmartAds.Instance.DidFailToRegisterForInterstitialAds(
                        "Invalid Engage response, missing 'parameters' key");
                    DDNASmartAds.Instance.DidFailToRegisterForRewardedAds(
                            "Invalid Engage response, missing 'parameters' key");
                    return;
                }

                config = response["parameters"] as IDictionary<string, object>;

                if (!config.ContainsKey("adShowSession")
                    || !(config["adShowSession"] as bool? ?? false)) {
                    DDNASmartAds.Instance.DidFailToRegisterForInterstitialAds(
                        "Ads disabled for this session");
                    DDNASmartAds.Instance.DidFailToRegisterForRewardedAds(
                        "Ads disabled for this session");
                    return;
                }

                if (!config.ContainsKey("adProviders")
                    && !config.ContainsKey("adRewardedProviders")) {
                    DDNASmartAds.Instance.DidFailToRegisterForInterstitialAds(
                        "Invalid Engage response, missing 'adProviders' key");
                    DDNASmartAds.Instance.DidFailToRegisterForRewardedAds(
                            "Invalid Engage response, missing 'adRewardedProviders' key");
                    return;
                }

                maxPerNetwork = (config.ContainsKey("adMaxPerNetwork"))
                    ? config["adMaxPerNetwork"] as int? ?? 0
                    : 0;
                adMinimumInterval = (config.ContainsKey("adMinimumInterval"))
                    ? config["adMinimumInterval"] as int? ?? -1
                    : -1;
                adMaxPerSession = (config.ContainsKey("adMaxPerSession"))
                    ? config["adMaxPerSession"] as int? ?? -1
                    : -1;

                if (!config.ContainsKey("adProviders")) {
                    DDNASmartAds.Instance.DidFailToRegisterForInterstitialAds(
                        "No interstitial ad providers defined");
                } else if ((config["adProviders"] as IList<object>).Count == 0) {
                    DDNASmartAds.Instance.DidFailToRegisterForInterstitialAds(
                        "No interstitial ad providers defined");
                } else {
                    interstitialAgent = new AdAgent(
                        false,
                        (config["adProviders"] as IList<object>).Count,
                        adMaxPerSession,
                        maxPerNetwork);
                    interstitialAgent.RequestAd();
                }

                if (!config.ContainsKey("adRewardedProviders")) {
                    DDNASmartAds.Instance.DidFailToRegisterForRewardedAds(
                        "No rewarded ad providers defined");
                }  else if ((config["adRewardedProviders"] as IList<object>).Count == 0) {
                    DDNASmartAds.Instance.DidFailToRegisterForRewardedAds(
                        "No rewarded ad providers defined");
                } else {
                    rewardedAgent = new AdAgent(
                        true,
                        (config["adRewardedProviders"] as IList<object>).Count,
                        adMaxPerSession,
                        maxPerNetwork);
                    rewardedAgent.RequestAd();
                }
            });
        }

        public void ShowInterstitialAd() {
            if (interstitialAgent != null) {
                ShowAd(interstitialAgent, null);
            } else {
                DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                    "Interstitial agent is not initialised");
            }
        }

        public void ShowInterstitialAd(string adPoint) {
            if (interstitialAgent != null) {
                ShowAd(interstitialAgent, adPoint);
            } else {
                DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                    "Interstitial agent is not initialised");
            }
        }

        public void ShowRewardedAd() {
            if (rewardedAgent != null) {
                ShowAd(rewardedAgent, null);
            } else {
                DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                    "Rewarded agent is not initialised");
            }
        }

        public void ShowRewardedAd(string adPoint) {
            if (rewardedAgent != null) {
                ShowAd(rewardedAgent, adPoint);
            } else {
                DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                    "Rewarded agent is not initialised");
            }
        }

        private bool IsAdAllowed(AdAgent agent, Engagement engagement) {
            if (agent == null) return false;

            var adPoint = (engagement != null) ? engagement.DecisionPoint : null;
            agent.SetAdPoint(adPoint);

            if (!(config["adShowSession"] as bool? ?? true)) {
                return false;
            }

            if (engagement != null
                && engagement.JSON != null
                && engagement.JSON.ContainsKey("adShowPoint")
                && !(engagement.JSON["adShowPoint"] as bool? ?? true)) {
                Debug.Log("Engage prevented ad from opening at " + adPoint);
                return false;
            }

            if (adMinimumInterval != -1
                && DateTime.UtcNow.Ticks - agent.lastShowTime
                <= adMinimumInterval * 1000) {
                Debug.Log("Not showing ad before minimum interval");
                return false;
            }

            if (adMaxPerSession != -1
                && agent.shownCount >= adMaxPerSession) {
                Debug.Log("Number of ads shown this session exceeded the maximum");
                return false;
            }

            if (!agent.IsAdLoaded()) {
                Debug.Log("No ad available");
                return false;
            }

            return true;
        }

        private void ShowAd(AdAgent agent, string adPoint) {
            if (config.ContainsKey("adShowPoint")
                && (config["adShowPoint"] as bool? ?? false)) {
                Debug.Log("Ad points not supported by configuration");

                if (!agent.rewarded) {
                    DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                        "Ad points not supported by configuration");
                } else {
                    DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                        "Ad points not supported by configuration");
                }

                return;
            }

            if (adMinimumInterval != -1
                && DateTime.UtcNow.Ticks - agent.lastShowTime
                <= adMinimumInterval * 1000) {
                Debug.Log("Not showing ad before minimum interval");

                if (!agent.rewarded) {
                    DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                        "Too soon");
                } else {
                    DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                        "Too soon");
                }

                return;
            }

            if (adMaxPerSession != -1
                && agent.shownCount >= adMaxPerSession) {
                Debug.Log("Number of ads shown this session exceeded the maximum");

                if (!agent.rewarded) {
                    DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                        "Session limit reached");
                } else {
                    DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                        "Session limit reached");
                }
                
                return;
            }

            if (!agent.IsAdLoaded()) {
                Debug.Log("No ad loaded by agent");

                if (!agent.rewarded) {
                    DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                        "Not ready");
                } else {
                    DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                        "Not ready");
                }

                return;
            }

            if (!string.IsNullOrEmpty(adPoint)) {
                var engagement = new Engagement(adPoint) { Flavour = "advertising" };
                DDNA.Instance.RequestEngagement(
                    engagement,
                    (response) => {
                        if (IsAdAllowed(agent, engagement)) {
                            ShowAd(agent, null);
                        } else if (!agent.rewarded) {
                            DDNASmartAds.Instance.DidFailToOpenInterstitialAd(
                                "Not allowed");
                        } else if (agent.rewarded) {
                            DDNASmartAds.Instance.DidFailToOpenRewardedAd(
                                "Not allowed");
                        }
                });
            } else {
                agent.ShowAd(adPoint);
            }
        }
    }
    #endif
}
