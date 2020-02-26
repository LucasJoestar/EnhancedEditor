using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         ***************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetHeight()
        {
            return ((HorizontalLineAttribute)attribute).Height + EditorGUIUtility.singleLineHeight;
        }

        // Make your own GUI for the decorator
        public override void OnGUI(Rect _position)
        {
            HorizontalLineAttribute _attribute = (HorizontalLineAttribute)attribute;

            // Set position at the middle of the area and line height
            _position.y += (EditorGUIUtility.singleLineHeight / 2f) - 1;
            _position.height = _attribute.Height;

            // Draw line at rect
            EditorGUIEnhanced.HorizontalLine(_position, _attribute.Width, _attribute.Color.GetColor());
        }
        #endregion
    }
}
