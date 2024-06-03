// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Utility class used to compare multiple <see cref="Color"/>, which can be used to sort arrays or lists.
    /// </summary>
    public sealed class ColorComparer : Comparer<Color> {
        #region Global Members
        /// <summary>
        /// Static instance comparer class, used to compare multiple <see cref="Color"/>.
        /// </summary>
        public static readonly ColorComparer Comparer = new ColorComparer();

        // -----------------------

        /// <summary>
        /// Compares two <see cref="Color"/> by their HSV values.
        /// </summary>
        /// <param name="a">First of the two <see cref="Color"/> to compare.</param>
        /// <param name="b">Second <see cref="Color"/> to compare.</param>
        /// <returns>Comparison value between the two colors.</returns>
        public override int Compare(Color a, Color b) {
            a = ConvertToHSV(a);
            b = ConvertToHSV(b);

            if (a.r != b.r)
                return a.r.CompareTo(b.r);

            if (a.g != b.g)
                return a.g.CompareTo(b.g);

            if (a.b != b.b)
                return a.b.CompareTo(b.b);

            return a.a.CompareTo(b.a);

            // ----- Local Method ----- \\

            static Color ConvertToHSV(Color _color) {
                Color.RGBToHSV(_color, out float _h, out float _s, out float _v);
                return new Color(_h, _s, _v, _color.a);
            }
        }
        #endregion
    }
}
