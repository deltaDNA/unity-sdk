using UnityEditor;
using UnityEngine;

namespace DeltaDNA
{
    internal static class EventsManagerUI
    {
        private static GUIStyle _leftAlignedButton;

        public static GUIStyle LeftAlignedButton
        {
            get
            {
                if (_leftAlignedButton == null)
                {
                    _leftAlignedButton = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _leftAlignedButton;
            }
        }

        public static void SizedTextAreaLabel(float areaWidth, string label, string content)
        {
            float textAreaWidth = areaWidth - EditorGUIUtility.labelWidth - 5.0f;
            GUIStyle descriptionStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField(label, content, descriptionStyle,
                                       GUILayout.MaxWidth(textAreaWidth),
                                       GUILayout.ExpandHeight(true));
        }

        public static string WordWrappedTextField(float areaWidth, string label, string content)
        {
            GUIContent descriptionContent = new GUIContent(content);
            float textAreaWidth = areaWidth - EditorGUIUtility.labelWidth - 5.0f;
            float textAreaHeight = EditorStyles.textArea.CalcHeight(descriptionContent, textAreaWidth);
            return EditorGUILayout.TextField(label,
                                             content,
                                             EditorStyles.textArea,
                                             GUILayout.MaxHeight(textAreaHeight));
        }
    }
}
