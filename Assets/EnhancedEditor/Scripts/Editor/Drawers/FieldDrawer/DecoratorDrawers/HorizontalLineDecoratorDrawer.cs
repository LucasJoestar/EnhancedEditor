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
    /// Special <see cref="DecoratorDrawer"/> for fields with the attribute <see cref="HorizontalLineAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute), true)]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {
        #region Decorator Content
        public override float GetHeight()
        {
            HorizontalLineAttribute _attribute = attribute as HorizontalLineAttribute;
            return _attribute.Height + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect _position)
        {
            HorizontalLineAttribute _attribute = attribute as HorizontalLineAttribute;
            _position.height -= EditorGUIUtility.standardVerticalSpacing;

            EnhancedEditorGUI.HorizontalLine(_position, _attribute.Color);
        }
        #endregion
    }
}
