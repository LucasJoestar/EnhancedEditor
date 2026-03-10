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
    /// Custom <see cref="BoxCollider"/> editor, adding save and load utilities.
    /// </summary>
    [CustomEditor(typeof(BoxCollider), true), CanEditMultipleObjects]
    public sealed class BoxColliderEditor : ColliderEditor {
        /// <summary>
        /// Serializable <see cref="BoxCollider"/> data.
        /// </summary>
        [Serializable]
        private sealed class Data : PlayModeObjectData {
            #region Content
            [SerializeField] public Vector3 Center  = Vector3.zero;
            [SerializeField] public Vector3 Size    = Vector3.one;
            [SerializeField] public bool IsTrigger  = false;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public Data() : base() { }

            // -------------------------------------------
            // Utility
            // -------------------------------------------

            public override void Save(Object _object) {

                if (_object is BoxCollider _collider) {

                    IsTrigger = _collider.isTrigger;
                    Center    = _collider.center;
                    Size      = _collider.size;
                }

                base.Save(_object);
            }

            public override bool Load(Object _object) {

                if (_object is BoxCollider _collider) {

                    _collider.isTrigger = IsTrigger;
                    _collider.center    = Center;
                    _collider.size  = Size;

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
            if ((target is BoxCollider _collider) && (_collider.center != Vector3.zero)) {

                Rect _position = EditorGUILayout.GetControlRect(true, 20f);
                _position.xMin = _position.xMax - SaveValueButtonWidth;

                if (EnhancedEditorGUI.IconDropShadowButton(_position, resetCenterGUI)) {
                    Object[] _targets = targets;

                    for (int i = _targets.Length; i-- > 0;) {
                        if (_targets[i] is BoxCollider _box) {
                            SceneDesignerUtility.AdjustBoxCenter(_box);
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
