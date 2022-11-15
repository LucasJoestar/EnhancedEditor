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
    /// <see cref="ScriptableObjectDrawerMode"/>-related utility class.
    /// </summary>
    public static class ScriptableObjectDrawerModeUtility {
        #region User Preferences
        private const string EditorPrefKey = EnhancedEditorSettings.EditorPrefsPath + "ScriptableObjectDrawerMode";
        private const ScriptableObjectDrawerMode DefaultModeValue = ScriptableObjectDrawerMode.Button | ScriptableObjectDrawerMode.Content;

        private static readonly GUIContent defaultModeGUI = new GUIContent("Scriptable Object default drawer mode",
                                                                    "The default mode used to draw a ScriptableObject in the editor.");

        private static ScriptableObjectDrawerMode defaultMode = (ScriptableObjectDrawerMode)(-1);

        /// <summary>
        /// The default mode used to draw a <see cref="ScriptableObject"/> in the editor.
        /// </summary>
        public static ScriptableObjectDrawerMode DefaultMode {
            get {
                if ((int)defaultMode == -1) {
                    defaultMode = (ScriptableObjectDrawerMode)EditorPrefs.GetInt(EditorPrefKey, (int)DefaultModeValue);
                }

                return defaultMode;
            }
            set {
                if (DefaultMode == value) {
                    return;
                }

                defaultMode = value;
                EditorPrefs.SetInt(EditorPrefKey, (int)value);
            }
        }

        // -----------------------

        [EnhancedEditorPreferences(Order = 50)]
        private static void DrawPreferences() {
            GUILayout.Space(10f);

            EnhancedEditorGUILayout.UnderlinedLabel(defaultModeGUI);
            GUILayout.Space(5f);

            DefaultMode = (ScriptableObjectDrawerMode)EditorGUILayout.EnumFlagsField(DefaultMode);
        }
        #endregion
    }
}
