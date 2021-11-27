// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a <see cref="Vector2"/> or a <see cref="Vector2Int"/> as a min-max slider,
    /// used to edit both a minimum and a maximum value.
    /// </summary>
    public class MinMaxAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Slider minimum allowed value.
        /// </summary>
        public readonly float MinValue = 0f;

        /// <summary>
        /// Slider maximum allowed value.
        /// </summary>
        public readonly float MaxValue = 0f;

        /// <summary>
        /// Name of the class member to get value from,
        /// used to determine both the minimum and maximum allowed value of the slider.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="Vector2"/>
        /// (min and max value respectively as x and y).
        /// </summary>
        public readonly MemberValue<Vector2>? MinMaxMember = null;

        // -----------------------

        /// <param name="_minValue"><inheritdoc cref="MinValue" path="/summary"/></param>
        /// <param name="_maxValue"><inheritdoc cref="MaxValue" path="/summary"/></param>
        /// <inheritdoc cref="MinMaxAttribute"/>
        public MinMaxAttribute(float _minValue, float _maxValue)
        {
            MinValue = _minValue;
            MaxValue = _maxValue;
        }

        /// <param name="_minMaxMember"><inheritdoc cref="MinMaxMember" path="/summary"/></param>
        /// <inheritdoc cref="MinMaxAttribute"/>
        public MinMaxAttribute(string _minMaxMember)
        {
            MinMaxMember = _minMaxMember;
        }
        #endregion
    }
}
