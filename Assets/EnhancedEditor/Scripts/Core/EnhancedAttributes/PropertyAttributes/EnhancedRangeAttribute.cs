// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Similar to the <see cref="RangeAttribute"/>, with an extra slider giving
    /// more control on value precision.
    /// </summary>
	public class EnhancedRangeAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly float MinValue = 0f;
        public readonly float MaxValue = 0f;
        public readonly float Precision = 100f;

        // -----------------------

        /// <inheritdoc cref="EnhancedRangeAttribute"/>
        /// <param name="_minValue">Minimum slider value.</param>
        /// <param name="_maxValue">Maximum slider value.</param>
        /// <param name="_precision">Extra slider precision (must be greater than 1). The greater the value, the more precise the slider is.
        /// <para/>
        /// For instance, a slider from 0 to 5 with a precision of 100 will result in an extra slider of 0.05 range.</param>
        public EnhancedRangeAttribute(float _minValue, float _maxValue, float _precision = 100f)
        {
            MinValue = _minValue;
            MaxValue = _maxValue;

            Precision = Mathf.Max(1f, _precision);
        }
        #endregion
    }
}
