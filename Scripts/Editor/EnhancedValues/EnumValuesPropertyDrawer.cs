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
    /// Custom <see cref="EnumValues{Enum, T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumValues<>), true)]
    public class EnumValuesPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        internal protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Make sure the target class have at least two generic arguments.
            Type[] _arguments = GetFieldInfo(_property).FieldType.GetGenericArguments();

            if (_arguments.Length < 2) {
                EditorGUI.PropertyField(_position, _property, _label);
                return _position.height;
            }

            float _height = 0f;

            // Header.
            _property.isExpanded = EditorGUI.Foldout(_position, _property.isExpanded, _label, true, EditorStyles.foldoutHeader);
            IncrementPosition();

            if (_property.isExpanded) {
                // Draw each enum value.
                //
                // As the EnumValues array is automatically refreshed on serialization,
                // its associated values and the enum names should have the same index.
                using (var _scope = new EditorGUI.IndentLevelScope(1)) {
                    Type _enum = _arguments[0];
                    string[] _names = Enum.GetNames(_enum);

                    SerializedProperty _arrayProperty = _property.FindPropertyRelative("Values");

                    for (int i = 0; i < _arrayProperty.arraySize; i++) {
                        SerializedProperty _valueProperty = _arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Second");

                        _position.height = EditorGUI.GetPropertyHeight(_valueProperty, true);
                        EditorGUI.PropertyField(_position, _valueProperty, EnhancedEditorGUIUtility.GetLabelGUI(_names[i]), true);

                        // Position increment.
                        IncrementPosition();
                    }
                }
            }

            // ----- Local Method ----- \\

            void IncrementPosition() {
                float _spacing = _position.height + EditorGUIUtility.standardVerticalSpacing;

                _height += _spacing;
                _position.y += _spacing;
            }

            return _height;
        }
        #endregion
    }
}
