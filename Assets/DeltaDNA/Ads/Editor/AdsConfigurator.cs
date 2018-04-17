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

using DeltaDNA.MiniJSON;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaDNA.Ads.Editor {
    
    internal sealed class AdsConfigurator {
        
        private const string DEFINITIONS = "Assets/DeltaDNA/Ads/Editor/networks.json";
        internal const string NAME = "name";
        internal const string INTEGRATION = "integration";
        internal const string TYPE = "type";
        
        internal readonly IList<object> networks;
        internal bool on;
        internal bool debugNotifications;
        
        internal readonly Networks[] handlers = new Networks[] {
            new AndroidNetworks(),
            new IosNetworks()
        };
        internal readonly IDictionary<Networks, SortedDictionary<string, bool>> enabled =
            new Dictionary<Networks, SortedDictionary<string, bool>>();
        
        internal AdsConfigurator() {
            networks = Json.Deserialize(File.ReadAllText(DEFINITIONS)) as IList<object>;
            
            on = handlers.Select(e => e.IsEnabled()).Aggregate((acc, e) => acc && e);
            debugNotifications = on && handlers
                .Select(e => e.AreDebugNotificationsEnabled())
                .Aggregate((acc, e) => acc && e);
            
            foreach (var handler in handlers) {
                enabled[handler] = new SortedDictionary<string, bool>();
                
                var persisted = handler.GetNetworks();
                foreach (IDictionary<string, object> network in networks) {
                    var value = network[handler.platform] as string;
                    if (value != null) {
                        enabled[handler][value] = persisted.Contains(value) || false;
                    }
                }
            }
        }
        
        internal bool On { 
            get { return this.on; }
            set {
                foreach (var handler in handlers) {
                    var anyEnabled = handler.GetNetworks().Count > 0;
                    if (!anyEnabled && !on && value) {
                        foreach (IDictionary<string, object> network in networks) {
                            var platformName = network[handler.platform] as string;
                            if (platformName != null) {
                                enabled[handler][platformName] = network["default"] as bool? ?? false;
                            }
                        }
                    }
                }
                this.on = value; 
            }
        }
         
        private IList<string> getEnabled(Networks handler) {
            return enabled[handler]
                .Where(e => e.Value == true)
                .Select(e => e.Key)
                .ToArray();
        }
        
        internal void Apply() {
            if (on) {
                DefineSymbolsHelper.Add(DefineSymbolsHelper.SMARTADS);
            } else {
                DefineSymbolsHelper.Remove(DefineSymbolsHelper.SMARTADS);
            }
            
            if (debugNotifications) {
                DefineSymbolsHelper.Add(DefineSymbolsHelper.DEBUG_NOTIFICATIONS);
            } else {
                DefineSymbolsHelper.Remove(DefineSymbolsHelper.DEBUG_NOTIFICATIONS);
            }
            
            foreach (var handler in handlers) {
                var networks = getEnabled(handler);
                
                if (handler.IsEnabled() != on
                    || !handler.GetNetworks().SequenceEqual(networks)
                    || handler.AreDebugNotificationsEnabled() != debugNotifications
                    || handler.AreDownloadsStale()) {
                    handler.ApplyChanges(on, networks, debugNotifications);
                }
            }
        }
    }
}
