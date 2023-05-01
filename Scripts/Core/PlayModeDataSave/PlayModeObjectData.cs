// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

#if UNITY_EDITOR
#define EDITOR_ID
#endif

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if EDITOR_ID
using UnityEditor;
#endif

using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedEditor {
    /// <summary>
    /// <see cref="PlayModeDataSave"/> single object data wrapper.
    /// </summary>
    [Serializable]
    public class PlayModeObjectData {
        #region Global Members
        /// <summary>
        /// Id of this data associated object.
        /// </summary>
        [SerializeField] internal protected string objectID = string.Empty;

        /// <summary>
        /// Name of this data associated object (in case the object id could not be retrieved).
        /// </summary>
        [SerializeField] internal protected string objectName = string.Empty;

        /// <summary>
        /// Type of this object (in case the object id could not be retrieved).
        /// </summary>
        [SerializeField] internal protected string objectType = string.Empty;

        /// <summary>
        /// Path of this object scene (in case the object id could not be retrieved).
        /// </summary>
        [SerializeField] internal protected string objectScenePath = string.Empty;

        /// <summary>
        /// Saved data of this object.
        /// </summary>
        [SerializeField] internal protected string data = string.Empty;

        /// <summary>
        /// This data class real type name.
        /// </summary>
        [SerializeField] internal protected string type = string.Empty;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="PlayModeObjectData(Object)"/>
        public PlayModeObjectData() {
            type = GetType().GetReflectionName();
        }

        /// <inheritdoc cref="PlayModeObjectData"/>
        public PlayModeObjectData(Object _object) : this() {
            Save(_object);
        }

        /// <inheritdoc cref="PlayModeObjectData"/>
        public PlayModeObjectData(PlayModeObjectData _data) {

            #if EDITOR_ID
            data = EditorJsonUtility.ToJson(_data);
            type = _data.type;

            objectID = _data.objectID;
            objectName = _data.objectName;
            objectType = _data.objectType;
            objectScenePath = _data.objectScenePath;
            #endif
        }
        #endregion

        #region Utility
        /// <summary>
        /// Set this data object.
        /// </summary>
        public virtual void Save(Object _object) {

            #if EDITOR_ID
            if (_object != null) {
                objectID = GlobalObjectId.GetGlobalObjectIdSlow(_object).ToString();

                if (_object is Component _component) {

                    GameObject _gameObject = _component.gameObject;

                    objectName = _gameObject.name;
                    objectType = _object.GetType().GetReflectionName();
                    objectScenePath = _gameObject.scene.path;
                }
            }

            data = EditorJsonUtility.ToJson(this);
            #endif
        }

        /// <summary>
        /// Loads this object data.
        /// </summary>
        /// <param name="_object">Object on which to apply saved data.</param>
        /// <returns>True if these data could be successfully loaded, false otherwise.</returns>
        public virtual bool Load(Object _object) {
            return false;
        }

        /// <inheritdoc cref="Load(Object)"/>
        public bool Load() {

            #if EDITOR_ID
            try {
                if (!GlobalObjectId.TryParse(objectID, out GlobalObjectId _id)) {
                    return false;
                }

                Object _object = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_id);
                if (_object == null) {

                    if (!SceneManager.GetSceneByPath(objectScenePath).isLoaded) {
                        return false;
                    }

                    GameObject _gameObject = GameObject.Find(objectName);

                    if (_gameObject != null) {

                        Type _type = Type.GetType(objectType);
                        _object = _gameObject.GetComponent(_type);
                    }

                    if (_object == null) {
                        return false;
                    }
                }

                Undo.RecordObject(_object, "Load Saved Data");
                return Load(_object);

            } catch (NullReferenceException) {
                return false;
            }
            #else
            return false;
            #endif
        }
        #endregion
    }
}
