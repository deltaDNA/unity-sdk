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
        private static extern void _showInterstitialAd(string adPoint); 

        [DllImport("__Internal")]
        private static extern bool _isRewardedAdAvailable();
        
        [DllImport("__Internal")]
        private static extern void _showRewardedAd(string adPoint); 

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

        public void ShowInterstitialAd(string adPoint)
        {
            _showInterstitialAd(adPoint);
        }

        public bool IsRewardedAdAvailable()
        {
            return _isRewardedAdAvailable();
        }

        public void ShowRewardedAd()
        {
            _showRewardedAd(null);
        }

        public void ShowRewardedAd(string adPoint)
        {
            _showRewardedAd(adPoint);
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
