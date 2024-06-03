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
    /// Multiple extension methods related to the <see cref="int"/> value type.
    /// </summary>
    public static class IntExtensions {
        #region Content
        /// <summary>
        /// Creates an uniform <see cref="Vector2"/> with this <see cref="int"/> value.
        /// </summary>
        /// <param name="_int">Value used to create a new <see cref="Vector2"/>.</param>
        /// <returns>Created <see cref="Vector2"/> of this float size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this int _int) {
            return new Vector2(_int, _int);
        }

        /// <summary>
        /// Creates an uniform <see cref="Vector3"/> with this <see cref="int"/> value.
        /// </summary>
        /// <param name="_int">Value used to create a new <see cref="Vector3"/>.</param>
        /// <returns>Created <see cref="Vector3"/> of this float size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this int _int) {
            return new Vector3(_int, _int, _int);
        }

        /// <summary>
        /// Creates an uniform <see cref="Vector2Int"/> with this <see cref="int"/> value.
        /// </summary>
        /// <param name="_int">Value used to create a new <see cref="Vector2Int"/>.</param>
        /// <returns>Created <see cref="Vector2Int"/> of this float size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ToVector2Int(this int _int) {
            return new Vector2Int(_int, _int);
        }

        /// <summary>
        /// Creates an uniform <see cref="Vector3Int"/> with this <see cref="int"/> value.
        /// </summary>
        /// <param name="_int">Value used to create a new <see cref="Vector3Int"/>.</param>
        /// <returns>Created <see cref="Vector3Int"/> of this float size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVector3Int(this int _int) {
            return new Vector3Int(_int, _int, _int);
        }

        /// <summary>
        /// Parse a specific <see cref="int"/> into a string with a specific amount of decimals.
        /// </summary>
        /// <param name="_int"><see cref="int"/> value to parse.</param>
        /// <param name="_decimals">Total amount of decimals to show.</param>
        /// <returns>This <see cref="int"/> value parsed into string.</returns>
        public static string ToStringX(this int _int, int _decimals) {
            StringBuilder _builder = new StringBuilder("");
            for (int i = 0; i < _decimals; i++) {
                _builder.Append('0');
            }

            return _int.ToString(_builder.ToString());
        }
        #endregion
    }
}
