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

using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

namespace DeltaDNA.Editor {

    [InitializeOnLoad]
    public sealed class NotificationsConfigurator {

        static NotificationsConfigurator() {
            // makes sure to avoid invalid Android resources after importing
            new NotificationsConfigurator().Apply();
        }

        private const string PATH = "Assets/Plugins/Android/deltadna-sdk-unity-notifications";
        internal const string NOTIFICATIONS_XML_PATH = PATH + "/res/values/values.xml";
        internal const string MANIFEST_XML_PATH = PATH + "/AndroidManifest.xml";
        internal static XNamespace NAMESPACE_ANDROID = "http://schemas.android.com/apk/res/android";

        internal const string ATTR_APP_ID = "ddna_application_id";
        internal const string ATTR_SENDER_ID = "ddna_sender_id";
        internal const string ATTR_ICON = "ddna_notification_icon";
        internal const string ATTR_TITLE = "ddna_notification_title";
        private const string DEFAULT_LISTENER_SERVICE = "com.deltadna.android.sdk.notifications.NotificationListenerService";

        internal string appId = "";
        internal string senderId = "";
        internal string listenerService = "";
        internal string notificationIcon = "";
        internal string notificationTitle = "";

        public NotificationsConfigurator() {
            if (File.Exists(NOTIFICATIONS_XML_PATH)) {
                XDocument.Parse(File.ReadAllText(NOTIFICATIONS_XML_PATH))
                    .Descendants("string")
                    .ToList()
                    .ForEach(e => {
                        switch (e.Attribute("name").Value) {
                            case ATTR_APP_ID:
                                appId = e.Value;
                                break;

                            case ATTR_SENDER_ID:
                                senderId = e.Value;
                                break;
                        }
                    });
            } else {
                appId = "";
                senderId = "";
            }

            if (File.Exists(MANIFEST_XML_PATH)) {
                var doc = XDocument.Parse(File.ReadAllText(MANIFEST_XML_PATH));
                listenerService = doc
                    .Descendants("service")
                    .First()
                    .Attribute(NAMESPACE_ANDROID + "name")
                    .Value;
                doc .Descendants("meta-data")
                    .ToList()
                    .ForEach(e => {
                        switch (e.Attribute(NAMESPACE_ANDROID + "name").Value) {
                            case ATTR_ICON:
                                notificationIcon = e.Attribute(NAMESPACE_ANDROID + "value").Value;
                                break;

                            case ATTR_TITLE:
                                var attr = e.Attribute(NAMESPACE_ANDROID + "resource");
                                if (attr != null) {
                                    notificationTitle = attr.Value;
                                } else {
                                    notificationTitle = e.Attribute(NAMESPACE_ANDROID + "value").Value;
                                }
                                break;
                        }
                    });
            } else {
                listenerService = "";
                notificationIcon = "";
                notificationTitle = "";
            }
        }
        
        internal void Apply() {
            if (!Directory.Exists(PATH)) return;

            if (!File.Exists(NOTIFICATIONS_XML_PATH)) {
                Directory.CreateDirectory(NOTIFICATIONS_XML_PATH.Substring(
                    0,
                    NOTIFICATIONS_XML_PATH.LastIndexOf('/')));
            }

            var notifications = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var notificationsResources = new XElement("resources");
            notifications.Add(notificationsResources);

            var manifest = XDocument.Parse(File.ReadAllText(MANIFEST_XML_PATH));

            var appIdPresent = false;
            if (!string.IsNullOrEmpty(appId)) {
                appIdPresent = true;

                notificationsResources.Add(new XElement(
                    "string",
                    new object[] {
                        new XAttribute("name", ATTR_APP_ID),
                        appId
                    }));

                var element = manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_APP_ID)
                    .FirstOrDefault();
                if (element != null) {
                    element.Attribute(NAMESPACE_ANDROID + "resource").Value = "@string/" + ATTR_APP_ID;
                } else {
                    manifest
                        .Descendants("application")
                        .First()
                        .Add(new XElement(
                            "meta-data",
                            new object[] {
                                new XAttribute(NAMESPACE_ANDROID + "name", ATTR_APP_ID),
                                new XAttribute(NAMESPACE_ANDROID + "resource", "@string/" + ATTR_APP_ID)}));
                }
            } else {
                notificationsResources
                    .Elements()
                    .Where(e => e.Attribute("name").Value == ATTR_APP_ID)
                    .Remove();
                manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_APP_ID)
                    .Remove();
            }

