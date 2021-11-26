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
    /// Special drawer for fields with the attribute <see cref="EnhancedCurveAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(EnhancedCurveAttribute))]
	public class EnhancedCurvePropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedCurveAttribute _attribute = Attribute as EnhancedCurveAttribute;
            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            // On curve context click, do not open the curve editor.
            Rect _temp = EnhancedEditorGUI.InvisiblePrefixLabel(_position, _label);
            if ((_temp.Event(out Event _event) == EventType.MouseDown) && (_event.button == 1))
            {
                _event.Use();
            }

            EditorGUI.CurveField(_position, _property, _attribute.Color, _attribute.Rect, _label);
            return true;
        }

        public override void OnContextMenu(GenericMenu _menu)
        {
            AnimationCurvePropertyDrawer.OnContextMenu(_menu, SerializedProperty);
        }
        #endregion
    }
}
