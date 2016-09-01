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

namespace DeltaDNAAds.Editor {
    public sealed class NetworksWindow : EditorWindow {
        
        private const int WIDTH_LABEL = 100;
        private const int WIDTH_TOGGLE = 60;
        private const string DEFINITIONS = MenuItems.EDITOR_PATH + "networks.json";
        private const string NAME = "name";
        
        private readonly IList<object> networks;
        
        private readonly Networks[] handlers = new Networks[] {
            new AndroidNetworks(false, true),
            new IosNetworks()
        };
        private readonly IDictionary<Networks, SortedDictionary<string, bool>> enabled =
            new Dictionary<Networks, SortedDictionary<string, bool>>();
        
        public NetworksWindow() : base() {
            networks = Json.Deserialize(File.ReadAllText(DEFINITIONS)) as IList<object>;
            
            foreach (var handler in handlers) {
                enabled[handler] = new SortedDictionary<string, bool>();
                
                var persisted = handler.GetPersisted();
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
            
            var style = new GUIStyle();
            style.wordWrap = true;
            style.margin = new RectOffset(5, 5, 5, 5);
            GUILayout.Label(
                "Select the ad networks to use for the game.\n" +
                "\n" +
                "After making changes press 'Apply', " +
                "which will update the build scripts for Android and iOS. " +
                "These should be committed to your version control system.\n" +
                "\n" +
                "For Android the ad network libraries will be downloaded automatically, " +
                "however for iOS the project will need to be built from Unity in order " +
                "for CocoaPods to download the dependencies and export it as an Xcode project.",
                style);
            GUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Network", GUILayout.Width(WIDTH_LABEL));
            
            foreach (var handler in handlers) {
                GUILayout.Label(
                    handler.platformVisible,
                    GUILayout.Width(WIDTH_TOGGLE));
            }
            
            GUILayout.EndHorizontal();
            
            foreach (IDictionary<string, object> network in networks) {
                GUILayout.BeginHorizontal();
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
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(20);
            if (GUILayout.Button("Apply")) Apply();
            
            GUILayout.EndVertical();
        }
        
        private IList<string> getEnabled(Networks handler) {
            return enabled[handler]
                .Where(e => e.Value == true)
                .Select(e => e.Key)
                .ToArray();
        }
        
        private void Apply() {
            foreach (var handler in handlers) {
                var items = getEnabled(handler);
                if (!handler.GetPersisted().SequenceEqual(items)
                    || handler.AreDownloadsStale())
                    handler.ApplyChanges(items);
            }
        }
    }
}
