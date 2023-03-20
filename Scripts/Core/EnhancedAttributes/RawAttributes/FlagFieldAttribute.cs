// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Use this <see cref="Attribute"/> on a <see cref="FlagsAttribute"/> enum field to specify whether to draw a mask or an int field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class FlagFieldAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// If true, draws a mask selection field for this enum.
        /// <br/> Otherwise, draw an int selection field.
        /// </summary>
        public readonly bool MaskField = false;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_maskField"><inheritdoc cref="MaskField" path="/summary"/></param>
        /// <inheritdoc cref="FlagFieldAttribute"/>
        public FlagFieldAttribute(bool _maskField = false) {
            MaskField = _maskField;
        }
        #endregion
    }
}
