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
    /// of the <see cref="UnityEngine.GameObject"/> it is attached to.
    /// <para/>
    /// Note:
    /// <br/> Cannot use the <see cref="ISerializationCallbackReceiver"/> interface because the
    /// <see cref="GameObject.GetComponent{T}"/> method is not allowed to be called during serialization.
    /// </summary>
    /// <typeparam name="T">Interface type to serialize.</typeparam>
    [Serializable]
    #pragma warning disable UNT0014
    public class SerializedInterface<T> : IComparer<SerializedInterface<T>> where T : class {
        #region Global Members
        [SerializeField] protected GameObject gameObject = null;
        private T interfaceInstance = default;

        /// <summary>
        /// The <see cref="GameObject"/> the associated interface is attached to.
        /// </summary>
        public GameObject GameObject {
            get {
                return gameObject;
            }
            set {
                gameObject = value;
                DeserializeInterfaceValue();
            }
        }

        /// <summary>
        /// The interface attached to the serialized <see cref="UnityEngine.GameObject"/>.
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
                gameObject = (value is Component _component) ? _component.gameObject : null;
            }
        }

        // -----------------------

        /// <inheritdoc cref="SerializedInterface{T}"/>
        public SerializedInterface() { }

        /// <param name="_interface"><inheritdoc cref="Interface" path="/summary"/></param>
        /// <inheritdoc cref="SerializedInterface{T}"/>
        public SerializedInterface(T _interface) {
            Interface = _interface;
        }
        #endregion

        #region Operators
        public static implicit operator T(SerializedInterface<T> _interface) {
            return _interface.Interface;
        }

        public static implicit operator GameObject(SerializedInterface<T> _interface) {
            return _interface.gameObject;
        }

        public static implicit operator SerializedInterface<T>(T _interface) {
            return new SerializedInterface<T>(_interface);
        }

        public override string ToString() {
            return (interfaceInstance != null) ? interfaceInstance.ToString() : "[Null Interface]";
        }

        public override bool Equals(object _object) {
            if (_object is SerializedInterface<T> _interface) {
                return Interface == _interface.Interface;
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
            if (gameObject != null) {
                interfaceInstance = gameObject.GetComponent<T>();

                if (interfaceInstance == null) {
                    gameObject.LogWarning($"The interface '{typeof(T).FullName}' component could not be found on this object!");
                }
            } else {
                interfaceInstance = null;
            }
        }
        #endregion
    }
}
