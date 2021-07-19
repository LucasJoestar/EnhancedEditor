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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="EnhancedCurveAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(EnhancedCurveAttribute))]
	public class EnhancedCurve : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedCurveAttribute _attribute = (EnhancedCurveAttribute)Attribute;

            _height = EditorGUIUtility.singleLineHeight;
            _position.height = _height;

            EditorGUI.CurveField(_position, _property, _attribute.Color, _attribute.Rect, _label);
            return true;
        }

        public override void OnContextMenu(GenericMenu _menu, SerializedProperty _property)
        {
            AnimationCurvePropertyDrawer.OnCurveBufferContextMenu(_menu, _property);
        }
        #endregion
    }
}
