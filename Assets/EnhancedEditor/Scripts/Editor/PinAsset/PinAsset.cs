// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// A <see cref="PinAsset"/> is a <see cref="ScriptableObject"/> associated with a specific <see cref="GameObject"/> in a scene.
    /// <para/>
    /// You can use them to keep track of important objects in your project.
    /// </summary>
	public class PinAsset : ScriptableObject
    {
        #region Global Members
        private const string PinAssetsFolder = "/CORE/Editor/Pin Assets/";

        [SerializeField] private int objectID = 999;
        [SerializeField] private string objectType = string.Empty;
        [SerializeField] private string sceneGUID = string.Empty;
        #endregion

        #region Pin Asset Behaviour
        /// <summary>
        /// Creates a new <see cref="PinAsset"/> associated with a given <see cref="MonoBehaviour"/> in scene.
        /// </summary>
        /// <param name="_folder">Folder where to create asset. Value is added to <see cref="PinAssetsFolder"/> from the Assets folder.</param>
        /// <param name="_object">Object to associate with this <see cref="PinAsset"/>.</param>
        public static void CreatePinAsset(string _folder, MonoBehaviour _object)
        {
            // Use the object name hash code as id.
            string _name = _object.gameObject.name;
            int _id = _name.GetHashCode();

            Scene _scene = _object.gameObject.scene;
            string _sceneGUID = _scene.path;
            if (!string.IsNullOrEmpty(_sceneGUID))
            {
                _sceneGUID = AssetDatabase.AssetPathToGUID(_sceneGUID);
                string[] _pinAssetPaths = EnhancedEditorUtility.FindAssets<PinAsset>();
                foreach (string _path in _pinAssetPaths)
                {
                    PinAsset _pinAsset = AssetDatabase.LoadAssetAtPath<PinAsset>(_path);
                    if (_pinAsset.DoMatch(_sceneGUID, _id))
                    {
                        // Refresh pin assets database if path has changed.
                        string _oldSceneName = Path.GetFileName(Path.GetDirectoryName(_path));
                        if (_oldSceneName != _scene.name)
                            PinAsset.CleanPinAssets();

                        // If a PinAsset associated with this object already exist,
                        // we've got nothing more the do here.
                        return;
                    }
                }

                // Create a pin asset for the target object.
                Type _type = _object.GetType();

                var _asset = CreateInstance<PinAsset>();
                _asset.sceneGUID = _sceneGUID;
                _asset.objectID = _id;
                _asset.objectType = $"{_type},{_type.Assembly}";

                string _directory = $"{Application.dataPath}{PinAssetsFolder}{Path.GetDirectoryName(_folder)}";
                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);

                _directory = $"Assets{PinAssetsFolder}{_folder}.asset";
                AssetDatabase.CreateAsset(_asset, _directory);
                AssetDatabase.SaveAssets();
            }
        }

        // -----------------------

        /// <summary>
        /// Get if this <see cref="PinAsset"/> matches a given id.
        /// </summary>
        /// <param name="_sceneGUID">GUID of associated scene.</param>
        /// <param name="_objectID">Id of associated object.</param>
        /// <returns>True if the id match, false otherwise.</returns>
        public bool DoMatch(string _sceneGUID, int _objectID)
        {
            return (_sceneGUID == sceneGUID) && (_objectID == objectID);
        }

        /// <summary>
        /// Pin the associated object in its scene.
        /// If not found, this asset automatically destroys itself.
        /// </summary>
        /// <returns>True if successfully found object, false otherwise.</returns>
        public bool PinAssetInScene()
        {
            Type _type = Type.GetType(objectType);
            string _scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            if (!string.IsNullOrEmpty(_scenePath) && (_type != null))
            {
                EditorSceneManager.OpenScene(_scenePath);
                MonoBehaviour[] _monobehaviours = FindObjectsOfType(_type) as MonoBehaviour[];
                foreach (MonoBehaviour _monobehaviour in _monobehaviours)
                {
                    if (_monobehaviour.gameObject.name.GetHashCode() == objectID)
                    {
                        EditorGUIUtility.PingObject(_monobehaviour);
                        Selection.activeObject = _monobehaviour;
                        return true;
                    }
                }
            }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this));
            return false;
        }
        #endregion

        #region Cleaner
        private const float ProgressBarSaveRatio = .1f;
        private const float ProgressBarAssetRatio = .8f;

        private const string ProgressBarTitle = "Cleaning project PinAssets, please wait until process ends...";
        private const string ProgressBarSaveInfos = "Saving open scenes...";
        private const string ProgressBarAssetInfos1 = "Checking asset '";
        private const string ProgressBarAssetInfos2 = "'...";
        private const string ProgressBarFolderInfos = "Deleting empty folders...";

        // -----------------------

        [MenuItem("Enhanced Editor/Clean Pin Assets", false, 900)]
        public static void CleanPinAssets() => EditorCoroutineUtility.StartCoroutineOwnerless(DoCleanPinAssets());

        private static IEnumerator DoCleanPinAssets()
        {
            // Save and store currently open scene(s) to reopen them after cleaning end.
            EditorSceneManager.SaveOpenScenes();

            string[] openScenes = new string[EditorSceneManager.sceneCount];
            for (int _i = 0; _i < openScenes.Length; _i++)
                openScenes[_i] = EditorSceneManager.GetSceneAt(_i).path;

            // Initialize progress bar.
            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarSaveInfos, ProgressBarSaveRatio / 2f);
            yield return null;

            string[] _pinAssetPaths = EnhancedEditorUtility.FindAssets<PinAsset>();
            yield return null;

            float _progress = ProgressBarSaveRatio;
            for (int _i = 0; _i < _pinAssetPaths.Length; _i++)
            {
                // Update progress bar.
                yield return null;

                string _path = _pinAssetPaths[_i];
                _progress += (ProgressBarAssetRatio - ProgressBarSaveRatio) / _pinAssetPaths.Length;
                EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarAssetInfos1 + _path + ProgressBarAssetInfos2, _progress);

                PinAsset _pinAsset = AssetDatabase.LoadAssetAtPath<PinAsset>(_path);
                if (_pinAsset.PinAssetInScene())
                {
                    // Associated object exist, that's cool.
                    //
                    // Now, check if the object type and names do match,
                    // and move asset in correct folder if not.
                    string _newPath = Path.Combine(EditorSceneManager.GetActiveScene().name, $"{Selection.activeObject.name}.asset");
                    var _attributes = Type.GetType(_pinAsset.objectType).GetCustomAttributes(typeof(PinnedObjectAttribute), true) as PinnedObjectAttribute[];
                    if (_attributes.Length > 0)
                    {
                        PinnedObjectAttribute _attribute = _attributes[0];
                        _newPath = $"Assets{PinAssetsFolder}{Path.Combine(_attribute.Path, _newPath)}";
                        if (_newPath != _path)
                        {
                            // Create destination directory and move asset.
                            string _fullPath = Application.dataPath + Path.GetDirectoryName(_newPath.Remove(0, 6));
                            if (!Directory.Exists(_fullPath))
                            {
                                yield return null;
                                Directory.CreateDirectory(_fullPath);
                                AssetDatabase.Refresh();
                            }

                            yield return null;
                            AssetDatabase.MoveAsset(_path, _newPath);
                        }
                        continue;
                    }

                    // If something went wrong,
                    // delete asset as it should not exist.
                    yield return null;
                    AssetDatabase.DeleteAsset(_path);
                }
            }

            // Refresh database, then delete empty folders
            AssetDatabase.Refresh();
            yield return null;
            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarFolderInfos, ProgressBarAssetRatio);

            string _directory = $"{Application.dataPath}{PinAssetsFolder}";
            DeleteEmptyDirectories(_directory);

            yield return null;
            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarFolderInfos, 1f);
            yield return null;

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            // Reopen previously loaded scene(s).
            EditorSceneManager.OpenScene(openScenes[0], OpenSceneMode.Single);
            for (int _i = 1; _i < openScenes.Length; _i++)
            {
                string _scenePath = openScenes[_i];
                EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
            }

            // ----- Local Methods ----- //

            void DeleteEmptyDirectories(string _checkDirectory)
            {
                // Delete each directories recursively.
                foreach (var _directory in Directory.GetDirectories(_checkDirectory))
                {
                    DeleteEmptyDirectories(_directory);
                    if (Directory.GetDirectories(_directory).Length == 0)
                    {
                        if (ShouldDestroyDirectory(_directory))
                            Directory.Delete(_directory, false);
                    }
                }
            }

            bool ShouldDestroyDirectory(string _directory)
            {
                foreach (string _file in Directory.GetFiles(_directory))
                {
                    // If another file type than .meta is found, do not delete the directory.
                    if (!_file.EndsWith("meta"))
                        return false;

                    File.Delete(_file);
                }

                return true;
            }
        }
        #endregion
    }
}
