using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    internal class EventsManagerParametersTab
    {
        private enum ParameterType
        {
            String,
            Integer,
            Float,
            Bool
        }

        private enum Mode
        {
            View,
            Create,
            Creating,
        }

        private const float LIST_WIDTH = 250.0f;

        private readonly EventsManagerWindow _parent;
        private readonly EventsManagerAuthProvider _authProvider;
        private readonly EventsManagerEnvironmentProvider _environmentProvider;
        private readonly EventsManagerDataProvider<DDNAEventManagerEvent> _eventProvider;
        private readonly ParameterProvider _parameterProvider;
        private readonly EventsManagerParameterCreator _parameterCreator;

        private Vector2 _listScrollPosition;
        private Mode _mode;
        private int _selectedParameterId;
        private DDNAEventManagerEventParameter _selectedParameterObject;

        private string _newName;
        private string _newDescription;
        private ParameterType _newType;
        private string _newFormat;

        public EventsManagerParametersTab(EventsManagerWindow parent,
                                          EventsManagerAuthProvider authProvider,
                                          EventsManagerEnvironmentProvider environmentProvider,
                                          EventsManagerDataProvider<DDNAEventManagerEvent> eventProvider,
                                          ParameterProvider parameterProvider)
        {
            _parent = parent;
            _authProvider = authProvider;
            _environmentProvider = environmentProvider;
            _eventProvider = eventProvider;
            _parameterProvider = parameterProvider;
            _parameterCreator = new EventsManagerParameterCreator();
            _parameterCreator.OnParameterCreated += ParameterCreated;
            _parameterCreator.OnCreationFailed += ParameterCreationFailed;
            _listScrollPosition = Vector2.zero;
        }

        public void Refresh()
        {
            // Relink the current parameter object, as it may no longer contain the same data
            // as before. The ID should never change after creation, but somebody may have changed
            // the underlying data via the website.
            if (_mode == Mode.View &&
                _selectedParameterId > 0 &&
                _parameterProvider.HasData)
            {
                foreach (DDNAEventManagerEventParameter apiParameter in _parameterProvider.Data)
                {
                    if (apiParameter.id == _selectedParameterId)
                    {
                        _selectedParameterObject = apiParameter;
                    }
                }
            }
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            DrawParameterList();
            switch (_mode)
            {
                case Mode.View:
                    // It does not seem possible for a parameter to have an ID of 0, so this should be safe.
                    if (_selectedParameterId > 0)
                    {
                        DrawParameterViewer(_selectedParameterObject);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Choose a parameter", MessageType.Info);
                    }
                    break;
                case Mode.Create:
                    DrawParameterCreator();
                    break;
                case Mode.Creating:
                    EditorGUILayout.HelpBox("Creating...", MessageType.Info);
                    break;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawParameterList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LIST_WIDTH));

            if (GUILayout.Button("Refresh") || _parameterProvider.IsStale)
            {
                _parent.RefreshData();
            }

            if (_environmentProvider.DrawEnvironmentDropdown())
            {
                // TODO: try to map to the same parameter in the other environment?
                // (they get different IDs so this would not be trivial)
                ClearSelectedParameter();
            }

            if (_parameterProvider.FetchInProgress)
            {
                EditorGUILayout.HelpBox("Fetching Parameters...", MessageType.Info);
            }

            if (_parameterProvider.HasData)
            {
                _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);
                foreach (DDNAEventManagerEventParameter parameter in _parameterProvider.Data)
                {
                    if (parameter.application == _parent.CurrentApplicationId)
                    {
                        GUIStyle buttonStyle = parameter.id == _selectedParameterId ? EditorStyles.boldLabel : EditorStyles.label;
                        if (GUILayout.Button(parameter.name, buttonStyle))
                        {
                            SelectNewParameter(parameter);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+"))
            {
                _mode = Mode.Create;
            }
            EditorGUILayout.EndVertical();
        }

        private void ClearSelectedParameter()
        {
            GUI.FocusControl(null);
            _selectedParameterId = 0;
            _selectedParameterObject = null;
            _mode = Mode.View;
        }

        private void SelectNewParameter(DDNAEventManagerEventParameter newParameter)
        {
            GUI.FocusControl(null);
            _selectedParameterId = newParameter.id;
            _selectedParameterObject = newParameter;
            _mode = Mode.View;
        }

        private List<string> GetEventNamesThatUseParameter(int parameterId)
        {
            List<string> events = new List<string>();

            if (_eventProvider.HasData)
            {
                foreach (DDNAEventManagerEvent e in _eventProvider.Data)
                {
                    foreach (DDNAEventManagerEventParameter p in e.parameters)
                    {
                        if (e.environment == _environmentProvider.CurrentEnvironmentId &&
                            p.id == parameterId)
                        {
                            events.Add(e.name);
                            break;
                        }
                    }
                }
            }

            return events;
        }

        private void DrawParameterViewer(DDNAEventManagerEventParameter parameter)
        {
            List<string> usingEvents = GetEventNamesThatUseParameter(parameter.id);

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Application ID", parameter.application.ToString());
            EditorGUILayout.LabelField("ID", parameter.id.ToString());
            EditorGUILayout.LabelField("Name", parameter.name);
            EventsManagerUI.SizedTextAreaLabel(_parent.position.width - LIST_WIDTH, "Description", parameter.description);

            EditorGUILayout.LabelField("Type", parameter.type);
            if (parameter.type == "STRING")
            {
                EditorGUILayout.LabelField("Format", parameter.format);
            }
            if (usingEvents.Count > 0)
            {
                EditorGUILayout.HelpBox("This parameter is in use by the following events: \n- " +
                                        String.Join("\n- ", usingEvents),
                                        MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("This parameter is not used by any events!",
                                        MessageType.Info);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawParameterCreator()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create New Parameter", EditorStyles.boldLabel);

            _newName = EditorGUILayout.TextField("Name", _newName);
            _newDescription = EventsManagerUI.WordWrappedTextField(_parent.position.width - LIST_WIDTH,
                                                                   "Description",
                                                                   _newDescription);
            
            _newType = (ParameterType)EditorGUILayout.EnumPopup("Type", _newType);
            
            if (_newType == ParameterType.String)
            {
                _newFormat = EditorGUILayout.TextField("Format", _newFormat);
            }

            if (String.IsNullOrEmpty(_newName))
            {
                EditorGUILayout.HelpBox("A name is required.", MessageType.Warning);
            }
            else if (_parameterCreator.NameIsInvalid(_newName))
            {
                EditorGUILayout.HelpBox("Parameter names must begin with a lower-case English letter (a-z), and otherwise can only include upper-case or lower-case English letters (a-z, A-Z), digits (0-9) or underscores (_).", MessageType.Warning);
            }
            else
            {
                bool nameAlreadyInUse = false;
                foreach (DDNAEventManagerEventParameter parameter in _parameterProvider.Data)
                {
                    if (_newName.Equals(parameter.name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        nameAlreadyInUse = true;
                        break;
                    }
                }

                if (nameAlreadyInUse)
                {
                    EditorGUILayout.HelpBox($"A parameter with the name {_newName} already exists.", MessageType.Warning);
                }
                else if (String.IsNullOrEmpty(_newDescription))
                {
                    EditorGUILayout.HelpBox("A description is required.", MessageType.Warning);
                }
                else if (GUILayout.Button("Create Parameter"))
                {
                    _mode = Mode.Creating;
                    CreateParameter();
                }
            }
            
            GUILayout.EndVertical();
        }

        private void CreateParameter()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                ["id"] = _selectedParameterId,
                ["name"] = _newName,
                ["description"] = _newDescription,
                ["type"] = TypeToString(_newType),
                ["application"] = _parent.CurrentApplicationId
            };

            if (_newType == ParameterType.String)
            {
                payload["format"] = _newFormat;
            }

            _parameterCreator.CreateParameter(_authProvider.AuthToken, payload);
        }

        private void ParameterCreationFailed()
        {
            _mode = Mode.Create;

            _parent.Repaint();
        }

        private void ParameterCreated(DDNAEventManagerEventParameter parameter)
        {
            _newDescription = null;
            _newFormat = null;
            _newName = null;
            _newType = ParameterType.String;

            _selectedParameterId = parameter.id;
            _selectedParameterObject = parameter;

            _parent.RefreshData();
            _mode = Mode.View;

            _parent.Repaint();
        }

        private string TypeToString(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Integer:
                    return "INTEGER";
                case ParameterType.Float:
                    return "FLOAT";
                case ParameterType.Bool:
                    return "BOOLEAN";
                case ParameterType.String:
                default:
                    // We only support the four primitive types in this editor, default is just to satisfy the compiler.
                    return "STRING";
            }
        }
    }
}
