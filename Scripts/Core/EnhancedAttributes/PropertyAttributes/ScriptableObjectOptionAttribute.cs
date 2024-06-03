// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Draws a <see cref="ScriptableObject"/> with additional options.
    /// </summary>
    public sealed class ScriptableObjectOptionAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <summary>
        /// Option mode used to draw this object.
        /// </summary>
        public readonly ScriptableObjectDrawerMode Mode = ScriptableObjectDrawerMode.None;

        /// <summary>
        /// Whether to draw this object field or not.
        /// </summary>
        public readonly bool DrawField = true;

        // -----------------------

        /// <param name="_mode"><inheritdoc cref="Mode" path="/summary"/></param>
        /// <param name="_drawField"><inheritdoc cref="DrawField" path="/summary"/></param>
        /// <inheritdoc cref="ScriptableObjectOptionAttribute"/>
        public ScriptableObjectOptionAttribute(ScriptableObjectDrawerMode _mode, bool _drawField = true) {
            Mode      = _mode;
            DrawField = _drawField;
        }
        #endregion
    }
}
