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
    /// Special drawer for fields with the attribute <see cref="RangeAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(RangeAttribute))]
    public class RangePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            RangeAttribute _attribute = Attribute as RangeAttribute;
            _height = _position.height
                    = EditorGUIUtility.singleLineHeight;

            EditorGUI.Slider(_position, _property, _attribute.Range.x, _attribute.Range.y);
            return true;
        }
        #endregion
    }
}
