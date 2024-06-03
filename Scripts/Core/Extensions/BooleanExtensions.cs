// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;

namespace EnhancedEditor {
    /// <summary>
    /// Special parser used to convert a <see cref="bool"/> into a <see cref="string"/>.
    /// </summary>
    public enum BooleanStringParser {
        TrueFalse       = 0,

        EnabledDisabled = 1,
        ActiveInactive  = 2,
    }

    /// <summary>
    /// Multiple extension methods related to the <see cref="bool"/> value type.
    /// </summary>
    public static class BooleanExtensions {
        #region Content
        /// <summary>
        /// Get a boolean as a sign.
        /// <br/> 1 if true, -1 otherwise.
        /// </summary>
        /// <param name="boolean">Boolean to get sign from.</param>
        /// <returns>Returns this boolean sign as 1 or -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(this bool _boolean) {
            return _boolean ? 1 : -1;
        }

        /// <summary>
        /// Get a boolean as a sign.
        /// <br/> 1 if true, -1 otherwise.
        /// </summary>
        /// <param name="_boolean">Boolean to get sign from.</param>
        /// <returns>Returns this boolean sign as 1 or -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Signf(this bool _boolean) {
            return _boolean ? 1f : -1f;
        }

        /// <summary>
        /// Converts this boolean to a formatted <see cref="string"/> value.
        /// </summary>
        /// <param name="_boolean">The boolean to convert.</param>
        /// <param name="_parser">Special parser to use.</param>
        /// <param name="_upperFirstLetter">Whether to parse the first letter as an upper character or not.</param>
        /// <returns>This boolean formatted <see cref="string"/> value.</returns>
        public static string ToString(this bool _boolean, BooleanStringParser _parser, bool _upperFirstLetter = true) {
            string _string;

            switch (_parser) {
                case BooleanStringParser.TrueFalse:
                    _string = _boolean ? "true" : "false";
                    break;

                case BooleanStringParser.EnabledDisabled:
                    _string = _boolean ? "enabled" : "disabled";
                    break;

                case BooleanStringParser.ActiveInactive:
                    _string = _boolean ? "active" : "inactive";
                    break;

                default:
                    return _boolean.ToString();
            }

            if (_upperFirstLetter) {
                _string = string.Concat(_string[..1].ToUpper(), _string[1..]); // Equivalent to: _string = string.Concat(_string.Substring(0, 1).ToUpper(), _string.Substring(1));
            }

            return _string;
        }
        #endregion
    }
}
