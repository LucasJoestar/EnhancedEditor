// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor window focused on build pipeline used to build with additional parameters,
    /// launch existing builds, and manage custom scripting define symbols.
    /// </summary>
    public class BuildPipeline : EditorWindow
    {
        #region Build Info
        private class BuildInfo
        {
            public readonly string Name = string.Empty;
            public readonly string Path = string.Empty;
            public readonly string Platform = string.Empty;
            public readonly GUIContent Icon = null;

            public BuildInfo(string _path)
            {
                Name = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(_path));
                Path = _path;

                Platform = GetBuildIcon(Name, out Icon);
            }
        }
        #endregion

        #region Build Scene
        private class BuildScene
        {
            public readonly SceneAsset Scene = null;
            public bool IsSelected = false;

            public BuildScene(SceneAsset _scene)
            {
                Scene = _scene;
                IsSelected = false;
            }
        }
        #endregion

        #region Scripting Define Symbol Info
        private class ScriptingDefineSymbolInfo
        {
            public ScriptingDefineSymbolAttribute DefineSymbol = null;
            public bool IsEnabled = false;
            public bool IsSelected = false;

            public ScriptingDefineSymbolInfo(ScriptingDefineSymbolAttribute _symbol)
            {
                DefineSymbol = _symbol;
                IsEnabled = false;
                IsSelected = false;
            }
        }
        #endregion

        #region Settings
        private static readonly AutoManagedResource<BuildPipelineSettings> settingsResource = new AutoManagedResource<BuildPipelineSettings>(true);

        /// <summary>
        /// Build Pipeline related <see cref="ScriptableObject"/> settings.
        /// </summary>
        public static BuildPipelineSettings Settings => settingsResource.GetResource();
        #endregion

        #region Menus
        /// <summary>
        /// Returns the first <see cref="BuildPipeline"/> currently on screen.
        /// <para/>
        /// Creates and shows a new instance if there is none.
        /// </summary>
        [MenuItem("Enhanced Editor/Build/Build Pipeline", false, 30)]
        public static BuildPipeline GetWindow()
        {
            BuildPipeline _window = GetWindow<BuildPipeline>("Build Pipeline");
            _window.Show();

            return _window;
        }

        /// <summary>
        /// Launches last created game build.
        /// </summary>
        [MenuItem("Enhanced Editor/Build/Launch Last Build", false, 31)]
        public static void LaunchLastBuild()
        {
            string[] _builds = GetBuilds();
            if (_builds.Length > 0)
            {
                Array.Sort(_builds, (a, b) =>
                {
                    return Directory.GetCreationTime(b).CompareTo(Directory.GetCreationTime(a));
                });

                LaunchBuild(_builds[0]);
            }
            else
            {
                EditorUtility.DisplayDialog("No build",
                                            $"No build could be found in the directory: \"{Settings.BuildDirectory}\".\n\n" +
                                            "Make sure you have selected a valid folder in the BuildPipeline window.",
                                            "OK");
            }
        }
        #endregion

        #region Build Pipeline Window
        private const float SectionWidthCoef = .6f;
        private const float SectionHeight = 400f;

        private const string SelectBuildDirectoryWindowTitle = "Build Directory";

        private readonly GUIContent buildPresetSymbolsGUI = new GUIContent("Preset Symbols:", "Symbols included in this build preset.");
        private readonly GUIContent setBuildDirectoryGUI = new GUIContent("...", "Select build directory.");
        private readonly GUIContent[] buildPipelineTabs = new GUIContent[]
                                                                {
                                                                    new GUIContent("Game Builder", "Create a new game build."),
                                                                    new GUIContent("Launcher", "Launch an existing game build."),
                                                                    new GUIContent("Configuration", "Configure game and build settings.")
                                                                };

        private readonly GUIContent savePresetGUI = new GUIContent("Save as...", "Save this preset.");
        private readonly GUIContent deletePresetGUI = new GUIContent("Delete", "Delete this preset.");

        private readonly Color oddColor = new Color(.25f, .25f, .25f);
        private readonly Color selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private readonly Color sectionColor = SuperColor.DarkGrey.Get();

        private readonly Color validColor = SuperColor.Lime.Get();
        private readonly Color warningColor = SuperColor.Crimson.Get();
        private readonly Color separatorColor = SuperColor.Grey.Get();

        private readonly AutoManagedResource<BuildPreset> presetResources = new AutoManagedResource<BuildPreset>("Custom", "BP_", string.Empty);

        private BuildPreset[] buildPresets;
        private GUIContent[] buildPresetsGUI = new GUIContent[] { };
        private int selectedBuildPreset = 0;

        private Vector2 scroll = new Vector2();
        private int selectedTab = 0;

        // -----------------------

        private void OnEnable()
        {
            RefreshBuildPresets();

            InitializeBuilder();
            InitializeLauncher();
            InitializeConfiguration();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            selectedTab = GUILayout.Toolbar(selectedTab, buildPipelineTabs, GUILayout.Height(25f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical();

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

            EditorGUILayout.EndVertical();
            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Builder
        private const float ProjectScenesSectionHeight = 200f;

        private readonly GUIContent buildScenesHeaderGUI = new GUIContent("Build Scenes:", "Included scenes in build.");
        private readonly GUIContent nonBuildScenesHeaderGUI = new GUIContent("Non Included Scenes:", "Non included scenes in build.");

        private readonly GUIContent refreshNonBuildScenesGUI = new GUIContent("Refresh", "Refresh non build scenes.");
        private readonly GUIContent addScenesToBuildGUI = new GUIContent("Add", "Add selected scene(s) to build.");
        private readonly GUIContent removeScenesFromBuildGUI = new GUIContent("Remove", "Remove selected scene(s) from build.");

        private readonly GUIContent buildTargetGUI = new GUIContent("Build Target", "Platform to build for.");
        private readonly GUIContent buildOptionsGUI = new GUIContent("Build Options", "Build specific options.");
        private readonly GUIContent scriptingSymbolsGUI = new GUIContent("Scripting Symbols", "Build scripting define symbols.");

        
        private readonly GUIContent builderDirectoryGUI = new GUIContent("Build in Directory:", "Directory in which to build.");
        private readonly GUIContent buildIdentifierGUI = new GUIContent("Build Identifier:", "Identifier of the build to create.");
        private readonly GUIContent buildPresetGUI = new GUIContent("Build Preset:", "Selected preset to build the game.");
        private readonly GUIContent buildButtonGUI = new GUIContent("BUILD", "Build with the selected settings.");

        private BuildScene[] buildScenes = new BuildScene[] { };
        private BuildScene[] nonBuildScenes = new BuildScene[] { };

        private ReorderableList buildScenesList = null;

        private bool canAddScene = false;
        private bool canRemoveScene = false;

        private Vector2 buildScenesScroll = new Vector2();
        private Vector2 projectScenesScroll = new Vector2();
        private int selectedNonBuildScene = -1;
        private string buildIdentifier = "v000";

        // -----------------------

        public void DrawBuilder()
        {
            DrawSection(DrawBuilderHeader,
                        buildScenesList.DoLayoutList,
                        DrawBuilderRightSide,
                        DrawBuilderBottom,
                        ref buildScenesScroll);
        }

        public void DrawBuilderHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(buildScenesHeaderGUI, EditorStyles.boldLabel, GUILayout.Width((Screen.width * SectionWidthCoef) + 8f));
            EditorGUILayout.LabelField(nonBuildScenesHeaderGUI, EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuildScene(Rect _rect, int _index, bool _isActive, bool _isFocused)
        {
            BuildScene _scene = buildScenes[_index];

            // Scene build index.
            Rect _indexRect = new Rect(_rect.x + 5f, _rect.y, 20f, _rect.height);
            EditorGUI.LabelField(_indexRect, _index.ToString(), EditorStyles.boldLabel);

            // Scene name.
            _rect.xMin += 25f;
            _rect.width -= 25f;
            EditorGUI.LabelField(_rect, _scene.Scene.name);

            // Selection toggle.
            _rect.x += _rect.width;
            _rect.width = 25f;

            bool _selected = GUI.Toggle(_rect, _scene.IsSelected, GUIContent.none);
            if (_selected != _scene.IsSelected)
            {
                _scene.IsSelected = _selected;
                canRemoveScene = Array.Find(buildScenes, (s) => s.IsSelected) != null;
            }
        }

        private void DrawBuildSceneBackground(Rect _rect, int _index, bool _isActive, bool _isFocused)
        {
            if (_isFocused)
            {
                EditorGUI.DrawRect(_rect, selectedColor);
            }
            else if ((_index % 2) == 0)
                EditorGUI.DrawRect(_rect, oddColor);
        }

        private void DrawBuilderRightSide()
        {
            // Non included build scenes.
            Rect _rect = EditorGUILayout.BeginVertical(GUILayout.Height(ProjectScenesSectionHeight));
            DrawSectionBackground(_rect);

            projectScenesScroll = EditorGUILayout.BeginScrollView(projectScenesScroll);
            DrawNonBuildScenes();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();

            // Non build scenes refresh button.
            if (GUILayout.Button(refreshNonBuildScenesGUI, GUILayout.Width(75f), GUILayout.Height(20f)))
                RefreshNonBuildScenes();

            GUILayout.FlexibleSpace();

            // Button to add scene(s) to build.
            EnhancedEditorGUIUtility.PushEnable(canAddScene);
            EnhancedEditorGUIUtility.PushGUIColor(validColor);

            if (GUILayout.Button(addScenesToBuildGUI, GUILayout.Width(60f), GUILayout.Height(20f)))
            {
                for (int _i = nonBuildScenes.Length; _i-- > 0;)
                {
                    BuildScene _scene = nonBuildScenes[_i];
                    if (_scene.IsSelected)
                    {
                        _scene.IsSelected = false;
                        UnityEditor.ArrayUtility.Add(ref buildScenes, _scene);
                        UnityEditor.ArrayUtility.RemoveAt(ref nonBuildScenes, _i);

                        
                    }
                }

                canAddScene = false;

                UpdateBuildScenes();
                RefreshBuildList();
            }

            EnhancedEditorGUIUtility.PopGUIColor();
            EnhancedEditorGUIUtility.PopEnable();
            EditorGUILayout.EndHorizontal();

            // Build Maker.
            GUILayout.FlexibleSpace();
            DrawBuildDirectory(builderDirectoryGUI);

            EditorGUILayout.BeginHorizontal();

            float _labelWidth = EditorStyles.label.CalcSize(buildIdentifierGUI).x + 5f;
            EditorGUILayout.LabelField(buildIdentifierGUI, GUILayout.Width(_labelWidth));

            float _width = (position.width * (1f - SectionWidthCoef)) - _labelWidth - 30f;
            buildIdentifier = EditorGUILayout.TextField(buildIdentifier, GUILayout.Width(_width));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNonBuildScenes()
        {
            for (int _i = 0; _i < nonBuildScenes.Length; _i++)
            {
                BuildScene _scene = nonBuildScenes[_i];

                Rect _rect = EditorGUILayout.GetControlRect();
                _rect.x -= 2f;
                _rect.width += 4f;

                // Background color.
                if (selectedNonBuildScene == _i)
                {
                    EditorGUI.DrawRect(_rect, selectedColor);
                }
                else if ((_i % 2) == 0)
                    EditorGUI.DrawRect(_rect, oddColor);

                // Scene highlight selection.
                _rect.xMin += 5f;
                _rect.width -= 25f;
                if (GUI.Button(_rect, _scene.Scene.name, EditorStyles.label))
                    selectedNonBuildScene = _i;

                // Selection toggle.
                _rect.x += _rect.width;
                _rect.width = 25f;

                bool _selected = GUI.Toggle(_rect, _scene.IsSelected, GUIContent.none);
                if (_selected != _scene.IsSelected)
                {
                    _scene.IsSelected = _selected;
                    canAddScene = Array.Find(nonBuildScenes, (s) => s.IsSelected) != null;
                }
            }

            // Unselect on empty space click.
            Event _event = Event.current;
            if (_event.type == EventType.MouseDown)
            {
                selectedNonBuildScene = -1;
                _event.Use();
            }
        }

        private void DrawBuilderBottom()
        {
            // Button to remove scene(s) from build.
            EnhancedEditorGUIUtility.PushEnable(canRemoveScene);
            EnhancedEditorGUIUtility.PushGUIColor(warningColor);

            if (GUILayout.Button(removeScenesFromBuildGUI, GUILayout.Width(75f), GUILayout.Height(20f)))
            {
                for (int _i = buildScenes.Length; _i-- > 0;)
                {
                    BuildScene _scene = buildScenes[_i];
                    if (_scene.IsSelected)
                    {
                        ArrayUtility.Add(ref nonBuildScenes, _scene);
                        ArrayUtility.RemoveAt(ref buildScenes, _i);

                        _scene.IsSelected = false;
                    }
                }

                canRemoveScene = false;

                UpdateBuildScenes();
                RefreshBuildList();
                SortNonBuildScenes();
            }

            EnhancedEditorGUIUtility.PopGUIColor();
            EnhancedEditorGUIUtility.PopEnable();

            // Build button.
            Rect _position = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);
            _position.Set(_position.xMax - 100f,
                      _position.y - EditorGUIUtility.singleLineHeight - 5f,
                      100f,
                      25f); 

            EnhancedEditorGUIUtility.PushGUIColor(validColor);
            if (GUI.Button(_position, buildButtonGUI))
                Build();

            EnhancedEditorGUIUtility.PopGUIColor();
            EditorGUILayout.Space(5f);

            // Separator.
            _position = EditorGUILayout.GetControlRect(false, 2f);
            _position.width *= .55f;

            EnhancedEditorGUI.HorizontalLine(_position, 1f, separatorColor);
            EditorGUILayout.Space(5f);

            // Selected build preset.
            EnhancedEditorGUILayout.UnderlinedLabel(buildPresetGUI, EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);

            DrawBuildPresets(false);
        }

        // -----------------------

        private void InitializeBuilder()
        {
            RefreshBuildList();
            RefreshNonBuildScenes();
        }

        private void RefreshBuildList()
        {
            buildScenes = Array.ConvertAll(EditorBuildSettings.scenes, (s) =>
            {
                return new BuildScene(AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path));
            });

            buildScenesList = new ReorderableList(buildScenes, typeof(BuildScene), true, false, false, false)
            {
                drawElementCallback = DrawBuildScene,
                drawElementBackgroundCallback = DrawBuildSceneBackground,
                showDefaultBackground = false,

                onReorderCallback = (r) => UpdateBuildScenes(),

                headerHeight = 1f,
                elementHeight = EditorGUIUtility.singleLineHeight
            };
        }

        private void RefreshNonBuildScenes()
        {
            nonBuildScenes = Array.ConvertAll(EnhancedEditorUtility.LoadAssets<SceneAsset>(), (s) => new BuildScene(s));
            nonBuildScenes = nonBuildScenes.Where(s => !buildScenes.Any(b => b.Scene == s.Scene)).ToArray();

            SortNonBuildScenes();
        }

        private void UpdateBuildScenes()
        {
            EditorBuildSettings.scenes = Array.ConvertAll(buildScenes, (s) =>
            {
                return new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(s.Scene), true);
            });
        }

        private void SortNonBuildScenes()
        {
            Array.Sort(nonBuildScenes, (a, b) => a.Scene.name.CompareTo(b.Scene.name));
        }

        private void Build()
        {
            BuildPreset _preset = buildPresets[selectedBuildPreset];

            // Get application short name.
            string _appName = string.Empty;
            foreach (char _char in Application.productName)
            {
                string _string = _char.ToString();
                if (!string.IsNullOrEmpty(_string) && (_string == _string.ToUpper()) && (_string != " "))
                    _appName += _string;
            }

            if (string.IsNullOrEmpty(_appName))
                _appName = Application.productName;

            // Set build directory name.
            string _buildPath = Path.Combine(Settings.BuildDirectory,
                                             $"{_appName}_{_preset.BuildAmount:000}" +
                                             $"_{_preset.name.Replace(presetResources.Prefix, string.Empty)}" +
                                             $"_{_preset.BuildTarget}_{buildIdentifier}");

            // Delete path before build to avoid conflicts or corrupted files.
            if (Directory.Exists(_buildPath))
                Directory.Delete(_buildPath, true);

            Directory.CreateDirectory(_buildPath);
            BuildPlayerOptions _options = new BuildPlayerOptions()
            {
                scenes = Array.ConvertAll(buildScenes, (s) => AssetDatabase.GetAssetPath(s.Scene)),
                locationPathName = $"{Path.Combine(_buildPath, Application.productName)}.exe",

                targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(_preset.BuildTarget),
                target = _preset.BuildTarget,
                options = _preset.BuildOptions
            };
            
            string _symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_options.targetGroup);
            SetScriptingDefineSymbols(_options.targetGroup, _preset.ScriptingDefineSymbols);

            if (UnityEditor.BuildPipeline.BuildPlayer(_options).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                _preset.BuildAmount++;
                SaveBuildPreset(_preset);

                Process.Start(_buildPath);
            }

            SetScriptingDefineSymbols(_options.targetGroup, _symbols);
        }
        #endregion

        #region Launcher
        private const string SelectedBuildHelpBox = "Selected build infos will be displayed here.";

        private readonly GUIContent launcherHeaderGUI = new GUIContent("Builds:", "All builds in selected directory.");
        private readonly GUIContent buildDirectoryGUI = new GUIContent("Build Directory:", "Directory where to load builds.");
        private readonly GUIContent buildInfoGUI = new GUIContent("Build Infos:", "Infos on the selected build.");

        private readonly GUIContent sortAscendingGUI = new GUIContent("↑", "Sort builds in ascending order.");
        private readonly GUIContent sortDescendingGUI = new GUIContent("↓", "Sort builds in descending order.");

        private readonly GUIContent refreshBuildsGUI = new GUIContent("Refresh", "Refresh builds in directory.");
        private readonly GUIContent[] buildSortOptionsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Sort by date", "Sort builds by creation date."),
                                                                new GUIContent("Sort by name", "Sort builds by name."),
                                                                new GUIContent("Sort by platform", "Sort builds by platform.")
                                                            };

        private readonly GUIContent launchAmountGUI = new GUIContent("Launch Amount:", "Amount of selected build instance to launch.");
        private readonly GUIContent launchBuildGUI = new GUIContent("LAUNCH", "Launch selected build.");

        private readonly Color launcherColor = SuperColor.Aquamarine.Get();

        private BuildInfo[] builds = new BuildInfo[] { };
        private BuildInfo[] filteredBuilds = new BuildInfo[] { };
        private int selectedLaunchBuildPreset = -1;

        private Vector2 buildsScroll = new Vector2();

        private bool doSortAscending = true;
        private int selectedBuildSortOption = 0;
        private string searchPattern = string.Empty;

        private int selectedBuild = -1;
        private int launchBuildAmount = 1;

        // -----------------------

        private void DrawLauncher()
        {
            DrawSection(DrawLauncherHeader,
                        DrawBuilds,
                        DrawBuildInfos,
                        DrawLauncherBottom,
                        ref buildsScroll);
        }

        private void DrawLauncherHeader()
        {
            EditorGUILayout.LabelField(launcherHeaderGUI, EditorStyles.boldLabel);
        }

        private void DrawBuilds()
        {
            // Toolbar.
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            int _option = EditorGUILayout.Popup(selectedBuildSortOption, buildSortOptionsGUI, EditorStyles.toolbarDropDown, GUILayout.Width(110f));
            if (_option != selectedBuildSortOption)
            {
                selectedBuildSortOption = _option;
                SortBuilds();
            }

            if (GUILayout.Button(doSortAscending ? sortAscendingGUI : sortDescendingGUI, EditorStyles.toolbarButton, GUILayout.Width(20f)))
            {
                doSortAscending = !doSortAscending;
                SortBuilds();
            }

            string _searchPattern = EnhancedEditorGUILayout.ToolbarSearchField(searchPattern);
            if (_searchPattern != searchPattern)
            {
                searchPattern = _searchPattern;
                FilterBuilds();
            }

            if (GUILayout.Button(refreshBuildsGUI, EditorStyles.toolbarButton, GUILayout.Width(55f)))
                RefreshBuilds();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10f);

            // Filtered Builds.
            for (int _i = 0; _i < filteredBuilds.Length; _i++)
            {
                BuildInfo _buildInfo = filteredBuilds[_i];

                Rect _rect = EditorGUILayout.GetControlRect();
                _rect.x -= 2f;
                _rect.width += 4f;

                // Background color.
                if (selectedBuild == _i)
                {
                    EditorGUI.DrawRect(_rect, selectedColor);
                }
                else if ((_i % 2) == 0)
                    EditorGUI.DrawRect(_rect, oddColor);

                // Build selection.
                if (GUI.Button(_rect, GUIContent.none, EditorStyles.label))
                {
                    selectedBuild = _i;
                    selectedLaunchBuildPreset = UnityEditor.ArrayUtility.FindIndex(buildPresets, (p) =>
                    {
                        return filteredBuilds[selectedBuild].Name.Contains(p.name.Replace(presetResources.Prefix, string.Empty));
                    });
                }
                
                // Build infos.
                _rect.xMin += 5f;
                EditorGUI.LabelField(_rect, _buildInfo.Icon);

                _rect.xMin += 25f;
                EditorGUI.LabelField(_rect, _buildInfo.Name);
            }

            // Unselect on empty space click.
            Event _event = Event.current;
            if (_event.type == EventType.MouseDown)
            {
                selectedBuild = -1;
                selectedLaunchBuildPreset = -1;

                _event.Use();
            }
        }

        private void DrawBuildInfos()
        {
            EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 3f);

            // Build directory.
            DrawBuildDirectory(buildDirectoryGUI);
            EditorGUILayout.Space(10f);

            // Build informations.
            EnhancedEditorGUILayout.UnderlinedLabel(buildInfoGUI, EditorStyles.boldLabel);

            if ((selectedBuild < filteredBuilds.Length) && (selectedBuild > -1))
            {
                BuildInfo _build = filteredBuilds[selectedBuild];
                GUIStyle _style = EnhancedEditorStyles.WordWrappedRichText;

                EditorGUILayout.LabelField($"Name:   <b><color=green>{ _build.Name}</color></b>", _style);
                EditorGUILayout.LabelField($"Platform:   <b><color=teal>{_build.Platform}</color></b>", _style);
                EditorGUILayout.LabelField($"Creation Date:   <b><color=orange>{Directory.GetCreationTimeUtc(_build.Path)}</color></b>", _style);

                EditorGUILayout.Space(10f);
                EnhancedEditorGUIUtility.PushGUIColor(validColor);

                if (GUILayout.Button("Open Folder", GUILayout.Width(100f), GUILayout.Height(25f)))
                    Process.Start(Path.GetDirectoryName(_build.Path));

                EnhancedEditorGUIUtility.PopGUIColor();

                // Draw associated build preset description if found.
                if (selectedLaunchBuildPreset > - 1)
                {
                    EditorGUILayout.Space(25f);
                    EnhancedEditorGUILayout.UnderlinedLabel(buildPresetGUI, EditorStyles.boldLabel);

                    BuildPreset _preset = buildPresets[selectedLaunchBuildPreset];
                    EditorGUILayout.LabelField(_preset.Description, EnhancedEditorStyles.BoldWordWrappedRichText);
                }
            }
            else
            {
                EditorGUILayout.Space(3f);
                EditorGUILayout.HelpBox(SelectedBuildHelpBox, UnityEditor.MessageType.Info);
            }
        }

        private void DrawLauncherBottom()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(Screen.width * SectionWidthCoef));

            bool _canLaunch = (selectedBuild < filteredBuilds.Length) && (selectedBuild > -1);
            EnhancedEditorGUIUtility.PushEnable(_canLaunch);
            EnhancedEditorGUIUtility.PushGUIColor(launcherColor);

            // Launch button.
            if (GUILayout.Button(launchBuildGUI, GUILayout.Width(100f), GUILayout.Height(25f)))
                LaunchBuild(filteredBuilds[selectedBuild].Path, launchBuildAmount);

            EnhancedEditorGUIUtility.PopGUIColor();
            EnhancedEditorGUIUtility.PopEnable();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();

            // Launch instance amount.
            EditorGUILayout.LabelField(launchAmountGUI, GUILayout.Width(100f));

            int _launchAmount = EditorGUILayout.IntField(launchBuildAmount, GUILayout.Width(50f));
            if (_launchAmount != launchBuildAmount)
                launchBuildAmount = Mathf.Clamp(_launchAmount, 1, 10);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();            

            EditorGUILayout.EndHorizontal();

            // Draw associated build preset symbols if found.
            if (selectedLaunchBuildPreset > -1)
            {
                EditorGUILayout.Space(25f);
                EnhancedEditorGUILayout.UnderlinedLabel(buildPresetSymbolsGUI, EditorStyles.boldLabel);

                BuildPreset _preset = buildPresets[selectedLaunchBuildPreset];
                DrawSymbols(_preset.ScriptingDefineSymbols);
            }
        }

        // -----------------------

        private void InitializeLauncher()
        {
            RefreshBuilds();
        }

        private void RefreshBuilds()
        {
            string[] _builds = GetBuilds();
            builds = Array.ConvertAll(_builds, (b) => new BuildInfo(b));

            FilterBuilds();
        }

        private void FilterBuilds()
        {
            if (!string.IsNullOrEmpty(searchPattern))
            {
                List<BuildInfo> _filteredBuilds = new List<BuildInfo>();
                string _searchPattern = searchPattern.ToLower();

                foreach (BuildInfo _build in builds)
                {
                    if (_build.Name.ToLower().Contains(_searchPattern))
                        _filteredBuilds.Add(_build);
                }

                filteredBuilds = _filteredBuilds.ToArray();
            }
            else
                filteredBuilds = builds;

            SortBuilds();
        }

        private void SortBuilds()
        {
            switch (selectedBuildSortOption)
            {
                case 0:
                    Array.Sort(filteredBuilds, (a, b) =>
                    {
                        return Directory.GetCreationTime(a.Path).CompareTo(Directory.GetCreationTime(b.Path));
                    });
                    break;

                case 1:
                    Array.Sort(filteredBuilds, (a, b) =>
                    {
                        return a.Name.CompareTo(b.Name);
                    });
                    break;

                case 2:
                    Array.Sort(filteredBuilds, (a, b) =>
                    {
                        return (a.Platform != b.Platform)
                               ? a.Platform.CompareTo(b.Platform)
                               : a.Name.CompareTo(b.Name);
                    });
                    break;
            }

            if (!doSortAscending)
                Array.Reverse(filteredBuilds);
        }
        #endregion

        #region Configuration
        private readonly GUIContent activeSymbolHeaderGUI = new GUIContent("Active Symbols:", "Active scripting define symbols in project.");
        private readonly GUIContent customSymbolHeaderGUI = new GUIContent("Scripting Define Symbols:",
                                                                           "All custom scripting define symbols in project.");
        
        private readonly GUIContent applySymbolsGUI = new GUIContent("Apply", "Apply selected symbols on project.");
        private readonly GUIContent refreshSymbolsGUI = new GUIContent("Refresh", "Refresh project custom scripting define symbols.");
        private readonly GUIContent useSelectedSymbolsGUI = new GUIContent("Use Selected Symbols", "Use selected symbols on selected build preset.");

        private readonly GUIContent buildPresetsHeaderGUI = new GUIContent("Build Presets:", "All registered build presets.");

        private readonly Color configurationColor = SuperColor.Lavender.Get();

        private ScriptingDefineSymbolInfo[] customSymbols = new ScriptingDefineSymbolInfo[] { };
        private string[] otherSymbols = new string[] { };
        private string[] allSymbols = new string[] { };

        private Vector2 customSymbolsScroll = new Vector2();
        private Vector2 allSymbolsScroll = new Vector2();

        private int selectedSymbol = -1;

        // -----------------------

        private void DrawConfiguration()
        {
            DrawSection(DrawConfigurationHeader,
                        DrawCustomSymbols,
                        DrawActiveSymbols,
                        DrawConfigurationBottom,
                        ref customSymbolsScroll);
        }

        private void DrawConfigurationHeader()
        {
            EditorGUILayout.LabelField(customSymbolHeaderGUI, EditorStyles.boldLabel);
        }

        private void DrawCustomSymbols()
        {
            // Toolbar.
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(refreshSymbolsGUI, EditorStyles.toolbarButton, GUILayout.Width(55f)))
                InitializeConfiguration();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10f);

            // Custom symbols.
            for (int _i = 0; _i < customSymbols.Length; _i++)
            {
                ScriptingDefineSymbolInfo _symbol = customSymbols[_i];

                Rect _rect = EditorGUILayout.GetControlRect();
                _rect.x -= 2f;
                _rect.width += 4f;

                // Background color.
                if (selectedSymbol == _i)
                {
                    EditorGUI.DrawRect(_rect, selectedColor);
                }
                else if ((_i % 2) == 0)
                    EditorGUI.DrawRect(_rect, oddColor);

                // Symbol highlight selection.
                Rect _drawRect = _rect;
                _drawRect.xMin += 25f;
                _drawRect.width -= 25f;

                if (GUI.Button(_drawRect, _symbol.DefineSymbol.Description, EditorStyles.label))
                    selectedSymbol = _i;

                // Symbol activation.
                _drawRect.width = 20f;
                _drawRect.xMin -= _drawRect.width;

                bool _isEnable = EditorGUI.ToggleLeft(_drawRect, GUIContent.none, _symbol.IsEnabled);
                if (_isEnable != _symbol.IsEnabled)
                {
                    _symbol.IsEnabled = _isEnable;
                    if (!_isEnable)
                    {
                        UnityEditor.ArrayUtility.Remove(ref allSymbols, _symbol.DefineSymbol.Symbol);

                    }
                    else if (!allSymbols.Contains(_symbol.DefineSymbol.Symbol))
                    {
                        UnityEditor.ArrayUtility.Add(ref allSymbols, _symbol.DefineSymbol.Symbol);
                    }
                }

                // Symbol selection.
                _drawRect.xMax = _rect.xMax;
                _drawRect.xMin = _drawRect.xMax - 25f;
                _symbol.IsSelected = EditorGUI.Toggle(_drawRect, GUIContent.none, _symbol.IsSelected, EditorStyles.radioButton);
            }

            // Unselect on empty space click.
            Event _event = Event.current;
            if (_event.type == EventType.MouseDown)
            {
                selectedSymbol = -1;
                _event.Use();
            }
        }

        private void DrawActiveSymbols()
        {
            allSymbolsScroll = EditorGUILayout.BeginScrollView(allSymbolsScroll, GUILayout.Height(SectionHeight));
            EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 3f);

            EnhancedEditorGUILayout.UnderlinedLabel(activeSymbolHeaderGUI, EditorStyles.boldLabel);
            DrawSymbols(allSymbols);

            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigurationBottom()
        {
            EditorGUILayout.BeginHorizontal();

            // Apply symbols button.
            EnhancedEditorGUIUtility.PushGUIColor(validColor);
            if (GUILayout.Button(applySymbolsGUI, GUILayout.Width(85f), GUILayout.Height(20f)))
                SetScriptingDefineSymbol();

            EnhancedEditorGUIUtility.PopGUIColor();
            GUILayout.FlexibleSpace();

            // Button to apply selected symbols on selected preset.
            EnhancedEditorGUIUtility.PushGUIColor(configurationColor);
            if (GUILayout.Button(useSelectedSymbolsGUI, GUILayout.Width(150f), GUILayout.Height(20f)))
                UseSelectedSymbolsOnPreset();

            EnhancedEditorGUIUtility.PopGUIColor();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5f);

            // Separator.
            Rect _position = EditorGUILayout.GetControlRect(false, 2f);
            _position.width *= .55f;

            EnhancedEditorGUI.HorizontalLine(_position, 1f, separatorColor);
            EditorGUILayout.Space(5f);

            // All registered build presets.
            EnhancedEditorGUILayout.UnderlinedLabel(buildPresetsHeaderGUI, EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);

            DrawBuildPresets(true);
        }

        // -----------------------

        private void InitializeConfiguration()
        {
            // Get all custom define symbols.
            var _symbols = new List<ScriptingDefineSymbolAttribute>();
            foreach (var _symbol in TypeCache.GetTypesWithAttribute<ScriptingDefineSymbolAttribute>())
                _symbols.AddRange(_symbol.GetCustomAttributes(typeof(ScriptingDefineSymbolAttribute), true) as ScriptingDefineSymbolAttribute[]);

            customSymbols = Array.ConvertAll(_symbols.ToArray(), (s) => new ScriptingDefineSymbolInfo(s));
            Array.Sort(customSymbols, (a, b) => a.DefineSymbol.Description.CompareTo(b.DefineSymbol.Description));

            // Get all enabled symbols.
            allSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');

            Array.Sort(allSymbols);
            otherSymbols = allSymbols;

            for (int _i = otherSymbols.Length; _i-- > 0;)
            {
                int _index = Array.FindIndex(customSymbols, (s) => s.DefineSymbol.Symbol == otherSymbols[_i]);
                if (_index > -1)
                {
                    customSymbols[_index].IsEnabled = true;
                    UnityEditor.ArrayUtility.RemoveAt(ref otherSymbols, _i);
                }
            }

            RefreshSelectedSymbols();
        }

        private void SetScriptingDefineSymbol()
        {
            SetScriptingDefineSymbols(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allSymbols));
        }

        private void SetScriptingDefineSymbols(BuildTargetGroup _targetGroup, string _symbols)
        {
            EditorUtility.DisplayProgressBar("Reloading Assemblies", "Reloading assemblies... This can take up to a few minutes.", 1f);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_targetGroup, _symbols);

            EditorUtility.ClearProgressBar();
        }
        #endregion

        #region Settings
        private void SaveSettings()
        {
            EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region Draw Utility
        private void DrawSection(Action _onDrawHeader, Action _onDrawSection, Action _onDrawRightSide, Action _onDrawBottom, ref Vector2 _sectionScroll)
        {
            // Header.
            _onDrawHeader();

            // Section.
            EditorGUILayout.BeginHorizontal();

            Rect _rect = EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width * SectionWidthCoef), GUILayout.Height(SectionHeight));
            DrawSectionBackground(_rect);

            _sectionScroll = EditorGUILayout.BeginScrollView(_sectionScroll);
            _onDrawSection();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right side.
            GUILayout.Space(10f);
            EditorGUILayout.BeginVertical(GUILayout.Height(SectionHeight));

            _onDrawRightSide();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);

            // Bottom.
            _onDrawBottom();
        }

        private void DrawSectionBackground(Rect _position)
        {
            EditorGUI.DrawRect(_position, sectionColor);
            GUI.Label(new Rect(_position.x - 1f, _position.y - 1f, _position.width + 2f, _position.height + 2f),
                      GUIContent.none, EditorStyles.helpBox);
        }

        private void DrawBuildDirectory(GUIContent _header)
        {
            EnhancedEditorGUILayout.UnderlinedLabel(_header, EditorStyles.boldLabel);

            EditorGUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();

            string _buildDirectory = EditorGUILayout.DelayedTextField(Settings.BuildDirectory);
            if (GUILayout.Button(setBuildDirectoryGUI, GUILayout.Width(25f)))
                _buildDirectory = EditorUtility.OpenFolderPanel(SelectBuildDirectoryWindowTitle, Settings.BuildDirectory, string.Empty);

            if (!string.IsNullOrEmpty(_buildDirectory) && (_buildDirectory != Settings.BuildDirectory))
            {
                Settings.BuildDirectory = _buildDirectory;

                SaveSettings();
                RefreshBuilds();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuildPresets(bool _canEdit)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Preset selection.
            int _selectedBuildPreset = GUILayout.Toolbar(selectedBuildPreset, buildPresetsGUI, GUILayout.Height(25f));
            if (_selectedBuildPreset != selectedBuildPreset)
            {
                selectedBuildPreset = _selectedBuildPreset;
                RefreshSelectedSymbols();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10f);

            BuildPreset _preset = buildPresets[selectedBuildPreset];
            bool _isCustom = selectedBuildPreset == (buildPresets.Length - 1);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width * SectionWidthCoef));

            if (_canEdit || _isCustom)
            {
                Undo.RecordObject(_preset, "build preset changes");
                
                _preset.Description = EditorGUILayout.TextArea(_preset.Description, EnhancedEditorStyles.TextArea, GUILayout.MaxWidth(Screen.width * SectionWidthCoef));

                EditorGUILayout.Space(5f);
                EditorGUI.BeginChangeCheck();

                _preset.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(buildTargetGUI, _preset.BuildTarget);
                _preset.BuildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField(buildOptionsGUI, _preset.BuildOptions);
                _preset.ScriptingDefineSymbols = EditorGUILayout.DelayedTextField(scriptingSymbolsGUI, _preset.ScriptingDefineSymbols);

                if (EditorGUI.EndChangeCheck() && !_isCustom)
                    SaveBuildPreset(_preset);

                EditorGUILayout.Space(15f);

                // Save / delete preset buttons.
                if (_isCustom)
                {
                    EnhancedEditorGUIUtility.PushGUIColor(validColor);

                    if (GUILayout.Button(savePresetGUI, GUILayout.Width(75f), GUILayout.Height(20f)))
                        CreateBuildPresetWindow.GetWindow(Array.ConvertAll(buildPresetsGUI, (b) => b.text), CreateBuildPreset);

                    EnhancedEditorGUIUtility.PopGUIColor();
                }
                else
                {
                    EnhancedEditorGUIUtility.PushGUIColor(warningColor);

                    if (GUILayout.Button(deletePresetGUI, GUILayout.Width(75f), GUILayout.Height(20f))
                        && EditorUtility.DisplayDialog($"Delete \"{buildPresetsGUI[selectedBuildPreset].text}\" preset",
                                                        "Are you sure you want to delete this preset?\n" +
                                                        "This action cannot be undone.", "Yes", "Cancel"))
                    {
                        DeleteBuildPreset(_preset);
                    }
                        
                    EnhancedEditorGUIUtility.PopGUIColor();
                }
            }
            else
            {
                EditorGUILayout.LabelField(_preset.Description, EnhancedEditorStyles.BoldWordWrappedRichText);
                EditorGUILayout.Space(5f);

                EditorGUILayout.EnumPopup(buildTargetGUI, _preset.BuildTarget);
                EditorGUILayout.EnumFlagsField(buildOptionsGUI, _preset.BuildOptions);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginVertical();

            // Preset symbols.
            EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing * 3f);
            EnhancedEditorGUILayout.UnderlinedLabel(buildPresetSymbolsGUI, EditorStyles.boldLabel);

            DrawSymbols(_preset.ScriptingDefineSymbols);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        // -----------------------

        private void DrawSymbols(string _symbols)
        {
            DrawSymbols(_symbols.Split(';'));
        }

        private void DrawSymbols(string[] _symbols)
        {
            string _allSymbols = string.Empty;
            foreach (string _symbol in _symbols)
            {
                string _color = Array.Exists(customSymbols, (s) => s.DefineSymbol.Symbol == _symbol)
                                ? "green"
                                : "teal";

                _allSymbols += $"<b><color={_color}>{_symbol}</color></b> ; ";
            }

            EditorGUILayout.LabelField(_allSymbols, EnhancedEditorStyles.WordWrappedRichText);
        }
        #endregion

        #region Build Preset Utility
        private void RefreshBuildPresets()
        {
            presetResources.Reload();
            buildPresets = presetResources.GetResources();

            BuildPreset _customPreset = ArrayUtility.Find(buildPresets, (p) =>
            {
                return p.name == $"{presetResources.Prefix}{presetResources.DefaultName}{presetResources.Suffix}";
            });

            // Create default preset if not found, and initialize it.
            if (!_customPreset)
            {
                _customPreset = presetResources.CreateResource(presetResources.DefaultName);
                ArrayUtility.Add(ref buildPresets, _customPreset);
            }
            else
            {
                int _index = Array.IndexOf(buildPresets, _customPreset);

                buildPresets[_index] = buildPresets[buildPresets.Length - 1];
                buildPresets[buildPresets.Length - 1] = _customPreset;
            }

            _customPreset.BuildAmount = 0;
            _customPreset.BuildOptions = default;
            _customPreset.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            _customPreset.ScriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.BuildPipeline.GetBuildTargetGroup(_customPreset.BuildTarget));

            buildPresetsGUI = Array.ConvertAll(buildPresets, (p) => new GUIContent(p.name.Replace(presetResources.Prefix, string.Empty)));
            selectedBuildPreset = 0;
        }

        private void CreateBuildPreset(string _name)
        {
            BuildPreset _template = buildPresets[buildPresets.Length - 1];
            BuildPreset _preset = ScriptableObject.CreateInstance<BuildPreset>();

            _preset.Description = _template.Description;
            _preset.BuildOptions = _template.BuildOptions;
            _preset.BuildTarget = _template.BuildTarget;
            _preset.ScriptingDefineSymbols = _template.ScriptingDefineSymbols;

            presetResources.CreateResource(_name, _preset);
            RefreshBuildPresets();
        }

        private void SaveBuildPreset(BuildPreset _preset)
        {
            EditorUtility.SetDirty(_preset);
            AssetDatabase.SaveAssets();
        }

        private void DeleteBuildPreset(BuildPreset _preset)
        {
            string _path = AssetDatabase.GetAssetPath(_preset);
            if (!string.IsNullOrEmpty(_path))
            {
                AssetDatabase.DeleteAsset(_path);
                AssetDatabase.Refresh();

                RefreshBuildPresets();
            }
        }
        #endregion

        #region Scripting Symbols Utility
        private void UseSelectedSymbolsOnPreset()
        {
            BuildPreset _preset = buildPresets[selectedBuildPreset];
            List<string> _symbols = new List<string>(_preset.ScriptingDefineSymbols.Split(';'));

            foreach (var _symbol in customSymbols)
            {
                if (_symbol.IsSelected)
                {
                    if (!_symbols.Contains(_symbol.DefineSymbol.Symbol))
                        _symbols.Add(_symbol.DefineSymbol.Symbol);
                }
                else if (_symbols.Contains(_symbol.DefineSymbol.Symbol))
                    _symbols.Remove(_symbol.DefineSymbol.Symbol);
            }

            _preset.ScriptingDefineSymbols = string.Join(";", _symbols);
        }

        private void RefreshSelectedSymbols()
        {
            string[] _symbols = buildPresets[selectedBuildPreset].ScriptingDefineSymbols.Split(';');
            foreach (var _symbol in customSymbols)
                _symbol.IsSelected = Array.Exists(_symbols, (s) => s == _symbol.DefineSymbol.Symbol);
        }
        #endregion

        #region Build Utility
        public static void LaunchBuild(string _path, int _amount = 1)
        {
            if (!File.Exists(_path))
            {
                Debug.LogError($"Specified build does not exist at path: \"{_path}\"");
                return;
            }

            for (int _i = 0; _i < _amount; _i++)
                Process.Start(_path);
        }

        public static string[] GetBuilds()
        {
            BuildPipelineSettings _settings = Settings;
            if (!Directory.Exists(_settings.BuildDirectory))
            {
                Directory.CreateDirectory(_settings.BuildDirectory);
                return new string[] { };
            }

            List<string> _builds = new List<string>();
            string[] _executables = Directory.GetFiles(_settings.BuildDirectory, "*.exe", SearchOption.AllDirectories);

            foreach (string _file in _executables)
            {
                if (Path.GetFileNameWithoutExtension(_file) == Application.productName)
                    _builds.Add(_file);
            }

            return _builds.ToArray();
        }

        public static string GetBuildIcon(string _buildName, out GUIContent _icon)
        {
            switch (_buildName)
            {
                case string _ when _buildName.Contains("Android"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Android.Small");
                    return "Android";

                case string _ when _buildName.Contains("Facebook"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Facebook.Small");
                    return "Facebook";

                case string _ when _buildName.Contains("Flash"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Flash.Small");
                    return "Flash";

                case string _ when _buildName.Contains("iPhone"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.iPhone.Small");
                    return "iPhone";

                case string _ when _buildName.Contains("Lumin"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Lumin.Small");
                    return "Lumin";

                case string _ when _buildName.Contains("N3DS"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.N3DS.Small");
                    return "N3DS";

                case string _ when _buildName.Contains("PS4"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.PS4.Small");
                    return "PS4";

                case string _ when _buildName.Contains("PS5"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.PS5.Small");
                    return "PS5";

                case string _ when _buildName.Contains("Stadia"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Stadia.Small");
                    return "Stadia";
                case string _ when _buildName.Contains("Switch"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Switch.Small");
                    return "Switch";

                case string _ when _buildName.Contains("tvOS"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.tvOS.Small");
                    return "tvOS";

                case string _ when _buildName.Contains("WebGL"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.WebGL.Small");
                    return "WebGL";

                case string _ when _buildName.Contains("Windows"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Metro.Small");
                    return "Windows";

                case string _ when _buildName.Contains("Xbox360"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Xbox360ebGL.Small");
                    return "Xbox360";

                case string _ when _buildName.Contains("XboxOne"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.XboxOne.Small");
                    return "XboxOne";

                case string _ when _buildName.Contains("Standalone"):
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Standalone.Small");
                    return "Standalone";

                default:
                    _icon = EditorGUIUtility.IconContent("BuildSettings.Editor.Small");
                    return "Unknown";
            }
        }
        #endregion

        #region Create Build Preset Window
        private class CreateBuildPresetWindow : EditorWindow
        {
            private const string EmptyNameError = "Name cannot be null or empty!";
            private const string ExistingNameError = "An preset with the same name already exist.";

            private readonly GUIContent presetNameGUI = new GUIContent("Build Preset name:", "Name of the build preset to create.");
            private readonly GUIContent createPresetGUI = new GUIContent("OK", "Create a build preset with this name.");

            private string[] existingPresets = new string[] { };
            private string presetName = "NewPreset";

            private Action<string> callback = null;

            // -----------------------

            public static void GetWindow(string[] _existingPresets, Action<string> _callback)
            {
                CreateBuildPresetWindow _window = GetWindow<CreateBuildPresetWindow>(true, "Create Build Preset", true);
                _window.existingPresets = _existingPresets;
                _window.callback = _callback;

                _window.minSize = _window.maxSize = new Vector2(250f, 70f);
                _window.ShowUtility();
            }

            private void OnGUI()
            {
                Rect _rect = new Rect(5f, 5f, 40f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_rect, presetNameGUI);
                _rect.x += 50f;

                _rect.width = position.width - _rect.x - 5f;
                presetName = EditorGUI.TextField(_rect, presetName);

                string _value = presetName.Trim();
                if (string.IsNullOrEmpty(_value))
                {
                    _rect.x = 5f;
                    _rect.y += _rect.height + 5f;
                    _rect.height = 35f;
                    _rect.width = position.width - 10f;

                    EditorGUI.HelpBox(_rect, EmptyNameError, UnityEditor.MessageType.Error);
                }
                else if (existingPresets.Contains(_value))
                {
                    _rect.x = 5f;
                    _rect.y += _rect.height + 5f;
                    _rect.height = 35f;
                    _rect.width = position.width - 10f;

                    EditorGUI.HelpBox(_rect, ExistingNameError, UnityEditor.MessageType.Error);
                }
                else
                {
                    _rect.x = position.width - 55f;
                    _rect.y += _rect.height + 10f;
                    _rect.width = 50f;
                    _rect.height = 25f;

                    if (GUI.Button(_rect, createPresetGUI))
                    {
                        callback(_value);
                        Close();
                    }
                }
            }
        }
        #endregion
    }
}
