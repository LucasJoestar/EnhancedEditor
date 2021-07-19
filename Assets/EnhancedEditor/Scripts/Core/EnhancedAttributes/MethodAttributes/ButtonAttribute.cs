// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays the associated method as a button in the object inspector.
    /// </summary>
    public class ButtonAttribute : EnhancedMethodAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.White;

        public readonly string ConditionMemberName = string.Empty;
        public readonly ConditionType ConditionType = ConditionType.True;
        public readonly ActivationMode Mode = ActivationMode.Always;
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
        public ButtonAttribute(string _conditionMemberName, ConditionType _type = ConditionType.True,
                               SuperColor _color = DefaultColor) : this(_color)
        {
            ConditionMemberName = _conditionMemberName;
            ConditionType = _type;
        }

        /// <inheritdoc cref="ButtonAttribute"/>
        /// <param name="_mode">When should this button be active?</param>
        /// <param name="_conditionMemberName">Name of the class member to get a value from,
        /// used as a condition to know if the button should be enabled or not.</param>
        /// <param name="_type">Defines how the condition is considered as fulfilled.</param>
        /// <param name="_color">Button color.</param>
        public ButtonAttribute(ActivationMode _mode, string _conditionMemberName, ConditionType _type = ConditionType.True,
                               SuperColor _color = DefaultColor) : this(_conditionMemberName, _type, _color)
        {
            Mode = _mode;
        }
        #endregion
    }
}
