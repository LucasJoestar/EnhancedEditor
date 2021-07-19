// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a horizontal line above this field in the inspector.
    /// </summary>
    public class HorizontalLineAttribute : EnhancedDecoratorAttribute
    {
        #region Global Members
        public const float DefaultWidth = 1f;
        public const float DefaultHeight = 2f;
        public const SuperColor DefaultColor = SuperColor.Grey;

        public readonly float Width = DefaultWidth;
        public readonly float Height = DefaultHeight;
        public readonly Color Color = default;

        // -----------------------

        /// <inheritdoc cref="HorizontalLineAttribute(float, float, SuperColor)"/>
        public HorizontalLineAttribute(float _height = DefaultHeight, SuperColor _color = DefaultColor)
        {
            Height = Mathf.Max(1f, _height);
            Color = _color.Get();
        }

        /// <inheritdoc cref="HorizontalLineAttribute"/>
        /// <param name="_height">Line height (in pixels).</param>
        /// <param name="_width">Line width, in percent (from 0 to 1).</param>
        /// <param name="_color">Line color.</param>
        public HorizontalLineAttribute(float _height, float _width, SuperColor _color = DefaultColor) : this(_height, _color)
        {
            Width = Mathf.Clamp(_width, 0f, 1f);
        }
        #endregion
    }
}
