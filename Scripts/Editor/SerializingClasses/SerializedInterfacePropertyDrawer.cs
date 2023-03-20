// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="SerializedInterface{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializedInterface<>), true)]
    public class SerializedInterfacePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const int CacheLimit = 100;
        private static readonly Dictionary<string, Type> interfaceInfos = new Dictionary<string, Type>();
        private static readonly string SerializedInterfaceTypeName = typeof(SerializedInterface<>).Name;

        // -----------------------

        internal protected override float GetDefaultHeight(SerializedProperty _property, GUIContent _label) {
            SerializedProperty _componentProperty = _property.FindPropertyRelative("component");

            Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
            float _height = _position.height;

            if (_property.FindPropertyRelative("required").boolValue) {

                _height += EnhancedEditorGUI.GetRequiredExtraHeight(_position, _label, _componentProperty.objectReferenceValue);
            }

            return _height;
        }

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Register this property to cache its interface type.
            string _key = EnhancedEditorUtility.GetSerializedPropertyID(_property);

            if (!interfaceInfos.TryGetValue(_key, out Type _interfaceType)) {
                // Clear cache on limit reach.
                if (interfaceInfos.Count > CacheLimit) {
                    interfaceInfos.Clear();
                }
                
                _interfaceType = EnhancedEditorUtility.GetFieldInfoType(GetFieldInfo(_property));
                interfaceInfos.Add(_key, _interfaceType);
            }

            // Required field help box.
            SerializedProperty _componentProperty = _property.FindPropertyRelative("component");
            _position.height = EditorGUIUtility.singleLineHeight;

            float _extraHeight = 0f;

            if (_property.FindPropertyRelative("required").boolValue) {

                _extraHeight += EnhancedEditorGUI.RequiredHelpBox(_position, _label, _componentProperty.objectReferenceValue);
                Rect _temp = new Rect(_position) {
                    height = _extraHeight
                };

                using (var _scope = new EditorGUI.PropertyScope(_temp, GUIContent.none, _property)) { }
            }

            _position.y += _extraHeight;

            // Interface picker.
            EnhancedEditorGUI.PickerField(_position, _componentProperty, _label, _interfaceType);

            float _height = _position.height + _extraHeight;
            return _height;
        }

        // -----------------------

        [SerializedPropertyMenu]
        private static void OnContextMenu(GenericMenu _menu, SerializedProperty _property) {

            if (!_property.serializedObject.isEditingMultipleObjects && _property.type.StartsWith(SerializedInterfaceTypeName)
                && (_property.serializedObject.targetObject is Component _component)) {

                _menu.AddItem(EnhancedEditorGUI.requiredGetReferenceGUI, false, () => {

                    if (EnhancedEditorUtility.FindSerializedPropertyField(_property, out FieldInfo _field)) {
                        Type _type = EnhancedEditorUtility.GetFieldInfoType(_field).GetGenericArguments()[0];

                        if (_component.TryGetComponent(_type, out Component _interface)) {
                            SerializedProperty _componentProperty = _property.FindPropertyRelative("component");

                            _componentProperty.objectReferenceValue = _interface;
                            _componentProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                });
            }
        }
        #endregion
    }
}
