// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Displays this field as readonly (non editable).
    /// </summary>
    public class ReadOnlyAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly bool UseRadioToggle = false;

        // -----------------------

        /// <inheritdoc cref="ReadOnlyAttribute"/>
        /// <param name="_useRadioToggle">If this field is a boolean, defines if using a classic or a radio toggle.</param>
        public ReadOnlyAttribute(bool _useRadioToggle = false)
        {
            UseRadioToggle = _useRadioToggle;
        }
        #endregion
    }
}
