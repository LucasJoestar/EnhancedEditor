// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="EnhancedLogger"/> initializer class,
    /// using conditional compiltation symbols.
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
        [Conditional("ENHANCED_LOGGER"), RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            EnhancedLogger.Initialize();
        }
        #endregion
    }
}
