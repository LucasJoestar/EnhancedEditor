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
    /// Special <see cref="DecoratorDrawer"/> for fields with attribute <see cref="SectionAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SectionAttribute))]
    public class SectionDecoratorDrawer : DecoratorDrawer
    {
        #region Decorator Content
        public override float GetHeight()
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;
            return (EditorGUIUtility.singleLineHeight + _attribute.Margins) * 2f;
        }

        public override void OnGUI(Rect _position)
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;
            EnhancedEditorGUI.Section(_position, _attribute.Label, _attribute.LineWidth);
        }
        #endregion
    }
}
