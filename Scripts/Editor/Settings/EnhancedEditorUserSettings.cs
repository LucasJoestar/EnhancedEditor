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
    /// <see cref="EnhancedEditor"/>-related user settings.
    /// </summary>
    [Serializable]
    public class EnhancedEditorUserSettings : EnhancedEditorSettings {
        #region Global Members
        private static string EditorPrefsKey => $"{PlayerSettings.productGUID}-{null}_EnhancedEditorUserSettings";
        private static EnhancedEditorUserSettings instance = null;

        /// <inheritdoc cref="EnhancedEditorUserSettings"/>
        public static EnhancedEditorUserSettings Instance {
            get {
                if (instance == null) {
                    try {
                        string _json = EditorPrefs.GetString(EditorPrefsKey, string.Empty);
                        instance = CreateInstance<EnhancedEditorUserSettings>();

                        if (string.IsNullOrEmpty(_json)) {
                            instance.Save();
                        } else {
                            EditorJsonUtility.FromJsonOverwrite(_json, instance);
                        }
                    } catch (UnityException) {
                        return null;
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Opens the Preferences window with the <see cref="EnhancedEditor"/> settings already selected.
        /// </summary>
        [MenuItem(MenuPath + "User Settings", false, -50), Button(SuperColor.Green, IsDrawnOnTop = false)]
        public static EditorWindow OpenSettings() {
            EditorWindow _preferences = SettingsService.OpenUserPreferences(UserSettingsPath);
            return _preferences;
        }

        /// <summary>
        /// Main editor toolbar extension used to open the <see cref="EnhancedEditor"/> preferences settings.
        /// </summary>
        [EditorToolbarRightExtension(Order = 500)]
        private static void ToolbarExtension() {
            #if SCENEVIEW_TOOLBAR
            GUILayout.Space(10f);
            #elif EDITOR_TOOLBAR
            GUILayout.FlexibleSpace();
            #endif

            if (EnhancedEditorToolbar.Button(Styles.UserSettingsButtonGUI, GUILayout.Width(32f))) {
                OpenSettings();
            }

            #if EDITOR_TOOLBAR
            GUILayout.Space(25f);
            #endif
        }

        public override void Save() {
            string _json = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(EditorPrefsKey, _json);
        }
        #endregion
    }
}
