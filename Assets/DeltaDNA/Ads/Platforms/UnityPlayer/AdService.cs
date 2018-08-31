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

namespace DeltaDNA.Ads.UnityPlayer {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    #if UNITY_EDITOR
    internal class AdService : ISmartAdsManager {

        private enum ShowResult {
            FULFILLED,
            NO_AD_AVAILABLE,
            AD_SHOW_POINT,
            SESSION_LIMIT_REACHED,
            SESSION_DECISION_POINT_LIMIT_REACHED,
            DAILY_DECISION_POINT_LIMIT_REACHED,
            MIN_TIME_NOT_ELAPSED,
            MIN_TIME_DECISION_POINT_NOT_ELAPSED
        };

        private const int CONFIG_DELAY = 60;
        private const bool DEFAULT_AD_SHOW_POINT = true;
        private const int DEFAULT_AD_MINIMUM_INTERVAL = 0;
        private const int DEFAULT_AD_MAX_PER_SESSION = -1;

        private readonly AdMetrics metrics = new AdMetrics();

        private int adMinimumInterval = DEFAULT_AD_MINIMUM_INTERVAL;
        private int adMaxPerSession = DEFAULT_AD_MAX_PER_SESSION;

        private AdAgent interstitialAgent;
        private AdAgent rewardedAgent;

        public void RegisterForAds(JSONObject response, bool userConsent, bool ageRestricted) {

            var config = response["parameters"] as IDictionary<string, object>;

            if (!config.ContainsKey("adShowSession")
                || !(config["adShowSession"] as bool? ?? false)) {
                SmartAds.Instance.DidFailToRegisterForInterstitialAds(
                    "Ads disabled for this session");
                SmartAds.Instance.DidFailToRegisterForRewardedAds(
                    "Ads disabled for this session");
                return;
            }

            if (!config.ContainsKey("adProviders")
                && !config.ContainsKey("adRewardedProviders")) {
                SmartAds.Instance.DidFailToRegisterForInterstitialAds(
                    "Invalid Engage response, missing 'adProviders' key");
                SmartAds.Instance.DidFailToRegisterForRewardedAds(
                    "Invalid Engage response, missing 'adRewardedProviders' key");
                return;
            }

            adMinimumInterval = config.GetOrDefault(
                "adMinimumInterval",
                DEFAULT_AD_MINIMUM_INTERVAL);
            adMaxPerSession = config.GetOrDefault(
                "adMaxPerSession",
                DEFAULT_AD_MAX_PER_SESSION);

            if (!config.ContainsKey("adProviders")) {
                SmartAds.Instance.DidFailToRegisterForInterstitialAds(
                    "No interstitial ad providers defined");
            } else if ((config["adProviders"] as IList<object>).Count == 0) {
                SmartAds.Instance.DidFailToRegisterForInterstitialAds(
                    "No interstitial ad providers defined");
            } else {
                interstitialAgent = new AdAgent(
                    false,
                    (config["adProviders"] as IList<object>).Count,
                    adMaxPerSession);
                interstitialAgent.RecordAdShown += ((dp, time) => metrics.RecordAdShown(dp, time));
                interstitialAgent.RequestAd();

                SmartAds.Instance.DidRegisterForInterstitialAds();
            }

            if (!config.ContainsKey("adRewardedProviders")) {
                SmartAds.Instance.DidFailToRegisterForRewardedAds(
                    "No rewarded ad providers defined");
            }  else if ((config["adRewardedProviders"] as IList<object>).Count == 0) {
                SmartAds.Instance.DidFailToRegisterForRewardedAds(
                    "No rewarded ad providers defined");
            } else {
                rewardedAgent = new AdAgent(
                    true,
                    (config["adRewardedProviders"] as IList<object>).Count,
                    adMaxPerSession);
                rewardedAgent.RecordAdShown += ((dp, time) => metrics.RecordAdShown(dp, time));
                rewardedAgent.RequestAd();

                SmartAds.Instance.DidRegisterForRewardedAds();
            }
        }

        public void OnNewSession() {
            metrics.NewSession(DateTime.Now);
        }

