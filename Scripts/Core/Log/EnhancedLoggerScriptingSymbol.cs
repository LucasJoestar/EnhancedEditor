// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Class that exist only for the use of its <see cref="ScriptingDefineSymbolAttribute"/>,
    /// allowing to enable / disable the <see cref="EnhancedLogger"/> from the BuildPipeline window.
    /// </summary>
    [Serializable]
    [ScriptingDefineSymbol("ENHANCED_LOGGER", "Enhanced Logger")]
    [ScriptingDefineSymbol("ENHANCED_LOGS", "Development / Debug Logs")]
    internal sealed class EnhancedLoggerScriptingSymbol { }
}
