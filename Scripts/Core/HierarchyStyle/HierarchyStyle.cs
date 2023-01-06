// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="EnhancedHierarchy"/> special override settings,
    /// for any <see cref="GameObject"/> on a specific layer.
    /// </summary>
    [Serializable]
    public class HierarchyStyle {
        #region Gloabal Members
        // --- Icon --- \\

        [Title("Icon")]

        [Tooltip("Override icon displayed next to the object")]
        public Texture Icon = null;

        [Tooltip("Color of the object displayed icon")]
        [Enhanced, DisplayName("Color"), Duo("UseIconColor", 20f)]
        
        public Color IconColor                  = Color.white;

        [Tooltip("If true, overrides the object icon color")]
        [HideInInspector, Enhanced, DisplayName("Enabled")]
        
        public bool UseIconColor                = false;

        [Title("Label"), Space(10f)]

        // --- Label --- \\

        [Tooltip("Color of the object displayed label")]
        [Enhanced, DisplayName("Color"), Duo("UseLabelColor", 20f)]
        
        public Color LabelColor                 = Color.white;

        [Tooltip("If true, overrides the object label color")]
        [HideInInspector, Enhanced, DisplayName("Enabled")]
        
        public bool UseLabelColor               = false;

        [Tooltip("Background gradient displayed behind the object label")]
        [Enhanced, DisplayName("Background"), Duo("UseBackground", 20f)]
        
        public Gradient Background              = new Gradient();

        [Tooltip("If true, overrides the object background color")]
        [HideInInspector, Enhanced, DisplayName("Enabled")]
        
        public bool UseBackground               = false;

        [Title("Header"), Space(10f)]

        // --- Header --- \\

        [Tooltip("Color of the object displayed label, if it's a header")]
        [Enhanced, DisplayName("Color"), Duo("UseHeaderColor", 20f)]

        public Color HeaderColor                = Color.white;

        [Tooltip("If true, overrides the object label color, if it's a header")]
        [HideInInspector, Enhanced, DisplayName("Enabled")]

        public bool UseHeaderColor              = false;

        [Tooltip("Background gradient displayed behind the object label, if it's a header")]
        [Enhanced, DisplayName("Background"), Duo("OverrideHeaderBackground", 20f)]
        
        public Gradient HeaderBackground        = new Gradient();

        [Tooltip("If true, overrides the object background color, if it's header")]
        [HideInInspector, Enhanced, DisplayName("Enabled")]
        
        public bool OverrideHeaderBackground    = false;
        #endregion

        #region Utility
        /// <summary>
        /// Copies the active style properties of another <see cref="HierarchyStyle"/>.
        /// </summary>
        /// <param name="_style">The style to copy.</param>
        /// <param name="_reset">If true, resets the non-active properties back to default.</param>
        public void Copy(HierarchyStyle _style, bool _reset = false) {
            // Icon.
            Texture _icon = _style.Icon;

            if (_icon != null) {
                Icon = _icon;
            } else if (_reset) {
                Icon = null;
            }

            if (_style.UseIconColor) {
                UseIconColor = true;
                IconColor = _style.IconColor;
            } else if (_reset) {
                UseIconColor = false;
            }

            // Label.
            if (_style.UseLabelColor) {
                UseLabelColor = true;
                LabelColor = _style.LabelColor;
            } else if (_reset) {
                UseLabelColor = false;
            }

            if (_style.UseBackground) {
                UseBackground = true;
                Background = _style.Background;
            } else if (_reset) {
                UseBackground = false;
            }

            // Header.
            if (_style.UseHeaderColor) {
                UseHeaderColor = true;
                HeaderColor = _style.HeaderColor;
            } else if (_reset) {
                UseHeaderColor = false;
            }

            if (_style.OverrideHeaderBackground) {
                OverrideHeaderBackground = true;
                HeaderBackground = _style.HeaderBackground;
            } else if (_reset) {
                OverrideHeaderBackground = false;
            }
        }

        /// <summary>
        /// Resets this style to default.
        /// </summary>
        public void Reset() {
            Icon = null;
            UseIconColor = false;

            UseLabelColor = false;
            UseBackground = false;

            UseHeaderColor = false;
            OverrideHeaderBackground = false;
        }
        #endregion
    }
}
