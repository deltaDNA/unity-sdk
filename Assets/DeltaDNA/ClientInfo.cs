using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	static class ClientInfo
	{
		private static string platform = null;
	
		public static string Platform { get { return platform ?? (platform = GetPlatform()); }}
		
		private static string deviceName = null;
		
		public static string DeviceName { get { return deviceName ?? (deviceName = GetDeviceName()); }}
		
		private static string deviceModel = null;
		
		public static string DeviceModel { get { return deviceModel ?? (deviceModel = GetDeviceModel()); }}
		
		private static string deviceType = null;
		
		public static string DeviceType { get { return deviceType ?? (deviceType = GetDeviceType()); }}
		
		private static string operatingSystem = null;
		
		public static string OperatingSystem { get { return operatingSystem ?? (operatingSystem = GetOperatingSystem()); }}
		
		private static string operatingSystemVersion = null;
		
		public static string OperatingSystemVersion { get { return operatingSystemVersion ?? (operatingSystemVersion = GetOperatingSystemVersion()); }}
		
		private static string manufacturer = null;
		
		public static string Manufacturer { get { return manufacturer ?? (manufacturer = GetManufacturer()); }}
		
		
	
		/// <summary>
		/// Gets the platform as an enumeration of the 'platform' key.
		/// </summary>
		/// <returns>The platform.</returns>
		private static string GetPlatform()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.Android: return "ANDROID";
				case RuntimePlatform.BlackBerryPlayer: return "BLACKBERRY_MOBILE";
				case RuntimePlatform.FlashPlayer: return "WEB";
				case RuntimePlatform.IPhonePlayer:
				{
					switch (UnityEngine.iPhone.generation)
					{
					case iPhoneGeneration.iPad1Gen:
					case iPhoneGeneration.iPad2Gen:
					case iPhoneGeneration.iPad3Gen:
					case iPhoneGeneration.iPad4Gen:
					case iPhoneGeneration.iPad5Gen:
					case iPhoneGeneration.iPadMini1Gen:
					case iPhoneGeneration.iPadMini2Gen: return "IOS_TABLET";
					case iPhoneGeneration.iPadUnknown: return "IOS";
					default: return "IOS_MOBILE";
					}
				}
				case RuntimePlatform.LinuxPlayer: return "PC_CLIENT";
				case RuntimePlatform.MetroPlayerARM: return "WINDOWS_TABLET";
				case RuntimePlatform.MetroPlayerX64: return "WINDOWS_TABLET";
				case RuntimePlatform.MetroPlayerX86: return "WINDOWS_TABLET";
				case RuntimePlatform.NaCl: return "WEB";
				case RuntimePlatform.OSXDashboardPlayer: return "MAC_CLIENT";
				case RuntimePlatform.OSXEditor: return "MAC_CLIENT";
				case RuntimePlatform.OSXPlayer: return "MAC_CLIENT";
				case RuntimePlatform.OSXWebPlayer: return "WEB";
				case RuntimePlatform.PS3: return "PS3";
				case RuntimePlatform.PS4: return "PS4";
				case RuntimePlatform.PSMPlayer: return "WEB";
				case RuntimePlatform.PSP2: return "PSVITA";
				case RuntimePlatform.SamsungTVPlayer: return "ANDROID";
				case RuntimePlatform.TizenPlayer: return "ANDROID";
				case RuntimePlatform.WindowsEditor: return "PC_CLIENT";
				case RuntimePlatform.WindowsPlayer: return "PC_CLIENT";
				case RuntimePlatform.WindowsWebPlayer: return "WEB";
				case RuntimePlatform.WP8Player: return "WINDOWS_MOBILE";
				case RuntimePlatform.XBOX360: return "XBOX360";
				case RuntimePlatform.XboxOne: return "XBOXONE";
				default:
				{
					Debug.LogWarning("Unsupported platform '"+Application.platform+"' returning UNKNOWN");
					return "UNKNOWN";
				}
			}
		}
		
		private static string GetDeviceName()
		{
			return ClientInfo.deviceName;
		}
		
		private static string GetDeviceModel()
		{
			return ClientInfo.deviceModel;
		}
		
		/// <summary>
		/// Gets the type of the device as an enumeration of 'deviceType'.
		/// </summary>
		/// <returns>The device type.</returns>
		private static string GetDeviceType()
		{
			switch (SystemInfo.deviceType)
			{
				case UnityEngine.DeviceType.Console: return "CONSOLE";
				case UnityEngine.DeviceType.Desktop: return "PC";
				case UnityEngine.DeviceType.Handheld: return "HANDHELD";
				case UnityEngine.DeviceType.Unknown: 
				{
					if (Application.platform == RuntimePlatform.SamsungTVPlayer) return "TV";
					return "UNKOWN";
				}
				default: return "UNKNOWN";
			}
		}
		
		private static string GetOperatingSystem()
		{
			// Unity gives a string with os plus version.  It's not documented 
			// how this string is generated but I can have a good guess.
			string os = SystemInfo.operatingSystem.ToUpper();
			if (os.Contains("WINDOWS")) return "WINDOWS";
			if (os.Contains("OSX")) return "OSX";
			if (os.Contains("MAC")) return "OSX";
			if (os.Contains("IOS")) return "IOS";
			if (os.Contains("LINUX")) return "LINUX";
			if (os.Contains("ANDROID")) return "ANDROID";
			if (os.Contains("BLACKBERRY")) return "BLACKBERRY";
			return "UNKNOWN";
		}
		
		private static string GetOperatingSystemVersion()
		{
			string pattern = @"^\w+";
			Regex regex = new Regex(pattern);
			string os = ClientInfo.operatingSystem;
			return regex.Replace(os, "").Trim();	// stripping out words should leave a version number
		}
		
		private static string GetManufacturer()
		{
			return "UNKNOWN";
		}
	}
}

