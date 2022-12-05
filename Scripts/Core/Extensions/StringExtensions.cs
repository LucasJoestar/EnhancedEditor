// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Text.RegularExpressions;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="string"/>-related extension methods.
    /// </summary>
	public static class StringExtensions {
        #region Content
        private static readonly Regex whitespaceRegex = new Regex(@"\s+");

        // -----------------------

        /// <summary>
        /// Removes all whitespaces from a given input <see cref="string"/>.
        /// </summary>
        /// <param name="_value">The <see cref="string"/> to remove all whitespace from.</param>
        /// <returns>The new formatted input <see cref="string"/> value.</returns>
        public static string RemoveWhitespace(this string _value) {
            return whitespaceRegex.Replace(_value, string.Empty);
        }
        #endregion
    }
}
