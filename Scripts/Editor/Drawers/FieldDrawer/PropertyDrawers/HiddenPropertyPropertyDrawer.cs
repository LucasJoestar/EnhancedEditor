// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="HiddenPropertyAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(HiddenPropertyAttribute))]
    public sealed class HiddenPropertyPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            _height = 0f;
            return !Settings.Value;
        }
        #endregion

        #region User Settings
        private static readonly GUIContent hiddenPropertiesGUI = new GUIContent("Display Hidden Properties", "Displays hidden properties in the inspector");
        private static readonly int settingsGUID = "EnhancedEditorHiddenProperties".GetHashCode();

        private static BooleanEnhancedSettings settings = null;

        // -----------------------

        /// <summary>
        /// Hidden properties related user settings.
        /// </summary>
        public static BooleanEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new BooleanEnhancedSettings(settingsGUID, false);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -------------------------------------------
        // Drawer
        // -------------------------------------------

        [EnhancedEditorUserSettings(Order = 25)]
        private static void DrawSettings() {
            GUILayout.Space(10f);

            var _settings = Settings;
            bool _toggle = EditorGUILayout.Toggle(hiddenPropertiesGUI, _settings.Value);

            if (_toggle != _settings.Value) {

                _settings.Value = _toggle;
                GUI.changed = true;
            }
        }
        #endregion
    }
}
