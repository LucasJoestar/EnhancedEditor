// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Diagnostics;

namespace EnhancedEditor
{
    /// <summary>
    /// Base class to derive custom method attributes from.
    /// <para/>
    /// A custom attribute can be hooked up with a custom drawer
    /// to get callbacks when the target script editor is being drawn in the inspector.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class EnhancedMethodAttribute : Attribute
    {
        #region Global Members
        /// <summary>
        /// Optional field to specify the order that multiple <see cref="EnhancedMethodAttribute"/> should be drawn in.
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Determines if associated drawer call should be performed before or after
        /// drawing target script editor (default is true).
        /// </summary>
        public bool IsDrawnOnTop { get; set; } = true;

        /// <summary>
        /// Tooltip displayed on mouse hover.
        /// </summary>
        public string Tooltip { get; set; } = string.Empty;
        #endregion
    }
}
