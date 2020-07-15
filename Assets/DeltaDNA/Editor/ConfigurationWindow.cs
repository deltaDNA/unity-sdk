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

using DeltaDNA.Ads.Editor;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA.Editor
{
    public sealed class ConfigurationWindow : EditorWindow {
        
        internal const string CONFIG = "Assets/DeltaDNA/Resources/ddna_configuration.xml";
        
        private readonly XmlSerializer analyticsSerialiser = new XmlSerializer(
            typeof(Configuration),
            new XmlRootAttribute("configuration"));

        // UI
        
        private Texture logo;
        private GUIStyle styleFoldout;
        private bool foldoutAnalytics = true;
        private bool foldoutAndroidNotifications = true;
        private bool foldoutiOSNotifications = true;
        private bool foldoutSmartAds = false;
        private Vector2 scrollPosition;
        
        // config
        
        private Configuration analytics;
        private NotificationsConfigurator notifications;
        private iOSConfiguration iOS;
        private AdsConfigurator ads;
        
        void OnEnable() {
            titleContent = new GUIContent(
                "Configuration",
                AssetDatabase.LoadAssetAtPath<Texture>("Assets/DeltaDNA/Editor/Resources/Logo_16.png"));
            
            Load();
        }
        
        void OnGUI() {
            // workaround for OnEnable weirdness when initialising values
            if (logo == null) logo = AssetDatabase.LoadAssetAtPath<Texture>("Assets/DeltaDNA/Editor/Resources/Logo.png");
            if (styleFoldout == null) styleFoldout = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };
            
            GUILayout.Label(logo, GUILayout.ExpandWidth(false));
            
            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foldoutAnalytics = CreateFoldout(
                foldoutAnalytics,
                "Analytics",
                true,
                styleFoldout);
            if (foldoutAnalytics) {
                GUILayout.Label("Required", EditorStyles.boldLabel);
                
                analytics.environmentKeyDev = EditorGUILayout.TextField(
                    new GUIContent(
                        "Environment key (dev)",
                        "Enter your game's development environment key"),
                    analytics.environmentKeyDev);
                analytics.environmentKeyLive = EditorGUILayout.TextField(
                    new GUIContent(
                        "Environment key (live)",
                        "Enter your game's live environment key"),
                    analytics.environmentKeyLive);
                analytics.environmentKey = EditorGUILayout.Popup(
                    new GUIContent(
                        "Selected key",
                        "Select which environment key to use for the build"),
                    analytics.environmentKey,
                    new GUIContent[] {
                        new GUIContent("Development"),
                        new GUIContent("Live")});
                analytics.collectUrl = EditorGUILayout.TextField(
                    new GUIContent(
                        "Collect URL",
                        "Enter your game's collect URL"),
                    analytics.collectUrl);
                analytics.engageUrl = EditorGUILayout.TextField(
                    new GUIContent(
                        "Engage URL",
                        "Enter your game's engage URL"),
                    analytics.engageUrl);
                
                GUILayout.Label("Optional", EditorStyles.boldLabel);
                
                analytics.hashSecret = EditorGUILayout.TextField(
                    new GUIContent(
                        "Hash secret",
                        "Enter your game's hash secret if hashing is enabled"),
                    analytics.hashSecret);
                EditorGUI.BeginDisabledGroup(analytics.useApplicationVersion);
                analytics.clientVersion = EditorGUILayout.TextField(
                    new GUIContent(
                        "Client version",
                        "Enter your game's version or use the Editor value by enabling the checkbox below"),
                    analytics.clientVersion);
                EditorGUI.EndDisabledGroup();
                analytics.useApplicationVersion = GUILayout.Toggle(
                    analytics.useApplicationVersion,
                    new GUIContent(
                        "Use application version",
                        "Check to use the application/bundle version as set in the Editor"));
            }
            
            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);
            
            foldoutAndroidNotifications = CreateFoldout(
                foldoutAndroidNotifications,
                "Android Notifications",
                true,
                styleFoldout);
            if (foldoutAndroidNotifications) {
                if (!AreAndroidNotificationsInProject()) {
                    GUILayout.Label("Configuration not available due to notification dependencies not present in project.");
                } else {
                    notifications.appId = EditorGUILayout.TextField(
                        new GUIContent(
                            "Application ID",
                            "Enter the Application ID for your application in the Firebase Console"),
                        notifications.appId);
                    notifications.senderId = EditorGUILayout.TextField(
                        new GUIContent(
                            "Sender ID",
                            "Enter the Sender ID for your application in the Firebase Console"),
                        notifications.senderId);
                    notifications.projectId = EditorGUILayout.TextField(
                        new GUIContent(
                            "Project ID",
                            "Enter the Project ID for your application in the Firebase Console"),
                        notifications.projectId);
                    notifications.apiKey = EditorGUILayout.TextField(
                        new GUIContent(
                            "Firebase API Key",
                            "Enter the API Key for your application in the Firebase Console"),
                        notifications.apiKey);
                    notifications.listenerService = EditorGUILayout.TextField(
                        new GUIContent(
                            "Listener Service",
                            "If you have your own implementation of the NotificationListenerService you should set the field to use your own class"),
                        notifications.listenerService);
                    
                    notifications.notificationIcon = EditorGUILayout.TextField(
                        new GUIContent(
                            "Notification Icon",
                            "The icon for the notification should be the name of the drawable resource, for example 'icon_notification' if you have 'icon_notification.png' in the 'res/drawable' folder"),
                        notifications.notificationIcon);
                    notifications.notificationTitle = EditorGUILayout.TextField(
                        new GUIContent(
                            "Notification Title",
                            "The title should be the string literal that you would like to appear in the notification, or a localisable string resource from the 'res/values' folder such as '@string/resource_name'"),
                        notifications.notificationTitle);
                }
            }
            
            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);


            foldoutiOSNotifications = CreateFoldout(
                foldoutiOSNotifications,
                "iOS Rich Push Notifications",
                true,
                styleFoldout);
            if (foldoutiOSNotifications)
            {
#if UNITY_2019_3_OR_NEWER
                DrawIOSRichPushNotificationSettings();
#elif UNITY_2018_4_OR_NEWER || UNITY_2019_1 || UNITY_2019_2
                EditorGUILayout.HelpBox("In order to support iOS Rich Push Notifications in this version of Unity, you must change the build system in the built XCode project to 'legacy' (File -> Project Settings -> Build System). This is done automatically in 2019.3 or newer.", MessageType.Warning);
                DrawIOSRichPushNotificationSettings();
#else
                EditorGUILayout.HelpBox("iOS rich push notifications can only be used in Unity 2018.4 or newer; 2019.3 or newer is recommended.", MessageType.Warning);
#endif
            }

            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                "Apply",
                GUILayout.Width(WindowHelper.WIDTH_BUTTON))) Apply();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawIOSRichPushNotificationSettings()
        {
            EditorGUI.BeginChangeCheck();
            iOS.enableRichPushNotifications = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Enable",
                        "Tick this to enable rich push notifications in the iOS app"),
                    iOS.enableRichPushNotifications);
            iOS.pushNotificationServiceExtensionIdentifier = EditorGUILayout.TextField(
                new GUIContent(
                        "Extension Identifier",
                        "This is the bundle identifier that will be given to the push notification service extension that is added to the XCode project"),
                    iOS.pushNotificationServiceExtensionIdentifier);
            if (EditorGUI.EndChangeCheck())
            {
                iOS.Dirty = true;
            }
        }
        
        private void Load() {
            if (File.Exists(CONFIG)) {
                using (var stringReader = new StringReader(File.ReadAllText(CONFIG))) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        analytics = analyticsSerialiser.Deserialize(xmlReader) as Configuration;
                    }
                }
            } else {
                analytics = new Configuration();
            }

            iOS = iOSConfiguration.Load();
            notifications = new NotificationsConfigurator();
            ads = new AdsConfigurator();
        }
        
        private void Apply() {
            using (var stringWriter = new StringWriter()) {
                using (XmlWriter xmlWriter = XmlWriter.Create(
                    stringWriter, new XmlWriterSettings() { Indent = true })) {
                    analyticsSerialiser.Serialize(xmlWriter, analytics);
                    File.WriteAllText(CONFIG, stringWriter.ToString());
                }
            }

            if (iOS.Dirty) iOS.Save();
            if (AreAndroidNotificationsInProject()) notifications.Apply();
            if (AreSmartAdsInProject()) ads.Apply();
            
            Debug.Log("[DeltaDNA] Changes have been applied to XML configuration files, please commit the updates to version control");
        }

        private static bool CreateFoldout(
            bool foldout,
            string content,
            bool toggleOnLabelClick,
            GUIStyle style) {

#if UNITY_5_5_OR_NEWER
            return EditorGUILayout.Foldout(foldout, content, toggleOnLabelClick, style);
#else
            return EditorGUILayout.Foldout(foldout, content, style);
#endif
        }
        
        private static bool AreAndroidNotificationsInProject() {
            return Directory.Exists("Assets/Plugins/Android/deltadna-sdk-unity-notifications");
        }
        
        private static bool AreSmartAdsInProject() {
            return Directory.Exists("Assets/DeltaDNA/Ads");
        }
    }
}
