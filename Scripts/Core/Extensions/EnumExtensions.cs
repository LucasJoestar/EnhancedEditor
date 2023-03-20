// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple <see cref="Enum"/>-related extension methods.
    /// </summary>
    public static class EnumExtensions {
        #region Flags
        /// <summary>
        /// Adds a flag to this enum.
        /// </summary>
        /// <typeparam name="T">This enum type.</typeparam>
        /// <param name="_value">This enum value.</param>
        /// <param name="_flag">The flag value to add.</param>
        /// <returns>This enum new value.</returns>
        public static T AddFlag<T>(this Enum _value, T _flag) where T : Enum {
            return (T)(object)(_value.ToInt() | _flag.ToInt());
        }

        /// <summary>
        /// Removes a flag to this enum.
        /// </summary>
        /// <typeparam name="T">This enum type.</typeparam>
        /// <param name="_value">This enum value.</param>
        /// <param name="_flag">The flag value to remove.</param>
        /// <returns>This enum new value.</returns>
        public static T RemoveFlag<T>(this Enum _value, T _flag) where T : Enum {
            return (T)(object)(_value.ToInt() & ~_flag.ToInt());
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Get the integer value of a specific enum.
        /// </summary>
        /// <param name="_value">Enum value to convert.</param>
        /// <returns>Int value of this enum.</returns>
        public static int ToInt(this Enum _value) {
            return Convert.ToInt32(_value);
        }
        #endregion

        #region Name
        /// <inheritdoc cref="EnumUtility.GetName(Enum)"/>
        public static string GetName(this Enum _value) {
            return EnumUtility.GetName(_value);
        }
        #endregion
    }
}
