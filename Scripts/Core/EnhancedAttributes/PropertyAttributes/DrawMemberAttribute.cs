// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor {
    /// <summary>
    /// Associated this field with a draw member, which is called whenever this field is drawn.
    /// <para/>
    /// Use this to perform additional operations when this field value is drawn in the inspector.
    /// </summary>
    public class DrawMemberAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <summary>
        /// Name of the class member to call whenever this field is drawn.
        /// <para/>
        /// Must be a parameterless method, or a member of the same type as this field.
        /// </summary>
        public readonly MemberValue<object> DrawMember = default;

        /// <summary>
        /// Determines when the associated draw member value is called.
        /// </summary>
        public readonly ActivationMode Mode = ActivationMode.Always;

        // -----------------------

        /// <param name="_drawMember"><inheritdoc cref="DrawMember" path="/summary"/></param>
        /// <param name="_mode"><inheritdoc cref="Mode" path="/summary"/></param>
        /// <inheritdoc cref="DrawMemberAttribute"/>
        public DrawMemberAttribute(string _drawMember, ActivationMode _mode = ActivationMode.Always) {
            DrawMember = _drawMember;
            Mode = _mode;
        }
        #endregion
    }
}
