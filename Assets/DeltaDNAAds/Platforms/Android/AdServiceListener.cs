//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//

using UnityEngine;
using System.Collections.Generic;

namespace DeltaDNAAds.Android
{
    #if UNITY_ANDROID
    internal class AdServiceListener : UnityEngine.AndroidJavaProxy {

        private DDNASmartAds ads;
        private readonly IDictionary<string, AndroidJavaObject> engageListeners;

        internal AdServiceListener(DDNASmartAds ads, IDictionary<string, AndroidJavaObject> engageListeners) : base(Utils.AdServiceListenerClassName) {
            this.ads = ads;
            this.engageListeners = engageListeners;
        }

        void onRegisteredForAds() {
            ads.DidRegisterForInterstitialAds();
        }

        void onRegisteredForRewardedAds() {
            ads.DidRegisterForRewardedAds();
        }

        void onFailedToRegisterForAds(string reason) {
            ads.DidFailToRegisterForInterstitialAds(reason);
        }

        void onFailedToRegisterForRewardedAds(string reason) {
            ads.DidFailToRegisterForRewardedAds(reason);
        }

        void onAdOpened() {
            ads.DidOpenInterstitialAd();
        }

        void onAdFailedToOpen() {
            ads.DidFailToOpenInterstitialAd();
        }

        void onAdClosed() {
            ads.DidCloseInterstitialAd();
        }

        void onRewardedAdOpened() {
            ads.DidOpenRewardedAd();
        }

        void onRewardedAdFailedToOpen() {
            ads.DidFailToOpenRewardedAd();
        }

        void onRewardedAdClosed(bool completed) {
            string reward = completed ? "true" : "false";
            ads.DidCloseRewardedAd("{\"reward\":"+reward+"}");
        }

        void onRecordEvent(string eventName, string eventParamsJson) {
            ads.RecordEvent("{\"eventName\":\""+eventName+"\",\"parameters\":"+eventParamsJson+"}");
        }

        void onRequestEngagement(string decisionPoint, string flavour, AndroidJavaObject listener) {
            string id = System.Guid.NewGuid().ToString();
            engageListeners.Add(id, listener);

            ads.RequestEngagement(string.Format(
                "{{\"decisionPoint\":\"{0}\",\"flavour\":\"{1}\",\"parameters\":\"\",\"id\":\"{2}\"}}",
                decisionPoint,
                flavour,
                id));
        }

        string toString() {
            return "AdListener";
        }
    }
    #endif
}
