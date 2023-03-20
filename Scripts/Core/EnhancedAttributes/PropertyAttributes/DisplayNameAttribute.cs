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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Label to be displayed in front of this field in the inspector.
        /// </summary>
        public readonly GUIContent Label = null;

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this field displayed name.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="string"/>.
        /// </summary>
        public readonly MemberValue<string>? NameMember = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

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

        /// <inheritdoc cref="DisplayNameAttribute(string, string, bool)"/>
        public DisplayNameAttribute(string _memberName, bool _) : this(string.Empty) {
            NameMember = _memberName;
        }

        /// <param name="_memberName"><inheritdoc cref="NameMember" path="/summary"/></param>
        /// <param name="_">Useless parameter, only here to differenciate this constructor from the one with an explicit name.</param>
        /// <inheritdoc cref="DisplayNameAttribute(string, string)"/>
        public DisplayNameAttribute(string _memberName, string _tooltip, bool _) : this(string.Empty, _tooltip) {
            NameMember = _memberName;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets the text name of this label.
        /// </summary>
        /// <param name="_name">This label new text name.</param>
        internal void SetName(string _name) {
            Label.text = _name;
        }
        #endregion
    }
}
