// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="AnimationCurve"/> field drawer with extra configurable options.
    /// </summary>
	public class EnhancedCurveAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly Color Color = default;
        public readonly Rect Rect = new Rect(0f, 0f, 0f, 0f);

        // -----------------------

        /// <inheritdoc cref="EnhancedCurveAttribute(SuperColor, float, float, float, float)"/>
        /// <param name="_color">Curve color.</param>
        public EnhancedCurveAttribute(SuperColor _color)
        {
            Color = _color.Get();
        }

        /// <inheritdoc cref="EnhancedCurveAttribute"/>
        /// <param name="_color">Curve color.</param>
        /// <param name="_minX">Minimum X-axis oriented value.</param>
        /// <param name="_minY">Minimum Y-axis oriented value.</param>
        /// <param name="_maxX">Maximum X-axis oriented value.</param>
        /// <param name="_maxY">Maximum Y-axis oriented value.</param>
        public EnhancedCurveAttribute(SuperColor _color, float _minX, float _minY, float _maxX, float _maxY) : this(_color)
        {
            Rect = new Rect(_minX, _minY, _maxX - _minX, _maxY - _minY);
        }
        #endregion
    }
}
