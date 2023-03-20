// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor
{
    /// <summary>
    /// Exception raised when trying to load or unload a scene that was not included in build.
    /// </summary>
    [Serializable]
    public class NonBuildSceneException : Exception
    {
        #region Global Members
        public const string MessageFormat = "Scene {0} was not included in build and cannot be loaded or unloaded";

        // -----------------------

        /// <inheritdoc cref="NonBuildSceneException(string, Exception)"/>
        public NonBuildSceneException() : base(string.Format(MessageFormat, string.Empty)) { }

        /// <inheritdoc cref="NonBuildSceneException(string, Exception)"/>
        public NonBuildSceneException(string _sceneName) : base(string.Format(MessageFormat, _sceneName)) { }

        /// <param name="_sceneName">Name of the scene that was not included in build.</param>
        /// <inheritdoc cref="NonBuildSceneException"/>
        public NonBuildSceneException(string _sceneName, Exception _innerException) : base(string.Format(MessageFormat, _sceneName), _innerException) { }
        #endregion
    }

    /// <summary>
    /// Serializable class used to reference a scene in the project.
    /// <br/> Only works with scenes included in build.
    /// <para/>
    /// You can manipulate them using their <see cref="BuildIndex"/>,
    /// or with the help of the multiple loading / unloading methods.
    /// </summary>
    [Serializable]
	public class SceneAsset : IEquatable<SceneAsset>, IEquatable<Scene>
    {
        #region Global Members
        [SerializeField] internal string guid = string.Empty;
        [NonSerialized] private int buildIndex = -2;

        /// <summary>
        /// The GUID of this scene asset in the project.
        /// </summary>
        public string GUID => guid;

        /// <summary>
        /// Get the index of this scene in the Build Settings.
        /// <br/> Returns -1 if this scene was not included in build.
        /// </summary>
        public int BuildIndex
        {
            get
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    string _path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    int _index = 0;

                    foreach (var _scene in UnityEditor.EditorBuildSettings.scenes)
                    {
                        if (!_scene.enabled)
                            continue;

                        if (_scene.path == _path)
                            return _index;

                        _index++;
                    }

                    return -1;
                }
                #endif

                if (buildIndex == -2)
                    buildIndex = BuildSceneDatabase.GetSceneBuildIndex(guid);

                return buildIndex;
            }
        }

        /// <summary>
        /// Get the name of this scene.
        /// <br/> Returns an empty string if this scene was not included in build.
        /// </summary>
        public string Name
        {
            get
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    string _path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    return Path.GetFileNameWithoutExtension(_path);
                }
                #endif

                int _buildIndex = BuildIndex;

                // Negative index means that the scene was not included in build.
                if (_buildIndex == -1)
                    return string.Empty;

                Scene _scene = SceneManager.GetSceneByBuildIndex(_buildIndex);
                return _scene.name;
            }
        }

        /// <summary>
        /// Get the <see cref="UnityEngine.SceneManagement.Scene"/> info struct associated with this asset.
        /// </summary>
        public Scene Scene {
            get {
                return SceneManager.GetSceneByBuildIndex(BuildIndex);
            }
        }

        // -----------------------

        /// <inheritdoc cref="SceneAsset(string)"/>
        public SceneAsset() { }

        /// <inheritdoc cref="SceneAsset"/>
        /// <param name="_guid"><inheritdoc cref="GUID" path="/summary"/></param>
        public SceneAsset(string _guid)
        {
            guid = _guid;
        }
        #endregion

        #region Operator
        public static implicit operator Scene(SceneAsset _scene) {
            return SceneManager.GetSceneByBuildIndex(_scene.BuildIndex);
        }

        public static bool operator ==(SceneAsset a, SceneAsset b) {
            if (!ReferenceEquals(a, null)) {
                return a.Equals(b);
            }

            return ReferenceEquals(b, null);
        }

        public static bool operator !=(SceneAsset a, SceneAsset b) {
            return !(a == b);
        }

        public override bool Equals(object _object) {
            if (_object != null) {
                if (_object is SceneAsset _sceneAsset) {
                    return Equals(_sceneAsset);
                }

                if (_object is Scene _scene) {
                    return Equals(_scene);
                }
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Unload Async
        /// <summary>
        /// Destroys all <see cref="GameObject"/> associated with this scene and remove it from the <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="_operation"><inheritdoc cref="LoadAsync(LoadSceneParameters, out AsyncOperation)" path="/param[@name='_operation']"/></param>
        /// <param name="_options">Scene unloading options.</param>
        /// <returns>True if this scene could be found and is now being unloaded, false otherwise.</returns>
        public bool UnloadAsync(out AsyncOperation _operation, UnloadSceneOptions _options = UnloadSceneOptions.None) {
            if (!IsValid || (SceneManager.sceneCount == 1)) {
                _operation = null;
                return false;
            }

            _operation = SceneManager.UnloadSceneAsync(BuildIndex, _options);
            return true;
        }
        #endregion

        #region Load Async
        /// <param name="_mode"><inheritdoc cref="Load(LoadSceneMode)" path="/param[@name='_mode']"/></param>
        /// <inheritdoc cref="LoadAsync(out AsyncOperation, LoadSceneParameters)"/>
        public bool LoadAsync(out AsyncOperation _operation, LoadSceneMode _mode = LoadSceneMode.Additive) {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return LoadAsync(out _operation, _parameters);
        }

        /// <summary>
        /// Loads this scene asynchronously in the background.
        /// </summary>
        /// <param name="_operation"><see cref="AsyncOperation"/> used to determine when the operation is complete.</param>
        /// <param name="_parameters"><inheritdoc cref="Load(LoadSceneParameters)" path="/param[@name='_parameters']"/></param>
        /// <returns>True if this scene could be found and is now being loaded, false otherwise.</returns>
        public bool LoadAsync(out AsyncOperation _operation, LoadSceneParameters _parameters) {
            if (!IsValid || !CanBeLoaded(_parameters.loadSceneMode)) {
                _operation = null;
                return false;
            }

            _operation = SceneManager.LoadSceneAsync(BuildIndex, _parameters);
            return true;
        }
        #endregion

        #region Load
        /// <summary>
        /// Loads this scene for the next frame.
        /// </summary>
        /// <param name="_mode">Allows you to specify whether or not to load this scene additively.</param>
        /// <returns>A handle to the scene being loaded.</returns>
        public Scene Load(LoadSceneMode _mode = LoadSceneMode.Single)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return Load(_parameters);
        }

        /// <param name="_parameters">Various parameters used to load this scene.</param>
        /// <inheritdoc cref="Load(LoadSceneMode)"/>
        public Scene Load(LoadSceneParameters _parameters)
        {
            if (!IsValid || !CanBeLoaded(_parameters.loadSceneMode)) {
                return default;
            }

            return SceneManager.LoadScene(BuildIndex, _parameters);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Is this scene the core scene?
        /// </summary>
        public bool IsCoreScene {
            get {
                return BuildIndex == BuildSceneDatabase.Database.coreSceneIndex;
            }
        }

        /// <summary>
        /// Is this scene currently loaded?
        /// </summary>
        public bool IsLoaded {
            get {
                return IsValid
                     ? SceneManager.GetSceneByBuildIndex(BuildIndex).isLoaded
                     : false;
            }
        }

        /// <summary>
        /// Is this scene a 'valid' scene? If not, then it should not be used.
        /// </summary>
        public bool IsValid {
            get {
                if (BuildIndex == -1) {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    //Debug.LogException(new NonBuildSceneException(BuildSceneDatabase.GetNonBuildSceneName(guid)), BuildSceneDatabase.Database);
                    #endif

                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Used to make sure a scene cannot be loaded twice (don't add it if it is already loaded).
        /// </summary>
        /// <param name="_mode">Mode used to load the scene.</param>
        /// <returns>True if the scene can be loaded, false otherwise.</returns>
        public bool CanBeLoaded(LoadSceneMode _mode) {
            return !IsLoaded || (_mode == LoadSceneMode.Single);
        }

        /// <summary>
        /// Get if this <see cref="SceneAsset"/> is wrapped around the same scene as another <see cref="SceneAsset"/>.
        /// </summary>
        /// <param name="_scene">The <see cref="SceneAsset"/> to check.</param>
        /// <returns>True if this <see cref="SceneAsset"/> matches the given scene, false otherwise.</returns>
        public bool Equals(SceneAsset _scene) {
            return !ReferenceEquals(_scene, null) && guid.Equals(_scene.guid, StringComparison.Ordinal);
        }

        /// <summary>
        /// Get if this <see cref="SceneAsset"/> is wrapped around a specific <see cref="Scene"/>.
        /// </summary>
        /// <param name="_scene">The <see cref="Scene"/> to check.</param>
        /// <returns>True if this <see cref="SceneAsset"/> matches the given scene, false otherwise.</returns>
        public bool Equals(Scene _scene) {
            return BuildIndex == _scene.buildIndex;
        }
        #endregion
    }
}
