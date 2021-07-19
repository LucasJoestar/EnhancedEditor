// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Used to define when a tool or a specific system should be used.
    /// </summary>
    public enum ActivationMode
    {
        Play,
        Editor,
        Always
    }

    public static class ActivationModeExtensions
    {
        /// <summary>
        /// Is this mode active?
        /// </summary>
        /// <param name="_mode">Mode to check activation.</param>
        /// <returns>True if the mode is active indeed, false otherwise.</returns>
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
