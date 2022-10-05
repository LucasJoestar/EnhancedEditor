// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Specifies a new label to be used for this field in the inspector, or for a class using <see cref="SerializedType{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Label to be displayed in front of this field in the inspector.
        /// </summary>
        public readonly GUIContent Label = null;

        // -----------------------

        /// <inheritdoc cref="DisplayNameAttribute(string, string)"/>
        public DisplayNameAttribute(string _name)
        {
            Label = new GUIContent(_name);
        }

        /// <param name="_name">Name used for this field in the inspector.</param>
        /// <param name="_tooltip">Tooltip to be hovering over this field in the inspector.</param>
        /// <inheritdoc cref="DisplayNameAttribute"/>
        public DisplayNameAttribute(string _name, string _tooltip)
        {
            Label = new GUIContent(_name, _tooltip);
        }
        #endregion
    }
}
