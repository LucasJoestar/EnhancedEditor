// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="float"/> value type.
    /// </summary>
    public static class FloatExtensions {
        #region Content
        /// <summary>
        /// Creates an uniform <see cref="Vector2"/> with this <see cref="float"/> value.
        /// </summary>
        /// <param name="_float">Value used to create a new <see cref="Vector2"/>.</param>
        /// <returns>Created <see cref="Vector2"/> of this float size.</returns>
        public static Vector2 ToVector2(this float _float) {
            return new Vector2(_float, _float);
        }

        /// <summary>
        /// Creates an uniform <see cref="Vector3"/> with this <see cref="float"/> value.
        /// </summary>
        /// <param name="_float">Value used to create a new <see cref="Vector3"/>.</param>
        /// <returns>Created <see cref="Vector3"/> of this float size.</returns>
        public static Vector3 ToVector3(this float _float) {
            return new Vector3(_float, _float, _float);
        }
        #endregion
    }
}
