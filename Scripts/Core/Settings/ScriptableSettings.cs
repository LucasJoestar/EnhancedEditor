// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using UnityEngine;

[assembly:InternalsVisibleTo("EnhancedFramework.Core")]
namespace EnhancedEditor {
    /// <summary>
    /// Base class to inherit from for all any game-related settings.
    /// </summary>
    public abstract class ScriptableSettings : ScriptableObject {
        #region Global Members
        /// <summary>
        /// Prevents inheriting from this class in other assemblies (except EnhancedFramework).
        /// </summary>
        internal protected ScriptableSettings() { }
        #endregion

        #region Content
        /// <summary>
        /// Initializes and set this settings instance as the general one to be used for the game.
        /// </summary>
        internal protected abstract void Init();
        #endregion
    }
}
