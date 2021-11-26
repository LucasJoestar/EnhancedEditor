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
    /// Special drawer for fields with the attribute <see cref="PrecisionSliderAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(PrecisionSliderAttribute))]
	public class PrecisionSliderPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            PrecisionSliderAttribute _attribute = Attribute as PrecisionSliderAttribute;
            EnhancedEditorGUI.PrecisionSliderField(_position, _property, _label, _attribute.MinValue, _attribute.MaxValue, _attribute.Precision, out _height);

            _height += _position.height;
            return true;
        }
        #endregion
    }
}
