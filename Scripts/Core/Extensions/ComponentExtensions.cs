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
    /// Contains multiple <see cref="Component"/>-related extension methods.
    /// </summary>
	public static class ComponentExtensions
    {
        #region Content
#if !UNITY_2019_2_OR_NEWER
        /// <summary>
        /// Gets the component of the specified type, if it exists.<br/>
        /// TryGetComponent will attempt to retrieve the component of the given type.The notable difference compared to
        /// GameObject.GetComponent is that this method does not allocate in the Editor when the requested component does not exist.
        /// </summary>
        /// <param name="_component">The output argument that will contain the component or null.</param>
        /// <returns>Returns true if the component is found, false otherwise.</returns>
        public static bool TryGetComponent<T>(this Component _source, out T _component) where T : Component
        {
            _component = _source.GetComponent<T>();
            return _component != null;
        }

        /// <inheritdoc cref="TryGetComponent{T}(GameObject, out T)"/>
        /// <param name="type">The type of the component to retrieve.</param>
        public static bool TryGetComponent(this Component _source, Type type, out Component _component)
        {
            _component = _source.GetComponent(type);
            return _component != null;
        }
#endif
        #endregion
    }
}
