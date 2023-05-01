// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

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
    public class BlockCollectionPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const float LineSpacing         = 5f;
        private const float ContentOffset       = 2f;

        private const float EmptyListHeight     = 50f;
        private const float FoldedListHeight    = 30f;

        private const string LabelFormat = "{0} - [{1}]";
        private static readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

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
                        bool _isEditable = _property.FindPropertyRelative("IsEditable").boolValue;
                        bool _isReorderable = _property.FindPropertyRelative("IsReorderable").boolValue;
                        bool _isReadonly = _property.FindPropertyRelative("IsReadonly").boolValue;

                        _list = new ReorderableList(_array.serializedObject, _array, _isReorderable, true, _isEditable, _isEditable) {
                            drawHeaderCallback = DrawHeader,

                            // By default, the list does not draw property children, so force it.
                            elementHeightCallback = (int _index) => {
                                float _height = EnhancedEditorGUI.GetEnhancedPropertyHeight(_array.GetArrayElementAtIndex(_index), _label, true) + ContentOffset;
                                if (_index != (_array.arraySize - 1)) {
                                    _height += LineSpacing * 2f;
                                }

                                return _height;
                            },

                            drawElementCallback = (Rect _position, int _index, bool _isActive, bool _isFocused) => {
                                _position.y += ContentOffset;
                                using (var _scope = new EditorGUI.DisabledGroupScope(_isReadonly)) {
                                    EnhancedEditorGUI.EnhancedPropertyField(_position, _array.GetArrayElementAtIndex(_index), GUIContent.none, true);
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
                        GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(string.Format(LabelFormat, _cacheLabel.text, _array.arraySize), _cacheLabel.tooltip);

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
