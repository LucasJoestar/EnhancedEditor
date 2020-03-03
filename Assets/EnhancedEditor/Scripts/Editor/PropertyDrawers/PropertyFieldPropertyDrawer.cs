using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(PropertyFieldAttribute))]
    public class PropertyFieldPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /*****************************
         *****   UNITY METHODS   *****
         ****************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Get linked property name
            PropertyFieldAttribute _attribute = (PropertyFieldAttribute)attribute;
            string _propertyName = string.IsNullOrEmpty(_attribute.PropertyName) ?
                                    (char.ToUpper(_property.name[0]) + _property.name.Substring(1)) :
                                    _attribute.PropertyName;

            // Draw field
            EditorGUIEnhanced.PropertyField(_position, _property, _label, _propertyName);
        }
        #endregion
    }
}
