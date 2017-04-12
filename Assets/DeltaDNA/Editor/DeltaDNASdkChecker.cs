﻿//
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

namespace DeltaDNA.Editor {

    [InitializeOnLoad]
    sealed class DeltaDNASdkChecker : SdkChecker {

        static DeltaDNASdkChecker() {
            new DeltaDNASdkChecker().Register();
        }

        protected override void PerformCheck(IList<Tuple<string, Severity>> problems) {
            var androidLibs = MenuItems.ASSETS_PATH + "/Plugins/Android";
            if (Directory.Exists(androidLibs)) {
                var files = Directory.GetFiles(androidLibs, "*.aar");
                if (files.Length > 1) {
                    problems.Add(Tuple.New(
                        "[Notifications] Found multiple libraries in '" + androidLibs + "' folder. Please make sure it only contains the most recent 'android-sdk-notifications' AAR.  Also remove any play-services-*.aar and support-v4-*.aar, these are now handled by Google's Play Services Resolver.",
                        Severity.ERROR));
                }
            }

            var androidManifest = "Assets/Plugins/Android/AndroidManifest.xml";
            if (File.Exists(androidManifest)) {
                var manifest = XDocument.Parse(File.ReadAllText(androidManifest));

                if (manifest
                    .Descendants("permissions")
                    .Where(e => e.Attribute(NotificationsWindow.NAMESPACE_ANDROID + "name").Value.EndsWith("C2D_MESSAGE"))
                    .Count() > 0) {
                    problems.Add(Tuple.New(
                        "[Notifications] Found invalid C2D_MESSAGE 'permission' entry in '" + androidManifest + "'. This entry should be removed for Firebase notifications.",
                        Severity.ERROR));
                }
                if (manifest
                    .Descendants("uses-permission")
                    .Where(e => e.Attribute(NotificationsWindow.NAMESPACE_ANDROID + "name").Value.EndsWith("C2D_MESSAGE"))
                    .Count() > 0) {
                    problems.Add(Tuple.New(
                        "[Notifications] Found invalid C2D_MESSAGE 'uses-permission' entry in '" + androidManifest + "'. This entry should be removed for Firebase notifications.",
                        Severity.ERROR));
                }
                if (manifest
                    .Descendants("receiver")
                    .Where(e => e.Attribute(NotificationsWindow.NAMESPACE_ANDROID + "name").Value == "com.google.android.gms.gcm.GcmReceiver")
                    .Count() > 0) {
                    problems.Add(Tuple.New(
                        "[Notifications] Found invalid GcmReceiver 'receiver' entry in '" + androidManifest + "'. This entry should be removed for Firebase notifications.",
                        Severity.ERROR));
                }
                if (manifest
                    .Descendants("service")
                    .Where(e => e.Attribute(NotificationsWindow.NAMESPACE_ANDROID + "name").Value.StartsWith("com.deltadna.android.sdk.notifications"))
                    .Count() > 0) {
                    problems.Add(Tuple.New(
                        "[Notifications] Found invalid deltaDNA notification 'service' entry in '" + androidManifest + "'. This entry should be removed for Firebase notifications.",
                        Severity.ERROR));
                }

                manifest
                    .Descendants("meta-data")
                    .ToList()
                    .ForEach(e => {
                        switch (e.Attribute(NotificationsWindow.NAMESPACE_ANDROID + "name").Value) {
                            case NotificationsWindow.ATTR_ICON:
                                problems.Add(Tuple.New(
                                    "[Notifications] Found conflicting 'meta-data' entry for '" + NotificationsWindow.ATTR_ICON + "' in '" + androidManifest + "'. This entry should be removed and configured through the Editor menu instead.",
                                    Severity.ERROR));
                                break;

                            case NotificationsWindow.ATTR_TITLE:
                                problems.Add(Tuple.New(
                                    "[Notifications] Found conflicting 'meta-data' entry for '" + NotificationsWindow.ATTR_TITLE + "' in '" + androidManifest + "'. This entry should be removed and configured through the Editor menu instead.",
                                    Severity.ERROR));
                                break;

                            case "ddna_sender_id":
                                problems.Add(Tuple.New(
                                    "[Notifications] Found deprecated 'meta-data' entry for 'ddna_sender_id' in in '" + androidManifest + "'. This entry should be removed and configured through the Editor menu instead.",
                                    Severity.WARNING));
                                break;

                            case "ddna_start_launch_intent":
                                problems.Add(Tuple.New(
                                    "[Notifications] Found deprecated 'meta-data' entry for 'ddna_start_launch_intent' in in '" + androidManifest + "'. This entry should be removed.",
                                    Severity.WARNING));
                                break;
                        }
                });
            }

            if (File.Exists(NotificationsWindow.MANIFEST_XML_PATH)) {
                var manifest = XDocument.Parse(File.ReadAllText(NotificationsWindow.MANIFEST_XML_PATH));

                if (manifest
                    .Descendants("service")
                    .First()
                    .Attribute(NotificationsWindow.NAMESPACE_ANDROID + "enabled")
                    .Value == "false") {
                    problems.Add(Tuple.New(
                        "Android push notifications not enabled due to disabled service. This can be configured through the Editor menu.",
                        Severity.WARNING));
                }
            }

            if (File.Exists(NotificationsWindow.NOTIFICATIONS_XML_PATH)) {
                new List<string> {
                    NotificationsWindow.ATTR_APP_ID,
                    NotificationsWindow.ATTR_SENDER_ID}
                    .Except(XDocument
                        .Parse(File.ReadAllText(NotificationsWindow.NOTIFICATIONS_XML_PATH))
                        .Descendants("string")
                        .Select(e => e.Attribute("name").Value))
                    .ToList()
                    .ForEach(e => {
                        switch (e) {
                            case NotificationsWindow.ATTR_APP_ID:
                                problems.Add(Tuple.New(
                                    "Application ID not set for Android push notifications. This can be configured through the Editor menu.",
                                    Severity.WARNING));
                                break;

                            case NotificationsWindow.ATTR_SENDER_ID:
                                problems.Add(Tuple.New(
                                    "Sender ID not set for Android push notifications. This can be configured through the Editor menu.",
                                    Severity.WARNING));
                                break;
                        }
                    });
            }
        }
    }
}