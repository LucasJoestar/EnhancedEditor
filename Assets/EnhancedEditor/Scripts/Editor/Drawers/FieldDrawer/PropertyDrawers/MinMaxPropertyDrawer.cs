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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="MinMaxAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(MinMaxAttribute))]
    public class MinMaxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            MinMaxAttribute _attribute = (MinMaxAttribute)Attribute;
            EnhancedEditorGUI.MinMaxField(_position, _property, _label, _attribute.MinValue, _attribute.MaxValue);

            _height = _position.height;
            return true;
        }
        #endregion
    }
}
