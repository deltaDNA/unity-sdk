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
using DeltaDNA.Ads;

namespace DeltaDNA
{
    using Application = UnityEngine.Application;
    using RuntimePlatform = UnityEngine.RuntimePlatform;
    using JSONObject = System.Collections.Generic.Dictionary<string, object>;
    using DeltaDNA.MiniJSON;

    /// <summary>
    /// The DDNASmartAds provides a service for fetching and showing ads.  It supports showing
    /// interstitial and rewarded ad types.
    /// </summary>
    public class SmartAds : Singleton<SmartAds> {

        private ISmartAdsManager manager;
        private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        private EngageCache engageCache;
        
        internal event Action<string> OnRewardedAdOpenedWithDecisionPoint;

        internal SmartAds() {}

        internal SmartAds Config(EngageCache engageCache) {
            this.engageCache = engageCache;
            return this;
        }
        
        #region Public interface

        /// <summary>
        /// Called when the registration for interstitial ads succeeds.
        /// </summary>
        public event Action OnDidRegisterForInterstitialAds;
        /// <summary>
        /// Called when the registration for interstitial ads fails.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<string> OnDidFailToRegisterForInterstitialAds;
        /// <summary>
        /// Called when the registration for rewarede ads succeeds.
        /// </summary>
        public event Action OnDidRegisterForRewardedAds;
        /// <summary>
        /// Called when the registration for rewarded ad fails.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<string> OnDidFailToRegisterForRewardedAds;

        /// <summary>
        /// Called when an interstitial ad is shown on screen.
        /// </summary>
        public event Action OnInterstitialAdOpened;
        /// <summary>
        /// Called when an interstitial ad fails to show.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<string> OnInterstitialAdFailedToOpen;
        /// <summary>
        /// Called when an interstitial ad is closed.
        /// </summary>
        public event Action OnInterstitialAdClosed;

        /// <summary>
        /// Called when a rewarded ad is loaded.
        /// </summary>
        public event Action OnRewardedAdLoaded;
        /// <summary>
        /// Called when a rewarded ad is shown on screen.
        /// </summary>
        public event Action OnRewardedAdOpened;
        /// <summary>
        /// Called when a rewarded ad fails to show.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<string> OnRewardedAdFailedToOpen;
        /// <summary>
        /// Called when a rewarded ad is closed.
        /// Whether the user should be rewarded will be passed in the parameter.
        /// </summary>
        public event Action<bool> OnRewardedAdClosed;

        #endregion

        #region Native Bridge
        
        internal bool IsInterstitialAdAllowed(Engagement engagement, bool checkTime) {
            return manager.IsInterstitialAdAllowed(engagement, checkTime);
        }
        
        internal bool IsRewardedAdAllowed(Engagement engagement, bool checkTime) {
            return manager.IsRewardedAdAllowed(engagement, checkTime);
        }
        
        internal long TimeUntilRewardedAdAllowed(Engagement engagement) {
            return manager.TimeUntilRewardedAdAllowed(engagement);
        }
        
        internal bool HasLoadedInterstitialAd() {
            return manager.HasLoadedInterstitialAd();
        }
        
        internal bool HasLoadedRewardedAd() {
            return manager.HasLoadedRewardedAd();
        }
        
        internal void ShowInterstitialAd(Engagement engagement) {
            manager.ShowInterstitialAd(engagement);
        }
        
        internal void ShowRewardedAd(Engagement engagement) {
            manager.ShowRewardedAd(engagement);
        }
        
        internal DateTime? GetLastShown(string decisionPoint) {
            return manager.GetLastShown(decisionPoint);
        }
        
        internal long GetSessionCount(string decisionPoint) {
            return manager.GetSessionCount(decisionPoint);
        }
        
        internal long GetDailyCount(string decisionPoint) {
            return manager.GetDailyCount(decisionPoint);
        }
        
        // Methods may be called from threads other than UnityMain, action queue ensures methods
        // execute on the UnityMain thread.  Actions created explicity to avoid variables not being
        // captured correctly.

