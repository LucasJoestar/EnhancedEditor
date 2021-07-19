// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a <see cref="Vector2"/> or a <see cref="Vector2Int"/> as a min-max slider,
    /// used to specify a range between a min and a max.
    /// </summary>
    public class MinMaxAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly float MinValue = 0f;
        public readonly float MaxValue = 0f;

        // -----------------------

        /// <inheritdoc cref="MinMaxAttribute"/>
        /// <param name="_minValue">Minimum slider value.</param>
        /// <param name="_maxValue">Maximum slider value.</param>
        public MinMaxAttribute(float _minValue, float _maxValue)
        {
            MinValue = _minValue;
            MaxValue = _maxValue;
        }
        #endregion
    }
}
