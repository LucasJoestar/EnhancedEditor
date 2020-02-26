using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarPropertyDrawer : PropertyDrawer
    {
        #region Methods
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
            ProgressBarAttribute _attribute = (ProgressBarAttribute)attribute;

            // Set label if needed
            if (!string.IsNullOrEmpty(_attribute.Label)) _label.text = _attribute.Label;

            // Draw editable or readonly progress bar
            if (_attribute.IsEditable)
            {
                if (string.IsNullOrEmpty(_attribute.MaxValueVariableName)) EditorGUIEnhanced.EditableProgressBar(_position, _label, _property, _attribute.MaxValue, _attribute.Color.GetColor());
                else EditorGUIEnhanced.EditableProgressBar(_position, _label, _property, _attribute.MaxValueVariableName, _attribute.Color.GetColor());
            }
            else
            {
                if (string.IsNullOrEmpty(_attribute.MaxValueVariableName)) EditorGUIEnhanced.ProgressBar(_position, _label, _property, _attribute.MaxValue, _attribute.Color.GetColor());
                else EditorGUIEnhanced.ProgressBar(_position, _label, _property, _attribute.MaxValueVariableName, _attribute.Color.GetColor());
            }
        }
        #endregion
    }
}
