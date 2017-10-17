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

using DeltaDNA.Editor;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

[InitializeOnLoad]
public class DeltaDNAUnityJarResolverDependencies : AssetPostprocessor {

    #if UNITY_ANDROID
    private const string CONFIGURATION = MenuItems.ASSETS_PATH + "Editor/Android/Dependencies.xml";
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
        var config = Configuration();

        config.Descendants("androidPackage").Remove();

        if (!MenuItems.AreAndroidNotificationsInProject()) {
            config.Save(CONFIGURATION);
            return;
        }

        config
            .Descendants("androidPackages")
            .First()
            .Add(new XElement(
                "androidPackage",
                new object[] {
                    new XAttribute(
                        "spec",
                        "com.deltadna.android:deltadna-sdk-notifications:" + VERSION),
                    new XElement(
                        "repositories",
                        new object[] { new XElement("repository", REPO) })
                }));

        config.Save(CONFIGURATION);
    }

    private static XDocument Configuration() {
        return XDocument.Parse(File.ReadAllText(CONFIGURATION));
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
