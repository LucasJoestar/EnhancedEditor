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
    /// Special drawer for fields with the attribute <see cref="MinMaxAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(MinMaxAttribute))]
    public class MinMaxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            MinMaxAttribute _attribute = Attribute as MinMaxAttribute;
            _height = _position.height;

            if (_attribute.MinMaxMember == null)
            {
                EnhancedEditorGUI.MinMaxField(_position, _property, _label, _attribute.MinValue, _attribute.MaxValue);
            }
            else
            {
                EnhancedEditorGUI.MinMaxField(_position, _property, _label, _attribute.MinMaxMember.Value);
            }

            return true;
        }
        #endregion
    }
}
