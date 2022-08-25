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
    /// Custom <see cref="SerializedType{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializedType<>), true)]
    public class SerializedITypePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const int CacheLimit = 25;
        private static readonly Dictionary<string, GUIContent[]> interfaceInfos = new Dictionary<string, GUIContent[]>();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Register this property to cache its selectable type values.
            string _key = _property.propertyPath;

            if (!interfaceInfos.TryGetValue(_key, out GUIContent[] _selectableTypes)) {
                // Clear cache on limit reach.
                if (interfaceInfos.Count > CacheLimit) {
                    interfaceInfos.Clear();
                }

                Type _baseType = EnhancedEditorUtility.GetFieldInfoType(fieldInfo);
                List<GUIContent> _temp = new List<GUIContent>();

                if (_property.FindPropertyRelative("canBeBaseType").boolValue) {
                    _temp.Add(new GUIContent(_baseType.Name, _baseType.GetReflectionName()));
                }

                // Register all derived types.
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var _assembly in _assemblies) {
                    Type[] _types = _assembly.GetTypes();
                    foreach (var _type in _types) {
                        if (!_type.IsAbstract && _type.IsSubclassOf(_baseType)) {
                            _temp.Add(new GUIContent(_type.Name, _type.GetReflectionName()));
                        }
                    }
                }

                _selectableTypes = _temp.ToArray();
                interfaceInfos.Add(_key, _selectableTypes);
            }

            SerializedProperty _typeNameProperty = _property.FindPropertyRelative("typeName");
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            // Type picker.
            int _selectedValue = Array.FindIndex(_selectableTypes, (t) => t.tooltip == _typeNameProperty.stringValue);

            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                _selectedValue = EditorGUI.Popup(_position, _label, _selectedValue, _selectableTypes);

                if (_changeCheck.changed) {
                    _typeNameProperty.stringValue = (_selectedValue != -1)
                                                  ? _selectableTypes[_selectedValue].tooltip
                                                  : string.Empty;

                    GUI.changed = true;
                }
            }

            return _height;
        }
        #endregion
    }
}
