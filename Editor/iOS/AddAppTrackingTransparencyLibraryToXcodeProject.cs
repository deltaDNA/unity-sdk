using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace DeltaDNA.Editor
{
    public sealed class AddAppTrackingTransparencyLibraryToXcodeProject : ScriptableObject
    {
#if UNITY_IOS
        [PostProcessBuild(3)]
        public static void AddAppTrackingTransparency(BuildTarget target, string path)
        {
            Debug.Log("Adding required frameworks to iOS build");
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            
#if UNITY_2019_3_OR_NEWER
            string targetID = project.GetUnityFrameworkTargetGuid();
#else
            string targetID = project.TargetGuidByName("Unity-iPhone");
#endif
            
            project.AddFrameworkToProject(targetID, "AppTrackingTransparency.framework", false);
            
            project.AddBuildProperty(targetID, "OTHER_LDFLAGS", "-ObjC");
            
            File.WriteAllText(projectPath, project.WriteToString());
        }
#endif
    }
}