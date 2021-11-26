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
    /// Special drawer for fields with the attribute <see cref="ColorPaletteAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ColorPaletteAttribute))]
	public class ColorPalettePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Incompatible property management.
            if (_property.propertyType != SerializedPropertyType.Color)
            {
                _height = 0f;
                return;
            }

            // Property behaviour and position calculs.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            Color _color = _property.hasMultipleDifferentValues
                         ? Color.white
                         : _property.colorValue;

            _position.y -= EditorGUIUtility.standardVerticalSpacing;
            _position.height = _height
                             = EnhancedEditorGUI.GetColorPaletteExtraHeight(_id, _position, _label, _color) - EditorGUIUtility.standardVerticalSpacing;

            // Color palette field.
            using (var _scope = new EditorGUI.PropertyScope(_position, GUIContent.none, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                _position.y += EditorGUIUtility.standardVerticalSpacing;
                _color = EnhancedEditorGUI.DoColorPaletteField(_id, _position, _label, _color);
                
                // Save new value.
                if (_changeCheck.changed)
                {
                    _property.colorValue = _color;
                }
            }
        }
        #endregion
    }
}
