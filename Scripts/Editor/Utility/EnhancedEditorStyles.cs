// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple <see cref="GUIStyle"/> to be used for <see cref="GUI"/> controls.
    /// </summary>
	public static class EnhancedEditorStyles
    {
        #region Common Styles
        /// <summary>
        /// Simple button style.
        /// </summary>
        public static GUIStyle Button => GetStyle(button);
        private static GUIStyle button = null;

        /// <summary>
        /// Simple button style with no padding.
        /// </summary>
        public static GUIStyle FullSizeButton => GetStyle(fullSizeButton);
        private static GUIStyle fullSizeButton = null;

        /// <summary>
        /// Simple label style with rich text enabled.
        /// </summary>
        public static GUIStyle RichText => GetStyle(richText);
        private static GUIStyle richText = null;

        /// <summary>
        /// Bold label style with rich text enabled.
        /// </summary>
        public static GUIStyle BoldRichText => GetStyle(boldRichText);
        private static GUIStyle boldRichText = null;

        /// <summary>
        /// Simple label style with middle-center alignment.
        /// </summary>
        public static GUIStyle CenteredLabel => GetStyle(centeredLabel);
        private static GUIStyle centeredLabel = null;

        /// <summary>
        /// Bold label style with middle-center alignment.
        /// </summary>
        public static GUIStyle BoldCenteredLabel => GetStyle(boldCenteredLabel);
        private static GUIStyle boldCenteredLabel = null;

        /// <summary>
        /// Simple label style with word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle WordWrappedLabel => GetStyle(wordWrappedLabel);
        private static GUIStyle wordWrappedLabel = null;

        /// <summary>
        /// Bold label style with word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle BoldWordWrappedLabel => GetStyle(boldWordWrappedLabel);
        private static GUIStyle boldWordWrappedLabel = null;

        /// <summary>
        /// Simple label style with rich text and word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle WordWrappedRichText => GetStyle(wordWrappedRichText);
        private static GUIStyle wordWrappedRichText = null;

        /// <summary>
        /// Bold label style with rich text and word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle BoldWordWrappedRichText => GetStyle(boldWordWrappedRichText);
        private static GUIStyle boldWordWrappedRichText = null;

        /// <summary>
        /// TextArea-like style with rich text and word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle TextArea => GetStyle(textArea);
        private static GUIStyle textArea = null;

        /// <summary>
        /// Big label style in size 24 with middle-center alignment and word wrapped enabled (auto line break).
        /// </summary>
        public static GUIStyle BigWordWrappedCenteredLabel => GetStyle(bigWordWrappedCenteredLabel);
        private static GUIStyle bigWordWrappedCenteredLabel = null;

        /// <summary>
        /// Toolbar button style with middle-left alignment.
        /// </summary>
        public static GUIStyle LeftAlignedToolbarButton => GetStyle(leftAlignedToolbarButton);
        private static GUIStyle leftAlignedToolbarButton = null;

        /// <summary>
        /// Cancel button style used in toolbar search fields.
        /// </summary>
        public static GUIStyle ToolbarSearchFieldCancel => GetStyle(toolbarSearchFieldCancel);
        private static GUIStyle toolbarSearchFieldCancel = null;

        /// <summary>
        /// Drop shadow label style.
        /// </summary>
        public static GUIStyle DropShadowLabel => GetStyle(dropShadowLabel);
        private static GUIStyle dropShadowLabel = null;

        /// <summary>
        /// Drop shadow label style with middle-center alignment.
        /// </summary>
        public static GUIStyle DropShadowCenteredLabel => GetStyle(dropShadowCenteredLabel);
        private static GUIStyle dropShadowCenteredLabel = null;

        /// <summary>
        /// Link label style.
        /// </summary>
        public static GUIStyle LinkLabel => GetStyle(linkLabel);
        private static GUIStyle linkLabel = null;

        /// <summary>
        /// Popup style with non fixed-height and middle-center alignement.
        /// </summary>
        public static GUIStyle CenteredPopup => GetStyle(centeredPopup);
        private static GUIStyle centeredPopup = null;

        /// <summary>
        /// Option panel related transparent button style.
        /// </summary>
        public static GUIStyle PaneOptions => GetStyle(paneOptions);
        private static GUIStyle paneOptions = null;
        #endregion

        #region Multi Tags Styles
        /// <summary>
        /// Style used to draw a <see cref="Tag"/>.
        /// </summary>
        public static GUIStyle CNCountBadge => GetStyle(cnCountBadge);
        private static GUIStyle cnCountBadge = null;

        /// <summary>
        /// Round minus button style.
        /// </summary>
        public static GUIStyle OlMinus => GetStyle(olMinus);
        private static GUIStyle olMinus = null;

        /// <summary>
        /// Round plus button style.
        /// </summary>
        public static GUIStyle OlPlus => GetStyle(olPlus);
        private static GUIStyle olPlus = null;
        #endregion

        #region Toolbar Styles
        /// <summary>
        /// Main editor toolbar control style (cannot display any label).
        /// </summary>
        public static GUIStyle ToolbarControl => GetStyle(toolbarControl);
        private static GUIStyle toolbarControl = null;

        /// <summary>
        /// Toolbar left button style (should be used together with <see cref="ToolbarButtonRight"/>).
        /// </summary>
        public static GUIStyle ToolbarButtonLeft => GetStyle(toolbarButtonLeft);
        private static GUIStyle toolbarButtonLeft = null;

        /// <summary>
        /// Toolbar right button style (should be used together with <see cref="ToolbarButtonLeft"/>).
        /// </summary>
        public static GUIStyle ToolbarButtonRight => GetStyle(toolbarButtonRight);
        private static GUIStyle toolbarButtonRight = null;

        /// <summary>
        /// Toolbar left command style (can be used together with <see cref="ToolbarCommandMid"/> and <see cref="ToolbarCommandRight"/>).
        /// </summary>
        public static GUIStyle ToolbarCommandLeft => GetStyle(toolbarCommandLeft);
        private static GUIStyle toolbarCommandLeft = null;

        /// <summary>
        /// Toolbar middle command style (can be used together with <see cref="ToolbarButtonLeft"/> and <see cref="ToolbarCommandRight"/>).
        /// </summary>
        public static GUIStyle ToolbarCommandMid => GetStyle(toolbarCommandMid);
        private static GUIStyle toolbarCommandMid = null;

        /// <summary>
        /// Toolbar right command style (can be used together with <see cref="ToolbarButtonLeft"/> and <see cref="ToolbarCommandMid"/>).
        /// </summary>
        public static GUIStyle ToolbarCommandRight => GetStyle(toolbarCommandRight);
        private static GUIStyle toolbarCommandRight = null;

        /// <summary>
        /// Toolbar dropdown style.
        /// </summary>
        public static GUIStyle ToolbarDropdown => GetStyle(toolbarDropdown);
        private static GUIStyle toolbarDropdown = null;

        /// <summary>
        /// Toolbar-like label style.
        /// </summary>
        public static GUIStyle ToolbarLabel => GetStyle(toolbarLabel);
        private static GUIStyle toolbarLabel = null;
        #endregion

        #region Getter & Initialization
        private static bool isInitialized = false;

        // -----------------------

        private static GUIStyle GetStyle(GUIStyle _style)
        {
            if (!isInitialized)
            {
                #region Common
                // Button.
                button = new GUIStyle("Button");

                // Full size button.
                fullSizeButton = new GUIStyle("Button")
                {
                    padding = new RectOffset(),
                };

                // Rich text.
                richText = new GUIStyle(EditorStyles.label)
                {
                    richText = true
                };

                // Bold rich text.
                boldRichText = new GUIStyle(EditorStyles.boldLabel)
                {
                    richText = true
                };

                // Centered label.
                centeredLabel = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                // Bold centered label.
                boldCenteredLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                // Word wrapped label.
                wordWrappedLabel = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true
                };

                // Bold word wrapped label.
                boldWordWrappedLabel = new GUIStyle(EditorStyles.boldLabel)
                {
                    wordWrap = true
                };

                // Word wrapped rich text.
                wordWrappedRichText = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = true
                };

                // Bold word wrapped rich text.
                boldWordWrappedRichText = new GUIStyle(EditorStyles.boldLabel)
                {
                    richText = true,
                    wordWrap = true
                };

                // Text area.
                textArea = new GUIStyle(EditorStyles.textArea)
                {
                    richText = true,
                    wordWrap = true
                };

                // Big word wrapped centered label.
                bigWordWrappedCenteredLabel = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24,
                    wordWrap = true
                };

                // Left aligned toolbar button.
                leftAlignedToolbarButton = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleLeft
                };

                // Toolbar search field cancel.
                toolbarSearchFieldCancel = new GUIStyle("ToolbarSeachCancelButton");

                // Drop shadow label.
                dropShadowLabel = new GUIStyle("PreOverlayLabel");

                // Drop shaow centered label.
                dropShadowCenteredLabel = new GUIStyle(dropShadowLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                // Link label.
                linkLabel = new GUIStyle(EditorStyles.whiteLabel)
                {
                    fontStyle = FontStyle.Bold
                };

                // Centered Popup.
                centeredPopup = new GUIStyle(EditorStyles.popup)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fixedHeight = 0,
                    stretchWidth = false
                };

                // Pane options.
                paneOptions = new GUIStyle("PaneOptions");
                #endregion

                #region Multi Tags
                // Multi-Tags related styles.
                cnCountBadge = new GUIStyle("CN CountBadge")
                {
                    fontStyle = FontStyle.Bold,
                };

                olMinus = new GUIStyle("OL Minus");
                olPlus = new GUIStyle("OL Plus");
                #endregion

                #region Toolbar
                // Global toolbar control.
                toolbarControl = new GUIStyle("AppCommand")
                {
                    fixedWidth = 0f
                };

                // Toolbar duet buttons.
                toolbarButtonLeft = new GUIStyle("AppToolbarButtonLeft");
                toolbarButtonRight = new GUIStyle("AppToolbarButtonRight");

                // Toolbar packed commands.
                toolbarCommandLeft = new GUIStyle("AppCommandLeft")
                {
                    fixedWidth = 0f
                };

                toolbarCommandMid = new GUIStyle("AppCommandMid")
                {
                    fixedWidth = 0f
                };

                toolbarCommandRight = new GUIStyle("AppCommandRight")
                {
                    fixedWidth = 0f
                };

                // Dropdown.
                toolbarDropdown = new GUIStyle("Dropdown");

                // Toolbar label is defined by the characteristic colors of toolbar buttons.
                toolbarLabel = new GUIStyle(EditorStyles.label);

                toolbarLabel.active.textColor =     toolbarButtonLeft.active.textColor;
                toolbarLabel.onActive.textColor =   toolbarButtonLeft.onActive.textColor;
                toolbarLabel.focused.textColor =    toolbarButtonLeft.focused.textColor;
                toolbarLabel.onFocused.textColor =  toolbarButtonLeft.onFocused.textColor;
                toolbarLabel.hover.textColor =      toolbarButtonLeft.hover.textColor;
                toolbarLabel.onHover.textColor =    toolbarButtonLeft.onHover.textColor;
                toolbarLabel.normal.textColor =     toolbarButtonLeft.normal.textColor;
                toolbarLabel.onNormal.textColor =   toolbarButtonLeft.onNormal.textColor;

                toolbarLabel.fixedHeight = toolbarControl.fixedHeight;
                #endregion

                // Return a simple label style on initialization as other styles will only be ready on the next repaint.
                isInitialized = true;
                return EditorStyles.label;
            }

            return _style;
        }
        #endregion
    }
}
