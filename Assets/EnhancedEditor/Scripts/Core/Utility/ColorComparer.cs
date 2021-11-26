// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Utility class used to compare multiple <see cref="Color"/>, which can be used to sort arrays or lists.
    /// </summary>
    public class ColorComparer : Comparer<Color>
    {
        #region Global Members
        /// <summary>
        /// Static instance comparer class, used to compare multiple <see cref="Color"/>.
        /// </summary>
        public static readonly ColorComparer Comparer = new ColorComparer();

        // -----------------------

        /// <summary>
        /// Compares two <see cref="Color"/> by their HSV values.
        /// </summary>
        /// <param name="_a">First of the two <see cref="Color"/> to compare.</param>
        /// <param name="_b">Second <see cref="Color"/> to compare.</param>
        /// <returns>Comparison value between the two colors.</returns>
        public override int Compare(Color _a, Color _b)
        {
            _a = ConvertToHSV(_a);
            _b = ConvertToHSV(_b);

            if (_a.r != _b.r)
                return _a.r.CompareTo(_b.r);

            if (_a.g != _b.g)
                return _a.g.CompareTo(_b.g);

            if (_a.b != _b.b)
                return _a.b.CompareTo(_b.b);

            return _a.a.CompareTo(_b.a);

            // ----- Local Method ----- \\

            Color ConvertToHSV(Color _color)
            {
                Color.RGBToHSV(_color, out float _h, out float _s, out float _v);
                return new Color(_h, _s, _v, _color.a);
            }
        }
        #endregion
    }
}
