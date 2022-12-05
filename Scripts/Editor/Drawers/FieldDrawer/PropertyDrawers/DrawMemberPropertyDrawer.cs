// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="DrawMemberAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(DrawMemberAttribute))]
    public class DrawMemberPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            DrawMemberAttribute _attribute = Attribute as DrawMemberAttribute;

            // Try to simply call the member (for a parameterless method), then to assign the member value to the property.
            if (_attribute.Mode.IsActive() && !_attribute.DrawMember.Call(_property.serializedObject)) {
                MemberValue<object> _propertyMember = _property.name;

                if (!_attribute.DrawMember.GetValue(_property, out object _value) || !_propertyMember.SetValue(_property, _value)) {

                    // Debug message.
                    Object _target = _property.serializedObject.targetObject;
                    _target.LogWarning($"Could not call the draw member \"{_attribute.DrawMember.Name}\" in the script \"{_target.GetType()}\"");
                }
            }

            _height = 0f;
            return false;
        }
        #endregion
    }
}
