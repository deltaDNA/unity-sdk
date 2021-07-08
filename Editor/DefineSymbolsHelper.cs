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

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DeltaDNA.Editor {
    
    internal class DefineSymbolsHelper {
        
        internal const string IOS_PUSH_NOTIFICATIONS_REMOVED = "DDNA_IOS_PUSH_NOTIFICATIONS_REMOVED";
        
        internal static void Add(string symbol) {
            var symbols = GetSymbols();
            if (!symbols.Remove(symbol)) {
                symbols.Add(symbol);
                SetSymbols(symbols);
            }
        }
        
        internal static void Remove(string symbol) {
            var symbols = GetSymbols();
            if (symbols.Remove(symbol)) {
                SetSymbols(symbols);
            }
        }
        
        private static List<string> GetSymbols() {
            return PlayerSettings
                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
                .Split(';')
                .ToList();
        }
        
        private static void SetSymbols(List<string> symbols) {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", symbols.ToArray()));
        }
    }
}
