// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Only shows this field in the inspector when a specific condition is fulfilled.
    /// <para/>
    /// This condition value can be either be a field, a property or a method.
    /// </summary>
	public class ShowIfAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Name of the class member to get value from,
        /// used as a condition to know if this field should be visible or not.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="bool"/>.
        /// </summary>
        public readonly MemberValue<bool> ConditionMember = default;

        /// <summary>
        /// Defines how this field condition value is considered as fulfilled.
        /// </summary>
        public readonly ConditionType ConditionType = ConditionType.True;

        // -----------------------

        /// <param name="_conditionMember"><inheritdoc cref="ConditionMember" path="/summary"/></param>
        /// <param name="_conditionType"><inheritdoc cref="ConditionType" path="/summary"/></param>
        /// <inheritdoc cref="ShowIfAttribute"/>
        public ShowIfAttribute(string _conditionMember, ConditionType _conditionType = ConditionType.True)
        {
            ConditionMember = _conditionMember;
            ConditionType = _conditionType;
        }
        #endregion
    }
}
