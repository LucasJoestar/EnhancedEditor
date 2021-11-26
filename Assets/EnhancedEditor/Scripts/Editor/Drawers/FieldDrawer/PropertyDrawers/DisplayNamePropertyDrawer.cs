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
    /// Special drawer for fields with the attribute <see cref="DisplayNameAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(DisplayNameAttribute))]
	public class DisplayNamePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            DisplayNameAttribute _attribute = Attribute as DisplayNameAttribute;
            _label.text = _attribute.Label.text;

            if (!string.IsNullOrEmpty(_attribute.Label.tooltip))
                _label.tooltip = _attribute.Label.tooltip;

            _height = 0f;
            return false;
        }
        #endregion
    }
}
