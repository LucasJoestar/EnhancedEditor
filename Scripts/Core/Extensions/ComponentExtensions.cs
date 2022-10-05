// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="Component"/>-related extension methods.
    /// </summary>
	public static class ComponentExtensions {
        #region Extended Behaviour
        /// <summary>
        /// Get the <see cref="ExtendedBehaviour"/> attached to this component <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_component">This component to get the <see cref="ExtendedBehaviour"/> from.</param>
        /// <returns>The <see cref="ExtendedBehaviour"/> attached to this component <see cref="GameObject"/>.</returns>
        public static ExtendedBehaviour GetExtendedBehaviour(this Component _component) {
            return _component.GetComponent<ExtendedBehaviour>();
        }

        /// <summary>
        /// Get all <see cref="Tag"/> assigned to this component <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_component">This component to get the tags from.</param>
        /// <returns>The <see cref="TagGroup"/> containing all tag assigned to this component <see cref="GameObject"/>.</returns>
        public static TagGroup GetTags(this Component _component) {
            if (_component.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                return _behaviour.Tags;
            }

            _component.LogWarning($"No {typeof(ExtendedBehaviour).Name} could be found on the object \'{_component.gameObject.name}\'.");
            return new TagGroup();
        }
        #endregion

        #region Utility
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
