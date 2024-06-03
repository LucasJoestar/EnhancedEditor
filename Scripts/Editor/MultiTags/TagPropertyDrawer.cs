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
    /// Custom <see cref="Tag"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Tag), true)]
    public sealed class TagPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            try {
                EnhancedEditorGUI.TagField(_position, _property, _label);
            } catch (NullReferenceException) { }

            return _height;
        }
        #endregion
    }
}
