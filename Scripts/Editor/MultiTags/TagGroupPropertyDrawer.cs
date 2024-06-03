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
    /// Custom <see cref="TagGroup"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagGroup), true)]
    public sealed class TagGroupPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        internal protected override float GetDefaultHeight(SerializedProperty _property, GUIContent _label) {
            Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
            float _height = _position.height;
            try {
                _height += EnhancedEditorGUI.GetTagGroupExtraHeight(_position, _property, _label);
            } catch (NullReferenceException) { }

            return _height;
        }

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            // As the full height is given on position, set it for one line only.
            _position.height = EditorGUIUtility.singleLineHeight;
            float _extraHeight = 0f;

            try {
                EnhancedEditorGUI.TagGroupField(_position, _property, _label, out _extraHeight);
            } catch (NullReferenceException) { }

            float _height = _position.height + _extraHeight;
            return _height;
        }
        #endregion
    }
}
