// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor window focused on the build pipeline, used to build with additional parameters,
    /// launch existing builds, and manage custom scripting define symbols activation.
    /// </summary>
    public class BuildPipelineWindow : EditorWindow
    {
        #region Build Info
        [Serializable]
        private class BuildInfo
        {
            public string Name = string.Empty;
            public string Path = string.Empty;
            public string Platform = string.Empty;
            public string CreationDate = string.Empty;
            public GUIContent Icon = new GUIContent();
            public bool IsVisible = true;

            // -----------------------

            public BuildInfo(string _path)
            {
                Name = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(_path));
                Path = _path;

                string _platform = LoadPresetFromBuild(this)
                                    ? selectedBuildPreset.BuildTarget.ToString()
                                    : Name;

                Platform = GetBuildIcon(_platform, Icon);
                CreationDate = Directory.GetCreationTime(_path).ToString();
            }
        }
        #endregion

        #region Scene Wrapper
        [Serializable]
        private class SceneWrapper
        {
            public UnityEditor.SceneAsset Scene = null;
            public GUIContent Label = null;
            public bool IsEnabled = false;
            public bool IsSelected = false;
            public bool IsVisible = true;

            // -----------------------

            public SceneWrapper(UnityEditor.SceneAsset _scene, bool _isEnabled)
            {
                Scene = _scene;
                Label = new GUIContent(_scene?.name);
                IsEnabled = _isEnabled;

                IsSelected = false;
            }
        }
        #endregion

        #region Scripting Define Symbol Info
        [Serializable]
        private class ScriptingDefineSymbolInfo
        {
            public ScriptingDefineSymbolAttribute DefineSymbol = null;
            public GUIContent Label = null;
            public bool IsEnabled = false;
            public bool IsSelected = false;
            public bool IsVisible = true;

            // -----------------------

            public ScriptingDefineSymbolInfo(ScriptingDefineSymbolAttribute _symbol)
            {
                DefineSymbol = _symbol;
                Label = new GUIContent(_symbol.Description, _symbol.Symbol);

                IsEnabled = false;
                IsSelected = false;
            }
        }
        #endregion

        #region Menu Navigation
        /// <summary>
        /// Returns the first <see cref="BuildPipelineWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="BuildPipelineWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Build/Build Pipeline", false, 30)]
        public static BuildPipelineWindow GetWindow()
        {
            BuildPipelineWindow _window = GetWindow<BuildPipelineWindow>("Build Pipeline");
            _window.Show();

            return _window;
        }

        /// <summary>
        /// Launches the last created game build.
        /// </summary>
        [MenuItem(InternalUtility.MenuItemPath + "Build/Launch Last Build", false, 31)]
        public static void LaunchLastBuild()
        {
            string[] _builds = GetBuilds();
            if (_builds.Length > 0)
            {
                Array.Sort(_builds, (a, b) =>
                {
                    return Directory.GetCreationTime(b).CompareTo(Directory.GetCreationTime(a));
                });

                string _build = _builds[0];
                LaunchBuild(_build);
            }
            else
            {
                EditorUtility.DisplayDialog("No Build",
                                            $"No build could be found in the directory: \n\"{BuildDirectory}\".\n\n" +
                                            "Make sure you have selected a valid folder in the Preferences settings.",
                                            "OK");
            }
        }
        #endregion

        #region Window GUI
        private const float SectionWidthCoef = .6f;
        private const float SectionHeight = 400f;
        private const float ButtonHeight = 20f;
        private const float LargeButtonHeight = 25f;
        private const float RefreshButtonWidth = 60f;

        private const string UndoRecordTitle = "Build Pipeline Change";
        private const string PresetMetaDataFile = "preset.meta";
        public const char ScriptingDefineSymbolSeparator = ';';

        private static string BuildDirectory => EnhancedEditorSettings.Settings.UserSettings.BuildDirectory;

        public static readonly AutoManagedResource<BuildPreset> PresetResources = new AutoManagedResource<BuildPreset>("Custom", "BP_", string.Empty);
        private readonly GUIContent[] tabsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Game Builder", "Make a new build of the game."),
                                                                new GUIContent("Launcher", "Launch an existing game build."),
                                                                new GUIContent("Configuration", "Configure the game and build settings.")
                                                            };

        private readonly EditorColor sectionColor = new EditorColor(new Color(.65f, .65f, .65f), SuperColor.DarkGrey.Get());
        private readonly EditorColor peerColor = new EditorColor(new Color(.8f, .8f, .8f), new Color(.25f, .25f, .25f));
        private readonly EditorColor selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;

        private readonly Color validColor = SuperColor.Green.Get();
        private readonly Color warningColor = SuperColor.Crimson.Get();
        private readonly Color separatorColor = SuperColor.Grey.Get();

        [SerializeField] private int selectedTab = 0;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            EditorBuildSettings.sceneListChanged -= RefreshAllScenes;
            EditorBuildSettings.sceneListChanged += RefreshAllScenes;

            Undo.undoRedoPerformed -= OnUndoRedoOperation;
            Undo.undoRedoPerformed += OnUndoRedoOperation;

            titleContent.image = EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");

            InitializeBuildPresets();

            InitializeBuilder();
            InitializeLauncher();
            InitializeConfiguration();
        }

        private void OnGUI()
        {
            Undo.RecordObject(this, UndoRecordTitle);
            GUILayout.Space(5f);

            // Tab selection.
            selectedTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTab, tabsGUI, GUILayout.Height(25f));
            GUILayout.Space(5f);

            // Selected tab content.
            using (var _scroll = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scroll.scrollPosition;
                using (var _scope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(5f);

                    using (var _verticalScope = new GUILayout.VerticalScope())
                    {
                        switch (selectedTab)
                        {
                            case 0:
                                DrawBuilder();
                                break;

                            case 1:
                                DrawLauncher();
                                break;

                            case 2:
                                DrawConfiguration();
                                break;
                        }
                    }

                    GUILayout.Space(5f);
                }

                GUILayout.Space(10f);
            }
        }

        private void OnFocus()
        {
            // Refresh presets if any have been destroyed.
            if (ArrayUtility.Contains(buildPresets, null))
            {
                RefreshBuildPresets();
            }
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= RefreshAllScenes;
            Undo.undoRedoPerformed -= OnUndoRedoOperation;
        }

        // -----------------------

        private void OnBuildResetLayout()
        {
            // When building, the editor loses all of its horizontal and vertical layout scopes,
            // so begin them once again to avoid errors in the console.
            GUILayout.BeginScrollView(scroll);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
        }

        private void OnUndoRedoOperation()
        {
            switch (selectedTab)
            {
                // Builder.
                case 0:
                    OnBuilderUndoRedo();
                    break;

                // Launcher.
                case 1:
                    OnLauncherUndoRedo();
                    break;

                // Configuration
                case 2:
                    OnConfigurationUndoRedo();
                    break;
            }

            // Avoid out of range preset index.
            selectedPreset = Mathf.Min(selectedPreset, buildPresets.Length - 1);

            Repaint();
        }
        #endregion

        #region Builder
        private const float ProjectScenesSectionHeight = 250f;
        private const float AddButtonWidth = 150f;
        private const float RemoveButtonWidth = 175f;
        private const float BuildButtonWidth = 100f;

        private readonly GUIContent buildScenesHeaderGUI = new GUIContent("Build Scenes:", "Scenes to be included in the build.");
        private readonly GUIContent projectScenesHeaderGUI = new GUIContent("Project Scenes:", "All other project scenes, non included in the build.");

        private readonly GUIContent refreshBuildScenesGUI = new GUIContent("Refresh", "Refresh build scenes.");
        private readonly GUIContent refreshProjectScenesGUI = new GUIContent("Refresh", "Refresh project scenes.");
        private readonly GUIContent buildSceneEnabledGUI = new GUIContent(string.Empty, "Is this scene enabled and included in build?");
        private readonly GUIContent addScenesToBuildGUI = new GUIContent("Add Selected Scene(s)", "Adds selected scene(s) to build.");
        private readonly GUIContent removeScenesFromBuildGUI = new GUIContent("Remove Selected Scene(s)", "Removes selected scene(s) from build.");
        private readonly GUIContent enableScenesForBuildGUI = new GUIContent("Enable for Build", "Enables selected scene(s) for build.");
        private readonly GUIContent disableScenesForBuildGUI = new GUIContent("Disable for Build", "Disables selected scene(s) for build.");

        private readonly GUIContent buildTargetGUI = new GUIContent("Build Target", "The platform to build for.");
        private readonly GUIContent buildOptionsGUI = new GUIContent("Build Options", "Additional build options to use for this build.");
        private readonly GUIContent scriptingSymbolsGUI = new GUIContent("Scripting Symbols", "Scripting define symbols to be active in this build.");

        private readonly GUIContent builderDirectoryGUI = new GUIContent("Build in Directory:", "Directory in which to build the game.");
        private readonly GUIContent buildIdentifierGUI = new GUIContent("Build Identifier", "Additional identifier of this build.");
        private readonly GUIContent buildVersionGUI = new GUIContent("Version", "Version of this build.");
        private readonly GUIContent buildPresetGUI = new GUIContent("Build Preset:", "Selected preset to build the game with.");
        private readonly GUIContent buildButtonGUI = new GUIContent("BUILD", "Builds with the selected settings.");

        [SerializeField] private SceneWrapper[] buildScenes = new SceneWrapper[] { };
        [SerializeField] private SceneWrapper[] projectScenes = new SceneWrapper[] { };

        [SerializeField] private string buildVersion = string.Empty;
        [SerializeField] private string buildIdentifier = string.Empty;
        [SerializeField] private string buildScenesSearchFilter = string.Empty;
        [SerializeField] private string projectScenesSearchFilter = string.Empty;

        [SerializeField] private int lastSelectedBuildScene = -1;
        [SerializeField] private int lastSelectedProjectScene = -1;

        private int buildSceneControlID = -1;
        private int projectSceneControlID = -1;

        private ReorderableList buildScenesList = null;
        private bool isManualSceneUpdate = false;

        private Vector2 buildScenesScroll = new Vector2();
        private Vector2 projectScenesScroll = new Vector2();

        private bool doFocusBuildScene = false;
        private bool doFocusProjectScene = false;

        // -----------------------

        private void DrawBuilder()
        {
            // Get control IDs to use for keyboard focus on scene(s) selection.
            buildSceneControlID = EnhancedEditorGUIUtility.GetControlID(828, FocusType.Keyboard);
            projectSceneControlID = EnhancedEditorGUIUtility.GetControlID(829, FocusType.Keyboard);

            DrawTab(DrawBuilderHeader,
                    DrawBuilderSectionToolbar,
                    DrawBuilderSection,
                    BuilderSectionEvents,
                    DrawBuilderRightSide,
                    DrawBuilderBottom,
                    ref buildScenesScroll);
        }

        private void DrawBuilderHeader()
        {
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUIStyle _style = EditorStyles.boldLabel;

                EditorGUILayout.LabelField(buildScenesHeaderGUI, _style, GUILayout.Width((position.width * SectionWidthCoef) + 8f));
                EditorGUILayout.LabelField(projectScenesHeaderGUI, _style);
            }
        }

        private void DrawBuilderSectionToolbar()
        {
            // Search filter.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(buildScenesSearchFilter, GUILayout.MinWidth(50f));
            if (_searchFilter != buildScenesSearchFilter)
            {
                buildScenesSearchFilter = _searchFilter;
                FilterBuildScenes();
            }

            // Refresh button.
            if (GUILayout.Button(refreshBuildScenesGUI, EditorStyles.toolbarButton, GUILayout.Width(RefreshButtonWidth)))
            {
                RefreshAllScenes();
            }
        }

        private void DrawBuilderSection()
        {
            if (buildScenes.Length == 0)
                return;

            // Build scenes.
            if (string.IsNullOrEmpty(buildScenesSearchFilter))
            {
                buildScenesList.DoLayoutList();
            }
            else
            {
                DrawScenes(buildScenes, DrawBuildScene);
            }
        }

        private void DrawScenes(SceneWrapper[] _scenes, Action<Rect, int> _onDrawScene)
        {
            GUILayout.Space(3f);

            int _index = 0;
            for (int _i = 0; _i < _scenes.Length; _i++)
            {
                SceneWrapper _scene = _scenes[_i];
                if (!_scene.IsVisible)
                    continue;

                Rect _position = GetSectionElementPosition();
                DrawSceneBackground(_position, _scene, _index);

                _index++;
                _onDrawScene(_position, _i);
            }
        }

        private void DrawSceneBackground(Rect _position, SceneWrapper _scene, int _index)
        {
            EnhancedEditorGUI.BackgroundLine(_position, _scene.IsSelected, _index, selectedColor, peerColor);
        }

        private void DrawBuildScene(Rect _position, int _index)
        {
            SceneWrapper _scene = buildScenes[_index];
            Rect _temp = new Rect(_position.x + 5f, _position.y, 20f, _position.height);

            if (string.IsNullOrEmpty(buildScenesSearchFilter))
            {
                // Scene build index.
                EditorGUI.LabelField(_temp, _index.ToString(), EditorStyles.boldLabel);
            }

            // Scene name.
            _temp = new Rect(_position)
            {
                xMin = _position.x + 25f,
                xMax = _position.xMax - 25f
            };

            EditorGUI.LabelField(_temp, _scene.Label);

            // Enabled toggle.
            _temp.xMin = _temp.xMax;
            _temp.width = 25f;

            bool _enabled = GUI.Toggle(_temp, _scene.IsEnabled, buildSceneEnabledGUI);
            if (_enabled != _scene.IsEnabled)
            {
                _scene.IsEnabled = _enabled;
                UpdateBuildScenes();
            }

            // Scroll focus.
            if ((_index == lastSelectedBuildScene) && doFocusBuildScene && (Event.current.type == EventType.Repaint))
            {
                Vector2 _areaSize = new Vector2(0f, SectionHeight - 21f);
                buildScenesScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(buildScenesScroll, _position, _areaSize);

                doFocusBuildScene = false;
                Repaint();
            }

            // Select scene on click.
            EnhancedEditorGUIUtility.MultiSelectionClick(_position, buildScenes, _index, IsBuildSceneSelected, SelectBuildScene);
        }

        private void BuilderSectionEvents(Rect _position)
        {
            // Unselect on empty space click.
            if (EnhancedEditorGUIUtility.DeselectionClick(_position))
            {
                foreach (SceneWrapper _scene in buildScenes)
                    _scene.IsSelected = false;

                lastSelectedBuildScene = -1;
            }

            // Multi-selection keys.
            if (GUIUtility.keyboardControl == buildSceneControlID)
            {
                EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(buildScenes, IsBuildSceneSelected, CanSelectBuildScene, SelectBuildScene, lastSelectedBuildScene);
            }

            // Context click menu.
            if ((lastSelectedBuildScene > -1) && EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = new GenericMenu();
                _menu.AddItem(enableScenesForBuildGUI, false, () =>
                {
                    foreach (SceneWrapper _scene in buildScenes)
                    {
                        if (_scene.IsSelected && _scene.IsVisible)
                            _scene.IsEnabled = true;
                    }
                });

                _menu.AddItem(disableScenesForBuildGUI, false, () =>
                {
                    foreach (SceneWrapper _scene in buildScenes)
                    {
                        if (_scene.IsSelected && _scene.IsVisible)
                            _scene.IsEnabled = false;
                    }
                });

                _menu.AddItem(removeScenesFromBuildGUI, false, RemoveScenesFromBuild);
                _menu.ShowAsContext();

                Repaint();
            }

            // ----- Local Method ----- \\

            bool CanSelectBuildScene(int _index)
            {
                SceneWrapper _scene = buildScenes[_index];
                return _scene.IsVisible;
            }
        }

        private void DrawBuilderRightSide()
        {
            using (var _scope = new EditorGUILayout.VerticalScope(GUILayout.Height(ProjectScenesSectionHeight)))
            {
                Rect _position = _scope.rect;
                DrawSectionBackground(_position);

                // Toolbar.
                using (var _toolbarScope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    // Draw an empty button all over the toolbar to draw its bounds.
                    {
                        Rect _toolbarPosition = _toolbarScope.rect;
                        _toolbarPosition.xMin += 1f;

                        GUI.Label(_toolbarPosition, GUIContent.none, EditorStyles.toolbarButton);
                    }

                    // Search filter.
                    string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(projectScenesSearchFilter, GUILayout.MinWidth(50f));
                    if (_searchFilter != projectScenesSearchFilter)
                    {
                        projectScenesSearchFilter = _searchFilter;
                        FilterProjectScenes();
                    }

                    // Refresh button.
                    if (GUILayout.Button(refreshProjectScenesGUI, EditorStyles.toolbarButton, GUILayout.Width(RefreshButtonWidth)))
                    {
                        RefreshProjectScenes();
                    }
                }

                // Project scenes.
                using (var _scrollScope = new GUILayout.ScrollViewScope(projectScenesScroll))
                {
                    projectScenesScroll = _scrollScope.scrollPosition;
                    DrawScenes(projectScenes, DrawProjectScene);
                }

                // Unselect on empty space click.
                if (EnhancedEditorGUIUtility.DeselectionClick(_position))
                {
                    foreach (SceneWrapper _scene in projectScenes)
                        _scene.IsSelected = false;

                    lastSelectedProjectScene = -1;
                }

                // Multi-selection keys.
                if (GUIUtility.keyboardControl == projectSceneControlID)
                {
                    EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(projectScenes, IsProjectSceneSelected, CanSelectProjectScene, SelectProjectScene, lastSelectedProjectScene);
                }

                // Context click menu.
                if ((lastSelectedProjectScene > -1) && EnhancedEditorGUIUtility.ContextClick(_position))
                {
                    GenericMenu _menu = new GenericMenu();

                    _menu.AddItem(addScenesToBuildGUI, false, AddScenesToBuild);
                    _menu.ShowAsContext();

                    Repaint();
                }
            }

            GUILayout.Space(5f);

            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                bool _enabled = lastSelectedProjectScene > -1;

                using (EnhancedGUI.GUIEnabled.Scope(_enabled))
                using (EnhancedGUI.GUIColor.Scope(validColor))
                {
                    // Button to add selected scene(s) to build.
                    if (GUILayout.Button(addScenesToBuildGUI, GUILayout.Width(AddButtonWidth), GUILayout.Height(ButtonHeight)))
                    {
                        AddScenesToBuild();
                    }
                }
            }

            // Build settings.
            GUILayout.FlexibleSpace();
            DrawBuildDirectory(builderDirectoryGUI);

            using (var _scope = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(buildVersionGUI, GUILayout.Width(90f));
                string _version = EditorGUILayout.TextField(buildVersion);

                if (_version != buildVersion)
                {
                    buildVersion = _version;
                    PlayerSettings.bundleVersion = buildVersion;
                }
            }

            using (var _scope = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(buildIdentifierGUI, GUILayout.Width(90f));
                buildIdentifier = EditorGUILayout.TextField(buildIdentifier);
            }

            // ----- Local Method ----- \\

            bool CanSelectProjectScene(int _index)
            {
                SceneWrapper _scene = projectScenes[_index];
                return _scene.IsVisible;
            }
        }

        private void DrawProjectScene(Rect _position, int _index)
        {
            SceneWrapper _scene = projectScenes[_index];
            Rect _temp = new Rect(_position)
            {
                xMin = _position.x + 5f
            };

            // Scene label.
            EditorGUI.LabelField(_temp, _scene.Label);

            // Scroll focus.
            if ((_index == lastSelectedProjectScene) && doFocusProjectScene && (Event.current.type == EventType.Repaint))
            {
                Vector2 _areaSize = new Vector2(0f, ProjectScenesSectionHeight - 21f);
                projectScenesScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(projectScenesScroll, _position, _areaSize);

                doFocusProjectScene = false;
                Repaint();
            }

            // Select scene on click.
            EnhancedEditorGUIUtility.MultiSelectionClick(_position, projectScenes, _index, IsProjectSceneSelected, SelectProjectScene);
        }

        private void DrawBuilderBottom()
        {
            bool _enabled = lastSelectedBuildScene > -1;

            using (EnhancedGUI.GUIEnabled.Scope(_enabled))
            using (EnhancedGUI.GUIColor.Scope(warningColor))
            {
                // Button to remove selected scene(s) from build.
                if (GUILayout.Button(removeScenesFromBuildGUI, GUILayout.Width(RemoveButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    RemoveScenesFromBuild();
                }
            }

            // Build button.
            Rect _position = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);
            _position.Set(_position.xMax - BuildButtonWidth,
                          _position.y - EditorGUIUtility.singleLineHeight - 5f,
                          BuildButtonWidth,
                          LargeButtonHeight);

            using (var _scope = EnhancedGUI.GUIColor.Scope(validColor))
            {
                if (GUI.Button(_position, buildButtonGUI))
                {
                    Build();
                }
            }

            GUILayout.Space(5f);

            // Separator.
            EnhancedEditorGUILayout.HorizontalLine(separatorColor, GUILayout.Width(position.width * .5f), GUILayout.Height(2f));
            GUILayout.Space(5f);

            // Selected build preset.
            EnhancedEditorGUILayout.UnderlinedLabel(buildPresetGUI, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            DrawBuildPresets(false);
        }

        // -----------------------

        private bool IsBuildSceneSelected(int _index)
        {
            SceneWrapper _scene = buildScenes[_index];
            bool _isSelected = _scene.IsSelected && _scene.IsVisible;

            return _isSelected;
        }

        private void SelectBuildScene(int _index, bool _isSelected)
        {
            SceneWrapper _scene = buildScenes[_index];
            if (_isSelected && !_scene.IsVisible)
                return;

            _scene.IsSelected = _isSelected;

            // Last selected index update.
            if (_isSelected)
            {
                lastSelectedBuildScene = _index;
                doFocusBuildScene = true;

                GUIUtility.keyboardControl = buildSceneControlID;
            }
            else if (!Array.Exists(buildScenes, (s) => s.IsSelected))
            {
                lastSelectedBuildScene = -1;
            }
        }

        private bool IsProjectSceneSelected(int _index)
        {
            SceneWrapper _scene = projectScenes[_index];
            bool _isSelected = _scene.IsSelected && _scene.IsVisible;

            return _isSelected;
        }

        private void SelectProjectScene(int _index, bool _isSelected)
        {
            SceneWrapper _scene = projectScenes[_index];
            if (_isSelected && !_scene.IsVisible)
                return;

            _scene.IsSelected = _isSelected;

            // Last selected index update.
            if (_isSelected)
            {
                lastSelectedProjectScene = _index;
                doFocusProjectScene = true;

                GUIUtility.keyboardControl = projectSceneControlID;
            }
            else if (!Array.Exists(projectScenes, (s) => s.IsSelected))
            {
                lastSelectedProjectScene = -1;
            }
        }

        // -----------------------

        private void InitializeBuilder()
        {
            // Configures the build scene reorderable list.
            buildScenesList = new ReorderableList(buildScenes, typeof(SceneWrapper), true, false, false, false)
            {
                drawElementCallback = (Rect _position, int _index, bool _isFocused, bool _isSelected) => DrawBuildScene(_position, _index),
                drawElementBackgroundCallback = (Rect _position, int _index, bool _isFocused, bool _isSelected) =>
                {
                    _position.xMin += 1f;
                    _position.xMax -= 1f;
                    DrawSceneBackground(_position, buildScenes[_index], _index);
                },

                showDefaultBackground = false,
                onReorderCallback = (r) =>
                {
                    UpdateBuildScenes();
                    FilterBuildScenes();
                },

                headerHeight = 1f,
                footerHeight = 0f,
                elementHeight = EditorGUIUtility.singleLineHeight
            };

            buildVersion = Application.version;

            // Refresh scenes.
            RefreshAllScenes();
        }

        private void OnBuilderUndoRedo()
        {
            // In case of any changes made to build scenes, update them.
            if (!UpdateBuildScenes())
            {
                buildScenesList.list = buildScenes;
            }

            // Update build version.
            if (buildVersion != Application.version)
            {
                PlayerSettings.bundleVersion = buildVersion;
            }
        }

        private bool UpdateBuildScenes()
        {
            bool _refresh = false;
            isManualSceneUpdate = true;

            // Remove all null scene entries.
            int _length = buildScenes.Length;
            ArrayUtility.Filter(ref buildScenes, (s) => s.Scene != null);

            if (_length != buildScenes.Length)
            {
                RefreshBuildScenes();
                _refresh = true;
            }

            EditorBuildSettings.scenes = Array.ConvertAll(buildScenes, (s) =>
            {
                return new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(s.Scene), s.IsEnabled);
            });

            return _refresh;
        }

        private void RefreshAllScenes()
        {
            // Do not refresh scenes on manual updates.
            if (isManualSceneUpdate)
            {
                isManualSceneUpdate = false;
                return;
            }

            // Get all build scenes without null entries.
            buildScenes = Array.ConvertAll(EditorBuildSettings.scenes, (s) =>
            {
                return new SceneWrapper(AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(s.path), s.enabled);
            });

            ArrayUtility.Filter(ref buildScenes, (s) => s.Scene != null);
            lastSelectedBuildScene = -1;

            RefreshBuildScenes();
            RefreshProjectScenes();

            Repaint();
        }

        private void RefreshBuildScenes()
        {
            buildScenesList.list = buildScenes;
            FilterBuildScenes();
        }

        private void RefreshProjectScenes()
        {
            UnityEditor.SceneAsset[] _projectScenes = EnhancedEditorUtility.LoadAssets<UnityEditor.SceneAsset>();
            for (int _i = _projectScenes.Length; _i-- > 0;)
            {
                UnityEditor.SceneAsset _scene = _projectScenes[_i];
                if (Array.Exists(buildScenes, (b) => b.Scene == _scene))
                {
                    ArrayUtility.RemoveAt(ref _projectScenes, _i);
                }
            }

            projectScenes = Array.ConvertAll(_projectScenes, (s) => new SceneWrapper(s, false));
            lastSelectedProjectScene = -1;

            Array.Sort(projectScenes, (a, b) =>
            {
                return a.Scene.name.CompareTo(b.Scene.name);
            });

            FilterProjectScenes();
        }

        private void FilterBuildScenes()
        {
            FilterScenes(buildScenes, buildScenesSearchFilter);
        }

        private void FilterProjectScenes()
        {
            FilterScenes(projectScenes, projectScenesSearchFilter);
        }

        private void FilterScenes(SceneWrapper[] _scenes, string _searchFilter)
        {
            _searchFilter = _searchFilter.ToLower();
            foreach (SceneWrapper _scene in _scenes)
            {
                bool _isVisible = _scene.Scene.name.ToLower().Contains(_searchFilter);
                _scene.IsVisible = _isVisible;
            }
        }

        private void AddScenesToBuild()
        {
            MoveSelectedScenes(ref projectScenes, ref buildScenes);
            lastSelectedProjectScene = -1;
        }

        private void RemoveScenesFromBuild()
        {
            MoveSelectedScenes(ref buildScenes, ref projectScenes);
            lastSelectedBuildScene = -1;
        }

        private void MoveSelectedScenes(ref SceneWrapper[] _removeFrom, ref SceneWrapper[] _addTo)
        {
            for (int _i = _removeFrom.Length; _i-- > 0;)
            {
                SceneWrapper _scene = _removeFrom[_i];
                if (_scene.IsSelected)
                {
                    ArrayUtility.Add(ref _addTo, _scene);
                    ArrayUtility.RemoveAt(ref _removeFrom, _i);

                    _scene.IsSelected = false;
                    _scene.IsEnabled = true;
                }
            }

            UpdateBuildScenes();

            RefreshBuildScenes();
            FilterProjectScenes();
        }

        private void Build()
        {
            BuildPreset _preset = buildPresets[selectedPreset];
            Build(_preset);
        }

        /// <summary>
        /// Builds the game with a specific preset.
        /// </summary>
        /// <param name="_preset">Preset to build with.</param>
        /// <returns>True if the build succeeded, false otherwise.</returns>
        public bool Build(BuildPreset _preset)
        {
            // Get application short name.
            string _appName = string.Empty;
            foreach (char _char in Application.productName)
            {
                string _string = _char.ToString();
                if (!string.IsNullOrEmpty(_string) && (_string == _string.ToUpper()) && (_string != " "))
                    _appName += _string;
            }

            if (_appName.Length < 2)
                _appName = Application.productName;

            // Set build directory name.
            string _buildName = string.IsNullOrEmpty(buildIdentifier)
                                ? ($"{_appName}_v{Application.version}" +
                                   $"_{_preset.name.Replace(PresetResources.Prefix, string.Empty)}_{_preset.buildCount:000}" +
                                   $"_{_preset.BuildTarget}")
                                : $"{_appName}_v{Application.version}_{buildIdentifier}";

            string _buildPath = Path.Combine(BuildDirectory, _buildName);

            // Delete path before build to avoid conflicts or corrupted files.
            if (Directory.Exists(_buildPath))
                Directory.Delete(_buildPath, true);

            Directory.CreateDirectory(_buildPath);
            BuildPlayerOptions _options = new BuildPlayerOptions()
            {
                scenes = Array.ConvertAll(buildScenes, (s) => AssetDatabase.GetAssetPath(s.Scene)),
                locationPathName = $"{Path.Combine(_buildPath, Application.productName)}.exe",
                targetGroup = BuildPipeline.GetBuildTargetGroup(_preset.BuildTarget),
                target = _preset.BuildTarget,
                options = _preset.BuildOptions
            };

            string _symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_options.targetGroup);
            SetScriptingDefineSymbols(_options.targetGroup, _preset.ScriptingDefineSymbols);

            bool _succeed = BuildPipeline.BuildPlayer(_options).summary.result == BuildResult.Succeeded;
            if (_succeed)
            {
                _preset.buildCount++;
                SaveBuildPreset(_preset);

                string _presetMetaData = EditorJsonUtility.ToJson(_preset, true);
                File.WriteAllText(Path.Combine(_buildPath, PresetMetaDataFile), _presetMetaData);

                Process.Start(_buildPath);
            }
            else
            {
                Directory.Delete(_buildPath, true);
            }

            SetScriptingDefineSymbols(_options.targetGroup, _symbols);
            OnBuildResetLayout();

            return _succeed;
        }
        #endregion

        #region Launcher
        private const float SortByButtonWidth = 135f;
        private const float OpenBuildDirectoryButtonWidth = 100f;
        private const float LaunchButtonWidth = 100f;

        private const string SelectedBuildHelpBox = "Informations about the selected game build will be displayed here.";

        private readonly GUIContent launcherHeaderGUI = new GUIContent("Builds:", "All game builds found in the build directory.");
        private readonly GUIContent buildDirectoryGUI = new GUIContent("Build Directory:", "Directory where to look for builds.");
        private readonly GUIContent buildInfoGUI = new GUIContent("Build Infos:", "Informations about the selected build.");
        private readonly GUIContent selectedBuildPresetGUI = new GUIContent("Build Preset:", "Preset used in this build.");

        private readonly GUIContent refreshBuildsGUI = new GUIContent("Refresh", "Refresh builds.");
        private readonly GUIContent[] buildSortOptionsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Sort by date", "Sort the builds by their creation date."),
                                                                new GUIContent("Sort by name", "Sort the builds by their name."),
                                                                new GUIContent("Sort by platform", "Sort the builds by their platform.")
                                                            };

        private readonly GUIContent openBuildDirectoryGUI = new GUIContent("Open Directory", "Opens this build directory.");
        private readonly GUIContent launchAmountGUI = new GUIContent("Launch Instance", "Amount of the selected build instance to launch.");
        private readonly GUIContent launchBuildGUI = new GUIContent("LAUNCH", "Launch selected build.");

        private readonly GUIContent launchBuildMenuGUI = new GUIContent("Launch", "Launch this build.");
        private readonly GUIContent deleteBuildGUI = new GUIContent("Delete", "Delete this build.");

        private readonly Color launcherColor = SuperColor.Cyan.Get();

        [SerializeField] private int selectedBuild = -1;
        [SerializeField] private int launchInstance = 1;

        [SerializeField] private int selectedBuildSortOption = 0;
        [SerializeField] private bool doSortBuildsAscending = true;
        [SerializeField] private string buildsSearchFilter = string.Empty;

        private int buildControlID = -1;

        private BuildInfo[] builds = new BuildInfo[] { };
        private static BuildPreset selectedBuildPreset = null;

        private Vector2 buildsScroll = new Vector2();
        private bool doFocusBuild = false;

        // -----------------------

        private void DrawLauncher()
        {
            // Get control ID to use for keyboard focus on build selection.
            buildControlID = EnhancedEditorGUIUtility.GetControlID(827, FocusType.Keyboard);

            DrawTab(DrawLauncherHeader,
                    DrawLauncherSectionToolbar,
                    DrawLauncherSection,
                    LauncherSectionEvents,
                    DrawLauncherRightSide,
                    DrawLauncherBottom,
                    ref buildsScroll);
        }

        private void DrawLauncherHeader()
        {
            EditorGUILayout.LabelField(launcherHeaderGUI, EditorStyles.boldLabel);
        }

        private void DrawLauncherSectionToolbar()
        {
            // Sort options.
            using (var _scope = new EditorGUI.ChangeCheckScope())
            {
                EnhancedEditorGUILayout.ToolbarSortOptions(ref selectedBuildSortOption, ref doSortBuildsAscending, buildSortOptionsGUI, GUILayout.Width(SortByButtonWidth));
                if (_scope.changed)
                {
                    SortBuilds();
                }
            }

            // Search filter.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(buildsSearchFilter, GUILayout.MinWidth(50f));
            if (_searchFilter != buildsSearchFilter)
            {
                buildsSearchFilter = _searchFilter;
                FilterBuilds();
            }

            // Refresh button.
            if (GUILayout.Button(refreshBuildsGUI, EditorStyles.toolbarButton, GUILayout.Width(RefreshButtonWidth)))
            {
                RefreshBuilds();
            }
        }

        private void DrawLauncherSection()
        {
            // Filtered Builds.
            GUILayout.Space(3f);

            int _index = 0;
            BuildInfo _selectedBuild = (selectedBuild != -1)
                                     ? builds[selectedBuild]
                                     : null;

            for (int _i = 0; _i < builds.Length; _i++)
            {
                BuildInfo _build = builds[_i];
                if (!_build.IsVisible)
                    continue;

                Rect _position = GetSectionElementPosition();

                // Background color.
                bool _isSelected = _build == _selectedBuild;
                EnhancedEditorGUI.BackgroundLine(_position, _isSelected, _index, selectedColor, peerColor);

                _index++;

                // Scroll focus.
                if (_isSelected && doFocusBuild && (Event.current.type == EventType.Repaint))
                {
                    Vector2 _areaSize = new Vector2(0f, SectionHeight - 21f);
                    buildsScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(buildsScroll, _position, _areaSize);

                    doFocusBuild = false;
                    Repaint();
                }

                // Build selection.
                if (EnhancedEditorGUIUtility.MouseDown(_position))
                {
                    SetSelectedBuild(_i);
                }

                // Build infos.
                _position.xMin += 5f;
                EditorGUI.LabelField(_position, _build.Icon);

                _position.xMin += 25f;
                EditorGUI.LabelField(_position, _build.Name);
            }
        }

        private void LauncherSectionEvents(Rect _position)
        {
            // Unselect on empty space click.
            if (EnhancedEditorGUIUtility.DeselectionClick(_position))
            {
                SetSelectedBuild(-1);
            }

            // Selection keys.
            if ((selectedBuild != -1) && (GUIUtility.keyboardControl == buildControlID))
            {
                int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                if (_switch != 0)
                {
                    int _index = selectedBuild;
                    while (true)
                    {
                        _index += _switch;
                        if ((_index == -1) || (_index == builds.Length))
                            break;

                        if (builds[_index].IsVisible)
                        {
                            SetSelectedBuild(_index);
                            break;
                        }
                    }
                }
            }

            // Context click menu.
            if ((selectedBuild != -1) && EnhancedEditorGUIUtility.ContextClick(_position))
            {
                BuildInfo _build = builds[selectedBuild];

                GenericMenu _menu = new GenericMenu();
                _menu.AddItem(openBuildDirectoryGUI, false, OpenBuildDirectory);
                _menu.AddItem(launchBuildMenuGUI, false, () => LaunchBuild(_build.Path, launchInstance));
                _menu.AddItem(deleteBuildGUI, false, DeleteBuild);

                _menu.ShowAsContext();
            }

            // ----- Local Method ----- \\

            void DeleteBuild()
            {
                BuildInfo _build = builds[selectedBuild];

                if (EditorUtility.DisplayDialog("Delete build", $"Are you sure you want to delete the build \"{_build.Name}\"?\n\n" +
                                                "This action cannot be undone.", "Yes", "Cancel"))
                {
                    Directory.Delete(Path.GetDirectoryName(_build.Path), true);
                    RefreshBuilds();
                }
            }
        }

        private void DrawLauncherRightSide()
        {
            EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 4f);

            // Build directory.
            DrawBuildDirectory(buildDirectoryGUI);
            GUILayout.Space(10f);

            // Build informations.
            EnhancedEditorGUILayout.UnderlinedLabel(buildInfoGUI, EditorStyles.boldLabel);
            GUILayout.Space(3f);

            if (selectedBuild != -1)
            {
                BuildInfo _build = builds[selectedBuild];
                GUIStyle _style = EnhancedEditorStyles.WordWrappedRichText;

                EditorGUILayout.LabelField($"Name:   <b><color=green>{_build.Name}</color></b>", _style);
                EditorGUILayout.LabelField($"Platform:   <b><color=teal>{_build.Platform}</color></b>", _style);
                EditorGUILayout.LabelField($"Creation Date:   <b><color=brown>{_build.CreationDate}</color></b>", _style);

                GUILayout.Space(10f);

                // Open build directory button.
                using (var _scope = new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (var _colorScope = EnhancedGUI.GUIColor.Scope(validColor))
                    {
                        if (GUILayout.Button(openBuildDirectoryGUI, GUILayout.Width(OpenBuildDirectoryButtonWidth), GUILayout.Height(LargeButtonHeight)))
                        {
                            OpenBuildDirectory();
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(SelectedBuildHelpBox, UnityEditor.MessageType.Info);
            }
        }

        private void DrawLauncherBottom()
        {
            using (var _scope = new GUILayout.HorizontalScope(GUILayout.Width(position.width * SectionWidthCoef)))
            {
                bool _canLaunch = selectedBuild != -1;

                using (EnhancedGUI.GUIEnabled.Scope(_canLaunch))
                using (EnhancedGUI.GUIColor.Scope(launcherColor))
                {
                    // Launch button.
                    if (GUILayout.Button(launchBuildGUI, GUILayout.Width(LaunchButtonWidth), GUILayout.Height(LargeButtonHeight)))
                    {
                        string _path = builds[selectedBuild].Path;
                        LaunchBuild(_path, launchInstance);
                    }
                }

                GUILayout.FlexibleSpace();

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(5f);
                    using (new GUILayout.HorizontalScope())
                    {
                        // Launch instance amount.
                        EditorGUILayout.LabelField(launchAmountGUI, GUILayout.Width(100f));

                        int _launchAmount = EditorGUILayout.IntField(launchInstance, GUILayout.Width(50f));
                        if (_launchAmount != launchInstance)
                        {
                            launchInstance = Mathf.Clamp(_launchAmount, 1, 10);
                        }
                    }
                }
            }

            // Draw associated build preset if one.
            if (selectedBuildPreset != null)
            {
                GUILayout.Space(9f);
                EnhancedEditorGUILayout.UnderlinedLabel(selectedBuildPresetGUI, EditorStyles.boldLabel);

                GUILayout.Space(22f);

                string _presetName = selectedBuildPreset.name.Replace(PresetResources.Prefix, string.Empty);
                EditorGUILayout.LabelField(_presetName, EditorStyles.boldLabel);
                DrawBuildPreset(selectedBuildPreset, true);
            }
        }

        // -----------------------

        private void InitializeLauncher()
        {
            RefreshBuilds();
        }

        private void OnLauncherUndoRedo()
        {
            // Refresh selected build.
            if (selectedBuild >= builds.Length)
                selectedBuild = -1;

            SetSelectedBuild(selectedBuild);
        }

        private void RefreshBuilds()
        {
            string[] _builds = GetBuilds();
            builds = Array.ConvertAll(_builds, (b) => new BuildInfo(b));

            SetSelectedBuild(-1);

            SortBuilds();
            FilterBuilds();
        }

        private void FilterBuilds()
        {
            string _searchFilter = buildsSearchFilter.ToLower();
            foreach (BuildInfo _build in builds)
            {
                bool _isVisible = _build.Name.ToLower().Contains(_searchFilter);
                _build.IsVisible = _isVisible;
            }
        }

        private void SortBuilds()
        {
            switch (selectedBuildSortOption)
            {
                // Creation date.
                case 0:
                    Array.Sort(builds, (a, b) =>
                    {
                        return Directory.GetCreationTime(a.Path).CompareTo(Directory.GetCreationTime(b.Path));
                    });
                    break;

                // Name.
                case 1:
                    Array.Sort(builds, (a, b) =>
                    {
                        return a.Name.CompareTo(b.Name);
                    });
                    break;

                // Platform.
                case 2:
                    Array.Sort(builds, (a, b) =>
                    {
                        return (a.Platform != b.Platform)
                               ? a.Platform.CompareTo(b.Platform)
                               : a.Name.CompareTo(b.Name);
                    });
                    break;
            }

            if (!doSortBuildsAscending)
                Array.Reverse(builds);
        }

        private bool SetSelectedBuild(int _selectedBuild)
        {
            if (_selectedBuild == -1)
            {
                selectedBuild = _selectedBuild;
                selectedBuildPreset = null;

                return false;
            }

            BuildInfo _build = builds[_selectedBuild];

            // Unvalid build: remove it.
            if (!File.Exists(_build.Path))
            {
                ArrayUtility.RemoveAt(ref builds, _selectedBuild);

                selectedBuild = -1;
                selectedBuildPreset = null;

                return false;
            }

            // Valid build: get associated preset.
            selectedBuild = _selectedBuild;
            LoadPresetFromBuild(_build);

            doFocusBuild = true;
            GUIUtility.keyboardControl = buildControlID;

            return true;
        }

        private void OpenBuildDirectory()
        {
            string _path = builds[selectedBuild].Path;
            Process.Start(Path.GetDirectoryName(_path));
        }

        private static bool LoadPresetFromBuild(BuildInfo _build)
        {
            string _presetPath = Path.Combine(Path.GetDirectoryName(_build.Path), PresetMetaDataFile);
            if (File.Exists(_presetPath))
            {
                try
                {
                    if (selectedBuildPreset == null)
                    {
                        selectedBuildPreset = CreateInstance<BuildPreset>();
                    }

                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(_presetPath), selectedBuildPreset);
                    return true;
                }
                catch (ArgumentException) { }
            }

            selectedBuildPreset = null;
            return false;
        }
        #endregion

        #region Configuration
        private const float ApplySymbolsButtonWidth = 65f;
        private const float UseSelectedSymbolsOnPresetButtonWidth = 210f;

        private const string NeedToApplySymbolsMessage = "You need to apply your changes in order to active newly selected symbols.";

        private readonly GUIContent activeSymbolHeaderGUI = new GUIContent("Active Symbols:", "Scripting define symbols currently active in the project.");
        private readonly GUIContent customSymbolHeaderGUI = new GUIContent("Scripting Define Symbols:", "All custom scripting define symbols in the project.");
        private readonly GUIContent buildPresetsHeaderGUI = new GUIContent("Build Presets:", "All registered build presets.");

        private readonly GUIContent applySymbolsGUI = new GUIContent("Apply", "Apply selected symbols on project.");
        private readonly GUIContent refreshSymbolsGUI = new GUIContent("Refresh", "Refresh project custom scripting define symbols.");
        private readonly GUIContent useSelectedSymbolsGUI = new GUIContent("Use Selected Symbol(s) on Preset", "Set the selected preset symbols as the ones currently selected.");
        private readonly GUIContent enableSymbolsGUI = new GUIContent("Enable Symbol(s)", "Enable the selected symbol(s).");
        private readonly GUIContent disableSymbolsGUI = new GUIContent("Disable Symbol(s)", "Disable the selected symbol(s).");

        private readonly Color configurationColor = SuperColor.Lavender.Get();

        [SerializeField] private string[] activeSymbols = new string[] { };
        [SerializeField] private string customSymbolsSearchFilter = string.Empty;
        [SerializeField] private int lastSelectedSymbol = -1;

        private int customSymbolControlID = -1;

        private ScriptingDefineSymbolInfo[] customSymbols = new ScriptingDefineSymbolInfo[] { };
        private string[] otherSymbols = new string[] { };
        private bool doNeedToApplySymbols = false;

        private Vector2 customSymbolsScroll = new Vector2();
        private Vector2 activeSymbolsScroll = new Vector2();
        private bool doFocusSymbol = false;

        // -----------------------

        private void DrawConfiguration()
        {
            // Get control ID to use for keyboard focus on symbol selection.
            customSymbolControlID = EnhancedEditorGUIUtility.GetControlID(826, FocusType.Keyboard);

            DrawTab(DrawConfigurationHeader,
                    DrawConfigurationSectionToolbar,
                    DrawConfigurationSection,
                    ConfigurationSectionEvents,
                    DrawConfigurationRightSide,
                    DrawConfigurationBottom,
                    ref customSymbolsScroll);
        }

        private void DrawConfigurationHeader()
        {
            EditorGUILayout.LabelField(customSymbolHeaderGUI, EditorStyles.boldLabel);
        }

        private void DrawConfigurationSectionToolbar()
        {
            // Search filter.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(customSymbolsSearchFilter, GUILayout.MinWidth(50f));
            if (_searchFilter != customSymbolsSearchFilter)
            {
                customSymbolsSearchFilter = _searchFilter;
                FilterCustomSymbols();
            }

            // Refresh button.
            if (GUILayout.Button(refreshSymbolsGUI, EditorStyles.toolbarButton, GUILayout.Width(RefreshButtonWidth)))
            {
                RefreshSymbols();
            }
        }

        private void DrawConfigurationSection()
        {
            GUILayout.Space(3f);
            int _index = 0;

            // Custom symbols.
            for (int _i = 0; _i < customSymbols.Length; _i++)
            {
                ScriptingDefineSymbolInfo _symbol = customSymbols[_i];
                if (!_symbol.IsVisible)
                    continue;

                Rect _position = GetSectionElementPosition();

                // Background color.
                EnhancedEditorGUI.BackgroundLine(_position, _symbol.IsSelected, _index, selectedColor, peerColor);
                _index++;

                // Symbol activation.
                Rect _temp = new Rect(_position)
                {
                    x = _position.x + 5f,
                    width = 20f
                };

                bool _isEnabled = EditorGUI.ToggleLeft(_temp, GUIContent.none, _symbol.IsEnabled);
                EnableSymbol(_symbol, _isEnabled);

                // Symbol description.
                _temp.xMin += _temp.width;
                _temp.xMax = _position.xMax;

                EditorGUI.LabelField(_temp, _symbol.Label);

                // Scroll focus.
                if ((_i == lastSelectedSymbol) && doFocusSymbol && (Event.current.type == EventType.Repaint))
                {
                    Vector2 _areaSize = new Vector2(0f, SectionHeight - 21f);
                    customSymbolsScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(customSymbolsScroll, _position, _areaSize);

                    doFocusSymbol = false;
                    Repaint();
                }

                // Select on click.
                EnhancedEditorGUIUtility.MultiSelectionClick(_position, customSymbols, _i, IsSymbolSelected, SelectSymbol);
            }
        }

        private void ConfigurationSectionEvents(Rect _position)
        {
            // Unselect on empty space click.
            if (EnhancedEditorGUIUtility.DeselectionClick(_position))
            {
                foreach (var _customSymbol in customSymbols)
                    _customSymbol.IsSelected = false;

                lastSelectedSymbol = -1;
            }

            // Multi-selection keys.
            if (GUIUtility.keyboardControl == customSymbolControlID)
            {
                EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(customSymbols, IsSymbolSelected, CanSelectSymbol, SelectSymbol, lastSelectedSymbol);
            }

            // Context click menu.
            if ((lastSelectedSymbol != -1) && EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = new GenericMenu();
                if (Array.Exists(customSymbols, (s) => s.IsSelected && !s.IsEnabled))
                {
                    _menu.AddItem(enableSymbolsGUI, false, EnableSymbols);
                }
                else
                {
                    _menu.AddDisabledItem(enableSymbolsGUI);
                }

                if (Array.Exists(customSymbols, (s) => s.IsSelected && s.IsEnabled))
                {
                    _menu.AddItem(disableSymbolsGUI, false, DisableSymbols);
                }
                else
                {
                    _menu.AddDisabledItem(disableSymbolsGUI);

                }

                _menu.AddItem(useSelectedSymbolsGUI, false, UseSelectedSymbolsOnPreset);
                _menu.ShowAsContext();
            }

            // ----- Local Methods ----- \\

            bool CanSelectSymbol(int _index)
            {
                ScriptingDefineSymbolInfo _symbol = customSymbols[_index];
                return _symbol.IsVisible;
            }

            void EnableSymbols()
            {
                foreach (var _symbol in customSymbols)
                {
                    if (_symbol.IsSelected)
                        EnableSymbol(_symbol, true);
                }
            }

            void DisableSymbols()
            {
                foreach (var _symbol in customSymbols)
                {
                    if (_symbol.IsSelected)
                        EnableSymbol(_symbol, false);
                }
            }
        }

        private void DrawConfigurationRightSide()
        {
            // Draw all currently active symbols.
            using (var _scroll = new GUILayout.ScrollViewScope(activeSymbolsScroll, GUILayout.Height(SectionHeight)))
            {
                activeSymbolsScroll = _scroll.scrollPosition;
                EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 4f);

                EnhancedEditorGUILayout.UnderlinedLabel(activeSymbolHeaderGUI, EditorStyles.boldLabel);
                GUILayout.Space(3f);

                DrawSymbols(activeSymbols);

                // Need to apply symbols message.
                if (doNeedToApplySymbols)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.HelpBox(NeedToApplySymbolsMessage, UnityEditor.MessageType.Warning);
                }
            }
        }

        private void DrawConfigurationBottom()
        {
            // Apply symbols button.
            using (var _scope = EnhancedGUI.GUIEnabled.Scope(doNeedToApplySymbols))
            using (EnhancedGUI.GUIColor.Scope(validColor))
            {
                if (GUILayout.Button(applySymbolsGUI, GUILayout.Width(ApplySymbolsButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    SetScriptingDefineSymbol();
                }
            }

            // Button to use selected symbols on selected preset.
            Rect _position = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);
            _position.Set(_position.xMax - UseSelectedSymbolsOnPresetButtonWidth,
                          _position.y - EditorGUIUtility.singleLineHeight - 5f,
                          UseSelectedSymbolsOnPresetButtonWidth,
                          LargeButtonHeight);

            bool _enabled = lastSelectedSymbol != -1;

            using (var _scope = EnhancedGUI.GUIEnabled.Scope(_enabled))
            using (EnhancedGUI.GUIColor.Scope(configurationColor))
            {
                if (GUI.Button(_position, useSelectedSymbolsGUI))
                {
                    UseSelectedSymbolsOnPreset();
                }
            }

            GUILayout.Space(5f);

            // Separator.
            EnhancedEditorGUILayout.HorizontalLine(separatorColor, GUILayout.Width(position.width * .5f), GUILayout.Height(2f));
            GUILayout.Space(5f);

            // All registered build presets.
            EnhancedEditorGUILayout.UnderlinedLabel(buildPresetsHeaderGUI, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            DrawBuildPresets(true);
        }

        // -----------------------

        private bool IsSymbolSelected(int _index)
        {
            ScriptingDefineSymbolInfo _symbol = customSymbols[_index];
            bool _isSelected = _symbol.IsSelected && _symbol.IsVisible;

            return _isSelected;
        }

        private void SelectSymbol(int _index, bool _isSelected)
        {
            ScriptingDefineSymbolInfo _symbol = customSymbols[_index];
            if (_isSelected && !_symbol.IsVisible)
                return;

            _symbol.IsSelected = _isSelected;

            // Last selected index update.
            if (_isSelected)
            {
                lastSelectedSymbol = _index;
                doFocusSymbol = true;

                GUIUtility.keyboardControl = customSymbolControlID;
            }
            else if (!Array.Exists(customSymbols, (s) => s.IsSelected))
            {
                lastSelectedSymbol = -1;
            }
        }

        // -----------------------

        private void InitializeConfiguration()
        {
            RefreshSymbols();
        }

        private void OnConfigurationUndoRedo()
        {
            UpdateNeedToApplySymbols();
        }

        private void RefreshSymbols()
        {
            // Get all custom define symbols.
            var _symbols = new List<ScriptingDefineSymbolAttribute>();

            #if UNITY_2019_2_OR_NEWER
            foreach (var _symbol in TypeCache.GetTypesWithAttribute<ScriptingDefineSymbolAttribute>())
            {
                _symbols.AddRange(_symbol.GetCustomAttributes(typeof(ScriptingDefineSymbolAttribute), true) as ScriptingDefineSymbolAttribute[]);
            }
            #else
            foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type _type in _assembly.GetTypes())
                {
                    var _typeSymbols = _type.GetCustomAttributes<ScriptingDefineSymbolAttribute>(true);
                    foreach (var _symbol in _typeSymbols)
                    {
                        _symbols.Add(_symbol);
                    }
                }
            }
            #endif

            customSymbols = Array.ConvertAll(_symbols.ToArray(), (s) => new ScriptingDefineSymbolInfo(s));
            Array.Sort(customSymbols, (a, b) => a.DefineSymbol.Symbol.CompareTo(b.DefineSymbol.Symbol));

            // Get all active symbols.
            activeSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(ScriptingDefineSymbolSeparator);

            // When no symbol is enabled, an empty string will be returned; ignore it.
            if ((activeSymbols.Length == 1) && string.IsNullOrEmpty(activeSymbols[0]))
            {
                activeSymbols = new string[] { };
                otherSymbols = new string[] { };
            }
            else
            {
                // Get "other" symbols, that is non custom active symbols.
                Array.Sort(activeSymbols);
                otherSymbols = activeSymbols;

                for (int _i = otherSymbols.Length; _i-- > 0;)
                {
                    int _index = Array.FindIndex(customSymbols, (s) => s.DefineSymbol.Symbol == otherSymbols[_i]);
                    if (_index > -1)
                    {
                        customSymbols[_index].IsEnabled = true;
                        ArrayUtility.RemoveAt(ref otherSymbols, _i);
                    }
                }
            }

            FilterCustomSymbols();
        }

        private void FilterCustomSymbols()
        {
            string _searchFilter = customSymbolsSearchFilter.ToLower();
            foreach (ScriptingDefineSymbolInfo _symbol in customSymbols)
            {
                bool _isVisible = _symbol.DefineSymbol.Symbol.ToLower().Contains(_searchFilter) || _symbol.DefineSymbol.Description.ToLower().Contains(_searchFilter);
                _symbol.IsVisible = _isVisible;
            }
        }

        private void EnableSymbol(ScriptingDefineSymbolInfo _symbol, bool _isEnabled)
        {
            if (_symbol.IsEnabled == _isEnabled)
                return;

            if (_isEnabled)
            {
                ArrayUtility.Add(ref activeSymbols, _symbol.DefineSymbol.Symbol);
                Array.Sort(activeSymbols);
            }
            else
            {
                ArrayUtility.Remove(ref activeSymbols, _symbol.DefineSymbol.Symbol);
            }

            _symbol.IsEnabled = _isEnabled;
            UpdateNeedToApplySymbols();
        }

        private void UpdateNeedToApplySymbols()
        {
            string _symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            doNeedToApplySymbols = _symbols != string.Join(ScriptingDefineSymbolSeparator.ToString(), activeSymbols);
        }

        private void UseSelectedSymbolsOnPreset()
        {
            BuildPreset _preset = buildPresets[selectedPreset];
            List<string> _symbols = new List<string>(_preset.ScriptingDefineSymbols.Split(ScriptingDefineSymbolSeparator));
            if ((_symbols.Count == 1) && string.IsNullOrEmpty(_symbols[0]))
            {
                _symbols.Clear();
            }

            foreach (var _symbol in customSymbols)
            {
                string _scriptingSymbol = _symbol.DefineSymbol.Symbol;
                if (_symbol.IsSelected)
                {
                    if (!_symbols.Contains(_scriptingSymbol))
                    {
                        _symbols.Add(_scriptingSymbol);
                    }
                }
                else if (_symbols.Contains(_scriptingSymbol))
                {
                    _symbols.Remove(_scriptingSymbol);
                }
            }

            _preset.ScriptingDefineSymbols = string.Join(ScriptingDefineSymbolSeparator.ToString(), _symbols);
        }

        private void SetScriptingDefineSymbol()
        {
            string _symbols = string.Join(ScriptingDefineSymbolSeparator.ToString(), activeSymbols);
            SetScriptingDefineSymbols(EditorUserBuildSettings.selectedBuildTargetGroup, _symbols);
        }

        private void SetScriptingDefineSymbols(BuildTargetGroup _targetGroup, string _symbols)
        {
            EditorUtility.DisplayProgressBar("Reloading Assemblies", "Reloading assemblies... This can take up to a few minutes.", 1f);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_targetGroup, _symbols);

            doNeedToApplySymbols = false;
            EditorUtility.ClearProgressBar();
        }
        #endregion

        #region Build Preset
        private readonly GUIContent buildPresetSymbolsGUI = new GUIContent("Preset Symbols:", "Active symbols in this build preset.");
        private readonly GUIContent savePresetGUI = new GUIContent("Save as...", "Save this preset.");
        private readonly GUIContent deletePresetGUI = new GUIContent("Delete", "Delete this preset.");

        private readonly int[] buildOptionsValues = Enum.GetValues(typeof(BuildOptions)) as int[];
        private string[] buildOptionsNames = null;

        [SerializeField] private int selectedPreset = 0;

        private BuildPreset[] buildPresets = new BuildPreset[] { };
        private GUIContent[] buildPresetsGUI = new GUIContent[] { };

        private bool areBuildOptionsUnfolded = false;

        // -----------------------

        private void DrawBuildPresets(bool _canEdit)
        {
            // Preset selection.
            GUIStyle _style = EnhancedEditorStyles.Button;
            float _width = 25f;

            foreach (var _content in buildPresetsGUI)
                _width += _style.CalcSize(_content).x;

            // Use a toolbar or a popup depending on the available space on screen.
            if (_width < position.width)
            {
                selectedPreset = EnhancedEditorGUILayout.CenteredToolbar(selectedPreset, buildPresetsGUI, GUILayout.Height(LargeButtonHeight));
            }
            else
            {
                GUIContent _label = buildPresetsGUI[selectedPreset];
                _width = _style.CalcSize(_label).x + 20f;

                selectedPreset = EnhancedEditorGUILayout.CenteredPopup(selectedPreset, buildPresetsGUI, GUILayout.Width(_width), GUILayout.Height(LargeButtonHeight));
            }

            GUILayout.Space(10f);

            BuildPreset _preset = buildPresets[selectedPreset];
            DrawBuildPreset(_preset, false, _canEdit);
        }

        private void DrawBuildPreset(BuildPreset _preset, bool _isFromBuild, bool _canEdit = false)
        {
            using (var _scope = new GUILayout.HorizontalScope())
            {
                using (var _verticalScope = new GUILayout.VerticalScope(GUILayout.Width(position.width * SectionWidthCoef)))
                using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(_preset, "Build Preset Change");

                    // Preset settings.
                    bool _isCustom = !_isFromBuild && (selectedPreset == 0);
                    _canEdit |= _isCustom;

                    if (_canEdit)
                    {
                        _preset.Description = EditorGUILayout.TextArea(_preset.Description, EnhancedEditorStyles.TextArea, GUILayout.MaxWidth(position.width * SectionWidthCoef));

                        GUILayout.Space(5f);

                        string _scriptingDefineSymbols = EditorGUILayout.DelayedTextField(scriptingSymbolsGUI, _preset.scriptingDefineSymbols, EnhancedEditorStyles.TextArea);
                        if (_scriptingDefineSymbols != _preset.scriptingDefineSymbols)
                        {
                            _preset.ScriptingDefineSymbols = _scriptingDefineSymbols;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(_preset.Description, EnhancedEditorStyles.WordWrappedRichText, GUILayout.MaxWidth(position.width * SectionWidthCoef));
                        GUILayout.Space(5f);
                    }

                    // Build target and options.
                    using (EnhancedGUI.GUIEnabled.Scope(_canEdit))
                    {
                        _preset.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(buildTargetGUI, _preset.BuildTarget);

                        // Options.
                        Rect _position = EditorGUILayout.GetControlRect();
                        _position.xMax -= 25f;

                        _preset.BuildOptions = (BuildOptions)EditorGUI.EnumFlagsField(_position, buildOptionsGUI, _preset.BuildOptions);

                        _position.xMin += _position.width + 10f;
                        _position.width = 15f;

                        // Draw each build option as a toggle.
                        areBuildOptionsUnfolded = EditorGUI.Foldout(_position, areBuildOptionsUnfolded, GUIContent.none);
                        if (areBuildOptionsUnfolded)
                        {
                            using (var _indent = new EditorGUI.IndentLevelScope())
                            {
                                for (int _i = 0; _i < buildOptionsValues.Length; _i++)
                                {
                                    int _value = buildOptionsValues[_i];
                                    if (_value == 0)
                                        continue;

                                    BuildOptions _option = (BuildOptions)_value;
                                    bool _wasEnabled = (_preset.BuildOptions & _option) == _option;
                                    bool _isEnabled = EditorGUILayout.ToggleLeft(buildOptionsNames[_i], _wasEnabled);

                                    if (_isEnabled != _wasEnabled)
                                    {
                                        if (_isEnabled)
                                        {
                                            _preset.BuildOptions |= _option;
                                        }
                                        else
                                        {
                                            _preset.BuildOptions &= ~_option;
                                        }
                                    }
                                }
                            }
                        }

                        // Save on any change.
                        if (!_isCustom && _changeCheck.changed)
                            SaveBuildPreset(_preset);
                    }

                    GUILayout.Space(15f);

                    // Save / delete preset buttons.
                    if (_isCustom)
                    {
                        using (EnhancedGUI.GUIColor.Scope(validColor))
                        {
                            if (GUILayout.Button(savePresetGUI, GUILayout.Width(75f), GUILayout.Height(ButtonHeight)))
                            {
                                string[] _presets = Array.ConvertAll(buildPresetsGUI, (b) => b.text);
                                CreateBuildPresetWindow.GetWindow();
                            }
                        }
                    }
                    else if (!_isFromBuild)
                    {
                        using (EnhancedGUI.GUIColor.Scope(warningColor))
                        {
                            if (GUILayout.Button(deletePresetGUI, GUILayout.Width(75f), GUILayout.Height(ButtonHeight))
                            && EditorUtility.DisplayDialog($"Delete \"{buildPresetsGUI[selectedPreset].text}\" preset",
                                                            "Are you sure you want to delete this build preset?\n" +
                                                            "This action cannot be undone.", "Yes", "Cancel"))
                            {
                                DeleteBuildPreset(_preset);
                            }
                        }
                    }
                }

                GUILayout.Space(5f);

                // Preset symbols.
                using (var _verticalScope = new GUILayout.VerticalScope())
                {
                    EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 4f);
                    EnhancedEditorGUILayout.UnderlinedLabel(buildPresetSymbolsGUI, EditorStyles.boldLabel);

                    GUILayout.Space(3f);
                    DrawSymbols(_preset.ScriptingDefineSymbols);
                }
            }
        }

        // -----------------------

        private void InitializeBuildPresets()
        {
            buildOptionsNames = Array.ConvertAll(buildOptionsValues, (v) =>
            {
                string _name = Enum.GetName(typeof(BuildOptions), v);
                return ObjectNames.NicifyVariableName(_name);
            });

            RefreshBuildPresets(true);
        }

        private void RefreshBuildPresets(bool _initializeCustomPreset = false)
        {
            buildPresets = PresetResources.Reload();
            Array.Sort(buildPresets);

            buildPresetsGUI = Array.ConvertAll(buildPresets, (p) => new GUIContent(p.name.Replace(PresetResources.Prefix, string.Empty)));

            RefreshCustomPreset(_initializeCustomPreset);
            selectedPreset = Mathf.Min(selectedPreset, buildPresets.Length - 1);

            GUIContent _custom = buildPresetsGUI[0];
            _custom.text = $"- {_custom.text} -";
        }

        private void RefreshCustomPreset(bool _initialize)
        {
            string _customPresetName = $"{PresetResources.Prefix}{PresetResources.DefaultAssetName}{PresetResources.Suffix}";
            BuildPreset _customPreset = Array.Find(buildPresets, (p) => p.name == _customPresetName);

            // Create the default preset if not found, and initialize it.
            if (_customPreset == null)
            {
                _customPreset = PresetResources.CreateResource(PresetResources.DefaultAssetName);
                AddPreset(_customPreset, 0);
            }
            else
            {
                int _index = Array.IndexOf(buildPresets, _customPreset);
                int _newIndex = 0;

                if (_index != _newIndex)
                {
                    buildPresets[_index] = buildPresets[_newIndex];
                    buildPresets[_newIndex] = _customPreset;

                    GUIContent _label = buildPresetsGUI[_index];

                    buildPresetsGUI[_index] = buildPresetsGUI[_newIndex];
                    buildPresetsGUI[_newIndex] = _label;
                }

                if (!_initialize)
                    return;
            }

            // Preset initialization.
            _customPreset.buildCount = 0;
            _customPreset.BuildOptions = 0;
            _customPreset.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            _customPreset.ScriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(_customPreset.BuildTarget));
        }

        private void CreateBuildPreset(string _name)
        {
            // Create the preset.
            BuildPreset _template = buildPresets[0];
            BuildPreset _preset = CreateInstance<BuildPreset>();

            _preset.Initialize(_template);
            _preset = PresetResources.CreateResource(_name, _preset);

            // Add it to the list and refresh it.
            AddPreset(_preset);
            Array.Sort(buildPresets, buildPresetsGUI, 1, buildPresets.Length - 1);

            selectedPreset = Array.IndexOf(buildPresets, _preset);
        }

        private void AddPreset(BuildPreset _preset, int _index = 1)
        {
            ArrayUtility.Insert(ref buildPresets, _index, _preset);
            ArrayUtility.Insert(ref buildPresetsGUI, _index, new GUIContent(_preset.name.Replace(PresetResources.Prefix, string.Empty)));
        }

        private void SaveBuildPreset(BuildPreset _preset)
        {
            EditorUtility.SetDirty(_preset);
        }

        private void DeleteBuildPreset(BuildPreset _preset)
        {
            string _path = AssetDatabase.GetAssetPath(_preset);
            if (!string.IsNullOrEmpty(_path))
            {
                AssetDatabase.DeleteAsset(_path);
                AssetDatabase.Refresh();

                ArrayUtility.RemoveAt(ref buildPresets, selectedPreset);
                ArrayUtility.RemoveAt(ref buildPresetsGUI, selectedPreset);

                selectedPreset = 0;
            }
        }
        #endregion

        #region Draw Utility
        private const string BuildDirectoryPanelTitle = "Build Directory";

        // -----------------------

        private void DrawTab(Action _onDrawHeader, Action _onDrawSectionToolbar, Action _onDrawSection, Action<Rect> _onSectionEvent,
                             Action _onDrawRightSide, Action _onDrawBottom, ref Vector2 _sectionScroll)
        {
            // Header.
            _onDrawHeader();

            using (var _globalScope = new GUILayout.HorizontalScope())
            {
                using (var _sectionScope = new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * SectionWidthCoef), GUILayout.Height(SectionHeight)))
                {
                    // Section background.
                    Rect _position = _sectionScope.rect;
                    DrawSectionBackground(_position);

                    // Section toolbar.
                    using (var _scope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        // Draw an empty button all over the toolbar to draw its bounds.
                        {
                            Rect _toolbarPosition = _scope.rect;
                            _toolbarPosition.xMin += 1f;

                            GUI.Label(_toolbarPosition, GUIContent.none, EditorStyles.toolbarButton);
                        }

                        _onDrawSectionToolbar();
                    }

                    // Section content.
                    using (var _scroll = new GUILayout.ScrollViewScope(_sectionScroll))
                    {
                        _sectionScroll = _scroll.scrollPosition;
                        _onDrawSection();
                    }

                    // Events.
                    _onSectionEvent(_position);
                }

                GUILayout.Space(10f);

                // Right side.
                using (var _scope = new GUILayout.VerticalScope(GUILayout.Height(SectionHeight)))
                {
                    _onDrawRightSide();
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(5f);

            // Bottom.
            _onDrawBottom();
        }

        private void DrawSectionBackground(Rect _position)
        {
            EditorGUI.DrawRect(_position, sectionColor);

            _position.y -= 1f;
            _position.height += 2f;

            GUI.Label(_position, GUIContent.none, EditorStyles.helpBox);
        }

        private void DrawBuildDirectory(GUIContent _header)
        {
            EnhancedEditorGUILayout.UnderlinedLabel(_header, EditorStyles.boldLabel);
            GUILayout.Space(3f);

            EnhancedEditorSettings _settings = EnhancedEditorSettings.Settings;
            string _directory = EnhancedEditorGUILayout.FolderField(GUIContent.none, _settings.UserSettings.BuildDirectory, true, BuildDirectoryPanelTitle);

            if (_directory != _settings.UserSettings.BuildDirectory)
            {
                _settings.UserSettings.BuildDirectory = _directory;
                _settings.SaveSettings();

                RefreshBuilds();
            }
        }

        private Rect GetSectionElementPosition()
        {
            Rect _position = EditorGUILayout.GetControlRect();
            _position.xMin -= 2f;
            _position.xMax += 2f;
            _position.height += 2f;

            return _position;
        }

        // -----------------------

        private void DrawSymbols(string _symbols)
        {
            string[] _splitSymbols = _symbols.Split(ScriptingDefineSymbolSeparator);
            DrawSymbols(_splitSymbols);
        }

        private void DrawSymbols(string[] _symbols)
        {
            string _allSymbols = string.Empty;
            foreach (string _symbol in _symbols)
            {
                if (string.IsNullOrEmpty(_symbol))
                    continue;

                string _color = Array.Exists(customSymbols, (s) => s.DefineSymbol.Symbol == _symbol)
                                ? "green"
                                : "teal";

                _allSymbols += $"<b><color={_color}>{_symbol}</color></b> ; ";
            }

            if (!string.IsNullOrEmpty(_allSymbols))
            {
                EditorGUILayout.LabelField(_allSymbols, EnhancedEditorStyles.WordWrappedRichText);
            }
        }
        #endregion

        #region Build Utility
        public static void LaunchBuild(string _path, int _amount = 1)
        {
            if (File.Exists(_path))
            {
                for (int _i = 0; _i < _amount; _i++)
                    Process.Start(_path);
            }
            else
            {
                Debug.LogError($"Specified build does not exist at path: \"{_path}\"");
            }
        }

        public static string[] GetBuilds()
        {
            if (!Directory.Exists(BuildDirectory))
            {
                Directory.CreateDirectory(BuildDirectory);
                return new string[] { };
            }

            List<string> _builds = new List<string>();
            string[] _executables = Directory.GetFiles(BuildDirectory, "*.exe", SearchOption.AllDirectories);

            foreach (string _file in _executables)
            {
                if (Path.GetFileNameWithoutExtension(_file) == Application.productName)
                    _builds.Add(_file);
            }

            return _builds.ToArray();
        }

        public static string GetBuildIcon(string _buildName, GUIContent _content)
        {
            switch (_buildName)
            {
                case string _ when _buildName.Contains("Android"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Android.Small");
                    return "Android";

                case string _ when _buildName.Contains("Facebook"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Facebook.Small");
                    return "Facebook";

                case string _ when _buildName.Contains("Flash"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Flash.Small");
                    return "Flash";

                case string _ when _buildName.Contains("iPhone"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.iPhone.Small");
                    return "iPhone";

                case string _ when _buildName.Contains("Lumin"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Lumin.Small");
                    return "Lumin";

                case string _ when _buildName.Contains("N3DS"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.N3DS.Small");
                    return "N3DS";

                case string _ when _buildName.Contains("PS4"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.PS4.Small");
                    return "PS4";

                case string _ when _buildName.Contains("PS5"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.PS5.Small");
                    return "PS5";

                case string _ when _buildName.Contains("Stadia"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Stadia.Small");
                    return "Stadia";

                case string _ when _buildName.Contains("Switch"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Switch.Small");
                    return "Switch";

                case string _ when _buildName.Contains("tvOS"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.tvOS.Small");
                    return "tvOS";

                case string _ when _buildName.Contains("WebGL"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.WebGL.Small");
                    return "WebGL";

                case string _ when _buildName.Contains("Windows"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Metro.Small");
                    return "Windows";

                case string _ when _buildName.Contains("Xbox360"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Xbox360ebGL.Small");
                    return "Xbox360";

                case string _ when _buildName.Contains("XboxOne"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.XboxOne.Small");
                    return "XboxOne";

                case string _ when _buildName.Contains("Standalone"):
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Standalone.Small");
                    return "Standalone";

                default:
                    _content.image = EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");
                    return "Unknown";
            }
        }
        #endregion

        #region Create Build Preset Window
        /// <summary>
        /// Utility window used to create a new build preset.
        /// </summary>
        public class CreateBuildPresetWindow : EditorWindow
        {
            /// <summary>
            /// Creates and shows a new <see cref="CreateBuildPresetWindow"/> instance,
            /// used to create a new build preset in the project.
            /// </summary>
            /// <returns><see cref="CreateBuildPresetWindow"/> instance on screen.</returns>
            public static CreateBuildPresetWindow GetWindow()
            {
                CreateBuildPresetWindow _window = GetWindow<CreateBuildPresetWindow>(true, "Create Build Preset", true);

                _window.minSize = _window.maxSize
                                = new Vector2(300f, 70f);

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const string UndoRecordTitle = "New Build Preset Name Changes";

            private const string EmptyNameMessage = "A Preset name cannot be null or empty!";
            private const string ExistingPresetMessage = "A similar Preset with this name already exist.";

            private readonly GUIContent presetNameGUI = new GUIContent("Preset Name", "Name of this build preset.");
            private readonly GUIContent createPresetGUI = new GUIContent("OK", "Create this build preset.");

            [SerializeField] private string presetName = "NewPreset";

            // -----------------------

            private void OnGUI()
            {
                Undo.RecordObject(this, UndoRecordTitle);

                // Preset name.
                Rect _position = new Rect(5f, 5f, 40f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, presetNameGUI);

                _position.x += 50f;
                _position.width = position.width - _position.x - 5f;

                presetName = EditorGUI.TextField(_position, presetName);

                string _value = presetName.Trim();
                GUILayout.Space(3f);

                // Invalid name message.
                if (string.IsNullOrEmpty(_value))
                {
                    DrawHelpBox(EmptyNameMessage, UnityEditor.MessageType.Error, position.width - 10f);
                    return;
                }

                // A preset with the same name already exist.
                // This is no problem, as the new preset will be renamed to not override the existing one.
                if (PresetResources.GetResource(presetName, out _))
                {
                    DrawHelpBox(ExistingPresetMessage, UnityEditor.MessageType.Info, position.width - 65f);
                }

                _position = new Rect()
                {
                    x = position.width - 55f,
                    y = _position.y + _position.height + 10f,
                    width = 50f,
                    height = 25f
                };

                // Create preset button.
                if (GUI.Button(_position, createPresetGUI))
                {
                    BuildPipelineWindow.GetWindow().CreateBuildPreset(presetName);
                    Close();
                }

                // ----- Local Method ----- \\

                void DrawHelpBox(string _message, UnityEditor.MessageType _messageType, float _width)
                {
                    Rect _temp = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, _messageType, _width),
                        width = _width
                    };

                    EditorGUI.HelpBox(_temp, _message, _messageType);
                }
            }
        }
        #endregion
    }
}
