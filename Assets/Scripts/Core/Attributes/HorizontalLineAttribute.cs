using System;
using UnityEngine;

namespace EnhancedAttributes
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
        public const float DefaultHeight = 2f;

        /// <summary>
        /// Default line color.
        /// </summary>
        public const EnhancedColor DefaultColor = EnhancedColor.Blue;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// This line height (in pixels).
        /// </summary>
        public readonly float Height = DefaultHeight;

        /// <summary>
        /// This line color.
        /// </summary>
        public readonly EnhancedColor Color = DefaultColor;
        #endregion

        #region Constructor
        /*******************************
         *******   CONSTRUCTOR   *******
         ******************************/
        
        /// <summary>
        /// Draw a horizontal line of customizable height and color.
        /// </summary>
        public HorizontalLineAttribute(float _height = DefaultHeight, EnhancedColor _color = DefaultColor)
        {
            Height = _height;
            Color = _color;
        }
        #endregion
    }
}
