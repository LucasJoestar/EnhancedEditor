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
    public static class ExtendedBehaviourUtility {
        #region Project Settings
        private const string InfoMessage = "When enabled, a new ExtendedBehaviour component is automatically added to each GameObject in the scene.";
        private static readonly int settingGUID = "ExtendedBehaviour".GetHashCode();

        private static readonly GUIContent headerGUI = new GUIContent("Extended Behaviour", "Extended Behaviour component related settings.");
        private static readonly GUIContent labelGUI = new GUIContent("Automatic Setup", "Toggles the extended behaviour automatic setup.");

        public static BooleanProjectSetting ExtendedBehaviourSetting {
            get {
                EnhancedEditorSettings _editorSettings = EnhancedEditorSettings.Settings;

                if (!_editorSettings.GetProjectSetting(settingGUID, out BooleanProjectSetting _setting)) {
                    _setting = new BooleanProjectSetting(settingGUID, true);
                    _editorSettings.AddProjectSetting(_setting);
                }

                return _setting;
            }
        }

        public static bool AutomaticSetup {
            get { return ExtendedBehaviourSetting.Value; }
        }

        // -----------------------

        [EnhancedEditorProjectSettings(Order = 15)]
        private static void DrawProjectSetting() {
            BooleanProjectSetting _setting = ExtendedBehaviourSetting;

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
