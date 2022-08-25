// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  • Custom editor to add content scenes into the build.
//
// ============================================================================ //

using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> used to group multiple scenes in a single bundle,
    /// which can then be used to easily load and unload these scenes together at once.
    /// </summary>
    #pragma warning disable IDE0052
    [CreateAssetMenu(fileName = "NewSceneBundle", menuName = "Enhanced Editor/Scene Bundle", order = 190)]
    public class SceneBundle : ScriptableObject {
        #region Global Members
        /// <summary>
        /// All scenes included in this bundle.
        /// </summary>
        public SceneAsset[] Scenes = new SceneAsset[] { };

        [Space(5f), HelpBox("Index of the scene to set active once loaded. Use -1 to leave it as it is.", MessageType.Info)]
        [SerializeField, Enhanced, ValidationMember("ActiveSceneIndex")] private int activeSceneIndex = 0;

        #if UNITY_EDITOR
        [SerializeField, Space(5f), Enhanced, EnhancedTextArea(true)] private string comment = string.Empty;
        #endif

        /// <summary>
        /// Index of the scene to set active once loaded.
        /// <br/> Use -1 to leave it as it is.
        /// </summary>
        public int ActiveSceneIndex {
            get {
                return activeSceneIndex;
            } set {
                activeSceneIndex = Mathf.Clamp(value, -1, Scenes.Length - 1);
            }
        }

        /// <summary>
        /// Is this bundle currently loaded?
        /// </summary>
        public bool IsLoaded {
            get {
                foreach (SceneAsset _scene in Scenes) {
                    if (!_scene.IsLoaded) {
                        return false;
                    }
                }

                return true;
            }
        }
        #endregion

        #region Unload Async
        /// <summary>
        /// Destroys all <see cref="GameObject"/> associated with this bundle and remove it from the <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="_options">Scene unloading options.</param>
        /// <returns><see cref="UnloadBundleAsyncOperation"/> used to determine when the operation has completed.</returns>
        [Button(ActivationMode.Play, SuperColor.Crimson, IsDrawnOnTop = false)]
        public UnloadBundleAsyncOperation UnloadAsync(UnloadSceneOptions _options = UnloadSceneOptions.None) {
            if (Scenes.Length == 0)
                return new UnloadBundleAsyncOperation();

            UnloadBundleAsyncOperation _operation = new UnloadBundleAsyncOperation(this, _options);
            return _operation;
        }
        #endregion

        #region Load Async
        /// <param name="_mode"><inheritdoc cref="Load(LoadSceneMode)" path="/param[@name='_mode']"/></param>
        /// <inheritdoc cref="LoadAsync(LoadSceneParameters)"/>
        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop = false)]
        public LoadBundleAsyncOperation LoadAsync(LoadSceneMode _mode = LoadSceneMode.Single) {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return LoadAsync(_parameters);
        }

        /// <summary>
        /// Loads this bundle asynchronously in the background.
        /// </summary>
        /// <param name="_parameters"><inheritdoc cref="Load(LoadSceneParameters)" path="/param[@name='_parameters']"/></param>
        /// <returns><see cref="LoadBundleAsyncOperation"/> used to determine when the operation has completed.</returns>
        public LoadBundleAsyncOperation LoadAsync(LoadSceneParameters _parameters) {
            if (Scenes.Length == 0)
                return new LoadBundleAsyncOperation();

            LoadBundleAsyncOperation _operation = new LoadBundleAsyncOperation(this, _parameters);
            return _operation;
        }
        #endregion

        #region Load
        /// <param name="_mode">Allows you to specify whether or not to load these scenes additively.</param>
        /// <inheritdoc cref="Load(LoadSceneParameters)"/>
        public void Load(LoadSceneMode _mode = LoadSceneMode.Single) {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            Load(_parameters);
        }

        /// <summary>
        /// Loads all scenes in this bundle for the next frame.
        /// </summary>
        /// <param name="_parameters">Various parameters used to load these scenes.</param>
        public void Load(LoadSceneParameters _parameters) {
            if (Scenes.Length == 0)
                return;

            LoadScene(0);
            _parameters.loadSceneMode = LoadSceneMode.Additive;

            for (int _i = 1; _i < Scenes.Length; _i++) {
                LoadScene(_i);
            }

            // ----- Local Method ----- \\

            void LoadScene(int _index) {
                Scene _scene = Scenes[_index].Load(_parameters);

                if (activeSceneIndex == _index) {
                    SceneManager.SetActiveScene(_scene);
                }
            }
        }
        #endregion
    }
}
