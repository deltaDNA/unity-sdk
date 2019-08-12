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

using UnityEngine;
using System;
using System.Collections;


namespace DeltaDNA {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

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

        internal static IEnumerator Request(
            MonoBehaviour caller,
            EngageCache cache,
            EngageRequest request,
            EngageResponse response, 
            bool useConfigurationTimeout = false) {

            string requestJSON = request.ToJSON();
            string url = DDNA.Instance.ResolveEngageURL(requestJSON);

            HttpRequest httpRequest = new HttpRequest(url);
            httpRequest.HTTPMethod = HttpRequest.HTTPMethodType.POST;
            httpRequest.HTTPBody = requestJSON;
            httpRequest.TimeoutSeconds = useConfigurationTimeout ? DDNA.Instance.Settings.HttpRequestConfigurationTimeoutSeconds : DDNA.Instance.Settings.HttpRequestEngageTimeoutSeconds;
            httpRequest.setHeader("Content-Type", "application/json");

            Action<int, string, string> httpHandler = (statusCode, data, error) => {
                if (error == null && statusCode >= 200 && statusCode < 300) {
                    cache.Put(request.DecisionPoint, request.Flavour, data);
                } else {
                    Logger.LogDebug("Engagement failed with "+statusCode+" "+error);
                    var isClientError = statusCode >= 400 && statusCode < 500;
                    var cached = cache.Get(request.DecisionPoint, request.Flavour);
                    if (cached != null && ! isClientError) {
                        Logger.LogDebug("Using cached response");
                        data = "{\"isCachedResponse\":true," + cached.Substring(1);
                    } else {
                        data = "{}";
                    }
                }

                response(data, statusCode, error);
            };

            yield return caller.StartCoroutine(Network.SendRequest(httpRequest, httpHandler));
        }
    }
}
