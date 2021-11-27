// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Associated this field with a validation member, which value is set to this one whenever it changes.
    /// <para/>
    /// Use this to perform additional operations when this field value is changed in the inspector.
    /// </summary>
    public class ValidationMemberAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Name of the class member to set whenever this value is changed.
        /// <para/>
        /// Can either be a field, a property or a one argument method, but it must be of the same type as this field.
        /// </summary>
        public readonly MemberValue<object> ValidationMember = default;

        /// <summary>
        /// Determines when the associated validation member value is updated.
        /// </summary>
        public readonly ActivationMode Mode = ActivationMode.Always;

        // -----------------------

        /// <param name="_validationMember"><inheritdoc cref="ValidationMember" path="/summary"/></param>
        /// <param name="_mode"><inheritdoc cref="Mode" path="/summary"/></param>
        /// <inheritdoc cref="ValidationMemberAttribute"/>
        public ValidationMemberAttribute(string _validationMember, ActivationMode _mode = ActivationMode.Always)
        {
            ValidationMember = _validationMember;
            Mode = _mode;
        }
        #endregion
    }
}
