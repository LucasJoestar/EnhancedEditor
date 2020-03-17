using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            MinMaxAttribute _attribute = (MinMaxAttribute)attribute;
            EditorGUIEnhanced.MinMaxField(_position, _property, _label, _attribute.Minvalue, _attribute.MaxValue);
        }
        #endregion
    }
}
