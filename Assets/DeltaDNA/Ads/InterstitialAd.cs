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

namespace DeltaDNA {
    
    public class InterstitialAd : Ad {
        
        /// <summary>
        /// Called when the ad is shown on screen.
        /// </summary>
        public event Action<InterstitialAd> OnInterstitialAdOpened;
        /// <summary>
        /// Called when the ad has failed to show.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<InterstitialAd, string> OnInterstitialAdFailedToOpen;
        /// <summary>
        /// Called when the ad has been closed.
        /// </summary>
        public event Action<InterstitialAd> OnInterstitialAdClosed;

        private InterstitialAd(Engagement engagement) : base(engagement)
        {

        }

        public static InterstitialAd Create()
        {
            if (!SmartAds.Instance.IsInterstitialAdAllowed(null, false)) return null;
            
            return CreateUnchecked(null);
        }

        public static InterstitialAd Create(Engagement engagement)
        {
            if (!SmartAds.Instance.IsInterstitialAdAllowed(engagement, false)) return null;
            
            return CreateUnchecked(engagement);
        }
        
        internal static InterstitialAd CreateUnchecked(Engagement engagement) {
            if (engagement != null && engagement.JSON == null) {
                return new InterstitialAd(null);
            } else {
                return new InterstitialAd(engagement);
            }
        }
        
        public override bool IsReady()
        {
            if (engagement  == null) {
                return SmartAds.Instance.HasLoadedInterstitialAd();
            } else {
                return SmartAds.Instance.IsInterstitialAdAllowed(engagement, true)
                    && SmartAds.Instance.HasLoadedInterstitialAd();
            }
        }
        
        public override void Show()
        {
            SmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            SmartAds.Instance.OnInterstitialAdOpened += this.OnInterstitialAdOpenedHandler;
            SmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;
            SmartAds.Instance.OnInterstitialAdFailedToOpen += this.OnInterstitialAdFailedToOpenHandler;
            SmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;
            SmartAds.Instance.OnInterstitialAdClosed += this.OnInterstitialAdClosedHandler;
            
            if (engagement == null) Logger.LogWarning("Prefer showing ads with Engagements");
            SmartAds.Instance.ShowInterstitialAd(engagement);
        }
        
        private void OnInterstitialAdOpenedHandler()
        {
            SmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            SmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;

            if (OnInterstitialAdOpened != null) OnInterstitialAdOpened(this);
        }

        private void OnInterstitialAdFailedToOpenHandler(string reason)
        {
            SmartAds.Instance.OnInterstitialAdOpened -= this.OnInterstitialAdOpenedHandler;
            SmartAds.Instance.OnInterstitialAdFailedToOpen -= this.OnInterstitialAdFailedToOpenHandler;
            SmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;

            if (OnInterstitialAdFailedToOpen != null) OnInterstitialAdFailedToOpen(this, reason);
        }

        private void OnInterstitialAdClosedHandler()
        {
            SmartAds.Instance.OnInterstitialAdClosed -= this.OnInterstitialAdClosedHandler;

            if (OnInterstitialAdClosed != null) OnInterstitialAdClosed(this);
        }
    }
}
