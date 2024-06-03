// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Begins a foldout group encapsulating each of the following fields
    /// until the next <see cref="EndFoldoutAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class BeginFoldoutAttribute : EnhancedPropertyAttribute {
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

        /// <summary>
        /// Optional <see cref="GUIStyle"/> used to draw this foldout header.
        /// </summary>
        public readonly GUIStyle HeaderStyle = GUIStyle.none;

        /// <summary>
        /// Optional <see cref="GUIStyle"/> used to draw this foldout background.
        /// </summary>
        public readonly GUIStyle BackgroundStyle = GUIStyle.none;

        /// <summary>
        /// Displayed icon next to this foldout.
        /// </summary>
        public readonly string Icon = string.Empty;

        /// <summary>
        /// If true, draw this foldout header at full width margins.
        /// </summary>
        public readonly bool FullWidth = false;

        /// <summary>
        /// If true, encapsulate this foldout within separated lines.
        /// </summary>
        public readonly bool Encapsulate = false;

        internal bool foldout = false;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="BeginFoldoutAttribute(string, SuperColor, bool, bool, string)"/>
        public BeginFoldoutAttribute(string _label, bool _fullWidth = false, bool _encapsulate = false, string _icon = "") {
            Label       = new GUIContent(_label);
            FullWidth   = _fullWidth;
            Encapsulate = _encapsulate;
            Icon        = _icon;
        }

        /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
        /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
        /// <param name="_fullWidth"><inheritdoc cref="FullWidth" path="/summary"/></param>
        /// <param name="_encapsulate"><inheritdoc cref="Encapsulate" path="/summary"/></param>
        /// <param name="_icon"><inheritdoc cref="Icon" path="/summary"/></param>
        /// <inheritdoc cref="BeginFoldoutAttribute"/>
        public BeginFoldoutAttribute(string _label, SuperColor _color, bool _fullWidth = false, bool _encapsulate = false, string _icon = "") : this(_label, _fullWidth, _encapsulate, _icon) {
            HasColor = true;
            Color    = _color.Get();
        }

        /// <param name="_headerStyle"><inheritdoc cref="HeaderStyle" path="/summary"/></param>
        /// <param name="_backgroundStyle"><inheritdoc cref="BackgroundStyle" path="/summary"/></param>
        /// <inheritdoc cref="BeginFoldoutAttribute(string, SuperColor, bool, bool, string)"/>
        public BeginFoldoutAttribute(string _label, string _headerStyle, string _backgroundStyle = null, bool _fullWidth = false, bool _encapsulate = false, string _icon = "") : this(_label, _fullWidth, _encapsulate, _icon) {
            HeaderStyle     = string.IsNullOrEmpty(_headerStyle) ? GUIStyle.none : _headerStyle;
            BackgroundStyle = string.IsNullOrEmpty(_backgroundStyle) ? GUIStyle.none : _backgroundStyle;
        }
        #endregion
    }
}
