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

    public class InterstitialAd {

        public event Action OnInterstitialAdOpened;
        public event Action<string> OnInterstitialAdFailedToOpen;
        public event Action OnInterstitialAdClosed;

        private InterstitialAd()
        {

        }

        public static InterstitialAd Create()
        {
            if (!DDNASmartAds.Instance.IsInterstitialAdAllowed(null)) return null;

            var instance = new InterstitialAd();
            instance.Parameters = new JSONObject();
            return instance;
        }

        public static InterstitialAd Create(Engagement engagement)
        {
            if (!DDNASmartAds.Instance.IsInterstitialAdAllowed(engagement)) return null;

            JSONObject parameters = null;

            if (engagement != null && engagement.JSON != null) {
                if (engagement.JSON.ContainsKey("parameters")) {
                    parameters = engagement.JSON["parameters"] as JSONObject;
                }
            }

            var instance = new InterstitialAd();
            instance.Parameters = parameters ?? new JSONObject();

            return instance;
        }

        public bool IsReady()
        {
            return DDNASmartAds.Instance.IsInterstitialAdAvailable();
        }

        public void Show()
        {
            DDNASmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            DDNASmartAds.Instance.OnInterstitialAdOpened += this.OnInterstitialAdOpenedHandler;
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen += this.OnInterstitialAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;
            DDNASmartAds.Instance.OnInterstitialAdClosed += this.OnInterstitialAdClosedHandler;

            DDNASmartAds.Instance.ShowInterstitialAd();
        }

        public JSONObject Parameters { get; private set; }

        private void OnInterstitialAdOpenedHandler()
        {
            DDNASmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;

            if (this.OnInterstitialAdOpened != null) {
                this.OnInterstitialAdOpened();
            }
        }

        private void OnInterstitialAdFailedToOpenHandler(string reason)
        {
            DDNASmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;

            if (this.OnInterstitialAdFailedToOpen != null) {
                this.OnInterstitialAdFailedToOpen(reason);
            }
        }

        private void OnInterstitialAdClosedHandler()
        {
            DDNASmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;

            if (this.OnInterstitialAdClosed != null) {
                this.OnInterstitialAdClosed();
            }
        }
    }
}
