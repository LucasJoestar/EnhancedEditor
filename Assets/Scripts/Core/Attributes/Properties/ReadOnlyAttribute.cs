using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        #region Fields
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// If associated field is boolean, indicates if using radio toggle
        /// instead or classic one.
        /// </summary>
        public readonly bool    UseRadioToggle =    true;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Draw a non editable field in the inspector.
        /// </summary>
        /// <param name="_useRadioToggle">If associated field is boolean, indicates if using radio toggle or classic one.</param>
        public ReadOnlyAttribute(bool _useRadioToggle = true)
        {
            UseRadioToggle = _useRadioToggle;
        }
        #endregion
    }
}
