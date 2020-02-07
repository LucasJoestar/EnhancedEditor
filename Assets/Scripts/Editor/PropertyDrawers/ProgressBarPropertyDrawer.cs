using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /********************************
         *****   ORIGINAL METHODS   *****
         *******************************/

        /// <summary>
        /// Get if an informative help box should be drawn or not.
        /// </summary>
        /// <param name="_property">Property to check object reference validity.</param>
        /// <returns>Returns false if property type is object reference and its value is not set, true otherwise.</returns>
        private bool IsPropertyValid(SerializedProperty _property)
        {
            return (_property.propertyType != SerializedPropertyType.ObjectReference) || _property.objectReferenceValue;
        }


        /*****************************
         *****   UNITY METHODS   *****
         ****************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            ProgressBarAttribute _attribute = (ProgressBarAttribute)attribute;
            return _attribute.Height;
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // If property type not compatible with progress bar, draw standard editor
            float _value = 0;
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    _value = _property.intValue;
                    break;

                case SerializedPropertyType.Float:
                    _value = _property.floatValue;
                    break;

                default:
                    // Wrong !
                    return;
            }

            ProgressBarAttribute _attribute = (ProgressBarAttribute)attribute;

            // Get bar maximum value
            float _maxValue = 0;
            if (!string.IsNullOrEmpty(_attribute.MaxValueVariableName))
            {
                //Convert.ToSingle()
            }
            else _maxValue = _attribute.MaxValue;

            // Draw bar
            Rect _rect = new Rect(_position.x, _position.y, _position.width, _position.y);

            // Draw label
            string _barLabel = $"{_label.text} [{_value} / {_maxValue}]";
            float _x = _position.x + (_position.width / 2f) - (EditorStyles.label.CalcSize(new GUIContent(_barLabel)).x / 2f);
            _position.x = _x;

            EditorGUI.LabelField(_position, _label);
        }
        #endregion
    }
}
