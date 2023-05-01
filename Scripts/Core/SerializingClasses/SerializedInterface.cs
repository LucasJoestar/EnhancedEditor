// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Interface wrapper, used to serialize an interface with the help
    /// of the <see cref="UnityEngine.Component"/> it is attached to.
    /// <para/>
    /// Note:
    /// <br/> Cannot use the <see cref="ISerializationCallbackReceiver"/> interface because the
    /// <see cref="GameObject.GetComponent{T}"/> method is not allowed to be called during serialization.
    /// </summary>
    /// <typeparam name="T">Interface type to serialize.</typeparam>
    [Serializable]
    #pragma warning disable
    public class SerializedInterface<T> : IComparer<SerializedInterface<T>> where T : class {
        #region Global Members
        [SerializeField] private Component component = null;
        private T interfaceInstance = default;

        [SerializeField] private bool required = false;        

        /// <summary>
        /// The <see cref="UnityEngine.Component"/> the associated interface is attached to.
        /// </summary>
        public Component Component {
            get {
                return component;
            }
            set {
                component = value;
                DeserializeInterfaceValue();
            }
        }

        /// <summary>
        /// The interface attached to the serialized <see cref="UnityEngine.Component"/>.
        /// </summary>
        public T Interface {
            get {

                if (interfaceInstance == null) {
                    DeserializeInterfaceValue();
                }

                return interfaceInstance;
            }
            set {
                interfaceInstance = value;
                component = value as Component;
            }
        }

        // -----------------------

        /// <inheritdoc cref="SerializedInterface{T}"/>
        public SerializedInterface(bool _required = true) {
            #if UNITY_EDITOR
            required = _required;        
            #endif
        }

        /// <param name="_interface"><inheritdoc cref="Interface" path="/summary"/></param>
        /// <param name="_required">If true, draw this interface as a required field in the inspector.</param>
        /// <inheritdoc cref="SerializedInterface{T}"/>
        public SerializedInterface(T _interface, bool _required = true) : this(_required) {
            Interface = _interface;
        }
        #endregion

        #region Operator
        public static bool operator ==(SerializedInterface<T> a, SerializedInterface<T> b) {
            if (!ReferenceEquals(a, null)) {
                return a.Equals(b);
            }

            return ReferenceEquals(b, null);
        }

        public static bool operator !=(SerializedInterface<T> a, SerializedInterface<T> b) {
            return !(a == b);
        }

        public static implicit operator T(SerializedInterface<T> _interface) {
            return _interface.Interface;
        }

        public static implicit operator Component(SerializedInterface<T> _interface) {
            return _interface.component;
        }

        public static implicit operator SerializedInterface<T>(T _interface) {
            return new SerializedInterface<T>(_interface);
        }

        public override string ToString() {
            return (interfaceInstance != null) ? interfaceInstance.ToString() : "[Null Interface]";
        }

        public override bool Equals(object _object) {
            if (_object is SerializedInterface<T> _interface) {
                return Equals(_interface);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Comparer
        int IComparer<SerializedInterface<T>>.Compare(SerializedInterface<T> a, SerializedInterface<T> b) {
            return a.Interface.GetType().Name.CompareTo(b.Interface.GetType().Name);
        }
        #endregion

        #region Serialization
        private void DeserializeInterfaceValue() {
            // Deserialize the interface value on the game object reference.
            if (component != null) {

                if (component is T _interface) {

                    Interface = _interface;
                    return;
                }

                component.LogWarning($"The interface '{typeof(T).FullName}' component could not be found on this object!");
            }

            Interface = null;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Compares this interface with another one.
        /// </summary>
        /// <param name="_interface">Interface to compare with this one.</param>
        /// <returns>True if both interfaces are equal, false otherwise.</returns>
        public bool Equals(SerializedInterface<T> _interface) {
            return !ReferenceEquals(_interface, null) && _interface.Interface == Interface;
        }
        #endregion
    }
}
