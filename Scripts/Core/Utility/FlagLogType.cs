// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Flag <see cref="LogType"/> enum, with additional values.
    /// </summary>
    [Flags]
    public enum FlagLogType {
        None        = 0,
        Log         = 1 << 1,
        Warning     = 1 << 2,
        Error       = 1 << 3,
        Assert      = 1 << 4,
        Exception   = 1 << 5,
    }

    /// <summary>
    /// Contains multiple <see cref="LogType"/> and <see cref="FlagLogType"/> related extension methods.
    /// </summary>
    public static class LogTypeExtensions {
        #region Content
        /// <summary>
        /// Get this <see cref="LogType"/> as a <see cref="FlagLogType"/> value.
        /// </summary>
        /// <param name="_type">The value to convert.</param>
        /// <returns>The converted <see cref="FlagLogType"/> value.</returns>
        public static FlagLogType ToFlag(this LogType _type) {
            switch (_type) {
                case LogType.Error:
                    return FlagLogType.Error;

                case LogType.Assert:
                    return FlagLogType.Assert;

                case LogType.Warning:
                    return FlagLogType.Warning;

                case LogType.Log:
                    return FlagLogType.Log;

                case LogType.Exception:
                    return FlagLogType.Exception;

                default:
                    return FlagLogType.None;
            }
        }
        #endregion
    }
}
