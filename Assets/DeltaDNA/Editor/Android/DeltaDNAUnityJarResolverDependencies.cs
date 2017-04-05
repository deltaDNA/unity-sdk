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

#if UNITY_ANDROID
using System;
using System.Collections.Generic;
#endif
using UnityEditor;

[InitializeOnLoad]
public class DeltaDNAUnityJarResolverDependencies : AssetPostprocessor {

    #if UNITY_ANDROID
    /// <summary>Instance of the PlayServicesSupport resolver</summary>
    public static object svcSupport;

    private const string VERSION_SUPPORT = "25.3.1";
    private const string VERSION_PLAYSERVICES = "10.2.1";
    #endif

    static DeltaDNAUnityJarResolverDependencies() {
        RegisterDependencies();
    }

    /// <summary>
    /// Registers the dependencies needed by this plugin.
    /// </summary>
    public static void RegisterDependencies() {
        #if UNITY_ANDROID
        RegisterAndroidDependencies();
        #endif
    }

    #if UNITY_ANDROID
    /// <summary>
    /// Registers the android dependencies.
    /// </summary>
    public static void RegisterAndroidDependencies() {
        if (!DeltaDNA.Editor.MenuItems.AreAndroidNotificationsInProject()) {
            return;
        }

        Type playServicesSupport = Google.VersionHandler.FindClass(
            "Google.JarResolver",
            "Google.JarResolver.PlayServicesSupport");
        if (playServicesSupport == null) {
            return;
        }

        svcSupport = svcSupport ?? Google.VersionHandler.InvokeStaticMethod(
            playServicesSupport,
            "CreateInstance",
            new object[] {
                "DeltaDNA",
                EditorPrefs.GetString("AndroidSdkRoot"),
                "ProjectSettings"});

        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport,
            "DependOn",
            new object[] {
                "com.android.support",
                "support-annotations",
                VERSION_SUPPORT},
            namedArgs: new Dictionary<string, object>() {
                { "packageIds", new string[] { "extra-android-m2repository" }}});
        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport,
            "DependOn",
            new object[] {
                "com.google.firebase",
                "firebase-messaging",
                VERSION_PLAYSERVICES},
            namedArgs: new Dictionary<string, object>() {
                { "packageIds", new string[] { "extra-google-m2repository" }}});
    }
    #endif

    // Handle delayed loading of the dependency resolvers.
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromPath) {
        foreach (string asset in importedAssets) {
            if (asset.Contains("JarResolver")) {
                RegisterDependencies();
                break;
            }
        }
    }
}
