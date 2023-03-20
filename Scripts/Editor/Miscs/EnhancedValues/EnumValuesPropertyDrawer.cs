// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UMessageType = UnityEditor.MessageType;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="EnumValues{Enum, T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumValues<,>), true)]
    public class EnumValuesPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const UMessageType EmptyEnumMessageType = UMessageType.Info;
        private const string EmptyEnumMessage           = "This Enum does not contain any value";

        private const int CacheLimit = 20;
        private static readonly Dictionary<string, Type> enumInfos = new Dictionary<string, Type>();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Register this property to cache its enum type.
            string _key = EnhancedEditorUtility.GetSerializedPropertyID(_property);

            if (!enumInfos.TryGetValue(_key, out Type _enumType)) {
                // Clear cache on limit reach.
                if (enumInfos.Count > CacheLimit) {
                    enumInfos.Clear();
                }

                _enumType = EnhancedEditorUtility.GetFieldInfoType(GetFieldInfo(_property));
                enumInfos.Add(_key, _enumType);
            }

            float _height = 0f;

            // Button.
            _property.isExpanded = EditorGUI.Foldout(_position, _property.isExpanded, _label, true);

            IncrementPosition();

            if (_property.isExpanded) {
                // Draw each enum value.
                //
                // As the EnumValues array is automatically refreshed on serialization,
                // its associated values and the enum names should have the same index.
                using (var _scope = new EditorGUI.IndentLevelScope(1)) {
                    SerializedProperty _arrayProperty = _property.FindPropertyRelative("Values");

                    // Empty enum message.
                    if (_arrayProperty.arraySize == 0) {
                        _position.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(EmptyEnumMessage, EmptyEnumMessageType, _position.width);
                        EditorGUI.HelpBox(_position, EmptyEnumMessage, EmptyEnumMessageType);

                        IncrementPosition();
                        return _height;
                    }

                    for (int i = 0; i < _arrayProperty.arraySize; i++) {
                        SerializedProperty _elementProperty = _arrayProperty.GetArrayElementAtIndex(i);
                        SerializedProperty _enumProperty    = _elementProperty.FindPropertyRelative("First");
                        SerializedProperty _valueProperty   = _elementProperty.FindPropertyRelative("Second");

                        string _name = EnumUtility.GetName(_enumType, _enumProperty.intValue);

                        _position.height = EditorGUI.GetPropertyHeight(_valueProperty, true);
                        EditorGUI.PropertyField(_position, _valueProperty, EnhancedEditorGUIUtility.GetLabelGUI(_name), true);

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
