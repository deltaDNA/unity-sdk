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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

namespace DeltaDNA.Editor {
    public sealed class ConfigurationWindow : EditorWindow {
        
        internal const string CONFIG = "Assets/DeltaDNA/Resources/ddna_configuration.xml";
        
        private readonly XmlSerializer analyticsSerialiser = new XmlSerializer(
            typeof(Configuration),
            new XmlRootAttribute("configuration"));
        
        // UI
        
        private Texture logo;
        private GUIStyle styleFoldout;
        private bool foldoutAnalytics = true;
        private bool foldoutNotifications = true;
        private bool foldoutSmartAds = true;
        private Vector2 scrollPosition;
        
        // config
        
        private Configuration analytics;
        private NotificationsConfigurator notifications;
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
            
            foldoutNotifications = CreateFoldout(
                foldoutNotifications,
                "Android Notifications",
                true,
                styleFoldout);
            if (foldoutNotifications) {
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
            
            GUILayout.BeginHorizontal();
            foldoutSmartAds = CreateFoldout(
                foldoutSmartAds,
                "SmartAds",
                true,
                styleFoldout);
            GUILayout.FlexibleSpace();
            ads.On = GUILayout.Toggle(ads.On, "Enabled");
            GUILayout.EndHorizontal();
            if (foldoutSmartAds) {
                GUILayout.Label("Networks", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(!ads.On);
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(
                    "Name",
                    EditorStyles.boldLabel,
                    GUILayout.Width(WindowHelper.WIDTH_LABEL));
                
                foreach (var handler in ads.handlers) {
                    GUILayout.Label(
                        handler.platformVisible,
                        EditorStyles.boldLabel,
                        GUILayout.Width(WindowHelper.WIDTH_TOGGLE));
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginVertical();
                foreach (IDictionary<string, object> network in ads.networks) {
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(0.5f));
                    
                    GUILayout.BeginHorizontal();
                    
                    var name = network[AdsConfigurator.NAME] as string;
                    var hint = network[AdsConfigurator.INTEGRATION] as string != "manual"
                        ? "Network works out of the box, "
                        : "Contact support@deltadna.com for setup details, ";
                    
                    var types = network[AdsConfigurator.TYPE] as IList<object>;
                    if (types.Contains("interstitial") && types.Contains("rewarded")) {
                        hint += "supports interstitial and rewarded ads";
                    } else if (types.Contains("interstitial")) {
                        hint += "supports interstitial ads";
                    } else {
                        hint += "supports rewarded ads";
                    }
                    
                    GUILayout.Label(
                        new GUIContent(name, hint),
                        GUILayout.Width(WindowHelper.WIDTH_LABEL));
                    
                    foreach (var handler in ads.handlers) {
                        var value = network[handler.platform] as string;
                        if (value == null) {
                            // empty label to fill the space
                            GUILayout.Label("", GUILayout.Width(WindowHelper.WIDTH_TOGGLE));
                            continue;
                        }
                        
                        ads.enabled[handler][value] = GUILayout.Toggle(
                            ads.enabled[handler].ContainsKey(value)
                                ? ads.enabled[handler][value]
                                : true,
                            new GUIContent("", name + " for " + handler.platformVisible),
                            GUILayout.Width(WindowHelper.WIDTH_TOGGLE));
                    }
                    
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                
                GUILayout.Label("Additional", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(!Ads.Editor.InitialisationHelper.IsDevelopment());
                ads.debugNotifications = GUILayout.Toggle(
                    ads.debugNotifications,
                    "Enable debug notifications in development builds");
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.EndDisabledGroup();
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
