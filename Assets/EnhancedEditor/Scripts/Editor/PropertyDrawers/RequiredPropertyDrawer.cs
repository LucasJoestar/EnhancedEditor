using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
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
            if (IsPropertyValid(_property)) return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight + 40;
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            Rect _rect = new Rect(_position.x, _position.y, _position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(_rect, _property, _label);
            if (IsPropertyValid(_property)) return;

            {
                float _difference = _rect.height + EditorGUIUtility.standardVerticalSpacing;

                _rect.y += _difference;
                _rect.height = _position.height - _difference;
            }

            _position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.HelpBox(_rect, "Keep in mind to set a reference to this field !", MessageType.Error);
        }
        #endregion
    }
}
