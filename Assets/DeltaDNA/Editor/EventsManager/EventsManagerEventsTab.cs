using System;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    internal class EventsManagerEventsTab
    {
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
        private readonly EventProvider _eventProvider;
        private readonly ParameterProvider _parameterProvider;
        private readonly EventsManagerEventEditor _eventEditor;

        private Mode _mode;
        private int _selectedEventId;
        private DDNAEventManagerEvent _selectedEventObject;
        private Vector2 _listScrollPosition;
        private Vector2 _parameterFoldoutPosition;
        private bool[] _parameterFoldouts;
        private Rect _addParameterButtonRect;

        private string _newEventName;
        private string _newEventDescription;

        public EventsManagerEventsTab(EventsManagerWindow parent,
                                      EventsManagerAuthProvider authProvider,
                                      EventsManagerEnvironmentProvider environmentProvider,
                                      EventProvider eventProvider,
                                      ParameterProvider parameterProvider)
        {
            _parent = parent;
            _authProvider = authProvider;
            _environmentProvider = environmentProvider;
            _eventProvider = eventProvider;
            _parameterProvider = parameterProvider;
            _eventEditor = new EventsManagerEventEditor(parent);

            _eventEditor.OnEventCreated += EventCreated;
            _eventEditor.OnEventCreationFailed += EventCreationFailed;

            ClearSelectedEvent();
        }

        public void AddParameterToCurrentEvent(int parameterId, bool required)
        {
            _eventEditor.AddParameter(_selectedEventId, parameterId, required, _authProvider.AuthToken);
        }

        public void Refresh()
        {
            // Relink the current event object, as it may no longer contain the same data
            // as before. The ID should never change after creation, but somebody may have changed
            // the underlying data via the website.
            if (_mode == Mode.View &&
                _selectedEventId > 0 &&
                _eventProvider.HasData)
            {
                foreach (DDNAEventManagerEvent e in _eventProvider.Data)
                {
                    if (e.id == _selectedEventId)
                    {
                        SelectEvent(e);
                    }
                }
            }
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            DrawEventList();

            switch (_mode)
            {
                case Mode.View:
                    DrawEventViewer();
                    break;
                case Mode.Create:
                    DrawEventCreator();
                    break;
                case Mode.Creating:
                    EditorGUILayout.HelpBox("Creating event...", MessageType.Info);
                    break;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEventList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LIST_WIDTH));
            if (GUILayout.Button("Refresh") || _eventProvider.IsStale)
            {
                _parent.RefreshData();
            }

            if (_environmentProvider.DrawEnvironmentDropdown())
            {
                // TODO: try to map to the same event in the other environment?
                // (they get different IDs so this would not be trivial)
                ClearSelectedEvent();
            }

            if (_eventProvider.FetchInProgress)
            {
                EditorGUILayout.HelpBox("Fetching Events...", MessageType.Info);
            }

            if (_eventProvider.HasData)
            {
                _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);
                foreach (DDNAEventManagerEvent apiEvent in _eventProvider.Data)
                {
                    // TODO: this is cheating, there's an environmentId= querystring parameter!
                    if (apiEvent.environment == _environmentProvider.CurrentEnvironmentId)
                    {
                        GUIStyle buttonStyle = apiEvent.id == _selectedEventId ? EditorStyles.boldLabel : EditorStyles.label;
                        if (GUILayout.Button(apiEvent.name, buttonStyle))
                        {
                            SelectEvent(apiEvent);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            if (_environmentProvider.CurrentEnvironmentIsEditable &&
                GUILayout.Button("+"))
            {
                _mode = Mode.Create;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEventViewer()
        {
            if (_selectedEventId > 0)
            {
                if (_environmentProvider.CurrentEnvironmentIsEditable)
                {
                    DrawSelectedEventEditor();
                }
                else
                {
                    DrawSelectedEventViewer();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Choose an event", MessageType.Info);
            }
        }

        private void ClearSelectedEvent()
        {
            GUI.FocusControl(null);
            _selectedEventId = 0;
            _selectedEventObject = null;
            _parameterFoldouts = new bool[0];
            _mode = Mode.View;
        }

        private void SelectEvent(DDNAEventManagerEvent e)
        {
            GUI.FocusControl(null);
            _selectedEventId = e.id;
            _selectedEventObject = e;
            _mode = Mode.View;

            if (e.id != _selectedEventId ||
                e.parameters.Count != _parameterFoldouts.Length)
            {
                // NOTE: if length has changed, we've either added or removed a parameter
                // Order is not guaranteed so there's no point trying to preserve which parameters are currently 
                // unfolded or not, so just blat the array with a new one.
                _parameterFoldouts = new bool[e.parameters.Count];
            }
        }

        private void DrawSelectedEventViewer()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Events cannot be edited in this environment.\n" +
                                    "To create new events or edit existing events, switch to a Dev environment!", MessageType.Info);

            DrawReadOnlyEventFields();
            DrawParametersList(false);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedEventEditor()
        {
            EditorGUILayout.BeginVertical();

            DrawReadOnlyEventFields();
            DrawParametersList(true);

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool needsPublishing = _eventProvider.IsDirty(_selectedEventObject);

            if (needsPublishing &&
                GUILayout.Button("Publish", GUILayout.Width(LIST_WIDTH)))
            {
                bool confirmPublish = EditorUtility.DisplayDialog("Publish Event",
                                            "Are you sure you want to publish this event to Live?\n" +
                                                "If you have deleted parameters from an event that has already been published," +
                                                " you may lose data.",
                                            "Yes",
                                            "No");
                if (confirmPublish)
                {
                    _eventEditor.PublishEvent(_selectedEventId, _authProvider.AuthToken);
                }
            }

            if (GUILayout.Button("Add Parameter", GUILayout.Width(LIST_WIDTH)))
            {
                AddParameterWindow popup = new AddParameterWindow();
                popup.Initialise(this, _selectedEventObject, _parameterProvider);
                PopupWindow.Show(_addParameterButtonRect, popup);
            }
            if (Event.current.type == EventType.Repaint)
            {
                _addParameterButtonRect = GUILayoutUtility.GetLastRect();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawReadOnlyEventFields()
        {
            EditorGUILayout.LabelField("Application ID", _selectedEventObject.application.ToString());
            EditorGUILayout.LabelField("ID", _selectedEventObject.id.ToString());
            EditorGUILayout.LabelField("Name", _selectedEventObject.name);
            EventsManagerUI.SizedTextAreaLabel(_parent.position.width - LIST_WIDTH, "Description", _selectedEventObject.description);
        }

        private void DrawParametersList(bool editable)
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

            // TODO: although the fields need to be evenly sized, they could be a little bit adaptable.
            // Find the width of the area and divide by 3?
            float fieldWidth = 150.0f;

            _parameterFoldoutPosition = EditorGUILayout.BeginScrollView(_parameterFoldoutPosition);
            for (int i = 0; i < _selectedEventObject.parameters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
#if UNITY_2019_4_OR_NEWER
                // TODO: remove this once 2018.4 LTS support is dropped
                _parameterFoldouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(_parameterFoldouts[i],
                                                                                _selectedEventObject.parameters[i].name);
#else
                // NOTE: this old foldout can only be folded by clicking the arrow, whereas the new one
                // accepts clicks on the entire text area as well as the arrow.
                _parameterFoldouts[i] = EditorGUILayout.Foldout(_parameterFoldouts[i],
                                                                _selectedEventObject.parameters[i].name);
#endif
                if (_parameterFoldouts[i])
                {
                    DrawParameterDetail(_selectedEventObject.parameters[i], fieldWidth);
                }
#if UNITY_2019_4_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif
                EditorGUILayout.EndVertical();
                if (editable)
                {
                    if (_parameterProvider.LockedParameterIDs.Contains(_selectedEventObject.parameters[i].id))
                    {
                        GUILayout.Space(50.0f);
                    }
                    else if (GUILayout.Button("Delete", GUILayout.Width(50.0f)))
                    {
                        _eventEditor.RemoveParameter(_selectedEventId, _selectedEventObject.parameters[i].id, _authProvider.AuthToken);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawParameterDetail(DDNAEventManagerEventParameter parameter, float fieldWidth)
        {
            bool isString = parameter.type == "STRING";

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Required", EditorStyles.boldLabel, GUILayout.Width(fieldWidth));
            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.Width(fieldWidth));
            if (isString)
            {
                EditorGUILayout.LabelField("Format", EditorStyles.boldLabel, GUILayout.Width(fieldWidth));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(parameter.required.ToString(), GUILayout.Width(fieldWidth));
            EditorGUILayout.LabelField(parameter.type, GUILayout.Width(fieldWidth));
            if (isString)
            {
                EditorGUILayout.LabelField(parameter.format, GUILayout.Width(fieldWidth));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private void DrawEventCreator()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create New Event", EditorStyles.boldLabel);

            _newEventName = EditorGUILayout.TextField("Event Name", _newEventName);
            _newEventDescription = EventsManagerUI.WordWrappedTextField(_parent.position.width - LIST_WIDTH,
                                                                        "Description",
                                                                        _newEventDescription);

            if (String.IsNullOrEmpty(_newEventName))
            {
                EditorGUILayout.HelpBox("A name is required.", MessageType.Warning);
            }
            else if (_eventEditor.NameIsInvalid(_newEventName))
            {
                EditorGUILayout.HelpBox("Event names can only include upper-case or lower-case English letters (a-z, A-Z), digits (0-9) or underscores (_).", MessageType.Warning);
            }
            else
            {
                bool nameAlreadyInUse = false;
                foreach (DDNAEventManagerEvent apiEvent in _eventProvider.Data)
                {
                    if (apiEvent.environment == _environmentProvider.CurrentEnvironmentId &&
                        _newEventName.Equals(apiEvent.name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        nameAlreadyInUse = true;
                        break;
                    }
                }

                if (nameAlreadyInUse)
                {
                    EditorGUILayout.HelpBox($"An event with the name {_newEventName} already exists.", MessageType.Warning);
                }
                else if (String.IsNullOrEmpty(_newEventDescription))
                {
                    EditorGUILayout.HelpBox("A description is required.", MessageType.Warning);
                }
                else if (GUILayout.Button("Create Event"))
                {
                    _mode = Mode.Creating;
                    _eventEditor.CreateEvent(_newEventName,
                                             _newEventDescription,
                                             _environmentProvider.CurrentEnvironmentId,
                                             _authProvider.AuthToken);
                }
            }
            GUILayout.EndVertical();
        }

        private void EventCreationFailed()
        {
            _mode = Mode.Create;

            _parent.Repaint();
        }

        private void EventCreated(DDNAEventManagerEvent createdEvent)
        {
            _newEventName = null;
            _newEventDescription = null;

            SelectEvent(createdEvent);

            _parent.RefreshData();
            _mode = Mode.View;

            _parent.Repaint();
        }
    }
}
