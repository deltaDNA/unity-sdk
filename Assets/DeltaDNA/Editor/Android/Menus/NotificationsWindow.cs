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

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DeltaDNA.Editor {
    public sealed class NotificationsWindow : EditorWindow {
        
        private const int WIDTH_LABEL = 100;
        private const int WIDTH_BUTTON = 80;
        private const int HEIGHT_SEPARATOR = 20;

        private const string NOTIFICATIONS_XML_PATH = "Assets/Plugins/Android/deltadna-sdk-unity-notifications/res/values/values.xml";
        private const string MANIFEST_XML_PATH = "Assets/Plugins/Android/deltadna-sdk-unity-notifications/AndroidManifest.xml";
        private XNamespace NAMESPACE_ANDROID = "http://schemas.android.com/apk/res/android";

        private const string ATTR_APP_ID = "google_app_id";
        private const string ATTR_SENDER_ID = "gcm_defaultSenderId";
        private const string DEFAULT_LISTENER_SERVICE = "com.deltadna.android.sdk.notifications.NotificationListenerService";

        private string appId = "";
        private string senderId = "";
        private string listenerService = "";

        public NotificationsWindow() : base() {
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
                listenerService = XDocument
                    .Parse(File.ReadAllText(MANIFEST_XML_PATH))
                    .Descendants("service")
                    .First()
                    .Attribute(NAMESPACE_ANDROID + "name")
                    .Value;
            } else {
                listenerService = "";
            }
        }
        
        void OnGUI() {
            var style = new GUIStyle();
            style.wordWrap = true;
            style.margin = new RectOffset(5, 5, 5, 5);

            if (!MenuItems.AreAndroidNotificationsInProject()) {
                GUILayout.Label(
                    "Configuration not available due to notification dependencies not present " +
                    "in project.",
                    style);
                return;
            }

            GUILayout.BeginVertical();
            
            GUILayout.Label(
                "Configure notifications for Android here, filling in the Application " +
                "and Sender ID from the Google or Firebase Console for your application.\n" +
                "\n" +
                "If you have your own implementation of the NotificationListenerService you " +
                "should set the field to use your own class.\n" +
                "\n" +
                "Hitting 'Apply' will persist the changes to the resource and manifest files. " +
                "You may leave the fields empty if you do not wish to use notifications.",
                style);

            GUILayout.Space(HEIGHT_SEPARATOR);

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Application ID",
                GUILayout.Width(WIDTH_LABEL));
            appId = EditorGUILayout.TextField(appId);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Sender ID",
                GUILayout.Width(WIDTH_LABEL));
            senderId = EditorGUILayout.TextField(senderId);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Listener Service",
                GUILayout.Width(WIDTH_LABEL));
            listenerService = EditorGUILayout.TextField(listenerService);
            GUILayout.EndHorizontal();

            GUILayout.Space(HEIGHT_SEPARATOR);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(WIDTH_BUTTON))) Apply();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
        
        private void Apply() {
            if (!File.Exists(NOTIFICATIONS_XML_PATH)) {
                Directory.CreateDirectory(NOTIFICATIONS_XML_PATH.Substring(
                    0,
                    NOTIFICATIONS_XML_PATH.LastIndexOf('/')));
            }

            var notifications = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var notificationsResources = new XElement("resources");
            notifications.Add(notificationsResources);

            var appIdPresent = false;
            if (!string.IsNullOrEmpty(appId)) {
                appIdPresent = true;
                notificationsResources.Add(new XElement(
                    "string",
                    new object[] {
                        new XAttribute("name", ATTR_APP_ID),
                        appId
                    }));
            } else {
                notificationsResources
                    .Elements()
                    .Where(e => e.Attribute("name").Value == ATTR_APP_ID)
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
            } else {
                notificationsResources
                    .Elements()
                    .Where(e => e.Attribute("name").Value == ATTR_SENDER_ID)
                    .Remove();
            }

            notifications.Save(NOTIFICATIONS_XML_PATH);

            var manifest = XDocument.Parse(File.ReadAllText(MANIFEST_XML_PATH));
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
            manifest.Save(MANIFEST_XML_PATH);

            Debug.Log(
                "Saved options to " + NOTIFICATIONS_XML_PATH +
                " and " + MANIFEST_XML_PATH);
        }
    }
}
