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
    /// Custom <see cref="Document"/> editor, used to switch between edit and display mode.
    /// </summary>
    [CustomEditor(typeof(Document), true)]
    public class DocumentEditor : UnityObjectEditor
    {
        #region Global Members
        private const float IconSize = 128f;
        private const int TitleSize = 26;
        private const int HeaderSize = 18;
        private const int LabelSize = 14;

        private static readonly GUIContent editGUI = new GUIContent("Edit", "Edit this Document.");
        private static readonly GUIContent displayGUI = new GUIContent("Display", "Display this Document.");

        private static bool isEditing = false;

        private GUIStyle BigTitleStyle = null;
        private GUIStyle TitleStyle = null;
        private GUIStyle HeaderStyle = null;
        private GUIStyle LabelStyle = null;
        private bool areStylesInitialized = false;

        private Document document = null;
        private Vector2 scroll = new Vector2();
        #endregion

        #region Document Drawer
        protected override void OnEnable()
        {
            base.OnEnable();
            document = target as Document;
        }

        protected override void OnHeaderGUI()
        {
            // Styles initialization.
            if (!areStylesInitialized)
                InitializeStyles();

            // Header.
            float _size = Mathf.Min(IconSize, (EditorGUIUtility.currentViewWidth * .5f) - 50f);
            using (var _scope = new EditorGUILayout.HorizontalScope(BigTitleStyle))
            {
                GUILayout.Label(document.Icon, EnhancedEditorStyles.CenteredLabel, GUILayout.Width(_size), GUILayout.Height(_size));

                using (var _verticalScope = new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Label(document.Title, TitleStyle);
                    GUILayout.FlexibleSpace();

                    // Edit / Display button.
                    using (var _colorScope = EnhancedGUI.GUIColor.Scope(SuperColor.Green.Get()))
                    {
                        Rect _position = EditorGUILayout.GetControlRect(true, 25f);
                        _position.xMin = _position.xMax - 75f;

                        GUIContent _gui = isEditing
                                        ? displayGUI
                                        : editGUI;

                        if (GUI.Button(_position, _gui))
                        {
                            isEditing = !isEditing;
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                if (isEditing)
                {
                    base.OnInspectorGUI();
                }
                else
                {
                    DrawDocument();
                }
            }
        }

        // -----------------------

        private void DrawDocument()
        {
            foreach (var _section in document.Sections)
            {
                GUILayout.Space(_section.Spacing);
                GUIContent _label;

                // Header.
                if (!string.IsNullOrEmpty(_section.Header))
                {
                    _label = EnhancedEditorGUIUtility.GetLabelGUI(_section.Header);
                    EnhancedEditorGUILayout.UnderlinedLabel(_label, HeaderStyle);

                    GUILayout.Space(5f);
                }

                // Text.
                if (!string.IsNullOrEmpty(_section.Text))
                {
                    _label = EnhancedEditorGUIUtility.GetLabelGUI(_section.Text);
                    EditorGUILayout.LabelField(_label, LabelStyle);
                }

                // Content.
                if (!string.IsNullOrEmpty(_section.InfoText))
                {
                    GUILayout.Space(5f);
                    using (var _scope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        float _width = EditorGUIUtility.currentViewWidth - 50f;
                        using (var _verticalScope = new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Width(_width)))
                        {
                            _label = EnhancedEditorGUIUtility.GetLabelGUI(_section.InfoText);
                            EditorGUILayout.LabelField(_label, LabelStyle);
                        }
                    }
                }

                // Image.
                if (_section.Image)
                {
                    EnhancedEditorGUILayout.Texture(_section.Image);
                }

                // Link.
                if (!string.IsNullOrEmpty(_section.LinkText))
                {
                    using (var _scope = EnhancedGUI.GUIStyleFontSize.Scope(EnhancedEditorStyles.LinkLabel, LabelSize))
                    {
                        _label = EnhancedEditorGUIUtility.GetLabelGUI(_section.LinkText);
                        EnhancedEditorGUILayout.LinkLabel(_label, _section.URL);
                    }
                }

                GUILayout.Space(5f);
            }
        }
        #endregion

        #region Utility
        private void InitializeStyles()
        {
            BigTitleStyle = new GUIStyle("In BigTitle");
            TitleStyle = new GUIStyle(EnhancedEditorStyles.BoldWordWrappedLabel)
            {
                fontSize = TitleSize
            };

            HeaderStyle = new GUIStyle(EnhancedEditorStyles.BoldWordWrappedRichText)
            {
                fontSize = HeaderSize
            };

            LabelStyle = new GUIStyle(EnhancedEditorStyles.WordWrappedRichText)
            {
                fontSize = LabelSize
            };

            areStylesInitialized = true;
        }
        #endregion
    }
}
