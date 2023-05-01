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

        /// <summary>
        /// Removes all special character(s) from a given input <see cref="string"/>.
        /// </summary>
        /// <param name="_value">The <see cref="string"/> to remove all special character(s) from.</param>
        /// <returns>The new formatted input <see cref="string"/> value.</returns>
        public static string RemoveSpecialCharacter(this string _value) {

            // Replaces everything that is not (^) a word character (\w), a digit (\d) or whitespace (\s) with an empty string.
            return Regex.Replace(_value, @"[^\w\d\s]", "");
        }

        /// <summary>
        /// Escapes this <see cref="string"/> value.
        /// </summary>
        /// <param name="_value">String value to parse.</param>
        /// <returns>Escaped <see cref="string"/> value.</returns>
        public static string Escape(this string _value) {
            return Regex.Escape(_value);
        }

        /// <summary>
        /// Unescapes this <see cref="string"/> value.
        /// </summary>
        /// <param name="_value">String value to parse.</param>
        /// <returns>Unescaped <see cref="string"/> value.</returns>
        public static string Unescape(this string _value) {
            return Regex.Unescape(_value);
        }

        /// <inheritdoc cref="GetPrefix(string, string)"/>
        public static string GetPrefix(this string _string) {
            return GetPrefix(_string, "_");
        }

        /// <summary>
        /// Get the prefix of this <see cref="string"/>.
        /// <br/> Uses the '_' character as a default separator.
        /// </summary>
        /// <param name="_string">This <see cref="string"/> value to get the prefix from.</param>
        /// <param name="_separator">This separator of this <see cref="string"/> prefix to search for.</param>
        /// <returns>The prefix of this <see cref="string"/>.</returns>
        public static string GetPrefix(this string _string, string _separator) {
            int _index = _string.IndexOf(_separator);

            return (_index != -1)
                 ? _string.Substring(0, _index + 1)
                 : string.Empty;
        }

        /// <inheritdoc cref="RemovePrefix(string, string)"/>
        public static string RemovePrefix(this string _string) {
            return RemovePrefix(_string, "_");
        }

        /// <summary>
        /// Removes the prefix of this <see cref="string"/>.
        /// <br/> Uses the '_' character as a default separator.
        /// </summary>
        /// <param name="_string">This <see cref="string"/> value to remove the prefix from.</param>
        /// <param name="_separator">This separator of this <see cref="string"/> prefix to search for.</param>
        /// <returns>This <see cref="string"/> with removed prefix.</returns>
        public static string RemovePrefix(this string _string, string _separator) {
            int _index = _string.IndexOf(_separator);

            return (_index != -1)
                 ? _string.Substring(_index + 1)
                 : _string;
        }

        /// <inheritdoc cref="EnhancedUtility.GetStableHashCode(string)"/>
        public static int GetStableHashCode(this string _string) {
            return EnhancedUtility.GetStableHashCode(_string);
        }

        /// <inheritdoc cref="EnhancedUtility.GetLongStableHashCode(string)"/>
        public static ulong GetLongStableHashCode(this string _string) {
            return EnhancedUtility.GetLongStableHashCode(_string);
        }
        #endregion
    }
}
