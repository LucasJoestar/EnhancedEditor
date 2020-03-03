using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AssetPreviewAttribute : PropertyAttribute
    {
        #region Fields
        /*****************************
         *******   CONSTANTS   *******
         ****************************/

        /// <summary>
        /// Default asset preview height (in pixels).
        /// </summary>
        public const float      DefaultHeight =     75;

        /// <summary>
        /// Default asset preview width (in pixels).
        /// </summary>
        public const float      DefaultWidth =      75;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Indicates if the asset preview is currently
        /// displayed in the editor or not.
        /// </summary>
        public bool             IsVisible =         true;


        /// <summary>
        /// Height of the asset preview displayed in the editor (in pixels).
        /// </summary>
        public readonly float   Height =            DefaultHeight;

        /// <summary>
        /// Width of the asset preview displayed in the editor (in pixels).
        /// </summary>
        public readonly float   Width =             DefaultWidth;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Display a preview of the asset assigned to this field.
        /// </summary>
        /// <param name="_width">Asset preview width (in pixels).</param>
        /// <param name="_height">Asset preview height (in pixels).</param>
        public AssetPreviewAttribute(float _width = DefaultWidth, float _height = DefaultHeight)
        {
            Width = Mathf.Max(1, _width);
            Height = Mathf.Max(1, _height);
        }
        #endregion
    }
}
