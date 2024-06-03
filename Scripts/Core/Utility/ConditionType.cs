// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor {
    /// <summary>
    /// Used to define how a condition is considered as fullfiled,
    /// notably used with <see cref="ShowIfAttribute"/> and <see cref="ButtonAttribute"/> attributes.
    /// </summary>
    public enum ConditionType {
        True  = 0,
        False = 1,
    }

    /// <summary>
    /// Contains multiple <see cref="ConditionType"/>-related extension methods.
    /// </summary>
    public static class ConditionTypeExtensions {
        #region Content
        /// <summary>
        /// Get this condition as a <see cref="bool"/> value.
        /// </summary>
        /// <param name="_condition"><see cref="ConditionType"/> to get value.</param>
        /// <returns>Condition as a <see cref="bool"/> value.</returns>
        public static bool Get(this ConditionType _condition) {
            switch (_condition) {
                case ConditionType.True:
                    return true;

                case ConditionType.False:
                    return false;

                default:
                    return true;
            }
        }
        #endregion
    }
}
