// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Enum value used to determine how a <see cref="ScriptableObject"/> is drawn in the editor.
    /// </summary>
    [Flags]
    public enum ScriptableObjectDrawerMode {
        None = 0,
        Button,
        Content,
    }

    /// <summary>
    /// ScriptableObject drawer related <see cref="EnhancedSettings"/> class.
    /// </summary>
    [Serializable]
    public class ScriptableObjectDrawerEnhancedSettings : EnhancedSettings {
        #region Global Members
        public ScriptableObjectDrawerMode DefaultMode = ScriptableObjectDrawerMode.Button | ScriptableObjectDrawerMode.Content;

        // -----------------------

        /// <inheritdoc cref="ScriptableObjectDrawerEnhancedSettings"/>
        public ScriptableObjectDrawerEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Settings
        private static readonly GUIContent defaultModeGUI = new GUIContent("Scriptable Object default drawer mode",
                                                                           "The default mode used to draw a ScriptableObject in the editor.");

        private static readonly int settingsGUID = "EnhancedEditorScriptableObjectDrawerSetting".GetHashCode();
        private static ScriptableObjectDrawerEnhancedSettings settings = null;

        /// <inheritdoc cref="ScriptableObjectDrawerEnhancedSettings"/>
        public static ScriptableObjectDrawerEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new ScriptableObjectDrawerEnhancedSettings(settingsGUID);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorUserSettings(Order = 70)]
        private static void DrawSettings() {
            GUILayout.Space(10f);

            EnhancedEditorGUILayout.UnderlinedLabel(defaultModeGUI);
            GUILayout.Space(5f);

            Settings.DefaultMode = (ScriptableObjectDrawerMode)EditorGUILayout.EnumFlagsField(Settings.DefaultMode);
        }
        #endregion
    }
}
