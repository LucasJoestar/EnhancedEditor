// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to various collection classes.
    /// </summary>
    public static class CollectionExtensions {
        #region Random
        private const int RandomMaxIteration = 5;

        // -----------------------

        /// <summary>
        /// Get a random element from this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get random element from.</param>
        /// <returns>Random element from this array.</returns>
        public static T Random<T>(this T[] _array) {
            if (_array.Length == 0) {
                return default;
            }

            return _array[UnityEngine.Random.Range(0, _array.Length)];
        }

        /// <summary>
        /// Get a random element from this list.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_list">List to get random element from.</param>
        /// <returns>Random element from this list.</returns>
        public static T Random<T>(this List<T> _list) {
            if (_list.Count == 0) {
                return default;
            }

            return _list[UnityEngine.Random.Range(0, _list.Count)];
        }

        /// <summary>
        /// Get a random element from this collection.
        /// </summary>
        /// <typeparam name="T">Collection content type.</typeparam>
        /// <param name="_collection">Collection to get random element from.</param>
        /// <returns>Random element from this collection.</returns>
        public static T Random<T>(this IList<T> _collection) {
            if (_collection.Count == 0) {
                return default;
            }

            return _collection[UnityEngine.Random.Range(0, _collection.Count)];
        }

        /// <summary>
        /// Get a random element from this collection.
        /// </summary>
        /// <typeparam name="T">Collection content type.</typeparam>
        /// <param name="_collection">Collection to get random element from.</param>
        /// <param name="_lastRandomIndex">Index of the last random element (used to avoid getting the same).</param>
        /// <returns>Random element from this collection.</returns>
        public static T Random<T>(this IList<T> _collection, ref int _lastRandomIndex) {
            if (_collection.Count == 0) {
                return default;
            }

            if (_collection.Count == 1) {
                return _collection[0];
            }

            int _iteration = RandomMaxIteration;
            int _index;

            do {
                _index = UnityEngine.Random.Range(0, _collection.Count);
            } while ((_index == _lastRandomIndex) && (_iteration-- != 0));

            _lastRandomIndex = _index;
            return _collection[_index];
        }

        /// <summary>
        /// Shuffles this collection content.
        /// </summary>
        public static IList<T> Shuffle<T>(this IList<T> _collection) {
            int _count = _collection.Count;

            for (int i = 0; i < _count - 1; i++) {
                int _index = UnityEngine.Random.Range(i, _count);

                if (_index != i) {
                    (_collection[i], _collection[_index]) = (_collection[_index], _collection[i]);
                }
            }

            return _collection;
        }
        #endregion

        #region Reference
        /// <summary>
        /// Get the index of a specific element from this collection.
        /// <br/> Performs an equality comparison.
        /// <para/>
        /// Always use this to get the index of an interface instance, as the default comparer may return true for a different instance.
        /// </summary>
        /// <typeparam name="T">This collection content type.</typeparam>
        /// <param name="_list">List to get the instance index from.</param>
        /// <param name="_element">Instance to get index.</param>
        /// <returns>True if the instance could be successfully retrieved along with its index, false otherwise.</returns>
        public static int IndexOfInstance<T>(this List<T> _list, T _element) where T : class {

            int _count = _list.Count;
            if (_count != 0) {

                for (int i = 0; i < _count; i++) {
                    if (EqualityUtility.Equals(_list[i], _element))
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes a specific reference instance from a list.
        /// <br/>Performs an equality comparison.
        /// <para/>
        /// Always use this to remove an interface instance, as the default comparer may return true for a different instance.
        /// </summary>
        /// <typeparam name="T">This collection content type.</typeparam>
        /// <param name="_list">List to remove the instance from.</param>
        /// <param name="_element">Instance to remove from list.</param>
        /// <returns>True if the instance could be successfully removed, false otherwise.</returns>
        public static bool RemoveInstance<T>(this List<T> _list, T _element) where T : class {
            int _index = IndexOfInstance(_list, _element);

            if (_index == -1) {
                return false;
            }

            _list.RemoveAt(_index);
            return true;
        }

        /// <summary>
        /// Get if a specific instance is contained within this collection.
        /// <br/> Performs an equality comparison.
        /// <para/>
        /// Always use this to check the presence of a an interface instance, as the default comparer may return true for a different instance.
        /// </summary>
        /// <typeparam name="T">This collection content type.</typeparam>
        /// <param name="_list">List to check content.</param>
        /// <param name="_element">Instance to check.</param>
        /// <returns>True if the instance is contained within the collection, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsInstance<T>(this List<T> _list, T _element) where T : class {
            return IndexOfInstance(_list, _element) != -1;
        }
        #endregion

        #region Enumeration
        // -------------------------------------------
        // Array
        // -------------------------------------------

        /// <summary>
        /// Get the first element from this array.
        /// </summary>
        /// <typeparam name="T">Array content type.</typeparam>
        /// <param name="_array">Array to get first element from.</param>
        /// <returns>First element from this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // -------------------------------------------
        // List
        // -------------------------------------------

        /// <summary>
        /// Get the first element from this list.
        /// </summary>
        /// <typeparam name="T">List content type.</typeparam>
        /// <param name="_array">List to get first element from.</param>
        /// <returns>First element from this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Last<T>(this List<T> _list) {
            return _list[_list.Count - 1];
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
            int _count = _list.Count;

            if (_count == 0)
                return false;

            int _index = _count - 1;
            _list.RemoveAt(_index);

            return true;
        }

        /// <summary>
        /// Clears all null reference(s) of this list.
        /// </summary>
        public static void RemoveNulls<T>(this List<T> _list) {
            for (int i = _list.Count; i-- > 0;) {
                if ((_list[i] == null) || _list[i].Equals(null)) {
                    _list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Shifts an element of a list at a specific index to a new index, looping at the list bounds (last become first, and vice-versa).
        /// </summary>
        /// <param name="_list">The list to shift elements from.</param>
        /// <param name="_oldIndex">Current index of the element to shift.</param>
        /// <param name="_newIndex">New destination index of the element to shift.</param>
        public static void ShiftLoopElement<T>(this List<T> _list, int _oldIndex, int _newIndex) {
            if ((_oldIndex == _newIndex) || (_newIndex < 0) || (_newIndex >= _list.Count) || (_list.Count == 0)) {
                return;
            }
            
            int _count = _newIndex - _oldIndex;

            if (_count > 0) {

                for (int i = 0; i < _count; i++) {
                    ShiftRight();
                };

            } else {

                for (int i = 0; i < _count; i++) {
                    ShiftLeft();
                };
            }

            // ----- Local Methods ----- \\

            void ShiftLeft() {
                var _tmp = _list.First();
                for (int i = 1; i < _list.Count; i++) {
                    _list[i - 1] = _list[i];
                }

                _list[_list.Count - 1] = _tmp;
            }

            void ShiftRight() {
                var _tmp = _list.Last();
                for (int i = _list.Count; i-- > 1;) {
                    _list[i] = _list[i - 1];
                }

                _list[0] = _tmp;
            }
        }

        /// <summary>
        /// Adds a given element to a list only if it does not already contain it.
        /// </summary>
        /// <param name="_list">The list to add an element into.</param>
        /// <param name="_element">The element to try to add.</param>
        /// <returns>True if the element could be added, false otherwise.</returns>
        public static bool AddIfNotExists<T>(this List<T> _list, T _element) {
            if (_list.Contains(_element)) {
                return false;
            }

            _list.Add(_element);
            return true;
        }
        #endregion

        #region Maths
        /// <summary>
        /// Calculates the average value of a given array.
        /// </summary>
        /// <returns>Average value of this array.</returns>
        public static float Average(this float[] _array) {
            float _value = 0f;
            int _length  = _array.Length;

            if (_length == 0)
                return _value;

            for (int i = 0; i < _length; i++) {
                _value += _array[i];
            }

            return _value / _length;
        }

        /// <summary>
        /// Calculates the average value of a given list.
        /// </summary>
        /// <returns>Average value of this list.</returns>
        public static float Average(this List<float> _list) {
            float _value = 0f;
            int _count   = _list.Count;

            if (_count == 0)
                return _value;

            for (int i = 0; i < _count; i++) {
                _value += _list[i];
            }

            return _value / _count;
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
