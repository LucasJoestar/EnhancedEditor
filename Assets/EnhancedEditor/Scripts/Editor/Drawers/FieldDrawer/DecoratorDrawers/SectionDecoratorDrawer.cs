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
    /// Special <see cref="DecoratorDrawer"/> for fields with the attribute <see cref="SectionAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SectionAttribute), true)]
    public class SectionDecoratorDrawer : DecoratorDrawer
    {
        #region Decorator Content
        public override float GetHeight()
        {
            SectionAttribute _attribute = attribute as SectionAttribute;
            return EditorGUIUtility.singleLineHeight + (_attribute.Margins * 2f) + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect _position)
        {
            SectionAttribute _attribute = attribute as SectionAttribute;
            _position.height -= EditorGUIUtility.standardVerticalSpacing;

            EnhancedEditorGUI.Section(_position, _attribute.Label, _attribute.LineWidth);
        }
        #endregion
    }
}
