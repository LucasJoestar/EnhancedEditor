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

            // Specify line width if needed
            if ((_attribute.Width > 0) && (_attribute.Width < _position.width))
            {
                _position.x += (_position.width - _attribute.Width) / 2f;
                _position.width = _attribute.Width;
            }

            // Draw rect at position of specified color
            EditorGUI.DrawRect(_position, _attribute.Color.GetColor());
        }
        #endregion
    }
}
