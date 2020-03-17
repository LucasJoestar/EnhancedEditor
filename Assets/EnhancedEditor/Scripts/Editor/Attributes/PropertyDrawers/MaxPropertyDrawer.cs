using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(MaxAttribute))]
    public class MaxPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            MaxAttribute _attribute = (MaxAttribute)attribute;
            EditorGUIEnhanced.MaxField(_position, _property, _label, _attribute.MaxValue);
        }
        #endregion
    }
}
