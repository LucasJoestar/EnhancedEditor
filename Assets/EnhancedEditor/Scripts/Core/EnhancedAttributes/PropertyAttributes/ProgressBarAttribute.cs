// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Displays this field as a progress bar.
    /// </summary>
    public class ProgressBarAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public const SuperColor DefaultColor = SuperColor.Sapphire;
        public const float DefaultHeight = 25f;

        public readonly float Height = DefaultHeight;
        public readonly float MaxValue = 0f;
        public readonly string MaxValueVariableName = string.Empty;
        public readonly Color Color = default;
        public readonly GUIContent Label = new GUIContent();

        public readonly bool IsEditable = false;
        #endregion

        #region Constructors
        /// <inheritdoc cref="ProgressBarAttribute(string, string, SuperColor, float, bool)"/>
        private ProgressBarAttribute(SuperColor _color, float _height, bool _isEditable = false)
        {
            Color = _color.Get();
            Height = Mathf.Max(1f, _height);

            IsEditable = _isEditable;
        }

        /// <inheritdoc cref="ProgressBarAttribute(string, float, SuperColor, float, bool)"/>
        public ProgressBarAttribute(float _maxValue, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) :
                                    this(_color, _height, _isEditable)
        {
            MaxValue = Mathf.Max(.1f, _maxValue);
        }

        /// <inheritdoc cref="ProgressBarAttribute(string, string, SuperColor, float, bool)"/>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        public ProgressBarAttribute(string _label, float _maxValue, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) :
                                    this(_maxValue, _color, _height, _isEditable)
        {
            Label.text = _label;
        }

        /// <inheritdoc cref="ProgressBarAttribute(string, string, SuperColor, float, bool)"/>
        public ProgressBarAttribute(string _maxValueVariableName, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) :
                                    this(_color, _height, _isEditable)
        {
            MaxValueVariableName = _maxValueVariableName;
        }

        /// <inheritdoc cref="ProgressBarAttribute"/>
        /// <param name="_label">Label displayed in the middle of the progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the class variable to get value from, acting as this progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Can the user edit this progress bar value?</param>
        public ProgressBarAttribute(string _label, string _maxValueVariableName, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) :
                                    this(_maxValueVariableName, _color, _height, _isEditable)
        {
            Label.text = _label;
        }
        #endregion
    }
}
