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

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this field range allowed value.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="Vector2"/>.
        /// </summary>
        public readonly MemberValue<Vector2>? RangeMember = null;

        // -----------------------

        /// <param name="_minValue">Minimum allowed value.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        /// <inheritdoc cref="RangeAttribute"/>
        public RangeAttribute(float _minValue, float _maxValue)
        {
            Range = new Vector2(_minValue, _maxValue);
        }

        /// <param name="_rangeMember"><inheritdoc cref="RangeMember" path="/summary"/></param>
        /// <inheritdoc cref="RangeAttribute"/>
        public RangeAttribute(string _rangeMember) {
            RangeMember = _rangeMember;
        }
        #endregion
    }
}
