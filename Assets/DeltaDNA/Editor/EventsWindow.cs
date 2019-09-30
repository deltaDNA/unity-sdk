#if UNITY_2017_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using DeltaDNA.MiniJSON;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking;


namespace DeltaDNA.Editor{
    using JSONObject = Dictionary<string, string>;

    public sealed class EventsWindow : EditorWindow{
        private string apiPassword;
    
        private string apiToken;
        private DateTime tokenExpiry;
        
        internal const string CONFIG = "Assets/DeltaDNA/Resources/ddna_api_configuration.xml";
        private APIConfiguration apiConfig;
        
        private readonly XmlSerializer apiConfigSerializer = new XmlSerializer(
            typeof(APIConfiguration),
            new XmlRootAttribute("configuration"));
        
        
        private UnityWebRequestAsyncOperation pending_request;
        private UnityWebRequestAsyncOperation pending_environment_request;
        private UnityWebRequestAsyncOperation pending_events_request;
  
        private List<object> environmentsJson;
        int environmentSelection;
        private Dictionary<int, string> envIdMap = new Dictionary<int, string>();
        private readonly Dictionary<string, List<object>> eventsDict = new Dictionary<string, List<object>>();
        private readonly Dictionary<string, EventsTreeView> eventsTreeDict = new Dictionary<string, EventsTreeView>();

        private Dictionary<string, TreeViewState> envTreeViews = new Dictionary<string, TreeViewState>();
        
        
        private string m_name;
        private string m_description;
        private bool m_published;
        private bool m_isLive;
        private string m_param_type;
        private string m_format;
        private string m_enumeration;
        private bool m_isRequired;
        private string m_type;
        private int oldEnvSelection;
        private Vector2 scrollPos; 

        
        
        void OnEnable() {
            titleContent = new GUIContent(
                "Events",
                AssetDatabase.LoadAssetAtPath<Texture>("Assets/DeltaDNA/Editor/Resources/Logo_16.png"));
            
            Load();
        }
        
        private void OnGUI(){
            if (apiConfig == null) return;
            if (GUILayout.Button("See API Documentation"))
            {
                Application.OpenURL("https://docs.deltadna.com/introduction-to-the-platform-api/");
            }
            apiConfig.ApiKey = EditorGUILayout.TextField("API Key", apiConfig.ApiKey);
            apiPassword = EditorGUILayout.PasswordField("API Password", apiPassword);
           

            if (GUILayout.Button("Persist API Key")){
                Apply();
            }
            if (GUILayout.Button("Authenticate & Fetch Event Definitions")){
                GetApiToken();
            }
         
            
         
            CheckTokenRequest();
            if (pending_environment_request == null && HasValidAPIToken() && !HasEnvironments()){
                GetEnvironments();
            }

            CheckEnvironmentRequest();

            if (HasEnvironments() && HasValidAPIToken()){
                environmentSelection =
                    EditorGUILayout.Popup("Environment", environmentSelection, GetEnvironmentOptions());
                GetEvents();
            }
          
            CheckEventsRequest();
            CheckAndDrawDetailView();

            if (envIdMap.ContainsKey(environmentSelection) &&
                eventsTreeDict.ContainsKey(envIdMap[environmentSelection])){
                Rect rect = GUILayoutUtility.GetRect(0, 1000, 0, 1000);
                eventsTreeDict[envIdMap[environmentSelection]].OnGUI(rect);
            }
           
        }

        private void CheckAndDrawDetailView(){
            if (oldEnvSelection != environmentSelection){
                m_type = null; 
            }
            if (!string.IsNullOrEmpty(m_type)){
                Rect rect3 = EditorGUILayout.GetControlRect(false, 1);

                rect3.height = 1;

                EditorGUI.DrawRect(rect3, new Color(0.5f, 0.5f, 0.5f, 1));
                EditorGUILayout.LabelField(m_name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(m_description, EditorStyles.wordWrappedLabel);
                if (m_type == "EVENT"){
                    EditorGUILayout.LabelField("Has been published to live  ", m_published.ToString());
                    EditorGUILayout.LabelField("Enabled  ", m_isLive.ToString());
                }
                else{
                    EditorGUILayout.LabelField("Type", m_param_type);
                    if (m_param_type == "STRING"){
                        EditorGUILayout.LabelField("Format  ", m_format);
                        if (m_enumeration != null){
                            EditorGUILayout.LabelField("Enumeration Values");

                            EditorGUILayout.BeginHorizontal();
                  
                            scrollPos =
                                EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUILayout.Height(100));

                            GUILayout.Label(m_enumeration);
                            EditorGUILayout.EndScrollView();

                            EditorGUILayout.EndHorizontal();
                        }

                    }

                    EditorGUILayout.LabelField("Required   ", m_isRequired.ToString());
                }

                Rect rect4 = EditorGUILayout.GetControlRect(false, 1);

                rect4.height = 1;

                EditorGUI.DrawRect(rect4, new Color(0.5f, 0.5f, 0.5f, 1));
            }
            oldEnvSelection = environmentSelection;
        }

        private void CheckEventsRequest(){
            if (pending_events_request == null || !pending_events_request.isDone) return;
            if (pending_events_request.webRequest.isNetworkError){
                Debug.LogError(pending_events_request.webRequest.responseCode == 400
                    ? "Invalid API Credentials"
                    : "Something went wrong when trying to retrieve events. Please try again later.");
                return;
            }

            var selectedEnv = envIdMap[environmentSelection];
            try{
                eventsDict[selectedEnv] =
                    (List<object>) Json.Deserialize(pending_events_request.webRequest
                        .downloadHandler.text);
                eventsTreeDict[selectedEnv] = new EventsTreeView(new TreeViewState(), setDetailView, eventsDict[selectedEnv]);
            }
            catch {
                Debug.LogError("Could not parse event response.");
                eventsDict[selectedEnv] = new List<object>();
            }
            finally{
                pending_events_request = null;
            }
        }

