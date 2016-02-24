//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//

using UnityEngine;
using System.Collections;

namespace DeltaDNAAds {

    internal interface ISmartAdsManager {

        void RegisterForAds(string decisionPoint);
        bool IsInterstitialAdAvailable();
        void ShowInterstitialAd();
        void ShowInterstitialAd(string adPoint);
        bool IsRewardedAdAvailable();
        void ShowRewardedAd();
        void ShowRewardedAd(string adPoint);
        void EngageResponse(string id, string response, int statusCode, string error);
        void OnPause();
        void OnResume();
        void OnDestroy();
    }

}