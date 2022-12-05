// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="ExtendedBehaviour"/>-related utility class.
    /// </summary>
    public static class ExtendedBehaviourEnhancedSettings {
        #region Project Settings
        private const string InfoMessage = "When enabled, a new ExtendedBehaviour component is automatically added to each GameObject in the scene.";

        private static readonly GUIContent headerGUI = new GUIContent("Extended Behaviour", "Extended Behaviour component related settings.");
        private static readonly GUIContent labelGUI = new GUIContent("Automatic Setup", "Toggles the extended behaviour automatic setup.");

        private static readonly int settingsGUID = "ExtendedBehaviour".GetHashCode();
        private static BooleanEnhancedSettings settings = null;

        /// <inheritdoc cref="ExtendedBehaviourEnhancedSettings"/>
        public static BooleanEnhancedSettings Settings {
            get {
                EnhancedEditorProjectSettings _projectSettings = EnhancedEditorProjectSettings.Instance;

                if ((settings == null) && !_projectSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new BooleanEnhancedSettings(settingsGUID, true);
                    _projectSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        public static bool AutomaticSetup {
            get { return Settings.Value; }
        }

        // -----------------------

        [EnhancedEditorProjectSettings(Order = 15)]
        private static void DrawSetting() {
            BooleanEnhancedSettings _setting = Settings;

            GUILayout.Space(10f);

            // Core scene system.
            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI);
            GUILayout.Space(2f);

            _setting.Value = EditorGUILayout.Toggle(labelGUI, _setting.Value);

            EditorGUILayout.HelpBox(InfoMessage, UnityEditor.MessageType.Info);
        }
        #endregion
    }
}
