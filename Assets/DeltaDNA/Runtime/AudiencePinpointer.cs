using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    public static class AudiencePinpointer
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int ddna_get_tracking_status();

        [DllImport("__Internal")]
        private static extern bool ddna_is_tracking_authorized();
#endif
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
        /// <param name="realCurrencyAmount">The amount of real currency spent in the purchase, in the smallest denomination of that currency (e.g. cents for USD, pence for GBP etc)</param>
        /// <param name="realCurrencyType">The currency code of the currency spent in this purchase (e.g. USD for US Dollars)</param>
        /// <param name="transactionID">The Apple transaction ID for this purchase as received from Apple's StoreKit API</param>
        /// <param name="transactionReceipt">The receipt data (base64 encoded) as received from Apple's StoreKit API</param>
        public static void RecordPurchaseEvent(int realCurrencyAmount,
                                               string realCurrencyType,
                                               string transactionID,
                                               string transactionReceipt)
        {
#if UNITY_IOS
            if (CheckForRequiredFields())
            {
                PinpointerEvent signalTrackingEvent = new PinpointerEvent("unitySignalPurchase");

                signalTrackingEvent.AddParam("realCurrencyAmount", realCurrencyAmount);
                signalTrackingEvent.AddParam("realCurrencyType", realCurrencyType);
                signalTrackingEvent.AddParam("transactionID", transactionID);

                DDNA.Instance.RecordEvent(signalTrackingEvent);

                if (DDNA.Instance.Settings.AutomaticallyGenerateTransactionForAudiencePinpointer) {
                    Transaction transactionEvent = new Transaction("Pinpointer Signal Transaction", "PURCHASE", new Product(), new Product());
                    transactionEvent.SetReceipt(transactionReceipt);
                    transactionEvent.SetTransactionId(transactionID);

                    DDNA.Instance.RecordEvent(transactionEvent);
                }
            }
#endif
        }

        private static bool CheckForRequiredFields()
        {
            if (String.IsNullOrEmpty(DDNA.Instance.AppleDeveloperID) || String.IsNullOrEmpty(DDNA.Instance.AppStoreID))
            {
                Debug.LogWarning("Pinpointer signal events require an Apple developer ID and App Store ID to be " +
                                 "registered with the DeltaDNA SDK. Please refer to the user guide for more information. " +
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

                AddParam("attTrackingStatus", ddna_get_tracking_status());

                bool idfaPresent = ddna_is_tracking_authorized();

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
