using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if (UNITY_IOS && UNITY_2017_1_OR_NEWER)
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace DeltaDNAAds
{
    public class EnableDebugNotificationExtension : ScriptableObject
    {
        #if (UNITY_IOS && UNITY_2017_1_OR_NEWER)
        private const int BUILD_ORDER_ADD_EXTENSION = 10000;    // after the UnityJarResolver runs pod install
    
        [PostProcessBuildAttribute(BUILD_ORDER_ADD_EXTENSION)]
        public static void OnPostProcessEnableNotificationExtension(
            BuildTarget buildTarget, string buildPath)
        {
            if (!Editor.InitialisationHelper.IsDevelopment()
                || !Editor.InitialisationHelper.IsDebugNotifications()) {
                return;
            }
            
            Debug.Log("Adding DDNA debug notification content extension into XCode Project");
            
            string unityPlugin = buildPath + "/Libraries/DeltaDNAAds/Plugins/iOS/DDNADebugUnityPlugin.mm";
            File.WriteAllText(
                unityPlugin,
                File.ReadAllText(unityPlugin).Replace(
                    "//IMPL_APP_CONTROLLER_SUBCLASS(DDNAUnityAppController)",
                    "IMPL_APP_CONTROLLER_SUBCLASS(DDNAUnityAppController)"));
            
            PBXProject proj = new PBXProject();
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            proj.ReadFromFile(projPath);

            string mainTarget = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
            string srcPath = Application.dataPath + "/DeltaDNAAds/Editor/iOS/NotificationContent/";
            string destPath = buildPath + "/Notification Content/";
 
            FileUtil.CopyFileOrDirectory(srcPath, destPath);

            string newTarget = proj.AddAppExtension(mainTarget, "Notification Content", PlayerSettings.applicationIdentifier + ".ddna-debug-ext", "Notification Content/Info.plist");
            proj.AddFileToBuild(newTarget, proj.AddFile(destPath + "NotificationViewController.h", "Notification Content/NotificationViewController.h"));
            proj.AddFileToBuild(newTarget, proj.AddFile(destPath + "NotificationViewController.m", "Notification Content/NotificationViewController.m"));
            proj.AddFileToBuild(newTarget, proj.AddFile(destPath + "Info.plist", "Notification Content/Info.plist"));
            proj.AddFileToBuild(newTarget, proj.AddFile(destPath + "Base.lproj/MainInterface.storyboard", "Notification Content/MainInterface.storyboard"));
            proj.AddFrameworkToProject(newTarget, "UserNotificationsUI.framework", true);
            proj.AddFrameworkToProject(newTarget, "UserNotifications.framework", true);

            // link in the code from the Pods library
            string podPath = buildPath + "/Pods/DeltaDNADebug/DeltaDNADebug/";
            proj.AddFileToBuild(newTarget, proj.AddFile(podPath + "Classes/DDNADebugNotificationViewController.h", "Notification Content/DDNADebugNotificationViewController.h"));
            proj.AddFileToBuild(newTarget, proj.AddFile(podPath + "Classes/DDNADebugNotificationViewController.m", "Notification Content/DDNADebugNotificationViewController.m"));
            proj.AddFileToBuild(newTarget, proj.AddFile(podPath + "Classes/DDNADebugContentViewController.h", "Notification Content/DDNADebugContentViewController.h"));
            proj.AddFileToBuild(newTarget, proj.AddFile(podPath + "Classes/DDNADebugContentViewController.m", "Notification Content/DDNADebugContentViewController.m"));
            proj.AddFileToBuild(newTarget, proj.AddFile(podPath + "Assets/DebugInterface.storyboard", "Notification Content/DebugInterface.storyboard"));
            proj.SetCompileFlagsForFile(newTarget, proj.FindFileGuidByProjectPath("Notification Content/DDNADebugContentViewController.m"), new List<string> { "-fobjc-arc" });
            
            proj.SetTeamId(newTarget, PlayerSettings.iOS.appleDeveloperTeamID);
            
            proj.SetBuildProperty(newTarget, "IPHONEOS_DEPLOYMENT_TARGET", PlayerSettings.iOS.targetOSVersionString);
            proj.SetBuildProperty(newTarget, "ONLY_ACTIVE_ARCH", "NO");
            proj.UpdateBuildProperty(newTarget, "ARCHS", new string[] { "armv7", "arm64" }, new string[] {});
                
            // Update the search paths
            proj.SetBuildProperty(newTarget, "CLANG_ENABLE_MODULES", "YES");
            proj.AddBuildProperty(newTarget, "OTHER_LDFLAGS", "$(inherited)");
            proj.AddBuildProperty(newTarget, "OTHER_CFLAGS", "$(inherited)");
            proj.AddBuildProperty(newTarget, "HEADER_SEARCH_PATHS",
                                   "$(inherited)");
            proj.AddBuildProperty(newTarget, "HEADER_SEARCH_PATHS",
                                    "$(PROJECT_DIR)/Pods/Headers/Public");
            proj.AddBuildProperty(newTarget, "FRAMEWORK_SEARCH_PATHS",
                                    "$(inherited)");
            proj.AddBuildProperty(newTarget, "FRAMEWORK_SEARCH_PATHS",
                                    "$(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(newTarget, "LIBRARY_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(newTarget, "OTHER_LDFLAGS", "-ObjC");     
            
            proj.WriteToFile(projPath);
        }
        #endif
    }
}