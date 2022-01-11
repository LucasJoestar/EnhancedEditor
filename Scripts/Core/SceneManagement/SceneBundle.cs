// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> used to group multiple scenes in a single bundle,
    /// which can then be used to easily load and unload these scenes together at once.
    /// </summary>
    #pragma warning disable IDE0052
    [CreateAssetMenu(fileName = "NewSceneBundle", menuName = "Enhanced Editor/Scene Bundle", order = 190)]
    public class SceneBundle : ScriptableObject
    {
        #region Global Members
        /// <summary>
        /// All scenes included in this bundle.
        /// </summary>
        public SceneAsset[] Scenes = new SceneAsset[] { };

        #if UNITY_EDITOR
        [SerializeField, Space(5f), Enhanced, EnhancedTextArea(true)] private string comment = string.Empty;
        #endif
        #endregion

        #region Load
        /// <inheritdoc cref="Load(LoadSceneParameters)"/>
        public void Load()
        {
            LoadSceneMode _mode = LoadSceneMode.Single;
            Load(_mode);
        }

        /// <param name="_mode">Allows you to specify whether or not to load these scenes additively.</param>
        /// <inheritdoc cref="Load(LoadSceneParameters)"/>
        public void Load(LoadSceneMode _mode)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            Load(_parameters);
        }

        /// <summary>
        /// Loads all scenes in this bundle for the next frame.
        /// </summary>
        /// <param name="_parameters">Various parameters used to load these scenes.</param>
        public void Load(LoadSceneParameters _parameters)
        {
            if (Scenes.Length == 0)
                return;

            Scenes[0].Load(_parameters);
            _parameters.loadSceneMode = LoadSceneMode.Additive;

            for (int _i = 1; _i < Scenes.Length; _i++)
            {
                SceneAsset _scene = Scenes[_i];
                _scene.Load(_parameters);
            }
        }
        #endregion

        #region Load Async
        /// <summary>
        /// Loads this scene asynchronously in the background.
        /// </summary>
        /// <param name="_setFirstSceneActive">Determines whether the first scene in this bundle
        /// should be set active once loaded or not.</param>
        /// <returns><see cref="LoadBundleAsyncOperation"/> used to determine when the operation has completed.</returns>
        public LoadBundleAsyncOperation LoadAsync(bool _setFirstSceneActive = false)
        {
            LoadSceneMode _mode = LoadSceneMode.Single;
            return LoadAsync(_mode, _setFirstSceneActive);
        }

        /// <param name="_mode"><inheritdoc cref="Load(LoadSceneMode)" path="/param[@name='_mode']"/></param>
        /// <inheritdoc cref="LoadAsync(bool)"/>
        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop = false)]
        public LoadBundleAsyncOperation LoadAsync(LoadSceneMode _mode, bool _setFirstSceneActive = false)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return LoadAsync(_parameters, _setFirstSceneActive);
        }

        /// <param name="_parameters"><inheritdoc cref="Load(LoadSceneParameters)" path="/param[@name='_parameters']"/></param>
        /// <inheritdoc cref="LoadAsync(bool)"/>
        public LoadBundleAsyncOperation LoadAsync(LoadSceneParameters _parameters, bool _setFirstSceneActive = false)
        {
            if (Scenes.Length == 0)
                return new LoadBundleAsyncOperation();

            LoadBundleAsyncOperation _operation = new LoadBundleAsyncOperation(this, _parameters, _setFirstSceneActive);
            return _operation;
        }
        #endregion

        #region Unload Async
        /// <summary>
        /// Destroys all <see cref="GameObject"/> associated with this scene and remove it from the <see cref="SceneManager"/>.
        /// </summary>
        /// <returns><see cref="UnloadBundleAsyncOperation"/> used to determine when the operation has completed.</returns>
        public UnloadBundleAsyncOperation UnloadAsync()
        {
            UnloadSceneOptions _options = UnloadSceneOptions.None;
            return UnloadAsync(_options);
        }

        /// <param name="_options">Scene unloading options.</param>
        /// <inheritdoc cref="UnloadAsync()"/>
        [Button(ActivationMode.Play, SuperColor.Crimson, IsDrawnOnTop = false)]
        public UnloadBundleAsyncOperation UnloadAsync(UnloadSceneOptions _options)
        {
            if (Scenes.Length == 0)
                return new UnloadBundleAsyncOperation();

            UnloadBundleAsyncOperation _operation = new UnloadBundleAsyncOperation(this, _options);
            return _operation;
        }
        #endregion
    }
}
