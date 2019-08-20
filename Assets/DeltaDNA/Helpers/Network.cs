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
#if UNITY_5_6_OR_NEWER
using UnityEngine.Networking;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DeltaDNA {

    internal class HttpRequest {

        private Dictionary<string, string> headers = new Dictionary<string, string>();

        internal enum HTTPMethodType {
            GET,
            POST
        }

        internal HttpRequest(string url) {
            this.URL = url;
            this.TimeoutSeconds = DDNA.Instance.Settings.HttpRequestCollectTimeoutSeconds;
        }

        internal string URL { get; private set; }

        internal HTTPMethodType HTTPMethod { get; set; }

        internal string HTTPBody { get; set; }

        internal int TimeoutSeconds { get; set; }

        internal Dictionary<string, string> getHeaders() {
            return this.headers;
        }

        internal void setHeader(string field, string value) {
            this.headers[field] = value;
        }

        public override string ToString()
        {
            return "HttpRequest: " + this.URL + "\n" +
                this.HTTPMethod + "\n" +
                this.HTTPBody + "\n";
        }
    }

    internal static class Network {

        const string HeaderKey = "STATUS";
        const string StatusRegex = @"^.*\s(\d{3})\s.*$";
        const string ErrorRegex = @"^(\d{3})\s.*$";

        internal static IEnumerator SendRequest(HttpRequest request, Action<int /*statusCode*/, string /*data*/, string /*error*/> completionHandler)
        {

            // timeout feature added in 5.6.2f1
            #if UNITY_5_6_OR_NEWER && !UNITY_5_6_0 && !UNITY_5_6_1

            UnityWebRequest www = new UnityWebRequest();
            www.url = request.URL;
            www.timeout = request.TimeoutSeconds;
            www.downloadHandler = new DownloadHandlerBuffer();
            if (request.HTTPMethod == HttpRequest.HTTPMethodType.POST)
            {
                www.method = UnityWebRequest.kHttpVerbPOST;
                foreach (var entry in request.getHeaders())
                {
                    www.SetRequestHeader(entry.Key, entry.Value);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(request.HTTPBody);
                www.uploadHandler = new UploadHandlerRaw(bytes);
                www.chunkedTransfer = false;
            }
            else
            {
                www.method = UnityWebRequest.kHttpVerbGET;
            }

            #if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
            #else
            yield return www.Send();
            #endif

            if (completionHandler != null) {
                completionHandler((int)www.responseCode, www.downloadHandler.text, www.error);
            }

            #else

            WWW www;

            if (request.HTTPMethod == HttpRequest.HTTPMethodType.POST) {
                Dictionary<string, string> headers = new Dictionary<string, string>();

                WWWForm form = new WWWForm();
                foreach (var entry in Utils.HashtableToDictionary<string, string>(form.headers)) {
                    headers[entry.Key] = entry.Value;
                }

                foreach (var entry in request.getHeaders()) {
                    headers[entry.Key] = entry.Value;
                }

                byte[] bytes = Encoding.UTF8.GetBytes(request.HTTPBody);

                www = new WWW(request.URL, bytes, headers);
            }
            else {
                www = new WWW(request.URL);
            }

            float timer = 0;
            bool timedout = false;
            while (!www.isDone) {
                if (timer > request.TimeoutSeconds) {
                    timedout = true;
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            int statusCode = 1001;
            string data = null;
            string error = null;

            if (timedout) {
                www.Dispose();
                error = "connect() timed out";
            } else {
                statusCode = ReadStatusCode(www);
                data = www.text;
                error = www.error;
            }

            if (completionHandler != null) {
                completionHandler(statusCode, data, error);
            }
            #endif // UNITY_5_6_OR_NEWER

        }
        #if ! UNITY_2017_2_OR_NEWER && UNITY_5_6_OR_NEWER && !UNITY_5_6_0 && !UNITY_5_6_1
        private static int ReadStatusCode(WWW www)
        {
            // 1) Best case there is a STATUS header which says something like 200 OK
            // 2) If there was an error, it has the status in the error text
            // 3) If there was no error then assume it was okay

            int statusCode = 200;

            if (www.responseHeaders.ContainsKey(HeaderKey)) {
                MatchCollection matches = Regex.Matches(www.responseHeaders[HeaderKey], StatusRegex);
                if (matches.Count > 0 && matches[0].Groups.Count > 0) {
                    statusCode = Convert.ToInt32(matches[0].Groups[1].Value);
                }
            }
            else if (!String.IsNullOrEmpty(www.error)) {
                MatchCollection matches = Regex.Matches(www.error, ErrorRegex);
                if (matches.Count > 0 && matches[0].Groups.Count > 0) {
                    statusCode = Convert.ToInt32(matches[0].Groups[1].Value);
                #if !UNITY_5_5_OR_NEWER
                } else if (Application.platform == RuntimePlatform.WindowsWebPlayer) {
                    Logger.LogDebug("IE11 Webplayer workaround, assuming request succeeded");
                    statusCode = 204; // Bug in IE11, can't handle 204 which Collect returns.
                #endif
                } else {
                    statusCode = 1002; // Assume service is unavailable, likely no network connection.
                }
            }
            else if (String.IsNullOrEmpty(www.text)) {
                statusCode = 204;   // No Content
            }

            return statusCode;
        }
        #endif


    }



}
