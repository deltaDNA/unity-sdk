using System;
using System.Diagnostics;
using UnityEditor;

namespace DeltaDNA
{
    [Serializable]
    public class Environment
    {
        public int id;
        public string game;
        public string name;
    }

    [Serializable]
    public class EnvironmentWrapper
    {
        public Environment[] Environments;
    }
    
    internal class EventsManagerEnvironmentProvider : EventsManagerDataProvider<Environment>
    {
        private const string ENDPOINT_URL = "https://api.deltadna.net/api/engage/v1/environments";

        private string[] _environmentDescriptions;
        private int _currentEnvironmentIndex;

        public int CurrentEnvironmentId
        {
            get
            {
                return HasData ? Data[_currentEnvironmentIndex].id : 0;
            }
        }

        public bool CurrentEnvironmentIsEditable
        {
            get
            {
                return HasData &&
                       String.Equals(Data[_currentEnvironmentIndex].name, "Dev", StringComparison.OrdinalIgnoreCase);
            }
        }

        public EventsManagerEnvironmentProvider() : base(ENDPOINT_URL)
        {
            _environmentDescriptions = new string[0];
            OnResponseArrived += RefreshEnvironmentDescriptions;
        }

        public override void Reset()
        {
            base.Reset();
            _currentEnvironmentIndex = 0;
        }

        public bool DrawEnvironmentDropdown()
        {
            int newEnvironment = EditorGUILayout.Popup(_currentEnvironmentIndex, _environmentDescriptions);
            if (newEnvironment != _currentEnvironmentIndex)
            {
                _currentEnvironmentIndex = newEnvironment;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RefreshEnvironmentDescriptions()
        {
            if (HasData)
            {
                _environmentDescriptions = new string[Data.Length];

                // NOTE: do this with indices as the arrays need to be parallel to ensure the dropdown selection
                // can translate from string description back to actual underlying data item.
                for (int i = 0; i < Data.Length; i++)
                {
                    _environmentDescriptions[i] = $"{Data[i].game} - {Data[i].name}";
                }

                if (_currentEnvironmentIndex > Data.Length)
                {
                    _currentEnvironmentIndex = 0;
                }
            }
        }
    }
}