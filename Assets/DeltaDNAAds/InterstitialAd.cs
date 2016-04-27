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
            DDNASmartAds.Instance.OnInterstitialAdOpened += this.OnInterstitialAdOpenedHandler;
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen += this.OnInterstitialAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnInterstitialAdClosed += this.OnInterstitialAdClosedHandler;
        }

        ~InterstitialAd()
        {
            DDNASmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler; 
            DDNASmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;
            DDNASmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;
        }

        public static InterstitialAd Create()
        {
            var instance = new InterstitialAd();
            instance.Parameters = new JSONObject();
            return instance;
        }

        public static InterstitialAd Create(Engagement engagement)
        {
            JSONObject parameters = null;

            if (engagement != null && engagement.JSON != null) {
                if (engagement.JSON.ContainsKey("parameters")) {
                    parameters = engagement.JSON["parameters"] as JSONObject;
                    if (parameters.ContainsKey("adShowPoint") && !(bool)parameters["adShowPoint"])
                        return null;
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
            DDNASmartAds.Instance.ShowInterstitialAd();
        }

        public JSONObject Parameters { get; private set; }

        private void OnInterstitialAdOpenedHandler() 
        {
            if (this.OnInterstitialAdOpened != null) {
                this.OnInterstitialAdOpened();
            }
        }

        private void OnInterstitialAdFailedToOpenHandler(string reason)
        {
            if (this.OnInterstitialAdFailedToOpen != null) {
                this.OnInterstitialAdFailedToOpen(reason);
            }
        }

        private void OnInterstitialAdClosedHandler()
        {
            if (this.OnInterstitialAdClosed != null) {
                this.OnInterstitialAdClosed();
            }
        }
    }
}
