// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor
{
    /// <summary>
    /// Utility extension methods for <see cref="Array"/> from the <see cref="EnhancedEditor"/>.
    /// </summary>
	public static class ArrayExtensions
    {
        #region Management
        /// <summary>
        /// Adds an element into the array.
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
        /// Adds an array into another the array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="_source">Source array to add elements in.</param>
        /// <param name="_other">Other array to add content in source.</param>
        public static void Add<T>(ref T[] _source, T[] _other)
        {
            int _sourceLength = _source.Length;
            int _otherLength = _other.Length;

            T[] _newArray = new T[_sourceLength + _otherLength];

            Array.Copy(_source, _newArray, _sourceLength);
            Array.Copy(_other, 0, _newArray, _sourceLength, _otherLength);

            _source = _newArray;
        }

        /// <summary>
        /// Removes an element from the array.
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
        /// Removes the element at the specified index from the array.
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
        #endregion
    }
}
