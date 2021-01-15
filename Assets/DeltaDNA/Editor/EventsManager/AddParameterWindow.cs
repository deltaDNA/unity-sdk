using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    internal class AddParameterWindow : PopupWindowContent
    {
        private EventsManagerEventsTab _parent;

        private Vector2 _scrollPosition;
        private List<DDNAEventManagerEventParameter> _addableParameters;

        private bool _newParameterRequired;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(350.0f, 250.0f);
        }

        public void Initialise(EventsManagerEventsTab parent,
                               DDNAEventManagerEvent currentEvent,
                               ParameterProvider parameterProvider)
        {
            _parent = parent;

            HashSet<int> existingParameters = FindParametersAlreadyOnEvent(currentEvent.parameters, parameterProvider);
            _addableParameters = FilterParameterList(existingParameters, parameterProvider.Data);
        }

        private HashSet<int> FindParametersAlreadyOnEvent(List<DDNAEventManagerEventParameter> existingParameters,
                                                          ParameterProvider allParameters)
        {
            HashSet<int> existingParameterIds = new HashSet<int>();
            foreach (DDNAEventManagerEventParameter parameter in existingParameters)
            {
                existingParameterIds.Add(parameter.id);
            }

            // NOTE: now we need to find the IDs of the special parameters,
            // as they are not included in the Event's own listing (they're outside eventParams)
            foreach (int parameterId in allParameters.SpecialParameterIDs)
            {
                existingParameterIds.Add(parameterId);
            }

            return existingParameterIds;
        }

        private List<DDNAEventManagerEventParameter> FilterParameterList(HashSet<int> existingParameters,
                                                                         DDNAEventManagerEventParameter[] allParameters)
        {
            List<DDNAEventManagerEventParameter> parameters = new List<DDNAEventManagerEventParameter>();

            foreach (DDNAEventManagerEventParameter apiParameter in allParameters)
            {
                if (!existingParameters.Contains(apiParameter.id))
                {
                    parameters.Add(apiParameter);
                }
            }

            return parameters;
        }

        public override void OnGUI(Rect rect)
        {
            _newParameterRequired = EditorGUILayout.Toggle("Required", _newParameterRequired);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (DDNAEventManagerEventParameter apiParameter in _addableParameters)
            {
                if (GUILayout.Button($"{apiParameter.name} ({apiParameter.type})", EventsManagerUI.LeftAlignedButton))
                {
                    AddParameter(apiParameter.id);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void AddParameter(int parameterId)
        {
            _parent.AddParameterToCurrentEvent(parameterId, _newParameterRequired);
            editorWindow.Close();
        }
    }
}
