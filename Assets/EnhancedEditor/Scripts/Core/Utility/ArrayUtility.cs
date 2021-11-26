// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor
{
    /// <summary>
    /// Contains multiple <see cref="Array"/>-related utility methods that can be used both at runtime and in editor.
    /// </summary>
	public static class ArrayUtility
    {
        #region Management
        /// <summary>
        /// Adds a new element into an existing array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to add element in.</param>
        /// <param name="_element">Element to add.</param>
        public static void Add<T>(ref T[] _array, T _element)
        {
            int _length = _array.Length;
            T[] _newArray = new T[_length + 1];

            Array.Copy(_array, _newArray, _length);
            _newArray[_length] = _element;

            _array = _newArray;
        }

        /// <summary>
        /// Adds all elements from an array into another array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_source">Source array to add elements in.</param>
        /// <param name="_content">Array to insert content into source.</param>
        public static void AddRange<T>(ref T[] _source, T[] _content)
        {
            int _sourceLength = _source.Length;
            int _contentLength = _content.Length;

            T[] _newArray = new T[_sourceLength + _contentLength];

            Array.Copy(_source, _newArray, _sourceLength);
            Array.Copy(_content, 0, _newArray, _sourceLength, _contentLength);

            _source = _newArray;
        }

        /// <summary>
        /// Inserts a new element into an array at a specific index.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to insert element in.</param>
        /// <param name="_index">Index of the array where to insert the new element.</param>
        /// <param name="_element">Element to insert.</param>
        public static void Insert<T>(ref T[] _array, int _index, T _element)
        {
            int _length = _array.Length;
            T[] _newArray = new T[_length + 1];

            Array.Copy(_array, _newArray, _index);
            _newArray[_index] = _element;

            Array.Copy(_array, _index, _newArray, _index + 1, _length - _index);
            _array = _newArray;
        }

        /// <summary>
        /// Removes an element from an array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to remove element from.</param>
        /// <param name="_element">Element to remove.</param>
        public static void Remove<T>(ref T[] _array, T _element)
        {
            int _index = Array.IndexOf(_array, _element);
            if (_index > -1)
            {
                RemoveAt(ref _array, _index);
            }
        }

        /// <summary>
        /// Removes the element at the specified index from an array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to remove element from.</param>
        /// <param name="_index">Index of the element to remove.</param>
        public static void RemoveAt<T>(ref T[] _array, int _index)
        {
            int _count = _array.Length;
            T[] _newTags = new T[_count - 1];

            Array.Copy(_array, _newTags, _index);
            Array.Copy(_array, _index + 1, _newTags, _index, _count - _index - 1);

            _array = _newTags;
        }

        /// <param name="_element">Element of the array to move.</param>
        /// <inheritdoc cref="Move{T}(T[], int, int)"/>
        public static void Move<T>(T[] _array, T _element, int _destinationIndex)
        {
            int _index = Array.IndexOf(_array, _element);
            if (_index > -1)
            {
                Move(_array, _index, _destinationIndex);
            }
        }

        /// <summary>
        /// Moves an element of an array at a specific index,
        /// by shifting all other elements on the way.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array where to move element.</param>
        /// <param name="_index">Current index of the element to move.</param>
        /// <param name="_destinationIndex">New destination index where to move this element.</param>
        public static void Move<T>(T[] _array, int _index, int _destinationIndex)
        {
            T _element = _array[_index];
            if (_index < _destinationIndex)
            {
                for (int _i = _index; _i < _destinationIndex; _i++)
                    _array[_i] = _array[_i + 1];
            }
            else
            {
                for (int _i = _index; _i-- > _destinationIndex;)
                    _array[_i + 1] = _array[_i];
            }

            _array[_destinationIndex] = _element;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if an array contains a specific element.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to check content.</param>
        /// <param name="_element">Element to check presence in array.</param>
        /// <returns>True if the array contains the specified element, false otherwise.</returns>
        public static bool Contains<T>(T[] _array, T _element)
        {
            foreach (T _arrayElement in _array)
            {
                if ((_arrayElement != null) && _arrayElement.Equals(_element))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Assigns a specific value to all elements of an array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to set all elements value.</param>
        /// <param name="_value">Value to assign to each element of the array.</param>
        public static void Fill<T>(T[] _array, T _value)
        {
            for (int _i = 0; _i < _array.Length; _i++)
            {
                _array[_i] = _value;
            }
        }

        /// <summary>
        /// Assigns a specific value to all elemens of an array within a certain range.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_array">Array to set elements value.</param>
        /// <param name="_value">Value to assign to the elements of the array.</param>
        /// <param name="_startIndex">Index at which to start filling the array.</param>
        /// <param name="_count">Amount of array elements to set value.</param>
        public static void Fill<T>(T[] _array, T _value, int _startIndex, int _count)
        {
            int _length = Math.Min(_startIndex + _count, _array.Length);
            for (int _i = _startIndex; _i < _length; _i++)
            {
                _array[_i] = _value;
            }
        }
        #endregion

        #region Filter
        /// <returns><inheritdoc cref="Filter{T1, T2}(T1[], T2, Func{T1, T2, bool})" path="/returns"/></returns>
        /// <inheritdoc cref="Filter{T}(ref T[], Func{T, bool})"/>
        public static T[] Filter<T>(T[] _array, Func<T, bool> _filter)
        {
            int _count = 0;
            T[] _newArray = new T[_array.Length];

            for (int _i = 0; _i < _array.Length; _i++)
            {
                if (_filter(_array[_i]))
                {
                    _newArray[_count] = _array[_i];
                    _count++;
                }
            }

            Array.Resize(ref _newArray, _count);
            return _newArray;
        }

        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_filter">Method used to filter an element.
        /// <br/>Must return true if the element matches the filter criteria, or false to discard it.</param>
        /// <returns></returns>
        /// <inheritdoc cref="Filter{T1, T2}(T1[], T2, Func{T1, T2, bool})"/>
        public static void Filter<T>(ref T[] _array, Func<T, bool> _filter)
        {
            int _count = 0;
            for (int _i = 0; _i < _array.Length; _i++)
            {
                if (_filter(_array[_i]))
                {
                    if (_count != _i)
                        _array[_count] = _array[_i];

                    _count++;
                }
            }

            Array.Resize(ref _array, _count);
        }

        /// <summary>
        /// Get a filtered version of a specific array.
        /// </summary>
        /// <typeparam name="T1">Array element type.</typeparam>
        /// <typeparam name="T2">Filter option type.</typeparam>
        /// <param name="_array">Array to filter.</param>
        /// <param name="_filterOption">Filter option used to determine if an element should be kept or discarded.</param>
        /// <param name="_filter">Method used to filter an element: element as first parameter, filter option as second.
        /// <br/>Must return true if the element matches the filter criteria, or false to discard it.</param>
        /// <returns>New array with filtered content.</returns>
        public static T1[] Filter<T1, T2>(T1[] _array, T2 _filterOption, Func<T1, T2, bool> _filter)
        {
            if (_filterOption == null)
                return _array;

            // Create a new array to store all filtered elements.
            T1[] _filteredArray = new T1[_array.Length];
            int _count = 0;

            // Get all filtered element.
            foreach (T1 _element in _array)
            {
                if (_filter(_element, _filterOption))
                {
                    _filteredArray[_count] = _element;
                    _count++;
                }
            }

            Array.Resize(ref _filteredArray, _count);
            return _filteredArray;
        }
        #endregion
    }
}
