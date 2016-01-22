using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeltaDNA {

    public static class BuildScripts {

        static string BuildNumber()
        {
            try {
                Process p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "bash";
                p.StartInfo.Arguments = "-c 'git log --oneline | wc -l | sed -e \"s/[ \t]*//g\"'";
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Console.WriteLine("Exit code: "+p.ExitCode);
                return output.Trim();

            } catch (Exception e) {
                Console.WriteLine("Caught Exception: "+e.Message);
            }

            return "0";
        }

        static string SdkVersion()
        {
            string tempLine;
            string regex = "SDK_VERSION\\s*=\\s*\"(.+)\";";
            string version = "";

            using (StreamReader inputReader = new StreamReader("Assets/DeltaDNA/Helpers/Settings.cs"))
            {
                while (null != (tempLine = inputReader.ReadLine ()))
                {
                    Match m = Regex.Match(tempLine, regex);
                    if (m.Success) {
                        version =  m.Groups[1].ToString();
                    }
                }
            }

            return version.Substring(version.IndexOf('v'));
        }

        static string OutputFilename(string name, string version)
        {
            return name+"-"+version+".unitypackage";
        }

        static void ExportSdkPackage()
        {
            string[] assets = {
                "Assets/DeltaDNA",
                "Assets/Plugins/iOS/DDNAUnityNotificationsPlugin.h",
                "Assets/Plugins/iOS/DDNAUnityNotificationsPlugin.m",
                "Assets/Plugins/iOS/NSString+DDNAHelpers.h",
                "Assets/Plugins/iOS/NSString+DDNAHelpers.m"
            };

            string filename = OutputFilename("deltadna-sdk", SdkVersion());
            AssetDatabase.ExportPackage(assets, filename, ExportPackageOptions.Recurse);
        }

        static void ExportSmartAdsPackage()
        {
            string[] assets = {
                "Assets/DeltaDNAAds",
                "Assets/Plugins/iOS/DDNASmartAdsUnityPlugin.h",
                "Assets/Plugins/iOS/DDNASmartAdsUnityPlugin.m"
            };

            string filename = OutputFilename("deltadna-smartads", SdkVersion());
            AssetDatabase.ExportPackage(assets, filename, ExportPackageOptions.Recurse);
        }

        static void ExportPackages()
        {
            ExportSdkPackage();
            ExportSmartAdsPackage();
        }
    }

}
