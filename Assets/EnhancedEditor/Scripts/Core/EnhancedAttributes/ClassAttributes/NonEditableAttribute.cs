// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Replaces the default inspector of all of this class instances
    /// by a simple customizable label.
    /// </summary>
	public class NonEditableAttribute : EnhancedClassAttribute
    {
        #region Global Members
        public const string DefaultLabel = "Edition of this object is not allowed.\nThese data may be sensitive.";

        /// <summary>
        /// Label displayed in the inspector, instead of its original content.
        /// </summary>
        public readonly GUIContent Label = null;

        // -----------------------

        /// <inheritdoc cref="NonEditableAttribute(string)"/>
        public NonEditableAttribute() : this(DefaultLabel) { }

        /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
        /// <inheritdoc cref="NonEditableAttribute"/>
        public NonEditableAttribute(string _label)
        {
            Label = new GUIContent(_label);
        }
        #endregion
    }
}
