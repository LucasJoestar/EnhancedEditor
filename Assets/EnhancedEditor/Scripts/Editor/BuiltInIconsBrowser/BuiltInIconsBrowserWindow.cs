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
    /// Editor window used to browse Unity built-in icons.
    /// </summary>
	public class BuiltInIconsBrowserWindow : EditorWindow
    {
        #region Icon Wrapper
        [Serializable]
        private class Icon
        {
            public string Name = "Icon";
            public GUIContent IconContent = null;
            public GUIContent FullContent = null;

            public Vector2 Size = new Vector2();
            public float LargerSide = 0f;

            public Icon(GUIContent _content)
            {
                Texture _texture = _content.image;

                Name = _texture.name;
                IconContent = _content;
                FullContent = new GUIContent(_content)
                {
                    text = $" {Name}"
                };

                Size.Set(_texture.width, _texture.height);
                LargerSide = Mathf.Max(Size.x, Size.y);
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="BuiltInIconsBrowserWindow"/> currently on screen.
        /// <para/>
        /// Creates and shows a new instance if there is none.
        /// </summary>
        [MenuItem("Enhanced Editor/Built-in Icons Browser", false, 10)]
        public static BuiltInIconsBrowserWindow GetWindow()
        {
            BuiltInIconsBrowserWindow _window = GetWindow<BuiltInIconsBrowserWindow>("Built-in Icons Browser");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float SortOptionsWidth = 120f;
        private const float DisplayedIconsWidth = 140f;
        private const float SizeSliderWidth = 50f;
        private const float SizeSliderMinValue = 1f;
        private const float SizeSliderMaxValue = 3f;

        private const float ListIconSize = 32f;
        private const float GridIconSize = 32f;
        private const float SmallIconSize = 24f;
        private const float LargeIconSize = 60f;

        private const float InspectorHeight = 125f;
        private const float InspectorPreview = 100f;
        private const float CodeSnippetMargin = 100f;
        private const float CopyButtonWidth = 75f;

        private const string DisplayedIconsFormat = "Displayed Icons: {0}";
        private const string SizeFormat = "Size: {0}x{1}";
        private const string CodeSnippetInfo = "To load this icon, you can use the following code snippets:";

        private readonly string[] CodeSnippetFormats = new string[]
                                                {
                                                            "EditorGUIUtility.IconContent(\"{0}\");",
                                                            "EditorGUIUtility.FindTexture(\"{0}\");"
                                                };

        private readonly GUIContent displayedIconsGUI = new GUIContent(string.Empty, "Total amount of currently displayed icons.");
        private readonly GUIContent[] sortOptionsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Sort by name", "Sort icons by their name."),
                                                                new GUIContent("Sort by size", "Sort icons by their size."),
                                                            };

        private readonly GUIContent iconNameHeaderGUI = new GUIContent("Name:", "Name of the selected icon.");
        private readonly GUIContent iconNameGUI = new GUIContent(string.Empty, "Name of this icon.");
        private readonly GUIContent copyCodeGUI = new GUIContent("Copy", "Copy this code snippet.");
        private readonly GUIContent sizeGUI = new GUIContent(string.Empty, "Size of this icon.");
        private readonly GUIContent[] tabsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("All Icons"),
                                                                new GUIContent("Small"),
                                                                new GUIContent("Medium"),
                                                                new GUIContent("Large")
                                                            };

        private readonly Color oddColor = EnhancedEditorGUIUtility.GUIOddColor;
        private readonly Color listselectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private readonly Color gridSelectedColor = new Color(.75f, .75f, .75f, 1f);

        private Icon[] icons = new Icon[] { };
        private Icon[] filteredIcons = new Icon[] { };

        private int selectedSortOption = 0;
        private bool doSortAscending = true;
        private string searchFilter = string.Empty;
        private float sizeSlider = 1f;

        private int selectedTab = 0;
        private Icon selectedIcon = null;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            List<Icon> _icons = new List<Icon>();
            Texture2D[] _textures = Resources.FindObjectsOfTypeAll<Texture2D>();

            // The most convenient way to detect built-in icons is to simply try to load them, as nothing in their path can tell them apart.
            // As Unity will automatically log an error when a load fails, disable logger temporarily.
            Debug.unityLogger.logEnabled = false;
            foreach (Texture2D _texture in _textures)
            {
                GUIContent _content = EditorGUIUtility.IconContent(_texture.name, _texture.name);
                if ((_content != null) && (_content.image != null))
                {
                    Icon _icon = new Icon(_content);
                    _icons.Add(_icon);
                }
            }

            Debug.unityLogger.logEnabled = true;

            icons = _icons.ToArray();
            if (selectedIcon == null)
                SelectIcon(icons[0]);

            FilterIcons();
        }

        private void OnGUI()
        {
            // Toolbar comes first.
            DrawToolbar();

            // Then draw selected icon inspector.
            DrawInspector();

            // Tabs to select icon type to be displayed.
            GUILayout.Space(5f);

            int _selectedTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTab, tabsGUI, GUILayout.Height(25f));
            if (_selectedTab != selectedTab)
            {
                selectedTab = _selectedTab;
                FilterIcons();
            }

            GUILayout.Space(5f);

            // Finally, draw each icons.
            DrawIcons();
        }
        #endregion

        #region GUI Drawers
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Sorting options.
            if (EnhancedEditorGUILayout.ToolbarSortOptions(ref selectedSortOption, ref doSortAscending, sortOptionsGUI, GUILayout.Width(SortOptionsWidth)))
            {
                SortIcons();
            }

            // Search field.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter, GUILayout.MinWidth(100f));
            if (_searchFilter != searchFilter)
            {
                searchFilter = _searchFilter;
                FilterIcons();
            }

            // Total displayed icons.
            GUILayout.Label(displayedIconsGUI, EnhancedEditorStyles.LeftAlignedToolbarButton, GUILayout.Width(DisplayedIconsWidth));

            // Size slider.
            sizeSlider = GUILayout.HorizontalSlider(sizeSlider, SizeSliderMinValue, SizeSliderMaxValue, GUILayout.Width(SizeSliderWidth));

            GUILayout.Space(7f);
            GUILayout.EndHorizontal();
        }

        private void DrawInspector()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Height(InspectorHeight));
            GUILayout.BeginVertical();

            // Icon name.
            Rect _position = EditorGUILayout.GetControlRect();
            _position.width = 50f;
            EnhancedEditorGUI.UnderlinedLabel(_position, iconNameHeaderGUI, EditorStyles.boldLabel);

            _position.xMin = _position.xMax;
            _position.xMax = position.xMax;

            GUI.Label(_position, iconNameGUI);

            // Code snippets.
            GUILayout.Space(10f);
            EditorGUILayout.HelpBox(CodeSnippetInfo, UnityEditor.MessageType.Info);
            GUILayout.Space(5f);

            for (int _i = 0; _i < CodeSnippetFormats.Length; _i++)
            {
                string _snippet = string.Format(CodeSnippetFormats[_i], selectedIcon.Name);

                _position = EditorGUILayout.GetControlRect();
                _position.xMin += Mathf.Min(position.width * .075f, CodeSnippetMargin);
                _position.xMax -= CopyButtonWidth + 5f;

                EditorGUI.TextField(_position, _snippet);
                _position.xMin = _position.xMax + 5f;
                _position.width = CopyButtonWidth;

                if (GUI.Button(_position, copyCodeGUI, EditorStyles.miniButton))
                    EditorGUIUtility.systemCopyBuffer = _snippet;
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10f);

            // Icon preview.
            GUILayout.BeginVertical(GUILayout.Width(InspectorPreview));
            GUILayout.Label(selectedIcon.IconContent.image, EnhancedEditorStyles.Button, GUILayout.Width(InspectorPreview), GUILayout.Height(InspectorPreview));

            GUILayout.Label(sizeGUI, EnhancedEditorStyles.CenteredLabel);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawIcons()
        {
            scroll = GUILayout.BeginScrollView(scroll);

            // Display icons as a list if slider value is at its minimum, and as a grid otherwise.
            if (sizeSlider == SizeSliderMinValue)
            {
                GUILayoutOption _height = GUILayout.Height(ListIconSize);
                for (int _i = 0; _i < filteredIcons.Length; _i++)
                {
                    Icon _icon = filteredIcons[_i];
                    Rect _position = EditorGUILayout.GetControlRect(_height);
                    _position.xMin = 0f;
                    _position.xMax = position.xMax;

                    // Background color.
                    if (_icon == selectedIcon)
                    {
                        EditorGUI.DrawRect(_position, listselectedColor);
                    }
                    else if ((_i % 2) == 0)
                    {
                        EditorGUI.DrawRect(_position, oddColor);
                    }

                    _position.xMin += 5f;
                    _position.yMin += 2f;
                    _position.yMax -= 2f;

                    // Selection button.
                    if (GUI.Button(_position, _icon.FullContent, EditorStyles.label))
                    {
                        SelectIcon(_icon);
                    }
                }
            }
            else
            {
                // Remove vertical spacing from height as it will be automatically reserved by layout.
                float _size = GridIconSize * sizeSlider;
                GUILayoutOption _height = GUILayout.Height(_size - EditorGUIUtility.standardVerticalSpacing);

                float _screenWidth = Screen.width - 15f;
                float _gridCount = Mathf.Floor(_screenWidth / _size);
                float _margin = (_screenWidth % _size) / 2f;
                int _index = 0;

                while (_index < filteredIcons.Length)
                {
                    Rect _position = EditorGUILayout.GetControlRect(_height);
                    _position.xMin += _margin;
                    _position.width = _size;
                    _position.height += EditorGUIUtility.standardVerticalSpacing;

                    for (int _i = 0; _i < _gridCount; _i++)
                    {
                        Icon _icon = filteredIcons[_index];
                        bool _selected = _icon == selectedIcon;

                        if (_selected)
                            EnhancedEditorGUIUtility.PushGUIBackgroundColor(gridSelectedColor);

                        // Selection button.
                        if (GUI.Button(_position, _icon.IconContent))
                        {
                            SelectIcon(_icon);
                        }

                        if (_selected)
                            EnhancedEditorGUIUtility.PopGUIBackgroundColor();

                        // Increment index.
                        _index++;
                        if (_index == filteredIcons.Length)
                            break;

                        _position.x += _position.width;
                    }
                }
            }

            GUILayout.EndScrollView();
        }
        #endregion

        #region Utility
        private void FilterIcons()
        {
            List<Icon> _filteredIcons = new List<Icon>();
            string _searchFilter = searchFilter.ToLower();
            bool _useSearchFilter = !string.IsNullOrEmpty(searchFilter);

            foreach (Icon _icon in icons)
            {
                switch (selectedTab)
                {
                    // All icons.
                    case 0:
                        break;

                    // Small.
                    case 1:
                        if (_icon.LargerSide > SmallIconSize)
                            continue;

                        break;

                    // Medium.
                    case 2:
                        if ((_icon.LargerSide < SmallIconSize) || (_icon.LargerSide > LargeIconSize))
                            continue;

                        break;

                    // Big.
                    case 3:
                        if (_icon.LargerSide < LargeIconSize)
                            continue;

                        break;

                    default:
                        break;
                }

                if (!_useSearchFilter || _icon.Name.ToLower().Contains(_searchFilter))
                {
                    _filteredIcons.Add(_icon);
                }
            }

            filteredIcons = _filteredIcons.ToArray();
            displayedIconsGUI.text = string.Format(DisplayedIconsFormat, filteredIcons.Length);

            SortIcons();
        }

        private void SortIcons()
        {
            switch (selectedSortOption)
            {
                // Name.
                case 0:
                    Array.Sort(filteredIcons, (a, b) =>
                    {
                        return a.Name.CompareTo(b.Name);
                    });
                    break;

                // Size.
                case 1:
                    Array.Sort(filteredIcons, (a, b) =>
                    {
                        int _compare = a.LargerSide.CompareTo(b.LargerSide);
                        if (_compare != 0)
                            return _compare;

                        return a.Name.CompareTo(b.Name);
                    });
                    break;

                default:
                    break;
            }

            if (!doSortAscending)
                Array.Reverse(filteredIcons);
        }

        private void SelectIcon(Icon _icon)
        {
            selectedIcon = _icon;

            iconNameGUI.text = _icon.Name;
            sizeGUI.text = string.Format(SizeFormat, _icon.Size.x, _icon.Size.y);

            EditorGUIUtility.editingTextField = false;
        }
        #endregion
    }
}
