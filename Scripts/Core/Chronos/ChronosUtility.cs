// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if UNITY_EDITOR
#define EDITOR_CHRONOS
#endif

using UnityEngine;

#if EDITOR_CHRONOS
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// Chronos global utility static class.
    /// </summary>
    #if EDITOR_CHRONOS
    [InitializeOnLoad]
    #endif
    public static class ChronosUtility {
        #region Global Members
        #if EDITOR_CHRONOS
        private static double lastUpdateTime = 0d;
        private static float editorDeltaTime = 0f;
        #endif

        /// <summary>
        /// Interval in seconds from the last frame to the current one.
        /// <br/> Works in editor, using the current time scale modifier.
        /// </summary>
        public static float DeltaTime {
            get {
                #if EDITOR_CHRONOS
                if (!Application.isPlaying) {
                    return editorDeltaTime * Time.timeScale;
                }
                #endif

                return Time.deltaTime;
            }
        }

        /// <summary>
        /// Time-scale independant interval in seconds from the last frame to the current one.
        /// <br/> Works in editor.
        /// </summary>
        public static float RealDeltaTime {
            get {
                #if EDITOR_CHRONOS
                if (!Application.isPlaying) {
                    return editorDeltaTime;
                }
                #endif

                return Time.unscaledDeltaTime;
            }
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        #if EDITOR_CHRONOS
        // Editor constructor.
        static ChronosUtility() {
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate() {
            // Editor deltaTime.
            if (!Application.isPlaying) {

                editorDeltaTime = (float)(EditorApplication.timeSinceStartup - lastUpdateTime);
                lastUpdateTime = EditorApplication.timeSinceStartup;
            }
        }
        #endif
        #endregion

        #region Utility
        /// <summary>
        /// Get the interval in seconds from the last frame to the current one.
        /// <br/> Works in editor.
        /// </summary>
        /// <param name="_realTime">If true, time scale will be ignored.</param>
        /// <returns>Interval in seconds from the last frame to the current one.</returns>
        public static float GetDeltaTime(bool _realTime) {
            return _realTime ? RealDeltaTime : DeltaTime;
        }
        #endregion
    }
}
