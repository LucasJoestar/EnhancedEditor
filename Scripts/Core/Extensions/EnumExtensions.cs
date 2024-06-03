// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple <see cref="Enum"/>-related extension methods.
    /// </summary>
    public static class EnumExtensions {
        #region Conversion
        /// <summary>
        /// Get the integer value of a specific enum.
        /// </summary>
        /// <param name="_value">Enum value to convert.</param>
        /// <returns>Int value of this enum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this Enum _value) {
            return Convert.ToInt32(_value);
        }
        #endregion

        #region Flag
        /// <inheritdoc cref="EnumUtility.HasFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagUnsafe<TEnum>(this TEnum _enum, TEnum _flag)      where TEnum : unmanaged, Enum {
            return EnumUtility.HasFlag(_enum, _flag);
        }

        /// <inheritdoc cref="EnumUtility.AddFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum AddFlag<TEnum>(this TEnum _enum, TEnum _flag)           where TEnum : unmanaged, Enum {
            return EnumUtility.AddFlag(_enum, _flag);
        }

        /// <inheritdoc cref="EnumUtility.RemoveFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum RemoveFlag<TEnum>(this TEnum _enum, TEnum _flag)        where TEnum : unmanaged, Enum {
            return EnumUtility.RemoveFlag(_enum, _flag);
        }

        /// <inheritdoc cref="EnumUtility.AddFlagRef{TEnum}(ref TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFlagRef<TEnum>(ref this TEnum _enum, TEnum _flag)     where TEnum : unmanaged, Enum {
            EnumUtility.AddFlagRef(ref _enum, _flag);
        }

        /// <inheritdoc cref="EnumUtility.RemoveFlagRef{TEnum}(ref TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFlagRef<TEnum>(ref this TEnum _enum, TEnum _flag)  where TEnum : unmanaged, Enum {
            EnumUtility.RemoveFlagRef(ref _enum, _flag);
        }
        #endregion

        #region Name
        /// <inheritdoc cref="EnumUtility.GetName(Enum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(this Enum _value) {
            return EnumUtility.GetName(_value);
        }
        #endregion
    }
}
