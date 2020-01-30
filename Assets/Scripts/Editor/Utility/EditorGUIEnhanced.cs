using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contain a bunch of useful methods for GUI related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EditorGUIEnhanced
    {
        #region Fields / Properties

        #endregion

        #region Methods

        #region Decorator Drawers
        /*********************************
         *****   DECORATOR DRAWERS   *****
         ********************************/

        #endregion

        #region Decorator Drawers
        /********************************
         *****   PROPERTY DRAWERS   *****
         *******************************/

        public static void HorizontalLine(Rect _position, Color _color)
        {
            EditorGUI.DrawRect(_position, _color);
        }
        #endregion

        #endregion
    }
}
