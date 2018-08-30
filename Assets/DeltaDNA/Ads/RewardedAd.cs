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
    
    public class RewardedAd : Ad {
        
        private bool waitingToLoad;
        
        /// <summary>
        /// Called when the ad has loaded.
        /// </summary>
        public event Action<RewardedAd> OnRewardedAdLoaded;
        /// <summary>
        /// Called when the ad has expired due to another ad being shown.
        /// </summary>
        public event Action<RewardedAd> OnRewardedAdExpired;
        /// <summary>
        /// Called when the ad is shown on screen.
        /// </summary>
        public event Action<RewardedAd> OnRewardedAdOpened;
        /// <summary>
        /// Called when the ad has failed to show.
        /// The reason for the failure will be passed in the parameter.
        /// </summary>
        public event Action<RewardedAd, string> OnRewardedAdFailedToOpen;
        /// <summary>
        /// Called when the ad has been closed.
        /// Whether the user should be rewarded will be passed in the parameter.
        /// </summary>
        public event Action<RewardedAd, bool> OnRewardedAdClosed;

        private RewardedAd(Engagement engagement) : base(engagement)
        {
            this.engagement = engagement;
            
            SmartAds.Instance.OnRewardedAdLoaded -= NotifyOnLoaded;
            SmartAds.Instance.OnRewardedAdLoaded += NotifyOnLoaded;
            SmartAds.Instance.OnRewardedAdOpenedWithDecisionPoint -= NotifyOnOpened;
            SmartAds.Instance.OnRewardedAdOpenedWithDecisionPoint += NotifyOnOpened;
        }

        public static RewardedAd Create()
        {
            if (!SmartAds.Instance.IsRewardedAdAllowed(null, false)) return null;
            
            return CreateUnchecked(null);
        }
        
        public static RewardedAd Create(Engagement engagement) {
            if (!SmartAds.Instance.IsRewardedAdAllowed(engagement, false)) return null;
            
            return CreateUnchecked(engagement);
        }
        
        internal static RewardedAd CreateUnchecked(Engagement engagement) {
            if (engagement != null && engagement.JSON == null) {
                return new RewardedAd(null);
            } else {
                return new RewardedAd(engagement);
            }
        }
        
        public override bool IsReady()
        {
            if (engagement == null) {
                return SmartAds.Instance.HasLoadedRewardedAd();
            } else {
                return SmartAds.Instance.IsRewardedAdAllowed(engagement, true)
                    && SmartAds.Instance.HasLoadedRewardedAd();
            }
        }
        
        public override void Show()
        {
            SmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            SmartAds.Instance.OnRewardedAdOpened += this.OnRewaredAdOpenedHandler;
            SmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;
            SmartAds.Instance.OnRewardedAdFailedToOpen += this.OnRewardedAdFailedToOpenHandler;
            SmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;
            SmartAds.Instance.OnRewardedAdClosed += this.OnRewardedAdClosedHandler;
            
            if (engagement == null) Logger.LogWarning("Prefer showing ads with Engagements");
            SmartAds.Instance.ShowRewardedAd(engagement);
        }
        
        public string RewardType {
            get {
                var parameters = EngageParams;
                return (parameters != null) ? parameters["ddnaAdRewardType"] as string : null;
            }
        }
        
        public long RewardAmount {
            get { return EngageParams.GetOrDefault("ddnaAdRewardAmount", 0L); }
        }
        
        private void NotifyOnLoaded() {
            if (SmartAds.Instance.IsRewardedAdAllowed(engagement, true)) {
                waitingToLoad = false;
                
                if (OnRewardedAdLoaded != null) OnRewardedAdLoaded(this);
            } else if (!waitingToLoad) {
                SmartAds.Instance.StartCoroutine(NotifyOnLoadedDelayable(
                    SmartAds.Instance.TimeUntilRewardedAdAllowed(engagement)));
            }
        }
        
        private System.Collections.IEnumerator NotifyOnLoadedDelayable(float waitFor) {
            waitingToLoad = true;
            
            yield return new UnityEngine.WaitForSeconds(waitFor);
            
            if (waitingToLoad) {
                waitingToLoad = false;
                
                if (SmartAds.Instance.HasLoadedRewardedAd()
                    && OnRewardedAdLoaded != null) {
                    OnRewardedAdLoaded(this);
                }
            }
        }
        
        private void NotifyOnOpened(string decisionPoint) {
            if (engagement != null
                && !engagement.DecisionPoint.Equals(decisionPoint)
                && !waitingToLoad
                && OnRewardedAdExpired != null) {
                OnRewardedAdExpired(this);
            }
        }
        
        private void OnRewaredAdOpenedHandler()
        {
            SmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            SmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;

            if (OnRewardedAdOpened != null) OnRewardedAdOpened(this);
        }

        private void OnRewardedAdFailedToOpenHandler(string reason)
        {
            SmartAds.Instance.OnRewardedAdOpened -= this.OnRewaredAdOpenedHandler;
            SmartAds.Instance.OnRewardedAdFailedToOpen -= this.OnRewardedAdFailedToOpenHandler;
            SmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;

            if (OnRewardedAdFailedToOpen != null) OnRewardedAdFailedToOpen(this, reason);
        }

        private void OnRewardedAdClosedHandler(bool reward)
        {
            SmartAds.Instance.OnRewardedAdClosed -= this.OnRewardedAdClosedHandler;

            if (OnRewardedAdClosed != null) OnRewardedAdClosed(this, reward);
        }
    }
}
