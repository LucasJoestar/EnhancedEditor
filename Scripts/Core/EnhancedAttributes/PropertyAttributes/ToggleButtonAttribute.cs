// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor {
    /// <summary>
    /// Draws a <see cref="bool"/> as a toggle button.
    /// </summary>
    public class ToggleButtonAttribute : EnhancedPropertyAttribute {
        #region Global Members
        public const float DefaultWidth     = 35f;
        public const float DefaultHeight    = 20f;

        /// <summary>
        /// Name of the icon to load and display on the button.
        /// </summary>
        public readonly string IconName = string.Empty;

        /// <summary>
        /// Width of this button (in pixels).
        /// </summary>
        public readonly float Width = DefaultWidth;

        /// <summary>
        /// Height of this button (in pixels).
        /// </summary>
        public readonly float Height = DefaultHeight;

        // -----------------------

        /// <param name="_iconName"><inheritdoc cref="IconName" path="/summary"/></param>
        /// <param name="_width"><inheritdoc cref="Width" path="/summary"/></param>
        /// <param name="_height"><inheritdoc cref="Height" path="/summary"/></param>
        public ToggleButtonAttribute(string _iconName, float _width = DefaultWidth, float _height = DefaultHeight) {
            IconName = _iconName;
            Width = _width;
            Height = _height;
        }
        #endregion
    }
}
