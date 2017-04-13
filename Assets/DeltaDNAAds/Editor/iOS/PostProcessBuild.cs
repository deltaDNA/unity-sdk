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

#if UNITY_IOS && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

/**
 *  Add support for CocoaPods to the Unity generated XCode project.  Run pods.command from the
 *  build folder to install ad network dependencies.
 */
public class PostProcessBuild {

    // Big number to avoid conflicting with Unity's internal ads PostProcess
    [PostProcessBuild(100000)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {

        if (buildTarget == BuildTarget.iOS) {

            string projectPath = PBXProject.GetPBXProjectPath(path);

            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string target = project.TargetGuidByName(PBXProject.GetUnityTargetName());

            // Add build settings for CocoaPods
            project.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_CFLAGS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");

            // Disable Bitcode
            project.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            // Copy Podfile into project

            // Default src location is the project root
            FileUtil.ReplaceFile("Assets/DeltaDNAAds/Editor/iOS/Podfile", path + "/Podfile");

            // Set Podfile platform version to match Unity
            #if UNITY_5_5_OR_NEWER
            var targetOSVersion = PlayerSettings.iOS.targetOSVersionString;
            string iosPlatform = targetOSVersion.ToString();
            #else
            var targetOSVersion = PlayerSettings.iOS.targetOSVersion;
            string iosPlatform = targetOSVersion.ToString().Substring(4).Replace('_', '.');
            #endif
            
            var podfile = new List<string>(File.ReadAllLines(path + "/Podfile"));
            podfile = new List<string>(podfile.Select(e => e.StartsWith("platform") ? string.Format("platform :ios, '{0}'", iosPlatform) : e).AsEnumerable());
            File.WriteAllLines(path + "/Podfile", podfile.ToArray());

            // Update the XCode project on disk
            project.WriteToFile(projectPath);

            // Run pod update
            Process process = new Process();
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            EditorUtility.DisplayProgressBar(
                    "DeltaDNA",
                    "Please wait, downloading ad network libraries...",
                    (float) 0.5);

            process.StartInfo.FileName = "/usr/local/bin/pod";
            process.StartInfo.Arguments = "update --project-directory=\""+path+"\"";
            process.Start();
            process.WaitForExit();

            EditorUtility.ClearProgressBar();

            // pod writes error reason to standard out
            if (process.ExitCode != 0) {
                UnityEngine.Debug.LogError(string.Format(
                    "Failed to download ad network libraries due to: \n{0}\n{1}",
                    process.StandardOutput.ReadToEnd(),
                    process.StandardError.ReadToEnd()));
            } else {
                UnityEngine.Debug.Log("Successfully downloaded ad network libraries");
            }
        }
    }
}
#endif
