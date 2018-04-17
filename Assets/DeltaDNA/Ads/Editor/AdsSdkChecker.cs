//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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

using DeltaDNA.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace DeltaDNA.Ads.Editor {

    [InitializeOnLoad]
    sealed class AdsSdkChecker : SdkChecker {

        static AdsSdkChecker() {
            new AdsSdkChecker().Register();
        }

        protected override void PerformCheck(IList<DDNATuple<string, Severity>> problems) {
            if (Directory.Exists("Assets/DeltaDNAAds")) {
                problems.Add(DDNATuple.New(
                    "[SmartAds] Assets/DeltaDNAAds should be removed as it has been replaced by Assets/DeltaDNA/Ads",
                    Severity.WARNING));
            }

            if (File.Exists("Assets/DeltaDNAAds/Editor/Menus/Networks/IosNetworksLoadHelper.cs")) {
                problems.Add(DDNATuple.New(
                    "[SmartAds] IosNetworksLoadHelper.cs should be deleted from the project in Assets/DeltaDNAAds/Editor/Menus/Networks.",
                    Severity.WARNING));
            }
            
            if (new AndroidNetworks().AreDownloadsStale()) {
                problems.Add(DDNATuple.New(
                    "[SmartAds] Android libraries are stale, please update them from the Editor menu.",
                    Severity.WARNING));
            }
        }
    }
}
