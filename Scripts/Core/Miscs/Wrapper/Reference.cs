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
    /// Value reference wrapper using the <see cref="SerializeReference"/> attribute.
    /// </summary>
    /// <typeparam name="T">The reference value type of this wrapper.</typeparam>
    [Serializable]
    public struct Reference<T> : IComparable<Reference<T>> {
        #region Global Members
        /// <summary>
        /// The value referenced by this wrapper.
        /// </summary>
        [SerializeReference] public T Value;

        // -----------------------

        /// <param name="_value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <inheritdoc cref="Reference{T}"/>
        public Reference(T _value) {
            Value = _value;
        }
        #endregion

        #region Operator
        public static implicit operator T(Reference<T> _reference) {
            return _reference.Value;
        }

        public static implicit operator Reference<T>(T _value) {
            return new Reference<T>(_value);
        }
        #endregion

        #region Comparer
        public int CompareTo(Reference<T> _other) {
            return Comparer<T>.Default.Compare(Value, _other.Value);
        }
        #endregion
    }
}
