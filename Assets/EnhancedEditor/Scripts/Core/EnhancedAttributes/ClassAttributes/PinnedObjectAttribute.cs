// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Creates and associates a PinAsset (<see cref="ScriptableObject"/>) to each instance of this component in the project.
    /// <para/>
    /// Created asset allow users to ping associated component <see cref="GameObject"/>
    /// in its scene from a simple button, easing important scene objects management in project.
    /// </summary>
    public class PinnedObjectAttribute : EnhancedClassAttribute
    {
        #region Global Members
        public readonly string Path = string.Empty;

        // -----------------------

        /// <inheritdoc cref="PinnedObjectAttribute"/>
        /// <param name="_path">Path where to create and store associated PinAssets (for exemple: MyDatas/Attacks).</param>
        public PinnedObjectAttribute(string _path)
        {
            Path = _path;
        }
        #endregion
    }
}
