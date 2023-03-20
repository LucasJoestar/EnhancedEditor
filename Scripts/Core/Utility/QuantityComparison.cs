// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Comparison methods used to compare two values.
    /// </summary>
    public enum Comparison {
        Equal               = 0,

        LessThan            = 1,
        GreaterThan         = 2,

        LessThanOrEqual     = 3,
        GreaterThanOrEqual  = 4,
    }

    /// <summary>
    /// Contains multiple <see cref="Comparison"/>-related extension methods.
    /// </summary>
    public static class ComparisonExtensions {
        #region Content
        /// <summary>
        /// Compares two <see cref="int"/> values according to a specific <see cref="Comparison"/> method.
        /// </summary>
        /// <param name="_comparison">Comparison method to use.</param>
        /// <param name="_value">Source value to compare.</param>
        /// <param name="_other">Other value to compare to.</param>
        /// <returns>True if the comparison is validated, false otherwise.</returns>
        public static bool Compare(this Comparison _comparison, int _value, int _other) {
            switch (_comparison) {

                case Comparison.Equal:
                    return _value == _other;

                case Comparison.LessThan:
                    return _value < _other;

                case Comparison.GreaterThan:
                    return _value > _other;

                case Comparison.LessThanOrEqual:
                    return _value <= _other;

                case Comparison.GreaterThanOrEqual:
                    return _value >= _other;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Compares two <see cref="float"/> values according to a specific <see cref="Comparison"/> method.
        /// </summary>
        /// <param name="_comparison">Comparison method to use.</param>
        /// <param name="_value">Source value to compare.</param>
        /// <param name="_other">Other value to compare to.</param>
        /// <returns>True if the comparison is validated, false otherwise.</returns>
        public static bool Compare(this Comparison _comparison, float _value, float _other) {
            switch (_comparison) {

                case Comparison.Equal:
                    return Mathf.Approximately(_value, _other);

                case Comparison.LessThan:
                    return _value < _other;

                case Comparison.GreaterThan:
                    return _value > _other;

                case Comparison.LessThanOrEqual:
                    return Mathf.Approximately(_value, _other) || (_value < _other);

                case Comparison.GreaterThanOrEqual:
                    return Mathf.Approximately(_value, _other) || (_value > _other);

                default:
                    return false;
            }
        }
        #endregion
    }
}
