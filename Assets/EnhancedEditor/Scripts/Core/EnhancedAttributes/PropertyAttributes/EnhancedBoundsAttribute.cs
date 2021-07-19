// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Implements a visual representation for a <see cref="Bounds"/> field,
    /// with easy to edit controls.
    /// </summary>
	public class EnhancedBoundsAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.Green;

        public readonly Color Color = default;

        // -----------------------

        /// <inheritdoc cref="EnhancedBoundsAttribute"/>
        /// <param name="_color">Color of the bounds visual representation.</param>
        public EnhancedBoundsAttribute(SuperColor _color = DefaultColor)
        {
            Color = _color.Get();
        }
        #endregion
    }
}
