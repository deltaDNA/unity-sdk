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
using System.Collections.Generic;

namespace DeltaDNA.Ads.Android
{
    #if UNITY_ANDROID
    internal class AdServiceListener : AndroidJavaProxy {

        private SmartAds ads;
        private readonly IDictionary<string, AndroidJavaObject> engageListeners;

        internal AdServiceListener(SmartAds ads, IDictionary<string, AndroidJavaObject> engageListeners) : base(Utils.AdServiceListenerClassName) {
            this.ads = ads;
            this.engageListeners = engageListeners;
        }

        void onRegisteredForInterstitialAds() {
            ads.DidRegisterForInterstitialAds();
        }

        void onFailedToRegisterForInterstitialAds(string reason) {
            ads.DidFailToRegisterForInterstitialAds(reason);
        }

        void onRegisteredForRewardedAds() {
            ads.DidRegisterForRewardedAds();
        }

        void onFailedToRegisterForRewardedAds(string reason) {
            ads.DidFailToRegisterForRewardedAds(reason);
        }

        void onInterstitialAdOpened() {
            ads.DidOpenInterstitialAd();
        }

        void onInterstitialAdFailedToOpen(string reason) {
            ads.DidFailToOpenInterstitialAd(reason);
        }

        void onInterstitialAdClosed() {
            ads.DidCloseInterstitialAd();
        }
        
        void onRewardedAdLoaded() {
            ads.DidLoadRewardedAd();
        }
        
        void onRewardedAdOpened(string decisionPoint) {
            ads.DidOpenRewardedAd(decisionPoint);
        }

        void onRewardedAdFailedToOpen(string reason) {
            ads.DidFailToOpenRewardedAd(reason);
        }

        void onRewardedAdClosed(bool completed) {
            string reward = completed ? "true" : "false";
            ads.DidCloseRewardedAd("{\"reward\":"+reward+"}");
        }

        void onRecordEvent(string eventName, string eventParamsJson) {
            ads.RecordEvent("{\"eventName\":\""+eventName+"\",\"parameters\":"+eventParamsJson+"}");
        }

        void onRequestEngagement(string decisionPoint, string flavour, string version, AndroidJavaObject listener) {
            string id = System.Guid.NewGuid().ToString();
            engageListeners.Add(id, listener);

            ads.RequestEngagement(string.Format(
                "{{\"decisionPoint\":\"{0}\",\"flavour\":\"{1}\",\"parameters\":{{\"adSdkVersion\":\"{2}\"}},\"id\":\"{3}\"}}",
                decisionPoint,
                flavour,
                version,
                id));
        }

        #if !UNITY_2017_1_OR_NEWER
        string toString() {
            return "UnityAdServiceListener";
        }
        #endif
    }
    #endif
}
