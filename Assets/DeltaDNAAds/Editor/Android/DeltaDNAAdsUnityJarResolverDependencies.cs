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

[InitializeOnLoad]
public class DeltaDNAAdsUnityJarResolverDependencies : AssetPostprocessor {

    #if UNITY_ANDROID
    private static Google.JarResolver.Resolver.ResolverImpl resolver =
        Google.JarResolver.Resolver.CreateSupportInstance("DeltaDNA");

    private const string VERSION_SUPPORT =
        DeltaDNAUnityJarResolverDependencies.VERSION_SUPPORT;
    private const string VERSION_PLAYSERVICES =
        DeltaDNAUnityJarResolverDependencies.VERSION_PLAYSERVICES;
    #endif

    static DeltaDNAAdsUnityJarResolverDependencies() {
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
    internal static void Resolve() {
        GooglePlayServices.PlayServicesResolver.Resolve();
    }

    /// <summary>
    /// Registers the android dependencies.
    /// </summary>
    internal static void RegisterAndroidDependencies() {
        // clear the dependencies as we add them depending on which networks are set
        resolver.ClearDependencies();

        resolver.DependOn(
            "com.android.support",
            "support-annotations",
            VERSION_SUPPORT);

        var networks = new DeltaDNAAds.Editor.AndroidNetworks(true, false).GetPersisted();
        if (networks.Count > 0) {
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-basement",
                VERSION_PLAYSERVICES);
        }
        if (networks.Contains("adcolony")) {
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-base",
                VERSION_PLAYSERVICES);
        }
        if (networks.Contains("admob")) {
            // instead of com.google.firebase:firebase-ads which is empty and breaks Unity
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-ads",
                VERSION_PLAYSERVICES);
        }
        if (networks.Contains("facebook")) {
            resolver.DependOn(
                "com.android.support",
                "appcompat-v7",
                VERSION_SUPPORT);
            resolver.DependOn(
                "com.android.support",
                "recyclerview-v7",
                VERSION_SUPPORT);
            resolver.DependOn(
                "com.android.support",
                "support-v4",
                VERSION_SUPPORT);
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-ads",
                VERSION_PLAYSERVICES);
        }
        if (networks.Contains("inmobi")) {
            resolver.DependOn(
                "com.android.support",
                "appcompat-v7",
                VERSION_SUPPORT);
            resolver.DependOn(
                "com.android.support",
                "recyclerview-v7",
                VERSION_SUPPORT);
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-base",
                VERSION_PLAYSERVICES);
        }
        if (networks.Contains("thirdpresence")) {
            resolver.DependOn(
                "com.android.support",
                "support-v4",
                VERSION_SUPPORT);
        }
        if (networks.Contains("vungle")) {
            resolver.DependOn(
                "com.google.android.gms",
                "play-services-base",
                VERSION_PLAYSERVICES);
        }
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
