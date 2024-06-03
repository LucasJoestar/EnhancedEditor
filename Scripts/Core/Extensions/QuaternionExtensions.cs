// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Quaternion"/> struct.
    /// </summary>
    public static class QuaternionExtensions {
        #region Content
        /// <summary>
        /// Converts this quaternion to its direction <see cref="Vector3"/>.
        /// </summary>
        /// <param name="_quaternion">Quaternion to convert.</param>
        /// <returns><see cref="Vector3"/> looking to this quaternion rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToDirection(this Quaternion _quaternion) {
            return _quaternion * Vector3.forward;
        }
        #endregion
    }
}
