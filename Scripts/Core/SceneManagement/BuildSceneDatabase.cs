// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

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
    public class BuildSceneDatabase : ScriptableObject {
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

                    database = CreateInstance<BuildSceneDatabase>();
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

        [SerializeField] internal SceneBundle[] sceneBundles        = new SceneBundle[0];
        [SerializeField] internal string[] buildSceneGUIDs          = new string[0];

        [SerializeField] internal int coreSceneIndex                = -1;

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] internal NonBuildScene[] nonBuildScenes    = new NonBuildScene[0];
        #endif
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

            foreach (SceneBundle _temp in sceneBundles) {
                if (_temp.name.Replace(SceneBundle.Prefix, string.Empty).Equals(_name, StringComparison.Ordinal)) {
                    _bundle = _temp;
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

            foreach (SceneBundle _temp in sceneBundles) {
                if (_temp.ContainScene(_scene)) {
                    _bundle = _temp;
                    _valid = true;

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

            foreach (SceneBundle _temp in sceneBundles) {
                if (_temp.ContainScenes(_scenes)) {
                    _bundle = _temp;
                    _valid = true;

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

            foreach (SceneBundle _temp in sceneBundles) {
                if (_temp.ContainScene(_scene)) {
                    _bundle = _temp;
                    _valid = true;

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

            foreach (SceneBundle _temp in sceneBundles) {
                if (_temp.ContainScenes(_scenes)) {
                    _bundle = _temp;
                    _valid = true;

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
        /// Get the build index of a specific scene from its GUID.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the scene to get associated build index.</param>
        /// <returns>Build index of the scene if it was included in build, -1 otherwise.</returns>
        public static int GetSceneBuildIndex(string _sceneGUID) {
            int _index = Array.IndexOf(Database.buildSceneGUIDs, _sceneGUID);
            return _index;
        }

        /// <summary>
        /// Internal method used to debug the name of a non included scene in build that is trying to be loaded or unlaoded.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the non included in build scene.</param>
        /// <returns>Name of this scene if in editor or in a development build, and an empty string otherwise.</returns>
        internal static string GetNonBuildSceneName(string _sceneGUID) {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            foreach (NonBuildScene _scene in Database.nonBuildScenes) {
                if (_scene.GUID == _sceneGUID)
                    return _scene.Name;
            }
            #endif

            return string.Empty;
        }

        // -----------------------

        /// <summary>
        /// Is this scene the core game scene?
        /// </summary>
        /// <param name="_scene">Scene to check.</param>
        /// <returns>True if this is the core scene, false otherwise.</returns>
        public bool IsCoreScene(Scene _scene) {
            return _scene.buildIndex == coreSceneIndex;
        }
        #endregion
    }
}
