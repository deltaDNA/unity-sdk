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
                "25.1.0"},
            namedArgs: new Dictionary<string, object>() {
                { "packageIds", new string[] { "extra-google-m2repository" }}});

        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport,
            "DependOn",
            new object[] {
                "com.google.firebase",
                "firebase-messaging",
                "10.0.1"},
            namedArgs: new Dictionary<string, object>() {
                { "packageIds", new string[] { "extra-android-m2repository" }}});
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
