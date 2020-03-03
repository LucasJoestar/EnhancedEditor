using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : PropertyAttribute
    {
        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Draw a field for a required object reference, with an error help box
        /// if set to null (useful to remind user from setting object value).
        /// </summary>
        public RequiredAttribute() { }
        #endregion
    }
}
