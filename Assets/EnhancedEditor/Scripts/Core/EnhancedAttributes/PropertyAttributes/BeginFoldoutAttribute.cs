// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Begins a foldout group encapsulating each following field
    /// until the next <see cref="EndFoldoutAttribute"/>.
    /// </summary>
	public class BeginFoldoutAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly bool HasColor = false;
        public readonly Color Color = default;
        public readonly GUIContent Label = null;

        public bool Foldout = false;

        // -----------------------

        /// <inheritdoc cref="BeginFoldoutAttribute(string, SuperColor)"/>
        public BeginFoldoutAttribute(string _label)
        {
            Label = new GUIContent(_label);
        }

        /// <inheritdoc cref="BeginFoldoutAttribute"/>
        /// <param name="_label">Label displayed next to the foldout.</param>
        /// <param name="_color">Color of the box encapsulating the group.</param>
        public BeginFoldoutAttribute(string _label, SuperColor _color) : this(_label)
        {
            HasColor = true;
            Color = _color.Get();
        }
        #endregion
    }
}
