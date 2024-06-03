// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnhancedEditor {
    /// <summary>
    /// Utility comparer used to check if two objects are equal.
    /// </summary>
    /// <typeparam name="T">Object type to compare.</typeparam>
    public class EnhancedEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer {
        #region Global Members
        /// <summary>
        /// Static comparer instance of this type.
        /// </summary>
        public static readonly EnhancedEqualityComparer<T> Default = new EnhancedEqualityComparer<T>();

        /// <summary>
        /// Whether this object type should be compared using reference equality or not.
        /// </summary>
        public static readonly bool UseReferenceEquality = EqualityUtility.ShouldUseReferenceEquality<T>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances of this class.
        /// </summary>
        private EnhancedEqualityComparer() { }
        #endregion

        #region Equality
        // -------------------------------------------
        // Equality
        // -------------------------------------------

        /// <summary>
        /// Compares if two objects are equal.
        /// <br/> This advanced method can use reference comparison if the object type suggest it (like for interfaces).
        /// </summary>
        /// <param name="x">First element to compare (order has no influence whatsoever).</param>
        /// <param name="y">Second element to compare (order has no influence whatsoever).</param>
        /// <param name="compareReferenceIfAvailable">Whether to use reference comparison if the object type suggest it (like for interfaces).</param>
        /// <returns>True if those two elements are equal, false otherwise.</returns>
        public bool EnhancedEquals(T x, T y, bool compareReferenceIfAvailable = true) {

            if (compareReferenceIfAvailable && UseReferenceEquality) {
                return ReferenceEquals(x, y);
            }

            return Equals(x, y);
        }

        /// <summary>
        /// Compares if two objects are equal, using a reference comparison.
        /// </summary>
        /// <inheritdoc cref="EnhancedEquals"/>
        public bool ReferenceEquals(T x, T y) {
            return object.ReferenceEquals(x, y);
        }

        /// <summary>
        /// Compares if two objects are equal.
        /// </summary>
        /// <inheritdoc cref="EnhancedEquals"/>
        public bool Equals(T x, T y) {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        /// <inheritdoc cref="Equals(T, T)"/>
        public new bool Equals(object x, object y) {
            if ((x is T tX) && (y is T tY)) {
                return Equals(tX, tY);
            }

            return object.Equals(x, y);
        }

        // -------------------------------------------
        // Hash Code
        // -------------------------------------------

        public int GetHashCode(T _object) {
            //return _object.GetHashCode();
            //return RuntimeHelpers.GetHashCode(_object);

            return EqualityComparer<T>.Default.GetHashCode(_object);
        }

        public int GetHashCode(object _object) {

            if (_object is T _tObject) {
                return GetHashCode(_tObject);
            }

            return RuntimeHelpers.GetHashCode(_object);
        }
        #endregion
    }

    /// <summary>
    /// Contains multiple equality comparison utilities.
    /// </summary>
    public static class EqualityUtility {
        #region Content
        /// <summary>
        /// Compares if two elements are equal.
        /// </summary>
        /// <typeparam name="U">Object type to compare.</typeparam>
        /// <inheritdoc cref="EnhancedEqualityComparer{T}.EnhancedEquals(T, T, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<U>(U x, U y, bool compareReferenceIfAvailable = true) {
            return EnhancedEqualityComparer<U>.Default.EnhancedEquals(x, y, compareReferenceIfAvailable);
        }

        /// <summary>
        /// Whether a given type should be compared using reference equality or not.
        /// </summary>
        /// <typeparam name="U">Object type to check.</typeparam>
        /// <returns>True if this type should be compared using reference equality, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldUseReferenceEquality<U>() {
            return typeof(U).IsInterface;
        }
        #endregion
    }
}
