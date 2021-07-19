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
    /// Special <see cref="DecoratorDrawer"/> for fields with attribute <see cref="HorizontalLineAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {
        #region Decorator Content
        public override float GetHeight()
        {
            HorizontalLineAttribute _attribute = (HorizontalLineAttribute)attribute;
            return EditorGUIUtility.singleLineHeight + _attribute.Height;
        }

        public override void OnGUI(Rect _position)
        {
            HorizontalLineAttribute _attribute = (HorizontalLineAttribute)attribute;

            // Set position at the middle of the area.
            _position.y += (EditorGUIUtility.singleLineHeight / 2f) - 1f;
            _position.height = _attribute.Height;

            EnhancedEditorGUI.HorizontalLine(_position, _attribute.Width, _attribute.Color);
        }
        #endregion
    }
}
