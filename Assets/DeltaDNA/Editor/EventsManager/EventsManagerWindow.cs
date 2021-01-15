using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    internal class EventsManagerWindow: EditorWindow
    {
        private const string EDITOR_PREFS_API_KEY = "DELTA_DNA_API_KEY";

        public const string WINDOW_TITLE = "Events Manager";

        private enum Tab
        {
            Events,
            Parameters
        }

        private enum State
        {
            LoggedOut,
            AuthInProgress,
            LoggedIn
        }

        private EventsManagerAuthProvider _authProvider;
        private EventsManagerEnvironmentProvider _environmentProvider;
        private EventProvider _eventProvider;
        private ParameterProvider _parameterProvider;

        private EventsManagerEventsTab _eventsTab;
        private EventsManagerParametersTab _parametersTab;
        
        private string _apiKey;
        private string _apiPassword;

        private State _state;
        private Tab _currentTab;

        public int CurrentApplicationId
        {
            get
            {
                // TODO: this is awful, would be nice if the Environments API call returned applicatonId directly
                // (It currently offers the application _name_ as the 'game' property, but not its ID.)
                if (_eventProvider.HasData)
                {
                    foreach (DDNAEventManagerEvent apiEvent in _eventProvider.Data)
                    {
                        if (apiEvent.environment == _environmentProvider.CurrentEnvironmentId)
                        {
                            // Assuming that an event can only contain parameters from its own application...
                            return apiEvent.parameters[0].application;
                        }
                    }
                }

                return 0;
            }
        }

        private void OnEnable()
        {
            _authProvider = new EventsManagerAuthProvider(this);
            _environmentProvider = new EventsManagerEnvironmentProvider();
            _eventProvider = new EventProvider();
            _parameterProvider = new ParameterProvider();

            _environmentProvider.OnResponseArrived += ProviderResponseArrived;
            _eventProvider.OnResponseArrived += ProviderResponseArrived;
            _parameterProvider.OnResponseArrived += ProviderResponseArrived;

            _eventsTab = new EventsManagerEventsTab(this, _authProvider, _environmentProvider, _eventProvider, _parameterProvider);
            _parametersTab = new EventsManagerParametersTab(this, _authProvider, _environmentProvider, _eventProvider, _parameterProvider);

            titleContent = new GUIContent(WINDOW_TITLE);

            _authProvider.LoadCacheToken();

            if (EditorPrefs.HasKey(EDITOR_PREFS_API_KEY))
            {
                _apiKey = EditorPrefs.GetString(EDITOR_PREFS_API_KEY);
            }
        }

        public void RefreshData()
        {
            _environmentProvider.RefreshData(_authProvider.AuthToken);
            _eventProvider.RefreshData(_authProvider.AuthToken);
            _parameterProvider.RefreshData(_authProvider.AuthToken);
        }

        private void ProviderResponseArrived()
        {
            if (_environmentProvider.DidFailToRetrieveData ||
                _eventProvider.DidFailToRetrieveData ||
                _parameterProvider.DidFailToRetrieveData)
            {
                _state = State.LoggedOut;
            }
            else
            {
                switch (_currentTab)
                {
                    case Tab.Events:
                        _eventsTab.Refresh();
                        break;
                    case Tab.Parameters:
                        _parametersTab.Refresh();
                        break;
                }

            }

            Repaint();
        }

        private void OnGUI()
        {
            switch (_state)
            {
                case State.LoggedOut:
                    DrawLoginPanel();
                    if (_authProvider.HasAuthToken)
                    {
                        _state = State.LoggedIn;
                    }
                    break;
                case State.AuthInProgress:
                    DrawAuthenticatingPanel();
                    if (!_authProvider.AuthInProgress)
                    {
                        if (_authProvider.AuthToken == null)
                        {
                            LogOut();
                        }
                        else
                        {
                            _state = State.LoggedIn;
                        }
                    }
                    break;
                case State.LoggedIn:
                    GUILayout.BeginVertical();
                    DrawToolbar();

                    switch (_currentTab)
                    {
                        case Tab.Events:
                            _eventsTab.Draw();
                            break;
                        case Tab.Parameters:
                            _parametersTab.Draw();
                            break;
                    }
                    GUILayout.EndVertical();
                    break;
            }
        }

        private void LogOut()
        {
            _authProvider.Expire();
            _environmentProvider.Reset();
            _eventProvider.Reset();
            _parameterProvider.Reset();
            _state = State.LoggedOut;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Log Out", GUILayout.MinWidth(200.0f)))
            {
                LogOut();
            }
            if (GUILayout.Button("Events Manager", GUILayout.MinWidth(200.0f)))
            {
                _currentTab = Tab.Events;
            }
            if (GUILayout.Button("Parameters Manager", GUILayout.MinWidth(200.0f)))
            {
                _currentTab = Tab.Parameters;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLoginPanel()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("API Key", EditorStyles.boldLabel);
            _apiKey = EditorGUILayout.TextField(_apiKey, GUILayout.MinWidth(400.0f));
            EditorGUILayout.LabelField("API Password", EditorStyles.boldLabel);
            _apiPassword = EditorGUILayout.PasswordField(_apiPassword, GUILayout.MinWidth(400.0f));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("API Help"))
            {
                Application.OpenURL("https://docs.deltadna.com/introduction-to-the-platform-api/");
            }
            if (GUILayout.Button("Authenticate"))
            {
                EditorPrefs.SetString(EDITOR_PREFS_API_KEY, _apiKey);
                _state = State.AuthInProgress;
                _eventProvider.Reset();
                _parameterProvider.Reset();
                _authProvider.GetAuthToken(_apiKey, _apiPassword);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void DrawAuthenticatingPanel()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox("Authenticating...", MessageType.Info);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
    }
}