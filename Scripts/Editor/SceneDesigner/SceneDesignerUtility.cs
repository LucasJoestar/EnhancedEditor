// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Scene Designer related editor utility class, used to perform various level-design operations.
    /// </summary>
    public static class SceneDesignerUtility {
        #region Collider
        public static void AdjustCapsuleCenter(CapsuleCollider _capsule) {
            if (_capsule.center == Vector3.zero)
                return;

            const string RecordName = "Reset Sphere Center";

            Undo.RecordObject(_capsule.transform, RecordName);
            Undo.RecordObject(_capsule, RecordName);

            _capsule.transform.position = _capsule.bounds.center;
            _capsule.center = Vector3.zero;

            EditorUtility.SetDirty(_capsule);
        }

        public static void AdjustSphereCenter(SphereCollider _sphere) {
            if (_sphere.center == Vector3.zero)
                return;

            const string RecordName = "Reset Sphere Center";

            Undo.RecordObject(_sphere.transform, RecordName);
            Undo.RecordObject(_sphere, RecordName);

            _sphere.transform.position = _sphere.bounds.center;
            _sphere.center = Vector3.zero;

            EditorUtility.SetDirty(_sphere);
        }

        public static void AdjustBoxCenter(BoxCollider _box) {
            if (_box.center == Vector3.zero)
                return;

            const string RecordName = "Reset Sphere Center";

            Undo.RecordObject(_box.transform, RecordName);
            Undo.RecordObject(_box, RecordName);

            _box.transform.position = _box.bounds.center;
            _box.center = Vector3.zero;

            EditorUtility.SetDirty(_box);
        }
        #endregion
    }
}
