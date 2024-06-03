// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Color"/> struct.
    /// </summary>
	public static class ColorExtensions {
        #region Content
        /// <summary>
        /// Set the alpha value of this color.
        /// </summary>
        /// <param name="_color">Color to set alpha.</param>
        /// <param name="_alpha">New alpha value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetAlpha(this Color _color, float _alpha) {
            return new Color(_color.r, _color.g, _color.b, _alpha);
        }
        #endregion
    }
}
