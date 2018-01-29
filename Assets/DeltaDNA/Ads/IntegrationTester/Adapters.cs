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

#if UNITY_5_OR_NEWER
using System.Collections.Generic;
#if UNITY_2017_1_OR_NEWER
using System;
#endif
using System.Linq;
using System.Xml;
using UnityEngine;

namespace DeltaDNA.Ads {
    internal class Adapters {

        internal readonly IList<AdapterWrapper> adapters = new List<AdapterWrapper>();

        internal void Populate() {
            var doc = new XmlDocument();
            doc.LoadXml((Resources.Load("networks", typeof(TextAsset)) as TextAsset).text);

            foreach (XmlNode node in doc.SelectNodes("adapters/adapter")) {
                adapters.Add(new AdapterWrapper(
                    node.Attributes.GetNamedItem("name").Value,
                    node.Attributes.GetNamedItem("android").Value,
                    node.Attributes.GetNamedItem("ios").Value,
                    node.SelectNodes("arguments/argument")
                        .Cast<XmlNode>()
                        .Select(e => {
                            switch (e.Attributes.GetNamedItem("type").Value) {
                                case "string":
                                    return e.InnerText;
                                case "bool":
                                    return bool.Parse(e.InnerText);
                                default:
                                    return e.InnerText as object;
                            }
                        })
                        .ToArray()));
            }
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
                if (!string.IsNullOrEmpty(android)) {
                    delegated = new AndroidAdapter(android, args);
                } else {
                    delegated = null;
                }
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
            private readonly AndroidJavaObject activity;

            public AndroidAdapter(string className, params object[] args) {
                try {
                native = new AndroidJavaObject(
                    className,
                    new object[] { 0, 0, 0 }.Concat(args).ToArray());
                } catch (AndroidJavaException e) {
                    Debug.LogException(e);
                    native = null;
                }

                activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity");
            }

            public void RequestAd(Listener listener) {
                if (native == null) return;

                #if UNITY_2017_1_OR_NEWER
                activity.Call("runOnUiThread", new Runnable(() => {
                #else
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                #endif
                    native.Call("requestAd", new object[] {
                        activity,
                        new AndroidListener(listener),
                        new AndroidJavaObject("org.json.JSONObject")
                    });
                }));
            }

            public void ShowAd() {
                if (native == null) return;

                #if UNITY_2017_1_OR_NEWER
                activity.Call("runOnUiThread", new Runnable(() => {
                #else
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                #endif
                    native.Call("showAd");
                }));
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
                delegated.OnAdFailedToLoad(result.Call<string>("name"), reason);
            }

            void onAdShowing(AndroidJavaObject adapter) {
                delegated.OnAdShowing();
            }

            void onAdFailedToShow(AndroidJavaObject adapter, AndroidJavaObject result) {
                delegated.OnAdFailedToShow(result.Call<string>("name"));
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

        #if UNITY_2017_1_OR_NEWER
        private class Runnable : AndroidJavaProxy {

            private readonly Action action;

            internal Runnable(Action action) : base("java.lang.Runnable") {
                this.action = action;
            }

            public void run() {
                action();
            }
        }
        #endif
    }
}
#endif
