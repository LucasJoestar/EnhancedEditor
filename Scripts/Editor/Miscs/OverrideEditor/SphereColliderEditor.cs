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
    /// Custom <see cref="SphereCollider"/> editor, adding save and load utilities.
    /// </summary>
    [CustomEditor(typeof(SphereCollider), true), CanEditMultipleObjects]
    public class SphereColliderEditor : UnityObjectEditor {
        #region Data
        /// <summary>
        /// Serializable <see cref="SphereCollider"/> data.
        /// </summary>
        [Serializable]
        private class Data : PlayModeObjectData {
            public Vector3 Center;
            public float Radius;
            public bool IsTrigger;

            // -----------------------

            public Data() : base() { }

            // -----------------------

            public override void Save(Object _object) {

                if (_object is SphereCollider _collider) {

                    Center = _collider.center;
                    Radius = _collider.radius;
                    IsTrigger = _collider.isTrigger;
                }

                base.Save(_object);
            }

            public override bool Load(Object _object) {

                if (_object is SphereCollider _collider) {

                    _collider.center = Center;
                    _collider.radius = Radius;
                    _collider.isTrigger = IsTrigger;

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Editor Content
        private static readonly Type editorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SphereColliderEditor");
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
