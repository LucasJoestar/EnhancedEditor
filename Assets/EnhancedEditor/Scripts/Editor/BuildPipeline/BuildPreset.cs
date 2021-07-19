// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Preset used to build game within the <see cref="BuildPipeline"/> window.
    /// </summary>
	public class BuildPreset : ScriptableObject
    {
        #region Content
        public string Description = "Enter a description here...";

        public BuildTarget BuildTarget = BuildTarget.StandaloneWindows;
        public BuildOptions BuildOptions = BuildOptions.Development;
        public int BuildAmount = 0;

        [SerializeField] private string scriptingDefineSymbols = string.Empty;

        public string ScriptingDefineSymbols
        {
            get => scriptingDefineSymbols;
            set
            {
                if (value == scriptingDefineSymbols)
                    return;

                string[] _symbols = value.Split(';');
                Array.Sort(_symbols);

                scriptingDefineSymbols = string.Join(";", _symbols);
            }
        }
        #endregion
    }
}
