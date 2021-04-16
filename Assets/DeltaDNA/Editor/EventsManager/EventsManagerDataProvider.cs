using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DeltaDNA
{
// DDSDK-69 - https://jira.unity3d.com/browse/DDSDK-69
#pragma warning disable CS0649
    [Serializable]
    internal class ArrayResponseWrapper<T>
    {
        public T[] Items;
    }
#pragma warning restore CS0649

    internal class EventsManagerDataProvider<T>
    {
        private readonly TimeSpan REFRESH_INTERVAL = TimeSpan.FromSeconds(30);

        private readonly string _url;
        private DateTime _lastFetchTime = DateTime.MinValue;

        public T[] Data { get; private set; }
        public bool DidFailToRetrieveData { get; private set; }
        public bool FetchInProgress { get; private set; }
        public bool HasData
        {
            get { return Data != null; }
        }

        public bool IsStale
        {
            get
            {
                return DateTime.Now - _lastFetchTime > REFRESH_INTERVAL;
            }
        }

        public event Action OnResponseArrived;

        public EventsManagerDataProvider(string endpointUrl)
        {
            _url = endpointUrl;

            Reset();
        }

        public virtual void Reset()
        {
            DidFailToRetrieveData = false;
            FetchInProgress = false;
            Data = null;
            _lastFetchTime = DateTime.MinValue;
        }

        public void RefreshData(string authToken)
        {
            if (!FetchInProgress)
            {
                FetchInProgress = true;
                _lastFetchTime = DateTime.Now;

                UnityWebRequest request = UnityWebRequest.Get(_url);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");

                UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
                requestAsyncOperation.completed += ResponseArrived;
            }
        }

        private void ResponseArrived(AsyncOperation asyncOperation)
        {
            FetchInProgress = false;
            UnityWebRequest request = ((UnityWebRequestAsyncOperation)asyncOperation).webRequest;

#if UNITY_2020_2_OR_NEWER
            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                DidFailToRetrieveData = true;
                Data = null;
            }
            else
            {
                // We need to wrap the json in a wrapper object as neither MiniJSON nor JsonUtility can handle 
                // parsing a list of events correctly when it is a plain json list. JsonUtility can handle this
                // case only if it is wrapped in a object first.
                string wrappedJsonResponse = "{\"Items\":" + request.downloadHandler.text + "}";
                try
                {
                    Data = JsonUtility.FromJson<ArrayResponseWrapper<T>>(wrappedJsonResponse).Items;
                    DidFailToRetrieveData = false;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON from events API: {e.Message}");
                    Debug.Log($"The json that failed to parse is: {wrappedJsonResponse}");
                }
            }

            OnResponseArrived?.Invoke();
        }
    }
}
