using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	/// <summary>
	/// The ClientInfo class determines facts about the device the game is being played on.  The 
	/// results are formatted to be valid game parameter values.
	/// </summary>
	static class ClientInfo
	{
		private static string platform = null;
	
		/// <summary>
		/// The platform the game is being played on.
		/// </summary>
		public static string Platform { get { return platform ?? (platform = GetPlatform()); }}
		
		private static string deviceName = null;

		/// <summary>
		/// The name of the device.
		/// </summary>		
		public static string DeviceName { get { return deviceName ?? (deviceName = GetDeviceName()); }}
		
		private static string deviceModel = null;
		
		/// <summary>
		/// The device's model number.
		/// </summary>
		/// <value>The device model.</value>
		public static string DeviceModel { get { return deviceModel ?? (deviceModel = GetDeviceModel()); }}
		
		private static string deviceType = null;
		
		/// <summary>
		/// The type of device.
		/// </summary>
		public static string DeviceType { get { return deviceType ?? (deviceType = GetDeviceType()); }}
		
		private static string operatingSystem = null;
		
		/// <summary>
		/// The operating system the device is running.
		/// </summary>
		public static string OperatingSystem { get { return operatingSystem ?? (operatingSystem = GetOperatingSystem()); }}
		
		private static string operatingSystemVersion = null;

		/// <summary>
		/// The version of the operating system the device is running.
		/// </summary>		
		public static string OperatingSystemVersion { get { return operatingSystemVersion ?? (operatingSystemVersion = GetOperatingSystemVersion()); }}
		
		private static string manufacturer = null;
		
		/// <summary>
		/// The manufacturer of the device the game is running on.
		/// </summary>
		public static string Manufacturer { get { return manufacturer ?? (manufacturer = GetManufacturer()); }}	
		
		private static string timezoneOffset = null;
		
		/// <summary>
		/// The timezone offset from UTC the device is set to.
		/// </summary>
		public static string TimezoneOffset { get { return timezoneOffset ?? (timezoneOffset = GetCurrentTimezoneOffset()); }}
		
		private static string countryCode = null;
		
		/// <summary>
		/// The country code the device is set to.
		/// </summary>
		public static string CountryCode { get { return countryCode ?? (countryCode = GetCountryCode()); }}
		
		private static string languageCode = null;
		
		/// <summary>
		/// The language code the device is set to.
		/// </summary>
		/// <value>The language code.</value>
		public static string LanguageCode { get { return languageCode ?? (languageCode = GetLanguageCode()); }}
		
		private static string locale = null;
		
		/// <summary>
		/// The locale of the device in.
		/// </summary>
		/// <value>The locale.</value>
		public static string Locale { get { return locale ?? (locale = GetLocale()); }}
		
		#region Private Helpers
	
		/// <summary>
		/// Gets the platform as an enumeration of the 'platform' key.
		/// </summary>
		/// <returns>The platform.</returns>
		private static string GetPlatform()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.Android: return "ANDROID";
				case RuntimePlatform.FlashPlayer: return "WEB";
				
				#if UNITY_IPHONE
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
				#endif
				
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
				case RuntimePlatform.TizenPlayer: return "ANDROID";
				case RuntimePlatform.WindowsEditor: return "PC_CLIENT";
				case RuntimePlatform.WindowsPlayer: return "PC_CLIENT";
				case RuntimePlatform.WindowsWebPlayer: return "WEB";
				case RuntimePlatform.WP8Player: return "WINDOWS_MOBILE";
				case RuntimePlatform.XBOX360: return "XBOX360";

				#if UNITY_4_5 || UNITY_4_5_1
				case RuntimePlatform.PS4: return "PS4";
				case RuntimePlatform.PSMPlayer: return "WEB";
				case RuntimePlatform.PSP2: return "PSVITA";
				case RuntimePlatform.SamsungTVPlayer: return "ANDROID";
				case RuntimePlatform.XboxOne: return "XBOXONE";
				case RuntimePlatform.BlackBerryPlayer: return "BLACKBERRY_MOBILE";
				#endif

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
					#if UNITY_4_5 || UNITY_4_5_1
					if (Application.platform == RuntimePlatform.SamsungTVPlayer) return "TV";
					#endif
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
			string os = SystemInfo.operatingSystem;
			return regex.Replace(os, "").Trim();	// stripping out words should leave a version number
		}
		
		private static string GetManufacturer()
		{
			return null;
		}
		
		private static string GetCurrentTimezoneOffset()
		{
			try
			{
				// WP8 doesn't support the 'old' way of getting the time zone.
				#if UNITY_WP8
				TimeSpan currentOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
				#else
				TimeZone localZone = TimeZone.CurrentTimeZone;
				DateTime currentDate = DateTime.Now;
				TimeSpan currentOffset = localZone.GetUtcOffset(currentDate);
				#endif
				return String.Format("{0}{1:D2}", currentOffset.Hours >= 0 ? "+" : "", currentOffset.Hours);
			}
			catch (Exception)
			{
				return null;
			}
		}
		
		private static string GetCountryCode()
		{
			return null;
		}
		
		private static string GetLanguageCode()
		{
			switch (Application.systemLanguage)
			{
				case SystemLanguage.Afrikaans: return "af";
				case SystemLanguage.Arabic: return "ar";
				case SystemLanguage.Basque: return "eu";
				case SystemLanguage.Belarusian: return "be";
				case SystemLanguage.Bulgarian: return "bg";
				case SystemLanguage.Catalan: return "ca";
				case SystemLanguage.Chinese: return "zh";
				case SystemLanguage.Czech: return "cs";
				case SystemLanguage.Danish: return "da";
				case SystemLanguage.Dutch: return "nl";
				case SystemLanguage.English: return "en";
				case SystemLanguage.Estonian: return "et";
				case SystemLanguage.Faroese: return "fo";
				case SystemLanguage.Finnish: return "fi";
				case SystemLanguage.French: return "fr";
				case SystemLanguage.German: return "de";
				case SystemLanguage.Greek: return "el";
				case SystemLanguage.Hebrew: return "he";
				case SystemLanguage.Hungarian: return "hu";
				case SystemLanguage.Icelandic: return "is";
				case SystemLanguage.Indonesian: return "id";
				case SystemLanguage.Italian: return "it";
				case SystemLanguage.Japanese: return "ja";
				case SystemLanguage.Korean: return "ko";
				case SystemLanguage.Latvian: return "lv";
				case SystemLanguage.Lithuanian: return "lt";
				case SystemLanguage.Norwegian: return "nn";
				case SystemLanguage.Polish: return "pl";
				case SystemLanguage.Portuguese: return "pt";
				case SystemLanguage.Romanian: return "ro";
				case SystemLanguage.Russian: return "ru";
				case SystemLanguage.SerboCroatian: return "sr";
				case SystemLanguage.Slovak: return "sk";
				case SystemLanguage.Slovenian: return "sl";
				case SystemLanguage.Spanish: return "es";
				case SystemLanguage.Swedish: return "sv";
				case SystemLanguage.Thai: return "th";
				case SystemLanguage.Turkish: return "tr";
				case SystemLanguage.Ukrainian: return "uk";
				case SystemLanguage.Vietnamese: return "vi";
				default: return "en";	// English...
			}
		}
		
		private static string GetLocale()
		{
			if (CountryCode != null)
			{
				return String.Format("{0}_{1}", LanguageCode, CountryCode);
			}
			else
			{
				return String.Format("{0}_ZZ", LanguageCode);
			}
		}
		
		#endregion
	}
}