        internal void DidRegisterForInterstitialAds()
        {
            Action action = delegate() {
                Logger.LogDebug("Registered for interstitial ads");

                if (OnDidRegisterForInterstitialAds != null) {
                    OnDidRegisterForInterstitialAds();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidFailToRegisterForInterstitialAds(string reason)
        {
            Action action = delegate() {
                Logger.LogDebug("Failed to register for interstitial ads: "+reason);

                if (OnDidFailToRegisterForInterstitialAds != null) {
                    OnDidFailToRegisterForInterstitialAds(reason);
                }
            };

            actions.Enqueue(action);
        }

        internal void DidOpenInterstitialAd()
        {
            Action action = delegate() {
                Logger.LogDebug("Opened an interstitial ad");

                if (OnInterstitialAdOpened != null) {
                    OnInterstitialAdOpened();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidFailToOpenInterstitialAd(string reason)
        {
            Action action = delegate() {
                Logger.LogDebug("Failed to open an interstitial ad: "+reason);

                if (OnInterstitialAdFailedToOpen != null) {
                    OnInterstitialAdFailedToOpen(reason);
                }
            };

            actions.Enqueue(action);
        }

        internal void DidCloseInterstitialAd()
        {
            Action action = delegate() {
                Logger.LogDebug("Closed an interstitial ad");

                if (OnInterstitialAdClosed != null) {
                    OnInterstitialAdClosed();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidRegisterForRewardedAds()
        {
            Action action = delegate() {
                Logger.LogDebug("Registered for rewarded ads");

                if (OnDidRegisterForRewardedAds != null) {
                    OnDidRegisterForRewardedAds();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidFailToRegisterForRewardedAds(string reason)
        {
            Action action = delegate() {
                Logger.LogDebug("Failed to register for rewarded ads: "+reason);

                if (OnDidFailToRegisterForRewardedAds != null) {
                    OnDidFailToRegisterForRewardedAds(reason);
                }
            };

            actions.Enqueue(action);
        }
        
        internal void DidLoadRewardedAd() {
            actions.Enqueue(() => {
                Logger.LogDebug("Loaded a rewarded ad");
                
                if (OnRewardedAdLoaded != null) OnRewardedAdLoaded();
            });
        }
        
        internal void DidOpenRewardedAd(string decisionPoint)
        {
            Action action = delegate() {
                Logger.LogDebug("Opened a rewarded ad: " + decisionPoint);
                
                if (OnRewardedAdOpenedWithDecisionPoint != null) {
                    OnRewardedAdOpenedWithDecisionPoint(decisionPoint);
                }
                if (OnRewardedAdOpened != null) {
                    OnRewardedAdOpened();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidFailToOpenRewardedAd(string reason)
        {
            Action action = delegate() {
                Logger.LogDebug("Failed to open a rewarded ad: " + reason);

                if (OnRewardedAdFailedToOpen != null) {
                    OnRewardedAdFailedToOpen(reason);
                }
            };

            actions.Enqueue(action);
        }

        internal void DidCloseRewardedAd(string rewardJSON)
        {
            Action action = delegate() {
                bool reward = false;
                try {
                    JSONObject obj = Json.Deserialize(rewardJSON) as JSONObject;
                    if (obj.ContainsKey("reward")) {
                        reward = (bool)obj["reward"];
                    }
                } catch (Exception) {}

                Logger.LogDebug("Closed a rewarded ad: " + reward);

                if (OnRewardedAdClosed != null) {
                    OnRewardedAdClosed(reward);
                }
            };

            actions.Enqueue(action);
        }

        internal void RecordEvent(string message)
        {
            Action action = delegate() {
                JSONObject obj = null;
                try {
                    obj = Json.Deserialize(message) as JSONObject;

                    var eventName = obj["eventName"] as String;
                    var eventParameters = obj["parameters"] as JSONObject;

                    if (obj != null) {
                        DDNA.Instance.RecordEvent(eventName, eventParameters).Run();
                    }

                } catch (Exception exception) {
                    Logger.LogError("Failed to record event: "+exception.Message);
                }
            };

            actions.Enqueue(action);
        }

        #endregion

        void Update()
        {
            // Action tasks from Android thread
            while (actions.Count > 0) {
                Action action = actions.Dequeue();
                action();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (manager != null) {
                if (pauseStatus) {
                    manager.OnPause();
                } else {
                    manager.OnResume();
                }
            }
        }
        
        public override void OnDestroy()
        {   
            if (manager != null) {
                manager.OnDestroy();
            }
            
            base.OnDestroy();
        }
        
        private void CreateManager() {
            Logger.LogDebug("Creating SmartAds manager");
            
            try {
                if (Application.platform == RuntimePlatform.IPhonePlayer) {
                    #if UNITY_IOS
                    manager = new Ads.iOS.SmartAdsManager();
                    #endif
                } else if (Application.platform == RuntimePlatform.Android) {
                    #if UNITY_ANDROID
                    manager = new Ads.Android.AdService(
                        this,
                        Settings.SDK_VERSION.Remove(0, Settings.SDK_VERSION.IndexOf(" v") + 2));
                    #endif
                } else {
                    #if UNITY_EDITOR
                    manager = new Ads.UnityPlayer.AdService();
                    #else
                    Logger.LogWarning("SmartAds is not currently supported on " + Application.platform);
                    #endif
                }
            } catch (Exception exception) {
                DidFailToRegisterForInterstitialAds(exception.Message);
                DidFailToRegisterForRewardedAds(exception.Message);
            }
        }
        
        internal void RegisterForAdsInternal(JSONObject config) {
            Logger.LogInfo("Registering for ads");

            if (manager == null) CreateManager();
            if (manager != null) manager.RegisterForAds(
                config,
                DDNA.Instance.Settings.AdvertiserGdprUserConsent,
                DDNA.Instance.Settings.AdvertiserGdprAgeRestrictedUser);
        }
    }
}
