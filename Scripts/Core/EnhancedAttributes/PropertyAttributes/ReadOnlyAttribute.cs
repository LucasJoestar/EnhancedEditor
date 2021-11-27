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

        // -----------------------

        /// <param name="_useRadioToggle"><inheritdoc cref="UseRadioToggle" path="/summary"/></param>
        /// <inheritdoc cref="ReadOnlyAttribute"/>
        public ReadOnlyAttribute(bool _useRadioToggle = false)
        {
            UseRadioToggle = _useRadioToggle;
        }
        #endregion
    }
}
