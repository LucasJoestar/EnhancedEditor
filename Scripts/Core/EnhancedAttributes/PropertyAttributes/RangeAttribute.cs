// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Restricts a <see cref="float"/> or an <see cref="int"/> value to a specific range.
    /// </summary>
    public class RangeAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Range allowed value.
        /// </summary>
        public readonly Vector2 Range = new Vector2();

        // -----------------------

        /// <param name="_minValue">Minimum allowed value.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        /// <inheritdoc cref="RangeAttribute"/>
        public RangeAttribute(float _minValue, float _maxValue)
        {
            Range = new Vector2(_minValue, _maxValue);
        }
        #endregion
    }
}
