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
public class DeltaDNAUnityJarResolverDependencies : AssetPostprocessor {

    #if UNITY_ANDROID
    private static Google.JarResolver.Resolver.ResolverImpl resolver =
        Google.JarResolver.Resolver.CreateSupportInstance("DeltaDNA");

    private const string REPO = "http://deltadna.bintray.com/android";
    private const string VERSION = "4.5.3";
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
            resolver.ClearDependencies();
        } else {
            resolver.DependOn(
                "com.deltadna.android",
                "deltadna-sdk-notifications",
                VERSION,
                repositories: new string[] { REPO });
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
