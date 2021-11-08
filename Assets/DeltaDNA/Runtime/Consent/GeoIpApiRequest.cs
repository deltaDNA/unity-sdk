using System;
using System.Collections;
using UnityEngine;

namespace DeltaDNA.Consent
{
    [Serializable]
    internal class GeoIpResponse
    {
        public string identifier;
        public string country;
        public string region;
        public int ageGateLimit;
    }
    
    internal class GeoIpApiRequest
    {
        public event Action<GeoIpResponse, string> OnCompleted;
        
        private static string s_PrivacyEndpoint = "https://pls.prd.mz.internal.unity3d.com/api/v1/user-lookup";

        public IEnumerator MakeRequest()
        {
            HttpRequest request = new HttpRequest(s_PrivacyEndpoint);
            yield return Network.SendRequest(request, RequestCompleted);
        }

        private void RequestCompleted(int code, string data, string error)
        {
            if (code == 200 && !string.IsNullOrEmpty(data))
            {
                GeoIpResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<GeoIpResponse>(data);
                } catch {}
                
                OnCompleted?.Invoke(response, response == null ? "Error occurred while deserializing the privacy GeoIP response" : null);
            }
            else
            {
                OnCompleted?.Invoke(null, $"Error occurred while performing the privacy GeoIP request: {error ?? "Unknown Error"}");
            }
        }
    }
}