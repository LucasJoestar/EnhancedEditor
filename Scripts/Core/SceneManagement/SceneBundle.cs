// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor {
    /// <summary>
    /// Default empty <see cref="SceneBundle"/> behaviour.
    /// </summary>
    [Serializable, DisplayName("<None>")]
    public class DefaultSceneBundleBehaviour : SceneBundleBehaviour { }

    /// <summary>
    /// <see cref="ScriptableObject"/> used to group multiple scenes in a single bundle,
    /// which can then be used to easily load and unload these scenes together at once.
    /// </summary>
    #pragma warning disable IDE0052
    [CreateAssetMenu(fileName = Prefix + "SceneBundle", menuName = InternalUtility.MenuPath + "Scene Bundle", order = InternalUtility.MenuOrder)]
    public class SceneBundle : ScriptableObject {
        public const string Prefix = "SCB_";

        #region Global Members
        [Section("Scene Bundle")]

        /// <summary>
        /// All scenes included in this bundle.
        /// </summary>
        public SceneAsset[] Scenes = new SceneAsset[] { };

        [Space(5f), HelpBox("Index of the scene to set active once loaded. Use -1 to leave it as it is.", MessageType.Info, false)]
        [SerializeField, Enhanced, ValidationMember("ActiveSceneIndex")] private int activeSceneIndex = 0;

        #if UNITY_EDITOR
        [SerializeField, Space(5f), Enhanced, EnhancedTextArea(true)] private string comment = string.Empty;
        #endif

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField] public PolymorphValue<SceneBundleBehaviour> Behaviour = new PolymorphValue<SceneBundleBehaviour>(SerializedTypeConstraint.None,
                                                                                                                          typeof(DefaultSceneBundleBehaviour));

        /// <summary>
        /// An empty class only displays its name in the inspector.
        /// <br/> To avoid it, only draw the class content if non-default type.
        /// </summary>
        #pragma warning disable IDE0051
        private bool ShowBehaviour {
            get { return Behaviour.GetType() != typeof(DefaultSceneBundleBehaviour); }
        }

        // -----------------------

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
            UnloadBundleAsyncOperation _operation = (Scenes.Length != 0) 
                                                  ? new UnloadBundleAsyncOperation(this, _options)
                                                  : new UnloadBundleAsyncOperation();
            
            Behaviour.Value.OnUnloadAsyncBundle(this, _operation, _options);
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
            LoadBundleAsyncOperation _operation = (Scenes.Length != 0)
                                                ? new LoadBundleAsyncOperation(this, _parameters)
                                                : new LoadBundleAsyncOperation();

            Behaviour.Value.OnLoadAsyncBundle(this, _operation, _parameters);
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

            Behaviour.Value.OnLoadBundle(this, _parameters);

            // ----- Local Method ----- \\

            void LoadScene(int _index) {
                Scene _scene = Scenes[_index].Load(_parameters);

                if (activeSceneIndex == _index) {
                    SceneManager.SetActiveScene(_scene);
                }
            }
        }
        #endregion

        #region Contain
        // -------------------------------------------
        // Scene
        // -------------------------------------------

        /// <inheritdoc cref="ContainScene(Scene, out SceneAsset)"/>
        public bool ContainScene(Scene _scene) {
            return ContainScene(_scene, out _);
        }

        /// <summary>
        /// Get if this <see cref="SceneBundle"/> contains a specific <see cref="Scene"/>.
        /// </summary>
        /// <param name="_scene">The <see cref="Scene"/> to check.</param>
        /// <param name="_asset">The matching <see cref="Scene"/> <see cref="SceneAsset"/>.</param>
        /// <returns>True if this bundle contains the given <see cref="Scene"/>, false otherwise.</returns>
        public bool ContainScene(Scene _scene, out SceneAsset _asset) {
            foreach (SceneAsset _temp in Scenes) {
                if (_temp.Equals(_scene)) {
                    _asset = _temp;
                    return true;
                }
            }

            _asset = null;
            return false;
        }

        /// <summary>
        /// Get if all the specified scenes are contained in this bundle.
        /// </summary>
        /// <param name="_scenes">All scenes to check.</param>
        /// <returns>True if all the given scenes are in this bundle, false otherwise.</returns>
        public bool ContainScenes(Scene[] _scenes) {
            foreach (Scene _scene in _scenes) {
                if (!ContainScene(_scene)) {
                    return false;
                }
            }

            return true;
        }

        // -------------------------------------------
        // Scene Asset
        // -------------------------------------------

        /// <inheritdoc cref="ContainScene(SceneAsset, out SceneAsset)"/>
        public bool ContainScene(SceneAsset _scene) {
            return ContainScene(_scene, out _);
        }

        /// <summary>
        /// Get if this <see cref="SceneBundle"/> contains a specific <see cref="SceneAsset"/>.
        /// </summary>
        /// <param name="_scene">The <see cref="SceneAsset"/> to check.</param>
        /// <param name="_asset">The matching <see cref="SceneAsset"/> <see cref="SceneAsset"/>.</param>
        /// <returns>True if this bundle contains the given <see cref="SceneAsset"/>, false otherwise.</returns>
        public bool ContainScene(SceneAsset _scene, out SceneAsset _asset) {
            foreach (SceneAsset _temp in Scenes) {
                if (_temp == _scene) {
                    _asset = _temp;
                    return true;
                }
            }

            _asset = null;
            return false;
        }

        /// <summary>
        /// Get if all the specified scenes are contained in this bundle.
        /// </summary>
        /// <param name="_scenes">All scenes to check.</param>
        /// <returns>True if all the given scenes are in this bundle, false otherwise.</returns>
        public bool ContainScenes(SceneAsset[] _scenes) {
            foreach (SceneAsset _scene in _scenes) {
                if (!ContainScene(_scene)) {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if this <see cref="SceneBundle"/> is the core scene bundle.
        /// </summary>
        /// <returns>True if this bundle only contain the core scene, false otherwise.</returns>
        public bool IsCoreBundle() {
            return (Scenes.Length == 1) && Scenes[0].IsCoreScene;
        }

        /// <summary>
        /// Get this bundle behaviour.
        /// </summary>
        /// <typeparam name="T"><see cref="SceneBundleBehaviour"/> to cast this bundle behaviour to.</typeparam>
        /// <param name="_behaviour">The casted behaviour of this bundle.</param>
        /// <returns>True if this bundle behaviour could be successfully casted into the given type, false otherwise.</returns>
        public bool GetBehaviour<T>(out T _behaviour) where T : SceneBundleBehaviour {
            return Behaviour.GetValue(out _behaviour);
        }

        // -------------------------------------------
        // Getter
        // -------------------------------------------

        /// <inheritdoc cref="BuildSceneDatabase.GetSceneBundle(Scene, out SceneBundle)"/>
        public static bool GetSceneBundle(Scene _scene, out SceneBundle _bundle) {
            return BuildSceneDatabase.Database.GetSceneBundle(_scene, out _bundle);
        }

        /// <inheritdoc cref="BuildSceneDatabase.GetSceneBundle(Scene[], out SceneBundle)"/>
        public static bool GetSceneBundle(Scene[] _scenes, out SceneBundle _bundle) {
            return BuildSceneDatabase.Database.GetSceneBundle(_scenes, out _bundle);
        }

        /// <inheritdoc cref="BuildSceneDatabase.GetSceneBundle(SceneAsset, out SceneBundle)"/>
        public static bool GetSceneBundle(SceneAsset _scene, out SceneBundle _bundle) {
            return BuildSceneDatabase.Database.GetSceneBundle(_scene, out _bundle);
        }

        /// <inheritdoc cref="BuildSceneDatabase.GetSceneBundle(SceneAsset[], out SceneBundle)"/>
        public static bool GetSceneBundle(SceneAsset[] _scenes, out SceneBundle _bundle) {
            return BuildSceneDatabase.Database.GetSceneBundle(_scenes, out _bundle);
        }
        #endregion
    }
}
