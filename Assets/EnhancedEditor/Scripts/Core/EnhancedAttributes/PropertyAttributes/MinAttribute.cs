// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Restrains a <see cref="float"/> or an <see cref="int"/> value so that it does not go under a specific minimum.
    /// </summary>
	public class MinAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Minimum allowed value.
        /// </summary>
        public readonly float MinValue = 0f;

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this field minimum allowed value.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.
        /// </summary>
        public readonly MemberValue<float>? MinMember = null;

        // -----------------------

        /// <param name="_minValue"><inheritdoc cref="MinValue" path="/summary"/></param>
        /// <inheritdoc cref="MinAttribute"/>
        public MinAttribute(float _minValue)
        {
            MinValue = _minValue;
        }

        /// <param name="_minMember"><inheritdoc cref="MinMember" path="/summary"/></param>
        /// <inheritdoc cref="MinAttribute"/>
        public MinAttribute(string _minMember)
        {
            MinMember = _minMember;
        }
        #endregion
    }
}