            var senderIdPresent = false;
            if (!string.IsNullOrEmpty(senderId)) {
                senderIdPresent = true;

                notificationsResources.Add(new XElement(
                    "string",
                    new object[] {
                        new XAttribute("name", ATTR_SENDER_ID),
                        senderId
                    }));

                var element = manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_SENDER_ID)
                    .FirstOrDefault();
                if (element != null) {
                    element.Attribute(NAMESPACE_ANDROID + "resource").Value = "@string/" + ATTR_SENDER_ID;
                } else {
                    manifest
                        .Descendants("application")
                        .First()
                        .Add(new XElement(
                            "meta-data",
                            new object[] {
                                new XAttribute(NAMESPACE_ANDROID + "name", ATTR_SENDER_ID),
                                new XAttribute(NAMESPACE_ANDROID + "resource", "@string/" + ATTR_SENDER_ID)}));
                }
            } else {
                notificationsResources
                    .Elements()
                    .Where(e => e.Attribute("name").Value == ATTR_SENDER_ID)
                    .Remove();
                manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_SENDER_ID)
                    .Remove();
            }

            notifications.Save(NOTIFICATIONS_XML_PATH);

            if (!string.IsNullOrEmpty(listenerService)
                && appIdPresent && senderIdPresent) {
                var service = manifest.Descendants("service").First();
                service.Attribute(NAMESPACE_ANDROID + "name").Value = listenerService;
                service.Attribute(NAMESPACE_ANDROID + "enabled").Value = "true";
            } else {
                manifest
                    .Descendants("service")
                    .First()
                    .Attribute(NAMESPACE_ANDROID + "enabled")
                    .Value = "false";
            }

            if (!string.IsNullOrEmpty(notificationIcon)) {
                var element = manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_ICON)
                    .FirstOrDefault();
                if (element != null) {
                    element.Attribute(NAMESPACE_ANDROID + "value").Value = notificationIcon;
                } else {
                    manifest
                        .Descendants("application")
                        .First()
                        .Add(new XElement(
                            "meta-data",
                            new object[] {
                                new XAttribute(NAMESPACE_ANDROID + "name", ATTR_ICON),
                                new XAttribute(NAMESPACE_ANDROID + "value", notificationIcon)}));
                }
            } else {
                manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_ICON)
                    .Remove();
            }
            if (!string.IsNullOrEmpty(notificationTitle)) {
                var resource = notificationTitle.StartsWith("@string/");
                var element = manifest
                    .Descendants("meta-data")
                    .Where(e => e
                        .Attribute(NAMESPACE_ANDROID + "name")
                        .Value == ATTR_TITLE)
                    .FirstOrDefault();

                if (element != null) {
                    element.RemoveAttributes();
                    element.SetAttributeValue(
                        NAMESPACE_ANDROID + "name",
                        ATTR_TITLE);
                    element.SetAttributeValue(
                        NAMESPACE_ANDROID + (resource ? "resource" : "value"),
                        notificationTitle);
                } else {
                    manifest
                        .Descendants("application")
                        .First()
                        .Add(new XElement(
                            "meta-data",
                            new object[] {
                                new XAttribute(NAMESPACE_ANDROID + "name", ATTR_TITLE),
                                new XAttribute(
                                    NAMESPACE_ANDROID + (resource ? "resource" : "value"),
                                    notificationTitle)}));
                }
            } else {
                manifest
                    .Descendants("meta-data")
                    .Where(e => e.Attribute(NAMESPACE_ANDROID + "name").Value == ATTR_TITLE)
                    .Remove();
            }
            manifest.Save(MANIFEST_XML_PATH);
        }
    }
}
