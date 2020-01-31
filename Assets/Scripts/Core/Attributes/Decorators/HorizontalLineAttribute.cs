using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class HorizontalLineAttribute : PropertyAttribute
    {
        #region Fields
        /*****************************
         *******   CONSTANTS   *******
         ****************************/

        /// <summary>
        /// Default line height (in pixels).
        /// </summary>
        public const float              DefaultHeight =     2f;

        /// <summary>
        /// Default line width (in pixels) ; 0 is full width.
        /// </summary>
        public const float              DefaultWidth =     0f;

        /// <summary>
        /// Default line color.
        /// </summary>
        public const SuperColor         DefaultColor =      SuperColor.Gray;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// This line height (in pixels).
        /// </summary>
        public readonly float           Height =            DefaultHeight;

        /// <summary>
        /// This line height (in pixels) ; 0 is full width.
        /// </summary>
        public readonly float           Width =             DefaultWidth;

        /// <summary>
        /// This line color.
        /// </summary>
        public readonly SuperColor      Color =             DefaultColor;
        #endregion

        #region Constructor
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Draw a horizontal line of customizable height and color.
        /// </summary>
        /// <param name="_height">Line height (in pixels).</param>
        /// <param name="_color">Line color.</param>
        public HorizontalLineAttribute(float _height = DefaultHeight, SuperColor _color = DefaultColor)
        {
            Height = Mathf.Max(1, _height);
            Color = _color;
        }

        /// <summary>
        /// Draw a horizontal line of customizable height, width and color.
        /// </summary>
        /// <param name="_height">Line height (in pixels).</param>
        /// <param name="_width">Line width (in pixels).</param>
        /// <param name="_color">Line color.</param>
        public HorizontalLineAttribute(float _height, float _width, SuperColor _color = DefaultColor) : this(_height, _color)
        {
            Width = Mathf.Max(0, _width);
        }
        #endregion
    }
}
