// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="EnhancedLogger"/> initializer class, using conditional compiltation symbols.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    internal static class EnhancedLoggerInitializer {
        #region Content
        // Only called in the editor.
        static EnhancedLoggerInitializer() {
            Initialize();
        }

        // -----------------------

        // Uses the first registration callback to be initialized as soon as possible.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            #if ENHANCED_LOGGER
            EnhancedLogger.Initialize();
            #endif
        }
        #endregion
    }
}
