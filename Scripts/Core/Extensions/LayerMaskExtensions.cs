// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple <see cref="LayerMask"/>-related extension methods.
    /// </summary>
	public static class LayerMaskExtensions {
        #region Content
        /// <summary>
        /// Get if a specific layer is contained within this mask.
        /// </summary>
        /// <param name="_mask">The layer mask to check content.</param>
        /// <param name="_layer">The layer to be included in the mask.</param>
        /// <returns>True if this layer mask contains the given layer, false otherwise.</returns>
        public static bool Contains(this LayerMask _mask, int _layer) {
            return _mask == (_mask | (1 << _layer));
        }
        #endregion
    }
}
