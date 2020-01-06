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
        public static string Platform {
            get { return platform ?? (platform = GetPlatform()); }
        }

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

        private static bool RuntimePlatformIs(string platformName)
        {
            return Enum.IsDefined(typeof(RuntimePlatform), platformName) && Application.platform.ToString() == platformName;
        }

        private static float ScreenSizeInches()
        {
            var x = Screen.width / Screen.dpi;
            var y = Screen.height / Screen.dpi;
            return (float)Math.Sqrt(x * x + y * y);
        }

        private static bool IsTablet()
        {
            return ScreenSizeInches() > 6;
        }

        /// <summary>
        /// Gets the platform as an enumeration of the 'platform' key.
        /// </summary>
        /// <returns>The platform.</returns>
        private static string GetPlatform()
        {
            if (RuntimePlatformIs("OSXEditor") ||
                RuntimePlatformIs("OSXPlayer")) return DeltaDNA.Platform.MAC_CLIENT;
            if (RuntimePlatformIs("WindowsEditor") ||
                RuntimePlatformIs("WindowsPlayer")) return DeltaDNA.Platform.PC_CLIENT;
            if (RuntimePlatformIs("OSXWebPlayer")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("OSXDashboardPlayer")) return DeltaDNA.Platform.MAC_CLIENT;
            if (RuntimePlatformIs("WindowsWebPlayer")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("IPhonePlayer")) return DeltaDNA.Platform.IOS;
            if (RuntimePlatformIs("PS3")) return DeltaDNA.Platform.PS3;
            if (RuntimePlatformIs("XBOX360")) return DeltaDNA.Platform.XBOX360;
            if (RuntimePlatformIs("Android")) return DeltaDNA.Platform.ANDROID;
            if (RuntimePlatformIs("NaCL")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("LinuxEditor") ||
                RuntimePlatformIs("LinuxPlayer")) return DeltaDNA.Platform.PC_CLIENT;
            if (RuntimePlatformIs("WebGLPlayer")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("FlashPlayer")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("MetroPlayerX86") ||
                RuntimePlatformIs("MetroPlayerX64") ||
                RuntimePlatformIs("MetroPlayerARM") ||
                RuntimePlatformIs("WSAPlayerX86") ||
                RuntimePlatformIs("WSAPlayerX64") ||
                RuntimePlatformIs("WSAPlayerARM")) {
                // Metro Apps can run anywhere...
                if (SystemInfo.deviceType == UnityEngine.DeviceType.Handheld) {
                    return DeltaDNA.Platform.WINDOWS_MOBILE;
                } else {
                    return DeltaDNA.Platform.PC_CLIENT;
                }
            }
            if (RuntimePlatformIs("WP8Player")) return DeltaDNA.Platform.WINDOWS_MOBILE;
            if (RuntimePlatformIs("BB10Player") ||
                RuntimePlatformIs("BlackBerryPlayer")) return DeltaDNA.Platform.BLACKBERRY;
            if (RuntimePlatformIs("TizenPlayer")) return DeltaDNA.Platform.ANDROID;
            if (RuntimePlatformIs("PSP2")) return DeltaDNA.Platform.PSVITA;
            if (RuntimePlatformIs("PS4")) return DeltaDNA.Platform.PS4;
            if (RuntimePlatformIs("PSMPlayer")) return DeltaDNA.Platform.WEB;
            if (RuntimePlatformIs("XboxOne")) return DeltaDNA.Platform.XBOXONE;
            if (RuntimePlatformIs("SamsungTVPlayer")) return DeltaDNA.Platform.ANDROID;
            if (RuntimePlatformIs("tvOS")) return DeltaDNA.Platform.IOS_TV;
            if (RuntimePlatformIs("WiiU")) return DeltaDNA.Platform.WIIU;
            if (RuntimePlatformIs("Switch")) return DeltaDNA.Platform.SWITCH;

            return DeltaDNA.Platform.UNKNOWN;
        }

        private static string GetDeviceName()
        {
            // We attempt to create a friendly name from the device model
            // else return the raw string.

            string name = SystemInfo.deviceModel;
            switch (name) {

                // Apple
                // https://www.theiphonewiki.com/wiki/Models

                case "iPhone1,1": return @"iPhone 1G";
                case "iPhone1,2": return @"iPhone 3G";
                case "iPhone2,1": return @"iPhone 3GS";
                case "iPhone3,1": return @"iPhone 4";
                case "iPhone3,2": return @"iPhone 4";
                case "iPhone3,3": return @"iPhone 4";
                case "iPhone4,1": return @"iPhone 4S";
                case "iPhone5,1": return @"iPhone 5";
                case "iPhone5,2": return @"iPhone 5";
                case "iPhone5,3": return @"iPhone 5C";
                case "iPhone5,4": return @"iPhone 5C";
                case "iPhone6,1": return @"iPhone 5S";
                case "iPhone6,2": return @"iPhone 5S";
                case "iPhone7,2": return @"iPhone 6";
                case "iPhone7,1": return @"iPhone 6 Plus";
                case "iPhone8,1": return @"iPhone 6s";
                case "iPhone8,2": return @"iPhone 6s Plus";
                case "iPhone8,4": return @"iPhone SE";
                case "iPhone9,1": return @"iPhone 7";
                case "iPhone9,2": return @"iPhone 7 Plus";
                case "iPhone9,3": return @"iPhone 7";
                case "iPhone9,4": return @"iPhone 7 Plus";
                case "iPhone10,1": return @"iPhone 8";
                case "iPhone10,4": return @"iPhone 8";
                case "iPhone10,2": return @"iPhone 8 Plus";
                case "iPhone10,5": return @"iPhone 8 Plus";
                case "iPhone10,3": return @"iPhone X";
                case "iPhone10,6": return @"iPhone X";
                case "iPhone11,2": return @"iPhone XS";
                case "iPhone11,4": return @"iPhone XS Max";
                case "iPhone11,6": return @"iPhone XS Max";
                case "iPhone11,8": return @"iPhone XR";
                case "iPhone12,1": return @"iPhone 11";
                case "iPhone12,3": return @"iPhone 11 Pro";
                case "iPhone12,5": return @"iPhone 11 Pro Max";

                case "iPod1,1": return @"iPod Touch 1G";
                case "iPod2,1": return @"iPod Touch 2G";
                case "iPod3,1": return @"iPod Touch 3G";
                case "iPod4,1": return @"iPod Touch 4G";
                case "iPod5,1": return @"iPod Touch 5G";
                case "iPod7,1": return @"iPod Touch 6G";

                case "iPad1,1": return @"iPad 1G";
                case "iPad2,1": return @"iPad 2";
                case "iPad2,2": return @"iPad 2";
                case "iPad2,3": return @"iPad 2";
                case "iPad2,4": return @"iPad 2";
                case "iPad2,5": return @"iPad Mini 1G";
                case "iPad2,6": return @"iPad Mini 1G";
                case "iPad2,7": return @"iPad Mini 1G";
                case "iPad3,1": return @"iPad 3G";
                case "iPad3,2": return @"iPad 3G";
                case "iPad3,3": return @"iPad 3G";
                case "iPad3,4": return @"iPad 4G";
                case "iPad3,5": return @"iPad 4G";
                case "iPad3,6": return @"iPad 4G";
                case "iPad4,1": return @"iPad Air";
                case "iPad4,2": return @"iPad Air";
                case "iPad4,3": return @"iPad Air";
                case "iPad4,4": return @"iPad Mini 2G";
                case "iPad4,5": return @"iPad Mini 2G";
                case "iPad4,6": return @"iPad Mini 2G";
                case "iPad4,7": return @"iPad Mini 3";
                case "iPad4,8": return @"iPad Mini 3";
                case "iPad4,9": return @"iPad Mini 3";
                case "iPad5,1": return @"iPad Mini 4";
                case "iPad5,2": return @"iPad Mini 4";
                case "iPad11,1": return @"iPad Mini 5";
                case "iPad11,2": return @"iPad Mini 5";
                
                case "iPad5,3": return @"iPad Air 2";
                case "iPad5,4": return @"iPad Air 2";
                case "iPad6,7": return @"iPad Pro 12.9";
                case "iPad6,8": return @"iPad Pro 12.9";
                case "iPad6,3": return @"iPad Pro 9.7";
                case "iPad6,4": return @"iPad Pro 9.7";
                case "iPad6,11": return @"iPad 5G";
                case "iPad6,12": return @"iPad 5G";
                case "iPad7,1": return @"iPad Pro 12.9 2G";
                case "iPad7,2": return @"iPad Pro 12.9 2G";
                case "iPad7,3": return @"iPad Pro 10.5";
                case "iPad7,4": return @"iPad Pro 10.5";
                case "iPad7,5": return @"iPad 6G";
                case "iPad7,6": return @"iPad 6G";
                case "iPad7,11": return @"iPad 7G";
                case "iPad7,12": return @"iPad 7G";
                case "iPad8,1": return @"iPad Pro 11 3G";
                case "iPad8,2": return @"iPad Pro 11 3G";
                case "iPad8,3": return @"iPad Pro 11 3G";
                case "iPad8,4": return @"iPad Pro 11 3G";
                case "iPad8,5": return @"iPad Pro 12.9 3G";
                case "iPad8,6": return @"iPad Pro 12.9 3G";
                case "iPad8,7": return @"iPad Pro 12.9 3G";
                case "iPad8,8": return @"iPad Pro 12.9 3G";
           


                // Amazon
                // https://developer.amazon.com/docs/fire-tablets/ft-device-and-feature-specifications.html

                case "Amazon KFSAWA": return "Fire HDX 8.9 (4th Gen)";
                case "Amazon KFASWI": return "Fire HD 7 (4th Gen)";
                case "Amazon KFARWI": return "Fire HD 6 (4th Gen)";
                case "Amazon KFAPWA": // fall through
                case "Amazon KFAPWI": return "Kindle Fire HDX 8.9 (3rd Gen)";
                case "Amazon KFTHWA": // fall through
                case "Amazon KFTHWI": return "Kindle Fire HDX 7 (3rd Gen)";
                case "Amazon KFSOWI": return "Kindle Fire HD 7 (3rd Gen)";
                case "Amazon KFJWA": // fall through
                case "Amazon KFJWI": return "Kindle Fire HD 8.9 (2nd Gen)";
                case "Amazon KFTT": return "Kindle Fire HD 7 (2nd Gen)";
                case "Amazon KFOT": return "Kindle Fire (2nd Gen)";
                case "Amazon Kindle Fire": return "Kindle Fire (1st Gen)";
                case "Amazon KFGIWI": return "Fire HD 8 (2016)";
                case "Amazon KFDOWI": return "Fire HD 8 (2017)";
                case "Amazon KFAUWI": return "Fire 7 (2017)";
                case "Amazon KFSUWI": return "Fire HD 10 (2017)";
                case "Amazon KFKAWI": return "Fire HD 8 (2018)";
                case "Amazon KFMUWI": return "Fire 7 (2019)";
                case "Amazon KFMAWI": return "Fire HD 10 (2019)";

                default:
                    return Trim(name, 72);
            }
        }
        private static string GetDeviceModel()
        {
            return Trim(SystemInfo.deviceModel, 72);
        }

        /// <summary>
        /// Gets the type of the device as an enumeration of 'deviceType'.
        /// </summary>
        /// <returns>The device type.</returns>
        private static string GetDeviceType()
        {
            if (RuntimePlatformIs("SamsungTVPlayer")) return "TV";
            if (RuntimePlatformIs("tvOS")) return "TV";

            switch (SystemInfo.deviceType)
            {
                case UnityEngine.DeviceType.Console: return "CONSOLE";
                case UnityEngine.DeviceType.Desktop: return "PC";
                case UnityEngine.DeviceType.Handheld:
                {
                    string model = SystemInfo.deviceModel;
                    if (model.StartsWith("iPhone")) return "MOBILE_PHONE";
                    if (model.StartsWith("iPad")) return "TABLET";
                    return IsTablet() ? "TABLET" : "MOBILE_PHONE";
                }
                default: return "UNKNOWN";
            }
        }

        private static string GetOperatingSystem()
        {
            if (RuntimePlatformIs("tvOS")) return "TVOS";

            // Unity gives a string with os plus version.  It's not documented
            // how this string is generated but I can have a good guess.
            string os = SystemInfo.operatingSystem.ToUpper();
            if (os.Contains("WINDOWS")) return "WINDOWS";
            if (os.Contains("OSX")) return "OSX";
            if (os.Contains("MAC")) return "OSX";
            if (os.Contains("IOS") || os.Contains("IPHONE") || os.Contains("IPAD")) return "IOS";
            if (os.Contains("LINUX")) return "LINUX";
            if (os.Contains("ANDROID")) {
                if (SystemInfo.deviceModel.ToUpper().Contains("AMAZON")) {
                    return "FIREOS";
                }
                return "ANDROID";
            }
            if (os.Contains("BLACKBERRY")) return "BLACKBERRY";
            return "UNKNOWN";
        }

        private static string GetOperatingSystemVersion()
        {
            try {
                const string pattern = @"[\d|\.]+";
                Regex regex = new Regex(pattern);
                string os = SystemInfo.operatingSystem;
                Match match = regex.Match(os);
                if (match.Success) return match.Groups[0].ToString();
                return "";
            } catch (Exception) {
                return null;
            }
        }

        private static string GetManufacturer()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            return Trim(
                new AndroidJavaObject("android.os.Build").GetStatic<string>("MANUFACTURER"),
                72);
            #elif UNITY_IOS && !UNITY_EDITOR
            return "Apple Inc";
            #else
            return null;
            #endif
        }

        private static string GetCurrentTimezoneOffset() {
            var currentDate = DateTime.Now;
            var currentOffset = TimeSpan.Zero;

            var retrieved = false;
            try {
                if (TimeZoneInfo.Local != null) {
                    currentOffset = TimeZoneInfo.Local.GetUtcOffset(currentDate);
                    retrieved = true;
                }
            } catch (TimeZoneNotFoundException) {
            } catch (NullReferenceException) {
                // happens in non-GMT timezones in the macOS Player
            }

            if (!retrieved) {
                try {
                    if (TimeZone.CurrentTimeZone != null) {
                        currentOffset = TimeZone.CurrentTimeZone.GetUtcOffset(currentDate);
                        retrieved = true;
                    }
                } catch (TimeZoneNotFoundException) {}
            }

            if (!retrieved) {
                Debug.LogWarning("Failed to retrieve timezone offset");
            }

            return string.Format(
                "{0}{1:D2}",
                currentOffset.Hours >= 0 ? "+" : "",
                currentOffset.Hours);
        }

        private static string GetCountryCode()
        {
            // Not supported in Unity.
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
                default: return "en";   // English...
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

        private static string Trim(string value, int length) {
            if (value == null) return null;

            return value.Substring(0, Math.Min(value.Length, length));
        }

        #endregion
    }
}
