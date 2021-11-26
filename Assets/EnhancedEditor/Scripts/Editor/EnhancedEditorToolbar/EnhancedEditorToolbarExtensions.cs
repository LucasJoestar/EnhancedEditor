// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Allows you to draw your own extension controls on the main editor toolbar.
    /// <br/>Extensions can be drawn whether on the left or on the right side of the play mode buttons.
    /// <para/>
    /// The methods must be static and without any argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class EditorToolbarExtension : Attribute
    {
        #region Global Members
        /// <summary>
        /// Order in which the extensions should be drawn in.
        /// </summary>
        public int Order { get; set; } = 0;
        #endregion
    }

    /// <inheritdoc cref="EditorToolbarExtension"/>
    /// <remarks>Order is from right (smaller) to left (greater).</remarks>
    public class EditorToolbarLeftExtension : EditorToolbarExtension { }

    /// <inheritdoc cref="EditorToolbarExtension"/>
    /// <remarks>Order is from left (smaller) to right (greater).</remarks>
    public class EditorToolbarRightExtension : EditorToolbarExtension { }
}
