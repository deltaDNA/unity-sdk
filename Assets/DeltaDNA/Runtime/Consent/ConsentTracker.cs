using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DeltaDNA.Consent
{
    internal enum ConsentStatus
    {
        Unknown, NotRequired, RequiredButUnchecked, ConsentGiven, ConsentDenied
    }

    public class ConsentCheckException : Exception
    {
        public ConsentCheckException(string message): base(message) {}
    }

    internal class ConsentTracker
    {
        private ConsentStatus m_PiplUseStatus = ConsentStatus.Unknown;
        private ConsentStatus m_PiplExportStatus = ConsentStatus.Unknown;

        private static string s_PiplUseSavedStatusKey = "ddnaPiplUseConsent";
        private static string s_PiplExportSavedStatusKey = "ddnaPiplExportConsent";

        public bool PiplUseConsentGiven => m_PiplUseStatus == ConsentStatus.ConsentGiven;
        public bool PiplExportConsentGiven => m_PiplExportStatus == ConsentStatus.ConsentGiven;

        private Action<bool> m_CurrentConsentCheckCallback;

        internal ConsentTracker()
        {
            if (PlayerPrefs.HasKey(s_PiplExportSavedStatusKey))
            {
                m_PiplExportStatus = PlayerPrefs.GetInt(s_PiplExportSavedStatusKey) == 1
                    ? ConsentStatus.ConsentGiven
                    : ConsentStatus.ConsentDenied;
            }
            if (PlayerPrefs.HasKey(s_PiplUseSavedStatusKey))
            {
                m_PiplUseStatus = PlayerPrefs.GetInt(s_PiplUseSavedStatusKey) == 1
                    ? ConsentStatus.ConsentGiven
                    : ConsentStatus.ConsentDenied;
            }
        }

        public bool HasCheckedForConsent()
        {
            return IsStatusInACheckedState(m_PiplUseStatus) && IsStatusInACheckedState(m_PiplExportStatus);
        }

        public bool IsConsentDenied()
        {
            return m_PiplExportStatus == ConsentStatus.ConsentDenied || m_PiplUseStatus == ConsentStatus.ConsentDenied;
        }

        private static bool IsStatusInACheckedState(ConsentStatus status)
        {
            return status == ConsentStatus.NotRequired || status == ConsentStatus.ConsentGiven ||
                status == ConsentStatus.ConsentDenied;
        }

        public IEnumerator IsPiplConsentFlowRequired(Action<bool> callback)
        {
            m_CurrentConsentCheckCallback = callback;
            if (HasCheckedForConsent())
            {
                // Already have checked for consent, don't need to do it again
                callback(false);
            }
            else
            {
                GeoIpApiRequest request = new GeoIpApiRequest();
                request.OnCompleted += delegate(GeoIpResponse response, string error)
                {
                    if (response != null)
                    {
                        bool isPiplConsentRequired = response.identifier == "pipl";

                        m_PiplExportStatus = isPiplConsentRequired ? ConsentStatus.RequiredButUnchecked : ConsentStatus.NotRequired; 
                        m_PiplUseStatus = isPiplConsentRequired ? ConsentStatus.RequiredButUnchecked : ConsentStatus.NotRequired;
                        
                        m_CurrentConsentCheckCallback(isPiplConsentRequired);
                    }
                    else
                    {
                        throw new ConsentCheckException($"Could not check for required consent at this time: {error ?? "Unknown Error"}");
                    }

                    m_CurrentConsentCheckCallback = null;
                };

                yield return request.MakeRequest();
            }
        }

        public void SetUserPiplUseConsent(bool consentGiven)
        {
            m_PiplUseStatus = consentGiven ? ConsentStatus.ConsentGiven : ConsentStatus.ConsentDenied;
            PlayerPrefs.SetInt(s_PiplUseSavedStatusKey, consentGiven ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void SetUserPiplExportConsent(bool consentGiven)
        {
            m_PiplExportStatus = consentGiven ? ConsentStatus.ConsentGiven : ConsentStatus.ConsentDenied;
            PlayerPrefs.SetInt(s_PiplExportSavedStatusKey, consentGiven ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool AllConsentsAreMet()
        {
            return (m_PiplExportStatus == ConsentStatus.ConsentGiven || m_PiplExportStatus == ConsentStatus.NotRequired) 
                && (m_PiplUseStatus == ConsentStatus.ConsentGiven || m_PiplUseStatus == ConsentStatus.NotRequired);
        }
    }
}