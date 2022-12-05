// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="Type"/>-related extension methods.
    /// </summary>
	public static class TypeExtensions {
        #region Content
        private static readonly Dictionary<Type, string> typeFriendlyNames = new Dictionary<Type, string> {
            {typeof(bool),      "bool"},
            {typeof(byte),      "byte"},
            {typeof(int),       "int"},
            {typeof(uint),      "uint"},
            {typeof(long),      "long"},
            {typeof(ulong),     "ulong"},
            {typeof(short),     "short"},
            {typeof(ushort),    "ushort"},
            {typeof(sbyte),     "sbyte"},
            {typeof(float),     "float"},
            {typeof(double),    "double"},
            {typeof(decimal),   "decimal"},
            {typeof(char),      "char"},
            {typeof(string),    "string"},
            {typeof(object),    "object"},
            {typeof(void),      "void"}
        };

        // -----------------------

        /// <summary>
        /// Get a user-friendly name for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="_type">The <see cref="Type"/> to get the associated name.</param>
        /// <returns>A friendly displayed name for the given type.</returns>
        public static string GetFriendlyName(this Type _type) {
            // Built-in type translation.
            if (typeFriendlyNames.TryGetValue(_type, out string _name)) {
                return _name;
            }

            // Array.
            if (_type.IsArray) {
                int _rank = _type.GetArrayRank();
                string _commas = (_rank > 1)
                               ? new string(',', _rank - 1)
                               : string.Empty;

                return GetFriendlyName(_type.GetElementType()) + $"[{_commas}]";
            }

            // Generic.
            if (_type.IsGenericType) {
                Type _nullable = Nullable.GetUnderlyingType(_type);

                if (_nullable != null) {
                    return $"{GetFriendlyName(_nullable)}?";
                }

                return $"{_type.Name.Split('`')[0]}<{string.Join(", ", Array.ConvertAll(_type.GetGenericArguments(), GetFriendlyName))}>";
            }

            return _type.Name;
        }

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
