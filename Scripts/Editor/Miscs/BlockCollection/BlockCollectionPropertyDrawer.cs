// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="BlockCollection{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(BlockCollection<>), true)]
    public sealed class BlockCollectionPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const float LineSpacing         = 5f;
        private const float ContentOffset       = 2f;

        private const float EmptyListHeight     = 50f;
        private const float FoldedListHeight    = 30f;

        private const string HeaderLabelFormat  = "{0} - [{1}]";

        private static readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();
        private static readonly Dictionary<string, List<float>> listElementHeight = new Dictionary<string, List<float>>();
        private static readonly GUIContent labelGUI = new GUIContent();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // List registration.
            string _key = EnhancedEditorUtility.GetSerializedPropertyID(_property);
            _position = EditorGUI.IndentedRect(_position);

            if (!lists.TryGetValue(_key, out ReorderableList _list)) {

                // Get collection property.
                SerializedProperty _array = _property.Copy();
                while (!_array.isArray && _array.Next(true)) { }

                if (_array.isArray) {

                    // Cache the label for the original may be modified.
                    GUIContent _cacheLabel = new GUIContent(_label.text, _label.tooltip);

                    if (_array.isExpanded) {

                        // Functional list.
                        bool _isEditable    = _property.FindPropertyRelative("IsEditable").boolValue;
                        bool _isReorderable = _property.FindPropertyRelative("IsReorderable").boolValue;
                        bool _isReadonly    = _property.FindPropertyRelative("IsReadonly").boolValue;

                        _list = new ReorderableList(_array.serializedObject, _array, _isReorderable, true, _isEditable, _isEditable) {
                            drawHeaderCallback = DrawHeader,

                            // By default, the list does not draw property children, so force it.
                            elementHeightCallback = (int _index) => {

                                SerializedProperty _elementProperty = _array.GetArrayElementAtIndex(_index);

                                string _propertyKey = EnhancedEditorUtility.GetSerializedPropertyID(_array);
                                float _height;

                                // Get cached height or default.
                                if (listElementHeight.TryGetValue(_propertyKey, out List<float> _values) && (_values.Count > _index)) {
                                    _height = _values[_index];
                                } else {
                                    _height = EnhancedEditorGUI.GetEnhancedPropertyHeight(_elementProperty, _label, true) + ContentOffset;
                                }

                                if (_index != (_array.arraySize - 1)) {
                                    _height += LineSpacing * 2f;
                                }

                                return _height;
                            },

                            drawElementCallback = (Rect _position, int _index, bool _isActive, bool _isFocused) => {
                                _position.y += ContentOffset;

                                using (var _scope = new EditorGUI.DisabledGroupScope(_isReadonly)) {

                                    SerializedProperty _elementProperty = _array.GetArrayElementAtIndex(_index);
                                    var _infos = EnhancedEditorGUI.GetPropertyEditor(_elementProperty);

                                    // If there is a custom drawer assigned, use it. Otherwise, display as a Block field and cache height.
                                    if (_infos.State != 0) {

                                        EnhancedEditorGUI.EnhancedPropertyField(_position, _elementProperty, labelGUI, true);

                                    } else {

                                        EnhancedEditorGUI.BlockField(_position, _elementProperty, GUIContent.none, out float _totalHeight, false);

                                        string _propertyKey = EnhancedEditorUtility.GetSerializedPropertyID(_array);
                                        if (!listElementHeight.TryGetValue(_propertyKey, out List<float> _values)) {

                                            _values = new List<float>();
                                            listElementHeight.Add(_propertyKey, _values);
                                        }

                                        while (_values.Count <= _index) {
                                            _values.Add(0f);
                                        }

                                        _values[_index] = _totalHeight;
                                    }
                                }

                                if (_index != (_array.arraySize - 1)) {
                                    _position.yMin = _position.yMax - (LineSpacing + 2f);
                                    _position.height = 1f;

                                    EnhancedEditorGUI.HorizontalLine(_position, SuperColor.Grey.Get(), 20f);
                                }
                            },
                        };
                    } else {
                        // Unusable list.
                        _list = new ReorderableList(_array.serializedObject, _array, false, true, false, false) {
                            drawHeaderCallback = DrawHeader,
                            elementHeightCallback = (int _index) => 0f,
                            drawElementCallback = (Rect _position, int _index, bool _isActive, bool _isFocused) => { },
                        };
                    }

                    // ----- Local Method ----- \\

                    void DrawHeader(Rect _position) {
                        GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(string.Format(HeaderLabelFormat, _cacheLabel.text, _array.arraySize), _cacheLabel.tooltip);

                        // Temporarily disable hierarchy to avoid foldout padding.
                        using (var _scope = EnhancedEditorGUI.HierarchyMode.Scope(false))
                        using (var _indentScope = EnhancedEditorGUI.ZeroIndentScope()) {

                            bool _wasExpanded = _list.serializedProperty.isExpanded;
                            bool _isExpanded = EditorGUI.Foldout(_position, _wasExpanded, _label);

                            // Recreate the list every time is foldout is changed.
                            if (_isExpanded != _wasExpanded) {
                                _list.serializedProperty.isExpanded = _isExpanded;

                                GUI.changed = true;
                                lists.Remove(_key);
                            }
                        }
                    }
                } else {
                    _list = null;
                }

                lists.Add(_key, _list);
            }

            float _height;

            try {
                _height = _position.height
                        = _list.serializedProperty.isExpanded ? _list.GetHeight() : ((_list.displayRemove || (_list.count == 0)) ? EmptyListHeight : FoldedListHeight);

                _list.DoList(_position);

            } catch (Exception) { // This can happen when the SerializedProperty target object is missing.
                // Default property drawer.
                lists.Remove(_key);

                EditorGUI.PropertyField(_position, _property, _label);
                _height = _position.height;
            }

            return _height;
        }
        #endregion
    }
}
