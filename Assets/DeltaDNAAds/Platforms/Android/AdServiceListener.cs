namespace DeltaDNAAds.Android
{
    #if UNITY_ANDROID

    internal class AdServiceListener : UnityEngine.AndroidJavaProxy {

        private DDNASmartAds ads;

        internal AdServiceListener(DDNASmartAds ads)
            : base(Utils.AdServiceListenerClassName)
        {
            this.ads = ads;
        }

        void onRegisteredForAds() {
            ads.DidRegisterForInterstitialAds();
        }

        void onRegisteredForRewardedAds() {
            ads.DidRegisterForRewardedAds();
        }

        void onFailedToRegisterForAds(string reason) {
            ads.DidFailToRegisterForInterstitialAds(reason);
        }

        void onFailedToRegisterForRewardedAds(string reason) {
            ads.DidFailToRegisterForRewardedAds(reason);
        }

        void onAdOpened() {
            ads.DidOpenInterstitialAd();
        }

        void onAdFailedToOpen() {
            ads.DidFailToOpenInterstitialAd();
        }

        void onAdClosed() {
            ads.DidCloseInterstitialAd();
        }

        void onRewardedAdOpened() {
            ads.DidOpenRewardedAd();
        }

        void onRewardedAdFailedToOpen() {
            ads.DidFailToOpenRewardedAd();
        }

        void onRewardedAdClosed(bool completed) {
            ads.DidCloseRewardedAd("{reward:"+completed+"}");
        }

        void onRecordEvent(string eventName, string eventParamsJson) {
            ads.RecordEvent("{eventName:"+eventName+",parameters:"+eventParamsJson+"}");
        }

        string toString() {
            return "AdListener";
        }
    }
    #endif
}