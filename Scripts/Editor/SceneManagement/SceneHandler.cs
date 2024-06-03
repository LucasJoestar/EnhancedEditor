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

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="ScriptableObject"/> storing data about the core scene and scene groups,
    /// which can be manipulated from the <see cref="SceneHandlerWindow"/>.
    /// </summary>
    [NonEditable("Please use the SceneHandler window to edit these settings.")]
    public sealed class SceneHandler : ScriptableObject {
        #region Scene Wrapper
        /// <summary>
        /// Base class for <see cref="UnityEditor.SceneAsset"/> and <see cref="SceneBundle"/> wrappers.
        /// <br/> You should simply ignore this.
        /// </summary>
        [Serializable]
        public abstract class DataWrapper {
            [NonSerialized] internal bool IsLoaded = false;
            [NonSerialized] internal bool IsVisible = true;
        }

        /// <summary>
        /// Wrapper class for <see cref="UnityEditor.SceneAsset"/> objects.
        /// </summary>
        [Serializable]
        public sealed class Scene : DataWrapper, IComparable<Scene> {
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
            public Scene(string _guid) {
                string _path = AssetDatabase.GUIDToAssetPath(_guid);

                GUID = _guid;
                Name = Path.GetFileNameWithoutExtension(_path);
            }

            // -----------------------

            int IComparable<Scene>.CompareTo(Scene _other) {
                return Name.CompareTo(_other.Name);
            }
        }

        /// <summary>
        /// Wrapper class for <see cref="SceneBundle"/> objects.
        /// </summary>
        [Serializable]
        public sealed class Bundle : DataWrapper, IComparable<Bundle> {
            /// <summary>
            /// Wrapped <see cref="EnhancedEditor.SceneBundle"/>.
            /// </summary>
            public SceneBundle SceneBundle = null;

            // -----------------------

            /// <param name="_bundle"><inheritdoc cref="SceneBundle" path="/summary"/></param>
            /// <inheritdoc cref="Bundle"/>
            public Bundle(SceneBundle _bundle) {
                SceneBundle = _bundle;
            }

            // -----------------------

            int IComparable<Bundle>.CompareTo(Bundle _other) {
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
        public abstract class Group : IComparable<Group> {
            /// <summary>
            /// Name of this group, displayed in the <see cref="SceneHandlerWindow"/>.
            /// </summary>
            public string Name = "New Group";

            [NonSerialized] internal bool IsVisible = true;

            // -----------------------

            public Group(string _name) {
                Name = _name;
            }

            // -----------------------

            int IComparable<Group>.CompareTo(Group _other) {
                return Name.CompareTo(_other.Name);
            }
        }

        /// <summary>
        /// Group used to store multiple <see cref="Scene"/> together in the <see cref="SceneHandlerWindow"/>.
        /// </summary>
        [Serializable]
        public sealed class SceneGroup : Group {
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
        public sealed class BundleGroup : Group {
            /// <summary>
            /// All <see cref="Bundle"/> packed in this group.
            /// </summary>
            public Bundle[] Bundles = new Bundle[] { };

            // -----------------------

            public BundleGroup(string _name) : base(_name) { }
        }
        #endregion

        #region Global Members
        [SerializeField] internal SceneGroup[]  sceneGroups  = new SceneGroup [] { new SceneGroup ("Default Group") };
        [SerializeField] internal BundleGroup[] bundleGroups = new BundleGroup[] { new BundleGroup("Default Group") };
        #endregion

        #region Management
        /// <summary>
        /// Refresh all scenes and bundles data.
        /// </summary>
        public void Refresh() {
            // Update scene data.
            List<string> _sceneGUIDs = new List<string>(EnhancedEditorUtility.FindAssetsGUID<UnityEditor.SceneAsset>());
            for (int i = 1; i < sceneGroups.Length; i++) {
                SceneGroup _group = sceneGroups[i];
                for (int j = 0; j < _group.Scenes.Length; j++) {
                    Scene _scene = _group.Scenes[j];
                    int _index = _sceneGUIDs.IndexOf(_scene.GUID);

                    if (_index == -1) {
                        // If the scene could not be found, remove it from the group.
                        ArrayUtility.RemoveAt(ref _group.Scenes, j);
                        if (_group.Scenes.Length == 0) {
                            ArrayUtility.RemoveAt(ref sceneGroups, i);
                            i--;

                            break;
                        }

                        j--;
                    } else {
                        // Initialize scene informations.
                        string _path = AssetDatabase.GUIDToAssetPath(_scene.GUID);
                        _scene.Name = Path.GetFileNameWithoutExtension(_path);

                        _sceneGUIDs.RemoveAt(_index);
                    }
                }
            }

            // Register unregistered scenes.
            Scene[] _defaultGroup = new Scene[_sceneGUIDs.Count];
            for (int i = 0; i < _defaultGroup.Length; i++) {
                Scene _scene = new Scene(_sceneGUIDs[i]);
                _defaultGroup[i] = _scene;
            }

            sceneGroups[0].Scenes = _defaultGroup;

            // Update bundle data.
            List<SceneBundle> bundles = new List<SceneBundle>(EnhancedEditorUtility.LoadAssets<SceneBundle>());
            for (int i = 1; i < bundleGroups.Length; i++) {
                BundleGroup _group = bundleGroups[i];
                for (int j = 0; j < _group.Bundles.Length; j++) {
                    Bundle _bundle = _group.Bundles[j];
                    if (_bundle == null) {
                        // If the scene bundle could not be found, remove it from the group.
                        ArrayUtility.RemoveAt(ref _group.Bundles, j);
                        if (_group.Bundles.Length == 0) {
                            ArrayUtility.RemoveAt(ref bundleGroups, i);
                            i--;

                            break;
                        }

                        j--;
                    } else {
                        // Initialize scene informations.
                        _bundle.IsLoaded = false;
                        bundles.Remove(_bundle.SceneBundle);
                    }
                }
            }

            // Register unregistered bundles.
            Bundle[] _defaultBundles = new Bundle[bundles.Count];
            for (int i = 0; i < _defaultBundles.Length; i++) {
                Bundle _bundle = new Bundle(bundles[i]);
                _defaultBundles[i] = _bundle;
            }

            bundleGroups[0].Bundles = _defaultBundles;

            // Sort all scenes.
            Sort();
            UpdateLoadedScenes();
        }

        /// <summary>
        /// Update the state of each scene depending whether it is loaded or not.
        /// </summary>
        internal void UpdateLoadedScenes(int _unloadedSceneIndex = -2) {
            // Get loaded scenes path.
            string[] _loadedScenePaths = new string[EditorSceneManager.sceneCount];
            for (int _i = 0; _i < _loadedScenePaths.Length; _i++) {
                UnityEngine.SceneManagement.Scene _scene = EditorSceneManager.GetSceneAt(_i);
                _loadedScenePaths[_i] = (_scene.buildIndex == _unloadedSceneIndex)
                                      ? string.Empty
                                      : _scene.path;
            }

            // Scenes loaded state.
            foreach (var _group in sceneGroups) {
                foreach (var _scene in _group.Scenes) {
                    bool _isLoaded = ArrayUtility.Contains(_loadedScenePaths, AssetDatabase.GUIDToAssetPath(_scene.GUID));
                    _scene.IsLoaded = _isLoaded;
                }
            }

            // Bundles loaded state.
            foreach (var _group in bundleGroups) {
                foreach (var _bundle in _group.Bundles) {
                    bool _isLoaded = true;

                    foreach (var _scene in _bundle.SceneBundle.Scenes) {
                        if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(_scene.GUID)) && !ArrayUtility.Contains(_loadedScenePaths, AssetDatabase.GUIDToAssetPath(_scene.GUID))) {
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
        internal void Sort() {
            if (sceneGroups.Length > 1) {

                foreach (var _group in sceneGroups) {
                    Array.Sort(_group.Scenes);
                }
            }

            if (bundleGroups.Length > 1) {

                foreach (var _group in bundleGroups) {
                    Array.Sort(_group.Bundles);
                }
            }

            SaveChanges();
        }
        #endregion

        #region Utility
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        private static void OpenSceneHandlerWindow() {
            SceneHandlerWindow.GetWindow();
        }

        internal void SaveChanges() {
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Clears all groups in this handler.
        /// </summary>
        public void ResetGroups() {
            sceneGroups  = new SceneGroup[] { new SceneGroup("Default Group") };
            bundleGroups = new BundleGroup[] { new BundleGroup("Default Group") };

            SaveChanges();
        }
        #endregion
    }
}
