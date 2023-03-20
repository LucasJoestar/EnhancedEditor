// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Collection class used to store a specific value associated to each values of an <see cref="Enum"/>.
    /// </summary>
    /// <typeparam name="Enum">This collection associated <see cref="Enum"/> type.</typeparam>
    /// <typeparam name="T">The type of values to be associated with the enum.</typeparam>
    [Serializable]
    public sealed class EnumValues<Enum, T> : IEnumerable<T>, ISerializationCallbackReceiver where Enum : System.Enum, IConvertible {
        #region Global Members
        /// <summary>
        /// This enum associated default value.
        /// </summary>
        public readonly T DefaultValue = default;

        /// <summary>
        /// Enum int value as first, the associated value as second.
        /// </summary>
        public Pair<int, T>[] Values = new Pair<int, T>[0];

        /// <summary>
        /// Total amount of values in this object.
        /// </summary>
        public int Count {
            get { return Values.Length; }
        }

        // -----------------------

        /// <param name="_defaultValue"><inheritdoc cref="DefaultValue" path="/summary"/></param>
        /// <inheritdoc cref="EnumValues{Enum, T}"/>
        public EnumValues(T _defaultValue = default) {
            DefaultValue = _defaultValue;
            Fill();
        }
        #endregion

        #region Operator
        public T this[Enum _enum] {
            get {
                GetValue(_enum, out T _value);
                return _value;
            } set {

                int _index = GetValueIndex(_enum);
                if (_index != -1) {
                    Values[_index].Second = value;
                }
            }
        }

        public T this[int _index] {
            get {
                return Values[_index].Second;
            } set {
                Values[_index].Second = value;
            }
        }

        public override string ToString() {
            return Values.ToString();
        }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return Values[i].Second;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region Serialization
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            Fill();
        }
        #endregion

        #region Management
        /// <summary>
        /// Get the value associated with a specific enum value.
        /// </summary>
        /// <param name="_enum">The <see cref="Enum"/> to get the associated value.</param>
        /// <param name="_value">The value associated with the given enum (-1 if none).</param>
        /// <returns>True if the value could be successfully found, false otherwise.</returns>
        public bool GetValue(Enum _enum, out T _value) {

            int _index = GetValueIndex(_enum);
            if (_index == -1) {

                _value = default;
                return false;
            }

            _value = Values[_index].Second;
            return true;
        }

        /// <summary>
        /// Get the enum <see cref="int"/> value associated with a specific value.
        /// </summary>
        /// <param name="_value">The value to get the associated enum value.</param>
        /// <param name="_enumValue">The <see cref="int"/> enum value associated with the given value (-1 if none).</param>
        /// <returns>True if the value could be successfully found, false otherwise.</returns>
        public bool GetEnum(T _value, out int _enumValue) {

            int _index = GetEnumIndex(_value);
            if (_index == -1) {

                _enumValue = -1;
                return false;
            }

            _enumValue = Values[_index].First;
            return true;
        }

        /// <summary>
        /// Get the index of the value associated with a specific enum value.
        /// </summary>
        /// <param name="_enum">The <see cref="Enum"/> to get the associated value index.</param>
        /// <returns>The index of the value associated with this enum (-1 if none).</returns>
        public int GetValueIndex(Enum _enum) {
            int _enumValue = Convert.ToInt32(_enum);

            for (int i = 0; i < Values.Length; i++) {
                if (_enumValue == Values[i].First) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the index of the enum associated with a specific value.
        /// </summary>
        /// <param name="_value">The value to get the associated enum index.</param>
        /// <returns>The index of the enum associated with this value (-1 if none).</returns>
        public int GetEnumIndex(T _value) {

            for (int i = 0; i < Values.Length; i++) {
                if (_value.Equals(Values[i].Second)) {
                    return i;
                }
            }

            return -1;
        }

        // -----------------------

        [Conditional("UNITY_EDITOR")]
        private void Fill() {
            Type _type = typeof(Enum);
            Enum[] _values = (Enum[])System.Enum.GetValues(typeof(Enum));

            // Remove ethereal elements.
            for (int i = _values.Length; i-- > 0;) {
                var _object = _type.GetField(_values[i].ToString());

                if ((_object != null) && _object.IsDefined(typeof(EtherealAttribute), true)) {
                    ArrayUtility.RemoveAt(ref _values, i);
                }
            }

            int[] _integers = Array.ConvertAll(_values, v => Convert.ToInt32(v));

            for (int i = Count; i-- > 0;) {
                if (!ArrayUtility.Contains(_integers, Values[i].First)) {
                    RemoveValue(i);
                }
            }

            foreach (int _value in _integers) {
                if (Array.FindIndex(Values, (v) => v.First == _value) == -1) {
                    AddValue(_value);
                }
            }
        }

        private int AddValue(Enum _enum) {
            return AddValue(Convert.ToInt32(_enum));
        }

        private int AddValue(int _enum) {
            int _index;
            for (_index = 0; _index < Values.Length; _index++) {
                if (_enum < Values[_index].First) {
                    break;
                }
            }

            T _value = default;
            if ((DefaultValue != null) && (_value != null)) {
                EnhancedUtility.CopyObjectContent(DefaultValue, _value);
            }

            ArrayUtility.Insert(ref Values, _index, new Pair<int, T>(_enum, _value));
            return _index;
        }

        private void RemoveValue(int _index) {
            ArrayUtility.RemoveAt(ref Values, _index);
        }
        #endregion
    }
}
