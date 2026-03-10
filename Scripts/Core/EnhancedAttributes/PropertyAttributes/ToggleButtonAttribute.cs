// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

namespace EnhancedEditor {
    /// <summary>
    /// Draws a <see cref="bool"/> as a toggle button.
    /// </summary>
    public sealed class ToggleButtonAttribute : EnhancedPropertyAttribute {
        #region Global Members
        public const float DefaultHeight = 20f;
        public const float DefaultWidth  = 35f;

        /// <summary>
        /// Name of the icon to load and display on the button.
        /// </summary>
        public readonly string IconName = string.Empty;

        /// <summary>
        /// Height of this button (in pixels).
        /// </summary>
        public readonly float Height = DefaultHeight;

        /// <summary>
        /// Width of this button (in pixels).
        /// </summary>
        public readonly float Width = DefaultWidth;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_iconName"><inheritdoc cref="IconName" path="/summary"/></param>
        /// <param name="_width"><inheritdoc cref="Width" path="/summary"/></param>
        /// <param name="_height"><inheritdoc cref="Height" path="/summary"/></param>
        public ToggleButtonAttribute(string _iconName, float _width = DefaultWidth, float _height = DefaultHeight) {
            IconName = _iconName;
            Height   = _height;
            Width    = _width;
        }
        #endregion
    }
}
