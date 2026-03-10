// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Used to define when a specific tool or system is active.
    /// </summary>
    public enum ActivationMode {
        Play    = 0,

        Editor  = 1,
        Always  = 2,
    }

    /// <summary>
    /// Contains multiple <see cref="ActivationMode"/>-related extension methods.
    /// </summary>
    public static class ActivationModeExtensions {
        #region Content
        /// <summary>
        /// Is this mode currently active?
        /// </summary>
        /// <param name="_mode">Mode to check active state.</param>
        /// <returns>True if the mode is actually active, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActive(this ActivationMode _mode) {

            switch (_mode) {
                case ActivationMode.Play:
                    #if UNITY_EDITOR
                    return Application.isPlaying;
                    #else
                    return true;
                    #endif

                case ActivationMode.Editor:
                    #if UNITY_EDITOR
                    return !Application.isPlaying;
                    #else
                    return false;
                    #endif

                case ActivationMode.Always:
                    return true;

                default:
                    return false;
            }
        }
        #endregion
    }
}
