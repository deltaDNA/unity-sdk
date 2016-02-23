using System;
using System.Collections.Generic;
using DeltaDNA;


namespace DeltaDNAAds
{
    using Application = UnityEngine.Application;
    using RuntimePlatform = UnityEngine.RuntimePlatform;
    using JSONObject = System.Collections.Generic.Dictionary<string, object>;
    using DeltaDNA.MiniJSON;

    public class DDNASmartAds : Singleton<DDNASmartAds> {

        public const string SMARTADS_DECISION_POINT = "advertising";

        private ISmartAdsManager manager;
        private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        #region Public interface

        public event Action OnDidRegisterForInterstitialAds;
        public event Action<string> OnDidFailToRegisterForInterstitialAds;
        public event Action OnInterstitialAdOpened;
        public event Action OnInterstitialAdFailedToOpen;
        public event Action OnInterstitialAdClosed;

        public event Action OnDidRegisterForRewardedAds;
        public event Action<string> OnDidFailToRegisterForRewardedAds;
        public event Action OnRewardedAdOpened;
        public event Action OnRewardedAdFailedToOpen;
        public event Action<bool> OnRewardedAdClosed;

        public void RegisterForAds()
        {
            Logger.LogInfo("Registering for ads");

            if (!DDNA.Instance.HasStarted) {
                Logger.LogError("The DeltaDNA SDK must be started before calling RegisterForAds.");
                return;
            }

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

        public bool IsInterstitialAdAvailable()
        {
            return manager != null && manager.IsInterstitialAdAvailable();
        }

        public void ShowInterstitialAd()
        {
            ShowInterstitialAd(null);
        }

        public void ShowInterstitialAd(string adPoint)
        {
            if (manager == null) {
                Logger.LogWarning("RegisterForAds must be called before calling trying to show ads");
                this.DidFailToOpenInterstitialAd();
            } else if (String.IsNullOrEmpty(adPoint)) {
                Logger.LogInfo("Showing interstitial ad");
                manager.ShowInterstitialAd();
            } else {
                Logger.LogInfo("Showing interstitial ad for "+adPoint);
                manager.ShowInterstitialAd(adPoint);
            }
        }

        public bool IsRewardedAdAvailable()
        {
            return manager != null && manager.IsRewardedAdAvailable();
        }

        public void ShowRewardedAd()
        {
            ShowRewardedAd(null);
        }

        public void ShowRewardedAd(string adPoint)
        {
            if (manager == null) {
                Logger.LogWarning("RegisterForAds must be called before calling trying to show ads");
                this.DidFailToOpenRewardedAd();
            } else if (String.IsNullOrEmpty(adPoint)) {
                Logger.LogInfo("Showing rewarded ad");
                manager.ShowRewardedAd();
            } else {
                Logger.LogInfo("Showing rewarded ad for "+adPoint);
                manager.ShowRewardedAd(adPoint);
            }
        }

        #endregion

        #region Native Bridge

        // Methods may be called from threads other than UnityMain, action queue ensures methods
        // execute on the UnityMain thread.

        internal void DidRegisterForInterstitialAds()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Registered for interstitial ads");
                if (OnDidRegisterForInterstitialAds != null) OnDidRegisterForInterstitialAds();
            });
        }

        internal void DidFailToRegisterForInterstitialAds(string reason)
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Failed to register for interstitial ads: "+reason);
                if (OnDidFailToRegisterForInterstitialAds != null) OnDidFailToRegisterForInterstitialAds(reason);
            });
        }

        internal void DidOpenInterstitialAd()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Opened an interstitial ad");
                if (OnInterstitialAdOpened != null) OnInterstitialAdOpened();
            });
        }

        internal void DidFailToOpenInterstitialAd()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Failed to open an insterstital ad");
                if (OnInterstitialAdFailedToOpen != null) OnInterstitialAdFailedToOpen();
            });
        }

        internal void DidCloseInterstitialAd()
        {
            Logger.LogInfo("Closed an interstitial ad");
            actions.Enqueue(() => {
                if (OnInterstitialAdClosed != null) OnInterstitialAdClosed();
            });
        }

        internal void DidRegisterForRewardedAds()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Registered for rewarded ads");
                if (OnDidRegisterForRewardedAds != null) OnDidRegisterForRewardedAds();
            });
        }

        internal void DidFailToRegisterForRewardedAds(string reason)
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Failed to register for rewarded ads: "+reason);
                if (OnDidFailToRegisterForRewardedAds != null) OnDidFailToRegisterForRewardedAds(reason);
            });
        }

        internal void DidOpenRewardedAd()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Opened a rewarded ad");
                if (OnRewardedAdOpened != null) OnRewardedAdOpened();
            });
        }

        internal void DidFailToOpenRewardedAd()
        {
            actions.Enqueue(() => {
                Logger.LogInfo("Failed to open a rewarded ad");
                if (OnRewardedAdFailedToOpen != null) OnRewardedAdFailedToOpen();
            });
        }

        internal void DidCloseRewardedAd(string rewardJSON)
        {
            bool reward = false;
            try {
                JSONObject obj = Json.Deserialize(rewardJSON) as JSONObject;
                if (obj.ContainsKey("reward")) {
                    reward = (bool)obj["reward"];
                }
            } catch (Exception) {}

            actions.Enqueue(() => {
                Logger.LogInfo("Closed a rewarded ad: reward="+reward);
                if (OnRewardedAdClosed != null) OnRewardedAdClosed(reward);
            });
        }

        internal void RecordEvent(string message)
        {
            JSONObject obj = null;
            try {
                obj = Json.Deserialize(message) as JSONObject;
            } catch (Exception exception) {
                Logger.LogError("Failed to record event: "+exception.Message);
            }

            var eventName = obj["eventName"] as String;
            var eventParameters = obj["parameters"] as JSONObject;

            if (obj != null) {
                actions.Enqueue(() => {
                    DDNA.Instance.RecordEvent(eventName, eventParameters);
                });
            }
        }

        internal void RequestEngagement(string request)
        {
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

                actions.Enqueue(() => {
                    EngageRequest engageRequest = new EngageRequest(decisionPoint);
                    engageRequest.Flavour = flavour;
                    engageRequest.Parameters = parameters;

                    StartCoroutine(Engage.Request(this, engageRequest, engageResponse));
                });
            }
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
                    Logger.LogDebug("Pausing SmartAds");
                    manager.OnPause();
                } else {
                    Logger.LogDebug("Resuming SmartAds");
                    manager.OnResume();
                }
            }
        }

        public override void OnDestroy()
        {
            if (manager != null) {
                Logger.LogDebug("Destroying StartAds");
                manager.OnDestroy();
            }
            base.OnDestroy();
        }
    }
}
