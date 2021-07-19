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
    /// Should be used within a <see cref="SerializedInterface{T}"/> instance.
    /// </summary>
    public interface ISerilizable { }

    /// <summary>
    /// Base class for the interface serializer system.
    /// Should NEVER be used directly, always use <see cref="SerializedInterface{T}"/>
    /// instead.
    /// <para/>
    /// This class only exist to use a custom property drawer for the derived class.
    /// </summary>
    [Serializable]
    public abstract class SerializedInterface
    {
        #region Content
        /// <summary>
        /// Serialized object the associated <see cref="ISerilizable"/> is attached to.
        /// </summary>
        public GameObject Object = null;
        #endregion
    }

    /// <summary>
    /// Interface wrapper, serializing a <see cref="GameObject"/> reference
    /// with a <see cref="ISerilizable"/> interface attached to it.
    /// <para/>
    /// Must be initialized with <see cref="Initialize"/> on start
    /// to properly load its interface reference.
    /// </summary>
    /// <typeparam name="T">Interface type to serialize.
    /// Must inherit from <see cref="ISerilizable"/>.</typeparam>
    [Serializable]
    public class SerializedInterface<T> : SerializedInterface where T : ISerilizable
    {
        #region Content
        /// <summary>
        /// Associated serialized reference
        /// (should call <see cref="Initialize"/> to properly load it before any use).
        /// </summary>
        public T Interface = default;

        // -----------------------

        /// <summary>
        /// Initializes this object interface reference.
        /// </summary>
        public void Initialize()
        {
            Interface = Object.GetComponent<T>();
        }
        #endregion
    }
}
