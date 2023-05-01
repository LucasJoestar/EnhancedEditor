// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special <see cref="EnhancedPropertyDrawer"/> for fields with the attribute <see cref="ToggleButtonAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(ToggleButtonAttribute))]
    public class ToggleButtonPropertyDrawer : EnhancedPropertyDrawer {
        #region Decorator Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {

            // Uncompatible property management.
            if (_property.propertyType != SerializedPropertyType.Boolean) {
                _height = 0f;
                return false;
            }

            ToggleButtonAttribute _attribute = Attribute as ToggleButtonAttribute;

            _position = EditorGUI.PrefixLabel(_position, _label);
            _position.size = new Vector2(_attribute.Width, _attribute.Height);

            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property)) {

                bool _value = _property.boolValue;
                bool _newValue = GUI.Toggle(_position, _value, EditorGUIUtility.IconContent(_attribute.IconName), EnhancedEditorStyles.ToolbarControl);

                if (_newValue != _value) {
                    _property.boolValue = _newValue;
                }
            }

            _height = _position.height;
            return true;
        }
        #endregion
    }
}
