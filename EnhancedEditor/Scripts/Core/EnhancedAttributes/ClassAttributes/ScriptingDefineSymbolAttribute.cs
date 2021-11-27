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
        /// <summary>
        /// Scripting define symbol to manage activation.
        /// <para/>
        /// Exemple: <c>UNITY_EDITOR</c>
        /// </summary>
        public readonly string Symbol = string.Empty;

        /// <summary>
        /// Short desctiption of this symbol (will be displayed next to it on the BuildPipeline window).
        /// </summary>
        public readonly string Description = string.Empty;

        // -----------------------

        /// <param name="_symbol"><inheritdoc cref="Symbol" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <inheritdoc cref="ScriptingDefineSymbolAttribute"/>
        public ScriptingDefineSymbolAttribute(string _symbol, string _description)
        {
            Symbol = _symbol.Replace(' ', '_');
            Description = _description;
        }
        #endregion
    }
}
