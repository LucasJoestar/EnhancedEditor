using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            return EditorGUIUtilityEnhanced.GetRequiredPropertyHeight(_property);
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUIEnhanced.RequiredProperty(_position, _property, _label);
        }
        #endregion
    }
}
