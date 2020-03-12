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
        /// Default asset preview aspect (in pixels).
        /// </summary>
        public const float      DefaultAspect =     75;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Indicates if the asset preview is currently
        /// displayed in the editor or not.
        /// </summary>
        public bool             Fodlout =         true;


        /// <summary>
        /// Aspect of the asset preview displayed in the editor (in pixels).
        /// </summary>
        public readonly float   Aspect =            DefaultAspect;
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
        public AssetPreviewAttribute(float _aspect = DefaultAspect)
        {
            Aspect = Mathf.Max(1, _aspect);
        }
        #endregion
    }
}
