using System;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    public static class AudiencePinpointer
    {
        /// <summary>
        /// Record this event when a new gameplay session is started.
        /// </summary>
        public static void RecordSessionEvent()
        {
#if UNITY_IOS
            if (CheckForRequiredFields())
            {
                PinpointerEvent signalTrackingEvent = new PinpointerEvent("unitySignalSession");
                DDNA.Instance.RecordEvent(signalTrackingEvent);;
            }
#endif
        }
        
        /// <summary>
        /// Record this event when a new install occurs.
        /// </summary>
        public static void RecordInstallEvent()
        {
#if UNITY_IOS
            if (CheckForRequiredFields())
            {
                PinpointerEvent signalTrackingEvent =  new PinpointerEvent("unitySignalInstall");

                DDNA.Instance.RecordEvent(signalTrackingEvent);;
            }
#endif
        }

        /// <summary>
        /// Record this event when an in-app purchase was made with real money.
        /// </summary>
        /// <param name="realCurrencyAmount"></param>
        /// <param name="realCurrencyType"></param>
        public static void RecordPurchaseEvent(int realCurrencyAmount,
                                               string realCurrencyType)
        {
#if UNITY_IOS
            if (CheckForRequiredFields())
            {
                PinpointerEvent signalTrackingEvent = new PinpointerEvent("unitySignalPurchase");

                signalTrackingEvent.AddParam("realCurrencyAmount", realCurrencyAmount);
                signalTrackingEvent.AddParam("realCurrencyType", realCurrencyType);

                DDNA.Instance.RecordEvent(signalTrackingEvent);
            }
#endif
        }

        private static bool CheckForRequiredFields()
        {
            if (String.IsNullOrEmpty(DDNA.Instance.AppleDeveloperID) || String.IsNullOrEmpty(DDNA.Instance.AppStoreID))
            {
                Debug.LogWarning("Pinpointer signal events require an Apple developer ID and App Store ID to be" +
                                 "registered with the DeltaDNA SDK. Please refer to the user guide for more information." +
                                 "The event has not been sent.");
                return false;
            }
            return true;
        }

#if UNITY_IOS
        private class PinpointerEvent : GameEvent<PinpointerEvent>
        {
            internal PinpointerEvent(string name) : base(name)
            {
                AddParam("deviceName", SystemInfo.deviceModel);
                AddParam("connectionType", GetConnectionType());
                AddParam("platform", ClientInfo.Platform);
                AddParam("sdkVersion", Settings.SDK_VERSION);

                // Returns null, not supported in Unity
                // AddParam("userCountry", ClientInfo.CountryCode);

                AddParam("appStoreID", DDNA.Instance.AppStoreID);
                AddParam("appBundleID", Application.identifier);
                AddParam("appDeveloperID", DDNA.Instance.AppleDeveloperID);

                bool idfaPresent = UnityEngine.iOS.Device.advertisingTrackingEnabled;

                AddParam("idfv", UnityEngine.iOS.Device.vendorIdentifier);
                if (idfaPresent)
                {
                    AddParam("idfa", UnityEngine.iOS.Device.advertisingIdentifier);
                }

                AddParam("limitedAdTracking", !idfaPresent);

                AddParam("privacyPermissionAds", false);
                AddParam("privacyPermissionExternal", false);
                AddParam("privacyPermissionGameExp", false);
                AddParam("privacyPermissionProfiling", false);
            }
        }

        private static string GetConnectionType()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "wifi";
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "cellular";
                default:
                    return "unknown";
            }
        }
#endif
    }
}
