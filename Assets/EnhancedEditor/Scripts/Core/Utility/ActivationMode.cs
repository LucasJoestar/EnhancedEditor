// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Used to define when a specific tool or system is active.
    /// </summary>
    public enum ActivationMode
    {
        Play,
        Editor,
        Always
    }

    /// <summary>
    /// Contains multiple <see cref="ActivationMode"/>-related extension methods.
    /// </summary>
    public static class ActivationModeExtensions
    {
        /// <summary>
        /// Is this mode currently active?
        /// </summary>
        /// <param name="_mode">Mode to check active state.</param>
        /// <returns>True if the mode is actually active, false otherwise.</returns>
        public static bool IsActive(this ActivationMode _mode)
        {
            switch (_mode)
            {
                case ActivationMode.Play:
                    return Application.isPlaying;

                case ActivationMode.Editor:
                   return !Application.isPlaying;

                case ActivationMode.Always:
                    return true;

                default:
                    return false;
            }
        }
    }
}
