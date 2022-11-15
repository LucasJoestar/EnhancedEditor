// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine.SceneManagement;

namespace EnhancedEditor {
    /// <summary>
    /// Base class to inherit you own <see cref="SceneBundle"/> behaviours from.
    /// <br/> Can be used to store additional parameters and receive callbacks on bundle loading.
    /// </summary>
    [Serializable]
    public abstract class SceneBundleBehaviour {
        #region Loading
        /// <summary>
        /// Called when the <see cref="SceneBundle"/> starts being asynchronously unloaded.
        /// </summary>
        /// <param name="_bundle">The bundle being unloaded.</param>
        /// <param name="_operation">The unloading operation.</param>
        /// <param name="_options">Options used to unload the bundle.</param>
        internal protected virtual void OnUnloadAsyncBundle(SceneBundle _bundle, UnloadBundleAsyncOperation _operation, UnloadSceneOptions _options) { }

        /// <summary>
        /// Called when the <see cref="SceneBundle"/> starts being asynchronously loaded.
        /// </summary>
        /// <param name="_bundle">The bundle being loaded.</param>
        /// <param name="_operation">The loading operation.</param>
        /// <param name="_parameters">Parameters used to load the bundle.</param>
        internal protected virtual void OnLoadAsyncBundle(SceneBundle _bundle, LoadBundleAsyncOperation _operation, LoadSceneParameters _parameters) { }

        /// <summary>
        /// Called when the <see cref="SceneBundle"/> has been synchronously loaded.
        /// </summary>
        /// <param name="_bundle">The loaded bundle.</param>
        /// <param name="_parameters">Parameters used to load the bundle.</param>
        internal protected virtual void OnLoadBundle(SceneBundle _bundle, LoadSceneParameters _parameters) { }
        #endregion
    }
}
