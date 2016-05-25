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
using System.Collections.Generic;
using DeltaDNA;


namespace DeltaDNAAds
{
    using Application = UnityEngine.Application;
    using RuntimePlatform = UnityEngine.RuntimePlatform;
    using JSONObject = System.Collections.Generic.Dictionary<string, object>;
    using DeltaDNA.MiniJSON;

    /// <summary>
    /// The DDNASmartAds provides a service for fetching and showing ads.  It supports showing
    /// interstitial and rewarded ad types.
    /// </summary>
    public class DDNASmartAds : Singleton<DDNASmartAds> {

        public const string SMARTADS_DECISION_POINT = "advertising";

        private ISmartAdsManager manager;
        private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        #region Public interface

        public event Action OnDidRegisterForInterstitialAds;
        public event Action<string> OnDidFailToRegisterForInterstitialAds;
        public event Action OnDidRegisterForRewardedAds;
        public event Action<string> OnDidFailToRegisterForRewardedAds;

        public event Action OnInterstitialAdOpened;
        public event Action<string> OnInterstitialAdFailedToOpen;
        public event Action OnInterstitialAdClosed;

        public event Action OnRewardedAdOpened;
        public event Action<string> OnRewardedAdFailedToOpen;
        public event Action<bool> OnRewardedAdClosed;

        public void RegisterForAds()
        {
            Logger.LogInfo("Registering for ads");

            if (!DDNA.Instance.HasStarted) {
                Logger.LogError("The DeltaDNA SDK must be started before calling RegisterForAds.");
                return;
            }

            DDNA.Instance.OnNewSession -= this.RegisterForAds;
            DDNA.Instance.OnNewSession += this.RegisterForAds;

            try {
                if (Application.platform == RuntimePlatform.IPhonePlayer) {
                    #if UNITY_IOS
                    manager = new DeltaDNAAds.iOS.SmartAdsManager();
                    manager.RegisterForAds(SMARTADS_DECISION_POINT);
                    #endif
                }
                else if (Application.platform == RuntimePlatform.Android) {
                    #if UNITY_ANDROID
                    manager = new DeltaDNAAds.Android.AdService(this);
                    manager.RegisterForAds(SMARTADS_DECISION_POINT);
                    #endif
                }
                else {
                    Logger.LogWarning("SmartAds is not currently supported on "+Application.platform);
                }
            } catch (Exception exception) {
                this.DidFailToRegisterForInterstitialAds(exception.Message);
                this.DidFailToRegisterForRewardedAds(exception.Message);
            }
        }

        public bool IsInterstitialAdAllowed(Engagement engagement)
        {
            return manager != null && manager.IsInterstitialAdAllowed(engagement);
        }

        public bool IsInterstitialAdAvailable()
        {
            return manager != null && manager.IsInterstitialAdAvailable();
        }

        public void ShowInterstitialAd()
        {
            ShowInterstitialAdImpl(null);
        }

        [Obsolete("Prefer 'InterstitialAd' with an 'Engagement' instead.")]
        public void ShowInterstitialAd(string decisionPoint)
        {
            ShowInterstitialAdImpl(decisionPoint);
        }

        public bool IsRewardedAdAllowed(Engagement engagement)
        {
            return manager != null && manager.IsRewardedAdAllowed(engagement);
        }

        public bool IsRewardedAdAvailable()
        {
            return manager != null && manager.IsRewardedAdAvailable();
        }

        public void ShowRewardedAd()
        {
            ShowRewardedAdImpl(null);
        }

        [Obsolete("Prefer 'RewardedAd' with an 'Engagement' instead.")]
        public void ShowRewardedAd(string decisionPoint)
        {
            ShowRewardedAdImpl(decisionPoint);
        }

        #endregion

        #region Native Bridge

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

        internal void DidOpenRewardedAd()
        {
            Action action = delegate() {
                Logger.LogDebug("Opened a rewarded ad");

                if (OnRewardedAdOpened != null) {
                    OnRewardedAdOpened();
                }
            };

            actions.Enqueue(action);
        }

        internal void DidFailToOpenRewardedAd(string reason)
        {
            Action action = delegate() {
                Logger.LogDebug("Failed to open a rewarded ad: "+reason);

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

                Logger.LogDebug("Closed a rewarded ad: reward="+reward);

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
                        DDNA.Instance.RecordEvent(eventName, eventParameters);
                    }

                } catch (Exception exception) {
                    Logger.LogError("Failed to record event: "+exception.Message);
                }
            };

            actions.Enqueue(action);
        }

        internal void RequestEngagement(string request)
        {
            Action action = delegate() {
                JSONObject engagement = null;
                try {
                    engagement = Json.Deserialize(request) as JSONObject;
                } catch (Exception exception) {
                    Logger.LogError("Failed to deserialise engage request: "+exception.Message);
                }

                if (engagement != null) {
                    var decisionPoint = engagement["decisionPoint"] as string;
                    var flavour = engagement["flavour"] as string;
                    var parameters = engagement["parameters"] as JSONObject;
                    var id = engagement["id"] as string;

                    EngageResponse engageResponse = (response, statusCode, error) => {
                        manager.EngageResponse(id, response, statusCode, error);
                    };

                    EngageRequest engageRequest = new EngageRequest(decisionPoint);
                    engageRequest.Flavour = flavour;
                    engageRequest.Parameters = parameters;

                    StartCoroutine(Engage.Request(this, engageRequest, engageResponse));
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

        void ShowInterstitialAdImpl(string decisionPoint)
        {
            if (manager == null) {
                Logger.LogWarning("RegisterForAds must be called before calling trying to show ads");
                this.DidFailToOpenInterstitialAd("Not registered");
            } else if (String.IsNullOrEmpty(decisionPoint)) {
                Logger.LogInfo("Showing interstitial ad");
                manager.ShowInterstitialAd();
            } else {
                Logger.LogInfo("Showing interstitial ad for "+decisionPoint);
                manager.ShowInterstitialAd(decisionPoint);
            }
        }

        void ShowRewardedAdImpl(string decisionPoint)
        {
            if (manager == null) {
                Logger.LogWarning("RegisterForAds must be called before calling trying to show ads");
                this.DidFailToOpenRewardedAd("Not registered");
            } else if (String.IsNullOrEmpty(decisionPoint)) {
                Logger.LogInfo("Showing rewarded ad");
                manager.ShowRewardedAd();
            } else {
                Logger.LogInfo("Showing rewarded ad for "+decisionPoint);
                manager.ShowRewardedAd(decisionPoint);
            }
        }

        public override void OnDestroy()
        {
            if (manager != null) {
                Logger.LogDebug("Destroying StartAds");
                DDNA.Instance.OnNewSession -= this.RegisterForAds;
                manager.OnDestroy();
            }
            base.OnDestroy();
        }
    }
}
