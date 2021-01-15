using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace DeltaDNA
{
    internal class EventsManagerEventEditor
    {
        private const string CREATE_ENDPOINT_URL = "https://api.deltadna.net/api/events/v1/events";
        private const string ADD_ENDPOINT_URL_PATTERN = "https://api.deltadna.net/api/events/v1/events/{0}/add/{1}";
        private const string REMOVE_ENDPOINT_URL_PATTERN = "https://api.deltadna.net/api/events/v1/events/{0}/remove/{1}";
        private const string PUBLISH_ENDPOINT_URL_PATTERN = "https://api.deltadna.net/api/events/v1/events/publish/{0}";

        // Must start with an English letter, either upper- or lower-case.
        // Must otherwise only have lower- and upper-case English letters, digits or underscore.
        private readonly Regex NAME_VALIDATOR = new Regex("^[a-zA-Z][_a-zA-Z0-9]*$");

        private readonly EventsManagerWindow _parent;

        public event Action<DDNAEventManagerEvent> OnEventCreated;
        public event Action OnEventCreationFailed;

        public EventsManagerEventEditor(EventsManagerWindow parent)
        {
            _parent = parent;
        }

        public bool NameIsInvalid(string name)
        {
            return !NAME_VALIDATOR.IsMatch(name);
        }

        public void CreateEvent(string name, string description, int environmentId, string authToken)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                ["name"] = name,
                ["description"] = description,
                ["environment"] = environmentId
            };

            string postData = MiniJSON.Json.Serialize(payload);
            UnityWebRequest request = new UnityWebRequest(CREATE_ENDPOINT_URL, UnityWebRequest.kHttpVerbPOST);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            requestAsyncOperation.completed += CreateEventResponseArrived;
        }

        private void CreateEventResponseArrived(AsyncOperation asyncOperation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError("Failed to create event: " + request.error);
                if (!request.isNetworkError)
                {
                    Debug.LogError(request.downloadHandler.text);
                }
                OnEventCreationFailed?.Invoke();
            }
            else
            {
                OnEventCreated?.Invoke(JsonUtility.FromJson<DDNAEventManagerEvent>(request.downloadHandler.text));
            }
        }

        public void AddParameter(int eventId, int parameterId, bool required, string authToken)
        {
            string url = String.Format(ADD_ENDPOINT_URL_PATTERN, eventId, parameterId);
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                ["required"] = required
            };

            string jsonPayload = MiniJSON.Json.Serialize(payload);
            byte[] bytePayload = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bytePayload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            requestAsyncOperation.completed += AddParameterResponseArrived;
        }

        private void AddParameterResponseArrived(AsyncOperation asyncOperation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Failed to add parameter: " + request.error);
            }
            else
            {
                _parent.RefreshData();
            }
        }

        public void RemoveParameter(int eventId, int parameterId, string authToken)
        {
            string url = String.Format(REMOVE_ENDPOINT_URL_PATTERN, eventId, parameterId);
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            requestAsyncOperation.completed += RemoveParameterResponseArrived;
        }

        private void RemoveParameterResponseArrived(AsyncOperation asyncOperation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Failed to remove parameter: " + request.error);
            }
            else
            {
                _parent.RefreshData();
            }
        }

        public void PublishEvent(int eventId, string authToken)
        {
            string url = String.Format(PUBLISH_ENDPOINT_URL_PATTERN, eventId);
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            requestAsyncOperation.completed += PublishEventResponseArrived;
        }

        private void PublishEventResponseArrived(AsyncOperation asyncOperation)
        {
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("Failed to publish event: " + request.error);
            }
            else
            {
                _parent.RefreshData();
            }
        }
    }
}
