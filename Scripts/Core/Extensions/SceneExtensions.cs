// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine.SceneManagement;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="Scene"/>-related extension methods.
    /// </summary>
	public static class SceneExtensions {
        #region Content
        /// <inheritdoc cref="BuildSceneDatabase.IsCoreScene(Scene)"/>
        public static bool IsCoreScene(this Scene _scene) {
            return BuildSceneDatabase.Database.IsCoreScene(_scene);
        }
        #endregion
    }
}
