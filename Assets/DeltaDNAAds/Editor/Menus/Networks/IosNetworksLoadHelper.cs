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
    
    /// <summary>
    /// Refreshes the iOS dependencies configuration once the Editor is loaded
    /// and ready, as retrieving the version cannot be done outside of the
    /// Editor's lifecycle. It also makes sure to keep the configuration in
    /// sync with the settings for the application.
    /// </summary>
    [InitializeOnLoad]
    internal sealed class IosNetworksLoadHelper : ScriptableObject {
        
        static IosNetworksLoadHelper() {
            EditorApplication.update += Update;
        }

        private static bool refresh;
        private static string version;

        static void Update() {
            if (refresh) {
                var instance = new IosNetworks();
                instance.ApplyChanges(instance.GetPersisted());

                refresh = false;
            }
            
            #if UNITY_5_5_OR_NEWER
            var newVersion = PlayerSettings.iOS.targetOSVersionString.ToString();
            #else
            var newVersion = PlayerSettings.iOS.targetOSVersionString.ToString().Substring(4).Replace('_', '.');
            #endif
            if (!newVersion.Equals(version)) {
                refresh = true;
                version = newVersion;
            }
        }
    }
}
