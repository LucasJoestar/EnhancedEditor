// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;

namespace EnhancedEditor.Editor {
    // ===== Base ===== \\
    
    /// <summary>
    /// Allows you to draw your own settings controls in the Enhanced Editor preferences window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class EnhancedEditorSettingsAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// Order in which the settings should be drawn in.
        /// </summary>
        public int Order { get; set; } = 0;
        #endregion
    }

    // ===== Derived ===== \\

    /// <inheritdoc cref="EnhancedEditorSettingsAttribute"/>
    /// <remarks>Use this to global shared project settings.
    /// <br/> The method must take a <see cref="EnhancedEditorProjectSettings"/> parameter.</remarks>
    public sealed class EnhancedEditorProjectSettingsAttribute : EnhancedEditorSettingsAttribute { }

    /// <inheritdoc cref="EnhancedEditorSettingsAttribute"/>
    /// <remarks>Use this to draw user-related preferences settings.</remarks>
    public sealed class EnhancedEditorUserSettingsAttribute    : EnhancedEditorSettingsAttribute { }
}
