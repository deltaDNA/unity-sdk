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

using UnityEngine;
#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace DeltaDNA.Editor
{
    public sealed class AddDefineSymbolsToXcodeProject : ScriptableObject
    {
        #if UNITY_IOS
        private const int BUILD_ORDER_ADD_DEFINES = 0; // after the UnityJarResolver runs pod install

        [PostProcessBuild(BUILD_ORDER_ADD_DEFINES)]
        public static void AddDefineSymbols(BuildTarget buildTarget, string buildPath)
        {
            PBXProject proj = new PBXProject();
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            proj.ReadFromFile(projPath);
            string target = proj.TargetGuidByName("Unity-iPhone");
            
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( EditorUserBuildSettings.selectedBuildTargetGroup );
            List<string> allDefines = definesString.Split ( ';' ).Where(i => i.Length > 0).Select(i => string.Format("{0}=1", i)).ToList ();
            allDefines.Add("$(inherited)");
            allDefines.Reverse();
            proj.UpdateBuildProperty(target, "GCC_PREPROCESSOR_DEFINITIONS", allDefines, new string [] {});
            proj.WriteToFile(projPath);
        }
        #endif
    }
} // namespace DeltaDNA.Editor
