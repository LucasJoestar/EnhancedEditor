using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SectionAttribute : PropertyAttribute
    {
        #region Fields
        /*****************************
         *******   CONSTANTS   *******
         ****************************/

        /// <summary>
        /// Default space on top and bottom of the section (in pixels).
        /// </summary>
        public const float              DefaultHeightSpace =    5;

        /// <summary>
        /// Default width of the lines surrounding the label (in pixels).
        /// </summary>
        public const float              DefaultLineWidth =      50;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Space on top and bottom of the section (in pixels).
        /// </summary>
        public readonly float           HeightSpace =           DefaultHeightSpace;

        /// <summary>
        /// Width of the lines surrounding the label (in pixels).
        /// </summary>
        public readonly float           LineWidth =             DefaultLineWidth;

        /// <summary>
        /// Label of the section.
        /// </summary>
        public readonly string          Label =                 string.Empty;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Draw a label surrounded by horizontal lines.
        /// </summary>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        /// <param name="_heightSpace">Space on top and bottom of the section (in pixels)</param>
        public SectionAttribute(string _label, float _lineWidth = DefaultLineWidth, float _heightSpace = DefaultHeightSpace)
        {
            Label = _label;
            LineWidth = Mathf.Max(0, _lineWidth);
            HeightSpace = Mathf.Max(0, _heightSpace);
        }
        #endregion
    }
}
