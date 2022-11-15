// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Class that exist only for the use of its <see cref="ScriptingDefineSymbolAttribute"/>,
    /// allowing to enable / disable non-release development features from the BuildPipeline window.
    /// </summary>
    [Serializable, ScriptingDefineSymbol("DEVELOPMENT", "Development / Debug Features")]
    internal sealed class DevelopmentScriptingSymbol { }
}
