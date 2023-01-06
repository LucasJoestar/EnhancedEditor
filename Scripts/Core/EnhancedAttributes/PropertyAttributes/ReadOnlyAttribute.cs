// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Displays this value as a readonly (non editable) field.
    /// </summary>
    public class ReadOnlyAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// If this field is a boolean, defines if using a classic or a radio-style toggle.
        /// </summary>
        public readonly bool UseRadioToggle = false;

        /// <summary>
        /// Name of the class member to get value from,
        /// indicating if this field should be editable or not.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="bool"/>.
        /// </summary>
        public readonly MemberValue<bool>? ConditionMember = null;

        // -----------------------

        /// <param name="_useRadioToggle"><inheritdoc cref="UseRadioToggle" path="/summary"/></param>
        /// <inheritdoc cref="ReadOnlyAttribute"/>
        public ReadOnlyAttribute(bool _useRadioToggle = false)
        {
            UseRadioToggle = _useRadioToggle;
        }

        /// <param name="_conditionMember"><inheritdoc cref="ConditionMember" path="/summary"/></param>
        /// <inheritdoc cref="ReadOnlyAttribute(bool)"/>
        public ReadOnlyAttribute(string _conditionMember, bool _useRadioToggle = false) : this(_useRadioToggle) {
            ConditionMember = _conditionMember;
        }
        #endregion
    }
}
