// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// A list of emoji that can be converted to their unicode value using
    /// <see cref="UnicodeEmojiExtensions.Get(UnicodeEmoji)"/>.
    /// </summary>
    public enum UnicodeEmoji {  // The enum index can be used as decimal code.
        CheckMark = 10004,
        Cross = 10006,
        HeavyCross = 10008,
        GreekCross = 10010,

        Star = 9733,
        StarFourPoint = 10022,

        Spade = 9824,
        HeartOutline = 9825,
        Club = 9827,
        Heart = 9829,
        Diamond = 9830,

        Flower = 10047,
        BeamedEighthNote = 9835,
        EighthNote = 9834,

        LeftArrow = 8592,
        UpArrow = 8593,
        RightArrow = 8594,
        DownArrow = 8595,

        HeavyRightArrow = 10132,

        UpTriangle = 9650,
        RightTriangle = 9654,
        DownTriangle = 9660,
        LeftTriangle = 9664,
    }

    /// <summary>
    /// Contains multiple <see cref="UnicodeEmoji"/>-related extension methods.
    /// </summary>
    public static class UnicodeEmojiExtensions {
        #region Content
        /// <inheritdoc cref="Get(UnicodeEmoji, Color)"/>
        public static string Get(this UnicodeEmoji _emoji) {
            string _hex = ((int)_emoji).ToString("X");
            return char.ConvertFromUtf32(Convert.ToInt32(_hex, 16));
        }

        /// <inheritdoc cref="Get(UnicodeEmoji, Color)"/>
        public static string Get(this UnicodeEmoji _emoji, SuperColor _color) {
            return Get(_emoji, _color.Get());
        }

        /// <summary>
        /// Get the <see cref="string"/>  representation value of this <see cref="UnicodeEmoji"/>.
        /// </summary>
        /// <param name="_emoji">The <see cref="UnicodeEmoji"/> to get the associated value.</param>
        /// <param name="_color">The color used to get this emoji.</param>
        /// <returns>This emoji unicode <see cref="string"/> value.</returns>
        public static string Get(this UnicodeEmoji _emoji, Color _color) {
            return RichTextUtility.Color(Get(_emoji), _color);
        }
        #endregion
    }
}
