using System;
using System.Diagnostics;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class MaxAttribute : PropertyAttribute
    {
        #region Fields
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Maximum allowed value.
        /// </summary>
        public readonly float       MaxValue =      0;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Ensure that this field value will not exceed a maximum one.
        /// </summary>
        /// <param name="_maxValue">Maximum field value.</param>
        public MaxAttribute(float _maxValue)
        {
            MaxValue = _maxValue;
        }
        #endregion
    }
}
