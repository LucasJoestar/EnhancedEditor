// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Dynamic-height word-wrapped <see cref="TextAreaAttribute"/>,
    /// automatically adjusting its size according to its content.
    /// </summary>
	public class EnhancedTextAreaAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// If true, the text area will take the full width available on screen.
        /// </summary>
        public readonly bool IsWide = false;

        // -----------------------

        /// <param name="_isWide"><inheritdoc cref="IsWide" path="/summary"/></param>
        /// <inheritdoc cref="EnhancedTextAreaAttribute"/>
        public EnhancedTextAreaAttribute(bool _isWide = false)
        {
            IsWide = _isWide;
        }
        #endregion
    }
}
