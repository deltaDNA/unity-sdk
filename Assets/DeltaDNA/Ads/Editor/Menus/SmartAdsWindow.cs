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

using DeltaDNA.MiniJSON;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaDNA.Ads.Editor {
    public sealed class SmartAdsWindow : EditorWindow {
        
        internal const string PREFS_DEBUG = "DeltaDNA.Ads.Debug";

        private const int WIDTH_LABEL = 100;
        private const int WIDTH_TOGGLE = 60;
        private const int WIDTH_TOGGLE_SMALL = 20;
        private const int WIDTH_BUTTON = 80;
        private const int HEIGHT_SEPARATOR = 20;

        private const string DEFINITIONS = MenuItems.EDITOR_PATH + "networks.json";
        private const string NAME = "name";
        
        private readonly IList<object> networks;
        private bool on;
        private bool debugNotifications;
        
        private readonly Networks[] handlers = new Networks[] {
            new AndroidNetworks(true),
            new IosNetworks()
        };
        private readonly IDictionary<Networks, SortedDictionary<string, bool>> enabled =
            new Dictionary<Networks, SortedDictionary<string, bool>>();
        
        public SmartAdsWindow() : base() {
            networks = Json.Deserialize(File.ReadAllText(DEFINITIONS)) as IList<object>;
            
            on = handlers.Select(e => e.IsEnabled()).Aggregate((acc, e) => acc && e);
            debugNotifications = InitialisationHelper.IsDebugNotifications();
            
            foreach (var handler in handlers) {
                enabled[handler] = new SortedDictionary<string, bool>();
                
                var persisted = handler.GetNetworks();
                foreach (IDictionary<string, object> network in networks) {
                    var value = network[handler.platform] as string;
                    if (value != null) {
                        enabled[handler][value] = persisted.Contains(value) || false;
                    }
                }
            }
        }
        
        void OnGUI() {
            GUILayout.BeginVertical();

            GUILayout.Space(5);
            on = GUILayout.Toggle(on, "Enable SmartAds");

            EditorGUI.BeginDisabledGroup(!on);
            GUILayout.Space(HEIGHT_SEPARATOR);

            var style = new GUIStyle();
            style.wordWrap = true;
            style.margin = new RectOffset(5, 5, 0, 0);
            GUILayout.Label(
                "Select the ad networks to build into the game.\n" +
                "\n" +
                "After making changes press 'Apply', " +
                "which will update the build scripts for Android and iOS. " +
                "These should be committed to your version control system.\n" +
                "\n" +
                "For Android the ad network libraries will be downloaded automatically, " +
                "however for iOS the project will need to be built from Unity in order " +
                "for CocoaPods to download the dependencies and export it as an Xcode project.\n" +
                "\n" +
                "Most networks work out of the box [A], to use others that require additional integration, " +
                "contact support@deltadna.com for more details.\n" +
                "\n" +
                "The networks are labelled as supporting interstitial type ads [I] and/or rewarded type ads [R].",
                style);

            GUILayout.Space(HEIGHT_SEPARATOR);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("A", GUILayout.Width(WIDTH_TOGGLE_SMALL));
            GUILayout.Label("Network", GUILayout.Width(WIDTH_LABEL));
            
            foreach (var handler in handlers) {
                GUILayout.Label(
                    handler.platformVisible,
                    GUILayout.Width(WIDTH_TOGGLE));
            }
            
            GUILayout.Label("I", GUILayout.Width(WIDTH_TOGGLE_SMALL));
            GUILayout.Label("R", GUILayout.Width(WIDTH_TOGGLE));
            GUILayout.EndHorizontal();
            
            foreach (IDictionary<string, object> network in networks) {
                GUILayout.BeginHorizontal();
                
                var integration = network["integration"] as string;
                if (integration != null && integration == "manual") {
                    GUILayout.Label("", GUILayout.Width(WIDTH_TOGGLE_SMALL));
                } else {
                    GUILayout.Label('\u25CF'.ToString(), GUILayout.Width(WIDTH_TOGGLE_SMALL));
                }
                
                GUILayout.Label(
                    network[NAME] as string,
                    GUILayout.Width(WIDTH_LABEL));
                
                foreach (var handler in handlers) {
                    var value = network[handler.platform] as string;
                    if (value == null) {
                        // empty label to fill the space
                        GUILayout.Label("", GUILayout.Width(WIDTH_TOGGLE));
                        continue;
                    }
                    
                    enabled[handler][value] = GUILayout.Toggle(
                        enabled[handler].ContainsKey(value)
                            ? enabled[handler][value]
                            : true,
                        "",
                        GUILayout.Width(WIDTH_TOGGLE));
                }

                bool interstitialFound = false;
                bool rewardedFound = false;
                var adTypes = network["type"] as IList<object>;
                foreach (string adType in adTypes) {
                    if (adType == "interstitial") interstitialFound = true;
                    if (adType == "rewarded") rewardedFound = true;
                }

                if (interstitialFound) {
                    GUILayout.Label('\u25CF'.ToString(), GUILayout.Width(WIDTH_TOGGLE_SMALL));
                } else {
                    GUILayout.Label("", GUILayout.Width(WIDTH_TOGGLE_SMALL));
                }

                if (rewardedFound) {
                    GUILayout.Label('\u25CF'.ToString(), GUILayout.Width(WIDTH_TOGGLE));
                } else {
                    GUILayout.Label("", GUILayout.Width(WIDTH_TOGGLE));
                }
                
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(HEIGHT_SEPARATOR);
            EditorGUI.BeginDisabledGroup(!InitialisationHelper.IsDevelopment() || !on);
            debugNotifications = GUILayout.Toggle(debugNotifications, "Enable debug notifications in development builds");
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(HEIGHT_SEPARATOR);

            EditorGUI.EndDisabledGroup();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(WIDTH_BUTTON))) Apply();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        
        private IList<string> getEnabled(Networks handler) {
            return enabled[handler]
                .Where(e => e.Value == true)
                .Select(e => e.Key)
                .ToArray();
        }
        
        internal void Apply() {
            if (on) {
                DefineSymbolsHelper.Add(DefineSymbolsHelper.SMARTADS);
            } else {
                DefineSymbolsHelper.Remove(DefineSymbolsHelper.SMARTADS);
            }
            
            if (debugNotifications) {
                DefineSymbolsHelper.Add(DefineSymbolsHelper.DEBUG_NOTIFICATIONS);
            } else {
                DefineSymbolsHelper.Remove(DefineSymbolsHelper.DEBUG_NOTIFICATIONS);
            }
            EditorPrefs.SetBool(PREFS_DEBUG, debugNotifications);
            
            foreach (var handler in handlers) {
                var networks = getEnabled(handler);

                if (handler.IsEnabled() != on
                    || !handler.GetNetworks().SequenceEqual(networks)
                    || handler.AreDownloadsStale()) {
                    handler.ApplyChanges(on, networks);
                }
            }
            
            Debug.Log(string.Format(
                "Changes have been applied to {0}, please commit the updates to version control",
                Networks.CONFIG));
        }
    }
}
