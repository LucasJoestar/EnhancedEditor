// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExtendedBehaviour GetExtendedBehaviour(this Component _component) {
            return _component.GetComponent<ExtendedBehaviour>();
        }

        /// <param name="_component"><see cref="Component"/> to check.</param>
        /// <inheritdoc cref="GameObjectExtensions.GetTags"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetTags(this Component _component, out TagGroup _tags) {
            return _component.gameObject.GetTags(out _tags);
        }

        /// <param name="_component"><see cref="Component"/> to check.</param>
        /// <inheritdoc cref="GameObjectExtensions.HasTag(GameObject, Tag)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTag(this Component _component, Tag _tag) {
            return _component.gameObject.HasTag(_tag);
        }

        /// <param name="_component"><see cref="Component"/> to check.</param>
        /// <inheritdoc cref="GameObjectExtensions.HasTags(GameObject, TagGroup)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTags(this Component _component, TagGroup _tags) {
            return _component.gameObject.HasTags(_tags);
        }

        /// <param name="_component"><see cref="Component"/> to check.</param>
        /// <inheritdoc cref="GameObjectExtensions.HasAnyTag(GameObject, TagGroup)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyTag(this Component _component, TagGroup _tags, bool validIfEmpty = false) {
            return _component.gameObject.HasAnyTag(_tags, validIfEmpty);
        }
        #endregion

        #region Component
        /// <param name="_component">This <see cref="Component"/> instance.</param>
        /// <inheritdoc cref="GameObjectExtensions.TryGetComponentInParent"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponentInParent<T>(this Component _component, out T _searchComponent, bool _includeInactive = false) {
            return _component.gameObject.TryGetComponentInParent(out _searchComponent, _includeInactive);
        }

        /// <param name="_component">This <see cref="Component"/> instance.</param>
        /// <inheritdoc cref="GameObjectExtensions.TryGetComponentInChildren"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponentInChildren<T>(this Component _component, out T _searchComponent, bool _includeInactive = false) {
            return _component.gameObject.TryGetComponentInChildren(out _searchComponent, _includeInactive);
        }

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
