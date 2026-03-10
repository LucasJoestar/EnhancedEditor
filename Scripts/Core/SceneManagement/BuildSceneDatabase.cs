// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define NON_BUILD_SCENES
#endif

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing informations about all scenes included in build.
    /// <para/> Should not be used directly.
    /// </summary>
    [NonEditable("This data is sensitive and should not be manipulated manually.")]
    public sealed class BuildSceneDatabase : ScriptableSettings {
        #region Non Build Scene
        [Serializable]
        internal struct NonBuildScene {
            public string Name;
            public string GUID;

            // -----------------------

            public NonBuildScene(string _name, string _guid) {
                Name = _name;
                GUID = _guid;
            }
        }
        #endregion

        #region Global Members
        private static BuildSceneDatabase database = null;

        #if UNITY_EDITOR
        /// <summary>
        /// Editor internal getter for the <see cref="BuildSceneDatabase"/> instance.
        /// <para/>
        /// As it needs to be set manually at runtime, it uses an internal getter when in editor mode
        /// to be safely able to load it from the database, even if the user deletes it.
        /// </summary>
        internal static Func<BuildSceneDatabase> EditorTagDatabaseGetter = null;
        #endif

        /// <summary>
        /// You have to set this reference at runtime to be properly able to use the Multi-Tags system.
        /// <br/>
        /// There are a variety of ways to assign its value:
        /// <list type="bullet">
        /// <item>by <see cref="ScriptableObject"/> reference</item>
        /// <item>using <see cref="Resources.Load(string)"/></item>
        /// <item><see cref="AssetBundle"/></item>
        /// <item>... or any other way you'd like.</item>
        /// </list><para/>
        /// </summary>
        public static BuildSceneDatabase Database {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (EditorTagDatabaseGetter != null)) {
                    return EditorTagDatabaseGetter();
                }

                if (database == null) {
                    Debug.LogError($"Unassigned {typeof(BuildSceneDatabase).Name} reference!\nYou must manually set this database " +
                                   $"reference on game start to be able to properly use {typeof(SceneAsset).Name}s and {typeof(SceneBundle).Name}s.");
                }
                #endif

                return database;
            }
            set {
                database = value;
            }
        }

        // -------------------------------------------
        // Database Content
        // -------------------------------------------

        [SerializeField] internal Pair<int, int>[] sceneIdentifiers = new Pair<int, int>[0];
        [SerializeField] internal SceneBundle[] sceneBundles        = new SceneBundle[0];
        [SerializeField] internal string[] buildSceneGUIDs          = new string[0];

        [SerializeField] internal int coreSceneIndex                = -1;

        #if NON_BUILD_SCENES
        [SerializeField] internal NonBuildScene[] nonBuildScenes    = new NonBuildScene[0];
        #endif
        #endregion

        #region Initialization
        internal protected override void Init() {
            Database = this;
        }
        #endregion

        #region Scene Bundle
        // -------------------------------------------
        // Bundle
        // -------------------------------------------

        /// <summary>
        /// Get the total count of <see cref="SceneBundle"/> in build.
        /// </summary>
        public int SceneBundleCount {
            get { return sceneBundles.Length; }
        }

        /// <summary>
        /// Get the <see cref="SceneBundle"/> at the given index.
        /// <para/>
        /// Use <see cref="SceneBundleCount"/> to get the total amount of bundles in build.
        /// </summary>
        /// <param name="_index">Index to get the associated <see cref="SceneBundle"/>.</param>
        /// <returns>The <see cref="SceneBundle"/> at the specified index.</returns>
        public SceneBundle GetSceneBundleAt(int _index) {
            return sceneBundles[_index];
        }

        /// <summary>
        /// Get the first <see cref="SceneBundle"/> that matches a given name.
        /// </summary>
        /// <param name="_name">Name of the <see cref="SceneBundle"/> to get.</param>
        /// <param name="_bundle">First found matching <see cref="SceneBundle"/> (null if none).</param>
        /// <returns>True if a matching <see cref="SceneBundle"/> could be found, false otherwise.</returns>
        public bool GetSceneBundle(string _name, out SceneBundle _bundle) {

            _name = _name.Replace(SceneBundle.Prefix, string.Empty);

            ref SceneBundle[] _span = ref sceneBundles;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _bundle = _span[i];

                if (_bundle.name.Replace(SceneBundle.Prefix, string.Empty).ContainsOrdinal(_name)) {
                    return true;
                }
            }

            _bundle = null;
            return false;
        }

        // -------------------------------------------
        // Helpers
        // -------------------------------------------

        /// <summary>
        /// Get the <see cref="SceneBundle"/> associated with a given <see cref="Scene"/>.
        /// </summary>
        /// <inheritdoc cref="GetSceneBundle(SceneAsset, out SceneBundle)"/>
        public bool GetSceneBundle(Scene _scene, out SceneBundle _bundle) {
            bool _valid = false;
            _bundle = null;

            ref SceneBundle[] _span = ref sceneBundles;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                SceneBundle _temp = _span[i];

                if (_temp.ContainScene(_scene)) {
                    _bundle = _temp;
                    _valid  = true;

                    if (_temp.Scenes.Length == 1) {
                        return true;
                    }
                }
            }

            return _valid;
        }

        /// <summary>
        /// Get the <see cref="SceneBundle"/> associated with all given <see cref="Scene"/>.
        /// </summary>
        /// <inheritdoc cref="GetSceneBundle(SceneAsset[], out SceneBundle)"/>
        public bool GetSceneBundle(Scene[] _scenes, out SceneBundle _bundle) {
            bool _valid = false;
            _bundle = null;

            ref SceneBundle[] _span = ref sceneBundles;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                SceneBundle _temp = _span[i];

                if (_temp.ContainScenes(_scenes)) {
                    _bundle = _temp;
                    _valid  = true;

                    if (_temp.Scenes.Length == _scenes.Length) {
                        return true;
                    }
                }
            }

            return _valid;
        }

        /// <summary>
        /// Get the <see cref="SceneBundle"/> associated with a given <see cref="SceneAsset"/>.
        /// </summary>
        /// <param name="_scene">The scene to get the associated bundle.</param>
        /// <param name="_bundle">The bundle associated with the given scene (null if none).</param>
        /// <returns>True if an associated <see cref="SceneBundle"/> could be found, false otherwise.</returns>
        public bool GetSceneBundle(SceneAsset _scene, out SceneBundle _bundle) {
            bool _valid = false;
            _bundle = null;

            ref SceneBundle[] _span = ref sceneBundles;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                SceneBundle _temp = _span[i];

                if (_temp.ContainScene(_scene)) {
                    _bundle = _temp;
                    _valid  = true;

                    if (_temp.Scenes.Length == 1) {
                        return true;
                    }
                }
            }

            return _valid;
        }

        /// <summary>
        /// Get the <see cref="SceneBundle"/> associated with all given <see cref="SceneAsset"/>.
        /// </summary>
        /// <param name="_scenes">The scenes to get the associated bundle.</param>
        /// <param name="_bundle">The bundle associated with the given scenes (null if none).</param>
        /// <returns>True if an associated <see cref="SceneBundle"/> could be found, false otherwise.</returns>
        public bool GetSceneBundle(SceneAsset[] _scenes, out SceneBundle _bundle) {
            bool _valid = false;
            _bundle = null;

            ref SceneBundle[] _span = ref sceneBundles;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                SceneBundle _temp = _span[i];

                if (_temp.ContainScenes(_scenes)) {
                    _bundle = _temp;
                    _valid  = true;

                    if (_temp.Scenes.Length == _scenes.Length) {
                        return true;
                    }
                }
            }

            return _valid;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the <see cref="Scene"/> build index associated with a given identifier.
        /// </summary>
        /// <param name="_identifier">Identifier to get the associated <see cref="Scene"/>.</param>
        /// <param name="_sceneBuildIndex">Build index of the <see cref="Scene"/> associated with the given identifier (-1 if none).</param>
        /// <returns>True if an associated <see cref="Scene"/> could be found, false otherwise.</returns>
        public static bool GetSceneFromIdentifier(int _identifier, out int _sceneBuildIndex) {
            ref var _ids = ref Database.sceneIdentifiers;

            for (int i = _ids.Length; i-- > 0;) {
                var _pair = _ids[i];

                if (_pair.Second == _identifier) {
                    _sceneBuildIndex = _pair.First;
                    return true;
                }
            }

            _sceneBuildIndex = -1;
            return true;
        }

        /// <summary>
        /// Get the <see cref="Scene"/> associated with a given identifier.
        /// </summary>
        /// <param name="_scene"><see cref="Scene"/> associated with the given identifier (default if none).</param>
        /// <inheritdoc cref="GetSceneFromIdentifier(int, out int)"/>
        public static bool GetSceneFromIdentifier(int _identifier, out Scene _scene) {
            if (GetSceneFromIdentifier(_identifier, out int _buildIndex)) {
                _scene = SceneManager.GetSceneByBuildIndex(_buildIndex);
                return true;
            }

            _scene = default;
            return false;
        }

        /// <summary>
        /// Get a stable identifier for a given <see cref="Scene"/>.
        /// </summary>
        /// <param name="_scene"><see cref="Scene"/> to get the associated identifier.</param>
        /// <returns>Identifier for the given scene.</returns>
        public static int GetSceneIdentifier(Scene _scene) {
            int _buildIndex = _scene.buildIndex;
            return GetSceneIdentifier(_buildIndex);
        }

        /// <summary>
        /// Get a stable identifier for a given scene build index.
        /// </summary>
        /// <param name="_sceneBuildIndex">Scene build index to get the associated identifier.</param>
        /// <returns>Identifier for the given scene build index.</returns>
        public static int GetSceneIdentifier(int _sceneBuildIndex) {
            ref var _ids = ref Database.sceneIdentifiers;

            for (int i = _ids.Length; i-- > 0;) {
                var _pair = _ids[i];

                if (_pair.First == _sceneBuildIndex)
                    return _pair.Second;
            }

            return 0;
        }

        /// <summary>
        /// Get the build index of a specific scene from its GUID.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the scene to get associated build index.</param>
        /// <returns>Build index of the scene if it was included in build, -1 otherwise.</returns>
        public static int GetSceneBuildIndex(string _sceneGUID) {
            return Array.IndexOf(Database.buildSceneGUIDs, _sceneGUID);
        }

        /// <summary>
        /// Internal method used to debug the name of a non included scene in build that is trying to be loaded or unlaoded.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the non included in build scene.</param>
        /// <returns>Name of this scene if in editor or in a development build, and an empty string otherwise.</returns>
        internal static string GetNonBuildSceneName(string _sceneGUID) {
            #if NON_BUILD_SCENES
            ref NonBuildScene[] _span = ref Database.nonBuildScenes;
            for (int i = _span.Length; i-- > 0;) {

                NonBuildScene _scene = _span[i];
                if (_scene.GUID.EqualOrdinal(_sceneGUID)) {
                    return _scene.Name;
                }
            }
            #endif

            return string.Empty;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Is this scene the core game scene?
        /// </summary>
        /// <param name="_scene">Scene to check.</param>
        /// <returns>True if this is the core scene, false otherwise.</returns>
        public bool IsCoreScene(Scene _scene) {
            return _scene.buildIndex == coreSceneIndex;
        }

        /// <summary>
        /// Setups this database (called from editor script).
        /// </summary>
        internal void Setup(SceneBundle[] _sceneBundles, string[] _sceneGUIDS, NonBuildScene[] _nonBuildScenes, int _coreSceneIndex) {
            // Ensure that each bundle has a unique GUID.
            for (int i = _sceneBundles.Length; i-- > 0;) {
                SceneBundle _bundle = _sceneBundles[i];

                for (int j = 0; j < i; j++) {
                    if (_bundle.GUID == _sceneBundles[j].GUID) {
                        _bundle.RegenerateGUID();
                    }
                }
            }

            Array.Resize(ref sceneIdentifiers, _sceneGUIDS.Length);
            for (int i = _sceneGUIDS.Length; i-- > 0;) {
                sceneIdentifiers[i] = new Pair<int, int>(i, _sceneGUIDS[i].GetStableHashCode());
            }

            sceneBundles    = _sceneBundles;
            buildSceneGUIDs = _sceneGUIDS;

            #if NON_BUILD_SCENES
            nonBuildScenes = _nonBuildScenes;
            #endif

            coreSceneIndex = _coreSceneIndex;
        }
        #endregion
    }
}
