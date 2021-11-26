// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

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
        #endregion
    }
}
