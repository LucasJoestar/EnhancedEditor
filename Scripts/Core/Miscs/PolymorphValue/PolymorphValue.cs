// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// Wrapper for a value that can change its type while it derives from a specific base <see cref="System.Type"/>.
    /// </summary>
    /// <typeparam name="T">Base type this value must be derived from.</typeparam>
    [Serializable]
    public class PolymorphValue<T> where T : class {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField] private string name = "[NONE]"; // Editor object name.
        #endif

        /// <summary>
        /// This value type.
        /// </summary>
        [SerializeField, Enhanced, ValidationMember("SetType")] private SerializedType<T> type = null;

        [Space(5f)]

        [SerializeReference] private T value = null;

        // -----------------------

        [SerializeField, HideInInspector] private bool doFullCopy = false;

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
                    this.value = null;

                    #if UNITY_EDITOR
                    name = "[NONE]";
                    #endif

                    return;
                }

                // Polymorphism.
                if (value != this.value?.GetType()) {
                    T _value = Activator.CreateInstance(value) as T;
                    if (this.value != null) {
                        _value = EnhancedUtility.CopyObjectContent(this.value, _value, doFullCopy) as T;
                    }

                    this.value = _value;

                    #if UNITY_EDITOR
                    var _attributes = value.GetCustomAttributes(typeof(DisplayNameAttribute), true);

                    name = (_attributes.Length != 0)
                         ? (_attributes[0] as DisplayNameAttribute).Label.text
                         : ObjectNames.NicifyVariableName(value.Name);
                    #endif
                }
            }
        }

        /// <summary>
        /// This object value.
        /// </summary>
        public T Value {
            get { return value; }
            set {
                if (value == null) {
                    Type = null;
                    return;
                }

                Type _type = value.GetType();
                Type = _type;

                if (Type == _type) {
                    this.value = value;
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
        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, Type[])"/>
        public PolymorphValue() : this(SerializedTypeConstraint.Null, null, true, null) { }

        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, Type[])"/>
        public PolymorphValue(Type _value, SerializedTypeConstraint _constraints = SerializedTypeConstraint.None) : this(_constraints, _value, true, null) { }

        /// <inheritdoc cref="PolymorphValue{T}.PolymorphValue(SerializedTypeConstraint, Type, bool, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, params Type[] _ignored) : this(_constraints, _value, true, _ignored) {  }

        /// <param name="_value">This object initial value.</param>
        /// <param name="_doFullCopy"><inheritdoc cref="EnhancedUtility.CopyObjectContent(object, object, bool)" path="/param[@name='_doFullCopy']"/>
        /// <br/> (True by default)</param>
        /// <inheritdoc cref="PolymorphValue{T}"/>
        /// <inheritdoc cref="SerializedType{T}.SerializedType(SerializedTypeConstraint, Type, Type[])"/>
        public PolymorphValue(SerializedTypeConstraint _constraints, Type _value, bool _doFullCopy, params Type[] _ignored) {
            // Must be instantiated.
            _constraints &= ~SerializedTypeConstraint.Abstract;

            type = new SerializedType<T>(_constraints, _value, _ignored);
            doFullCopy = _doFullCopy;

            // Create type instance.
            Type = type;
        }
        #endregion

        #region Operator
        public static implicit operator T(PolymorphValue<T> _value) {
            return _value.value;
        }

        public static implicit operator Type(PolymorphValue<T> _value) {
            return _value.Type;
        }

        public override string ToString() {
            return (value != null) ? value.ToString() : "[Null]";
        }

        public override bool Equals(object _object) {
            if (_object is PolymorphValue<T> _value) {
                return value == _value.value;
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Convertion
        /// <summary>
        /// Get the converted value of this object if it of the given type.
        /// </summary>
        /// <typeparam name="U">The type to get this object value into.</typeparam>
        /// <param name="_value">The casted value of this object (null if failed).</param>
        /// <returns>True if this object value could be casted into the given type, false otherwise.</returns>
        public bool GetValue<U>(out U _value) where U : T {
            if (value is U _temp) {
                _value = _temp;
                return true;
            }

            _value = default;
            return false;
        }
        #endregion
    }
}
