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
using UnityEngine;

namespace DeltaDNA.Ads {
    public class IntegrationTester : MonoBehaviour {

        private Adapters adapters = new Adapters();
        private Adapters.Listener listener = new Listener();

        private Adapters.AdapterWrapper current;

        void Start() {
            adapters.Populate();
        }

        void FixedUpdate() {
            // Make our cube rotate
            transform.Rotate(new Vector3(-15, -30, -45) * Time.deltaTime);
        }

        void OnGUI() {
            if (GUI.Button(new Rect(220, 20, 200, 80), "Request Ad")) {
                if (current != null) {
                    Debug.Log("Requesting ad from " + current.GetName());
                    current.RequestAd(listener);
                }
            }

            if (GUI.Button(new Rect(220, 120, 200, 80), "Show Ad")) {
                if (current != null) {
                    Debug.Log("Showing ad from " + current.GetName());
                    current.ShowAd();
                }
            }

            var y = 20;
            foreach (Adapters.AdapterWrapper adapter in adapters.adapters) {
                if (GUI.Button(new Rect(10, y, 200, 80), adapter.GetName())) {
                    current = adapter;
                    Debug.Log("Changed to " + current.GetName());
                }

                y += 100;
            }
        }

        private class Listener : Adapters.Listener {

            public void OnAdClicked() {
                Debug.Log("OnAdClicked");
            }

            public void OnAdClosed(bool complete) {
                Debug.Log("OnAdClosed: " + complete);
            }

            public void OnAdFailedToLoad(string result, string reason) {
                Debug.Log("OnAdFailedToLoad: " + result + " " + reason);
            }

            public void OnAdFailedToShow(string result) {
                Debug.Log("OnAdFailedToShow: " + result);
            }

            public void OnAdLeftApplication() {
                Debug.Log("OnAdLeftApplication");
            }

            public void OnAdLoaded() {
                Debug.Log("OnAdLoaded");
            }

            public void OnAdShowing() {
                Debug.Log("OnAdShowing");
            }
        }
    }
}
#endif
