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

namespace DeltaDNA.Ads {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;
    
    internal interface ISmartAdsManager {
        
        void RegisterForAds(JSONObject config, bool userConsent, bool ageRestricted);
        
        bool IsInterstitialAdAllowed(Engagement engagement, bool checkTime);
        bool IsRewardedAdAllowed(Engagement engagement, bool checkTime);
        long TimeUntilRewardedAdAllowed(Engagement engagement);
        
        bool HasLoadedInterstitialAd();
        bool HasLoadedRewardedAd();
        
        void ShowInterstitialAd(Engagement engagement);
        void ShowRewardedAd(Engagement engagement);
        
        DateTime? GetLastShown(string decisionPoint);
        long GetSessionCount(string decisionPoint);
        long GetDailyCount(string decisionPoint);
        
        void OnPause();
        void OnResume();
        void OnDestroy();
        void OnNewSession();
    }
}
