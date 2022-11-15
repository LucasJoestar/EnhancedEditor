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
    public class SerializedTypePropertyDrawer : EnhancedPropertyEditor {
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

                Type _baseType = EnhancedEditorUtility.GetFieldInfoType(GetFieldInfo(_property));
                List<GUIContent> _temp = new List<GUIContent>();

                SerializedTypeConstraint _constraints = (SerializedTypeConstraint)_property.FindPropertyRelative("constraints").intValue;

                // Register all derived types.
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var _assembly in _assemblies) {
                    Type[] _types = _assembly.GetTypes();

                    foreach (var _type in _types) {
                        if (!_type.IsDefined(typeof(EtherealAttribute), false) && _baseType.IsAssignableFrom(_type)) {
                            AddType(_temp, _type, _constraints);
                        }
                    }
                }

                if (_constraints.HasFlag(SerializedTypeConstraint.BaseType)) {
                    AddType(_temp, _baseType, _constraints);
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

            // ----- Local Method ----- \\

            void AddType(List<GUIContent> _temp, Type _type, SerializedTypeConstraint _constraints) {
                if (_type.IsAbstract && !_constraints.HasFlag(SerializedTypeConstraint.Abstract)) {
                    return;
                }

                DisplayNameAttribute _attribute = _type.GetCustomAttribute<DisplayNameAttribute>();
                string _label = (_attribute != null)
                              ? _attribute.Label.text
                              : ObjectNames.NicifyVariableName(_type.Name);

                _temp.Add(new GUIContent(_label, _type.GetReflectionName()));
            }
        }
        #endregion
    }
}
