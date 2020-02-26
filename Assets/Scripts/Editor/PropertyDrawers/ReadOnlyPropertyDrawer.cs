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
            EditorGUIEnhanced.ReadOnlyProperty(_position, _property, _label, _attribute.UseRadioToggle);
        }
        #endregion
    }
}
