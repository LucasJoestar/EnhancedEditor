// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

namespace EnhancedEditor {
    /// <summary>
    /// Only shows this field in the inspector if the "Display Hidden Property" in toggled in the preferences.
    /// </summary>
	public sealed class HiddenPropertyAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <inheritdoc cref="HiddenPropertyAttribute"/>
        public HiddenPropertyAttribute() { }
        #endregion
    }
}
