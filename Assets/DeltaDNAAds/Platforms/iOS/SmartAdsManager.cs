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

using DeltaDNA;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace DeltaDNAAds.iOS {

    internal class SmartAdsManager : ISmartAdsManager {

        #if UNITY_IOS
        #region Interface to native implementation

        [DllImport("__Internal")]
        private static extern void _registerForAds(string decisionPoint);

        [DllImport("__Internal")]
        private static extern int _isInterstitialAdAllowed(string decisionPoint, string engageParams);

        [DllImport("__Internal")]
        private static extern int _isInterstitialAdAvailable();

        [DllImport("__Internal")]
        private static extern void _showInterstitialAd(string decisionPoint);

        [DllImport("__Internal")]
        private static extern int _isRewardedAdAllowed(string decisionPoint, string engageParams);

        [DllImport("__Internal")]
        private static extern int _isRewardedAdAvailable();

        [DllImport("__Internal")]
        private static extern void _showRewardedAd(string decisionPoint);

        [DllImport("__Internal")]
        private static extern void _engageResponse(string id, string response, int statusCode, string error);

        [DllImport("__Internal")]
        private static extern void _pause();

        [DllImport("__Internal")]
        private static extern void _resume();

        [DllImport("__Internal")]
        private static extern void _destroy();

        #endregion
        #endif

        #region Public interface

        public void RegisterForAds(string decisionPoint)
        {
            #if UNITY_IOS
            _registerForAds(decisionPoint);
            #endif
        }

        public bool IsInterstitialAdAllowed(Engagement engagement)
        {
            #if UNITY_IOS
            string decisionPoint = null;
            string engageParams = null;

            if (engagement != null && engagement.JSON != null) {
                try {
                    decisionPoint = engagement.DecisionPoint;
                    engageParams = DeltaDNA.MiniJSON.Json.Serialize(engagement.JSON[@"parameters"]);
                } catch (System.Exception) {}
            }
            return _isInterstitialAdAllowed(decisionPoint, engageParams) > 0;
            #else
            return false;
            #endif
        }

        public bool IsInterstitialAdAvailable()
        {
            #if UNITY_IOS
            return _isInterstitialAdAvailable() > 0;
            #else
            return false;
            #endif
        }

        public void ShowInterstitialAd()
        {
            #if UNITY_IOS
            _showInterstitialAd(null);
            #endif
        }

        public void ShowInterstitialAd(string decisionPoint)
        {
            #if UNITY_IOS
            _showInterstitialAd(decisionPoint);
            #endif
        }

        public bool IsRewardedAdAllowed(Engagement engagement)
        {
            #if UNITY_IOS
            string decisionPoint = null;
            string engageParams = null;

            if (engagement != null && engagement.JSON != null) {
                try {
                    decisionPoint = engagement.DecisionPoint;
                    engageParams = DeltaDNA.MiniJSON.Json.Serialize(engagement.JSON[@"parameters"]);
                } catch (System.Exception) {}
            }
            return _isRewardedAdAllowed(decisionPoint, engageParams) > 0;
            #else
            return false;
            #endif
        }

        public bool IsRewardedAdAvailable()
        {
            #if UNITY_IOS
            return _isRewardedAdAvailable() > 0;
            #else
            return false;
            #endif
        }

        public void ShowRewardedAd()
        {
            #if UNITY_IOS
            _showRewardedAd(null);
            #endif
        }

        public void ShowRewardedAd(string decisionPoint)
        {
            #if UNITY_IOS
            _showRewardedAd(decisionPoint);
            #endif
        }

        public void EngageResponse(string id, string response, int statusCode, string error)
        {
            #if UNITY_IOS
            _engageResponse(id, response, statusCode, error);
            #endif
        }

        public void OnPause()
        {
            #if UNITY_IOS
            _pause();
            #endif
        }

        public void OnResume()
        {
            #if UNITY_IOS
            _resume();
            #endif
        }

        public void OnDestroy()
        {
            #if UNITY_IOS
            _destroy();
            #endif
        }

        #endregion
    }
}
