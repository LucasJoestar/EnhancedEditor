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
        #region Play Mode State Changed
        private const string SaveFormat = "ROEPM_{0}";

        // -----------------------

        [InitializeOnLoadMethod]
        private static void Initialize() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _mode) {
            switch (_mode) {
                case PlayModeStateChange.EnteredEditMode:
                    DeserializeObjects();
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    SerializeObjects();
                    break;

                default:
                    break;
            }
        }

        private static void SerializeObjects() {
            foreach (Type _type in GetObjectTypes()) {
                foreach (ScriptableObject _object in LoadObjects(_type)) {
                    SessionState.SetString(string.Format(SaveFormat, AssetDatabase.GetAssetPath(_object)), EditorJsonUtility.ToJson(_object));
                }
            }
        }

        private static void DeserializeObjects() {
            foreach (Type _type in GetObjectTypes()) {
                foreach (ScriptableObject _object in LoadObjects(_type)) {
                    string _json = SessionState.GetString(string.Format(SaveFormat, AssetDatabase.GetAssetPath(_object)), string.Empty);

                    if (!string.IsNullOrEmpty(_json)) {
                        EditorJsonUtility.FromJsonOverwrite(_json, _object);
                    }
                }
            }
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
