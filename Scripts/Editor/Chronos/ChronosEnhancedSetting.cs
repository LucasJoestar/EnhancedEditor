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
    /// Chronos-related <see cref="EnhancedSettings"/> class.
    /// </summary>
    [Serializable]
    public class ChronosEnhancedSettings : EnhancedSettings {
        #region Global Members
        public InputAction IncreaseTimeScale = new InputAction();
        public InputAction ResetTimeScale = new InputAction();
        public InputAction DecreaseTimeScale = new InputAction();

        // -----------------------

        /// <inheritdoc cref="ChronosEnhancedSettings"/>
        public ChronosEnhancedSettings(int _guid) : base(_guid) {
            Keyboard _keyboard = Keyboard.current;

            if (_keyboard != null) {
                IncreaseTimeScale.AddBinding(_keyboard.numpadPlusKey);
                ResetTimeScale.AddBinding(_keyboard.numpadMultiplyKey);
                DecreaseTimeScale.AddBinding(_keyboard.numpadMinusKey);
            }
        }
        #endregion

        #region Settings
        private static readonly int settingsGUID = "EnhancedEditorChronosSetting".GetHashCode();

        private static readonly GUIContent headerGUI = new GUIContent("Chronos Inputs [Increase - Reset - Decrease]", "Chronos-related input shortcuts");
        private static readonly GUIContent applyGUI = new GUIContent("Apply", "Apply selected inputs");

        private static ChronosEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty = null;

        /// <inheritdoc cref="ChronosEnhancedSettings"/>
        public static ChronosEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty.serializedObject.targetObject == null))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new ChronosEnhancedSettings(settingsGUID);
                    settingsProperty = _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorUserSettings(Order = 100)]
        public static void DrawSettings() {
            // Initialize setting.
            var _ = Settings;

            // Draw inputs.
            GUILayout.Space(10f);
            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI);
            GUILayout.Space(5f);

            EnhancedEditorGUILayout.BlockField(settingsProperty);

            // Save button.
            GUILayout.Space(3f);
            Rect _position = EditorGUILayout.GetControlRect(true, 15f);
            {
                _position.width = 70f;
            }

            if (GUI.Button(_position, applyGUI, EditorStyles.miniButton)) {
                EnhancedEditorUserSettings.Instance.Save();
            }
        }
        #endregion
    }
}
#endif
