// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// GUI getter / setter buffer, allowing to dynamically push and pop values.
    /// </summary>
    /// <typeparam name="T">Buffer value type.</typeparam>
    public class GUIBuffer<T>
    {
        #region Scope
        /// <summary>
        /// Disposable GUI helper class used to wrap your controls within a pushed-in-buffer value.
        /// </summary>
        public class GUIBufferScope : GUI.Scope
        {
            private readonly GUIBuffer<T> buffer = null;

            /// <param name="_buffer">Associated GUI buffer.</param>
            /// <param name="_value">Pushed-in-buffer value.</param>
            /// <inheritdoc cref="GUIBufferScope"/>
            public GUIBufferScope(GUIBuffer<T> _buffer, T _value)
            {
                buffer = _buffer;
                _buffer.Push(_value);
            }

            // -----------------------

            protected override void CloseScope()
            {
                buffer.Pop();
            }
        }
        #endregion

        #region Global Members
        private readonly string name = string.Empty;
        private readonly Func<T> getter = null;
        private readonly Action<T> setter = null;

        /// <summary>
        /// All pushed-in-buffer values.
        /// </summary>
        public List<T> Buffer = new List<T>();

        // -----------------------

        /// <param name="_getter">Associated value getter.</param>
        /// <param name="_setter">Associated value setter.</param>
        /// <param name="_name">Name of the buffer associated value (used for error logs).</param>
        /// <inheritdoc cref="GUIBuffer{T}"/>
        public GUIBuffer(Func<T> _getter, Action<T> _setter, string _name)
        {
            getter = _getter;
            setter = _setter;
            name = _name;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Push the current associated value into the buffer and set its new value.
        /// </summary>
        /// <param name="_value">New value.</param>
        public void Push(T _value)
        {
            T _previous = getter();

            Buffer.Add(_previous);
            setter(_value);
        }

        /// <summary>
        /// Reverts the buffer to its previous pushed-in value.
        /// </summary>
        /// <returns>Previous pushed-in-buffer value.</returns>
        public T Pop()
        {
            if (Buffer.Count == 0)
            {
                // Logs an error when the buffer is empty.
                Debug.LogError($"You are popping more {name} than you are pushing!");
                return default;
            }

            int _index = Buffer.Count - 1;
            T _value = Buffer[_index];

            setter(_value);
            Buffer.RemoveAt(_index);

            return _value;
        }

        /// <summary>
        /// Clears the buffer and resets its associated value.
        /// </summary>
        /// <returns>First initial pushed-in-buffer value.</returns>
        public T Clear()
        {
            T _value = Buffer[0];
            Buffer.Clear();

            return _value;
        }

        /// <summary>
        /// Creates a new GUI scope for this buffer. Use this with the using keyword to wrapp your GUI controls within.
        /// </summary>
        /// <param name="_value">New buffer value.</param>
        /// <returns>Disposable helper class to wrap your controls within.</returns>
        public GUIBufferScope Scope(T _value)
        {
            GUIBufferScope _scope = new GUIBufferScope(this, _value);
            return _scope;
        }
        #endregion
    }

    /// <typeparam name="T1">Reference object type.</typeparam>
    /// <typeparam name="T2">Object value type.</typeparam>
    /// <inheritdoc cref="GUIBuffer{T}"/>
    public class GUIBuffer<T1, T2>
    {
        #region Scope
        /// <inheritdoc cref="GUIBuffer{T}.GUIBufferScope"/>
        public class GUIBufferScope : GUI.Scope
        {
            private readonly GUIBuffer<T1, T2> buffer = null;
            private readonly T1 key = default;

            /// <param name="_buffer">Associated GUI buffer.</param>
            /// <param name="_key">Reference object to set value.</param>
            /// <param name="_value">Pushed-in-buffer value.</param>
            /// <inheritdoc cref="GUIBufferScope"/>
            public GUIBufferScope(GUIBuffer<T1, T2> _buffer, T1 _key, T2 _value)
            {
                buffer = _buffer;
                key = _key;

                _buffer.Push(_key, _value);
            }

            // -----------------------

            protected override void CloseScope()
            {
                buffer.Pop(key);
            }
        }
        #endregion

        #region Global Members
        private readonly string name = string.Empty;
        private readonly Func<T1, T2> getter = null;
        private readonly Action<T1, T2> setter = null;

        /// <summary>
        /// All pushed-in-buffer values.
        /// <para/>
        /// Reference object as key, with all its pushed-in values as value.
        /// </summary>
        public Dictionary<T1, List<T2>> Buffer = new Dictionary<T1, List<T2>>();

        // -----------------------

        /// <param name="_getter">Associated value getter.</param>
        /// <param name="_setter">Associated value setter.</param>
        /// <param name="_name">Name of the buffer associated value (used for error logs).</param>
        /// <inheritdoc cref="GUIBuffer{T}"/>
        public GUIBuffer(Func<T1, T2> _getter, Action<T1, T2> _setter, string _name)
        {
            getter = _getter;
            setter = _setter;
            name = _name;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Push the current value of a specific key into the buffer and set its new value.
        /// </summary>
        /// <param name="_key">Reference object to push and set value.</param>
        /// <param name="_value">New value.</param>
        public void Push(T1 _key, T2 _value)
        {
            T2 _previous = getter(_key);

            if (!Buffer.ContainsKey(_key))
                Buffer.Add(_key, new List<T2>());

            Buffer[_key].Add(_previous);
            setter(_key, _value);
        }

        /// <summary>
        /// Reverts the buffer of a specific key to its previous pushed-in value.
        /// </summary>
        /// <param name="_key">Reference object to revert value.</param>
        /// <returns>Previous key pushed-in-buffer value.</returns>
        public T2 Pop(T1 _key)
        {
            if (!Buffer.ContainsKey(_key))
            {
                // Logs an error when the buffer is empty.
                Debug.LogError($"You are popping more {name} than you are pushing!");
                return default;
            }

            int _index = Buffer[_key].Count - 1;
            T2 _value = Buffer[_key][_index];

            setter(_key, _value);
            Buffer[_key].RemoveAt(_index);

            if (_index == 0)
                Buffer.Remove(_key);

            return _value;
        }

        /// <summary>
        /// Clears the buffer of a specific key and resets its associated value.
        /// </summary>
        /// <param name="_key">Reference object to clear and reset value.</param>
        public void Clear(T1 _key)
        {
            Buffer[_key].Clear();
        }

        /// <summary>
        /// Creates a new GUI scope for this buffer. Use this with the using keyword to wrapp your GUI controls within.
        /// </summary>
        /// <param name="_key">Reference object to set value.</param>
        /// <param name="_value">New object buffer value.</param>
        /// <returns>Disposable helper class to wrap your controls within.</returns>
        public GUIBufferScope Scope(T1 _key, T2 _value)
        {
            GUIBufferScope _scope = new GUIBufferScope(this, _key, _value);
            return _scope;
        }
        #endregion
    }
}
