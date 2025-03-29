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
    public sealed class CapsuleColliderEditor : UnityObjectEditor {
        #region Data
        /// <summary>
        /// Serializable <see cref="CapsuleCollider"/> data.
        /// </summary>
        [Serializable]
        private sealed class Data : PlayModeObjectData {
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
        private static readonly GUIContent resetCenterGUI = new GUIContent(" Adjust Center", "Adjust this collider position and reset its center.");
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

            // Adjust center button.
            if ((target is CapsuleCollider _collider) && (_collider.center != Vector3.zero)) {
                Rect position = EditorGUILayout.GetControlRect(true, 20f);
                position.xMin = position.xMax - SaveValueButtonWidth;

                if (EnhancedEditorGUI.IconDropShadowButton(position, resetCenterGUI)) {

                    foreach (Object _target in targets) {
                        if (_target is CapsuleCollider _capsule) {
                            SceneDesignerUtility.AdjustCapsuleCenter(_capsule);
                        }
                    }
                }
            }

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
