using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        #region Fields
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Type of help box do display.
        /// </summary>
        public readonly HelpBoxType     Type =      HelpBoxType.Info;

        /// <summary>
        /// Label displayed in the box.
        /// </summary>
        public readonly string          Label =     string.Empty;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Display a help box with a message in the editor.
        /// </summary>
        /// <param name="_label">Label to display in the box.</param>
        /// <param name="_type">Type of box to display.</param>
        public HelpBoxAttribute(string _label, HelpBoxType _type)
        {
            Type = _type;
            Label = _label;
        }
        #endregion
    }
}
