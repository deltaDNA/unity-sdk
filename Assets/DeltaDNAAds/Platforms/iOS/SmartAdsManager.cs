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
using System.Runtime.InteropServices;

namespace DeltaDNAAds.iOS {

    internal class SmartAdsManager : ISmartAdsManager {

        #region Interface to native implementation

        [DllImport("__Internal")]
        private static extern void _registerForAds(string decisionPoint);

        [DllImport("__Internal")]
        private static extern bool _isInterstitialAdAvailable();

        [DllImport("__Internal")]
        private static extern void _showInterstitialAd(string decisionPoint);

        [DllImport("__Internal")]
        private static extern bool _isRewardedAdAvailable();

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

        #region Public interface

        public void RegisterForAds(string decisionPoint)
        {
            _registerForAds(decisionPoint);
        }

        public bool IsInterstitialAdAvailable()
        {
            return _isInterstitialAdAvailable();
        }

        public void ShowInterstitialAd()
        {
            _showInterstitialAd(null);
        }

        public void ShowInterstitialAd(string decisionPoint)
        {
            _showInterstitialAd(decisionPoint);
        }

        public bool IsRewardedAdAvailable()
        {
            return _isRewardedAdAvailable();
        }

        public void ShowRewardedAd()
        {
            _showRewardedAd(null);
        }

        public void ShowRewardedAd(string decisionPoint)
        {
            _showRewardedAd(decisionPoint);
        }

        public void EngageResponse(string id, string response, int statusCode, string error)
        {
            _engageResponse(id, response, statusCode, error);
        }

        public void OnPause()
        {
            _pause();
        }

        public void OnResume()
        {
            _resume();
        }

        public void OnDestroy()
        {
            _destroy();
        }

        #endregion

    }

}
