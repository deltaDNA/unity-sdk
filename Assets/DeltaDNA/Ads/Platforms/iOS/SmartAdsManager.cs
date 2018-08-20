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
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace DeltaDNA.Ads.iOS {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    internal class SmartAdsManager : ISmartAdsManager {

        #if UNITY_IOS && DDNA_SMARTADS
        #region Interface to native implementation

        [DllImport("__Internal")]
        private static extern void _registerForAds(string config, bool userConsent, bool ageRestricted);

        [DllImport("__Internal")]
        private static extern int _isInterstitialAdAllowed(string decisionPoint, string engageParams, bool checkTime);

        [DllImport("__Internal")]
        private static extern int _hasLoadedInterstitialAd();

        [DllImport("__Internal")]
        private static extern void _showInterstitialAd(string decisionPoint, string engageParams);

        [DllImport("__Internal")]
        private static extern int _isRewardedAdAllowed(string decisionPoint, string engageParams, bool CheckTime);

        [DllImport("__Internal")]
        private static extern long _timeUntilRewardedAdAllowed(string decisionPoint, string engageParams);

        [DllImport("__Internal")]
        private static extern int _hasLoadedRewardedAd();

        [DllImport("__Internal")]
        private static extern void _showRewardedAd(string decisionPoint, string engageParams);

        [DllImport("__Internal")]
        private static extern long _getLastShown(string decisionPoint);

        [DllImport("__Internal")]
        private static extern long _getSessionCount(string decisionPoint);

        [DllImport("__Internal")]
        private static extern long _getDailyCount(string decisionPoint);

        [DllImport("__Internal")]
        private static extern void _pause();

        [DllImport("__Internal")]
        private static extern void _resume();

        [DllImport("__Internal")]
        private static extern void _destroy();
        
        [DllImport("__Internal")]
        private static extern void _setLoggingLevel(int level);
        
        [DllImport("__Internal")]  
        private static extern void _fireEventNewSession(); 

        #endregion
        #endif

        #region Public interface

        public void RegisterForAds(JSONObject config, bool userConsent, bool ageRestricted)
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _setLoggingLevel((int)Logger.LogLevel);
            try {
                _registerForAds(MiniJSON.Json.Serialize(config), userConsent, ageRestricted);
            } catch (Exception e) {
                Logger.LogDebug("Exception serialising session config: " + e.Message);
            }
            
            #endif
        }

        public bool IsInterstitialAdAllowed(Engagement engagement, bool checkTime)
        {
            #if UNITY_IOS && DDNA_SMARTADS
            return _isInterstitialAdAllowed(
                (engagement != null) ? engagement.DecisionPoint : null,
                GetParameters(engagement),
                checkTime) > 0;
            #else
            return false;
            #endif
        }

        public bool HasLoadedInterstitialAd()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            return _hasLoadedInterstitialAd() > 0;
            #else
            return false;
            #endif
        }

        public void ShowInterstitialAd(Engagement engagement)
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _showInterstitialAd(
                (engagement != null) ? engagement.DecisionPoint : null,
                GetParameters(engagement));
            #endif
        }

        public bool IsRewardedAdAllowed(Engagement engagement, bool checkTime)
        {
            #if UNITY_IOS && DDNA_SMARTADS
            return _isRewardedAdAllowed(
                (engagement != null) ? engagement.DecisionPoint : null,
                GetParameters(engagement),
                checkTime) > 0;
            #else
            return false;
            #endif
        }

        public long TimeUntilRewardedAdAllowed(Engagement engagement) {
            #if UNITY_IOS && DDNA_SMARTADS
            return _timeUntilRewardedAdAllowed(
                (engagement != null) ? engagement.DecisionPoint : null,
                GetParameters(engagement));
            #else
            return 0;
            #endif
        }

        public bool HasLoadedRewardedAd()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            return _hasLoadedRewardedAd() > 0;
            #else
            return false;
            #endif
        }

        public void ShowRewardedAd(Engagement engagement)
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _showRewardedAd(
                (engagement != null) ? engagement.DecisionPoint : null,
                GetParameters(engagement));
            #endif
        }

        public DateTime? GetLastShown(string decisionPoint) {
            #if UNITY_IOS && DDNA_SMARTADS
            var value = _getLastShown(decisionPoint);
            if (value > 0) {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(value);
            } else {
                return null;
            }
            #else
            return null;
            #endif
        }

        public long GetSessionCount(string decisionPoint) {
            #if UNITY_IOS && DDNA_SMARTADS
            return _getSessionCount(decisionPoint);
            #else
            return 0;
            #endif
        }

        public long GetDailyCount(string decisionPoint) {
            #if  UNITY_IOS && DDNA_SMARTADS
            return _getDailyCount(decisionPoint);
            #else
            return 0;
            #endif
        }

        public void OnPause()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _pause();
            #endif
        }

        public void OnResume()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _resume();
            #endif
        }

        public void OnDestroy()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _destroy();
            #endif
        }

        public void OnNewSession()
        {
            #if UNITY_IOS && DDNA_SMARTADS
            _fireEventNewSession();
            #endif
        }

        #endregion

        #if UNITY_IOS && DDNA_SMARTADS
        private static string GetParameters(Engagement engagement) {
            string parameters = null;
            if (engagement != null
                && engagement.JSON != null
                && engagement.JSON.ContainsKey("parameters")) {
                try {
                    parameters = MiniJSON.Json.Serialize(engagement.JSON[@"parameters"]);
                } catch (Exception e) {
                    Logger.LogDebug("Exception serialising Engagement response parameters: " + e.Message);
                }
            }
            
            return parameters;
        }
        #endif
    }
}
