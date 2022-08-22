// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

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
        #endregion
    }
}
