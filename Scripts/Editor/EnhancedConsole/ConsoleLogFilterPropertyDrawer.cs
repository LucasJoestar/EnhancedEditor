// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="ConsoleLogFilter"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConsoleLogFilter), true)]
    public class ConsoleLogFilterPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private static readonly GUIContent enabledGUI = new GUIContent(EditorGUIUtility.FindTexture("scenevis_visible_hover"), "Toggles this filter activation (currently enabled)");
        private static readonly GUIContent disabledGUI = new GUIContent(EditorGUIUtility.FindTexture("SceneViewVisibility"), "Toggles this filter activation (currently disabled)");

        private static readonly GUIContent previewHeaderGUI = new GUIContent("Preview", "Log preview with this filter");
        private static readonly GUIContent previewLabelGUI = new GUIContent("Here is a preview log", string.Empty);

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            _position.height = EditorGUIUtility.singleLineHeight;

            // Header with the filter name next to a foldout, and an enabled toggle.
            SerializedProperty _nameProperty = _property.FindPropertyRelative("Name");
            SerializedProperty _enabledProperty = _property.FindPropertyRelative("Enabled");

            _property.isExpanded = EditorGUI.Foldout(_position, _property.isExpanded, GUIContent.none, false);

            Rect _temp = new Rect(_position) {
                xMin = _position.xMin + EnhancedEditorGUIUtility.FoldoutWidth,
                xMax = _position.xMax - (EnhancedEditorGUIUtility.IconWidth + 5f),
            };

            EditorGUI.PropertyField(_temp, _nameProperty, GUIContent.none);

            _temp.xMin = _temp.xMax + 5f;
            _temp.xMax = _position.xMax;

            using (var _scope = new EditorGUI.PropertyScope(_temp, GUIContent.none, _enabledProperty)) {
                bool _enabled = _enabledProperty.boolValue;
                GUIContent _gui = _enabled
                                ? enabledGUI
                                : disabledGUI;

                if (GUI.Button(_temp, _gui, EditorStyles.label)) {
                    _enabledProperty.boolValue = !_enabled;
                }
            }

            // Draw the filter content as a block.
            float _extraHeight = 0f;

            if (_property.isExpanded) {
                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;

                using (var _scope = new EditorGUI.DisabledGroupScope(!_enabledProperty.boolValue)) {
                    EnhancedEditorGUI.BlockField(_position, _property, out _extraHeight, false);
                }

                _extraHeight += 5f;

                _position.y += _extraHeight + EditorGUIUtility.standardVerticalSpacing;
                _position.height += EditorGUIUtility.standardVerticalSpacing;

                // Filter preview.
                _extraHeight += DrawPreview(_position, _property) + (EditorGUIUtility.standardVerticalSpacing * 2f);
            }

            return _position.height + _extraHeight;
        }

        private static float DrawPreview(Rect _position, SerializedProperty _property) {
            float _height = _position.height;

            // Header.
            EditorGUI.LabelField(_position, previewHeaderGUI, EditorStyles.boldLabel);

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            _height += _position.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.DrawRect(_position, SuperColor.Black.Get());

            _position.x += 1f;
            _position.y += 1f;
            _position.width -= 2f;
            _position.height -= 2f;

            // Background color.
            Color _color = EnhancedEditorGUIUtility.GUIPeerLineColor;
            EditorGUI.DrawRect(_position, _color);

            if (_property.FindPropertyRelative("UseColor").boolValue) {
                EditorGUI.DrawRect(_position, _property.FindPropertyRelative("Color").colorValue);
            }

            _color = _property.FindPropertyRelative("UseTextColor").boolValue ? _property.FindPropertyRelative("TextColor").colorValue : GUI.contentColor;

            // Icon.
            GUIContent _content = EnhancedEditorGUIUtility.GetLabelGUI(string.Empty);
            Rect _iconPosition = new Rect(_position) {
                x = _position.x + 5f,
                y = _position.y + 1f,
                width = EnhancedEditorGUIUtility.IconWidth,
                yMax = _position.yMax - 1f,
            };

            _content.image = _property.FindPropertyRelative("Icon").objectReferenceValue as Texture;
            _position.xMin = _iconPosition.xMax + 2f;

            EditorGUI.LabelField(_iconPosition, _content);

            // Log string.
            using (var _contentScope = EnhancedGUI.GUIContentColor.Scope(_color)) {
                EditorGUI.LabelField(_position, previewLabelGUI, EnhancedConsoleWindow.Styles.LogStyle);
            }

            return _height;
        }
        #endregion
    }
}
