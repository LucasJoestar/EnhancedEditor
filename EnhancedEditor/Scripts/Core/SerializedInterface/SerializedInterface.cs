// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Base interface to derive all your serializable interfaces from.
    /// <para/>
    /// Must be implemented in a <see cref="MonoBehaviour"/> class and used with a <see cref="SerializedInterface{T}"/> instance.
    /// </summary>
    public interface ISerilizable { }

    /// <summary>
    /// Base class for the interface serializer system.
    /// Should NEVER be used directly, always use <see cref="SerializedInterface{T}"/>
    /// instead.
    /// <para/>
    /// This class only exist to allow the use of a custom property drawer for the derived class,
    /// as generic classes cannot have a custom drawer.
    /// </summary>
    [Serializable]
    public abstract class SerializedInterface
    {
        #region Content
        /// <summary>
        /// The <see cref="GameObject"/> the associated <see cref="ISerilizable"/> interface is attached to.
        /// </summary>
        [SerializeField] protected GameObject gameObject = null;
        #endregion
    }

    /// <summary>
    /// Interface wrapper, used to serialize an <see cref="ISerilizable"/> interface with the help
    /// of the <see cref="UnityEngine.GameObject"/> it is attached to.
    /// </summary>
    /// <typeparam name="T">Interface type to serialize.
    /// Must inherit from <see cref="ISerilizable"/>.</typeparam>
    [Serializable]
    #pragma warning disable UNT0014
    public class SerializedInterface<T> : SerializedInterface where T : ISerilizable
    {
        #region Content
        private T interfaceInstance = default;

        /// <summary>
        /// The <see cref="ISerilizable"/> interface attached to the serialized <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        public T Interface
        {
            get
            {
                // Get interface if not yet loaded.
                if ((interfaceInstance == null) && gameObject)
                {
                    interfaceInstance = gameObject.GetComponent<T>();
                }

                return interfaceInstance;
            }
        }

        /// <inheritdoc cref="SerializedInterface.gameObject"/>
        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                interfaceInstance = default;
                gameObject = value;
            }
        }
        #endregion
    }
}
