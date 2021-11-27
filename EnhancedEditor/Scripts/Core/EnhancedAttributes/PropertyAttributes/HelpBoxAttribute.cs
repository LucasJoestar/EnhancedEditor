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
        /// <summary>
        /// Label displayed in the help box.
        /// </summary>
        public readonly GUIContent Label = null;

        /// <summary>
        /// Help box message type.
        /// </summary>
        public readonly MessageType MessageType = MessageType.Info;

        /// <summary>
        /// Should the help box be drawn above or below this field?
        /// </summary>
        public readonly bool IsAbove = true;

        // -----------------------

        /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
        /// <param name="_messageType"><inheritdoc cref="MessageType" path="/summary"/></param>
        /// <param name="_isAbove"><inheritdoc cref="IsAbove" path="/summary"/></param>
        /// <inheritdoc cref="HelpBoxAttribute"/>
        public HelpBoxAttribute(string _label, MessageType _messageType, bool _isAbove = true)
        {
            Label = new GUIContent(_label);
            MessageType = _messageType;
            IsAbove = _isAbove;
        }
        #endregion
    }
}
