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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="ProgressBarAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            ProgressBarAttribute _attribute = (ProgressBarAttribute)Attribute;
            _height = _attribute.Height;
            _position.height = _height;

            if (!string.IsNullOrEmpty(_attribute.Label.text))
                _label.text = _attribute.Label.text;

            // Draw progress bar.
            if (_attribute.IsEditable)
            {
                if (string.IsNullOrEmpty(_attribute.MaxValueVariableName))
                {
                    EnhancedEditorGUI.EditableProgressBar(_position, _property, _label, _attribute.MaxValue, _attribute.Color);
                }
                else
                {
                    EnhancedEditorGUI.EditableProgressBar(_position, _property, _label, _attribute.MaxValueVariableName, _attribute.Color);
                }
            }
            else if (string.IsNullOrEmpty(_attribute.MaxValueVariableName))
            {
                EnhancedEditorGUI.ProgressBar(_position, _property, _label, _attribute.MaxValue, _attribute.Color);
            }
            else
            {
                EnhancedEditorGUI.ProgressBar(_position, _property, _label, _attribute.MaxValueVariableName, _attribute.Color);
            }

            return true;
        }
        #endregion
    }
}
