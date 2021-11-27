// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing informations about all scenes included in build.
    /// <para/> Should not be used directly.
    /// </summary>
    [NonEditable("This data is sensitive and should not be manipulated manually.")]
	public class BuildSceneDatabase : ScriptableObject
    {
        #region Non Build Scene
        [Serializable]
        internal struct NonBuildScene
        {
            public string Name;
            public string GUID;

            // -----------------------

            public NonBuildScene(string _name, string _guid)
            {
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
        public static BuildSceneDatabase Database
        {
            get
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (EditorTagDatabaseGetter != null))
                {
                    return EditorTagDatabaseGetter();
                }

                if (database == null)
                {
                    Debug.LogError($"Unassigned {typeof(BuildSceneDatabase).Name} reference!\nYou must manually set this database " +
                                   $"reference on game start to be able to properly use {typeof(SceneAsset).Name}s and {typeof(SceneBundle).Name}s.");

                    database = CreateInstance<BuildSceneDatabase>();
                }
                #endif

                return database;
            }
            set
            {
                database = value;
            }
        }

        [SerializeField] internal string[] buildSceneGUIDs = new string[] { };

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] internal NonBuildScene[] nonBuildScenes = new NonBuildScene[] { };
        #endif
        #endregion

        #region Behaviour
        /// <summary>
        /// Get the build index of a specific scene from its GUID.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the scene to get associated build index.</param>
        /// <returns>Build index of the scene if it was included in build, -1 otherwise.</returns>
        public static int GetSceneBuildIndex(string _sceneGUID)
        {
            int _index = Array.IndexOf(Database.buildSceneGUIDs, _sceneGUID);
            return _index;
        }

        /// <summary>
        /// Internal method used to debug the name of a non included scene in build that is trying to be loaded or unlaoded.
        /// </summary>
        /// <param name="_sceneGUID">GUID of the non included in build scene.</param>
        /// <returns>Name of this scene if in editor or in a development build, and an empty string otherwise.</returns>
        internal static string GetNonBuildSceneName(string _sceneGUID)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            foreach (NonBuildScene _scene in Database.nonBuildScenes)
            {
                if (_scene.GUID == _sceneGUID)
                    return _scene.Name;
            }
            #endif

            return string.Empty;
        }
        #endregion
    }
}
