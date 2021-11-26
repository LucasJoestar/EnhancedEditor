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
    /// Special drawer for fields with the attribute <see cref="EnhancedTextAreaAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(EnhancedTextAreaAttribute))]
	public class EnhancedTextAreaPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedTextAreaAttribute _attribute = Attribute as EnhancedTextAreaAttribute;
            EnhancedEditorGUI.TextArea(_position, _property, _label, _attribute.IsWide, out _height);

            return true;
        }
        #endregion
    }
}
