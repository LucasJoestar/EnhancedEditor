using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            ReadOnlyAttribute _attribute = (ReadOnlyAttribute)attribute;
            GUI.enabled = false;

            // Draw radio field if needed
            if ((_property.propertyType == SerializedPropertyType.Boolean) && _attribute.UseRadioToggle)
            {
                EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
            }
            else EditorGUI.PropertyField(_position, _property, _label);

            GUI.enabled = true;
        }
        #endregion
    }
}
