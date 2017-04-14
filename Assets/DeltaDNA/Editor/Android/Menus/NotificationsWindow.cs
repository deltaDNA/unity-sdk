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

namespace DeltaDNA.Editor {
    public sealed class NotificationsWindow : EditorWindow {
        
        private const int WIDTH_LABEL = 100;
        private const int WIDTH_BUTTON = 80;
        private const int HEIGHT_SEPARATOR = 20;

        private readonly NotificationsConfigurator configurator =
            new NotificationsConfigurator();
        
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
                "The icon for the notification should be the name of the drawable resource, " +
                "for example 'icon_notification' if you have 'icon_notification.png' in the " +
                "'res/drawable' folders.\n" +
                "The title should be the string literal that you would like to appear in the " +
                "notification, or a localisable string resource from the 'res/values' folder " +
                "such as '@string/resource_name'.\n" +
                "\n" +
                "Hitting 'Apply' will persist the changes to the resource and manifest files. " +
                "You may leave the fields empty if you do not wish to use notifications.",
                style);

            GUILayout.Space(HEIGHT_SEPARATOR);

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Application ID",
                GUILayout.Width(WIDTH_LABEL));
            configurator.appId = EditorGUILayout.TextField(configurator.appId);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Sender ID",
                GUILayout.Width(WIDTH_LABEL));
            configurator.senderId = EditorGUILayout.TextField(configurator.senderId);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Listener Service",
                GUILayout.Width(WIDTH_LABEL));
            configurator.listenerService = EditorGUILayout.TextField(configurator.listenerService);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Notification Icon",
                GUILayout.Width(WIDTH_LABEL));
            configurator.notificationIcon = EditorGUILayout.TextField(configurator.notificationIcon);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                "Notification Title",
                GUILayout.Width(WIDTH_LABEL));
            configurator.notificationTitle = EditorGUILayout.TextField(configurator.notificationTitle);
            GUILayout.EndHorizontal();

            GUILayout.Space(HEIGHT_SEPARATOR);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(WIDTH_BUTTON))) configurator.Apply();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}
