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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA.Editor {
    public sealed class MenuItems : MonoBehaviour {

        internal const string MENU_PATH = "DeltaDNA/Notifications/Android/";
        internal const string EDITOR_PATH = "Assets/DeltaDNA/Editor/";
        
        internal static string AndroidSdkLocation {
            get { return EditorPrefs.GetString("AndroidSdkRoot"); }
        }

        [MenuItem(MENU_PATH + "Configure", priority = 3)]
        public static void Configure() {
            System.Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType(
                "UnityEditor.InspectorWindow");

            var foundInspector = false;
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
                if (window.GetType() == inspectorType) {
                    foundInspector = true;
                    break;
                }
            }

            if (foundInspector) {
                EditorWindow.GetWindow<NotificationsWindow>(
                    "Notifications",
                    true,
                    inspectorType)
                    .Show();
            } else {
                EditorWindow.GetWindow<NotificationsWindow>(
                    "Notifications",
                    true)
                    .Show();
            }
        }

        internal static bool AreAndroidNotificationsInProject() {
            return File.Exists("Assets/Plugins/Android/deltadna-sdk-unity-notifications")
                && File.Exists("Assets/DeltaDNA/Plugins/Android");
        }
    }
}
