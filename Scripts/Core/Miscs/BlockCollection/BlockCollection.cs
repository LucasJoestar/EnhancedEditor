// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;

namespace EnhancedEditor {
    /// <summary>
    /// Base collection class to display its content as a block in the inspector.
    /// </summary>
    [Serializable]
    public abstract class BlockCollection<T> : IEnumerable<T> {
        #region Global Members
        /// <summary>
        /// The total amount of element in this collection.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Whether this collection size can be edited in the editor.
        /// </summary>
        public bool IsEditable = true;

        /// <summary>
        /// Whether this collection content can be reordered in the editor.
        /// </summary>
        public bool IsReorderable = true;

        /// <summary>
        /// Whether this collection content is displayed as readonly or not.
        /// </summary>
        public bool IsReadonly = false;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="BlockCollection(bool, bool, bool)"/>
        public BlockCollection() : this(true, true) { }

        /// <param name="_isEditable"><inheritdoc cref="IsEditable" path="/summary"/></param>
        /// <param name="_isReorderable"><inheritdoc cref="IsReorderable" path="/summary"/></param>
        /// <param name="_isReadonly"><inheritdoc cref="IsReadonly" path="/summary"/></param>
        /// <inheritdoc cref="BlockCollection{T}"/>
        public BlockCollection(bool _isEditable, bool _isReorderable = true, bool _isReadonly = false) {
            IsEditable = _isEditable;
            IsReorderable = _isReorderable;
            IsReadonly = _isReadonly;
        }
        #endregion

        #region Operator
        public abstract T this[int _index] { get;set; }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Adds an element to this collection.
        /// </summary>
        /// <param name="_element">Element to add.</param>
        public abstract void Add(T _element);

        /// <summary>
        /// Clears this collection content.
        /// </summary>
        public abstract void Clear();
        #endregion
    }

    /// <summary>
    /// Displays an array as a block in the inspector.
    /// </summary>
    /// <typeparam name="T">Array content type.</typeparam>
    [Serializable]
    public class BlockArray<T> : BlockCollection<T> {
        #region Global Members
        /// <summary>
        /// This wrapper array.
        /// </summary>
        [Block(false)] public T[] Array = new T[] { };

        public override int Count {
            get { return Array.Length; }
        }

        // -----------------------

        /// <inheritdoc cref="BlockCollection{T}.BlockCollection()"/>
        public BlockArray() : base() { }

        /// <inheritdoc cref="BlockCollection{T}.BlockCollection(bool, bool, bool)"/>
        public BlockArray(bool _isEditable, bool _isReorderable = true, bool _isReadonly = false) : base(_isEditable, _isReorderable, _isReadonly) { }
        #endregion

        #region Operator
        public override T this[int _index] {
            get { return Array[_index]; }
            set { Array[_index] = value; }
        }

        public static implicit operator T[](BlockArray<T> _collection) {
            return _collection.Array;
        }
        #endregion

        #region Utility
        public override void Add(T _element) {
            ArrayUtility.Add(ref Array, _element);
        }

        public override void Clear() {
            System.Array.Resize(ref Array, 0);
        }
        #endregion
    }

    /// <summary>
    /// Displays a list as a block in the inspector.
    /// </summary>
    /// <typeparam name="T">List content type.</typeparam>
    [Serializable]
    public class BlockList<T> : BlockCollection<T> {
        #region Global Members
        /// <summary>
        /// This wrapper list.
        /// </summary>
        [Block(false)] public List<T> List = new List<T>();

        public override int Count {
            get { return List.Count; }
        }

        // -----------------------

        /// <inheritdoc cref="BlockCollection{T}.BlockCollection()"/>
        public BlockList() : base() { }

        /// <inheritdoc cref="BlockCollection{T}.BlockCollection(bool, bool, bool)"/>
        public BlockList(bool _isEditable, bool _isReorderable = true, bool _isReadonly = false) : base(_isEditable, _isReorderable, _isReadonly) { }
        #endregion

        #region Operator
        public override T this[int _index] {
            get { return List[_index]; }
            set { List[_index] = value; }
        }

        public static implicit operator List<T>(BlockList<T> _collection) {
            return _collection.List;
        }
        #endregion

        #region Utility
        public override void Add(T _element) {
            List.Add(_element);
        }

        public override void Clear() {
            List.Clear();
        }
        #endregion
    }
}
