// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //
#if ENABLE_INPUT_SYSTEM
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="Chronos"/>-related preferences drawer class.
    /// </summary>
    public static class ChronosPreferences {
        #region Chronos Input
        [Serializable]
        private class InputWrapper {
            public InputAction IncreaseTimeScale = new InputAction();
            public InputAction ResetTimeScale = new InputAction();
            public InputAction DecreaseTimeScale = new InputAction();
        }

        /// <summary>
        /// Inputs wrapper as a scriptable object, used to create a serialized property for it.
        /// </summary>
        private class Inputs : ScriptableObject {
            public InputWrapper Wrapper = new InputWrapper();
        }
        #endregion

        #region User Preferences
        private const string EditorPrefKey = EnhancedEditorSettings.EditorPrefsPath + "Chronos";

        private static readonly GUIContent headerGUI = new GUIContent("Chronos Inputs [Increase - Reset - Decrease]", "Chronos-related input shortcuts");
        private static readonly GUIContent applyGUI = new GUIContent("Apply", "Apply selected inputs");

        private static Inputs inputs = null;
        private static SerializedObject serializedObject = null;
        private static SerializedProperty serializedProperty = null;

        // -----------------------

        [EnhancedEditorPreferences(Order = 100)]
        public static void DrawPreferences() {
            // Get serialized object.
            if (inputs == null) {
                string _json = EditorPrefs.GetString(EditorPrefKey, string.Empty);
                inputs = ScriptableObject.CreateInstance<Inputs>();

                if (!string.IsNullOrEmpty(_json)) {
                    EditorJsonUtility.FromJsonOverwrite(_json, inputs.Wrapper);
                } else if (Keyboard.current != null) {
                    inputs.Wrapper.IncreaseTimeScale.AddBinding(Keyboard.current.numpadPlusKey);
                    inputs.Wrapper.ResetTimeScale.AddBinding(Keyboard.current.numpadMultiplyKey);
                    inputs.Wrapper.DecreaseTimeScale.AddBinding(Keyboard.current.numpadMinusKey);
                }

                serializedObject = new SerializedObject(inputs);
                serializedProperty = serializedObject.FindProperty("Wrapper");
            }

            // Draw inputs.
            GUILayout.Space(10f);
            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI);
            GUILayout.Space(5f);

            serializedObject.Update();
            EnhancedEditorGUILayout.BlockField(serializedProperty);

            serializedObject.ApplyModifiedProperties();

            // Save button.
            GUILayout.Space(3f);
            Rect _position = EditorGUILayout.GetControlRect(true, 15f); {
                _position.width = 70f;
            }

            if (GUI.Button(_position, applyGUI, EditorStyles.miniButton)) {
                PlayerPrefs.SetString(Chronos.IncreaseInputKey, JsonUtility.ToJson(inputs.Wrapper.IncreaseTimeScale));
                PlayerPrefs.SetString(Chronos.ResetInputKey, JsonUtility.ToJson(inputs.Wrapper.ResetTimeScale));
                PlayerPrefs.SetString(Chronos.DecreaseInputKey, JsonUtility.ToJson(inputs.Wrapper.DecreaseTimeScale));

                string _json = EditorJsonUtility.ToJson(inputs.Wrapper);
                EditorPrefs.SetString(EditorPrefKey, _json);
            }
        }
        #endregion
    }
}
#endif
