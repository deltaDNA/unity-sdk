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
using System;

namespace DeltaDNAAds {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class RewardedAd {

        public event Action OnRewardedAdOpened;
        public event Action<string> OnRewardedAdFailedToOpen;
        public event Action<bool> OnRewardedAdClosed;

        private RewardedAd()
        {

        }

        public static RewardedAd Create()
        {
            if (!DDNASmartAds.Instance.IsRewardedAdAllowed(null)) return null;

            var instance = new RewardedAd();
            instance.Parameters = new JSONObject();
            return instance;
        }

        public static RewardedAd Create(Engagement engagement)
        {
            if (!DDNASmartAds.Instance.IsRewardedAdAllowed(engagement)) return null;

            JSONObject parameters = null;

            if (engagement != null && engagement.JSON != null) {
                if (engagement.JSON.ContainsKey("parameters")) {
                    parameters = engagement.JSON["parameters"] as JSONObject;
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
            DDNASmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdOpened += this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen += this.OnRewardedAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;
            DDNASmartAds.Instance.OnRewardedAdClosed += this.OnRewardedAdClosedHandler;

            DDNASmartAds.Instance.ShowRewardedAd();
        }

        public JSONObject Parameters { get; private set; }

        private void OnRewaredAdOpenedHandler()
        {
            DDNASmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;

            if (this.OnRewardedAdOpened != null) {
                this.OnRewardedAdOpened();
            }
        }

        private void OnRewardedAdFailedToOpenHandler(string reason)
        {
            DDNASmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            DDNASmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;

            if (this.OnRewardedAdFailedToOpen != null) {
                this.OnRewardedAdFailedToOpen(reason);
            }
        }

        private void OnRewardedAdClosedHandler(bool reward)
        {
            DDNASmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;

            if (this.OnRewardedAdClosed != null) {
                this.OnRewardedAdClosed(reward);
            }
        }
    }
}
