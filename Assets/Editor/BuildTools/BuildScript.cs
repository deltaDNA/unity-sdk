using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeltaDNA {

	public static class BuildScript {
	
		static void BuildAndroidEclipseProject()
		{
			string[] scenes = { "Assets/DeltaDNA/Example/deltaDNA.unity" };
			string outputPath = "Build/Android";
			BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Development);
		}
		
		static string GetBuildNumber()
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
		
		static string GetDeltaDNAVersion()
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
		
		static string GetDeltaDNAAdsVersion()
		{
			string tempLine;
			string regex = "SMARTADS_VERSION\\s*=\\s*\"(.+)\";";
			string version = "";
			
			using (StreamReader inputReader = new StreamReader("Assets/DeltaDNAAds/DDNASmartAds.cs"))
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
		
		static string BuildOutputFilename(string name, string version, string build)
		{
			return "Build/Unity/"+name+"-"+version+"."+build+".unitypackage";
		}
		
		static void ExportDeltaDNAPackage()
		{
			string[] fixedAssets = {
				"Assets/DeltaDNA",
				"Assets/Plugins/iOS"
			};
			
			string[] androidAssets = Directory.GetFiles("Assets/Plugins/Android", "ddnasdk-v*.aar");
			
			List<string> assets = new List<string>();
			assets.AddRange(fixedAssets);
			assets.AddRange(androidAssets);
		
			string outputFileName = BuildOutputFilename("ddnasdk", GetDeltaDNAVersion(), GetBuildNumber());
			AssetDatabase.ExportPackage(assets.ToArray(), outputFileName, ExportPackageOptions.Recurse);
		}
		
		static void ExportDeltaDNAAdsPackage()
		{
			string[] fixedAssets = {
				"Assets/DeltaDNAAds"
			};
			
			string[] androidAssets = Directory.GetFiles("Assets/Plugins/Android", "ddnasdk-ads-v*.aar");
			
			List<string> assets = new List<string>();
			assets.AddRange(fixedAssets);
			assets.AddRange(androidAssets);
			
			string outputFileName = BuildOutputFilename("ddnasdk-ads", GetDeltaDNAAdsVersion(), GetBuildNumber());
			AssetDatabase.ExportPackage(assets.ToArray(), outputFileName, ExportPackageOptions.Recurse);
		}
	}

}
