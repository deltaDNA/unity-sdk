
//
// Copyright (c) 2018 deltaDNA Ltd. All rights reserved.
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
        
        static MenuItems() {
            if (File.Exists("Assets/DeltaDNA/Editor/Android/Menus/MenuItems.cs")) {
                File.Delete("Assets/DeltaDNA/Editor/Android/Menus/MenuItems.cs");
            }
        }
        
        internal const string MENU_PATH = "DeltaDNA";
        internal const string ASSETS_PATH = "Assets/DeltaDNA/";
        
        internal static string AndroidSdkLocation {
            get { return EditorPrefs.GetString("AndroidSdkRoot"); }
        }
        
        [MenuItem(MENU_PATH + "/Configure", priority = 1)]
        public static void ConfigureSdk() {
            WindowHelper.Show<ConfigurationWindow>("Configuration");
        }
     
        #if UNITY_2017_1_OR_NEWER
        [MenuItem(MENU_PATH + "/Event Definitions...", priority = 2)]
        public static void EventDefinitions() {
            WindowHelper.Show<EventsWindow>("Events");
        }
        #endif
        [MenuItem(MENU_PATH + "/Run Health Check", priority = 3)]
        public static void CheckSdk() {
            SdkChecker.Run();
        }
        
    }
}