        private void GetEvents(){
            var selectedEnv = envIdMap[environmentSelection];
            if (eventsDict.ContainsKey(selectedEnv) || pending_events_request != null) return;
            var url = "https://api.deltadna.net/api/events/v1/events?environmentId=" + selectedEnv;
            var request =
                new UnityWebRequest(url, "GET"){downloadHandler = new DownloadHandlerBuffer()};
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            pending_events_request = request.SendWebRequest();
        }

        public string[] GetEnvironmentOptions(){
            List<string> returnList = new List<string>();
            envIdMap = new Dictionary<int, string>();
            foreach (var jsonObject in environmentsJson){
                var castJsonObject = jsonObject as Dictionary<string, object>;
                var currentEnv = castJsonObject["game"] as string;
                var currentType = castJsonObject["name"] as string;
                var currentId = ((long) castJsonObject["id"]).ToString();
                var fullname = currentEnv + " - " + currentType;
                returnList.Add(fullname);
                envIdMap.Add(returnList.IndexOf(fullname), currentId);
            }
            
            return returnList.ToArray();
        }

        private void CheckEnvironmentRequest(){
            if (pending_environment_request == null || !pending_environment_request.isDone) return;
            if (pending_environment_request.webRequest.isNetworkError){
                Debug.LogError(pending_environment_request.webRequest.responseCode == 400
                    ? "Invalid API Credentials"
                    : "Something went wrong when trying to retrieve API key. Please try again later.");
                return;
            }

            try{
                Debug.Log("Got Environments Token");
                environmentsJson =
                    (List<object>) Json.Deserialize(pending_environment_request.webRequest
                        .downloadHandler.text);
            }
            catch {
                Debug.LogError("There was an error parsing environment data.");
                environmentsJson = new List<object>();
            }
            finally{
                pending_environment_request = null;
            }
        }

        private bool HasEnvironments(){
            return environmentsJson != null;
        }

        private bool HasValidAPIToken(){
            return apiToken != null && DateTime.Now < tokenExpiry;
        }

        private void CheckTokenRequest(){
            if (pending_request == null || !pending_request.isDone) return;
            if (pending_request.webRequest.isNetworkError){
                Debug.LogError(pending_request.webRequest.responseCode == 400
                    ? "Invalid API Credentials"
                    : "Something went wrong when trying to retrieve API key. Please try again later.");
                return;
            }

            try{
                var responseDict = MiniJSON.Json.Deserialize(pending_request.webRequest
                    .downloadHandler.text) as Dictionary<string, object>;
                apiToken = (string) responseDict["idToken"];
                tokenExpiry = DateTime.Now.AddMinutes(30);
                environmentsJson = null;
                pending_environment_request = null;
            }
            catch{
                Debug.LogError("Could not parse API Token response. Are your credentials correct?");
            }
        finally{
                pending_request = null;
            }
        }

        void GetApiToken(){
            var url = "https://api.deltadna.net/api/authentication/v1/authenticate";
            var dict = new Dictionary<String, String> {
                {"key", apiConfig.ApiKey},
                {"password", apiPassword}
            };
            var bodyJsonString = Json.Serialize(dict);
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            pending_request = request.SendWebRequest();
        }

        void GetEnvironments(){
            var url = "https://api.deltadna.net/api/engage/v1/environments";
            var request =
                new UnityWebRequest(url, "GET"){downloadHandler = new DownloadHandlerBuffer()};
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);

            pending_environment_request = request.SendWebRequest();
        }

        private void setDetailView(Dictionary<string, object> data){
            m_type = data["ddna_tree_type"] as string;
            m_name = data["name"] as string;
            m_description = data["description"] as string;
            if (m_type == "EVENT"){
                m_published = (bool) data["published"];
                m_isLive = (bool) data["active"];
            }
            else{
                m_name = data["name"] as string;
                m_param_type = data["type"] as string;
                if (m_param_type == "STRING"){
                    if (data.ContainsKey("format")){
                        m_format = data["format"] as string;
                    }
                    else{
                        m_format = "-";
                    }

                    if (data.ContainsKey("enumeration")){
                        List<object> enumarray =  data["enumeration"] as List<object>;
                        m_enumeration = "";
                        foreach (var enumobj in enumarray){
                            m_enumeration +=   enumobj as string  + "\n";
                        }
                    }
                    else{
                        m_enumeration = null;
                    }
                }
                else{
                    m_format = null;
                    m_enumeration = null;
                }

                m_isRequired = (bool) data["required"];
            }
            scrollPos = new Vector2();
        }

        private void Load(){
            if (File.Exists(CONFIG)) {
                using (var stringReader = new StringReader(File.ReadAllText(CONFIG))) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        apiConfig = apiConfigSerializer.Deserialize(xmlReader) as APIConfiguration;
                    }
                }
            } else {
                apiConfig = new APIConfiguration();
            }
        }
        
        
        private void Apply() {
            using (var stringWriter = new StringWriter()) {
                using (XmlWriter xmlWriter = XmlWriter.Create(
                    stringWriter, new XmlWriterSettings() { Indent = true })) {
                    apiConfigSerializer.Serialize(xmlWriter, apiConfig);
                    File.WriteAllText(CONFIG, stringWriter.ToString());
                }
            }
            
           
            
            Debug.Log("[DeltaDNA] Changes have been applied to XML API configuration file, please commit the updates to version control");
        }
       
    }
}
#endif