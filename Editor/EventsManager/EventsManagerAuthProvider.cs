using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DeltaDNA
{
    [Serializable]
    internal class DeltaDnaApiAuthRequest
    {
        public string key;
        public string password;
    }

// DDSDK-69 - https://jira.unity3d.com/browse/DDSDK-69
#pragma warning disable CS0649
    [Serializable]
    internal class DeltaDnaApiAuthResponse
    {
        public string idToken;
    }
#pragma warning restore CS0649

    internal class EventsManagerAuthProvider
    {
        private readonly EventsManagerWindow _parentWindow;

        private const string EDITOR_PREFS_ID_TOKEN = "DELTA_DNA_ID_TOKEN";
        private const string EDITOR_PREFS_ID_TOKEN_LAST_CACHED_DATE = "DELTA_DNA_ID_LAST_CACHED";
        private const long ID_TOKEN_EXPIRY_IN_SECONDS = 1800; // Tokens last half an hour

        public string AuthToken { get; private set; }
        public bool AuthInProgress { get; private set; }
        public bool HasAuthToken { get { return !String.IsNullOrEmpty(AuthToken); } }

        public EventsManagerAuthProvider(EventsManagerWindow parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public void Expire()
        {
            AuthToken = null;
            EditorPrefs.DeleteKey(EDITOR_PREFS_ID_TOKEN);
            EditorPrefs.DeleteKey(EDITOR_PREFS_ID_TOKEN_LAST_CACHED_DATE);
        }

        public void GetAuthToken(string apiKey, string password)
        {
            LoadCacheToken();
            if (AuthToken != null)
            {
                return;
            }
            
            AuthInProgress = true;

            string url = "https://api.deltadna.net/api/authentication/v1/authenticate";
            DeltaDnaApiAuthRequest requestBody = new DeltaDnaApiAuthRequest()
            {
                key = apiKey,
                password = password
            };
            string serialisedAuthenticationData = JsonUtility.ToJson(requestBody);
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(serialisedAuthenticationData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            AsyncOperation ao = request.SendWebRequest();
            ao.completed += AuthenticationResponseReceived;
        }

        private void AuthenticationResponseReceived(AsyncOperation asyncOperation)
        {
            AuthInProgress = false;

            UnityWebRequestAsyncOperation webOperation = (UnityWebRequestAsyncOperation)asyncOperation;

#if UNITY_2020_2_OR_NEWER
            if (webOperation.webRequest.result == UnityWebRequest.Result.ProtocolError || webOperation.webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
            if (webOperation.webRequest.isHttpError || webOperation.webRequest.isNetworkError)
#endif
            {
                Debug.LogError("Unable to authenticate with deltaDNA API: " + webOperation.webRequest.error);
            }
            else
            {
                string jsonResponse = webOperation.webRequest.downloadHandler.text;
                DeltaDnaApiAuthResponse response = JsonUtility.FromJson<DeltaDnaApiAuthResponse>(jsonResponse);
                AuthToken = response.idToken;
                UpdateCacheToken();
            }

            // State of the main window has now changed, so force a repaint to ensure it doesn't wait before updating.
            _parentWindow.Repaint();
        }

        public void LoadCacheToken()
        {
            string previousUpdatedStringTimestamp = EditorPrefs.GetString(EDITOR_PREFS_ID_TOKEN_LAST_CACHED_DATE);
            string cachedToken = EditorPrefs.GetString(EDITOR_PREFS_ID_TOKEN);
            if (!string.IsNullOrEmpty(previousUpdatedStringTimestamp) && !string.IsNullOrEmpty(cachedToken))
            {
                long timestampForLastCacheUpdate = long.Parse(previousUpdatedStringTimestamp);
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now - timestampForLastCacheUpdate < ID_TOKEN_EXPIRY_IN_SECONDS)
                {
                    AuthToken = cachedToken;
                    _parentWindow.Repaint();
                }
                else
                {
                    Expire();
                }
            }
        }

        private void UpdateCacheToken()
        {
            EditorPrefs.SetString(EDITOR_PREFS_ID_TOKEN, AuthToken);
            string now = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            EditorPrefs.SetString(EDITOR_PREFS_ID_TOKEN_LAST_CACHED_DATE, now);
        }
    }
}