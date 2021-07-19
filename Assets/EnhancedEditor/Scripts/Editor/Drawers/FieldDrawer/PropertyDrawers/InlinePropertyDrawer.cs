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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="InlineAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(InlineAttribute))]
	public class InlinePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // We only want to draw the first child field of this property,
            // so store next object property to break chain when getting to it.
            SerializedProperty _next = _property.Copy();
            _next.Next(false);

            if (!_property.NextVisible(true))
            {
                _height = 0f;
                return false;
            }

            // Property label header.
            _position.height = _height
                            = EditorGUI.GetPropertyHeight(_property);

            EditorGUI.PropertyField(_position, _property, _label);

            return true;
        }
        #endregion
    }
}
