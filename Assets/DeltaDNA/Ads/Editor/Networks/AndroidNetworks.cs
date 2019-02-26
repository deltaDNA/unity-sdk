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
using System.Xml.Linq;
using UnityEditor;

namespace DeltaDNA.Ads.Editor {
    
    [InitializeOnLoad]
    internal sealed class AndroidNetworks : Networks {
        
        static AndroidNetworks() {
            var instance = new AndroidNetworks();
            instance.ApplyChanges(
                instance.IsEnabled(),
                instance.GetNetworks(),
                instance.AreDebugNotificationsEnabled());
        }
        
        private static readonly string[] REPOS = {
            "https://deltadna.bintray.com/android"
        };
        private const string VERSION = "1.11.0";
        private const string PLUGINS_PATH = "Assets/Plugins/Android";
        
        public AndroidNetworks() : base("android", "Android") {}
        
        internal override bool IsEnabled() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("androidPackage")
                    .Where(e => e
                        .Attribute("spec")
                        .Value
                        .StartsWith("com.deltadna.android:deltadna-smartads-core:"))
                    .Any();
            }
        }
        
        internal override IList<string> GetNetworks() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("androidPackage")
                    .Select(e => e.Attribute("spec").Value)
                    .Where(e => e.StartsWith("com.deltadna.android:deltadna-smartads-provider-"))
                    .Select(e => {
                        var value = e.Substring(e.IndexOf("-provider-") + 10);
                        return value.Substring(0, value.LastIndexOf(':'));
                    })
                    .ToList();
            }
        }
        
        internal override bool AreDebugNotificationsEnabled() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("androidPackage")
                    .Where(e => e
                        .Attribute("spec")
                        .Value
                        .StartsWith("com.deltadna.android:deltadna-smartads-debug:"))
                    .Any();
            }
        }
        
        internal override void ApplyChanges(
            bool enabled,
            IList<string> networks,
            bool debugNotifications) {
            
            lock (LOCK) {
                var config = Configuration();
                
                config
                    .Descendants("androidPackage")
                    .Remove();
                
                if (enabled) {
                    var packages = config.Descendants("androidPackages").First();
                    packages.Add(new XElement(
                        "androidPackage",
                        new object[] {
                            new XAttribute(
                                "spec",
                                "com.deltadna.android:deltadna-smartads-core:" + VERSION),
                            new XElement(
                                "repositories",
                                REPOS.Select(e => new XElement("repository", e)))
                        }));
                    
                    foreach (var network in networks) {
                        var repos = REPOS.Select(e => new XElement("repository", e)).ToList();
                        if (network.Equals("hyprmx")) {
                            repos.Add(new XElement(
                                "repository",
                                "https://raw.githubusercontent.com/HyprMXMobile/Android-SDKs/master"));
                        } else if (network.Equals("ironsource")) {
                            repos.Add(new XElement(
                                "repository",
                                "https://dl.bintray.com/ironsource-mobile/android-sdk"));
                        } else if (network.Equals("mopub")) {
                            repos.Add(new XElement(
                                "repository",
                                "https://s3.amazonaws.com/moat-sdk-builds"));
                        } else if (network.Equals("tapjoy")) {
                            repos.Add(new XElement(
                                "repository",
                                "https://tapjoy.bintray.com/maven"));
                        }
                        
                        packages.Add(new XElement(
                            "androidPackage",
                            new object[] {
                                new XAttribute(
                                    "spec",
                                    "com.deltadna.android:deltadna-smartads-provider-" + network + ":" + VERSION),
                                new XElement(
                                    "repositories",
                                    repos.ToArray())
                            }));
                    }
                    
                    if (debugNotifications) {
                        packages.Add(new XElement(
                            "androidPackage",
                            new object[] {
                                new XAttribute(
                                    "spec",
                                    "com.deltadna.android:deltadna-smartads-debug:" + VERSION),
                                new XElement(
                                    "repositories",
                                    REPOS.Select(e => new XElement("repository", e)))
                            }));
                    }
                }
                
                config.Save(CONFIG);
            }
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
            
            foreach (var network in GetNetworks()) {
                if (!downloaded.Contains(string.Format(
                    "{0}-{1}.aar",
                    network,
                    VERSION))) return true;
            }
            
            return false;
        }
    }
}
