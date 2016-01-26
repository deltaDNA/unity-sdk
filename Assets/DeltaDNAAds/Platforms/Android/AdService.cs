using UnityEngine;

namespace DeltaDNAAds.Android 
{
    #if UNITY_ANDROID
    internal class AdService : ISmartAdsManager
    {
        private AndroidJavaObject adService;
        private DDNASmartAds ads;
        
        internal AdService(DDNASmartAds ads) 
        {
            try {
                AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
                AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");           
                this.adService = new AndroidJavaObject(Utils.AdServiceClassName, activity, new AdServiceListener(ads));
            } catch (AndroidJavaException exception) {
                DeltaDNA.Logger.LogDebug("Exception creating Android AdService: "+exception.Message);
                throw new System.Exception("Native Android SmartAds AAR not found.");
            }
        }
            
        public void RegisterForAds(string decisionPoint) 
        {
            DeltaDNA.DDNA ddna = DeltaDNA.DDNA.Instance;
        
            adService.Call("init", 
                decisionPoint,
                ddna.EngageURL, 
                ddna.CollectURL,
                ddna.EnvironmentKey, 
                ddna.HashSecret, 
                ddna.UserID, 
                ddna.SessionID, 
                DeltaDNA.Settings.ENGAGE_API_VERSION, 
                DeltaDNA.Settings.SDK_VERSION, 
                ddna.Platform, 
                DeltaDNA.ClientInfo.TimezoneOffset, 
                DeltaDNA.ClientInfo.Manufacturer, 
                DeltaDNA.ClientInfo.OperatingSystemVersion
            );
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

        public void EngageResponse(string id, string response, int statusCode, string error)
        {
            // TODO: Pass back to Android SDK
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