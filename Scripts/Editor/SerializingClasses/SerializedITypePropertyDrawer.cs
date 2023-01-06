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
        private const int CacheLimit = 100;
        private static readonly Dictionary<string, GUIContent[]> interfaceInfos = new Dictionary<string, GUIContent[]>();

        private static readonly GUIContent nullGUI = new GUIContent("<None>", string.Empty);

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // Register this property to cache its selectable type values.
            string _key = EnhancedEditorUtility.GetSerializedPropertyID(_property);

            if (!interfaceInfos.TryGetValue(_key, out GUIContent[] _selectableTypes)) {
                // Clear cache on limit reach.
                if (interfaceInfos.Count > CacheLimit) {
                    interfaceInfos.Clear();
                }

                Type _baseType = EnhancedEditorUtility.GetFieldInfoType(GetFieldInfo(_property));
                List<GUIContent> _temp = new List<GUIContent>();

                // Get required interfaces.
                SerializedProperty _interfacesProperty = _property.FindPropertyRelative("interfaces");

                Type[] _interfaces = new Type[_interfacesProperty.arraySize];
                for (int i = 0; i < _interfaces.Length; i++) {
                    _interfaces[i] = Type.GetType(_interfacesProperty.GetArrayElementAtIndex(i).stringValue);
                }

                SerializedTypeConstraint _constraints = (SerializedTypeConstraint)_property.FindPropertyRelative("constraints").intValue;

                // Register all derived types.
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var _assembly in _assemblies) {
                    try {
                        Type[] _types = _assembly.GetTypes();

                        foreach (var _type in _types) {
                            if (!_type.IsDefined(typeof(EtherealAttribute), false) && _baseType.IsAssignableFrom(_type)
                                                                                   && SerializedTypeUtility.IsAssignableFrom(_type, _interfaces)
                                                                                   && (_type != _baseType)) {
                                AddType(_temp, _type, _constraints);
                            }
                        }
                    } catch { }
                }

                if (_constraints.HasFlag(SerializedTypeConstraint.BaseType)) {
                    AddType(_temp, _baseType, _constraints);
                }

                // Sort and add separators.
                _temp.Sort((a, b) => {
                    bool _aSpecial = a.text.StartsWith("<");
                    bool _bSpecial = b.text.StartsWith("<");

                    return _aSpecial == _bSpecial ? a.text.CompareTo(b.text) : _aSpecial ? 1 : -1;
                });

                if (_constraints.HasFlag(SerializedTypeConstraint.Null)) {
                    _temp.Add(nullGUI);
                }

                string _lastPrefix = ".";
                string _lastSeparatorPrefix = _lastPrefix;

                for (int i = 0; i < _temp.Count; i++) {
                    string _text = _temp[i].text;

                    int _index = _text.LastIndexOf("<");
                    string _prefix = (_index == -1) ? string.Empty : _text.Substring(0, _index);

                    if ((_lastPrefix == _prefix) && (_prefix != _lastSeparatorPrefix) && _text.EndsWith(">")) {
                        _temp.Insert(i, new GUIContent($"{_prefix}/", "Separator"));
                        _lastSeparatorPrefix = _prefix;
                    }

                    _lastPrefix = _prefix;
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
