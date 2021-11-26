// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays this method as a button in the inspector.
    /// </summary>
    public class ButtonAttribute : EnhancedMethodAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.White;

        /// <summary>
        /// When should this button be active and enabled?
        /// </summary>
        public readonly ActivationMode Mode = ActivationMode.Always;

        /// <summary>
        /// Defines how this button condition value is considered as fulfilled.
        /// </summary>
        public readonly ConditionType ConditionType = ConditionType.True;

        /// <summary>
        /// Name of the class member to get value from,
        /// used as a condition to know if the button should be active and enabled.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="bool"/>.
        /// </summary>
        public readonly MemberValue<bool> ConditionMember = default;

        /// <summary>
        /// Button color.
        /// </summary>
        public readonly Color Color = default;
        #endregion

        #region Constructors
        /// <inheritdoc cref="ButtonAttribute(ActivationMode, string, ConditionType, SuperColor)"/>
        public ButtonAttribute(SuperColor _color = DefaultColor)
        {
            Color = _color.Get();
        }

        /// <inheritdoc cref="ButtonAttribute(ActivationMode, string, ConditionType, SuperColor)"/>
        public ButtonAttribute(ActivationMode _mode, SuperColor _color = DefaultColor) : this(_color)
        {
            Mode = _mode;
        }

        /// <inheritdoc cref="ButtonAttribute(ActivationMode, string, ConditionType, SuperColor)"/>
        public ButtonAttribute(string _conditionMember, ConditionType _type = ConditionType.True,
                               SuperColor _color = DefaultColor) : this(_color)
        {
            ConditionMember = _conditionMember;
            ConditionType = _type;
        }

        /// <param name="_mode"><inheritdoc cref="Mode" path="/summary"/></param>
        /// <param name="_conditionMember"><inheritdoc cref="ConditionMember" path="/summary"/></param>
        /// <param name="_type"><inheritdoc cref="ConditionType" path="/summary"/></param>
        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <inheritdoc cref="ButtonAttribute"/>
        public ButtonAttribute(ActivationMode _mode, string _conditionMember, ConditionType _type = ConditionType.True,
                               SuperColor _color = DefaultColor) : this(_conditionMember, _type, _color)
        {
            Mode = _mode;
        }
        #endregion
    }
}
