using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace DeltaDNA
{
    internal class EventsManagerParameterCreator
    {
        private const string ENDPOINT_URL = "https://api.deltadna.net/api/events/v1/event-parameters";

        // Must start with a lower-case English letter.
        // Must otherwise only have lower- and upper-case English letters, digits or underscore.
        private readonly Regex NAME_VALIDATOR = new Regex("^[a-z][_a-zA-Z0-9]*$");
        
        public event Action<DDNAEventManagerEventParameter> OnParameterCreated;
        public event Action OnCreationFailed;

        public bool NameIsInvalid(String name)
        {
            return !NAME_VALIDATOR.IsMatch(name);
        }

        public void CreateParameter(string authToken, Dictionary<string,object> payload)
        {
            UnityWebRequest request = new UnityWebRequest(ENDPOINT_URL, UnityWebRequest.kHttpVerbPOST);

            string jsonPayload = MiniJSON.Json.Serialize(payload);
            byte[] bytePayload = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bytePayload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            requestAsyncOperation.completed += ResponseArrived;
        }

        private void ResponseArrived(AsyncOperation asyncOperation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError("Failed to create parameter: " + request.error);
                if (!request.isNetworkError)
                {
                    Debug.LogError(request.downloadHandler.text);
                }
                OnCreationFailed?.Invoke();
            }
            else
            {
                OnParameterCreated?.Invoke(JsonUtility.FromJson<DDNAEventManagerEventParameter>(request.downloadHandler.text));
            }
        }
    }
}
