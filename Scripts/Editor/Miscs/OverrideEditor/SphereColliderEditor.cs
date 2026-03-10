// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="SphereCollider"/> editor, adding save and load utilities.
    /// </summary>
    [CustomEditor(typeof(SphereCollider), true), CanEditMultipleObjects]
    public sealed class SphereColliderEditor : ColliderEditor {
        /// <summary>
        /// Serializable <see cref="SphereCollider"/> data.
        /// </summary>
        [Serializable]
        private sealed class Data : PlayModeObjectData {
            #region Content
            public Vector3 Center;
            public float Radius;
            public bool IsTrigger;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public Data() : base() { }

            // -------------------------------------------
            // Utility
            // -------------------------------------------

            public override void Save(Object _object) {

                if (_object is SphereCollider _collider) {

                    IsTrigger = _collider.isTrigger;
                    Center    = _collider.center;
                    Radius    = _collider.radius;
                }

                base.Save(_object);
            }

            public override bool Load(Object _object) {

                if (_object is SphereCollider _collider) {

                    _collider.isTrigger = IsTrigger;
                    _collider.center    = Center;
                    _collider.radius    = Radius;

                    return true;
                }

                return false;
            }
            #endregion
        }

        #region Editor Content
        private static readonly Data data = new Data();

        protected override bool CanSaveData {
            get { return true; }
        }

        // -----------------------

        public override void OnInspectorGUI() {

            // Save load properties.
            SaveLoadButtonGUILayout();

            // Adjust center button.
            if ((target is SphereCollider _collider) && (_collider.center != Vector3.zero)) {

                Rect _position = EditorGUILayout.GetControlRect(true, 20f);
                _position.xMin = _position.xMax - SaveValueButtonWidth;

                if (EnhancedEditorGUI.IconDropShadowButton(_position, resetCenterGUI)) {
                    Object[] _targets = targets;

                    for (int i = _targets.Length; i-- > 0;) {
                        if (_targets[i] is SphereCollider _sphere) {
                            SceneDesignerUtility.AdjustSphereCenter(_sphere);
                        }
                    }
                }
            }

            base.OnInspectorGUI();
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
