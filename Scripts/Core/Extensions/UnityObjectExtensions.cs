// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Object"/> class.
    /// </summary>
	public static class UnityObjectExtensions {
        #region Content
        /// <summary>
        /// Indicates if this object is null using the ReferenceEquals method.
        /// <br/> Might not work properly if the object has been destroyed.
        /// </summary>
        /// <param name="_unityObject">Object to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Object _unityObject) {
            return _unityObject is null;
        }

        /// <summary>
        /// Indicates if this object is not null using the ReferenceEquals method.
        /// <br/> Might not work properly if the object has been destroyed.
        /// </summary>
        /// <param name="_unityObject">Object to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this Object _unityObject) {
            return !IsNull(_unityObject);
        }
        #endregion
    }
}
