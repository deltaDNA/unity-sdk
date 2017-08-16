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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DeltaDNAAds.Editor {
    internal sealed class AndroidNetworks : Networks {
        
        private const string CONFIG_SCRIPT = MenuItems.EDITOR_PATH + "Android/networks";
        private const string PLUGINS_PATH = "Assets/Plugins/Android";

        private readonly bool download;
        
        public AndroidNetworks(bool download) : base("android", "Android") {
            this.download = download;
        }

        internal override IList<string> GetPersisted() {
            return File.ReadAllLines(CONFIG_SCRIPT)
                .Where(e => !e.StartsWith("//"))
                .ToArray();
        }
        
        internal override void ApplyChanges(IList<string> enabled) {
            var buildScript = new List<string>(File.ReadAllLines(CONFIG_SCRIPT));
            
            buildScript.RemoveAll(e => !e.StartsWith("//"));
            buildScript.InsertRange(1, enabled.AsEnumerable());
            
            File.WriteAllLines(CONFIG_SCRIPT, buildScript.ToArray());
            
            UnityEngine.Debug.Log(string.Format(
                "Changes have been applied to {0}, please commit the updates to version control",
                CONFIG_SCRIPT));
            
            if (download) DownloadLibraries();
        }
        
        internal override bool AreDownloadsStale() {
            var downloaded = (!Directory.Exists(PLUGINS_PATH))
                ? Enumerable.Empty<string>()
                : Directory
                    .GetFiles(PLUGINS_PATH)
                    .Where(e => e.Contains("deltadna-smartads-provider-") && e.EndsWith(".aar"))
                    .Select(e => e.Substring(e.IndexOf("-provider-") + 10))
                    .Concat((!Directory.Exists(PLUGINS_PATH))
                        ? Enumerable.Empty<string>()
                        : Directory
                            .GetDirectories(PLUGINS_PATH)
                            .Where(e => e.Contains("deltadna-smartads-provider-"))
                            .Select(e => e.Substring(e.IndexOf("-provider-") + 10) + ".aar"));
            
            foreach (var network in GetPersisted()) {
                if (!downloaded.Contains(string.Format(
                    "{0}-{1}.aar",
                    network,
                    DeltaDNAAdsUnityJarResolverDependencies.VERSION))) return true;
            }
            
            return false;
        }
        
        internal static void DownloadLibraries() {
            #if UNITY_ANDROID
            DeltaDNAAdsUnityJarResolverDependencies.RegisterAndroidDependencies();
            DeltaDNAAdsUnityJarResolverDependencies.Resolve();
            #endif
        }
    }
}
