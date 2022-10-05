// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="ResetOnExitPlayModeAttribute"/>-related manager class. 
    /// </summary>
    internal static class ResetOnExitPlayModeManager {
        #region Serialization Wrappers
        [Serializable]
        private class JsonWrapper {
            public List<ObjectWrapper> Objects = new List<ObjectWrapper>();
        }

        [Serializable]
        private class ObjectWrapper {
            public string AssetPath = string.Empty;
            public string Json = string.Empty;

            // -----------------------

            public ObjectWrapper(string _path, string _value) {
                AssetPath = _path;
                Json = _value;
            }
        }
        #endregion

        #region Play Mode State Changed
        private const string MainKey = "ResetOnExitPlayMode_MainKey";
        private const string RefreshKey = "ResetOnExitPlayMode_RefreshKey";

        // -----------------------

        [InitializeOnLoadMethod]
        private static void Initialize() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Deserialize content if needed, then save its value.
            if (EditorPrefs.GetBool(RefreshKey, false)) {
                DeserializeObjects();
            }

            SerializeObjects();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _mode) {
            switch (_mode) {
                case PlayModeStateChange.EnteredEditMode:
                    DeserializeObjects();
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    SerializeObjects();
                    EditorPrefs.SetBool(RefreshKey, true);
                    break;

                default:
                    break;
            }
        }

        private static void SerializeObjects() {
            JsonWrapper _json = new JsonWrapper();

            foreach (Type _type in GetObjectTypes()) {
                foreach (ScriptableObject _object in LoadObjects(_type)) {
                    _json.Objects.Add(new ObjectWrapper(AssetDatabase.GetAssetPath(_object), EditorJsonUtility.ToJson(_object)));
                }
            }

            EditorPrefs.SetString(MainKey, EditorJsonUtility.ToJson(_json));
        }

        private static void DeserializeObjects() {
            string _json = EditorPrefs.GetString(MainKey, string.Empty);
            if (string.IsNullOrEmpty(_json)) {
                return;
            }

            JsonWrapper _wrapper = new JsonWrapper();
            EditorJsonUtility.FromJsonOverwrite(_json, _wrapper);

            foreach (Type _type in GetObjectTypes()) {
                foreach (ScriptableObject _object in LoadObjects(_type)) {
                    string _path = AssetDatabase.GetAssetPath(_object);
                    int _index = _wrapper.Objects.FindIndex(o => o.AssetPath == _path);

                    if (_index != -1) {
                        EditorJsonUtility.FromJsonOverwrite(_wrapper.Objects[_index].Json, _object);
                        _wrapper.Objects.RemoveAt(_index);
                    }
                }
            }

            EditorPrefs.SetBool(RefreshKey, false);
        }
        #endregion

        #region Utility
        private static readonly MethodInfo loadObjectGenericMethodInfo = typeof(ResetOnExitPlayModeManager).GetMethod("DoLoadObjects", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly List<ScriptableObject> _buffer = new List<ScriptableObject>();

        // -----------------------

        private static List<Type> GetObjectTypes() {
            List<Type> _types = new List<Type>();
            Type _scriptableType = typeof(ScriptableObject);

            #if UNITY_2019_2_OR_NEWER
            foreach (var _type in TypeCache.GetTypesWithAttribute<ResetOnExitPlayModeAttribute>()) {
                if (_type.IsSubclassOf(_scriptableType)) {
                    _types.Add(_type);
                }
            }
            #else
            foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type _type in _assembly.GetTypes()) {
                    if (_type.IsSubclassOf(_scriptableType) && (_type.GetCustomAttribute<ResetOnExitPlayModeAttribute>(true) != null)) {
                        _types.Add(_type);
                    }
                }
            }
            #endif

            return _types;
        }

        private static List<ScriptableObject> LoadObjects(Type _type) {
            loadObjectGenericMethodInfo.MakeGenericMethod(_type).Invoke(null, null);

            return _buffer;
        }

        #pragma warning disable IDE0051
        private static void DoLoadObjects<T>() where T : ScriptableObject {
            _buffer.Clear();
            _buffer.AddRange(EnhancedEditorUtility.LoadAssets<T>());
        }
        #endregion
    }
}
