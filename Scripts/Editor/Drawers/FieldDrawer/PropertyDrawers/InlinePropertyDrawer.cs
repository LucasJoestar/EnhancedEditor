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
    /// Special drawer for fields with the attribute <see cref="InlineAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(InlineAttribute))]
	public class InlinePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.InlineField(_position, _property, _label, out float _extraHeight);
            _height += _extraHeight;

            return true;
        }
        #endregion
    }
}
