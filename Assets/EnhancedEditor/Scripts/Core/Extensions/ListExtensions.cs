// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;

namespace EnhancedEditor
{
    /// <summary>
    /// Contains multiple <see cref="List{T}"/>-related extension methods.
    /// </summary>
	public static class ListExtensions
    {
        #region Management
        /// <returns><inheritdoc cref="First{T}(List{T}, out T)" path="/param[@name='_element']"/></returns>
        /// <inheritdoc cref="First{T}(List{T}, out T)"/>
        public static T First<T>(this List<T> _list)
        {
            First(_list, out T _element);
            return _element;
        }

        /// <summary>
        /// Get the first element of this list.
        /// </summary>
        /// <typeparam name="T">List element type from.</typeparam>
        /// <param name="_list">List to get first element.</param>
        /// <param name="_element">First element of the list (null if none).</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool First<T>(this List<T> _list, out T _element)
        {
            if (_list.Count == 0)
            {
                _element = default;
                return false;
            }

            _element = _list[0];
            return true;
        }

        /// <returns><inheritdoc cref="Last{T}(List{T}, out T)" path="/param[@name='_element']"/></returns>
        /// <inheritdoc cref="Last{T}(List{T}, out T)"/>
        public static T Last<T>(this List<T> _list)
        {
            Last(_list, out T _element);
            return _element;
        }

        /// <summary>
        /// Get the last element of this list.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to get last element from.</param>
        /// <param name="_element">Last element of the list (null if none).</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool Last<T>(this List<T> _list, out T _element)
        {
            if (_list.Count == 0)
            {
                _element = default;
                return false;
            }

            int _index = _list.Count - 1;
            _element = _list[_index];

            return true;
        }

        /// <summary>
        /// Removes the first element of this list.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to remove first element from.</param>
        /// <returns>True if this list is not empty, false otherwise.</returns>
        public static bool RemoveFirst<T>(this List<T> _list)
        {
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
        public static bool RemoveLast<T>(this List<T> _list)
        {
            if (_list.Count == 0)
                return false;

            int _index = _list.Count - 1;
            _list.RemoveAt(_index);

            return true;
        }
        #endregion

        #region Size
        /// <summary>
        /// Changes the number of elements in this list to a specific size.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="_list">List to resize.</param>
        /// <param name="_size">New list size.</param>
        public static void Resize<T>(this List<T> _list, int _size)
        {
            int _count = _list.Count;
            if (_count > _size)
            {
                _list.RemoveRange(_size, _count - _size);
            }
            else if (_size > _count)
            {
                if (_size > _list.Capacity)
                    _list.Capacity = _size;

                for (int _i = _count; _i < _size; _i++)
                {
                    _list.Add(default);
                }
            }
        }
        #endregion
    }
}
