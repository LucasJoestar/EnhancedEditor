// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Specifies a new label to be used for this field in the inspector.
    /// </summary>
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
