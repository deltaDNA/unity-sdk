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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeltaDNAAds.Editor {
    internal sealed class AndroidNetworks : Networks {
        
        private const string BUILD_SCRIPT = MenuItems.EDITOR_PATH + "Android/build.gradle";
        private const string GRADLEW_PATH = MenuItems.EDITOR_PATH + "Android/";
        private const string GRADLEW = "gradlew";
        private const string PLUGINS_PATH = "Assets/DeltaDNAAds/Plugins/Android/";
        
        private readonly bool quiet;
        private readonly bool download;
        
        public AndroidNetworks(bool quiet, bool download) : base("android", "Android") {
            this.quiet = quiet;
            this.download = download;
        }

        internal override IList<string> GetPersisted() {
            return File.ReadAllLines(BUILD_SCRIPT)
                .Where(e => e.StartsWith("    network('"))
                .Select(e => e.Substring(e.IndexOf("('") + 2).TrimEnd("')".ToCharArray()))
                .ToArray();
        }
        
        internal override void ApplyChanges(IList<string> enabled) {
            var buildScript = new List<string>(File.ReadAllLines(BUILD_SCRIPT));
            
            buildScript.RemoveAll(e => e.StartsWith("    network('"));
            buildScript.InsertRange(
                buildScript.FindIndex(e => e.Equals("dependencies {")) + 1,
                enabled
                    .Select(e => string.Format("    network('{0}')", e))
                    .AsEnumerable());
            
            File.WriteAllLines(BUILD_SCRIPT, buildScript.ToArray());
            
            UnityEngine.Debug.Log(string.Format(
                "Changes have been applied to {0}, please commit the updates to version control",
                BUILD_SCRIPT));
            
            if (download) DownloadLibraries(quiet);
        }
        
        internal override bool AreDownloadsStale() {
            var downloaded = (!Directory.Exists(PLUGINS_PATH))
                ? Enumerable.Empty<string>()
                : Directory
                .GetFiles(PLUGINS_PATH)
                .Where(e => e.Contains("-provider-") && e.EndsWith(".aar"))
                .Select(e => e.Substring(e.IndexOf("-provider-") + 10));
            var version = File
                .ReadAllLines(BUILD_SCRIPT)
                .Where(e => e.StartsWith("def version = '"))
                .Select(e => e.Substring(15).TrimEnd("'".ToCharArray()))
                .First();
            
            foreach (var network in GetPersisted()) {
                if (!downloaded.Contains(string.Format(
                    "{0}-{1}.aar",
                    network,
                    version))) return true;
            }
            
            return false;
        }
        
        internal static void DownloadLibraries(bool quiet) {
            if (string.IsNullOrEmpty(DeltaDNA.Editor.MenuItems.AndroidSdkLocation)) {
                UnityEngine.Debug.LogError(
                    "Failed to download Android libraries as the Android SDK location has not been set in the Unity configuration");
                return;
            }
            
            var process = new Process();
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            
            if (quiet) {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
            } else {
                #if !UNITY_EDITOR_OSX
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                #else
                EditorUtility.DisplayProgressBar(
                    "DeltaDNA",
                    "Please wait, downloading Android libraries...",
                    (float) 0.5);
                #endif
            }
            
            #if UNITY_EDITOR_WIN
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = string.Format(
                "/c cd \"{0}\" && >local.properties echo sdk.dir={1}&& {2} clean download -Psmartads",
                Application.dataPath + "/../" + GRADLEW_PATH,
                DeltaDNA.Editor.MenuItems.AndroidSdkLocation
                    .Replace(":/", "\\:\\\\")
                    .Replace("/", "\\\\"),
                GRADLEW + ".bat");
            #else
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = string.Format(
                "-c 'cd \"`pwd`\" && echo \"sdk.dir={0}\" > local.properties && ./{1} clean download -Psmartads'",
                DeltaDNA.Editor.MenuItems.AndroidSdkLocation,
                GRADLEW);
            process.StartInfo.WorkingDirectory = Application.dataPath + "/../" + GRADLEW_PATH;
            #endif
            
            process.Start();
            process.WaitForExit();
            
            #if UNITY_EDITOR_OSX
            EditorUtility.ClearProgressBar();
            #endif
            
            if (process.ExitCode != 0) {
                UnityEngine.Debug.LogError(string.Format(
                    "Failed to download Android libraries due to: {0}",
                    process.StandardError.ReadToEnd()));
            } else {
                UnityEngine.Debug.Log("Successfully downloaded Android libraries");
                
                if (Application.unityVersion.StartsWith("4.")) {
                    UnityEngine.Debug.LogWarning(string.Format(
                        "On Unity 4.x the downloaded libraries in {0} need to be moved to {1}.",
                        PLUGINS_PATH,
                        "Assets/Plugins/Android"));
                }
            }
        }
    }
}
