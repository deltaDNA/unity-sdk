//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeltaDNA.Ads.UnityPlayer {

    #if UNITY_EDITOR
    internal class AdAgent : AdListener {

        internal readonly bool rewarded;
        private readonly IList<AdAdapter> adapters;
        private readonly int adMaxPerSession;
        private readonly int maxPerNetwork;
        
        private AdAdapter adapter;

        private string adPoint;
        private State state;

        internal long lastShowTime;
        internal int shownCount;

        internal AdAgent(
            bool rewarded,
            int count,
            int adMaxPerSession,
            int maxPerNetwork) {

            this.rewarded = rewarded;
            adapters = Enumerable
                .Range(0, count)
                .Select(e => new AdAdapter(rewarded, e))
                .ToList();
            this.adMaxPerSession = adMaxPerSession;
            this.maxPerNetwork = maxPerNetwork;

            adapter = (adapters.Count > 0) ? adapters.First() : null;
            state = State.READY;
        }

        internal void SetAdPoint(string adPoint) {
            this.adPoint = adPoint;
        }

        internal bool IsAdLoaded() {
            return state == State.LOADED;
        }

        internal void RequestAd() {
            if (adapter == null) {
                Debug.Log("Ignoring ad request due to no providers");
                return;
            } else if (HasReachedAdLimit()) {
                Debug.Log("Ignoring ad request due to session limit");
                return;
            } else if (state != State.READY) {
                Debug.Log("Ignoring ad request due to an existing request in progress");
                return;
            }

            state = State.LOADING;
            adapter.Load(this);
        }

        internal void ShowAd(string adPoint) {
            this.adPoint = adPoint;

            if (state == State.LOADED) {
                adapter.Show(this);
            }
        }

        public void OnLoaded(AdAdapter adapter) {
            if (state == State.LOADING) {
                state = State.LOADED;
            }
        }

        public void OnShowing(AdAdapter adapter) {
            if (state == State.LOADED) {
                state = State.SHOWING;
                
                adapter.shownCount++;

                if (!rewarded) {
                    SmartAds.Instance.DidOpenInterstitialAd();
                } else {
                    SmartAds.Instance.DidOpenRewardedAd();
                }
            }
        }

        public void OnClosed(AdAdapter adapter, bool complete) {
            if (state == State.SHOWING) {
                lastShowTime = DateTime.UtcNow.Ticks;
                state = State.READY;

                if (!rewarded) {
                    SmartAds.Instance.DidCloseInterstitialAd();
                } else {
                    SmartAds.Instance.DidCloseRewardedAd(
                        "{\"reward\":" + (complete ? "true" : "false") + "}");
                }

                ChangeToNextAdapter();
                RequestAd();
            }
        }

        private void ChangeToNextAdapter() {
            var index = adapters.IndexOf(adapter);
            if (index == adapters.Count - 1) {
                adapter = adapters.First();
            } else {
                adapter = adapters.ElementAt(index + 1);
            }
        }
        
        private bool HasReachedAdLimit() {
            return adMaxPerSession != -1 && shownCount >= adMaxPerSession;
        }

        private enum State {
            READY,
            LOADING,
            LOADED,
            SHOWING
        }
    }
    #endif
}
