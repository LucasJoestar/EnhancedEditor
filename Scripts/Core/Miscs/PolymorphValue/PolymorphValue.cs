// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Wrapper for a value that can change its type while it derives from a specific base <see cref="System.Type"/>.
    /// </summary>
    /// <typeparam name="T">Base type this value must be derived from.</typeparam>
    [Serializable]
    public class PolymorphValue<T> where T : class {
        #region Global Members
        /// <summary>
        /// This value type.
        /// </summary>
        [SerializeField, Enhanced, DisplayName("displayName", true), ValidationMember("SetType")] private SerializedType<T> type = null;

        /// <summary>
        /// This object value.
        /// </summary>
        [field: SerializeReference, Space(5f), Enhanced, Block]
        public T Value { get; private set; } = null;

        // -----------------------

        [SerializeField, HideInInspector] private bool doFullCopy = false;
        [SerializeField, HideInInspector] private string displayName = "Type";

        /// <summary>
        /// This value type.
        /// <br/> Automatically modifies this value when changed.
        /// </summary>
        public Type Type {
            get { return type; }
            set {
                type.Type = value;

                value = type.Type;

                // Null value.
                if (value == null) {
                    Value = null;
                    return;
                }

                // Polymorphism.
                if (value != Value?.GetType()) {
                    T _value = Activator.CreateInstance(value) as T;

                    if (Value != null) {
                        _value = EnhancedUtility.CopyObjectContent(Value, _value, doFullCopy) as T;
                    }

                    Value = _value;
                }
            }
        }

        /// <summary>
        /// Inspector callback.
        /// </summary>
        #pragma warning disable IDE0051
        private SerializedType<T> SetType {
            set {
                Type = value.Type;
            }
        }
        #endregion

        #region Constructors
        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, string, Type[])"/>
        public PolymorphValue(Type _value, SerializedTypeConstraint _constraints = SerializedTypeConstraint.None) : this(_constraints, _value, true, "Type", null) { }

        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, string, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, params Type[] _ignored) : this(_constraints, _value, true, _ignored) {  }

        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, string, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, string _displayName, params Type[] _ignored) :
                              this(_constraints, _value, true, _displayName, _ignored) { }

        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, string, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, bool _doFullCopy, params Type[] _ignored) :
                              this(_constraints, _value, _doFullCopy, "Type", _ignored) { }

        /// <param name="_value">This object initial value.</param>
        /// <param name="_doFullCopy"><inheritdoc cref="EnhancedUtility.CopyObjectContent(object, object, bool)" path="/param[@name='_doFullCopy']"/>
        /// <br/> (True by default)</param>
        /// <param name="_displayName">The displayed name of this value 'Type' field.</param>
        /// <inheritdoc cref="PolymorphValue{T}"/>
        /// <inheritdoc cref="SerializedType{T}.SerializedType(SerializedTypeConstraint, Type, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, bool _doFullCopy, string _displayName, params Type[] _ignored) {
            // Must be instantiated.
            _constraints &= ~SerializedTypeConstraint.Abstract;

            type = new SerializedType<T>(_constraints, _value, _ignored);
            doFullCopy = _doFullCopy;
            displayName = _displayName;

            // Create type instance.
            Type = type;
        }
        #endregion

        #region Operators
        public static implicit operator T(PolymorphValue<T> _value) {
            return _value.Value;
        }

        public static implicit operator Type(PolymorphValue<T> _value) {
            return _value.Type;
        }

        public override string ToString() {
            return (Value != null) ? Value.ToString() : "[Null]";
        }

        public override bool Equals(object _object) {
            if (_object is PolymorphValue<T> _value) {
                return Value == _value.Value;
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Conversation
        /// <summary>
        /// Get the converted value of this object if it of the given type.
        /// </summary>
        /// <typeparam name="U">The type to get this object value into.</typeparam>
        /// <param name="_value">The casted value of this object (null if failed).</param>
        /// <returns>True if this object value could be casted into the given type, false otherwise.</returns>
        public bool GetValue<U>(out U _value) where U : T {
            if (Value is U _temp) {
                _value = _temp;
                return true;
            }

            _value = default;
            return false;
        }
        #endregion
    }
}
