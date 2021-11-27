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
    /// Special drawer for fields with the attribute <see cref="PickerAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(PickerAttribute))]
	public class PickerPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            PickerAttribute _attribute = Attribute as PickerAttribute;
            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.PickerField(_position, _property, _label, _attribute.RequiredTypes);
            return true;
        }

        public override void OnValueChanged()
        {
            // Reset object value if it does not match.
            if ((SerializedProperty.propertyType == SerializedPropertyType.ObjectReference)
             && EnhancedEditorGUI.ResetPickerObjectIfDontMatch(SerializedProperty.objectReferenceValue, (Attribute as PickerAttribute).RequiredTypes))
            {
                SerializedProperty.objectReferenceValue = null;
            }
        }
        #endregion
    }
}
