// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="BuildPipeline"/> related <see cref="ScriptableObject"/> settings.
    /// </summary>
    [NonEditable("These datas are sensitive. Please use the BuildPipeline window to edit these settings")]
	public class BuildPipelineSettings : ScriptableObject
    {
        #region Content
        /// <summary>
        /// Directory where to search for builds and where new ones are created.
        /// </summary>
        public string BuildDirectory = string.Empty;

        // -----------------------

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(BuildDirectory))
                BuildDirectory = Application.dataPath.Replace("Assets", "Builds/");
        }
        #endregion
    }
}
