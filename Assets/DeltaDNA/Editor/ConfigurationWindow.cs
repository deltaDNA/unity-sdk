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

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

namespace DeltaDNA.Editor
{
    public sealed class ConfigurationWindow : EditorWindow {

        // UI
        
        private Texture logo;
        private GUIStyle styleFoldout;
        private bool foldoutAnalytics = true;
        private bool foldoutAndroidNotifications = true;
        private bool foldoutiOSNotifications = true;
        private Vector2 scrollPosition;
        
        // config
        
        private NotificationsConfigurator notifications;
        private iOSConfiguration iOS;
        
        void OnEnable() {
            titleContent = new GUIContent(
                "Configuration",
                AssetDatabase.LoadAssetAtPath<Texture>(WindowHelper.FindFile("Editor/Resources/Logo_16.png")));
            
            Load();
        }


        
        void OnGUI() {
            // workaround for OnEnable weirdness when initialising values
            if (logo == null) logo = AssetDatabase.LoadAssetAtPath<Texture>(WindowHelper.FindFile("Editor/Resources/Logo.png"));
            if (styleFoldout == null) styleFoldout = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            SerializedObject cfg = GetSerializedConfig();
            
            GUILayout.Label(logo, GUILayout.ExpandWidth(false));
            
            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foldoutAnalytics = CreateFoldout(
                foldoutAnalytics,
                "Analytics",
                true,
                styleFoldout);
            if (foldoutAnalytics) 
            {
                GUILayout.Label("Required", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(
                    cfg.FindProperty("environmentKeyDev"),
                    new GUIContent(
                        "Environment key (dev)",
                        "Enter your game's development environment key"));

                EditorGUILayout.PropertyField(
                    cfg.FindProperty("environmentKeyLive"),
                    new GUIContent(
                        "Environment key (live)",
                        "Enter your game's live environment key"));

                SerializedProperty env_key = cfg.FindProperty("environmentKey");

                env_key.intValue = EditorGUILayout.Popup(

                    new GUIContent(
                        "Selected key",
                        "Select which environment key to use for the build"),
                    env_key.intValue,
                    new GUIContent[] {
                        new GUIContent("Development"),
                        new GUIContent("Live")});
                
                EditorGUILayout.PropertyField(
                    cfg.FindProperty("collectUrl"),
                    new GUIContent(
                        "Collect URL",
                        "Enter your game's collect URL"));

                EditorGUILayout.PropertyField(
                    cfg.FindProperty("engageUrl"),
                    new GUIContent(
                        "Engage URL",
                        "Enter your game's engage URL"));


                
                GUILayout.Label("Optional", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(
                    cfg.FindProperty("hashSecret"),
                    new GUIContent(
                        "Hash secret",
                        "Enter your game's hash secret if hashing is enabled"));

                EditorGUI.BeginDisabledGroup(cfg.FindProperty("useApplicationVersion").boolValue);

                EditorGUILayout.PropertyField(
                    cfg.FindProperty("clientVersion"),
                    new GUIContent(
                        "Client version",
                        "Enter your game's version or use the Editor value by enabling the checkbox below"));

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(
                    cfg.FindProperty("useApplicationVersion"),
                    new GUIContent(
                        "Use application version",
                        "Check to use the application/bundle version as set in the Editor"));

                if (cfg.hasModifiedProperties)
                {
                    cfg.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
            }
            
            GUILayout.Space(WindowHelper.HEIGHT_SEPARATOR);
            
            EditorGUI.BeginChangeCheck();

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
                    
                    if (GUILayout.Button("Apply Android Notification Settings"))
                    {
                        ApplyAndroidNotificationSettings();
                    }
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if (AreAndroidNotificationsInProject()) {
                    notifications.Apply();
                    Debug.Log("[DeltaDNA] Changes have been applied to XML configuration files, please commit the updates to version control");
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
                iOS.Save();
                Debug.Log("[DeltaDNA] Changes have been applied to XML configuration files, please commit the updates to version control");
            }
        }
        
        private void Load() {
            iOS = iOSConfiguration.Load();
            notifications = new NotificationsConfigurator();
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
        
        private static bool AreAndroidNotificationsInProject() 
        {
            return Directory.Exists(NotificationsConfigurator.NOTIFICATION_PATH);
        }

        private SerializedObject GetSerializedConfig()
        {
            Configuration cfg = AssetDatabase.LoadAssetAtPath<Configuration>(Configuration.FULL_ASSET_PATH);

            if (cfg != null)
            {
                return new SerializedObject(cfg);
            }

            // If we couldn't load the asset we should create a new instance.
            cfg = ScriptableObject.CreateInstance<Configuration>();

            if (!AssetDatabase.IsValidFolder(Configuration.ASSET_DIRECTORY))
            {
                AssetDatabase.CreateFolder(Configuration.RESOURCES_CONTAINER, Configuration.RESOURCES_DIRECTORY);
            }
            AssetDatabase.CreateAsset(cfg, Configuration.FULL_ASSET_PATH);
            AssetDatabase.SaveAssets();

            return new SerializedObject(cfg);
        }
        
        private void ApplyAndroidNotificationSettings()
        {
            // Ensure that all fields are filled in
            if (
                String.IsNullOrEmpty(notifications.apiKey)
                || String.IsNullOrEmpty(notifications.appId)
                || String.IsNullOrEmpty(notifications.projectId)
                || String.IsNullOrEmpty(notifications.senderId)
            )
            {
                Debug.LogError(
                    "Some required information for Android notifications is missing from the deltaDNA configuration. " +
                    "Please check these values are present: Application ID, Project ID, Sender ID and Firebase API Key." +
                    "The notifications configuration has not been saved."
                );
                return;
            }

            try
            {
                CopyAndroidNotificationFolder();
                // Inform the user that the plugin has been correctly configured in their assets folder
                Debug.Log(
                    "deltaDNA Android notification setup complete. The configured plugin has been updated in your Assets/Plugins/Android folder"
                );
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to update the android notification configuration, changes have not been saved.");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            
        }

        private void CopyAndroidNotificationFolder()
        {
            string pluginFolder = NotificationsConfigurator.NOTIFICATION_PATH;
            string pluginTargetPath = $"Plugins/Android/{NotificationsConfigurator.PLUGIN_FOLDER_NAME}.androidlib";
            string assetPath = $"Assets/{pluginTargetPath}";
            string targetFolder = $"{Application.dataPath}/{pluginTargetPath}";

            AssetDatabase.DeleteAsset(assetPath);
            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }
            DirectoryCopy(pluginFolder, targetFolder);
            AssetDatabase.ImportAsset(assetPath);
        }

        // Adapted from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        // as there was no inbuilt method to copy a directory recursively.
        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            
            Directory.CreateDirectory(destDirName);        

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension.EndsWith("meta"))
                {
                    continue;
                }
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }
            
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath);
            }
        }
    }
    

}
