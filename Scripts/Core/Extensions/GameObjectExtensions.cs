// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="GameObject"/>-related extension methods.
    /// </summary>
	public static class GameObjectExtensions {
        #region Content
        /// <summary>
        /// Sets the layer of this <see cref="GameObject"/> and all its children.
        /// </summary>
        /// <param name="_gameObject"><see cref="GameObject"/> instance to set layer.</param>
        /// <param name="_layer">Layer to assign.</param>
        public static void SetLayer(this GameObject _gameObject, int _layer) {

            _gameObject.layer = _layer;

            foreach (Transform _child in _gameObject.transform) {
                SetLayer(_child.gameObject, _layer);
            }
        }

        /// <summary>
        /// Adds a specific component to a <see cref="GameObject"/>
        /// if none of that type is already attached to it, and return it.
        /// </summary>
        /// <typeparam name="T">Component type to add.</typeparam>
        /// <param name="_gameObject"><see cref="GameObject"/> to add the component to.</param>
        /// <returns>Newly added or already attached component to the <see cref="GameObject"/>.</returns>
        public static T AddComponentIfNone<T>(this GameObject _gameObject) where T : Component {
            if (_gameObject.TryGetComponent(out T _component))
                return _component;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return Undo.AddComponent<T>(_gameObject);
            }
            #endif

            return _gameObject.AddComponent<T>();
        }

        /// <param name="_componentType">The type of component to add.</param>
        /// <inheritdoc cref="AddComponentIfNone{T}(GameObject)"/>
        public static Component AddComponentIfNone(this GameObject _gameObject, Type _componentType) {
            if (_gameObject.TryGetComponent(_componentType, out Component _component))
                return _component;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return Undo.AddComponent(_gameObject, _componentType);
            }
            #endif

            return _gameObject.AddComponent(_componentType);
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

        #region Extended Behaviour
        /// <summary>
        /// Get the <see cref="ExtendedBehaviour"/> attached to this <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_gameObject">This <see cref="GameObject"/> to get the <see cref="ExtendedBehaviour"/> from.</param>
        /// <returns>The <see cref="ExtendedBehaviour"/> attached to this <see cref="GameObject"/>.</returns>
        public static ExtendedBehaviour GetExtendedBehaviour(this GameObject _gameObject) {
            return _gameObject.GetComponent<ExtendedBehaviour>();
        }

        /// <summary>
        /// Get all <see cref="Tag"/> assigned to this <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_gameObject">This <see cref="GameObject"/> to get the tags from.</param>
        /// <returns>The <see cref="TagGroup"/> containing all tag assigned to this <see cref="GameObject"/>.</returns>
        public static TagGroup GetTags(this GameObject _gameObject) {
            if (_gameObject.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                return _behaviour.Tags;
            }

            _gameObject.LogWarning($"No {typeof(ExtendedBehaviour).Name} could be found on the object \'{_gameObject.name}\'.");
            return new TagGroup();
        }

        /// <summary>
        /// Set the <see cref="Tag"/> of this <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_gameObject">This <see cref="GameObject"/> to set the tags.</param>
        /// <param name="_tags"><see cref="TagGroup"/> to assign to this object.</param>
        /// <returns>True if the <see cref="GameObject"/> tags could be successfully assigned, false otherwise.</returns>
        public static bool SetTags(this GameObject _gameObject, TagGroup _tags) {
            if (_gameObject.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                _behaviour.Tags = _tags;
                return true;
            }

            _gameObject.LogWarning($"No {typeof(ExtendedBehaviour).Name} could be found on the object \'{_gameObject.name}\'.");
            return false;
        }

        /// <summary>
        /// Get if this <see cref="GameObject"/> has a specific <see cref="Tag"/>.
        /// </summary>
        /// <param name="_gameObject"><see cref="GameObject"/> to check.</param>
        /// <param name="_tag"><see cref="Tag"/> to check.</param>
        /// <returns>True if the <see cref="GameObject"/> has the given <see cref="Tag"/>, false otherwise.</returns>
        public static bool HasTag(this GameObject _gameObject, Tag _tag) {
            if (_gameObject.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                return _behaviour.Tags.Contains(_tag);
            }

            return false;
        }

        /// <summary>
        /// Get if this <see cref="GameObject"/> has all tags in a given <see cref="TagGroup"/>.
        /// </summary>
        /// <param name="_gameObject"><see cref="GameObject"/> to check.</param>
        /// <param name="_tags"><see cref="TagGroup"/> to check.</param>
        /// <returns>True if the <see cref="GameObject"/> has all the tags in the given <see cref="TagGroup"/>, false otherwise.</returns>
        public static bool HasTags(this GameObject _gameObject, TagGroup _tags) {
            if (_gameObject.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                return _behaviour.Tags.ContainsAll(_tags);
            }

            return false;
        }

        /// <summary>
        /// Get if this <see cref="GameObject"/> has any tag in a given <see cref="TagGroup"/>.
        /// </summary>
        /// <param name="_gameObject"><see cref="GameObject"/> to check.</param>
        /// <param name="_tags"><see cref="TagGroup"/> to check.</param>
        /// <returns>True if the <see cref="GameObject"/> has any tag in the given <see cref="TagGroup"/>, false otherwise.</returns>
        public static bool HasAnyTag(this GameObject _gameObject, TagGroup _tags) {
            if (_gameObject.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                return _behaviour.Tags.ContainsAny(_tags);
            }

            return false;
        }
        #endregion
    }
}
