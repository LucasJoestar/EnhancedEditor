// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Draws a help box above or below this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class HelpBoxAttribute : EnhancedPropertyAttribute {
        #region Global Members
        /// <summary>
        /// Message displayed in the help box.
        /// </summary>
        public readonly string Message = null;

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this help box message content.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="string"/>.
        /// </summary>
        public readonly MemberValue<string>? MessageMember = null;

        /// <summary>
        /// Help box message type.
        /// </summary>
        public readonly MessageType MessageType = MessageType.Info;

        /// <summary>
        /// Should the help box be drawn above or below this field?
        /// </summary>
        public readonly bool IsAbove = true;

        /// <summary>
        /// Spacing between this help box and its field.
        /// </summary>
        public readonly float Spacing = 0f;

        /// <summary>
        /// Name of the class member to get value from,
        /// used as a condition to know if this help box should be visible or not.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="bool"/>.
        /// </summary>
        public readonly MemberValue<bool>? ConditionMember = null;

        /// <summary>
        /// Defines how this help box condition value is considered as fulfilled.
        /// </summary>
        public readonly ConditionType ConditionType = ConditionType.True;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_message"><inheritdoc cref="Message" path="/summary"/></param>
        /// <param name="_messageType"><inheritdoc cref="MessageType" path="/summary"/></param>
        /// <param name="_isAbove"><inheritdoc cref="IsAbove" path="/summary"/></param>
        /// <param name="_spacing"><inheritdoc cref="Spacing" path="/summary"/></param>
        /// <inheritdoc cref="HelpBoxAttribute"/>
        public HelpBoxAttribute(string _message, MessageType _messageType, bool _isAbove = true, float _spacing = 0f) {
            Message     = _message;
            MessageType = _messageType;
            IsAbove     = _isAbove;
            Spacing     = _spacing;
        }

        /// <param name="_conditionMember"><inheritdoc cref="ConditionMember" path="/summary"/></param>
        /// <param name="_conditionType"><inheritdoc cref="ConditionType" path="/summary"/></param>
        /// <inheritdoc cref="HelpBoxAttribute(string, MessageType, bool, float)"/>
        public HelpBoxAttribute(string _message, MessageType _messageType, string _conditionMember, ConditionType _conditionType = ConditionType.True, bool _isAbove = true, float _spacing = 0f) :
                                this(_message, _messageType, _isAbove, _spacing) {
            ConditionMember = _conditionMember;
            ConditionType   = _conditionType;
        }

        /// <param name="_">Useless parameter, only here to differenciate this constructor from the one with an explicit name.</param>
        /// <inheritdoc cref="HelpBoxAttribute(bool, string, MessageType, string, ConditionType, bool, float)"/>
        public HelpBoxAttribute(bool _, string _messageMember, MessageType _messageType, bool _isAbove = true, float _spacing = 0f) : this(string.Empty, _messageType, _isAbove, _spacing) {
            MessageMember = _messageMember;
        }

        /// <param name="_messageMember"><inheritdoc cref="MessageMember" path="/summary"/></param>
        /// <param name="_">Useless parameter, only here to differenciate this constructor from the one with an explicit message.</param>
        /// <inheritdoc cref="HelpBoxAttribute(string, MessageType, string, ConditionType, bool, float)"/>
        public HelpBoxAttribute(bool _, string _messageMember, MessageType _messageType, string _conditionMember,
                                ConditionType _conditionType = ConditionType.True, bool _isAbove = true, float _spacing = 0f) :
                                this(string.Empty, _messageType, _conditionMember, _conditionType, _isAbove, _spacing) {
            MessageMember = _messageMember;
        }
        #endregion
    }
}
