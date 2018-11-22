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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA.Editor {
    
    [InitializeOnLoad]
    internal sealed class InitialisationHelper : ScriptableObject {
        
        static InitialisationHelper() {
            EditorApplication.update += Update;
        }
        
        private static bool iosPushNotificationsRemoved;
        
        static void Update() {
            bool refresh = false;

            var value = AreIosPushNotificationsRemoved();
            if (iosPushNotificationsRemoved != value) {
                iosPushNotificationsRemoved = value;
                refresh = true;
            }

            if (refresh) {
                if (iosPushNotificationsRemoved) {
                    DefineSymbolsHelper.Add(DefineSymbolsHelper.IOS_PUSH_NOTIFICATIONS_REMOVED);
                } else {
                    DefineSymbolsHelper.Remove(DefineSymbolsHelper.IOS_PUSH_NOTIFICATIONS_REMOVED);
                }
            }
        }

        private static bool AreIosPushNotificationsRemoved() {
            return !File.Exists("Assets/DeltaDNA/Editor/iOS/EnableNotificationsPostProcessBuild.cs")
                && !File.Exists("Assets/DeltaDNA/Notifications/IosNotifications.cs");
        }
    }
}
