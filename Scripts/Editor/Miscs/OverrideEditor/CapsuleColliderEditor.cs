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
    /// Custom <see cref="CapsuleCollider"/> editor, adding save and load utilities.
    /// </summary>
    [CustomEditor(typeof(CapsuleCollider), true), CanEditMultipleObjects]
    public class CapsuleColliderEditor : UnityObjectEditor {
        #region Data
        /// <summary>
        /// Serializable <see cref="CapsuleCollider"/> data.
        /// </summary>
        [Serializable]
        private class Data : PlayModeObjectData {
            public Vector3 Center;
            public float Height;
            public int Direction;
            public bool IsTrigger;

            // -----------------------

            public Data() : base() { }

            // -----------------------

            public override void Save(Object _object) {

                if (_object is CapsuleCollider _collider) {

                    Center = _collider.center;
                    Height = _collider.height;
                    Direction = _collider.direction;
                    IsTrigger = _collider.isTrigger;
                }

                base.Save(_object);
            }

            public override bool Load(Object _object) {

                if (_object is CapsuleCollider _collider) {

                    _collider.center = Center;
                    _collider.height = Height;
                    _collider.direction = Direction;
                    _collider.isTrigger = IsTrigger;

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Editor Content
        private static readonly Type editorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.CapsuleColliderEditor");
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
