// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
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
    public class SerializedInterface<T> where T : class {
        #region Global Members
        [SerializeField] protected GameObject gameObject = null;
        private T interfaceInstance = default;

        /// <summary>
        /// The <see cref="GameObject"/> the associated interface is attached to.
        /// </summary>
        public GameObject GameObject {
            get => gameObject;
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
        #endregion

        #region Operators
        public static implicit operator T(SerializedInterface<T> _interface) {
            return _interface.interfaceInstance;
        }

        public static implicit operator GameObject(SerializedInterface<T> _interface) {
            return _interface.gameObject;
        }

        public static implicit operator SerializedInterface<T>(Type _interface) {
            return typeof(SerializedType<>).MakeGenericType(_interface);
        }

        public override string ToString() {
            return (interfaceInstance != null) ? interfaceInstance.ToString() : "[Null Interface]";
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
