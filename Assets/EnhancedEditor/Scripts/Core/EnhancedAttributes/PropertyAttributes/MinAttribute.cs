// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Restricts a float or int variable to a specific minimum value.
    /// </summary>
	public class MinAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly float MinValue = 0f;

        // -----------------------

        /// <inheritdoc cref="MinAttribute"/>
        /// <param name="_minValue">Minimum allowed value.</param>
        public MinAttribute(float _minValue)
        {
            MinValue = _minValue;
        }
        #endregion
    }
}
