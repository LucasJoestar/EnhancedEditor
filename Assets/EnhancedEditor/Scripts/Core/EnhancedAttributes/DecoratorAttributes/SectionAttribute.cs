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

        public readonly GUIContent Label = null;
        public readonly float LineWidth = DefaultLineWidth;
        public readonly float Margins = DefaultMargins;

        // -----------------------

        /// <inheritdoc cref="SectionAttribute"/>
        /// <param name="_label">Label displayed in the middle of the section.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        /// <param name="_margins">Space on top and bottom of the section (in pixels).</param>
        public SectionAttribute(string _label, float _lineWidth = DefaultLineWidth, float _margins = DefaultMargins)
        {
            Label = new GUIContent(_label);
            LineWidth = Mathf.Max(0f, _lineWidth);
            Margins = Mathf.Max(0f, _margins);
        }
        #endregion
    }
}
