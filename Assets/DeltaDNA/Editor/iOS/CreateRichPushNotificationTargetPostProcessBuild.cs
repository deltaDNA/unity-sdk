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

#if UNITY_IOS && (UNITY_2018_4_OR_NEWER || UNITY_2019_1 || UNITY_2019_2)
using System;
using System.IO;
using DeltaDNA.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

public sealed class CreateRichPushNotificationTargetPostProcessBuild
{
    private const string PATH_TO_INFO_PLIST_INSIDE_TARGET = "Info.plist";
    
    [PostProcessBuild(2)]
    public static void CreateRichPushNotificationTarget(BuildTarget buildTarget, string buildOutputPath)
    {
        iOSConfiguration config = iOSConfiguration.Load();

        if (!config.enableRichPushNotifications)
        {
            return;
        }
        
        string xcodeProjectPath = PBXProject.GetPBXProjectPath(buildOutputPath);
        PBXProject project = new PBXProject();
        project.ReadFromFile(xcodeProjectPath);

        string guidOfInitialTarget = GetTargetGuid(project);

        string pathToInfoPlist = Path.Combine(buildOutputPath, PATH_TO_INFO_PLIST_INSIDE_TARGET);
        PlistDocument mainProjectInfoPlist = new PlistDocument();
        mainProjectInfoPlist.ReadFromFile(pathToInfoPlist);
        PlistElementArray array = mainProjectInfoPlist.root.CreateArray("UIBackgroundModes");
        array.AddString("remote-notification");
        mainProjectInfoPlist.WriteToFile(pathToInfoPlist);
        
        string bundleIdentifierForNotificationService = config.pushNotificationServiceExtensionIdentifier;
        int indexOfLastIdentifierSection = bundleIdentifierForNotificationService.LastIndexOf('.') + 1;
        string displayName = bundleIdentifierForNotificationService.Substring(indexOfLastIdentifierSection);
        
        string pathToNotificationServiceImplementation = Path.Combine(buildOutputPath, displayName);
        
        if (!Directory.Exists(pathToNotificationServiceImplementation))
        {
            Directory.CreateDirectory(pathToNotificationServiceImplementation);
        }

        PlistDocument notificationServicePlist = new PlistDocument();
        string plistTemplatePath = Path.Combine(GetPathToSourceDirectory(), "Info.plist");
        notificationServicePlist.ReadFromFile(plistTemplatePath);
        notificationServicePlist.root.SetString("CFBundleDisplayName", displayName);
        notificationServicePlist.root.SetString("CFBundleIdentifier", bundleIdentifierForNotificationService);
        notificationServicePlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
        notificationServicePlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString());
        
        string pathToNotificationServicePlist = Path.Combine(pathToNotificationServiceImplementation, PATH_TO_INFO_PLIST_INSIDE_TARGET);
        notificationServicePlist.WriteToFile(pathToNotificationServicePlist);
        
        string guidOfExtension = PBXProjectExtensions.AddAppExtension(
            project, guidOfInitialTarget,
            displayName,
            PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + ".notificationservice",
            pathToNotificationServicePlist
        );
        string buildPhaseId = project.AddSourcesBuildPhase(guidOfExtension);

        AddSourceFileToProject(
            project, 
            "NotificationService.h", 
            displayName, 
            guidOfExtension, 
            buildPhaseId, 
            pathToNotificationServiceImplementation
        );
        AddSourceFileToProject(
            project, 
            "NotificationService.m", 
            displayName, 
            guidOfExtension, 
            buildPhaseId, 
            pathToNotificationServiceImplementation
        );
        AddFileToProject(
            project,
            pathToNotificationServicePlist,
            "Info.plist",
            displayName,
            guidOfExtension,
            buildPhaseId
        );
        
        project.AddFrameworkToProject(guidOfExtension, "NotificationCenter.framework", true);
        project.AddFrameworkToProject(guidOfExtension, "UserNotifications.framework", true);
        project.SetBuildProperty(guidOfExtension, "ARCHS", "$(ARCHS_STANDARD");
        project.SetBuildProperty(guidOfExtension, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);

        string[] copyableProperties =
        {
            "IPHONEOS_DEPLOYMENT_TARGET",
            "TARGETED_DEVICE_FAMILY"
        };
        foreach (string copyableProperty in copyableProperties)
        {
            string originalBuildProperty = project.GetBuildPropertyForAnyConfig(guidOfInitialTarget, copyableProperty);
            project.SetBuildProperty(guidOfExtension, copyableProperty, originalBuildProperty);
        }

        project.WriteToFile(xcodeProjectPath);
    }

    private static string GetTargetGuid(PBXProject project)
    {
#if UNITY_2019_3_OR_NEWER
            return project.GetUnityMainTargetGuid();
#else
        return project.TargetGuidByName("Unity-iPhone");
#endif
    }

    private static PlistDocument LoadPlistFromPath(string path)
    {
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(path);
        return plist;
    }

    private static void AddSourceFileToProject(
        PBXProject project, 
        string filename, 
        string extensionDisplayName,
        string extensionGuid, 
        string buildPhase,
        string pathToImplementation
        )
    {
        string sourceFilepath = Path.Combine(GetPathToSourceDirectory(), filename);
        string destinationFilepath = Path.Combine(pathToImplementation, filename);
        if (File.Exists(destinationFilepath))
        {
            File.Delete(destinationFilepath);
        }
        File.Copy(sourceFilepath, destinationFilepath);
        AddFileToProject(project, destinationFilepath, filename, extensionDisplayName, extensionGuid, buildPhase);
    }

    private static string GetPathToSourceDirectory()
    {
        return Path.Combine("Assets", "DeltaDNA", "Editor", "iOS", "NotificationService");
    }

    private static void AddFileToProject(PBXProject project, string filepath, string filename, string extensionDisplayName, string extensionGuid, string buildPhase)
    {
        string xcodePath = extensionDisplayName + "/" + filename;
        string fileGuid = project.AddFile(filepath, xcodePath);
        project.AddFileToBuildSection(extensionGuid, buildPhase, fileGuid);
    }
}
#endif