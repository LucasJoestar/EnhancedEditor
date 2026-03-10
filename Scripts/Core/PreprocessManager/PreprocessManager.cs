// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;

using Object = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// Inherit from this interface on any <see cref="ScriptableSettings"/>
    /// to automatically receive a callback on preprocess - when entering play mode and before a build.
    /// <para/>
    /// Use the <see cref="PreprocessManager"/> class for various related operations.
    /// </summary>
    public interface IPreprocessCallback {
        #region Content
        /// <summary>
        /// Called when to perform preprocess operations - when entering play mode and before a build.
        /// </summary>
        /// <returns>True if this asset should automatically be set as dirty, false otherwise.</returns>
        bool OnPreprocess();
        #endregion
    }

    /// <summary>
    /// Utility class used for various preprocess operations.
    /// </summary>
    public static class PreprocessManager {
        #region Content
        private static Func<Type, Object[]> onLoadAssets = null;

        // -----------------------

        /// <summary>
        /// Loads all assets of a given type in the project.
        /// </summary>
        /// <typeparam name="T">Type of assets to load.</typeparam>
        /// <returns>All assets of the given type in the project.</returns>
        public static T[] Load<T>() where T : Object {
            return Array.ConvertAll(onLoadAssets(typeof(T)), x => x as T);
        }

        /// <summary>
        /// Set all editor delegates of this preprocessor.
        /// </summary>
        internal static void SetEditorDelegates(Func<Type, Object[]> _onLoadAssets) {
            onLoadAssets = _onLoadAssets;
        }
        #endregion
    }
}
