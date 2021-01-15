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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA.Editor {
    internal sealed class WindowHelper {
        
        internal const int WIDTH_LABEL = 145;
        internal const int WIDTH_BUTTON = 80;
        internal const int WIDTH_TOGGLE = 80;
        internal const int HEIGHT_SEPARATOR = 20;
        
        internal static void Show<T>(string title) where T : EditorWindow {
            System.Type inspectorType = typeof(UnityEditor.Editor)
                .Assembly
                .GetType("UnityEditor.InspectorWindow");
            
            var foundInspector = false;
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
                if (window.GetType() == inspectorType) {
                    foundInspector = true;
                    break;
                }
            }
            
            if (foundInspector) {
                EditorWindow.GetWindow<T>(
                    title,
                    true,
                    inspectorType)
                    .Show();
            } else {
                EditorWindow.GetWindow<T>(
                    title,
                    true)
                    .Show();
            }
        }

        internal static string FindFile(string searchPattern)
        {
            // Search for file in these folders.
            var searchFolders = new[] {
                "Packages/com.unity.deltadna.sdk",
                "Packages/com.unity.accelerate",
                "Assets",
                "Assets/DeltaDNA"
	    };
            string adaptersInfoPath = "";
            foreach (var folder in searchFolders)
            {
                try
                {
                    adaptersInfoPath = Directory.GetFiles(folder, searchPattern,
                        SearchOption.AllDirectories).FirstOrDefault();
                    if (adaptersInfoPath != null)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return adaptersInfoPath;
        }

        internal static string FindDir(string searchPattern)
        {
            // Search for file in these folders.
            var searchFolders = new[] {
                "Packages/com.unity.deltadna.sdk",
                "Packages/com.unity.accelerate",
                "Assets",
                "Assets/DeltaDNA"
	    };
            string adaptersInfoPath = "";
            foreach (var folder in searchFolders)
            {
                try
                {
                    adaptersInfoPath = Directory.GetDirectories(folder, searchPattern,
                        SearchOption.AllDirectories).FirstOrDefault();
                    if (adaptersInfoPath != null)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return adaptersInfoPath;
        }
    }
}
