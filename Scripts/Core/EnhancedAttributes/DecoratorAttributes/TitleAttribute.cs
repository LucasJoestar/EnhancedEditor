// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Draws a title above this field.
    /// <para/>
    /// A title consists of a left-centered underlined label, like a header but more elegant.
    /// </summary>
    public class TitleAttribute : EnhancedDecoratorAttribute {
        #region Global Members
        /// <summary>
        /// Label displayed as a title.
        /// </summary>
        public readonly GUIContent Label = null;

        // -----------------------

        /// <param name="_title"><see cref="string"/> text of this title.</param>
        /// <param name="_tooltip">Tooltip displayed on this title.</param>
        /// <inheritdoc cref="TitleAttribute"/>
        public TitleAttribute(string _title, string _tooltip = "") {
            Label = new GUIContent(_title, _tooltip);
        }
        #endregion
    }
}
