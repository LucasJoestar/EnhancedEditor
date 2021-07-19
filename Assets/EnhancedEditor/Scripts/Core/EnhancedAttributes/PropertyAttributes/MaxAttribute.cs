// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor
{
    /// <summary>
    /// Restricts a float or int variable to a specific maximum value.
    /// </summary>
    public class MaxAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly float MaxValue = 0f;

        // -----------------------

        /// <inheritdoc cref="MaxAttribute"/>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public MaxAttribute(float _maxValue)
        {
            MaxValue = _maxValue;
        }
        #endregion
    }
}
