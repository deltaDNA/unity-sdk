//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//

using UnityEngine;
using System.Collections.Generic;

namespace DeltaDNAAds.Android 
{
    #if UNITY_ANDROID
    internal class AdService : ISmartAdsManager
    {
        private readonly IDictionary<string, AndroidJavaObject> engageListeners;
        private readonly AndroidJavaObject activity;
        private AndroidJavaObject adService;
        
        internal AdService(DDNASmartAds ads) {
            engageListeners = new Dictionary<string, AndroidJavaObject>();

            try {
                AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
                activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");           
                adService = new AndroidJavaObject(Utils.AdServiceClassName, activity, new AdServiceListener(ads, engageListeners));
            } catch (AndroidJavaException exception) {
                DeltaDNA.Logger.LogDebug("Exception creating Android AdService: "+exception.Message);
                throw new System.Exception("Native Android SmartAds AAR not found.");
            }
        }
        
        public void RegisterForAds(string decisionPoint) 
        {
            adService.Call("init", decisionPoint);
        }

        public bool IsInterstitialAdAvailable()
        {
            return adService.Call<bool>("isInterstitialAdAvailable");
        }
        
        public void ShowInterstitialAd() 
        {
            adService.Call("showAd");
        }
        
        public void ShowInterstitialAd(string adPoint) 
        {
            adService.Call("showAd", adPoint);
        }

        public bool IsRewardedAdAvailable()
        {
            return adService.Call<bool>("isRewardedAdAvailable");
        }

        public void ShowRewardedAd()
        {
            adService.Call("showRewardedAd");
        }

        public void ShowRewardedAd(string adPoint)
        {
            adService.Call("showRewardedAd", adPoint);
        }

        public void EngageResponse(string id, string response, int statusCode, string error) {
            // android sdk expects request listener callbacks on the main thread
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                AndroidJavaObject listener;
                if (engageListeners.TryGetValue(id, out listener)) {
                    if (statusCode >= 200 && statusCode < 300) {
                        listener.Call("onSuccess", new AndroidJavaObject(Utils.JSONObjectClassName, response));
                    } else {
                        listener.Call("onFailure", new AndroidJavaObject(Utils.ThrowableClassName, error));
                    }

                    engageListeners.Remove(id);
                }
            }));
        }
        
        public void OnPause() 
        {
            adService.Call("onPause");
        }
        
        public void OnResume() 
        {
            adService.Call("onResume");
        }
        
        public void OnDestroy() 
        {
            adService.Call("onDestroy");
        }
    }
    #endif
}