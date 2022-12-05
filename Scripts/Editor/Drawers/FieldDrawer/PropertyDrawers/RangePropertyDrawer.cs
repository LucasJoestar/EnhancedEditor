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
    public class RangePropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            RangeAttribute _attribute = Attribute as RangeAttribute;
            Vector2 _rangeValue;

            // Get minimum allowed value and floor property value.
            if (_attribute.RangeMember == null) {
                _rangeValue = _attribute.Range;
            } else if (!_attribute.RangeMember.Value.GetValue(_property, out _rangeValue)) {
                _height = 0f;
                return false;
            }

            _height = _position.height
                    = EditorGUIUtility.singleLineHeight;

            switch (_property.propertyType) {
                case SerializedPropertyType.Integer:
                    int _rangeX = (int)_rangeValue.x;
                    int _rangeY = (int)_rangeValue.y;

                    _property.intValue = Mathf.Clamp(_property.intValue, _rangeX, _rangeY);
                    EditorGUI.IntSlider(_position, _property, _rangeX, _rangeY);
                    break;

                case SerializedPropertyType.Float:
                    _property.floatValue = Mathf.Clamp(_property.floatValue, _rangeValue.x, _rangeValue.y);
                    EditorGUI.Slider(_position, _property, _rangeValue.x, _rangeValue.y);
                    break;
                
                default:
                    EditorGUI.PropertyField(_position, _property, _label);
                    break;
            }

            return true;
        }
        #endregion
    }
}
