// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Only displays this field in the inspector when a specific condition is fulfilled.
    /// <para/>
    /// Condition can be whether a field, a property or a method.
    /// </summary>
	public class ShowIfAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly string ConditionMemberName = string.Empty;
        public readonly ConditionType Type = ConditionType.True;

        // -----------------------

        /// <inheritdoc cref="ShowIfAttribute"/>
        /// <param name="_conditionMemberName">Name of the class member to get value from, acting as condition validator.</param>
        /// <param name="_type">Defines how the condition is considered as fulfilled.</param>
        public ShowIfAttribute(string _conditionMemberName, ConditionType _type = ConditionType.True)
        {
            ConditionMemberName = _conditionMemberName;
            Type = _type;
        }
        #endregion
    }
}
