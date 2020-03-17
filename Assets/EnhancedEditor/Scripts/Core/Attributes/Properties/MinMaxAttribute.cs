using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinMaxAttribute : PropertyAttribute
    {
        #region Fields
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Maximum allowed value.
        /// </summary>
        public readonly float       MaxValue =      0;

        /// <summary>
        /// Minimum allowed value.
        /// </summary>
        public readonly float       Minvalue =      0;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Vector2 attribute used to manipulate a minimum and a maximum value respectively
        /// in the vector x & y fields, restrained by minimum and maximum allowed values.
        /// </summary>
        /// <param name="_minValue">Minimum allowed value.</param>
        /// <param name="_maxValue">Maximum allwoed value.</param>
        public MinMaxAttribute(float _minValue, float _maxValue)
        {
            Minvalue = _minValue;
            MaxValue = _maxValue;
        }
        #endregion
    }
}
