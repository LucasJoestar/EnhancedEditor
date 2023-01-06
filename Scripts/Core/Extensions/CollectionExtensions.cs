// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Collections.Generic;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to various collection classes.
    /// </summary>
    public static class CollectionExtensions {
        #region Random
        /// <summary>
        /// Get a random element from this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get random element from.</param>
        /// <returns>Random element from this array.</returns>
        public static T Random<T>(this T[] _array) {
            return _array[UnityEngine.Random.Range(0, _array.Length)];
        }

        /// <summary>
        /// Get a random element from this list.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="list">List to get random element from.</param>
        /// <returns>Random element from this list.</returns>
        public static T Random<T>(this List<T> _list) {
            return _list[UnityEngine.Random.Range(0, _list.Count)];
        }
        #endregion

        #region Enumeration
        /// <summary>
        /// Get the first element from this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get first element from.</param>
        /// <returns>First element from this array.</returns>
        public static T First<T>(this T[] _array) {
            return _array[0];
        }

        /// <summary>
        /// A safe version of <see cref="First{T}(T[])"/>.
        /// <br/> Get the first element from this array or the default value if empty.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get first element from.</param>
        /// <param name="_element">First element from this array or the default value if empty.</param>
        /// <returns>False if this array is empty, true otherwise.</returns>
        public static bool SafeFirst<T>(this T[] _array, out T _element) {
            if (_array.Length == 0) {
                _element = default;
                return false;
            }

            _element = _array.First();
            return true;
        }

        /// <summary>
        /// Get the last element from this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get last element from.</param>
        /// <returns>Last element from this array.</returns>
        public static T Last<T>(this T[] _array) {
            return _array[_array.Length - 1];
        }

        /// <summary>
        /// A safe version of <see cref="Last{T}(T[])"/>.
        /// <br/> Get the last element from this array or the default value if empty.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get last element from.</param>
        /// <param name="_element">Last element from this array or the default value if empty.</param>
        /// <returns>False if this array is empty, true otherwise.</returns>
        public static bool SafeLast<T>(this T[] _array, out T _element) {
            if (_array.Length == 0) {
                _element = default;
                return false;
            }

            _element = _array.Last();
            return true;
        }

        // -----------------------

        /// <summary>
        /// Get the first element from this list.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_array">List to get first element from.</param>
        /// <returns>First element from this list.</returns>
        public static T First<T>(this List<T> _list) {
            return _list[0];
        }

        /// <summary>
        /// A safe version of <see cref="First{T}(List{T})"/>.
        /// <br/> Get the first element from this list or the default value if empty.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_array">List to get first element from.</param>
        /// <param name="_element">First element from this list or the default value if empty.</param>
        /// <returns>False if this list is empty, true otherwise.</returns>
        public static bool SafeFirst<T>(this List<T> _list, out T _element) {
            if (_list.Count == 0) {
                _element = default;
                return false;
            }

            _element = _list.First();
            return true;
        }

        /// <summary>
        /// Get the last element from this list.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_list">List to get last element from.</param>
        /// <returns>Last element from the given list.</returns>
        public static T Last<T>(this List<T> _list) {
            return (_list.Count == 0) ? default : _list[_list.Count - 1];
        }

        /// <summary>
        /// A safe version of <see cref="Last{T}(List{T})"/>.
        /// <br/> Get the last element from this list or the default value if empty.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_list">List to get last element from.</param>
        /// <param name="_element">Last element from this list or the default value if empty.</param>
        /// <returns>False if this list is empty, true otherwise.</returns>
        public static bool SafeLast<T>(this List<T> _list, out T _element) {
            if (_list.Count == 0) {
                _element = default;
                return false;
            }

            _element = _list.Last();
            return true;
        }

        // -----------------------

        /// <summary>
        /// Get the first element of this list.
        /// </summary>
        /// <typeparam name="T">List element type from.</typeparam>
        /// <param name="_list">List to get first element.</param>
        /// <param name="_element">First element of the list (null if none).</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool First<T>(this List<T> _list, out T _element) {
            if (_list.Count == 0) {
                _element = default;
                return false;
            }

            _element = _list[0];
            return true;
        }

        /// <summary>
        /// Get the last element of this list.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to get last element from.</param>
        /// <param name="_element">Last element of the list (null if none).</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool Last<T>(this List<T> _list, out T _element) {
            if (_list.Count == 0) {
                _element = default;
                return false;
            }

            int _index = _list.Count - 1;
            _element = _list[_index];

            return true;
        }
        #endregion

        #region Management
        /// <summary>
        /// Removes the first element of this list.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to remove first element from.</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool RemoveFirst<T>(this List<T> _list) {
            if (_list.Count == 0)
                return false;

            _list.RemoveAt(0);
            return true;
        }

        /// <summary>
        /// Removes the last element of this list.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to remove last element from.</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool RemoveLast<T>(this List<T> _list) {
            if (_list.Count == 0)
                return false;

            int _index = _list.Count - 1;
            _list.RemoveAt(_index);

            return true;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Finds a specific element within this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to find an element from.</param>
        /// <param name="_match">Predicate to find matching element.</param>
        /// <param name="_element">Matching element.</param>
        /// <returns>True if found a matching element, false otherwise.</returns>
        public static bool Find<T>(this T[] _array, Predicate<T> _match, out T _element) {
            foreach (var _arrayElement in _array) {
                _element = _arrayElement;
                if (_match(_element))
                    return true;
            }

            _element = default;
            return false;
        }

        /// <summary>
        /// Changes the number of elements in this list to a specific size.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to resize.</param>
        /// <param name="_size">New list size.</param>
        public static void Resize<T>(this List<T> _list, int _size) {
            int _count = _list.Count;
            if (_count > _size) {
                _list.RemoveRange(_size, _count - _size);
            } else if (_size > _count) {
                if (_size > _list.Capacity)
                    _list.Capacity = _size;

                for (int _i = _count; _i < _size; _i++) {
                    _list.Add(default);
                }
            }
        }
        #endregion
    }
}
