// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// A list of emoji that can be converted to their ascii value using
    /// <see cref="AsciiEmojiExtensions.Get(AsciiEmoji)"/>.
    /// </summary>
    public enum AsciiEmoji {
        FlippingTable,
        AngryFlippingTable,
        AlarmedFlippingTable,
        FlippingYou,
        TableDown,

        FlowerGirl,
        Happy,
        Angry,
        Sad,
    }

    /// <summary>
    /// Contains multiple <see cref="AsciiEmoji"/>-related extension methods.
    /// </summary>
    public static class AsciiEmojiExtensions {
        #region Content
        /// <inheritdoc cref="Get(AsciiEmoji, Color)"/>
        public static string Get(this AsciiEmoji _emoji) {
            switch (_emoji) {
                case AsciiEmoji.FlippingTable:
                    string _value = @"(╯°□°)╯︵ ┻━┻";
                    return _value;

                case AsciiEmoji.AngryFlippingTable:
                    return @"(ノಠ益ಠ)ノ彡 ┻━┻";

                case AsciiEmoji.AlarmedFlippingTable:
                    return @"(┛◉Д◉)┛彡 ┻━┻";

                case AsciiEmoji.FlippingYou:
                    return @"(╯°Д°)╯︵ ( \o°o)\ ";

                case AsciiEmoji.TableDown:
                    return @"┬─┬ノ( º _ ºノ)";

                case AsciiEmoji.FlowerGirl:
                    return @"(◕‿◕✿)";

                case AsciiEmoji.Happy:
                    return @"(｡◕‿◕｡)";

                case AsciiEmoji.Angry:
                    return @"(つ◉益◉)つ";

                case AsciiEmoji.Sad:
                    return @"(.づ◡﹏◡)づ.";

                default:
                    return "???";
            }
        }

        /// <inheritdoc cref="Get(AsciiEmoji, Color)"/>
        public static string Get(this AsciiEmoji _emoji, SuperColor _color) {
            return Get(_emoji, _color.Get());
        }

        /// <summary>
        /// Get the <see cref="string"/> representation value of this <see cref="AsciiEmoji"/>.
        /// </summary>
        /// <param name="_emoji">The <see cref="AsciiEmoji"/> to get the associated value.</param>
        /// <param name="_color">The color used to get this emoji.</param>
        /// <returns>This emoji ascii <see cref="string"/> value.</returns>
        public static string Get(this AsciiEmoji _emoji, Color _color) {
            return RichTextUtility.Color(Get(_emoji), _color);
        }
        #endregion
    }
}
