// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays this value as a progress bar.
    /// </summary>
    public class ProgressBarAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.Sapphire;
        public const float DefaultHeight = 25f;

        /// <summary>
        /// Maximum bar value, used to determine its filled amount. Minimum value is always 0.
        /// </summary>
        public readonly float MaxValue = 0f;

        /// <summary>
        /// Name of the class member to get value from,
        /// acting as this bar maximum value and used to determine its filled amount. Minimum value is always 0.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.
        /// </summary>
        public readonly MemberValue<float>? MaxMember = null;

        /// <summary>
        /// Height of the progress bar (in pixels).
        /// </summary>
        public readonly float Height = DefaultHeight;

        /// <summary>
        /// Progress bar color.
        /// </summary>
        public readonly Color Color = default;

        /// <summary>
        /// Is this progress bar value editable (draggable) by users?
        /// </summary>
        public readonly bool IsEditable = false;
        #endregion

        #region Constructors
        /// <inheritdoc cref="ProgressBarAttribute(float, float, SuperColor, bool)"/>
        public ProgressBarAttribute(float _maxValue, SuperColor _color = DefaultColor, bool _isEditable = false) : this(_maxValue, DefaultHeight, _color, _isEditable) { }

        /// <param name="_maxValue"><inheritdoc cref="MaxValue" path="/summary"/></param>
        /// <inheritdoc cref="ProgressBarAttribute(SuperColor, float, bool)"/>
        public ProgressBarAttribute(float _maxValue, float _height, SuperColor _color = DefaultColor, bool _isEditable = false) : this(_color, _height, _isEditable)
        {
            MaxValue = Mathf.Max(.0001f, _maxValue);
        }

        /// <inheritdoc cref="ProgressBarAttribute(string, float, SuperColor, bool)"/>
        public ProgressBarAttribute(string _maxMember, SuperColor _color = DefaultColor, bool _isEditable = false) : this(_maxMember, DefaultHeight, _color, _isEditable) { }

        /// <param name="_maxMember"><inheritdoc cref="MaxMember" path="/summary"/></param>
        /// <inheritdoc cref="ProgressBarAttribute(SuperColor, float, bool)"/>
        public ProgressBarAttribute(string _maxMember, float _height, SuperColor _color = DefaultColor, bool _isEditable = false) : this(_color, _height, _isEditable)
        {
            MaxMember = _maxMember;
        }

        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <param name="_height"><inheritdoc cref="Height" path="/summary"/></param>
        /// <param name="_isEditable"><inheritdoc cref="IsEditable" path="/summary"/></param>
        /// <inheritdoc cref="ProgressBarAttribute"/>
        private ProgressBarAttribute(SuperColor _color, float _height, bool _isEditable)
        {
            Height = Mathf.Max(1f, _height);
            Color = _color.Get();
            IsEditable = _isEditable;
        }
        #endregion
    }
}
