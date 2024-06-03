// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using System.Text;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this float _float) {
            return new Vector2(_float, _float);
        }

        /// <summary>
        /// Creates an uniform <see cref="Vector3"/> with this <see cref="float"/> value.
        /// </summary>
        /// <param name="_float">Value used to create a new <see cref="Vector3"/>.</param>
        /// <returns>Created <see cref="Vector3"/> of this float size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this float _float) {
            return new Vector3(_float, _float, _float);
        }

        /// <summary>
        /// Parse a specific <see cref="float"/> into a string with a specific amount of decimals.
        /// </summary>
        /// <param name="_float"><see cref="float"/> value to parse.</param>
        /// <param name="_decimals">Total amount of decimals to show.</param>
        /// <returns>This <see cref="float"/> value parsed into string.</returns>
        public static string ToStringX(this float _float, int _decimals) {
            StringBuilder _builder = new StringBuilder("");
            for (int i = 0; i < _decimals; i++) {
                _builder.Append('0');
            }

            return _float.ToString(_builder.ToString());
        }
        #endregion
    }
}
