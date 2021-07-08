using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    [InitializeOnLoadAttribute]
    public static class PlayModeHelper
    {
        static PlayModeHelper()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                DDNA.ResetSingletonState();
            }
        }
    }
}
