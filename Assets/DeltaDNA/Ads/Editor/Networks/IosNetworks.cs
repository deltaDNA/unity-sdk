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
using System.Linq;
using System.Xml.Linq;

namespace DeltaDNA.Ads.Editor {
    
    internal sealed class IosNetworks : Networks {
        
        private const string VERSION = "~> 1.10.1";
        private const string VERSION_DEBUG = "~> 1.0.0";
        
        private readonly object[] sources = new object[] {
            new XElement(
                "source",
                "https://github.com/deltaDNA/CocoaPods.git"),
            new XElement(
                "source",
                "https://github.com/CocoaPods/Specs.git")
        };
        
        public IosNetworks() : base("ios", "iOS") {}
        
        internal override bool IsEnabled() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("iosPod")
                    .Where(e => e
                        .Attribute("name")
                        .Value
                        .Equals("DeltaDNAAds/SmartAds"))
                    .Any();
            }
        }
        
        internal override IList<string> GetNetworks() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("iosPod")
                    .Select(e => e.Attribute("name").Value)
                    .Where(e => (e.StartsWith("DeltaDNAAds/") && !e.Equals("DeltaDNAAds/SmartAds")))
                    .Select(e => e.Substring("DeltaDNAAds/".Length))
                    .ToList();
            }
        }
        
        internal override bool AreDebugNotificationsEnabled() {
            lock (LOCK) {
                return Configuration()
                    .Descendants("iosPod")
                    .Where(e => e
                        .Attribute("name")
                        .Value
                        .Equals("DeltaDNADebug"))
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
                    .Descendants("iosPod")
                    .Remove();
                
                if (enabled) {
                    var packages = config.Descendants("iosPods").First();
                    packages.Add(new XElement(
                        "iosPod",
                        new object[] {
                            new XAttribute(
                                "name",
                                "DeltaDNAAds/SmartAds"),
                            new XAttribute(
                                "version",
                                VERSION),
                            new XAttribute(
                                "bitcodeEnabled",
                                "true"),
                            new XAttribute(
                                "minTargetSdk",
                                InitialisationHelper.IosMinTargetVersion()),
                            new XElement("sources", sources)
                        }));
                    
                    foreach (var network in networks) {
                        packages.Add(new XElement(
                            "iosPod",
                            new object[] {
                                new XAttribute(
                                    "name",
                                    "DeltaDNAAds/" + network),
                                new XAttribute(
                                    "version",
                                    VERSION),
                                new XAttribute(
                                    "bitcodeEnabled",
                                    "true"),
                                new XAttribute(
                                    "minTargetSdk",
                                    InitialisationHelper.IosMinTargetVersion()),
                                new XElement("sources", sources)
                            }));
                    }
                    
                    if (debugNotifications) {
                        packages.Add(new XElement(
                            "iosPod",
                            new object[] {
                                new XAttribute(
                                    "name",
                                    "DeltaDNADebug"),
                                new XAttribute(
                                    "version",
                                    VERSION_DEBUG),
                                new XAttribute(
                                    "bitcodeEnabled",
                                    "true"),
                                new XAttribute(
                                    "minTargetSdk",
                                    InitialisationHelper.IosMinTargetVersion()),
                                new XElement("sources", sources)
                            }));
                    }
                }
                
                config.Save(CONFIG);
            }
        }
        
        internal override bool AreDownloadsStale() {
            return false;
        }
    }
}
