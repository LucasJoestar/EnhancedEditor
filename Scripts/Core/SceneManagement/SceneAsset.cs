// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
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
	public class SceneAsset
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
                int _buildIndex = BuildIndex;

                // Negative index means that the scene was not included in build.
                if (_buildIndex == -1)
                    return string.Empty;

                Scene _scene = SceneManager.GetSceneByBuildIndex(_buildIndex);
                return _scene.name;
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

        #region Load
        /// <summary>
        /// Loads this scene for the next frame.
        /// </summary>
        /// <returns>A handle to the scene being loaded.</returns>
        public Scene Load()
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);
            return Load(_parameters);
        }

        /// <param name="_mode">Allows you to specify whether or not to load this scene additively.</param>
        /// <inheritdoc cref="Load()"/>
        public Scene Load(LoadSceneMode _mode)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return Load(_parameters);
        }

        /// <param name="_parameters">Various parameters used to load this scene.</param>
        /// <inheritdoc cref="Load()"/>
        public Scene Load(LoadSceneParameters _parameters)
        {
            if (!IsValid())
                return default;

            return SceneManager.LoadScene(buildIndex, _parameters);
        }
        #endregion

        #region Load Async
        /// <inheritdoc cref="LoadAsync(LoadSceneParameters)"/>
        public AsyncOperation LoadAsync()
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);
            return LoadAsync(_parameters);
        }

        /// <param name="_mode"><inheritdoc cref="Load(LoadSceneMode)" path="/param[@name='_mode']"/></param>
        /// <inheritdoc cref="LoadAsync(LoadSceneParameters)"/>
        public AsyncOperation LoadAsync(LoadSceneMode _mode)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return LoadAsync(_parameters);
        }

        /// <summary>
        /// Loads this scene asynchronously in the background.
        /// </summary>
        /// <param name="_parameters"><inheritdoc cref="Load(LoadSceneParameters)" path="/param[@name='_parameters']"/></param>
        /// <returns><see cref="AsyncOperation"/> used to determine when the operation has completed.</returns>
        public AsyncOperation LoadAsync(LoadSceneParameters _parameters)
        {
            LoadAsync(_parameters, out AsyncOperation _operation);
            return _operation;
        }

        /// <inheritdoc cref="LoadAsync(LoadSceneParameters, out AsyncOperation)"/>
        public bool LoadAsync(out AsyncOperation _operation)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);
            return LoadAsync(_parameters, out _operation);
        }

        /// <inheritdoc cref="LoadAsync(LoadSceneParameters, out AsyncOperation)"/>
        public bool LoadAsync(LoadSceneMode _mode, out AsyncOperation _operation)
        {
            LoadSceneParameters _parameters = new LoadSceneParameters(_mode, LocalPhysicsMode.None);
            return LoadAsync(_parameters, out _operation);
        }

        /// <param name="_operation"><inheritdoc cref="LoadAsync(LoadSceneParameters)" path="/returns"/></param>
        /// <returns>True if this scene could be found and is now being loaded, false otherwise.</returns>
        /// <inheritdoc cref="LoadAsync(LoadSceneParameters)"/>
        public bool LoadAsync(LoadSceneParameters _parameters, out AsyncOperation _operation)
        {
            if (!IsValid())
            {
                _operation = null;
                return false;
            }

            _operation = SceneManager.LoadSceneAsync(BuildIndex, _parameters);
            return true;
        }
        #endregion

        #region Unload Async
        /// <inheritdoc cref="UnloadAsync(UnloadSceneOptions)"/>
        public AsyncOperation UnloadAsync()
        {
            UnloadSceneOptions _options = UnloadSceneOptions.None;
            return UnloadAsync(_options);
        }

        /// <summary>
        /// Destroys all <see cref="GameObject"/> associated with this scene and remove it from the <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="_options">Scene unloading options.</param>
        /// <returns><inheritdoc cref="LoadAsync()" path="/returns"/></returns>
        public AsyncOperation UnloadAsync(UnloadSceneOptions _options)
        {
            UnloadAsync(_options, out AsyncOperation _operation);
            return _operation;
        }

        /// <inheritdoc cref="UnloadAsync(UnloadSceneOptions, out AsyncOperation)"/>
        public bool UnloadAsync(out AsyncOperation _operation)
        {
            UnloadSceneOptions _options = UnloadSceneOptions.None;
            return UnloadAsync(_options, out _operation);
        }

        /// <param name="_operation"><inheritdoc cref="UnloadAsync(UnloadSceneOptions)" path="/returns"/></param>
        /// <returns>True if this scene could be found and is now being unloaded, false otherwise.</returns>
        /// <inheritdoc cref="UnloadAsync(UnloadSceneOptions)"/>
        public bool UnloadAsync(UnloadSceneOptions _options, out AsyncOperation _operation)
        {
            if (!IsValid())
            {
                _operation = null;
                return false;
            }

            _operation = SceneManager.UnloadSceneAsync(BuildIndex, _options);
            return true;
        }
        #endregion

        #region Utility
        private bool IsValid()
        {
            if (BuildIndex == -1)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogException(new NonBuildSceneException(BuildSceneDatabase.GetNonBuildSceneName(guid)), BuildSceneDatabase.Database);
                #endif

                return false;
            }

            return true;
        }
        #endregion
    }
}
