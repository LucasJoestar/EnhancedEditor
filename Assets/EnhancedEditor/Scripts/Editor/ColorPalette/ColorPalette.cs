// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// A color palette can be used to stock multiple different colors in one place,
    /// where they can then be easily retrieved and used.
    /// </summary>
    [Serializable]
    public class ColorPalette
    {
        #region Global Members
        /// <summary>
        /// Name of this color palette.
        /// </summary>
        public string Name = "New Color Palette";

        /// <summary>
        /// All colors contained in this palette.
        /// </summary>
        public Color[] Colors = new Color[] { Color.black, Color.white };

        [SerializeField, HideInInspector] internal string guid = string.Empty;
        [SerializeField, HideInInspector] internal bool isPersistent = false;

        /// <summary>
        /// Total amount of colors in this palette.
        /// </summary>
        public int Count => Colors.Length;

        // -----------------------

        /// <inheritdoc cref="ColorPalette(ColorPalette)"/>
        public ColorPalette() { }

        /// <summary>
        /// Creates a new color palette that can be used to stock different colors in one place.
        /// </summary>
        /// <param name="_palette"><see cref="ColorPalette"/> used to initialize this palette colors with.</param>
        public ColorPalette(ColorPalette _palette)
        {
            Colors = new Color[_palette.Colors.Length];
            Array.Copy(_palette.Colors, Colors, Count);
        }
        #endregion
    }
}
