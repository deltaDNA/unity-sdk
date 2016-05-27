////
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

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Diagnostics;

/**
 *  Add support for CocoaPods to the Unity generated XCode project.  Run pods.command from the
 *  build folder to install ad network dependencies.
 */
public class PostProcessBuild {

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {
        UnityEngine.Debug.Log("OnPostprocessBuild "+buildTarget+" "+path);

        if (buildTarget == BuildTarget.iOS) {

            string projectPath = PBXProject.GetPBXProjectPath(path);

            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            UnityEngine.Debug.Log(PBXProject.GetUnityTargetName());
            string target = project.TargetGuidByName(PBXProject.GetUnityTargetName());

            UnityEngine.Debug.Log(target);

            // Add build settings for CocoaPods
            project.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_CFLAGS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");

            // Disable Bitcode
            project.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            // Copy Podfile into project
            string unityProjectRootPath = Path.GetFullPath("./").Normalize();
            UnityEngine.Debug.Log(unityProjectRootPath);

            // Default src location is the project root
            FileUtil.ReplaceFile("Assets/DeltaDNAAds/Editor/iOS/Podfile", path + "/Podfile");

            // Update the XCode project on disk
            project.WriteToFile(projectPath);

            // Run pod update
            Process proc = new Process();
            proc.StartInfo.FileName = "/usr/local/bin/pod";
            proc.StartInfo.Arguments = "install --project-directory=\""+path+"\"";
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
        }

    }
}
