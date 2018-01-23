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

using UnityEditor;
using UnityEngine;

namespace DeltaDNAAds.Editor {
    
    [InitializeOnLoad]
    internal sealed class InitialisationHelper : ScriptableObject {
        
        static InitialisationHelper() {
            EditorApplication.update += Update;
        }

        private static bool isDevelopment;
        private static bool isDebugNotifications;
        private static string iosMinTargetVersion;

        static void Update() {
            bool refresh = false;

            if (EditorUserBuildSettings.development != isDevelopment) {
                isDevelopment = EditorUserBuildSettings.development;
                refresh = true;
            }

            if (EditorPrefs.GetBool(NetworksWindow.PREFS_DEBUG) != isDebugNotifications) {
                isDebugNotifications = EditorPrefs.GetBool(NetworksWindow.PREFS_DEBUG);
                refresh = true;
            }
            
            #if UNITY_5_5_OR_NEWER
            var newVersion = PlayerSettings.iOS.targetOSVersionString.ToString();
            #elif UNITY_5_OR_NEWER
            var newVersion = PlayerSettings.iOS.targetOSVersionString.ToString().Substring(4).Replace('_', '.');
            #else
            var newVersion = PlayerSettings.iOS.targetOSVersion.ToString().Substring(4).Replace('_', '.');
            #endif
            if (!newVersion.Equals(iosMinTargetVersion)) {
                iosMinTargetVersion = newVersion;
                refresh = true;
            }

            if (refresh) {
                Networks instance = new AndroidNetworks(true);
                instance.ApplyChanges(instance.GetPersisted());

                instance = new IosNetworks();
                instance.ApplyChanges(instance.GetPersisted());
            }
        }

        internal static bool IsDevelopment() {
            return isDevelopment;
        }

        internal static bool IsDebugNotifications() {
            return isDebugNotifications;
        }
        
        internal static string IosMinTargetVersion() {
            return iosMinTargetVersion;
        }
    }
}
