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
        private const float LineSpacing = 5f;
        private const float ContentOffset = 2f;

        private const string LabelFormat = "{0} - [{1}]";
        private static readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // List registration.
            string _key = _property.propertyPath + _property.serializedObject.targetObject.GetInstanceID().ToString();

            if (!lists.TryGetValue(_key, out ReorderableList _list)) {
                // Cache the label for the original may be modified.
                SerializedProperty _array = _property.Copy();

                // Get collection property.
                while (!_array.isArray && _array.Next(true)) { }

                if (_array.isArray) {
                    GUIContent _cacheLabel = new GUIContent(_label.text, _label.tooltip);

                    bool _isEditable = _property.FindPropertyRelative("IsEditable").boolValue;
                    bool _isReorderable = _property.FindPropertyRelative("IsReorderable").boolValue;
                    bool _isReadonly = _property.FindPropertyRelative("IsReadonly").boolValue;

                    _list = new ReorderableList(_array.serializedObject, _array, _isReorderable, true, _isEditable, _isEditable) {
                        drawHeaderCallback = (Rect _position) => {
                            EditorGUI.LabelField(_position, EnhancedEditorGUIUtility.GetLabelGUI(string.Format(LabelFormat, _cacheLabel.text, _array.arraySize), _cacheLabel.tooltip));
                        },

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
                    _list = null;
                }

                lists.Add(_key, _list);
            }

            float _height;

            try {
                _height = _position.height
                          = _list.GetHeight();

                _list.DoList(_position);
            } catch (ArgumentException) { // This can happen when the SerializedProperty target object is missing.
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
