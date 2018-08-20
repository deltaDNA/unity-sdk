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
#if UNITY_ANDROID
using System.Collections.Generic;
#if DDNA_SMARTADS
using UnityEngine;
#endif
#endif

namespace DeltaDNA.Ads.Android {

    #if UNITY_ANDROID
    using JSONObject = Dictionary<string, object>;
    #if DDNA_SMARTADS
    #endif

    internal class AdService : ISmartAdsManager {

        #if DDNA_SMARTADS
        private readonly AndroidJavaObject activity;
        private AndroidJavaObject adService;
        #endif

        internal AdService(SmartAds ads, string sdkVersion) {
            #if DDNA_SMARTADS
            try {
                activity = new AndroidJavaClass(Utils.UnityActivityClassName).GetStatic<AndroidJavaObject>("currentActivity");
                adService = new AndroidJavaObject(Utils.AdServiceWrapperClassName).CallStatic<AndroidJavaObject>(
                    "create", activity, new AdServiceListener(ads), sdkVersion);
            } catch (AndroidJavaException exception) {
                Logger.LogDebug("Exception creating Android AdService: "+exception.Message);
                throw new Exception("Native Android SmartAds AAR not found.");
            }
            #endif
        }

        public void RegisterForAds(JSONObject config, bool userConsent, bool ageRestricted) {
            #if DDNA_SMARTADS
            adService.Call(
                "configure",
                new AndroidJavaObject(Utils.JSONObjectClassName, MiniJSON.Json.Serialize(config)),
                config.ContainsKey("isCachedResponse") && (config["isCachedResponse"] as bool? ?? false),
                userConsent,
                ageRestricted);
            #endif
        }
        
        public bool IsInterstitialAdAllowed(Engagement engagement, bool checkTime) {
            #if DDNA_SMARTADS
            string parameters = getParameters(engagement);
            
            return adService.Call<bool>(
                "isInterstitialAdAllowed",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null,
                checkTime);
            #else
            return false;
            #endif
        }
        
        public bool IsRewardedAdAllowed(Engagement engagement, bool checkTime) {
            #if DDNA_SMARTADS
            string parameters = getParameters(engagement);
            
            return adService.Call<bool>(
                "isRewardedAdAllowed",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null,
                checkTime);
            #else
            return false;
            #endif
        }
        
        public long TimeUntilRewardedAdAllowed(Engagement engagement) {
            #if DDNA_SMARTADS
            string parameters = getParameters(engagement);
            
            return adService.Call<int>(
                "timeUntilRewardedAdAllowed",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null);
            #else
            return 0;
            #endif
        }
        
        public bool HasLoadedInterstitialAd() {
            #if DDNA_SMARTADS
            return adService.Call<bool>("hasLoadedInterstitialAd");
            #else
            return false;
            #endif
        }
        
        public bool HasLoadedRewardedAd() {
            #if DDNA_SMARTADS
            return adService.Call<bool>("hasLoadedRewardedAd");
            #else
            return false;
            #endif
        }
        
        public void ShowInterstitialAd(Engagement engagement) {
            #if DDNA_SMARTADS
            string parameters = getParameters(engagement);
            
            adService.Call(
                "showInterstitialAd",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null);
            #endif
        }
        
        public void ShowRewardedAd(Engagement engagement) {
            #if DDNA_SMARTADS
            string parameters = getParameters(engagement);
            
            adService.Call(
                "showRewardedAd",
                (engagement != null) ? engagement.DecisionPoint : null,
                (parameters != null) ? new AndroidJavaObject(Utils.JSONObjectClassName, parameters) : null);
            #endif
        }
        
        public DateTime? GetLastShown(string decisionPoint) {
            #if DDNA_SMARTADS
            var value = adService.Call<AndroidJavaObject>("getLastShown", decisionPoint);
            if (value != null) {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(value.Call<long>("getTime"));
            } else {
                return null;
            }
            #else
            return null;
            #endif
        }
        
        public long GetSessionCount(string decisionPoint) {
            #if DDNA_SMARTADS
            return adService.Call<int>("getSessionCount", decisionPoint);
            #else
            return 0;
            #endif
        }
        
        public long GetDailyCount(string decisionPoint) {
            #if DDNA_SMARTADS
            return adService.Call<int>("getDailyCount", decisionPoint);
            #else
            return 0;
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
        
        public void OnNewSession() {
            #if DDNA_SMARTADS
            adService.Call("onNewSession");
            #endif
        }
        
        #if DDNA_SMARTADS
        private static string getParameters(Engagement engagement) {
            string parameters = null;
            if (engagement != null
                && engagement.JSON != null
                && engagement.JSON.ContainsKey("parameters")) {
                try {
                    parameters = MiniJSON.Json.Serialize(engagement.JSON["parameters"]);
                } catch (Exception e) {
                    Logger.LogDebug("Exception serialising Engagement response parameters: " + e.Message);
                }
            }
            
            return parameters;
        }
        #endif
    }
    #endif
}
