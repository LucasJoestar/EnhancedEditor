// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="Type"/>-related extension methods.
    /// </summary>
	public static class TypeExtensions {
        #region Content
        /// <summary>
        /// Get the full reflection name of this type, which can then be used to recreate it using <see cref="Type.GetType(string)"/>.
        /// </summary>
        /// <param name="_type"><see cref="Type"/> to get the reflection name.</param>
        /// <returns>Full reflection name from this type..</returns>
        public static string GetReflectionName(this Type _type) {
            return $"{_type.FullName}, {_type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// Get if a specific <see cref="Type"/> is the same or a subclass of another <see cref="Type"/>.
        /// </summary>
        /// <param name="_derived">The potential derived type.</param>
        /// <param name="_base">The potential base type.</param>
        /// <returns>True if the derived type is the same or a subclass of the base, false otherwise.</returns>
        public static bool IsSameOrSubclass(this Type _derived, Type _base) {
            return (_derived == _base) || _derived.IsSubclassOf(_base);
        }

        /// <summary>
        /// Get if a specific <see cref="Type"/> is a subclass of another <see cref="Type"/>, even if it is generic.
        /// </summary>
        /// <param name="_derived">The potential derived type.</param>
        /// <param name="_base">The potential base type.</param>
        /// <returns>True if the derived type is a subclass of the base, false otherwise.</returns>
        public static bool IsSubclassOfGeneric(this Type _derived, Type _base) {
            while ((_derived != null) && (_derived != typeof(object))) {
                var cur = _derived.IsGenericType ? _derived.GetGenericTypeDefinition() : _derived;
                if (_base == cur) {
                    return true;
                }

                _derived = _derived.BaseType;
            }

            return false;
        }
        #endregion
    }
}
