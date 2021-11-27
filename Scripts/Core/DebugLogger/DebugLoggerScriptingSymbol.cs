// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor
{
    /// <summary>
    /// Class that exist only for the use of its <see cref="ScriptingDefineSymbolAttribute"/>,
    /// allowing to enable / disable the DebugLogger logs from the BuildPipeline window.
    /// </summary>
    [Serializable, ScriptingDefineSymbol("DEBUG_LOGGER", "Debug Logger")]
    internal sealed class DebugLoggerScriptingSymbol { }
}
