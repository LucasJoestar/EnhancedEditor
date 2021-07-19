// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor
{
    /// <summary>
    /// Creates a custom scripting define symbol,
    /// whose activation can be managed from the BuildPipeline window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ScriptingDefineSymbolAttribute : EnhancedClassAttribute
    {
        #region Global Members
        public readonly string Symbol = string.Empty;
        public readonly string Description = string.Empty;

        // -----------------------

        /// <inheritdoc cref="ScriptingDefineSymbolAttribute"/>
        /// <param name="_symbol">Scripting define symbol to manage activation.</param>
        /// <param name="_description">Description to be displayed in front of the symbol management.</param>
        public ScriptingDefineSymbolAttribute(string _symbol, string _description)
        {
            Symbol = _symbol;
            Description = _description;
        }
        #endregion
    }
}
