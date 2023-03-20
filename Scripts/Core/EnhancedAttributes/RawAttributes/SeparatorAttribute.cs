// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="SeparatorAttribute"/>-related draw position.
    /// </summary>
    [Flags]
    public enum SeparatorPosition {
        None    = 0,
        Top     = 1,
        Bottom  = 2,
    }

    /// <summary>
    /// Use this attribute on an <see cref="Enum"/> value to draw a separator when selecting it from an editor popup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class SeparatorAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// Position where to draw a separator.
        /// </summary>
        public readonly SeparatorPosition Position = SeparatorPosition.None;

        // -----------------------

        /// <param name="_position"><inheritdoc cref="Position" path="/summary"/></param>
        /// <inheritdoc cref="SeparatorAttribute"/>
        public SeparatorAttribute(SeparatorPosition _position = SeparatorPosition.None) {
            Position = _position;
        }
        #endregion
    }
}
