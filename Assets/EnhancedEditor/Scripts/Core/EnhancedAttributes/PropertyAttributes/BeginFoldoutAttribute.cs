// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //


using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Begins a foldout group encapsulating each of the following fields
    /// until the next <see cref="EndFoldoutAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BeginFoldoutAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Label displayed in front of the foldout.
        /// </summary>
        public readonly GUIContent Label = null;

        /// <summary>
        /// Color of this foldout group background.
        /// </summary>
        public readonly Color Color = default;

        /// <summary>
        /// Is a specific color assigned to this group?
        /// </summary>
        public readonly bool HasColor = false;

        internal bool foldout = false;

        // -----------------------

        /// <inheritdoc cref="BeginFoldoutAttribute(string, SuperColor)"/>
        public BeginFoldoutAttribute(string _label)
        {
            Label = new GUIContent(_label);
        }

        /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <inheritdoc cref="BeginFoldoutAttribute"/>
        public BeginFoldoutAttribute(string _label, SuperColor _color) : this(_label)
        {
            HasColor = true;
            Color = _color.Get();
        }
        #endregion
    }
}
