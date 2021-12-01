// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> storing data about the core scene and scene groups,
    /// which can be manipulated from the <see cref="SceneHandlerWindow"/>.
    /// </summary>
    [NonEditable("Please use the SceneHandler window to edit these settings.")]
    public class SceneHandler : ScriptableObject
    {
        #region Scene Wrapper
        /// <summary>
        /// Base class for <see cref="UnityEditor.SceneAsset"/> and <see cref="SceneBundle"/> wrappers.
        /// <br/> You should simply ignore this.
        /// </summary>
        [Serializable]
        public abstract class DataWrapper
        {
            [NonSerialized] internal bool IsLoaded = false;
            [NonSerialized] internal bool IsVisible = true;
        }

        /// <summary>
        /// Wrapper class for <see cref="UnityEditor.SceneAsset"/> objects.
        /// </summary>
        [Serializable]
        public class Scene : DataWrapper, IComparable<Scene>
        {
            /// <summary>
            /// GUID of the wrapped scene.
            /// </summary>
            public string GUID = string.Empty;

            /// <summary>
            /// Name of this scene.
            /// </summary>
            public string Name { get; internal set; }

            // -----------------------

            /// <param name="_guid"><inheritdoc cref="GUID" path="/summary"/></param>
            /// <inheritdoc cref="Scene"/>
            public Scene(string _guid)
            {
                string _path = AssetDatabase.GUIDToAssetPath(_guid);

                GUID = _guid;
                Name = Path.GetFileNameWithoutExtension(_path);
            }

            // -----------------------

            int IComparable<Scene>.CompareTo(Scene _other)
            {
                return Name.CompareTo(_other.Name);
            }
        }

        /// <summary>
        /// Wrapper class for <see cref="SceneBundle"/> objects.
        /// </summary>
        [Serializable]
        public class Bundle : DataWrapper, IComparable<Bundle>
        {
            /// <summary>
            /// Wrapped <see cref="EnhancedEditor.SceneBundle"/>.
            /// </summary>
            public SceneBundle SceneBundle = null;

            // -----------------------

            /// <param name="_bundle"><inheritdoc cref="SceneBundle" path="/summary"/></param>
            /// <inheritdoc cref="Bundle"/>
            public Bundle(SceneBundle _bundle)
            {
                SceneBundle = _bundle;
            }

            // -----------------------

            int IComparable<Bundle>.CompareTo(Bundle _other)
            {
                return SceneBundle.name.CompareTo(_other.SceneBundle.name);
            }
        }
        #endregion

        #region Group
        /// <summary>
        /// Base class for <see cref="SceneGroup"/> and <see cref="BundleGroup"/> inheritance.
        /// <br/> You should simply ignore this.
        /// </summary>
        [Serializable]
        public abstract class Group : IComparable<Group>
        {
            /// <summary>
            /// Name of this group, displayed in the <see cref="SceneHandlerWindow"/>.
            /// </summary>
            public string Name = "New Group";

            [NonSerialized] internal bool IsVisible = true;

            // -----------------------

            public Group(string _name)
            {
                Name = _name;
            }

            // -----------------------

            int IComparable<Group>.CompareTo(Group _other)
            {
                return Name.CompareTo(_other.Name);
            }
        }

        /// <summary>
        /// Group used to store multiple <see cref="Scene"/> together in the <see cref="SceneHandlerWindow"/>.
        /// </summary>
        [Serializable]
        public class SceneGroup : Group
        {
            /// <summary>
            /// All <see cref="Scene"/> packed in this group.
            /// </summary>
            public Scene[] Scenes = new Scene[] { };

            // -----------------------

            public SceneGroup(string _name) : base(_name) { }
        }

        /// <summary>
        /// Group used to store multiple <see cref="Bundle"/> together in the <see cref="SceneHandlerWindow"/>.
        /// </summary>
        [Serializable]
        public class BundleGroup : Group
        {
            /// <summary>
            /// All <see cref="Bundle"/> packed in this group.
            /// </summary>
            public Bundle[] Bundles = new Bundle[] { };

            // -----------------------

            public BundleGroup(string _name) : base(_name) { }
        }
        #endregion

        #region Global Members
        [SerializeField] internal SceneGroup[] sceneGroups = new SceneGroup[] { new SceneGroup("Default Group") };
        [SerializeField] internal BundleGroup[] bundleGroups = new BundleGroup[] { new BundleGroup("Default Group") };
        #endregion

        #region Management
        /// <summary>
        /// Refresh all scenes and bundles data.
        /// </summary>
        public void Refresh()
        {
            // Update scene data.
            List<string> _sceneGUIDs = new List<string>(EnhancedEditorUtility.FindAssetsGUID<UnityEditor.SceneAsset>());
            for (int _i = 1; _i < sceneGroups.Length; _i++)
            {
                SceneGroup _group = sceneGroups[_i];
                for (int _j = 0; _j < _group.Scenes.Length; _j++)
                {
                    Scene _scene = _group.Scenes[_j];
                    int _index = _sceneGUIDs.IndexOf(_scene.GUID);

                    if (_index == -1)
                    {
                        // If the scene could not be found, remove it from the group.
                        ArrayUtility.RemoveAt(ref _group.Scenes, _j);
                        if (_group.Scenes.Length == 0)
                        {
                            ArrayUtility.RemoveAt(ref sceneGroups, _i);
                            _i--;

                            break;
                        }

                        _j--;
                    }
                    else
                    {
                        // Initialize scene informations.
                        string _path = AssetDatabase.GUIDToAssetPath(_scene.GUID);
                        _scene.Name = Path.GetFileNameWithoutExtension(_path);

                        _sceneGUIDs.RemoveAt(_index);
                    }
                }
            }

            // Register unregistered scenes.
            Scene[] _defaultGroup = new Scene[_sceneGUIDs.Count];
            for (int _i = 0; _i < _defaultGroup.Length; _i++)
            {
                Scene _scene = new Scene(_sceneGUIDs[_i]);
                _defaultGroup[_i] = _scene;
            }

            sceneGroups[0].Scenes = _defaultGroup;

            // Update bundle data.
            List<SceneBundle> bundles = new List<SceneBundle>(EnhancedEditorUtility.LoadAssets<SceneBundle>());
            for (int _i = 1; _i < bundleGroups.Length; _i++)
            {
                BundleGroup _group = bundleGroups[_i];
                for (int _j = 0; _j < _group.Bundles.Length; _j++)
                {
                    Bundle _bundle = _group.Bundles[_j];
                    if (_bundle == null)
                    {
                        // If the scene bundle could not be found, remove it from the group.
                        ArrayUtility.RemoveAt(ref _group.Bundles, _j);
                        if (_group.Bundles.Length == 0)
                        {
                            ArrayUtility.RemoveAt(ref bundleGroups, _i);
                            _i--;

                            break;
                        }

                        _j--;
                    }
                    else
                    {
                        // Initialize scene informations.
                        _bundle.IsLoaded = false;
                        bundles.Remove(_bundle.SceneBundle);
                    }
                }
            }

            // Register unregistered bundles.
            Bundle[] _defaultBundles = new Bundle[bundles.Count];
            for (int _i = 0; _i < _defaultBundles.Length; _i++)
            {
                Bundle _bundle = new Bundle(bundles[_i]);
                _defaultBundles[_i] = _bundle;
            }

            bundleGroups[0].Bundles = _defaultBundles;

            // Sort all scenes.
            Sort();
            UpdateLoadedScenes();
        }

        /// <summary>
        /// Update the state of each scene depending whether it is loaded or not.
        /// </summary>
        internal void UpdateLoadedScenes(int _unloadedSceneIndex = -2)
        {
            // Get loaded scenes path.
            string[] _loadedScenePaths = new string[EditorSceneManager.sceneCount];
            for (int _i = 0; _i < _loadedScenePaths.Length; _i++)
            {
                UnityEngine.SceneManagement.Scene _scene = EditorSceneManager.GetSceneAt(_i);
                _loadedScenePaths[_i] = (_scene.buildIndex == _unloadedSceneIndex)
                                      ? string.Empty
                                      : _scene.path;
            }

            // Scenes loaded state.
            foreach (var _group in sceneGroups)
            {
                foreach (var _scene in _group.Scenes)
                {
                    bool _isLoaded = ArrayUtility.Contains(_loadedScenePaths, AssetDatabase.GUIDToAssetPath(_scene.GUID));
                    _scene.IsLoaded = _isLoaded;
                }
            }

            // Bundles loaded state.
            foreach (var _group in bundleGroups)
            {
                foreach (var _bundle in _group.Bundles)
                {
                    bool _isLoaded = true;

                    foreach (var _scene in _bundle.SceneBundle.Scenes)
                    {
                        if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(_scene.GUID)) && !ArrayUtility.Contains(_loadedScenePaths, AssetDatabase.GUIDToAssetPath(_scene.GUID)))
                        {
                            _isLoaded = false;
                            break;
                        }
                    }

                    _bundle.IsLoaded = _isLoaded;
                }
            }
        }

        /// <summary>
        /// Sort all groups and data.
        /// </summary>
        internal void Sort()
        {
            Array.Sort(sceneGroups, 1, sceneGroups.Length - 1);
            Array.Sort(bundleGroups, 1, bundleGroups.Length - 1);

            foreach (var _group in sceneGroups)
                Array.Sort(_group.Scenes);

            foreach (var _group in bundleGroups)
                Array.Sort(_group.Bundles);

            SaveChanges();
        }
        #endregion

        #region Utility
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        #pragma warning disable IDE0051
        private static void OpenSceneHandlerWindow()
        {
            SceneHandlerWindow.GetWindow();
        }

        internal void SaveChanges()
        {
            EditorUtility.SetDirty(this);
        }
        #endregion
    }
}
