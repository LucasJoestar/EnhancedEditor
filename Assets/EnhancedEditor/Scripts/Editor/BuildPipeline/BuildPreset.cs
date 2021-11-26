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
    /// Preset used in the <see cref="BuildPipelineWindow"/> to build the game.
    /// </summary>
    public class BuildPreset : ScriptableObject, IComparable
    {
        #region Global Members
        /// <summary>
        /// A short description of this preset function.
        /// </summary>
        public string Description = "Enter a description here...";

        /// <summary>
        /// The build target platform this preset is configured for.
        /// </summary>
        public BuildTarget BuildTarget = BuildTarget.StandaloneWindows;

        /// <summary>
        /// Additional build options used when building with this preset.
        /// </summary>
        public BuildOptions BuildOptions = BuildOptions.Development;

        [SerializeField] internal int buildCount = 0;
        [SerializeField] internal string scriptingDefineSymbols = string.Empty;

        /// <summary>
        /// All active scripting define symbols when building with this preset.
        /// </summary>
        public string ScriptingDefineSymbols
        {
            get => scriptingDefineSymbols;
            set
            {
                string[] _symbols = value.Split(BuildPipelineWindow.ScriptingDefineSymbolSeparator);
                Array.Sort(_symbols);

                scriptingDefineSymbols = string.Join(BuildPipelineWindow.ScriptingDefineSymbolSeparator.ToString(), _symbols);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes this preset with another preset values.
        /// </summary>
        /// <param name="_template">Preset to initialize this object with.</param>
        public void Initialize(BuildPreset _template)
        {
            Description = _template.Description;
            BuildTarget = _template.BuildTarget;
            BuildOptions = _template.BuildOptions;
            scriptingDefineSymbols = _template.scriptingDefineSymbols;
        }
        #endregion

        #region Comparable
        int IComparable.CompareTo(object _other)
        {
            if ((_other == null) || !(_other is BuildPreset _preset))
                return 1;

            return name.CompareTo(_preset.name);
        }
        #endregion
    }
}
