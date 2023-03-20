// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Collection class used to store a specific value associated to each values of an other collection (<see cref="IList"/>).
    /// </summary>
    /// <typeparam name="T">The type of values to be associated with the collection.</typeparam>
    [Serializable]
    public class CollectionValues<T> {
        #region Global Members
        #if UNITY_EDITOR
        /// <summary>
        /// This collection associated default value.
        /// </summary>
        [SerializeField] private T defaultValue = default;

        /// <summary>
        /// Can either be a field or a property, but must be convertible to an <see cref="IList"/>.
        /// </summary>
        [SerializeField] private string collectionMember = string.Empty;
        #endif

        /// <summary>
        /// All values associated with this collection.
        /// </summary>
        public T[] Values = new T[0];

        /// <summary>
        /// The total count of values in this collection.
        /// </summary>
        public int Count {
            get { return Values.Length; }
        }

        // -----------------------

        /// <param name="_collectionMember"><inheritdoc cref="collectionMember" path="/summary"/></param>
        /// <param name="_defaultValue"><inheritdoc cref="defaultValue" path="/summary"/></param>
        /// <inheritdoc cref="CollectionValues{T}"/>
        public CollectionValues(string _collectionMember, T _defaultValue = default) {
            #if UNITY_EDITOR
            collectionMember = _collectionMember;
            defaultValue = _defaultValue;
            #endif
        }
        #endregion

        #region Operator
        public T this[int _index] {
            get { return Values[_index]; }
            set { Values[_index] = value; }
        }

        public override string ToString() {
            return Values.ToString();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Resizes and fills this object content based on its associated collection.
        /// </summary>
        /// <param name="_collectionMemberSource">The source object containing this object associated collection.</param>
        public void Fill(object _collectionMemberSource) {
            #if UNITY_EDITOR
            if (!new MemberValue<IList>(collectionMember).GetValue(_collectionMemberSource, _collectionMemberSource.GetType(), out IList _list)) {
                return;
            }

            int _count = Count;
            if (_count != _list.Count) {
                Array.Resize(ref Values, _list.Count);
                for (int i = _count; i < Count; i++) {
                    Values[i] = defaultValue;
                }
            }
            #endif
        }
        #endregion
    }
}
