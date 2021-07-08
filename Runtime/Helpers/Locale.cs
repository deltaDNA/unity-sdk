using System.Globalization;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DeltaDNA
{
    internal static class Locale
    {
        
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string DDNA_current_culture_language_code();
        [DllImport("__Internal")]
        private static extern string DDNA_current_culture_country_code();

        /// <summary>
        /// Returns the current country code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentCountryCode()
        {
            //  we would need sysglobl.dll to create any unknown CultureInfo, and that dll may not be available on our targets
            // as such, returning the strings directly is simpler for us, and CultureInfo will throw for unknown cultures
            return DDNA_current_culture_country_code();
        }

        /// <summary>
        /// Returns the current language code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentLanguageCode()
        {
            return DDNA_current_culture_language_code();
        }
        
#elif UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Returns the current country code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentCountryCode()
        {
            var localeClass = new AndroidJavaClass("java.util.Locale");
            var defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            var country = defaultLocale.Call<string>("getCountry");
            return country;
        }

        /// <summary>
        /// Returns the current language code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentLanguageCode()
        {
            var localeClass = new AndroidJavaClass("java.util.Locale");
            var defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            var language = defaultLocale.Call<string>("getLanguage");
            return language;
        }
#else


        /// <summary>
        /// Returns the current country code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentCountryCode()
        {
            // Not supported in Unity
            return null;
        }

        /// <summary>
        /// Returns the current language code from the culture info. Invokes native method on Android and iOS.
        /// </summary>
        /// <returns>Country Code as a string</returns>
        public static string CurrentLanguageCode()
        {
            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

#endif
        
    }
}
