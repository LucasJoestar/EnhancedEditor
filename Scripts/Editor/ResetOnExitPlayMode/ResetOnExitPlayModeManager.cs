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

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="ResetOnExitPlayModeAttribute"/>-related manager class. 
    /// </summary>
    [InitializeOnLoad]
    internal static class ResetOnExitPlayModeManager {
        #region Serialization Wrappers
        [Serializable]
        private class JsonWrapper {
            public List<ObjectWrapper> Objects = new List<ObjectWrapper>();
        }

        [Serializable]
        private class ObjectWrapper {
            public string GUID = string.Empty;
            public string Json = string.Empty;

            // -----------------------

            public ObjectWrapper(Object _object) {
                GUID = EnhancedEditorUtility.GetAssetGUID(_object);
                Json = EditorJsonUtility.ToJson(_object);
            }
        }
        #endregion

        #region Play Mode State Changed
        private const string MainKey = "ResetOnExitPlayMode_MainKey";

        // -----------------------

        static ResetOnExitPlayModeManager() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Restore values.
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                DeserializeObjects();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _mode) {
            switch (_mode) {
                // Reset values when entering edit mode.
                case PlayModeStateChange.EnteredEditMode:
                    DeserializeObjects();
                    break;

                // Save values before entering Play mode.
                case PlayModeStateChange.ExitingEditMode:
                    SerializeObjects();
                    break;

                default:
                    break;
            }
        }

        private static void SerializeObjects() {
            JsonWrapper _json = new JsonWrapper();

            foreach (Type _type in GetObjectTypes())
            foreach (ScriptableObject _object in LoadObjects(_type)) {
                
                ObjectWrapper _wrapper = new ObjectWrapper(_object);
                _json.Objects.Add(_wrapper);
            }

            // Register values to be restored outside Play mode.
            EditorPrefs.SetString(MainKey, EditorJsonUtility.ToJson(_json));
        }

        private static void DeserializeObjects() {
            // Get values to restore.
            string _json = EditorPrefs.GetString(MainKey, string.Empty);
            if (string.IsNullOrEmpty(_json)) {
                return;
            }

            JsonWrapper _wrapper = new JsonWrapper();
            EditorJsonUtility.FromJsonOverwrite(_json, _wrapper);

            foreach (var _obj in _wrapper.Objects) {
                string _path = AssetDatabase.GUIDToAssetPath(_obj.GUID);

                // If an object can't be found, skip it.
                if (string.IsNullOrEmpty(_path)) {
                    continue;
                }

                Object _asset = AssetDatabase.LoadMainAssetAtPath(_path);
                EditorJsonUtility.FromJsonOverwrite(_obj.Json, _asset);
            }

            // Reset Prefs value to indicate there is nothing to be restored.
            EditorPrefs.SetString(MainKey, string.Empty);
        }
        #endregion

        #region Utility
        private static readonly List<Type> resetTypes   = new List<Type>();
        private static readonly List<Object> buffer     = new List<Object>();

        // -----------------------

        private static List<Type> GetObjectTypes() {
            Type _scriptableType = typeof(ScriptableObject);
            resetTypes.Clear();

            #if UNITY_2019_2_OR_NEWER
            foreach (var _type in TypeCache.GetTypesWithAttribute<ResetOnExitPlayModeAttribute>()) {
                if (_type.IsSubclassOf(_scriptableType)) {
                    resetTypes.Add(_type);
                }
            }
            #else
            foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type _type in _assembly.GetTypes()) {
                    if (_type.IsSubclassOf(_scriptableType) && (_type.GetCustomAttribute<ResetOnExitPlayModeAttribute>(true) != null)) {
                        resetTypes.Add(_type);
                    }
                }
            }
            #endif

            return resetTypes;
        }

        private static List<Object> LoadObjects(Type _type) {
            buffer.Clear();
            buffer.AddRange(EnhancedEditorUtility.LoadAssets(_type));

            return buffer;
        }
        #endregion
    }
}
