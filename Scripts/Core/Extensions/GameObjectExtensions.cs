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
    /// Contains multiple <see cref="GameObject"/>-related extension methods.
    /// </summary>
	public static class GameObjectExtensions
    {
        #region Content
        /// <summary>
        /// Adds a specific component to a <see cref="GameObject"/>
        /// if none of that type is already attached to it, and return it.
        /// </summary>
        /// <typeparam name="T">Component type to add.</typeparam>
        /// <param name="_gameObject"><see cref="GameObject"/> to add the component to.</param>
        /// <returns>Newly added or already attached component to the <see cref="GameObject"/>.</returns>
        public static T AddComponentIfNone<T>(this GameObject _gameObject) where T : Component
        {
            if (_gameObject.TryGetComponent(out T _component))
                return _component;

            return _gameObject.AddComponent<T>();
        }

        #if !UNITY_2019_2_OR_NEWER
        /// <summary>
        /// Gets the component of the specified type, if it exists.<br/>
        /// TryGetComponent will attempt to retrieve the component of the given type.The notable difference compared to
        /// GameObject.GetComponent is that this method does not allocate in the Editor when the requested component does not exist.
        /// </summary>
        /// <param name="_component">The output argument that will contain the component or null.</param>
        /// <returns>Returns true if the component is found, false otherwise.</returns>
        public static bool TryGetComponent<T>(this GameObject _gameObject, out T _component) where T : Component
        {
            _component = _gameObject.GetComponent<T>();
            return _component != null;
        }

        /// <inheritdoc cref="TryGetComponent{T}(GameObject, out T)"/>
        /// <param name="type">The type of the component to retrieve.</param>
        public static bool TryGetComponent(this GameObject _gameObject, Type type, out Component _component)
        {
            _component = _gameObject.GetComponent(type);
            return _component != null;
        }
        #endif
        #endregion
    }
}
