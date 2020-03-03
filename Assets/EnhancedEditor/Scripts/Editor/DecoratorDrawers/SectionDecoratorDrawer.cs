using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(SectionAttribute))]
    public class SectionDecoratorDrawer : DecoratorDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         ***************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetHeight()
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;
            return (EditorGUIUtility.singleLineHeight + _attribute.HeightSpace) * 2;
        }

        // Make your own GUI for the decorator
        public override void OnGUI(Rect _position)
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;
            EditorGUIEnhanced.Section(_position, _attribute.Label, _attribute.LineWidth);
        }
        #endregion
    }
}
