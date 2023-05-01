// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="Separator"/> editor.
    /// </summary>
    [CustomEditor(typeof(Separator), true), CanEditMultipleObjects]
    public class SeparatorEditor : UnityObjectEditor {
        #region GUI Draw
        public override void OnInspectorGUI() {
            Rect _rect = EditorGUILayout.GetControlRect(false, -14f + Settings.Value); {
                _rect.yMin -= 25f;
                _rect.yMax += 10f;

                _rect.x = 0f;
                _rect.width = Screen.width;
            }

            EditorGUI.DrawRect(_rect, EnhancedEditorGUIUtility.GUIThemeBackgroundColor);
        }
        #endregion

        #region User Settings
        private const float DefaultHeight = 20f;

        private static readonly GUIContent separatorHeightGUI = new GUIContent("Separator Height", "");
        private static readonly int settingsGUID = "EnhancedEditorSeparatorSetting".GetHashCode();

        private static FloatEnhancedSettings settings = null;

        /// <summary>
        /// Separator-related user settings.
        /// </summary>
        public static FloatEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new FloatEnhancedSettings(settingsGUID, DefaultHeight);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }


        // -----------------------

        [EnhancedEditorUserSettings(Order = 20)]
        private static void DrawSettings() {
            var _settings = Settings;
            float _height = EditorGUILayout.Slider(separatorHeightGUI, _settings.Value, 0f, 100f);

            if (_height != _settings.Value) {

                _settings.Value = _height;
                GUI.changed = true;
            }
        }
        #endregion
    }
}
