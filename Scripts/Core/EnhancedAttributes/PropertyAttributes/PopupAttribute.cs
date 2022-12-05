// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;

namespace EnhancedEditor {
    /// <summary>
    /// Use this on an <see cref="int"/> variable to let the user select its value from a popup field.
    /// </summary>
    public class PopupAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <summary>
        /// Name of the class member used to get the array of the selectable options from the popup.
        /// <para/>
        /// Can either be a field, a property or a one argument method, but it must return an <see cref="IList{string}"/> of <see cref="string"/>.
        /// </summary>
        public readonly MemberValue<IList<string>> OptionMember = default;

        // -----------------------

        /// <param name="_optionMember"><inheritdoc cref="OptionMember" path="/summary"/></param>
        /// <inheritdoc cref="PopupAttribute"/>
        public PopupAttribute(string _optionMember) {
            OptionMember = _optionMember;
        }
        #endregion
    }
}
