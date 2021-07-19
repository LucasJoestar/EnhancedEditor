// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multi-Tags system management window.
    /// </summary>
	public class MultiTagsWindow : EditorWindow
    {
        #region Window GUI
        /// <summary>
        /// Get and open the Multi-Tags window.
        /// </summary>
        [MenuItem("Enhanced Editor/Multi-Tags", false, 130)]
        public static void GetWindow() => GetWindow<MultiTagsWindow>("Multi-Tags");

        // -------------------------------------------
        // Global Members
        // -------------------------------------------

        private readonly GUIContent createTagGUI = new GUIContent(" Create Tag", "Create a new tag and add it to the project.");
        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh project tags.");
        private readonly GUIContent headerGUI = new GUIContent("Game Tags:", "All created tags in the project.");

        private readonly string deleteTagDialogTitle = "Delete tag";
        private readonly string deleteTagDialogMessage = "You are about to completely erase this tag from your project.\n" +
                                                         "Are you sure you want to do this? All uses of this tag will be obsolete and ignored.";

        private readonly string deleteTagDialogOk = "Yes";
        private readonly string deleteTagDialogCancel = "Cancel";

        private readonly string tooltipMessage = "You can edit existing tags name and color with the context menu.";
        private readonly string noTagWithPatternMessage = "No tags found with the entered search pattern.";
        private readonly string noTagMessage = "No custom tags found on the project. You can create one using the button above.";

        private TagObject[] tags = new TagObject[] { };

        private TagObject[] filteredTags = new TagObject[] { };
        private string searchPattern = string.Empty;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            createTagGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");
            RefreshTags();

            MultiTagsUtility.OnCreateTag += OnCreateTag;
            MultiTagsUtility.OnDeleteTag += OnDeleteTag;
        }

        private void OnDisable()
        {
            MultiTagsUtility.OnCreateTag -= OnCreateTag;
            MultiTagsUtility.OnDeleteTag -= OnDeleteTag;
        }

        private void OnGUI()
        {
            // Toolbar.
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(createTagGUI, EditorStyles.toolbarButton, GUILayout.Width(95f)))
            {
                CreateTagWindow.GetWindow();
            }

            string _searchPattern = EnhancedEditorGUILayout.ToolbarSearchField(searchPattern);
            if (_searchPattern != searchPattern)
            {
                searchPattern = _searchPattern;
                FilterTags();
            }

            //GUILayout.FlexibleSpace();
            if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton, GUILayout.Width(55f)))
            {
                RefreshTags();
            }

            GUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Draw tags.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 10f));

            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI, EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);

            if (tags.Length > 0)
            {
                if (filteredTags.Length > 0)
                {
                    Rect _position = EditorGUILayout.GetControlRect(true, 0f);
                    _position.width = position.width - 10f;

                    float _height = EnhancedEditorGUI.TagGroupField(_position, filteredTags, null, ShowDeleteTagDialog, null);
                    EditorGUILayout.GetControlRect(true, _height);

                    EditorGUILayout.Space(5f);
                    EditorGUILayout.HelpBox(tooltipMessage, UnityEditor.MessageType.Info, true);
                }
                else
                {
                    EditorGUILayout.HelpBox(noTagWithPatternMessage, UnityEditor.MessageType.Info, true);
                }                
            }
            else
            {
                EditorGUILayout.HelpBox(noTagMessage, UnityEditor.MessageType.Info, true);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
            Repaint();
        }
        #endregion

        #region Utility
        public void ShowDeleteTagDialog(long _id)
        {
            if (EditorUtility.DisplayDialog(deleteTagDialogTitle, deleteTagDialogMessage, deleteTagDialogOk, deleteTagDialogCancel))
            {
                MultiTagsUtility.DeleteTag(_id);
            }
        }

        private void FilterTags()
        {
            if (!string.IsNullOrEmpty(searchPattern))
            {
                List<TagObject> _filteredTags = new List<TagObject>();
                string _searchPattern = searchPattern.ToLower();

                foreach (TagObject _tag in tags)
                {
                    if (_tag.Name.ToLower().Contains(_searchPattern))
                        _filteredTags.Add(_tag);
                }

                filteredTags = _filteredTags.ToArray();
            }
            else
                filteredTags = tags;
        }
        #endregion

        #region Refresh
        private void OnCreateTag(long _id) => RefreshTags();

        private void OnDeleteTag(long _id) => RefreshTags();

        // -----------------------

        private void RefreshTags()
        {
            TagSettings _settings = MultiTagsUtility.Resource.Reload()[0];
            tags = _settings.GetAllTagObjects();

            FilterTags();
        }
        #endregion
    }

    /// <summary>
    /// Utility window used to create a new tag in the project.
    /// </summary>
    public class CreateTagWindow : EditorWindow
    {
        #region Window GUI
        private const float WindowWidth = 350f;
        private const float WindowHeight = 70f;

        private const float PrefixWidth = 40f;
        private const float NameWidth = 150f;
        private const float ColorWidth = 130f;

        private readonly GUIContent tagGUI = new GUIContent("Tag:", "Use underscores ( \"_\" ) to create categories in the context menu.");
        private readonly GUIContent okGUI = new GUIContent("OK", "Create this tag.");

        private readonly string emptyTagMessage = "Tag cannot be null or empty!";
        private readonly string existingTagMessage = "Similar Tag already exist.";

        private Action<long> onCreate = null;

        private string tagName = "NewTag";
        private Color tagColor = Color.white;

        // -----------------------

        public static void GetWindow(Action<long> _onCreate = null)
        {
            CreateTagWindow _window = GetWindow<CreateTagWindow>(true, "Create new Tag", true);
            _window.minSize = new Vector2(WindowWidth, WindowHeight);
            _window.maxSize = new Vector2(WindowWidth, WindowHeight);

            _window.onCreate = _onCreate;

            _window.ShowUtility();
        }

        private void OnGUI()
        {
            Rect _position = new Rect(5f, 5f, PrefixWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(_position, tagGUI);

            _position.x += 50f;
            _position.width = NameWidth;
            tagName = EditorGUI.TextField(_position, tagName);

            _position.xMin = _position.xMax + 10f;
            _position.width = ColorWidth;
            tagColor = EditorGUI.ColorField(_position, tagColor);

            // Validation.
            string _value = tagName.Trim();
            if (string.IsNullOrEmpty(_value))
            {
                DrawHelpBox(emptyTagMessage);
            }
            else if (MultiTagsUtility.DoesTagExist(_value, out _))
            {
                DrawHelpBox(existingTagMessage);
            }
            else
            {
                _position.x = position.width - 55f;
                _position.y += _position.height + 10f;
                _position.width = 50f;
                _position.height = 25f;

                if (GUI.Button(_position, okGUI))
                {
                    long _id = MultiTagsUtility.CreateTag(tagName, tagColor);
                    onCreate?.Invoke(_id);

                    Close();
                }
            }

            // ----- Local Method ----- //
            void DrawHelpBox(string _message)
            {
                _position.x = 5f;
                _position.y += _position.height + 5f;
                _position.height = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;
                _position.width = position.width - 10f;

                EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);
            }
        }
        #endregion
    }

    /// <summary>
    /// Utility window used to modify the name of an existing tag.
    /// </summary>
    public class ModifyTagNameWindow : EditorWindow
    {
        #region Window GUI
        private const float WindowWidth = 200f;
        private const float WindowHeight = 70f;

        private const float PrefixWidth = 40f;

        private readonly GUIContent tagGUI = new GUIContent("Tag:", "Use underscores ( \"_\" ) to create categories in the context menu.");
        private readonly GUIContent okGUI = new GUIContent("OK", "Modify this tag name.");

        private readonly string emptyTagMessage = "Tag cannot be null or empty!";
        private readonly string existingTagMessage = "A Tag with this name already exist.";

        private TagObject tag = null;
        private string tagName = string.Empty;

        // -----------------------

        public static void GetWindow(TagObject _tag)
        {
            ModifyTagNameWindow _window = GetWindow<ModifyTagNameWindow>(true, "Modify Tag name", true);
            _window.minSize = new Vector2(WindowWidth, WindowHeight);
            _window.maxSize = new Vector2(WindowWidth, WindowHeight);

            _window.tag = _tag;
            _window.tagName = _tag.Name;

            _window.ShowUtility();
        }

        private void OnGUI()
        {
            Rect _position = new Rect(5f, 5f, PrefixWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(_position, tagGUI);

            _position.x += 50f;
            _position.width = position.width - _position.x;
            tagName = EditorGUI.TextField(_position, tagName);

            // Validation.
            string _value = tagName.Trim();
            if (string.IsNullOrEmpty(_value))
            {
                DrawHelpBox(emptyTagMessage);
            }
            else if (MultiTagsUtility.DoesTagExist(_value, out _) && (_value != tag.Name))
            {
                DrawHelpBox(existingTagMessage);
            }
            else
            {
                _position.x = position.width - 55f;
                _position.y += _position.height + 10f;
                _position.width = 50f;
                _position.height = 25f;

                if (GUI.Button(_position, okGUI))
                {
                    MultiTagsUtility.SetTagName(tag.ID, tagName);
                    Close();
                }
            }

            // ----- Local Method ----- //
            void DrawHelpBox(string _message)
            {
                _position.x = 5f;
                _position.y += _position.height + 5f;
                _position.height = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;
                _position.width = position.width - 10f;

                EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);
            }
        }
        #endregion
    }
}
