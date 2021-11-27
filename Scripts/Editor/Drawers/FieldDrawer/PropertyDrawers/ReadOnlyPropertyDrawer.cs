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
    /// Special drawer for fields with the attribute <see cref="ReadOnlyAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedGUI.GUIEnabled.Push(false);
            _height = 0f;

            return false;
        }

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            ReadOnlyAttribute _attribute = Attribute as ReadOnlyAttribute;
            if (_attribute.UseRadioToggle && (_property.propertyType == SerializedPropertyType.Boolean))
            {
                EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
                _height = _position.height;

                return true;
            }

            _height = 0f;
            return false;
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedGUI.GUIEnabled.Pop();
            _height = 0f;
        }
        #endregion
    }
}
