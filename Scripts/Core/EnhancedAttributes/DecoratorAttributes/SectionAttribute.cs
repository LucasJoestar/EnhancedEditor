// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a section above this field.
    /// <para/>
    /// A section is a middle-centered label surrounded by horizontal lines, like a header but more elegant.
    /// </summary>
    public class SectionAttribute : EnhancedDecoratorAttribute
    {
        #region Global Members
        public const float DefaultLineWidth = 50f;
        public const float DefaultMargins = 5f;

        /// <summary>
        /// Label displayed as this section header.
        /// </summary>
        public readonly GUIContent Label = null;

        /// <summary>
        /// Width of the lines surrounding the label (in pixels).
        /// </summary>
        public readonly float LineWidth = DefaultLineWidth;

        /// <summary>
        /// Space on top and bottom of the section (in pixels).
        /// </summary>
        public readonly float Margins = DefaultMargins;

        // -----------------------

        /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
        /// <param name="_lineWidth"><inheritdoc cref="LineWidth" path="/summary"/></param>
        /// <param name="_margins"><inheritdoc cref="Margins" path="/summary"/></param>
        /// <inheritdoc cref="SectionAttribute"/>
        public SectionAttribute(string _label, float _lineWidth = DefaultLineWidth, float _margins = DefaultMargins)
        {
            Label = new GUIContent(_label);
            LineWidth = Mathf.Max(0f, _lineWidth);
            Margins = Mathf.Max(0f, _margins);
        }
        #endregion
    }
}
