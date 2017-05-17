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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeltaDNAAds {
    internal class Adapters {

        internal readonly IList<AdapterWrapper> adapters = new List<AdapterWrapper>();

        internal void Populate() {
            adapters.Add(new AdapterWrapper(
                "AdColony",
                "com.deltadna.android.sdk.ads.provider.adcolony.AdColonyAdapter",
                null,
                new object[] {
                    "appc804f742b8064114a9",
                    "vzbb9fa7accb4e4185b7"
                }));
            adapters.Add(new AdapterWrapper(
                "AppLovin",
                "com.deltadna.android.sdk.ads.provider.applovin.AppLovinRewardedAdapter",
                null,
                new object[] {
                    "ElG63iTpOQfZvG4kizCGhhXZQiWt37hIszOvfyi3MNdFdh-KeAbKt7vHrQ9uXrBNpZHTV-WtL87-r6IUGvp80h",
                    "Interstitial",
                    true
                }));
        }

        private interface Adapter {
            void RequestAd(Listener listener);
            void ShowAd();
        }

        internal class AdapterWrapper : ScriptableObject, Adapter {

            private readonly string adapterName;
            private readonly Adapter delegated;

            internal AdapterWrapper(
                string name,
                string android,
                string ios,
                object[] args) {

                adapterName = name;

                #if UNITY_ANDROID
                delegated = new AndroidAdapter(android, args);
                #elif UNITY_IOS
                delegated = null;
                #endif
            }

            public string GetName() {
                return adapterName;
            }

            public void RequestAd(Listener listener) {
                delegated.RequestAd(listener);
            }

            public void ShowAd() {
                delegated.ShowAd();
            }
        }

        private class AndroidAdapter : Adapter {

            private readonly AndroidJavaObject native;

            public AndroidAdapter(string className, params object[] args) {
                native = new AndroidJavaObject(
                    className,
                    new object[] { 0, 0, 0 }.Concat(args).ToArray());
            }

            public void RequestAd(Listener listener) {
                native.Call("requestAd", new object[] {
                    new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                        .GetStatic<AndroidJavaObject>("currentActivity"),
                    new AndroidListener(listener),
                    new AndroidJavaObject("org.json.JSONObject")
                });
            }

            public void ShowAd() {
                native.Call("showAd");
            }
        }

        internal interface Listener {

            void OnAdLoaded();
            void OnAdFailedToLoad(string result, string reason);
            void OnAdShowing();
            void OnAdFailedToShow(string result);
            void OnAdClicked();
            void OnAdLeftApplication();
            void OnAdClosed(bool complete);
        }

        private class AndroidListener : AndroidJavaProxy {

            private readonly Listener delegated;

            internal AndroidListener(Listener delegated)
                : base("com.deltadna.android.sdk.ads.bindings.MediationListener") {

                this.delegated = delegated;
            }

            void onAdLoaded(AndroidJavaObject adapter) {
                delegated.OnAdLoaded();
            }

            void onAdFailedToLoad(AndroidJavaObject adapter, AndroidJavaObject result, string reason) {
                delegated.OnAdFailedToLoad(result.Call<string>("toString"), reason);
            }

            void onAdShowing(AndroidJavaObject adapter) {
                delegated.OnAdShowing();
            }

            void onAdFailedToShow(AndroidJavaObject adapter, AndroidJavaObject result) {
                delegated.OnAdFailedToShow(result.Call<string>("toString"));
            }

            void onAdClicked(AndroidJavaObject adapter) {
                delegated.OnAdClicked();
            }

            void onAdLeftApplication(AndroidJavaObject adapter) {
                delegated.OnAdLeftApplication();
            }

            void onAdClosed(AndroidJavaObject adapter, bool complete) {
                delegated.OnAdClosed(complete);
            }
        }
    }
}
