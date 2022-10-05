// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="EnumValues{Enum, T}"/> base class.
    /// <para/>
    /// This class only exist for having a property drawer assigned to it
    /// <br/>(classes with more than one generic argument cannot have a specific property drawer).
    /// <para/>
    /// Should not be used directly, always prefer using <see cref="EnumValues{Enum, T}"/>
    /// </summary>
    /// <typeparam name="T">The type of values to be associated with the enum.</typeparam>
    [Serializable]
    public abstract class EnumValues<T> {
        #region Global Members
        /// <summary>
        /// Enum int value as first, the associated value as second.
        /// </summary>
        public Pair<int, T>[] Values = new Pair<int, T>[0];
        #endregion
    }

    /// <summary>
    /// Collection class used to store a specific value associated to each values of an <see cref="Enum"/>.
    /// </summary>
    /// <typeparam name="Enum">This collection associated <see cref="Enum"/> type.</typeparam>
    /// <typeparam name="T">The type of values to be associated with the enum.</typeparam>
    [Serializable]
    public class EnumValues<Enum, T> : EnumValues<T>, ISerializationCallbackReceiver where Enum : System.Enum {
        #region Global Members
        /// <summary>
        /// This enum associated default value.
        /// </summary>
        public readonly T DefaultValue = default;

        // -----------------------

        /// <param name="_defaultValue"><inheritdoc cref="DefaultValue" path="/summary"/></param>
        /// <inheritdoc cref="EnumValues{Enum, T}"/>
        public EnumValues(T _defaultValue = default) {
            DefaultValue = _defaultValue;
        }
        #endregion

        #region Operators
        public T this[Enum _enum] {
            get {
                return GetValue(_enum);
            } set {
                Values[GetValueIndex(_enum)].Second = value;
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
        /// <returns>The value associated with this enum.</returns>
        public T GetValue(Enum _enum) {
            return Values[GetValueIndex(_enum)].Second;
        }

        /// <summary>
        /// Get the index of the value associated with a specific enum value.
        /// </summary>
        /// <param name="_enum">The <see cref="Enum"/> to get the associated value index.</param>
        /// <returns>The index of the value associated with this enum.</returns>
        public int GetValueIndex(Enum _enum) {
            int _enumValue = Convert.ToInt32(_enum);

            for (int i = 0; i < Values.Length; i++) {
                if (_enumValue == Values[i].First) {
                    return i;
                }
            }

            return AddValue(_enum);
        }

        // -----------------------

        private void Fill() {
            int[] _values = Array.ConvertAll((Enum[])System.Enum.GetValues(typeof(Enum)), v => Convert.ToInt32(v));

            for (int i = Values.Length; i-- > 0;) {
                if (!ArrayUtility.Contains(_values, Values[i].First)) {
                    RemoveValue(i);
                }
            }

            for (int i = 0; i < _values.Length; i++) {
                int _value = _values[i];
                if (Array.FindIndex(Values, (v) => v.First == _value) == -1) {
                    AddValue(i);
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
