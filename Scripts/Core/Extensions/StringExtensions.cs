// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="string"/>-related extension methods.
    /// </summary>
	public static class StringExtensions {
        #region Content
        private static readonly Regex whitespaceRegex = new Regex(@"\s+");

        private static readonly char[] splitTextDefaultChars = new char[] {
            '\n', '\t', ' ', '.', ','
        };

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveSpecialCharacter(this string _value) {
            // Replaces everything that is not (^) a word character (\w), a digit (\d) or whitespace (\s) with an empty string.
            return Regex.Replace(_value, @"[^\w\d\s]", "");
        }

        /// <summary>
        /// Escapes this <see cref="string"/> value.
        /// </summary>
        /// <param name="_value">String value to parse.</param>
        /// <returns>Escaped <see cref="string"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Escape(this string _value) {
            return Regex.Escape(_value);
        }

        /// <summary>
        /// Unescapes this <see cref="string"/> value.
        /// </summary>
        /// <param name="_value">String value to parse.</param>
        /// <returns>Unescaped <see cref="string"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Unescape(this string _value) {
            return Regex.Unescape(_value);
        }

        // -------------------------------------------
        // Split
        // -------------------------------------------

        /// <inheritdoc cref="GetTextSplitIndex(string, int, int, char[], bool)"/>
        public static int GetTextSplitIndex(this string _text, bool _onlyWhiteSpaceForLastIndex = true) {
            return GetTextSplitIndex(_text, 0, _text.Length, splitTextDefaultChars, _onlyWhiteSpaceForLastIndex);
        }

        /// <inheritdoc cref="GetTextSplitIndex(string, int, int, char[], bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTextSplitIndex(this string _text, char[] _splitChars, bool _onlyWhiteSpaceForLastIndex = true) {
            return GetTextSplitIndex(_text, 0, _text.Length, _splitChars, _onlyWhiteSpaceForLastIndex);
        }

        /// <inheritdoc cref="GetTextSplitIndex(string, int, int, char[], bool)"/>
        public static int GetTextSplitIndex(this string _text, int _startIndex, int _count, bool _onlyWhiteSpaceForLastIndex = true) {
            return GetTextSplitIndex(_text, _startIndex, _count, splitTextDefaultChars, _onlyWhiteSpaceForLastIndex);
        }

        /// <summary>
        /// Get the index of the last character for splitting this text in a user-friendly way.
        /// </summary>
        /// <param name="_text">Text content.</param>
        /// <param name="_startIndex">Text start index.</param>
        /// <param name="_count">Amount of characters to check.</param>
        /// <param name="_splitChars">All characters used to detect a split.</param>
        /// <param name="_onlyWhiteSpaceForLastIndex">If true, only check for white spaces for the last character.</param>
        /// <returns>Index of this text to use for split (-1 if none).</returns>
        public static int GetTextSplitIndex(this string _text, int _startIndex, int _count, char[] _splitChars, bool _onlyWhiteSpaceForLastIndex = true) {

            int _index = _startIndex + _count;

            // Last index special.
            if (_onlyWhiteSpaceForLastIndex) {

                _index--;

                if (char.IsWhiteSpace(_text, _index)) {
                    return _index;
                }
            }

            // Reverse order for last element.
            for (int i = _index; i-- > _startIndex;) {

                char _char = _text[i];

                if (char.IsWhiteSpace(_char) || ArrayUtility.Contains(_splitChars, _char)) {
                    return i;
                }
            }

            return -1;
        }

        // -------------------------------------------
        // Prefix - Suffix
        // -------------------------------------------

        /// <inheritdoc cref="GetPrefix(string, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // -------------------------------------------
        // Hash
        // -------------------------------------------

        /// <inheritdoc cref="EnhancedUtility.GetStableHashCode(string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetStableHashCode(this string _string) {
            return EnhancedUtility.GetStableHashCode(_string);
        }

        /// <inheritdoc cref="EnhancedUtility.GetLongStableHashCode(string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetLongStableHashCode(this string _string) {
            return EnhancedUtility.GetLongStableHashCode(_string);
        }
        #endregion
    }
}
