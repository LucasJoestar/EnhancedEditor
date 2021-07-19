// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Draws a help box above or below this field.
    /// </summary>
    public class HelpBoxAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly bool IsAbove = true;
        public readonly MessageType Type = MessageType.Info;
        public readonly GUIContent Label = null;

        // -----------------------

        /// <inheritdoc cref="HelpBoxAttribute"/>
        /// <param name="_label">Label displayed in the help box.</param>
        /// <param name="_type">Help box type.</param>
        /// <param name="_isAbove">Should the help box be drawn above or below the associated field?</param>
        public HelpBoxAttribute(string _label, MessageType _type, bool _isAbove = true)
        {
            Label = new GUIContent(_label);
            Type = _type;
            IsAbove = _isAbove;
        }
        #endregion
    }
}
