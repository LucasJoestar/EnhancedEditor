// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Implements a visual representation of this field <see cref="Bounds"/> value in the scene,
    /// with easy-to-edit controls.
    /// </summary>
	public class EnhancedBoundsAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.Green;

        /// <summary>
        /// <see cref="UnityEngine.Color"/> used for the visual representation of these bounds.
        /// </summary>
        public readonly Color Color = default;

        // -----------------------

        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <inheritdoc cref="EnhancedBoundsAttribute"/>
        public EnhancedBoundsAttribute(SuperColor _color = DefaultColor)
        {
            Color = _color.Get();
        }
        #endregion
    }
}
