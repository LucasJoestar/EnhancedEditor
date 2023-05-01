// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="BoxCollider"/> editor, adding save and load utilities.
    /// </summary>
    [CustomEditor(typeof(BoxCollider), true), CanEditMultipleObjects]
    public class BoxColliderEditor : UnityObjectEditor {
        #region Data
        /// <summary>
        /// Serializable <see cref="BoxCollider"/> data.
        /// </summary>
        [Serializable]
        private class Data : PlayModeObjectData {
            [SerializeField] public Vector3 Center  = Vector3.zero;
            [SerializeField] public Vector3 Size    = Vector3.one;
            [SerializeField] public bool IsTrigger  = false;

            // -----------------------

            public Data() : base() { }

            // -----------------------

            public override void Save(Object _object) {

                if (_object is BoxCollider _collider) {

                    Center = _collider.center;
                    Size = _collider.size;
                    IsTrigger = _collider.isTrigger;
                }

                base.Save(_object);
            }

            public override bool Load(Object _object) {

                if (_object is BoxCollider _collider) {

                    _collider.center = Center;
                    _collider.size = Size;
                    _collider.isTrigger = IsTrigger;

                    return true;
                }


                return false;
            }
        }
        #endregion

        #region Editor Content
        private static readonly Type editorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.BoxColliderEditor");
        private static Data data = new Data();

        private UnityEditor.Editor colliderEditor = null;

        protected override bool CanSaveData {
            get { return true; }
        }

        // -----------------------

        protected override void OnEnable() {
            base.OnEnable();

            colliderEditor = CreateEditor(serializedObject.targetObjects, editorType);
        }

        public override void OnInspectorGUI() {

            // Save load properties.
            SaveLoadButtonGUILayout();
            colliderEditor.OnInspectorGUI();
        }

        protected override void OnDisable() {
            base.OnDisable();

            // Avoid memory leak.
            DestroyImmediate(colliderEditor);
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        protected override PlayModeObjectData SaveData(Object _object) {

            data.Save(_object);
            return data;
        }
        #endregion
    }
}
