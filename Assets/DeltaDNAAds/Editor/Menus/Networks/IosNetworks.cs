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

namespace DeltaDNAAds.Editor {
    internal sealed class IosNetworks : Networks {
        
        private const string POD_FILE = MenuItems.EDITOR_PATH + "iOS/Podfile";
        
        public IosNetworks() : base("ios", "iOS") {}

        internal override IList<string> GetPersisted() {
            return File.ReadAllLines(POD_FILE)
                .Where(e => e.StartsWith("    pod 'DeltaDNAAds/"))
                .Select(e => e.Substring(e.IndexOf("/") + 1).Replace("', version", ""))
                .ToList();
        }
        
        internal override void ApplyChanges(IList<string> enabled) {
            var podSpecs = new List<string>(File.ReadAllLines(POD_FILE));
            
            podSpecs.RemoveAll(e => e.StartsWith("    pod 'DeltaDNAAds/"));
            podSpecs.InsertRange(
                podSpecs.FindIndex(e => e.Equals("target 'Unity-iPhone' do")) + 1,
                enabled
                    .Select(e => string.Format("    pod 'DeltaDNAAds/{0}', version", e))
                    .AsEnumerable());
            
            File.WriteAllLines(POD_FILE, podSpecs.ToArray());
            
            UnityEngine.Debug.Log(string.Format(
                "Changes have been applied to {0}, please commit the updates to version control",
                POD_FILE));
        }

        internal override bool AreDownloadsStale() {
            return false;
        }
    }
}
