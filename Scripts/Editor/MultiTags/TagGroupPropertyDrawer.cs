// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="TagGroup"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagGroup), true)]
	public class TagGroupPropertyDrawer : EnhancedPropertyEditor
    {
        #region Drawer Content
        protected override float GetDefaultHeight(SerializedProperty _property, GUIContent _label)
        {
            Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
            float _height = _position.height + EnhancedEditorGUI.GetTagGroupExtraHeight(_position, _property, _label);

            return _height;
        }

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // As the full height is given on position, set it for one line only.
            _position.height = EditorGUIUtility.singleLineHeight;
            EnhancedEditorGUI.TagGroupField(_position, _property, _label, out float _extraHeight);

            float _height = _position.height + _extraHeight;
            return _height;
        }
        #endregion
    }
}
