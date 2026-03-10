// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="DelayedAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(DelayedAttribute))]
    public sealed class DelayedPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {

            switch (_property.propertyType) {

                case SerializedPropertyType.Integer:
                    EditorGUI.DelayedIntField(_position, _property, _label);
                    break;

                case SerializedPropertyType.Float:
                    EditorGUI.DelayedFloatField(_position, _property, _label);
                    break;

                case SerializedPropertyType.String:
                    EditorGUI.DelayedTextField(_position, _property, _label);
                    break;
                
                default:
                    this.LogErrorMessage($"Attribute of type {typeof(DelayedAttribute).Name} cannot be used with field of type {_property.propertyType}");
                    return base.OnGUI(_position, _property, _label, out _height);
            }

            _height = _position.height;
            return true;
        }
        #endregion
    }
}
