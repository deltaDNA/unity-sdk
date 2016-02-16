using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

/**
 *  Add support for CocoaPods to the Unity generated XCode project.  Run pods.command from the
 *  build folder to install ad network dependencies.
 */
public class PostProcessBuild {

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {
        Debug.Log("OnPostprocessBuild "+buildTarget+" "+path);

        if (buildTarget == BuildTarget.iOS) {

            string projectPath = PBXProject.GetPBXProjectPath(path);

            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            Debug.Log(PBXProject.GetUnityTargetName());
            string target = project.TargetGuidByName(PBXProject.GetUnityTargetName());

            Debug.Log(target);

            // Add build settings for CocoaPods
            project.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_CFLAGS", "$(inherited)");
            project.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");

            // Copy Podfile into project
            string unityProjectRootPath = Path.GetFullPath("./").Normalize();
            Debug.Log(unityProjectRootPath);

            // Default src location is the project root
            FileUtil.ReplaceFile("Assets/DeltaDNAAds/Editor/iOS/Podfile", path + "/Podfile");

            // Update the XCode project on disk
            project.WriteToFile(projectPath);
            
            // Useful automation...

            // Run pod update
            FileUtil.ReplaceFile("Assets/DeltaDNAAds/Editor/iOS/pods.command", path + "/pods.command");
        }

    }
}
