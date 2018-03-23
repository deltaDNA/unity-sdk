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
using System.Xml.Linq;

namespace DeltaDNA.Ads.Editor {
    internal abstract class Networks {
        
        internal const string CONFIG = "Assets/DeltaDNA/Ads/Editor/Dependencies.xml";
        
        protected static readonly object LOCK = new object();
        
        internal readonly string platform;
        internal readonly string platformVisible;
        
        public Networks(string platform, string platformVisible) {
            this.platform = platform;
            this.platformVisible = platformVisible;
        }
        
        internal abstract bool IsEnabled();
        internal abstract IList<string> GetNetworks();
        internal abstract bool AreDebugNotificationsEnabled();
        internal abstract void ApplyChanges(
            bool enabled,
            IList<string> networks,
            bool debugNotifications);
        internal abstract bool AreDownloadsStale();
        
        internal static XDocument Configuration() {
            return XDocument.Parse(File.ReadAllText(CONFIG));
        }
    }
}
