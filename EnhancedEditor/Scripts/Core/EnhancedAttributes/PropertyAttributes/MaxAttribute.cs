// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Restrains a <see cref="float"/> or an <see cref="int"/> value so that it does not exceed a specific maximum.
    /// </summary>
    public class MaxAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Maximum allowed value.
        /// </summary>
        public readonly float MaxValue = 0f;

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this field maximum allowed value.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.
        /// </summary>
        public readonly MemberValue<float>? MaxMember = null;

        // -----------------------

        /// <param name="_maxValue"><inheritdoc cref="MaxValue" path="/summary"/></param>
        /// <inheritdoc cref="MaxAttribute"/>
        public MaxAttribute(float _maxValue)
        {
            MaxValue = _maxValue;
        }

        /// <param name="_maxMember"><inheritdoc cref="MaxMember" path="/summary"/></param>
        /// <inheritdoc cref="MaxAttribute"/>
        public MaxAttribute(string _maxMember)
        {
            MaxMember = _maxMember;
        }
        #endregion
    }
}
