// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="PolymorphValue{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(PolymorphValue<>), true)]
    public class PolymorphValuePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Type selection.
            float _height = EditorGUIUtility.singleLineHeight;
            _position.height = _height;

            using (var _scope = EnhancedGUI.GUIEnabled.Scope(true)) {
                EditorGUI.PropertyField(_position, _property.FindPropertyRelative("type"), _label);
            }

            // Value content.
            SerializedProperty _valueProperty = _property.FindPropertyRelative("value");

            if (_valueProperty.hasVisibleChildren) {
                // Foldout.
                _valueProperty.isExpanded = EditorGUI.Foldout(_position, _valueProperty.isExpanded, GUIContent.none, false);
                _position.y += _height;

                if (_valueProperty.isExpanded) {
                    _height += EditorGUIUtility.standardVerticalSpacing;
                    _position.y += EditorGUIUtility.standardVerticalSpacing;

                    // Single block field.
                    using (var _scope = new EditorGUI.IndentLevelScope(1)) {
                        EnhancedEditorGUI.BlockField(_position, _valueProperty, out float _extraHeight, false);
                        _height += _extraHeight;
                    }
                }
            }

            return _height;
        }
        #endregion
    }
}
