// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> registering all instances of a specific <see cref="Component"/> in each scenes of the project.
    /// <br/> Usefull to keep track of important objects.
    /// </summary>
    [InitializeOnLoad]
    public class InstanceTracker : ScriptableObject
    {
        #region Scene Track
        [Serializable]
        internal class SceneTrack
        {
            public string SceneGUID = string.Empty;
            public string[] Instances = new string[] { };

            [NonSerialized] public List<Component> SceneInstances = new List<Component>();
            [NonSerialized] public string SceneName = string.Empty;
            [NonSerialized] public bool IsLoaded = false;
            [NonSerialized] public bool IsVisible = false;

            // -----------------------

            public void Load(List<Component> _sceneInstances, string _sceneName)
            {
                SceneInstances = _sceneInstances;
                SceneName = _sceneName;

                SceneInstances.Sort((a, b) => a.name.CompareTo(b.name));

                IsLoaded = true;
                IsVisible = false;
            }

            public bool Load(Scene _scene, string _sceneGUID, Type _targetType)
            {
                if (_sceneGUID != SceneGUID)
                    return false;

                // Get all instances in scene.
                SceneInstances = new List<Component>();
                SceneName = _scene.name;

                _scene.GetRootGameObjects(rootGameObjects);

                foreach (var _object in rootGameObjects)
                {
                    SceneInstances.AddRange(_object.GetComponentsInChildren(_targetType, true));
                }

                SceneInstances.Sort((a, b) => a.name.CompareTo(b.name));

                IsLoaded = true;
                IsVisible = false;

                return true;
            }

            public bool Close(string _sceneGUID)
            {
                if (_sceneGUID != SceneGUID)
                    return false;

                SceneInstances.Clear();
                IsLoaded = false;

                return true;
            }

            public bool OnEnabled()
            {
                if (!IsLoaded)
                {
                    string _path = AssetDatabase.GUIDToAssetPath(SceneGUID);
                    if (string.IsNullOrEmpty(_path))
                        return false;

                    SceneName = Path.GetFileNameWithoutExtension(_path);
                    SceneInstances.Clear();
                }

                IsVisible = !IsLoaded;
                return true;
            }

            public void UpdateVisibility(string _searchFilter)
            {
                IsVisible = !IsLoaded && SceneName.ToLower().Contains(_searchFilter);
            }
        }
        #endregion

        #region Global Members
        [SerializeField] internal string targetTypeName = string.Empty;
        [SerializeField] internal SceneTrack[] tracks = new SceneTrack[] { };

        [NonSerialized] internal InstanceTrackerEditor editor = null;
        internal Type targetType = null;

        // -----------------------

        static InstanceTracker()
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }
        #endregion

        #region Tracker
        private static readonly List<GameObject> rootGameObjects = new List<GameObject>();
        private static InstanceTracker[] trackers = null;

        // -----------------------

        private static void OnSceneSaved(Scene _scene)
        {
            // Ignore invalid scenes.
            if (!_scene.IsValid() || string.IsNullOrEmpty(_scene.path))
                return;

            // Load all trackers and create new ones.
            if (trackers == null)
                trackers = EnhancedEditorUtility.LoadAssets<InstanceTracker>();

            dynamic _types = null;

            #if UNITY_2019_2_OR_NEWER
            _types = TypeCache.GetTypesWithAttribute<InstanceTrackerAttribute>();
            #else
            _types = new Type[] { };
            foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type _type in _assembly.GetTypes())
                {
                    if (_type.GetCustomAttribute<InstanceTrackerAttribute>() != null)
                    {
                        ArrayUtility.Add(ref _types, _type);
                    }
                }
            }
            #endif

            foreach (var _type in _types)
            {
                string _target = _type.AssemblyQualifiedName;

                if (!Array.Exists(trackers, t => t.targetTypeName == _target))
                {
                    // Tracker directory creation.
                    string _path = Path.Combine(Application.dataPath, EnhancedEditorSettings.Settings.InstanceTrackerDirectory);
                    if (!Directory.Exists(_path))
                        Directory.CreateDirectory(_path);

                    _path = Path.Combine("Assets", EnhancedEditorSettings.Settings.InstanceTrackerDirectory, $"{_type}.asset");

                    // Create new tracker.
                    var _tracker = CreateInstance<InstanceTracker>();
                    _tracker.targetTypeName = _target;

                    ArrayUtility.Add(ref trackers, _tracker);
                    AssetDatabase.CreateAsset(_tracker, _path);
                    AssetDatabase.SaveAssets();
                }
            }

            string _guid = AssetDatabase.AssetPathToGUID(_scene.path);
            _scene.GetRootGameObjects(rootGameObjects);

            for (int _i = trackers.Length; _i-- > 0;)
            {
                InstanceTracker _tracker = trackers[_i];

                if (_tracker.Initialize())
                    _tracker.TrackScene(_scene, _guid);
            }
        }

        private void TrackScene(Scene _scene, string _sceneGUID)
        {
            bool _hasEditor = editor != null;

            // Track all target instances.
            List<Component> _sceneInstances = new List<Component>();
            List<string> _instances = new List<string>();

            foreach (var _object in rootGameObjects)
            {
                Component[] _components = _object.GetComponentsInChildren(targetType, true);
                foreach (var _component in _components)
                {
                    _instances.Add(_component.name);
                }

                if (_hasEditor)
                    _sceneInstances.AddRange(_components);
            }

            SceneTrack _track = Array.Find(tracks, t => t.SceneGUID == _sceneGUID);
            if (_instances.Count == 0)
            {
                if (_track != null)
                {
                    if (_hasEditor)
                        editor.OnRemoveTrack(_track);

                    ArrayUtility.Remove(ref tracks, _track);
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }

                return;
            }

            // Register this scene instances.
            bool _isNewTrack = false;
            if (_track == null)
            {
                _track = new SceneTrack()
                {
                    SceneGUID = _sceneGUID
                };

                _isNewTrack = true;
                ArrayUtility.Add(ref tracks, _track);
            }

            _track.Instances = _instances.ToArray();
            if (_hasEditor)
            {
                _track.Load(_sceneInstances, _scene.name);
                if (_isNewTrack)
                    editor.OnNewTrack(_track);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region Utility
        internal bool Initialize()
        {
            if (targetType == null)
            {
                targetType = Type.GetType(targetTypeName);
                if (targetType == null)
                {
                    ArrayUtility.Remove(ref trackers, this);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this));

                    return false;
                }
            }

            return true;
        }

        internal void RemoveTrack(SceneTrack _track)
        {
            ArrayUtility.Remove(ref tracks, _track);
        }
        #endregion
    }
}
