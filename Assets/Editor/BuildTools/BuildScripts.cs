//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
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
                "Assets/DeltaDNAAds"
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