        public bool IsInterstitialAdAllowed(Engagement engagement, bool checkTime) {
            return IsAdAllowed(
                interstitialAgent,
                (engagement != null) ? engagement.DecisionPoint : null,
                (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters"))
                    ? engagement.JSON["parameters"] as JSONObject
                    : null,
                checkTime);
        }

        public bool IsRewardedAdAllowed(Engagement engagement, bool checkTime) {
            return IsAdAllowed(
                rewardedAgent,
                (engagement != null) ? engagement.DecisionPoint : null,
                (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters"))
                    ? engagement.JSON["parameters"] as JSONObject
                    : null,
                checkTime);
        }

        public long TimeUntilRewardedAdAllowed(Engagement engagement) {
            if (rewardedAgent == null) {
                return 0;
            } else if (engagement == null
                || engagement.JSON == null
                || !engagement.JSON.ContainsKey("parameters")) {
                return 0;
            } else {
                var now = DateTime.UtcNow;
                var wait = (engagement.JSON["parameters"] as JSONObject)
                    ["ddnaAdShowWaitSecs"] as long? ?? 0;

                if (adMinimumInterval >= wait) {
                    var lastShown = now.Subtract(rewardedAgent.lastShowTime).Seconds;
                    if (lastShown < adMinimumInterval) {
                        return adMinimumInterval - lastShown;
                    }
                } else {
                    var lastShownDate = metrics.LastShown(engagement.DecisionPoint);
                    if (lastShownDate.HasValue) {
                        var lastShown = now.Subtract(lastShownDate.Value).Seconds;
                        if (lastShown < wait) {
                            return wait - lastShown;
                        }
                    }
                }

                return 0;
            }
        }

        public bool HasLoadedInterstitialAd() {
            return interstitialAgent != null && interstitialAgent.IsAdLoaded();
        }

        public bool HasLoadedRewardedAd() {
            return rewardedAgent != null && rewardedAgent.IsAdLoaded();
        }

        public void ShowInterstitialAd(Engagement engagement) {
            if (interstitialAgent != null) {
                ShowAd(
                    interstitialAgent,
                    (engagement != null) ? engagement.DecisionPoint : null,
                    (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters"))
                        ? engagement.JSON["parameters"] as JSONObject
                        : null);
            } else {
                SmartAds.Instance.DidFailToOpenInterstitialAd("Not registered");
            }
        }

        public void ShowRewardedAd(Engagement engagement) {
            if (rewardedAgent != null) {
                ShowAd(
                    rewardedAgent,
                    (engagement != null) ? engagement.DecisionPoint : null,
                    (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters"))
                        ? engagement.JSON["parameters"] as JSONObject
                        : null);
            } else {
                SmartAds.Instance.DidFailToOpenRewardedAd("Not registered");
            }
        }

        public DateTime? GetLastShown(string decisionPoint) {
            return metrics.LastShown(decisionPoint);
        }

        public long GetSessionCount(string decisionPoint) {
            return metrics.SessionCount(decisionPoint);
        }

        public long GetDailyCount(string decisionPoint) {
            return metrics.DailyCount(decisionPoint);
        }

        public void EngageResponse(string id, string response, int statusCode, string error) {
            throw new NotImplementedException();
        }

        public void OnPause() {}

        public void OnResume() {}

        public void OnDestroy() {}

        private bool IsAdAllowed(
            AdAgent agent,
            string decisionPoint,
            JSONObject parameters,
            bool checkTime) {

            if (agent == null)  {
                Logger.LogDebug("Ads disabled for this session");
                return false;
            }

            if (!string.IsNullOrEmpty(decisionPoint) && parameters == null) {
                Logger.LogDebug("Ad cannot be shown with an invalid Engagement");
                return false;
            } else if (string.IsNullOrEmpty(decisionPoint) && parameters == null) {
                Logger.LogWarning("Using an empty Engagement is deprecated");
                return true;
            }

            var allowed = false;
            switch (Result(agent, decisionPoint, parameters)) {
                case ShowResult.MIN_TIME_NOT_ELAPSED:
                case ShowResult.MIN_TIME_DECISION_POINT_NOT_ELAPSED:
                case ShowResult.NO_AD_AVAILABLE:
                    allowed = !checkTime;
                    break;

                case ShowResult.FULFILLED:
                    allowed = true;
                    break;
            };

            return allowed;
        }

        private void ShowAd(
            AdAgent agent,
            string decisionPoint,
            JSONObject parameters) {

            if (!string.IsNullOrEmpty(decisionPoint) && parameters == null) {
                DidFailToShowAd(agent, "Invalid engagement");
                return;
            } else if (string.IsNullOrEmpty(decisionPoint) && parameters == null) {
                Logger.LogWarning("Prefer showing ads with Engagements");
            }

            agent.SetAdPoint(decisionPoint);

            switch (Result(agent, decisionPoint, parameters)) {
                case ShowResult.AD_SHOW_POINT:
                    DidFailToShowAd(agent, "Engage disallowed the ad");
                    return;

                case ShowResult.SESSION_LIMIT_REACHED:
                    DidFailToShowAd(agent, "Session limit for environment reached");
                    return;

                case ShowResult.SESSION_DECISION_POINT_LIMIT_REACHED:
                    DidFailToShowAd(agent, "Session limit for decision point reached");
                    return;

                case ShowResult.DAILY_DECISION_POINT_LIMIT_REACHED:
                    DidFailToShowAd(agent, "Daily limit for decision point reached");
                    return;

                case ShowResult.MIN_TIME_NOT_ELAPSED:
                    DidFailToShowAd(agent, "Minimum environment time between ads not elapsed");
                    return;

                case ShowResult.MIN_TIME_DECISION_POINT_NOT_ELAPSED:
                    DidFailToShowAd(agent, "Minimum decision point time between ads not elapsed");
                    return;
            }

            if (!agent.IsAdLoaded()) {
                DidFailToShowAd(agent, "Ad not loaded");
                return;
            }

            agent.ShowAd(decisionPoint);
        }

        private ShowResult Result(AdAgent agent, string decisionPoint, JSONObject parameters) {
            if (parameters != null
                && !(parameters["adShowPoint"] as bool? ?? DEFAULT_AD_SHOW_POINT)) {
                return ShowResult.AD_SHOW_POINT;
            }

            if (adMaxPerSession != -1 && agent.shownCount >= adMaxPerSession) {
                return ShowResult.SESSION_LIMIT_REACHED;
            }

            if (!string.IsNullOrEmpty(decisionPoint)
                && parameters != null
                && parameters.ContainsKey("ddnaAdSessionCount")) {
                var value = parameters["ddnaAdSessionCount"] as long? ?? 0;
                if (metrics.SessionCount(decisionPoint) >= value) {
                    return ShowResult.SESSION_DECISION_POINT_LIMIT_REACHED;
                }
            }

            if (!string.IsNullOrEmpty(decisionPoint)
                && parameters != null
                && parameters.ContainsKey("ddnaAdDailyCount")) {
                var value = parameters["ddnaAdDailyCount"] as long? ?? 0;
                if (metrics.DailyCount(decisionPoint) >= value) {
                    return ShowResult.DAILY_DECISION_POINT_LIMIT_REACHED;
                }
            }

            var now = DateTime.UtcNow;
            if (now.Subtract(agent.lastShowTime).Seconds < adMinimumInterval) {
                return ShowResult.MIN_TIME_NOT_ELAPSED;
            }

            if (!string.IsNullOrEmpty(decisionPoint)
                && parameters != null
                && parameters.ContainsKey("ddnaAdShowWaitSecs")) {
                var wait = parameters["ddnaAdShowWaitSecs"] as long? ?? 0;
                var lastShown = metrics.LastShown(decisionPoint);
                if (lastShown.HasValue
                    && now.Subtract(lastShown.Value).Seconds < wait) {
                    return ShowResult.MIN_TIME_DECISION_POINT_NOT_ELAPSED;
                }
            }

            if (!agent.IsAdLoaded()) {
                return ShowResult.NO_AD_AVAILABLE;
            }

            return ShowResult.FULFILLED;
        }

        private void DidFailToShowAd(AdAgent agent, String reason) {
            if (agent == interstitialAgent) {
                SmartAds.Instance.DidFailToOpenInterstitialAd(reason);
            } else if (agent == rewardedAgent) {
                SmartAds.Instance.DidFailToOpenRewardedAd(reason);
            }
        }
    }
    #endif
}
