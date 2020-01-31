using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PropertyFieldAttribute : PropertyAttribute
    {
        #region Fields
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Name of the property to set when changing field value.
        /// </summary>
        public readonly string PropertyName = string.Empty;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// When this field value is changed, set linked property value to it.
        /// Default property is field name as PascalCase.
        /// </summary>
        public PropertyFieldAttribute() { }

        /// <summary>
        /// When this field value is changed, set linked property value to it.
        /// </summary>
        /// <param name="_propertyName">Name of the property to link.</param>
        public PropertyFieldAttribute(string _propertyName)
        {
            PropertyName = _propertyName;
        }
        #endregion
    }
}
