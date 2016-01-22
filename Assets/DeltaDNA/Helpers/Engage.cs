using UnityEngine;
using System;
using System.Collections;

using JSONObject = System.Collections.Generic.Dictionary<string, object>;

namespace DeltaDNA {

    public delegate void EngageResponse(string response, int statusCode, string error);

    internal class EngageRequest {

        public EngageRequest(string decisionPoint) 
        { 
            this.DecisionPoint = decisionPoint;
            this.Flavour = "engagement";
            this.Parameters = new JSONObject(); 
        }

        public string DecisionPoint { get; private set; }
        public string Flavour { get; set; }
        public JSONObject Parameters { get; set; }

        public string ToJSON()
        {
            try {
                JSONObject request = new JSONObject()
                {
                    { "userID", DDNA.Instance.UserID },
                    { "decisionPoint", this.DecisionPoint },
                    { "flavour", this.Flavour },
                    { "sessionID", DDNA.Instance.SessionID },
                    { "version", Settings.ENGAGE_API_VERSION },
                    { "sdkVersion", Settings.SDK_VERSION },
                    { "platform", DDNA.Instance.Platform },
                    { "timezoneOffset", Convert.ToInt32(ClientInfo.TimezoneOffset) }
                };
                
                if (ClientInfo.Locale != null)
                {
                    request.Add("locale", ClientInfo.Locale);
                }
                
                if (this.Parameters != null && this.Parameters.Count > 0)
                {
                    request.Add("parameters", this.Parameters);
                }

                return DeltaDNA.MiniJSON.Json.Serialize(request);

            } catch (Exception exception) {
                Logger.LogError("Error serialising engage request: "+exception.Message);
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("[EngageRequest]"+this.DecisionPoint
                                 +"("+this.Flavour+")\n"
                                 +this.Parameters);
        }

    }

    internal class Engage {

        internal static IEnumerator Request(MonoBehaviour caller, EngageRequest request, EngageResponse response)
        {
            // TODO - what happens if requestJSON is null?
            // TODO - what happens if URL is null?
            string requestJSON = request.ToJSON();
            string url = DDNA.Instance.ResolveEngageURL(requestJSON);
            
            HttpRequest httpRequest = new HttpRequest(url);
            httpRequest.HTTPMethod = HttpRequest.HTTPMethodType.POST;
            httpRequest.HTTPBody = requestJSON;
            httpRequest.TimeoutSeconds = DDNA.Instance.Settings.HttpRequestEngageTimeoutSeconds;
            httpRequest.setHeader("Content-Type", "application/json");

            System.Action<int, string, string> httpHandler = (statusCode, data, error) => {


                string engagementKey = "DDSDK_ENGAGEMENT_" + request.DecisionPoint + "_" + request.Flavour;
                if (statusCode < 400 && error == null) {
                    Logger.LogDebug("Engagement succeeded with "+statusCode+" "+data);
                    try {
                        PlayerPrefs.SetString(engagementKey, data);
                    } catch (Exception exception) {
                        Logger.LogWarning("Unable to cache engagement: "+exception.Message);
                    }
                } else {
                    Logger.LogDebug("Engagement failed with "+statusCode+" "+error);
                    if (PlayerPrefs.HasKey(engagementKey)) { 
                        Logger.LogDebug("Using cached response");
                        data = PlayerPrefs.GetString(engagementKey);
                    }
                } 

//                JSONObject responseJSON = null;
//                if (data != null) {
//                    try {
//                        responseJSON = DeltaDNA.MiniJSON.Json.Deserialize(data) as JSONObject;
//                    } catch (Exception exception) {
//                        Logger.LogError("Engagement "+request.DecisionPoint+" responded with invalid JSON: "+exception.Message);
//                    }
//                }

                response(data, statusCode, error);
            };

            yield return caller.StartCoroutine(Network.SendRequest(httpRequest, httpHandler));
        }   

        internal static void ClearCache()
        {
            // TODO record engage keys so they can be removed.
        }
    }

}
