// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a horizontal line above this field.
    /// </summary>
    public class HorizontalLineAttribute : EnhancedDecoratorAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.Grey;
        public const float DefaultHeight = 2f;
        public const float DefaultMargins = 1f;

        /// <summary>
        /// Line color.
        /// </summary>
        public readonly Color Color = default;

        /// <summary>
        /// Line height (in pixels).
        /// </summary>
        public readonly float Height = DefaultHeight;

        /// <summary>
        /// Margins on both sides of the line (in pixels)
        /// </summary>
        public readonly float Margins = DefaultMargins;

        // -----------------------

        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <param name="_height"><inheritdoc cref="Height" path="/summary"/></param>
        /// <param name="_margins"><inheritdoc cref="Margins" path="/summary"/></param>
        /// <inheritdoc cref="HorizontalLineAttribute"/>
        public HorizontalLineAttribute(SuperColor _color = DefaultColor, float _height = DefaultHeight, float _margins = DefaultMargins)
        {
            Color = _color.Get();
            Height = Mathf.Max(1f, _height);
            Margins = Mathf.Max(0f, _margins);
        }
        #endregion
    }
}
