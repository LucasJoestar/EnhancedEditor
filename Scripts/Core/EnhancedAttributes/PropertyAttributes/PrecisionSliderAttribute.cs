// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Similar to the <see cref="RangeAttribute"/>, but with an extra secondary slider used to adjust the value more precisely.
    /// </summary>
	public class PrecisionSliderAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const float DefaultFloatPrecision = .5f;
        public const int DefaultIntPrecision = 10;

        /// <summary>
        /// Slider minimum allowed value.
        /// </summary>
        public readonly float MinValue = 0f;

        /// <summary>
        /// Slider maximum allowed value.
        /// </summary>
        public readonly float MaxValue = 0f;

        /// <summary>
        /// Extra slider precision. This represents half of the difference between its minimum and maximum value.
        /// </summary>
        public readonly float Precision = 0f;

        // -----------------------

        /// <inheritdoc cref="PrecisionSliderAttribute(float, float, float)"/>
        public PrecisionSliderAttribute(int _minValue, int _maxValue, int _precision = DefaultIntPrecision) : this(_minValue, _maxValue, (float)_precision) { }

        /// <param name="_minValue"><inheritdoc cref="MinValue" path="/summary"/></param>
        /// <param name="_maxValue"><inheritdoc cref="MaxValue" path="/summary"/></param>
        /// <param name="_precision"><inheritdoc cref="Precision" path="/summary"/></param>
        /// <inheritdoc cref="PrecisionSliderAttribute"/>
        public PrecisionSliderAttribute(float _minValue, float _maxValue, float _precision = DefaultFloatPrecision)
        {
            MinValue = _minValue;
            MaxValue = _maxValue;
            Precision = _precision;
        }
        #endregion
    }
}
