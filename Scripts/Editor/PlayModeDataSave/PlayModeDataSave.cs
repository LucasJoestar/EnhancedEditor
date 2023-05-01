// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Static utility class used to save <see cref="Object"/> data in play mode and load them in edit mode.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeDataSave {
        /// <summary>
        /// <see cref="PlayModeDataSave"/> global data wrapper.
        /// </summary>
        [Serializable]
        private class PlayModeData {
            #region Global Members
            /// <summary>
            /// All stored data.
            /// </summary>
            [SerializeField] public List<PlayModeObjectData> Data = new List<PlayModeObjectData>();

            /// <summary>
            /// Total count of loaded data.
            /// </summary>
            public int Count {
                get { return Data.Count; }
            }
            #endregion

            #region Utility
            /// <summary>
            /// Adds a new object data.
            /// </summary>
            public void Save(PlayModeObjectData _data) {

                _data = new PlayModeObjectData(_data);
                int _index = Data.FindIndex(d => d.objectID == _data.objectID);

                if (_index != -1) {
                    Data[_index] = _data;
                } else {
                    Data.Add(_data);
                }
            }

            // -------------------------------------------
            // Load
            // -------------------------------------------

            /// <summary>
            /// Loads and removes a specific object data.
            /// </summary>
            public bool LoadObject(Object _object) {

                try {
                    Undo.RecordObject(_object, "Load Saved Data");

                    if (Contains(_object, out int _index) && Data[_index].Load(_object)) {

                        if (!Application.isPlaying) {
                            EditorUtility.SetDirty(_object);
                        }

                        Data.RemoveAt(_index);
                        SaveSessionData();

                        return true;
                    }
                } catch (Exception e) {
                    Debug.LogException(e);
                }

                return false;
            }

            /// <summary>
            /// Loads all loaded objects data.
            /// </summary>
            public void LoadAllObjects() {

                for (int i = Count; i-- > 0;) {

                    if (Data[i].Load()) {
                        Data.RemoveAt(i);
                    }
                }

                SaveSessionData();
            }

            // -------------------------------------------
            // Other
            // -------------------------------------------

            /// <summary>
            /// Get if a specific object data is stored.
            /// </summary>
            public bool Contains(Object _object, out int _index) {
                string _id = GlobalObjectId.GetGlobalObjectIdSlow(_object).ToString();
                _index = Data.FindIndex(d => {

                    if (d.objectID == _id) {
                        return true;
                    }

                    if (_object is Component _component) {

                        GameObject _gameObject = _component.gameObject;
                        return (_gameObject.scene.path == d.objectScenePath) && (_gameObject.name == d.objectName) && (_object.GetType().GetReflectionName() == d.objectType);
                    }

                    return false;
                });

                return _index != -1;
            }

            /// <summary>
            /// Refresh datas.
            /// </summary>
            internal void RefreshData() {

                for (int i = Count; i-- > 0;) {

                    PlayModeObjectData _data = Data[i];
                    EditorJsonUtility.FromJsonOverwrite(_data.data, _data);

                    Type _type = Type.GetType(_data.type);

                    if (_type != _data.GetType()) {

                        string _json = _data.data;

                        _data = Activator.CreateInstance(_type) as PlayModeObjectData;
                        EditorJsonUtility.FromJsonOverwrite(_json, _data);

                        _data.data = _json;
                        Data[i] = _data;
                    }
                }
            }
            #endregion
        }

        #region Global Members
        private const string SessionKey = "PlayModeDataSave";
        private const string SessionDialogKey = "PlayModeDataSaveDialog";

        private static readonly PlayModeData data = new PlayModeData();

        // -----------------------

        static PlayModeDataSave() {

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneLoaded;

            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                OnPlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
            }
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        private static void OnPlayModeStateChanged(PlayModeStateChange _mode) {

            string _json;
            switch (_mode) {

                // Load.
                case PlayModeStateChange.EnteredEditMode:

                    _json = SessionState.GetString(SessionKey, string.Empty);
                    if (!string.IsNullOrEmpty(_json)) {
                        EditorJsonUtility.FromJsonOverwrite(_json, data);
                    }

                    data.RefreshData();
                    DelayedLoadObjectsData();

                    break;

                // Save.
                case PlayModeStateChange.ExitingPlayMode:
                    SaveSessionData();
                    break;


                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                default:
                    break;
            }
        }

        private static void OnSceneLoaded(Scene _scene, OpenSceneMode _mode) {

            if (!EditorApplication.isPlayingOrWillChangePlaymode && (_mode != OpenSceneMode.AdditiveWithoutLoading)) {
                DelayedLoadObjectsData();
            }
        }

        private static void DelayedLoadObjectsData() {

            EditorApplication.delayCall -= LoadObjectsData;
            EditorApplication.delayCall += LoadObjectsData;
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Loads all data from active objects.
        /// </summary>
        internal static void LoadObjectsData() {

            if ((data.Count == 0) || !SessionState.GetBool(SessionDialogKey, true)) {
                return;
            }

            if (!EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisSession, SessionDialogKey)) {
                return;
            }

            int _result = EditorUtility.DisplayDialogComplex("Load Play Mode data", "One or more object(s) data were saved during Play Mode.\n\n" +
                                                             "Would you like to load them and restore their saved value(s)?",
                                                             "Yes please", "No - Don't Ask Me Again", "No thanks");

            switch (_result) {

                // Load.
                case 0:
                    data.LoadAllObjects();
                    break;

                // Clear.
                case 1:
                    SessionState.SetBool(SessionDialogKey, false);
                    break;

                // Cancel.
                case 2:
                default:
                    break;
            }
        }

        /// <summary>
        /// Saves all session data.
        /// </summary>
        internal static void SaveSessionData() {
            string _json = EditorJsonUtility.ToJson(data);

            SessionState.SetString(SessionKey, _json);
            SessionState.SetBool(SessionDialogKey, true);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="SaveData(Object, string)"/>
        public static void SaveData(PlayModeObjectData _object) {
            data.Save(_object);
        }

        /// <summary>
        /// Loads a specific <see cref="Object"/> data.
        /// </summary>
        /// <param name="_object"><see cref="Object"/> instance to load associated data.</param>
        /// <returns>True if this object data could be successfully loaded, false otherwise.</returns>
        public static bool LoadData(Object _object) {
            return data.LoadObject(_object);
        }

        /// <summary>
        /// Get if a specific <see cref="Object"/> data is registered.
        /// </summary>
        /// <param name="_object"><see cref="Object"/> instance to check for data.</param>
        /// <returns>True if this object data are currently stored, false otherwise.</returns>
        public static bool Contain(Object _object) {
            return data.Contains(_object, out _);
        }
        #endregion
    }
}
