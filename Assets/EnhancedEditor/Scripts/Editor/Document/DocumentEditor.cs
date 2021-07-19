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
    /// Custom <see cref="Document"/> editor, used to nicely display its content.
    /// </summary>
    [CustomEditor(typeof(Document), true)]
    public class DocumentEditor : UnityEditor.Editor
    {
        #region Global Members
        private const float IconSize = 128f;
        private const int TitleSize = 26;
        private const int HeaderSize = 18;

        private static readonly GUIContent editGUI = new GUIContent("Edit", "Edit this Document.");
        private static readonly GUIContent displayGUI = new GUIContent("Display", "Display this Document.");

        private static bool isEditing = false;

        private GUIStyle BigTitleStyle = null;
        private GUIStyle TitleStyle = null;
        private GUIStyle HeaderStyle = null;
        private bool areStylesInitialized = false;

        private Document document = null;
        private Vector2 scroll = new Vector2();
        #endregion

        #region Document Drawer
        private void OnEnable()
        {
            document = target as Document;
        }

        protected override void OnHeaderGUI()
        {
            if (!areStylesInitialized)
                InitializeStyles();

            float _size = Mathf.Min(IconSize, (EditorGUIUtility.currentViewWidth * .5f) - 50f);

            GUILayout.BeginHorizontal(BigTitleStyle);
            {
                GUILayout.Label(document.Icon, GUILayout.Width(_size), GUILayout.Height(_size));
                GUILayout.Label(document.Title, TitleStyle);
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(Screen.height - 240f));
            scroll = EditorGUILayout.BeginScrollView(scroll);

            GUIContent _buttonGUI;
            if (isEditing)
            {
                _buttonGUI = displayGUI;
                base.OnInspectorGUI();
            }
            else
            {
                _buttonGUI = editGUI;
                DrawDocument();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Edit button.
            EditorGUILayout.Space(10f);

            Rect _buttonPosition = EditorGUILayout.GetControlRect(true, 25f);
            _buttonPosition.xMin = _buttonPosition.xMax - 75f;

            EnhancedEditorGUIUtility.PushGUIColor(SuperColor.Green.Get());

            if (GUI.Button(_buttonPosition, _buttonGUI))
            {
                isEditing = !isEditing;
            }

            EnhancedEditorGUIUtility.PopGUIColor();
        }

        // -----------------------

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

            areStylesInitialized = true;
        }

        private void DrawDocument()
        {
            for (int _i = 0; _i < document.Sections.Length; _i++)
            {
                Document.Section _section = document.Sections[_i];
                EditorGUILayout.Space(_section.Space);

                // Header.
                if (!string.IsNullOrEmpty(_section.Header))
                {
                    EnhancedEditorGUILayout.UnderlinedLabel(new GUIContent(_section.Header), HeaderStyle);
                    EditorGUILayout.Space(5f);
                }

                // Text.
                if (!string.IsNullOrEmpty(_section.Text))
                {
                    EditorGUILayout.LabelField(_section.Text, EnhancedEditorStyles.WordWrappedRichText);
                }

                // Content.
                if (!string.IsNullOrEmpty(_section.ContextText))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    float _width = EditorGUIUtility.currentViewWidth - 50f;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(_width));

                    EditorGUILayout.LabelField(_section.ContextText, EnhancedEditorStyles.WordWrappedRichText);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }

                // Image.
                if (_section.Image)
                {
                    Rect _position = EditorGUILayout.GetControlRect(false, 0f);
                    _position.xMin = 5f;

                    float _height = EnhancedEditorGUI.Texture(_position, _section.Image);
                    EditorGUILayout.GetControlRect(false, _height);
                }

                // Link.
                if (!string.IsNullOrEmpty(_section.LinkText))
                {
                    EnhancedEditorGUILayout.LinkLabel(new GUIContent(_section.LinkText), _section.URL);
                }
            }

            // Repaint to correctly display link labels.
            Repaint();
        }
        #endregion
    }
}
