// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor.Editor {
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

    /// <inheritdoc cref="EnhancedEditorSettingsAttribute"/>
    /// <remarks>Use this to draw user-related preferences settings.</remarks>
    public class EnhancedEditorUserSettingsAttribute : EnhancedEditorSettingsAttribute { }

    /// <inheritdoc cref="EnhancedEditorSettingsAttribute"/>
    /// <remarks>Use this to global shared project settings.
    /// <br/> The method must take a <see cref="EnhancedEditorProjectSettings"/> parameter.</remarks>
    public class EnhancedEditorProjectSettingsAttribute : EnhancedEditorSettingsAttribute { }
}
