// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multi-Tags system editor window, used to manage all tags in the project.
    /// </summary>
    [InitializeOnLoad]
    public class MultiTagsWindow : EditorWindow
    {
        #region Editor Database
        /// <summary>
        /// Auto-managed <see cref="ScriptableObject"/> resource for this project Multi-Tags database.
        /// </summary>
        private static readonly AutoManagedResource<TagDatabase> resource = new AutoManagedResource<TagDatabase>(false);

        /// <summary>
        /// Database containing all tags in the project.
        /// </summary>
        public static TagDatabase Database => resource.GetResource();

        // -----------------------

        static MultiTagsWindow()
        {
            MultiTags.EditorTagDatabaseGetter = () => Database;
            TagDatabase.OnOpenMultiTagsWindow = () => GetWindow();
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="MultiTagsWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="MultiTagsWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Multi-Tags", false, 130)]
        public static MultiTagsWindow GetWindow()
        {
            MultiTagsWindow _window = GetWindow<MultiTagsWindow>("Multi-Tags");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string UndoRecordTitle = "Multi-Tags Window Change";
        private const string TooltipMessage = "You can edit any tag name and color using the context menu.";
        private const string NoTagMessage = "No tag could be found in the project! Create a new one using the plus button on the toolbar.";

        private readonly GUIContent createTagGUI = new GUIContent(" Create Tag", "Creates a new tag in the project.");
        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh all project tags.");
        private readonly GUIContent headerGUI = new GUIContent("Project Tags:", "All created tags in the project.");

        [SerializeField] private string searchFilter = string.Empty;
        [SerializeField] private bool[] tagsVisibility = new bool[] { };

        private TagData[] tags = new TagData[] { };
        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            // Repaint on undo / redo.
            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;

            titleContent.image = EditorGUIUtility.FindTexture("FilterByLabel");
            createTagGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");

            RefreshTags();
        }

        private void OnGUI()
        {
            Undo.RecordObject(this, UndoRecordTitle);

            // Toolbar.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Create tag.
                if (GUILayout.Button(createTagGUI, EditorStyles.toolbarButton, GUILayout.Width(95f)))
                {
                    CreateTagWindow.GetWindow();
                }

                // Search filter.
                string _searchPattern = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter);
                if (_searchPattern != searchFilter)
                {
                    searchFilter = _searchPattern;
                    FilterTags();
                }

                // Refresh.
                if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton, GUILayout.Width(55f)) || (Database.Count != tags.Length))
                {
                    RefreshTags();
                }
            }

            // Project tags.
            using (var _scrollScrop = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scrollScrop.scrollPosition;

                GUILayout.Space(3f);
                using (var _scope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(5f);

                    using (var verticalScope = new GUILayout.VerticalScope(GUILayout.Width(position.width - 10f)))
                    {
                        // Header.
                        EnhancedEditorGUILayout.UnderlinedLabel(headerGUI, EditorStyles.boldLabel);
                        GUILayout.Space(7f);

                        if (tags.Length == 0)
                        {
                            // No tag.
                            EditorGUILayout.HelpBox(NoTagMessage, UnityEditor.MessageType.Warning, true);
                        }
                        else
                        {
                            // Displayed tags.
                            Rect _position = EditorGUILayout.GetControlRect();
                            _position.width = position.width - 10f;

                            Rect _temp = new Rect(_position);
                            for (int _i = 0; _i < tags.Length; _i++)
                            {
                                if (!tagsVisibility[_i])
                                    continue;

                                TagData _tag = tags[_i];

                                // Draw this tag and remove it from the project on associated button click.
                                if (EnhancedEditorGUI.DrawTagGroupElement(_position, ref _temp, _tag)
                                 && EditorUtility.DisplayDialog("Delete this Tag?",
                                                                "You are about to completely erase this tag from your project.\n\n" +
                                                                "Are you sure you want to do this? All uses of this tag will become obsolete and will be ignored.", "Yes", "Cancel"))
                                {
                                    MultiTags.DeleteTag(_tag);
                                    InternalEditorUtility.RepaintAllViews();
                                }

                                _temp.x += _temp.width + 5f;
                            }

                            // Layout update.
                            float _extraHeight = _temp.yMax - _position.yMax;
                            EnhancedEditorGUILayout.ManageDynamicGUIControlHeight(GUIContent.none, _extraHeight);

                            if (_temp.position != _position.position)
                            {
                                GUILayout.Space(5f);
                                EditorGUILayout.HelpBox(TooltipMessage, UnityEditor.MessageType.Info, true);
                            }
                        }
                    }

                    GUILayout.Space(5f);
                }
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
        }
        #endregion

        #region Utility
        private void RefreshTags()
        {
            TagDatabase _settings = resource.Reload()[0];
            tags = _settings.Tags;

            tagsVisibility = new bool[tags.Length];
            ArrayUtility.Fill(tagsVisibility, true);

            FilterTags();
        }

        private void FilterTags()
        {
            string _searchFilter = searchFilter.ToLower();
            for (int _i = 0; _i < tags.Length; _i++)
            {
                bool _isVisible = tags[_i].Name.ToLower().Contains(_searchFilter);
                tagsVisibility[_i] = _isVisible;
            }
        }
        #endregion

        #region Create Tag Window
        /// <summary>
        /// Utility window used to create a new tag in the project.
        /// </summary>
        public class CreateTagWindow : TagWindow
        {
            /// <summary>
            /// Creates and shows a new <see cref="CreateTagWindow"/> instance,
            /// used to create a new tag in the project.
            /// </summary>
            /// <returns><see cref="CreateTagWindow"/> instance on screen.</returns>
            public static CreateTagWindow GetWindow()
            {
                CreateTagWindow _window = GetWindow<CreateTagWindow>("Create a new Tag", new Vector2(350f, 70f));
                _window.tagName = "NewTag";

                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const float NameWidth = 150f;
            private const float ColorWidth = 130f;

            private static readonly GUIContent createGUI = new GUIContent("OK", "Create this tag.");

            [SerializeField] private Color tagColor = TagData.DefaultColor.Get();

            protected override GUIContent ValidateGUI => createGUI;

            // -----------------------

            protected override void WindowContentGUI(Rect _position)
            {
                // Name.
                _position.x += 50f;
                _position.width = NameWidth;
                tagName = EditorGUI.TextField(_position, tagName);

                // Color.
                _position.xMin += _position.width + 10f;
                _position.width = ColorWidth;
                tagColor = EditorGUI.ColorField(_position, tagColor);
            }

            protected override void Validate()
            {
                MultiTags.CreateTag(tagName, tagColor);
                InternalEditorUtility.RepaintAllViews();
            }
        }
        #endregion

        #region Rename Tag Window
        /// <summary>
        /// Utility window used to rename an existing tag.
        /// </summary>
        public class RenameTagWindow : TagWindow
        {
            /// <summary>
            /// Creates and shows a new <see cref="CreateTagWindow"/> instance,
            /// used to rename an existing tag in the project.
            /// </summary>
            /// <returns><see cref="CreateTagWindow"/> instance on screen.</returns>
            public static RenameTagWindow GetWindow(TagData _tag)
            {
                RenameTagWindow _window = GetWindow<RenameTagWindow>("Rename this Tag", new Vector2(325f, 70f));

                _window.tagName = _tag.name;
                _window.tag = _tag;

                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private readonly GUIContent renameGUI = new GUIContent("OK", "Rename this tag.");

            [SerializeField] private TagData tag = null;

            protected override GUIContent ValidateGUI => renameGUI;

            // -----------------------

            protected override void WindowContentGUI(Rect _position)
            {
                // Name.
                _position.x += 50f;
                _position.width = position.width - _position.x - 5f;
                tagName = EditorGUI.TextField(_position, tagName);
            }

            protected override void Validate()
            {
                MultiTags.SetTagName(tag, tagName);
                InternalEditorUtility.RepaintAllViews();
            }
        }
        #endregion

        #region Tag Window Base Class
        /// <summary>
        /// Abstract class inherited by both <see cref="CreateTagWindow"/> and <see cref="RenameTagWindow"/> windows.
        /// </summary>
        public abstract class TagWindow : EditorWindow
        {
            internal static T GetWindow<T>(string _title, Vector2 _size) where T : EditorWindow
            {
                T _window = GetWindow<T>(true, _title, true);
                _window.minSize = _window.maxSize
                                = _size;

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const string UndoRecordTitle = "New Tag Changes";

            protected const string EmptyTagMessage = "A Tag name cannot be null or empty!";
            protected const string ExistingTagMessage = "A similar Tag with this name already exist.";
            protected const string NameTooltip = "You can use underscores ( \" _ \" ) to create categories in the selection menus.";

            protected const float PrefixWidth = 40f;

            protected static readonly GUIContent tagGUI = new GUIContent("Tag:");

            protected abstract GUIContent ValidateGUI { get; }

            [SerializeField] protected string tagName = string.Empty;

            // -----------------------

            private void OnGUI()
            {
                Undo.RecordObject(this, UndoRecordTitle);

                // Header and content.
                Rect _position = new Rect(5f, 5f, PrefixWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, tagGUI);

                WindowContentGUI(_position);

                // Name validation.
                string _value = tagName.Trim();
                if (string.IsNullOrEmpty(_value))
                {
                    DrawHelpBox(EmptyTagMessage);
                }
                else if (MultiTags.DoesTagExist(_value))
                {
                    DrawHelpBox(ExistingTagMessage);
                }
                else
                {
                    // Window validation button.
                    DrawTooltipHelpBox();

                    _position = new Rect()
                    {
                        x = position.width - 55f,
                        y = _position.y + _position.height + 10f,
                        width = 50f,
                        height = 25f
                    };

                    if (GUI.Button(_position, ValidateGUI))
                    {
                        Validate();
                        Close();
                    }
                }

                // ----- Local Method ----- \\

                void DrawHelpBox(string _message)
                {
                    _position = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 10f
                    };

                    _position.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, UnityEditor.MessageType.Error, _position.width);
                    EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);
                }

                void DrawTooltipHelpBox()
                {
                    Rect _temp = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 65f
                    };

                    _temp.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(NameTooltip, UnityEditor.MessageType.Info, _temp.width);
                    EditorGUI.HelpBox(_temp, NameTooltip, UnityEditor.MessageType.Info);
                }
            }

            protected abstract void WindowContentGUI(Rect _position);

            protected abstract void Validate();
        }
        #endregion
    }
}
