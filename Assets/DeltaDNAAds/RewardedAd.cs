using UnityEngine;
using System.Collections;
using DeltaDNA;
using System;

namespace DeltaDNAAds {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class RewardedAd {

        public event Action OnRewardedAdOpened;
        public event Action<string> OnRewardedAdFailedToOpen;
        public event Action<bool> OnRewardedAdClosed;

        private RewardedAd()
        {
            DDNASmartAds.Instance.OnRewardedAdOpened += this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen += this.OnRewardedAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnRewardedAdClosed += this.OnRewardedAdClosedHandler;
        }

        ~RewardedAd()
        {
            DDNASmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;
        }

        public static RewardedAd Create()
        {
            var instance = new RewardedAd();
            instance.Parameters = new JSONObject();
            return instance;
        }

        public static RewardedAd Create(Engagement engagement)
        {
            JSONObject parameters = null;

            if (engagement != null && engagement.JSON != null) {
                if (engagement.JSON.ContainsKey("parameters")) {
                    parameters = engagement.JSON["parameters"] as JSONObject;
                    if (parameters.ContainsKey("adShowPoint") && !(bool)parameters["adShowPoint"])
                        return null;
                }
            }

            var instance = new RewardedAd();
            instance.Parameters = parameters ?? new JSONObject();

            return instance;
        }

        public bool IsReady()
        {
            return DDNASmartAds.Instance.IsRewardedAdAvailable();
        }

        public void Show()
        {
            DDNASmartAds.Instance.ShowRewardedAd();
        }

        public JSONObject Parameters { get; private set; }

        private void OnRewaredAdOpenedHandler() 
        {
            if (this.OnRewardedAdOpened != null) {
                this.OnRewardedAdOpened();
            }
        }

        private void OnRewardedAdFailedToOpenHandler(string reason)
        {
            if (this.OnRewardedAdFailedToOpen != null) {
                this.OnRewardedAdFailedToOpen(reason);
            }
        }

        private void OnRewardedAdClosedHandler(bool reward)
        {
            if (this.OnRewardedAdClosed != null) {
                this.OnRewardedAdClosed(reward);
            }
        }
    }
}
