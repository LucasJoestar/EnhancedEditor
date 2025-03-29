// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;
using UnityEngine.Rendering;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Mesh"/> class.
    /// </summary>
    public static class MeshExtensions {
        #region Content
        /// <summary>
        /// Get the index of this <see cref="Mesh"/> corresponding sub mesh at a given triangle index.
        /// </summary>
        /// <param name="_mesh"><see cref="Mesh"/> to get the corresponding sub mesh.</param>
        /// <param name="_triangleIndex">Index of the triangle to get the associated sub mesh.</param>
        /// <returns>Index of the sub mesh at the given triangle index (-1 if none).</returns>
        public static int GetSubMeshIndex(this Mesh _mesh, int _triangleIndex) {

            if (!_mesh.isReadable) {
                _mesh.LogWarningMessage("Mesh needs to be set as Read/Write Enabled in Import Settings to get its sub mesh info");
                return -1;
            }

            if (_triangleIndex == -1)
                return -1;

            int _count = 0;
            for (int i = 0; i < _mesh.subMeshCount; i++) {

                SubMeshDescriptor _subMesh = _mesh.GetSubMesh(i);
                _count += _subMesh.indexCount / 3;

                if (_triangleIndex < _count) {
                    return i;
                }
            }

            return -1;
        }
        #endregion
    }
}
