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

#if DDNA_SMARTADS
using UnityEngine;
using System;
using System.Collections.Generic;
#endif

namespace DeltaDNA.Ads.Android {

    #if UNITY_ANDROID
    #if DDNA_SMARTADS
    using JSONObject = Dictionary<string, object>;
    #endif

    internal class AdService : ISmartAdsManager {

        #if DDNA_SMARTADS
        private readonly IDictionary<string, AndroidJavaObject> engageListeners;
        private readonly AndroidJavaObject activity;
        private AndroidJavaObject adService;
        #endif

        internal AdService(SmartAds ads, string sdkVersion) {
            #if DDNA_SMARTADS
            engageListeners = new Dictionary<string, AndroidJavaObject>();

            try {
                activity = new AndroidJavaClass(Utils.UnityActivityClassName).GetStatic<AndroidJavaObject>("currentActivity");
                adService = new AndroidJavaObject(Utils.AdServiceWrapperClassName).CallStatic<AndroidJavaObject>(
                    "create", activity, new AdServiceListener(ads, engageListeners), sdkVersion);
            } catch (AndroidJavaException exception) {
                Logger.LogDebug("Exception creating Android AdService: "+exception.Message);
                throw new Exception("Native Android SmartAds AAR not found.");
            }
            #endif
        }

        public void RegisterForAds(string decisionPoint) {
            #if DDNA_SMARTADS
            adService.Call("registerForAds", decisionPoint);
            #endif
        }

        public void OnNewSession() {
            #if DDNA_SMARTADS
            adService.Call("onNewSession");
            #endif
        }

        public bool IsInterstitialAdAllowed(Engagement engagement) {
            #if DDNA_SMARTADS
            string parameters = null;
            if (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters")) {
                try {
                    parameters = MiniJSON.Json.Serialize(engagement.JSON["parameters"]);
                } catch (Exception e) {
                    Logger.LogDebug("Exception serialising Engagement response parameters: " + e.Message);
                }
            }

            return adService.Call<bool>(
                "isInterstitialAdAllowed",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null);
            #else
            return false;
            #endif
        }

        public bool IsInterstitialAdAvailable() {
            #if DDNA_SMARTADS
            return adService.Call<bool>("isInterstitialAdAvailable");
            #else
            return false;
            #endif
        }

        public void ShowInterstitialAd() {
            ShowInterstitialAd(null);
        }

        public void ShowInterstitialAd(string decisionPoint) {
            #if DDNA_SMARTADS
            adService.Call("showInterstitialAd", decisionPoint);
            #endif
        }

        public bool IsRewardedAdAllowed(Engagement engagement) {
            #if DDNA_SMARTADS
            string parameters = null;
            if (engagement != null && engagement.JSON != null && engagement.JSON.ContainsKey("parameters")) {
                try {
                    parameters = MiniJSON.Json.Serialize(engagement.JSON["parameters"]);
                } catch (Exception e) {
                    Logger.LogDebug("Exception serialising Engagement response parameters: " + e.Message);
                }
            }

            return adService.Call<bool>(
                "isRewardedAdAllowed",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null);
            #else
            return false;
            #endif
        }

        public bool IsRewardedAdAvailable() {
            #if DDNA_SMARTADS
            return adService.Call<bool>("isRewardedAdAvailable");
            #else
            return false;
            #endif
        }

        public void ShowRewardedAd() {
            ShowRewardedAd(null);
        }

        public void ShowRewardedAd(string decisionPoint) {
            #if DDNA_SMARTADS
            adService.Call("showRewardedAd", decisionPoint);
            #endif
        }

        public void EngageResponse(string id, string response, int statusCode, string error) {
            #if DDNA_SMARTADS
            // android sdk expects request listener callbacks on the main thread
            #if UNITY_2017_1_OR_NEWER
            activity.Call("runOnUiThread", new Runnable(() => {
            #else
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            #endif
                AndroidJavaObject listener;
                if (engageListeners.TryGetValue(id, out listener)) {
                    JSONObject json = null;
                    if (!string.IsNullOrEmpty(response)) {
                        try {
                            json = MiniJSON.Json.Deserialize(response) as JSONObject;
                        } catch (Exception) { /* invalid json */ }
                    }

                    if (json == null
                        || (statusCode < 200
                            || statusCode >= 300
                            && !json.ContainsKey("isCachedResponse"))) {
                        listener.Call("onFailure", new AndroidJavaObject(Utils.ThrowableClassName, error));
                    } else {
                        listener.Call("onSuccess", new AndroidJavaObject(Utils.JSONObjectClassName, response));
                    }

                    engageListeners.Remove(id);
                }
            }));
            #endif
        }

        public void OnPause()
        {
            #if DDNA_SMARTADS
            adService.Call("onPause");
            #endif
        }

        public void OnResume()
        {
            #if DDNA_SMARTADS
            adService.Call("onResume");
            #endif
        }

        public void OnDestroy()
        {
            #if DDNA_SMARTADS
            adService.Call("onDestroy");
            #endif
        }
        
        #if DDNA_SMARTADS && UNITY_2017_1_OR_NEWER
        private class Runnable : AndroidJavaProxy {

            private readonly Action action;

            internal Runnable(Action action) : base("java.lang.Runnable") {
                this.action = action;
            }

            public void run() {
                action();
            }
        }
        #endif
    }
    #endif
}
