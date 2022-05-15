// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor class manipulating and updating the data contained in the <see cref="BuildSceneDatabase"/>.
    /// </summary>
    [InitializeOnLoad]
	public class BuildSceneDatabaseManager : IPreprocessBuildWithReport
    {
        #region Global Members
        private static readonly AutoManagedResource<BuildSceneDatabase> resource = new AutoManagedResource<BuildSceneDatabase>("BuildSceneDatabase", false);

        int IOrderedCallback.callbackOrder => 999;

        /// <summary>
        /// Database containing informations about all scenes included in build.
        /// </summary>
        public static BuildSceneDatabase Database => resource.GetResource();

        // -----------------------

        static BuildSceneDatabaseManager()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            BuildSceneDatabase.EditorTagDatabaseGetter = () => Database;
        }
        #endregion

        #region Management
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report)
        {
            // Called just before a build is started.
            UpdateDatabase();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            if (_state == PlayModeStateChange.EnteredPlayMode)
                UpdateDatabase();
        }

        private static void UpdateDatabase()
        {
            List<string> _allScenesPath = new List<string>(EnhancedEditorUtility.FindAssets<UnityEditor.SceneAsset>());

            // Register the GUID of all scenes included in build.
            EditorBuildSettingsScene[] _buildScenes = EditorBuildSettings.scenes;
            string[] _sceneGUIDs = new string[_buildScenes.Length];
            int _count = 0;

            for (int _i = 0; _i < _sceneGUIDs.Length; _i++)
            {
                var _scene = _buildScenes[_i];

                // Skip disabled scenes.
                if (!_scene.enabled)
                    continue;

                _allScenesPath.Remove(_scene.path);

                string _guid = AssetDatabase.AssetPathToGUID(_scene.path);
                _sceneGUIDs[_count] = _guid;

                _count++;
            }

            Array.Resize(ref _sceneGUIDs, _count);

            // Register the name of all non build scenes for debug.
            BuildSceneDatabase.NonBuildScene[] _nonBuildScenes = new BuildSceneDatabase.NonBuildScene[_allScenesPath.Count];
            for (int _i = 0; _i < _nonBuildScenes.Length; _i++)
            {
                string _path = _allScenesPath[_i];

                string _name = Path.GetFileNameWithoutExtension(_path);
                string _guid = AssetDatabase.AssetPathToGUID(_path);

                _nonBuildScenes[_i] = new BuildSceneDatabase.NonBuildScene(_name, _guid);
            }

            // Register informations in the database.
            Database.buildSceneGUIDs = _sceneGUIDs;
            Database.nonBuildScenes = _nonBuildScenes;

            EditorUtility.SetDirty(Database);
        }
        #endregion
    }
}
