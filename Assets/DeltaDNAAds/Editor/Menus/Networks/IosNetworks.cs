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

namespace DeltaDNAAds.Editor {
    
    internal sealed class IosNetworks : Networks {

        private const string VERSION = "~> 1.6.0";

        public IosNetworks() : base("ios", "iOS") {}

        internal override IList<string> GetPersisted() {
            return Configuration()
                .Descendants("iosPod")
                .Select(e => e
                    .Attribute("name")
                    .Value
                    .Substring("DeltaDNAAds/".Length))
                .ToList();
        }

        internal override void ApplyChanges(IList<string> enabled) {
            var config = Configuration();

            config.Descendants("iosPod").Remove();

            var packages = config.Descendants("iosPods").First();
            foreach (var network in enabled) {
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
                        #if UNITY_5_5_OR_NEWER
                            PlayerSettings.iOS.targetOSVersionString.ToString()),
                        #elif UNITY_5_OR_NEWER
                            PlayerSettings.iOS.targetOSVersionString.ToString().Substring(4).Replace('_', '.')),
                        #else
                            PlayerSettings.iOS.targetOSVersion.ToString().Substring(4).Replace('_', '.')),
                        #endif
                        new XElement(
                            "sources",
                            new object[] {
                                new XElement(
                                    "source",
                                    "https://github.com/deltaDNA/CocoaPods.git"),
                                new XElement(
                                    "source",
                                    "https://github.com/CocoaPods/Specs.git")
                            })
                    }));
            }

            config.Save(CONFIG);
        }

        internal override bool AreDownloadsStale() {
            return false;
        }
    }
}
