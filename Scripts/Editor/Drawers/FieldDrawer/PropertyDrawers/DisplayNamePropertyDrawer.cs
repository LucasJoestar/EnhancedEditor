// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="DisplayNameAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(DisplayNameAttribute))]
    public class DisplayNamePropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            DisplayNameAttribute _attribute = Attribute as DisplayNameAttribute;
            if ((_attribute.NameMember != null) && _attribute.NameMember.Value.GetValue(_property, out string _name)) {
                _attribute.SetName(_name);
            }

            // Get label.
            GUIContent _displayLabel =_attribute.Label;

            if (!string.IsNullOrEmpty(_label.text) && !string.IsNullOrEmpty(_displayLabel.text)) {
                _label.text = _displayLabel.text;

                if (!string.IsNullOrEmpty(_label.tooltip) && !string.IsNullOrEmpty(_displayLabel.tooltip)) {
                    _label.tooltip = _displayLabel.tooltip;
                }
            }

            _height = 0f;
            return false;
        }
        #endregion
    }
}
