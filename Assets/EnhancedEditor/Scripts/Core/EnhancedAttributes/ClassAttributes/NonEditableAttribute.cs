// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Avoid default inspector of target objects from being drawn.
    /// <para/>
    /// A custom label will be displayed instead.
    /// </summary>
	public class NonEditableAttribute : EnhancedClassAttribute
    {
        #region Global Members
        public const string DefaultLabel = "Edition of this object is not allowed.\nThese datas may be sensitives.";

        public readonly GUIContent Label = null;

        // -----------------------

        /// <inheritdoc cref="NonEditableAttribute"/>
        /// <param name="_label">Label displayed instead of the usual inspector.</param>
        public NonEditableAttribute(string _label)
        {
            Label = new GUIContent(_label);
        }

        /// <inheritdoc cref="NonEditableAttribute(string)"/>
        public NonEditableAttribute() : this(DefaultLabel) { }
        #endregion
    }
}
