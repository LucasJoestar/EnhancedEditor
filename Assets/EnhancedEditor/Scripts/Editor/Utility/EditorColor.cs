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
    /// Editor struct used to store two colors at once, one for the editor light theme and the other one for the dark theme.
    /// </summary>
	public struct EditorColor
    {
        #region Global Members
        /// <summary>
        /// Color used in light theme.
        /// </summary>
        public Color LightColor;

        /// <summary>
        /// Color used in dark theme.
        /// </summary>
        public Color DarkColor;

        // -----------------------

        /// <param name="_lightColor"><inheritdoc cref="LightColor" path="/summary"/></param>
        /// <param name="_darkColor"><inheritdoc cref="DarkColor" path="/summary"/></param>
        /// <inheritdoc cref="EditorColor"/>
        public EditorColor(Color _lightColor, Color _darkColor)
        {
            LightColor = _lightColor;
            DarkColor = _darkColor;
        }
        #endregion

        #region Operators
        public static implicit operator Color(EditorColor _color)
        {
            return _color.Get();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the actual color to use, depending on whether currently using the light theme or the dark theme in the editor.
        /// </summary>
        /// <returns>Editor color to use.</returns>
        public Color Get()
        {
            Color _color = EditorGUIUtility.isProSkin
                            ? DarkColor
                            : LightColor;

            return _color;
        }
        #endregion
    }
}
