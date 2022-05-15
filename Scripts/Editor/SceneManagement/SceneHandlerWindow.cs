// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor window used to manage all scenes and scene bundles in the project,
    /// with the ability to easily open and close them in the editor.
    /// </summary>
    [InitializeOnLoad]
    public class SceneHandlerWindow : EditorWindow
    {
        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="SceneHandlerWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="SceneHandlerWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Scene Handler", false, 110)]
        public static SceneHandlerWindow GetWindow()
        {
            SceneHandlerWindow _window = GetWindow<SceneHandlerWindow>("Scene Handler");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float CreateGroupButtonWidth = 105f;
        private const float RefreshButtonWidth = 55f;
        private const float CoreSceneButtonWidth = 102f;

        private const string OpenFormat = "Close all open scene(s) and open this {0} in the editor.";
        private const string AddFormat = "Open this {0} additively in the editor.";
        private const string CloseFormat = "Close this {0} in the editor.";
        private const string PlayFormat = "Enter play mode with this {0} first loaded.";
        private const string SceneFormat = "scene";
        private const string BundleFormat = "bundle";

        private const string UndoRecordTitle = "Scene Handler Change";
        private const string UnloadCoreSceneKey = "UnloadCoreScene";
        private const string PlayScenesKey = "PlayScenes";
        private const char PlaySceneSeparator = ':';

        private static readonly AutoManagedResource<SceneHandler> resource = new AutoManagedResource<SceneHandler>();

        private readonly GUIContent createGroupGUI = new GUIContent(" Create Group", "Create a new group.");
        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh all scenes and scene bundles.");
        private readonly GUIContent coreSceneGUI = new GUIContent(" Core Scene", "Edit the Core Scene settings");
        private readonly GUIContent[] tabsGUI = new GUIContent[]
                                                    {
                                                        new GUIContent("Scenes", "Browse and manage all Scenes in the project."),
                                                        new GUIContent("Scene Bundles", "Browse and manage all Scene Bundles in the project."),
                                                    };

        private readonly GUIContent moveUpGUI = new GUIContent("Move Up ↑", "Move this group up in the hierarchy.");
        private readonly GUIContent moveDownGUI = new GUIContent("Move Down ↓", "Move this group down in the hierarchy.");
        private readonly GUIContent deleteGroupGUI = new GUIContent(string.Empty, "Delete this group.");

        private readonly GUIContent openGUI = new GUIContent("OPEN");
        private readonly GUIContent addGUI = new GUIContent("ADD.");
        private readonly GUIContent closeGUI = new GUIContent("CLOSE");
        private readonly GUIContent playGUI = new GUIContent(string.Empty);
        private readonly GUIContent menuGUI = new GUIContent(string.Empty, "Show additional options.");

        /// <summary>
        /// Database containing all scenes and scene bundles group settings.
        /// </summary>
        public static SceneHandler Database => resource.GetResource();

        [SerializeField] private int selectedTab = 0;
        [SerializeField] private string searchFilter = string.Empty;

        private Vector2 scroll = new Vector2();

        // -----------------------

        static SceneHandlerWindow()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnEnable()
        {
            Database.Refresh();
            FilterScenes();

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneUnloaded += OnSceneUnloaded;

            titleContent.image = EditorGUIUtility.IconContent("SceneAsset On Icon").image;
            createGroupGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");
            coreSceneGUI.image = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            deleteGroupGUI.image = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
            playGUI.image = EditorGUIUtility.FindTexture("PlayButton");
            menuGUI.image = EditorGUIUtility.FindTexture("_Menu");
        }

        private void OnGUI()
        {
            Undo.RecordObject(this, UndoRecordTitle);
            Undo.RecordObject(Database, UndoRecordTitle);

            // Toolbar.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(createGroupGUI, EditorStyles.toolbarButton, GUILayout.Width(CreateGroupButtonWidth)))
                {
                    switch (selectedTab)
                    {
                        case 0:
                            ArrayUtility.Add(ref Database.sceneGroups, new SceneHandler.SceneGroup("New Group"));
                            break;

                        case 1:
                            ArrayUtility.Add(ref Database.bundleGroups, new SceneHandler.BundleGroup("New Group"));
                            break;

                        default:
                            break;
                    }
                }

                string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter, GUILayout.MinWidth(50f));
                if (_searchFilter != searchFilter)
                {
                    searchFilter = _searchFilter;
                    FilterScenes();
                }

                if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton, GUILayout.Width(RefreshButtonWidth)))
                {
                    Database.Refresh();
                    FilterScenes();
                }
            }

            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                GUILayout.Space(5f);

                selectedTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTab, tabsGUI, GUILayout.Height(25f));

                // Core scene.
                Rect _position = new Rect(GUILayoutUtility.GetLastRect())
                {
                    x = position.width - (CoreSceneButtonWidth + 3f),
                    width = CoreSceneButtonWidth,
                    height = 25f
                };

                if (GUI.Button(_position, coreSceneGUI))
                {
                    EnhancedEditorSettings.OpenPreferencesSettings();
                }

                switch (selectedTab)
                {
                    // Scenes.
                    case 0:
                        DrawScenes();
                        break;

                    // Scene Bundles.
                    case 1:
                        DrawBundles();
                        break;

                    default:
                        break;
                }
            }

            // Renaming group focus update.
            if ((GUIUtility.keyboardControl != 0) && !EditorGUIUtility.editingTextField || (Event.current.type == EventType.MouseDown))
            {
                GUIUtility.keyboardControl = 0;
                Database.SaveChanges();

                Repaint();
            }
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        #endregion

        #region GUI Draw
        private const float MoveUpButtonWidth = 77f;
        private const float MoveDownButtonWidth = 90f;
        private const float OpenButtonWidth = 45f;
        private const float AddCloseButtonWidth = 50f;
        private const float PlayButtonWidth = 25f;

        private readonly GUIContent moveInGroupGUI = new GUIContent("Move in Group/", "Move this object into one this group.");
        private readonly GUIContent selectAssetGUI = new GUIContent("Select Asset", "Select this asset in the project window.");
        private readonly GUIContent deleteGUI = new GUIContent("Delete", "Permanently delete this asset from the project.");

        private readonly Color groupColor = new Color(1f, 1f, 1f, .1f);

        // -----------------------

        private void DrawScenes()
        {
            openGUI.tooltip = string.Format(OpenFormat, SceneFormat);
            addGUI.tooltip = string.Format(AddFormat, SceneFormat);
            closeGUI.tooltip = string.Format(CloseFormat, SceneFormat);
            playGUI.tooltip = string.Format(PlayFormat, SceneFormat);

            for (int _i = 1; _i < Database.sceneGroups.Length; _i++)
            {
                var _group = Database.sceneGroups[_i];
                if (_group.IsVisible && DrawGroup(Database.sceneGroups, _i, _group.Scenes.Length, IsElementVisible, IsElementLoaded, GetElementName,
                                                  OnOpen, OnAdd, OnClose, OnPlay, OnOptionsMenu))
                {
                    ArrayUtility.AddRange(ref Database.sceneGroups[0].Scenes, _group.Scenes);
                    ArrayUtility.RemoveAt(ref Database.sceneGroups, _i);

                    Database.Sort();
                    FilterScenes();

                    _i--;
                }
            }

            // Draw the first default group at last.
            var _defaultGroup = Database.sceneGroups[0];
            if (_defaultGroup.IsVisible)
            {
                DrawGroup(Database.sceneGroups, 0, _defaultGroup.Scenes.Length, IsElementVisible, IsElementLoaded, GetElementName, OnOpen, OnAdd, OnClose, OnPlay, OnOptionsMenu);
            }

            // ----- Local Methods ----- \\

            bool IsElementVisible(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                return _element.IsVisible;
            }

            bool IsElementLoaded(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                return _element.IsLoaded;
            }

            GUIContent GetElementName(int _groupIndex, int _elementIndex)
            {
                var _settings = EnhancedEditorSettings.Settings;
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];

                GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_element.Name);

                if (_settings.IsCoreSceneEnabled && (_settings.CoreScene.GUID == _element.GUID))
                {
                    _label.image = coreSceneGUI.image;
                }

                return _label;
            }

            void OnOpen(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                OpenSceneFromGUID(_element.GUID, OpenSceneMode.Single);
            }

            void OnAdd(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                OpenSceneFromGUID(_element.GUID, OpenSceneMode.Additive);
            }

            void OnClose(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                CloseSceneFromGUID(_element.GUID);
            }

            void OnPlay(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                string _path = AssetDatabase.GUIDToAssetPath(_element.GUID);

                SessionState.SetString(PlayScenesKey, _path);
                EditorApplication.EnterPlaymode();
            }

            void OnOptionsMenu(int _groupIndex, int _elementIndex, GenericMenu _menu)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];

                for (int _i = 0; _i < Database.sceneGroups.Length; _i++)
                {
                    if (_i == _groupIndex)
                        continue;

                    var _group = Database.sceneGroups[_i];
                    GUIContent _gui = new GUIContent(moveInGroupGUI)
                    {
                        text = $"{moveInGroupGUI.text}{_group.Name}"
                    };

                    _menu.AddItem(_gui, false, () =>
                    {
                        ArrayUtility.Add(ref _group.Scenes, _element);
                        ArrayUtility.RemoveAt(ref Database.sceneGroups[_groupIndex].Scenes, _elementIndex);

                        Database.Sort();
                        FilterScenes();
                    });
                }

                _menu.AddItem(selectAssetGUI, false, () =>
                {
                    string _path = AssetDatabase.GUIDToAssetPath(_element.GUID);
                    if (!string.IsNullOrEmpty(_path))
                    {
                        Object _object = AssetDatabase.LoadMainAssetAtPath(_path);

                        EditorGUIUtility.PingObject(_object);
                        Selection.activeObject = _object;
                    }
                });
                _menu.AddItem(deleteGUI, false, () =>
                {
                    string _path = AssetDatabase.GUIDToAssetPath(_element.GUID);
                    string _name = Path.GetFileNameWithoutExtension(_path);

                    if (!string.IsNullOrEmpty(_path) && EditorUtility.DisplayDialog("Confirm Action",
                                                                                    $"Are you sure you want to delete the scene {_name} from the project?\n\n" +
                                                                                    "This action cannot be undone.",
                                                                                    "Yes", "Cancel"))
                    {
                        AssetDatabase.DeleteAsset(_path);
                        ArrayUtility.RemoveAt(ref Database.sceneGroups[_groupIndex].Scenes, _elementIndex);

                        Database.SaveChanges();
                    }
                });
            }
        }

        private void DrawBundles()
        {
            openGUI.tooltip = string.Format(OpenFormat, BundleFormat);
            addGUI.tooltip = string.Format(AddFormat, BundleFormat);
            closeGUI.tooltip = string.Format(CloseFormat, BundleFormat);
            playGUI.tooltip = string.Format(PlayFormat, BundleFormat);

            for (int _i = 1; _i < Database.bundleGroups.Length; _i++)
            {
                var _group = Database.bundleGroups[_i];
                if (_group.IsVisible && DrawGroup(Database.bundleGroups, _i, _group.Bundles.Length, IsElementVisible, IsElementLoaded, GetElementName,
                                                  OnOpen, OnAdd, OnClose, OnPlay, OnOptionsMenu))
                {
                    ArrayUtility.AddRange(ref Database.bundleGroups[0].Bundles, _group.Bundles);
                    ArrayUtility.RemoveAt(ref Database.bundleGroups, _i);

                    Database.Sort();
                    FilterScenes();

                    _i--;
                }
            }

            // Draw the first default group at last.
            var _defaultGroup = Database.bundleGroups[0];
            if (_defaultGroup.IsVisible)
            {
                DrawGroup(Database.bundleGroups, 0, _defaultGroup.Bundles.Length, IsElementVisible, IsElementLoaded, GetElementName, OnOpen, OnAdd, OnClose, OnPlay, OnOptionsMenu);
            }

            // ----- Local Methods ----- \\

            bool IsElementVisible(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                return _element.IsVisible;
            }

            bool IsElementLoaded(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                return _element.IsLoaded;
            }

            GUIContent GetElementName(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                return EnhancedEditorGUIUtility.GetLabelGUI(_element.SceneBundle.name);
            }

            void OnOpen(int _groupIndex, int _elementIndex)
            {
                SceneBundle _bundle = Database.bundleGroups[_groupIndex].Bundles[_elementIndex].SceneBundle;
                if (_bundle.Scenes.Length > 0)
                {
                    OpenSceneFromGUID(_bundle.Scenes[0].GUID, OpenSceneMode.Single);
                    for (int _i = 1; _i < _bundle.Scenes.Length; _i++)
                    {
                        OpenSceneFromGUID(_bundle.Scenes[_i].GUID, OpenSceneMode.Additive);
                    }
                }
            }

            void OnAdd(int _groupIndex, int _elementIndex)
            {
                SceneBundle _bundle = Database.bundleGroups[_groupIndex].Bundles[_elementIndex].SceneBundle;
                for (int _i = 0; _i < _bundle.Scenes.Length; _i++)
                {
                    OpenSceneFromGUID(_bundle.Scenes[_i].GUID, OpenSceneMode.Additive);
                }
            }

            void OnClose(int _groupIndex, int _elementIndex)
            {
                SceneBundle _bundle = Database.bundleGroups[_groupIndex].Bundles[_elementIndex].SceneBundle;
                for (int _i = 0; _i < _bundle.Scenes.Length; _i++)
                {
                    CloseSceneFromGUID(_bundle.Scenes[_i].GUID);
                }
            }

            void OnPlay(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                string _playScenes = string.Empty;

                foreach (SceneAsset _scene in _element.SceneBundle.Scenes)
                {
                    string _path = AssetDatabase.GUIDToAssetPath(_scene.guid);
                    _playScenes += $"{_path}{PlaySceneSeparator}";
                }

                SessionState.SetString(PlayScenesKey, _playScenes);
                EditorApplication.EnterPlaymode();
            }

            void OnOptionsMenu(int _groupIndex, int _elementIndex, GenericMenu _menu)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];

                for (int _i = 0; _i < Database.bundleGroups.Length; _i++)
                {
                    if (_i == _groupIndex)
                        continue;

                    var _group = Database.bundleGroups[_i];
                    GUIContent _gui = new GUIContent(moveInGroupGUI)
                    {
                        text = $"{moveInGroupGUI.text}{_group.Name}"
                    };

                    _menu.AddItem(_gui, false, () =>
                    {
                        ArrayUtility.Add(ref _group.Bundles, _element);
                        ArrayUtility.RemoveAt(ref Database.bundleGroups[_groupIndex].Bundles, _elementIndex);

                        Database.Sort();
                        FilterScenes();
                    });
                }

                _menu.AddItem(selectAssetGUI, false, () =>
                {
                    EditorGUIUtility.PingObject(_element.SceneBundle);
                    Selection.activeObject = _element.SceneBundle;
                });
                _menu.AddItem(deleteGUI, false, () =>
                {
                    string _path = AssetDatabase.GetAssetPath(_element.SceneBundle);

                    if (!string.IsNullOrEmpty(_path) && EditorUtility.DisplayDialog("Confirm Action",
                                                                                    $"Are you sure you want to delete the scene bundle {_element.SceneBundle.name} from the project?\n\n" +
                                                                                    "This action cannot be undone.",
                                                                                    "Yes", "Cancel"))
                    {
                        AssetDatabase.DeleteAsset(_path);
                        ArrayUtility.RemoveAt(ref Database.bundleGroups[_groupIndex].Bundles, _elementIndex);

                        Database.SaveChanges();
                    }
                });
            }
        }

        // -----------------------

        private bool DrawGroup(SceneHandler.Group[] _groups, int _index, int _length,
                               Func<int, int, bool> _isElementVisible, Func<int, int, bool> _isElementLoaded, Func<int, int, GUIContent> _getElementName,
                               Action<int, int> _onOpen, Action<int, int> _onAdd, Action<int, int> _onClose, Action<int, int> _onPlay, Action<int, int, GenericMenu> _onOptionsMenu)
        {
            bool _isDeleted = false;
            GUILayout.Space(5f);

            // Group header and buttons.
            Rect _groupPosition = EditorGUILayout.GetControlRect(true, 25f);
            {
                Rect _temp = new Rect(_groupPosition)
                {
                    xMin = _groupPosition.x + 7f,
                    y = _groupPosition.y + 3f,
                    height = EditorGUIUtility.singleLineHeight,
                };

                if (_index == 0)
                {
                    EnhancedEditorGUI.UnderlinedLabel(_temp, _groups[_index].Name, EditorStyles.boldLabel);
                }
                else
                {
                    string _id = EnhancedEditorGUIUtility.GetControlID(FocusType.Keyboard).ToString();
                    _temp.xMax = _groupPosition.xMax - (MoveUpButtonWidth + MoveDownButtonWidth + EnhancedEditorGUIUtility.IconWidth + 10f + 10f + 5f + 1f);

                    GUI.SetNextControlName(_id);
                    GUIStyle _style = (GUI.GetNameOfFocusedControl() == _id)
                                    ? EditorStyles.textField
                                    : EditorStyles.boldLabel;

                    _groups[_index].Name = EditorGUI.TextField(_temp, _groups[_index].Name, _style);

                    _temp.x += _temp.width + 10f;
                    _temp.y += 2f;
                    _temp.width = MoveUpButtonWidth;

                    // Move buttons.
                    if (GUI.Button(_temp, moveUpGUI, EditorStyles.miniButtonLeft))
                    {
                        int _destinationIndex = Mathf.Max(1, _index - 1);
                        ArrayUtility.Move(_groups, _index, _destinationIndex);

                        Database.SaveChanges();
                        return false;
                    }

                    _temp.x += _temp.width;
                    _temp.width = MoveDownButtonWidth;

                    if (GUI.Button(_temp, moveDownGUI, EditorStyles.miniButtonRight))
                    {
                        int _destinationIndex = Mathf.Min(_groups.Length - 1, _index + 1);
                        ArrayUtility.Move(_groups, _index, _destinationIndex);

                        Database.SaveChanges();
                        return false;
                    }

                    _temp.x += _temp.width + 10f;
                    _temp.width = EnhancedEditorGUIUtility.IconWidth;

                    // Delete button.
                    if (EnhancedEditorGUI.IconButton(_temp, deleteGroupGUI))
                    {
                        _isDeleted = true;
                    }
                }
            }

            GUILayout.Space(5f);

            Rect _position = default;
            bool _twoColumns = position.width > 500f;

            // Draw each element in group.
            for (int _i = 0; _i < _length; _i++)
            {
                if (!_isElementVisible(_index, _i))
                    continue;

                bool _isLoaded = _isElementLoaded(_index, _i);
                _position = (_twoColumns && (_i % 2 == 1))
                          ? new Rect(_position)
                          {
                              x = _position.xMax + 10f
                          }
                          : new Rect(EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 3f))
                          {
                              height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 3f
                          };

                if (_twoColumns)
                    _position.width = (_groupPosition.width / 2f) - 5f;

                EnhancedEditorGUI.BackgroundLine(_position, false, _twoColumns ? (_i / 2) : _i);

                // Element buttons and content.
                Rect _temp = new Rect(_position)
                {
                    x = _position.x + 10f,
                    y = _position.y + 1f,
                    width = OpenButtonWidth,
                    height = _position.height - 2f
                };

                Color _color = _isLoaded
                             ? SuperColor.Crimson.Get(.9f)
                             : SuperColor.Green.Get(.9f);

                // Open button.
                using (var _scope = EnhancedGUI.GUIColor.Scope(SuperColor.Cyan.Get(.9f)))
                {
                    if (GUI.Button(_temp, openGUI))
                        _onOpen(_index, _i);
                }

                _temp.x += _temp.width + 2f;
                _temp.width = AddCloseButtonWidth;

                // Add / Close button.
                using (var _scope = EnhancedGUI.GUIColor.Scope(_color))
                {
                    if (_isLoaded)
                    {
                        if (GUI.Button(_temp, closeGUI))
                            _onClose(_index, _i);
                    }
                    else if (GUI.Button(_temp, addGUI))
                        _onAdd(_index, _i);
                }

                _temp.x += _temp.width + 5f;
                _temp.xMax = _position.xMax - (PlayButtonWidth + EnhancedEditorGUIUtility.IconWidth + 10f);

                // Element name.
                GUIContent _elementName = _getElementName(_index, _i);
                EditorGUI.LabelField(_temp, _elementName);

                _temp.x += _temp.width + EnhancedEditorGUIUtility.IconWidth + 5f;
                _temp.width = PlayButtonWidth;

                // Play button.
                if (EnhancedEditorGUI.IconButton(_temp, playGUI))
                    _onPlay(_index, _i);

                _temp.x -= 17f;
                _temp.y += 2f;
                _temp.width = EnhancedEditorGUIUtility.IconWidth;

                // Menu button.
                if (EditorGUI.DropdownButton(_temp, GUIContent.none, FocusType.Passive, EnhancedEditorStyles.PaneOptions))
                {
                    GenericMenu _menu = new GenericMenu();
                    _onOptionsMenu(_index, _i, _menu);

                    _menu.DropDown(_temp);
                }
            }

            using (var _scope = EnhancedGUI.GUIBackgroundColor.Scope(groupColor))
            {
                Rect _endGroup = new Rect(EditorGUILayout.GetControlRect(false, 5f))
                {
                    yMin = _groupPosition.y
                };

                EditorGUI.LabelField(_endGroup, GUIContent.none, EditorStyles.helpBox);
            }

            return _isDeleted;
        }
        #endregion

        #region Utility
        private void FilterScenes()
        {
            string _searchFilter = searchFilter.ToLower();

            // Scene visibility.
            for (int _i = 0; _i < Database.sceneGroups.Length; _i++)
            {
                var _group = Database.sceneGroups[_i];
                bool _isGroupVisible = _i != 0;

                foreach (var _scene in _group.Scenes)
                {
                    bool _isVisible = _scene.Name.ToLower().Contains(_searchFilter);
                    _scene.IsVisible = _isVisible;

                    if (_isVisible)
                        _isGroupVisible = true;
                }

                _group.IsVisible = _isGroupVisible;
            }

            // Bundle visibility.
            for (int _i = 0; _i < Database.bundleGroups.Length; _i++)
            {
                var _group = Database.bundleGroups[_i];
                bool _isGroupVisible = _i != 0;

                foreach (var _bundle in _group.Bundles)
                {
                    bool _isVisible = _bundle.SceneBundle.name.ToLower().Contains(_searchFilter);
                    _bundle.IsVisible = _isVisible;

                    if (_isVisible)
                        _isGroupVisible = true;
                }

                _group.IsVisible = _isGroupVisible;
            }
        }
        #endregion

        #region Scene Management Delegates
        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            Database.UpdateLoadedScenes();

            switch (_state)
            {
                // Unload the core scene if not wanted when entering edit mode.
                case PlayModeStateChange.EnteredEditMode:
                {
                    if (SessionState.GetBool(UnloadCoreSceneKey, false))
                    {
                        CloseSceneFromGUID(EnhancedEditorSettings.Settings.CoreScene.guid);
                        SessionState.SetBool(UnloadCoreSceneKey, false);
                    }
                }
                break;

                // Load the core scene if enabled when exiting edit mode.
                case PlayModeStateChange.ExitingEditMode:
                {
                    if (EnhancedEditorSettings.Settings.IsCoreSceneEnabled)
                    {
                        string _path = AssetDatabase.GUIDToAssetPath(EnhancedEditorSettings.Settings.CoreScene.guid);
                        if (string.IsNullOrEmpty(_path))
                            return;

                        Scene _coreScene = EditorSceneManager.GetSceneByPath(_path);
                        Scene _firstScene = EditorSceneManager.GetSceneAt(0);

                        if (!_coreScene.isLoaded && OpenScene(_path, OpenSceneMode.Additive))
                        {
                            SessionState.SetBool(UnloadCoreSceneKey, true);
                            _coreScene = EditorSceneManager.GetSceneByPath(_path);
                        }

                        if (_coreScene != _firstScene)
                            EditorSceneManager.MoveSceneBefore(_coreScene, _firstScene);

                        EditorSceneManager.SetActiveScene(_coreScene);
                    }
                }
                break;

                // Load all required scenes when entering play mode.
                case PlayModeStateChange.EnteredPlayMode:
                    string _playScenes = SessionState.GetString(PlayScenesKey, string.Empty);
                    if (!string.IsNullOrEmpty(_playScenes))
                    {
                        string _coreScenePath = AssetDatabase.GUIDToAssetPath(EnhancedEditorSettings.Settings.CoreScene.guid);
                        bool _isCoreSceneEnabled = EnhancedEditorSettings.Settings.IsCoreSceneEnabled;
                        int _loadedCount = EditorSceneManager.loadedSceneCount;

                        string[] _allScenes = _playScenes.Split(PlaySceneSeparator);
                        foreach (string _scenePath in _allScenes)
                        {
                            Scene _scene = EditorSceneManager.GetSceneByPath(_scenePath);
                            if (!_scene.isLoaded)
                                OpenScene(_scenePath, OpenSceneMode.Additive);
                        }

                        for (int _i = 0; _i < _loadedCount; _i++)
                        {
                            Scene _scene = EditorSceneManager.GetSceneAt(_i);
                            if ((_scene.path != _coreScenePath) && !_playScenes.Contains(_scene.path))
                                CloseScene(_scene.path);
                        }

                        SessionState.SetString(PlayScenesKey, string.Empty);
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    break;

                default:
                    break;
            }
        }

        private void OnSceneOpened(Scene _scene, OpenSceneMode _mode)
        {
            Database.UpdateLoadedScenes();
        }

        private void OnSceneClosed(Scene _scene)
        {
            Database.UpdateLoadedScenes();
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            Database.UpdateLoadedScenes();
        }

        private void OnSceneUnloaded(Scene _scene)
        {
            Database.UpdateLoadedScenes(_scene.buildIndex);
        }
        #endregion

        #region Opening and Closing Scenes
        /// <inheritdoc cref="OpenSceneFromGUID(string, OpenSceneMode)"/>
        public static bool OpenSceneFromGUID(string _sceneGUID)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_sceneGUID);
            return OpenScene(_path);
        }

        /// <param name="_sceneGUID">GUID of the scene to open.</param>
        /// <inheritdoc cref="OpenScene(string, OpenSceneMode)"/>
        public static bool OpenSceneFromGUID(string _sceneGUID, OpenSceneMode _mode)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_sceneGUID);
            return OpenScene(_path, _mode);
        }

        /// <inheritdoc cref="OpenScene(string, OpenSceneMode)"/>
        public static bool OpenScene(string _scenePath)
        {
            if (string.IsNullOrEmpty(_scenePath))
                return false;

            string _name = Path.GetFileNameWithoutExtension(_scenePath);
            int _result = EditorUtility.DisplayDialogComplex($"Open Scene \"{_name}\"",
                                                             $"You are about to open the following scene:\n  {_scenePath}\n\nWhich mode do you want to use to open it?",
                                                             "Single", "Additive", "Cancel");

            switch (_result)
            {
                case 0:
                    return OpenScene(_scenePath, OpenSceneMode.Single);

                case 1:
                    return OpenScene(_scenePath, OpenSceneMode.Additive);

                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Opens a specific scene in the editor.
        /// </summary>
        /// <param name="_scenePath">Path of the scene to open.</param>
        /// <param name="_mode">Specifies how the scene should be opened.</param>
        /// <returns>True if the scene was found and could be opened, false otherwise.</returns>
        public static bool OpenScene(string _scenePath, OpenSceneMode _mode)
        {
            if (string.IsNullOrEmpty(_scenePath))
                return false;

            try
            {
                if (Application.isPlaying)
                {
                    string _name = Path.GetFileNameWithoutExtension(_scenePath);
                    LoadSceneMode _loadMode = (_mode == OpenSceneMode.Single)
                                            ? LoadSceneMode.Single
                                            : LoadSceneMode.Additive;

                    SceneManager.LoadScene(_name, _loadMode);
                }
                else
                {
                    if ((_mode == OpenSceneMode.Single) && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        return false;

                    EditorSceneManager.OpenScene(_scenePath, _mode);
                }
            }
            catch (ArgumentException)
            {
                // If the specified scene cannot be found in the database, catch the exception and return false.
                return false;
            }

            Database.UpdateLoadedScenes();
            return true;
        }

        // -----------------------

        /// <param name="_sceneGUID">GUID of the scene to close.</param>
        /// <inheritdoc cref="CloseScene(string)"/>
        public static bool CloseSceneFromGUID(string _sceneGUID)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_sceneGUID);
            return CloseScene(_path);
        }

        /// <summary>
        /// Closes a specific scene in the editor.
        /// </summary>
        /// <param name="_scenePath">Path of the scene to close.</param>
        /// <returns>True if the scene was found and could be closed, false otherwise.</returns>
        public static bool CloseScene(string _scenePath)
        {
            if (string.IsNullOrEmpty(_scenePath))
                return false;

            try
            {
                if (Application.isPlaying)
                {
                    Scene _scene = SceneManager.GetSceneByPath(_scenePath);
                    SceneManager.UnloadSceneAsync(_scene);
                }
                else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Scene _scene = EditorSceneManager.GetSceneByPath(_scenePath);
                    EditorSceneManager.CloseScene(_scene, true);
                }
            }
            catch (ArgumentException)
            {
                // If the specified scene cannot be found in the database, catch the exception and return false.
                return false;
            }

            Database.UpdateLoadedScenes();
            return true;
        }
        #endregion
    }
}
