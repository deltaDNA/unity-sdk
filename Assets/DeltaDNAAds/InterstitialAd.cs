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
