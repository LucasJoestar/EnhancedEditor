// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Diagnostics;
using System;

namespace EnhancedEditor {
    /// <summary>
    /// Specifies a draw order value for this property field in the inspector.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyOrderAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// Draw order of this property field.
        /// <br/> Negative value to be drawn on top, 0 as default, and positive values for bottom.
        /// </summary>
        public readonly int PropertyOrder = 0;

        // -----------------------

        /// <param name="_order"><inheritdoc cref="PropertyOrder" path="/summary"/></param>
        /// <inheritdoc cref="PropertyOrderAttribute"/>
        public PropertyOrderAttribute(int _order = 0) {
            PropertyOrder = _order;
        }
        #endregion
    }
}
