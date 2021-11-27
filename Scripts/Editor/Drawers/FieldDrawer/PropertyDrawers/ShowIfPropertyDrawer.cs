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
    /// Special drawer for fields with the attribute <see cref="ShowIfAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ShowIfAttribute))]
	public class ShowIfPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            ShowIfAttribute _attribute = Attribute as ShowIfAttribute;
            bool _hide = _attribute.ConditionMember.GetValue(_property.serializedObject, out bool _value) && (_value != _attribute.ConditionType.Get());

            _height = 0f;
            return _hide;
        }
        #endregion
    }
}
