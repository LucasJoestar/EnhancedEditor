// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Pair struct, used to associate two values together.
    /// </summary>
    /// <typeparam name="T">Type of the first pair value.</typeparam>
    /// <typeparam name="U">Type of the second pair value.</typeparam>
    [Serializable]
    public struct Pair<T, U> : IComparable<Pair<T, U>> {
        #region Global Members
        /// <summary>
        /// First pair value.
        /// </summary>
        [SerializeField] public T First;

        /// <summary>
        /// Second pair value.
        /// </summary>
        [SerializeField] public U Second;

        // -----------------------

        /// <summary>
        /// <inheritdoc cref="Pair{T, U}"/>
        /// </summary>
        /// <param name="_first"><inheritdoc cref="First" path="/summary"/></param>
        /// <param name="_second"><inheritdoc cref="Second" path="/summary"/></param>
        public Pair(T _first, U _second) {
            First = _first;
            Second = _second;
        }
        #endregion

        #region Operator
        public static implicit operator Pair<T, U>(KeyValuePair<T, U> _pair) {
            return new Pair<T, U>(_pair.Key, _pair.Value);
        }
        #endregion

        #region Comparison
        int IComparable<Pair<T, U>>.CompareTo(Pair<T, U> _other) {
            if (First is IComparable<T> _comparerT) {
                return _comparerT.CompareTo(_other.First);
            }

            if (Second is IComparable<U> _comparerU) {
                return _comparerU.CompareTo(_other.Second);
            }

            return 0;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Set this pair first value.
        /// </summary>
        /// <param name="_first">This pair new first value.</param>
        /// <returns>This pair instance.</returns>
        public Pair<T, U> SetFirst(T _first) {
            First = _first;
            return this;
        }

        /// <summary>
        /// Set this pair second value.
        /// </summary>
        /// <param name="_first">This pair new second value.</param>
        /// <returns>This pair instance.</returns>
        public Pair<T, U> SetSecond(U _second) {
            Second = _second;
            return this;
        }
        #endregion
    }
}
