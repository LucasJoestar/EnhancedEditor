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
    /// Special drawer for fields with the attribute <see cref="ProgressBarAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            ProgressBarAttribute _attribute = (ProgressBarAttribute)Attribute;
            _position.height = _height
                             = _attribute.Height;

            if (_attribute.MaxMember == null)
            {
                EnhancedEditorGUI.ProgressBar(_position, _property, _attribute.MaxValue, _attribute.Color, _attribute.IsEditable);
            }
            else
            {
                EnhancedEditorGUI.ProgressBar(_position, _property, _attribute.MaxMember.Value, _attribute.Color, _attribute.IsEditable);
            }

            return true;
        }
        #endregion
    }
}
