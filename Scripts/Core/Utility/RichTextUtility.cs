// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple rich text related utility methods.
    /// </summary>
	public static class RichTextUtility {
        #region Encapsulation
        /// <inheritdoc cref="Encapsulate(string, string, string)"/>
        public static string Encapsulate(string _text, string _richTextTag) {
            return $"<{_richTextTag}>{_text}</{_richTextTag}>";
        }

        /// <summary>
        /// Encapsulates a specific <see cref="string"/> text within a rich text tag.
        /// </summary>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        /// <param name="_richTextTag">The rich text tag to encapsulate this text within.</param>
        /// <param name="_tagValue">The value of this tag.</param>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        public static string Encapsulate(string _text, string _richTextTag, string _tagValue) {
            return $"<{_richTextTag}={_tagValue}>{_text}</{_richTextTag}>";
        }
        #endregion

        #region Rich Text
        public const string ColorTag    = "color";
        public const string SizeTag     = "size";
        public const string BoldTag     = "b";
        public const string ItalicTag   = "i";

        // --- Color --- \\

        /// <inheritdoc cref="Color(string, UnityEngine.Color)"/>
        public static string Color(string _text, SuperColor _color) {
            return Color(_text, _color.Get());
        }

        /// <summary>
        /// Colors a specific <see cref="string"/> text.
        /// </summary>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        /// <param name="_color">The new color of the text.</param>
        /// <returns><inheritdoc cref="Doc(string)" path="/returns"/></returns>
        public static string Color(string _text, Color _color) {
            return Encapsulate(_text, ColorTag, $"#{ColorUtility.ToHtmlStringRGBA(_color)}");
        }

        // --- Various --- \\

        /// <summary>
        /// Renders a specific <see cref="string"/> text in boldface.
        /// </summary>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        /// <returns><inheritdoc cref="Doc(string)" path="/returns"/></returns>
        public static string Bold(string _text) {
            return Encapsulate(_text, BoldTag);
        }

        /// <summary>
        /// Renders a specific <see cref="string"/> text in italics.
        /// </summary>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        /// <returns><inheritdoc cref="Doc(string)" path="/returns"/></returns>
        public static string Italic(string _text) {
            return Encapsulate(_text, ItalicTag);
        }

        /// <summary>
        /// Modifies the size of a specific <see cref="string"/> text.
        /// </summary>
        /// <param name="_text"><inheritdoc cref="Doc(string)" path="/param[@name='_text']"/></param>
        /// <param name="_size">The new size of the text.</param>
        /// <returns><inheritdoc cref="Doc(string)" path="/returns"/></returns>
        public static string Size(string _text, int _size) {
            return Encapsulate(_text, SizeTag, _size.ToString());
        }
        #endregion

        #region Documentation
        /// <summary>
        /// Documentation method.
        /// </summary>
        /// <param name="_text">The <see cref="string"/> text value to modify.</param>
        /// <returns>The modified value of the text.</returns>
        private static string Doc(string _text) => _text;
        #endregion
    }

    /// <summary>
    /// Contains multiple rich text <see cref="string"/> related extension methods.
    /// </summary>
    public static class RichTextStringExtensions {
        #region Content
        /// <inheritdoc cref="RichTextUtility.Color(string, SuperColor)"/>
        public static string Color(this string _text, SuperColor _color) {
            return RichTextUtility.Color(_text, _color);
        }

        /// <inheritdoc cref="RichTextUtility.Color(string, Color)"/>
        public static string Color(this string _text, Color _color) {
            return RichTextUtility.Color(_text, _color);
        }

        /// <inheritdoc cref="RichTextUtility.Bold(string)"/>
        public static string Bold(this string _text) {
            return RichTextUtility.Bold(_text);
        }

        /// <inheritdoc cref="RichTextUtility.Italic(string)"/>
        public static string Italic(this string _text) {
            return RichTextUtility.Italic(_text);
        }

        /// <inheritdoc cref="RichTextUtility.Size(string, int)"/>
        public static string Size(this string _text, int _size) {
            return RichTextUtility.Size(_text, _size);
        }

        // -----------------------

        /// <inheritdoc cref="RichTextUtility.Encapsulate(string, string)"/>
        public static string Encapsulate(this string _text, string _richTextTag) {
            return RichTextUtility.Encapsulate(_text, _richTextTag);
        }

        /// <inheritdoc cref="RichTextUtility.Encapsulate(string, string, string)"/>
        public static string Encapsulate(this string _text, string _richTextTag, string _tagValue) {
            return RichTextUtility.Encapsulate(_text, _richTextTag, _tagValue);
        }
        #endregion
    }
}
