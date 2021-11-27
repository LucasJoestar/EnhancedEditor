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

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor window used to manage all scenes and scene bundles in the project,
    /// with the ability to easily open and close them in the editor.
    /// </summary>
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
        private const string UndoRecordTitle = "Scene Handler Change";

        private static readonly AutoManagedResource<SceneHandler> resource = new AutoManagedResource<SceneHandler>();

        private readonly GUIContent createGroupGUI = new GUIContent(" Create Group", "Create a new group.");
        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh all scenes and scene bundles.");
        private readonly GUIContent[] tabsGUI = new GUIContent[]
                                                    {
                                                        new GUIContent("Scenes", "Browse and manage all Scenes in the project."),
                                                        new GUIContent("Scene Bundles", "Browse and manage all Scene Bundles in the project."),
                                                        new GUIContent("Core Scene", "Edit the Core Scene settings.")
                                                    };

        private readonly GUIContent moveUpGUI = new GUIContent("Move Up ↑", "Move this group up in the hierarchy.");
        private readonly GUIContent moveDownGUI = new GUIContent("Move Down ↓", "Move this group down in the hierarchy.");
        private readonly GUIContent deleteGroupGUI = new GUIContent(string.Empty, "Delete this group.");

        private readonly GUIContent openSceneGUI = new GUIContent("OPEN", "Close all open scene(s) and open this one in the editor.");
        private readonly GUIContent addSceneGUI = new GUIContent("ADD.", "Open this scene additively in the editor.");
        private readonly GUIContent closeSceneGUI = new GUIContent("CLOSE", "Close this scene in the editor.");
        private readonly GUIContent playSceneGUI = new GUIContent(string.Empty, "Switch to play mode with this scene as the first loaded one.");
        private readonly GUIContent menuGUI = new GUIContent(string.Empty, "Show additional options.");

        /// <summary>
        /// Database containing all scenes and scene bundles group settings.
        /// </summary>
        public static SceneHandler Database => resource.GetResource();

        [SerializeField] private int selectedTab = 0;
        [SerializeField] private string searchFilter = string.Empty;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            Database.Refresh();
            FilterScenes();

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            createGroupGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");
            deleteGroupGUI.image = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
            playSceneGUI.image = EditorGUIUtility.FindTexture("PlayButton");
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
                }
            }

            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                GUILayout.Space(5f);

                selectedTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTab, tabsGUI, GUILayout.Height(25f));
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

                    // Core Scene.
                    case 2:
                        DrawCoreScene();
                        break;

                    default:
                        break;
                }
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

        private readonly Color groupColor = new Color(1f, 1f, 1f, .1f);

        // -----------------------

        private void DrawScenes()
        {
            for (int _i = 1; _i < Database.sceneGroups.Length; _i++)
            {
                var _group = Database.sceneGroups[_i];
                if (_group.IsVisible && DrawGroup(Database.sceneGroups, _i, _group.Scenes.Length, IsElementVisible, IsElementLoaded, GetElementName,
                                                  OnOpen, OnAdd, OnClose, OnPlay))
                {
                    ArrayUtility.AddRange(ref Database.sceneGroups[0].Scenes, _group.Scenes);
                    ArrayUtility.RemoveAt(ref Database.sceneGroups, _i);

                    _i--;
                }
            }

            // Draw the first default group at last.
            var _defaultGroup = Database.sceneGroups[0];
            if (_defaultGroup.IsVisible)
            {
                DrawGroup(Database.sceneGroups, 0, _defaultGroup.Scenes.Length, IsElementVisible, IsElementLoaded, GetElementName, OnOpen, OnAdd, OnClose, OnPlay);
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

            string GetElementName(int _groupIndex, int _elementIndex)
            {
                var _element = Database.sceneGroups[_groupIndex].Scenes[_elementIndex];
                return _element.Name;
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
                // Play.
            }
        }

        private void DrawBundles()
        {
            for (int _i = 1; _i < Database.bundleGroups.Length; _i++)
            {
                var _group = Database.bundleGroups[_i];
                if (_group.IsVisible && DrawGroup(Database.bundleGroups, _i, _group.Bundles.Length, IsElementVisible, IsElementLoaded, GetElementName,
                                                  OnOpen, OnAdd, OnClose, OnPlay))
                {
                    ArrayUtility.AddRange(ref Database.bundleGroups[0].Bundles, _group.Bundles);
                    ArrayUtility.RemoveAt(ref Database.bundleGroups, _i);

                    _i--;
                }
            }

            // Draw the first default group at last.
            var _defaultGroup = Database.bundleGroups[0];
            if (_defaultGroup.IsVisible)
            {
                DrawGroup(Database.bundleGroups, 0, _defaultGroup.Bundles.Length, IsElementVisible, IsElementLoaded, GetElementName, OnOpen, OnAdd, OnClose, OnPlay);
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

            string GetElementName(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                return _element.SceneBundle.name;
            }

            void OnOpen(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                // Open
            }

            void OnAdd(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                // Add
            }

            void OnClose(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                // Close
            }

            void OnPlay(int _groupIndex, int _elementIndex)
            {
                var _element = Database.bundleGroups[_groupIndex].Bundles[_elementIndex];
                // Play.
            }
        }

        private void DrawCoreScene()
        {

        }

        // -----------------------

        private bool DrawGroup(SceneHandler.Group[] _groups, int _index, int _length,
                               Func<int, int, bool> _isElementVisible, Func<int, int, bool> _isElementLoaded, Func<int, int, string> _getElementName,
                               Action<int, int> _onOpen, Action<int, int> _onAdd, Action<int, int> _onClose, Action<int, int> _onPlay)
        {
            bool _isDeleted = false;
            GUILayout.Space(5f);

            // Group header and buttons.
            Rect _groupPosition = EditorGUILayout.GetControlRect(true, 25f);
            {
                Rect _temp = new Rect(_groupPosition)
                {
                    x = _groupPosition.x + 7f,
                    y = _groupPosition.y + 3f,
                    height = EditorGUIUtility.singleLineHeight,
                    xMax = _groupPosition.xMax - (MoveUpButtonWidth + MoveDownButtonWidth + EnhancedEditorGUIUtility.IconWidth + 10f + 10f + 5f + 1f)
                };

                if (_index == 0)
                {
                    EnhancedEditorGUI.UnderlinedLabel(_temp, _groups[_index].Name, EditorStyles.boldLabel);
                }
                else
                {
                    _groups[_index].Name = EditorGUI.TextField(_temp, _groups[_index].Name, EditorStyles.boldLabel);

                    _temp.x += _temp.width + 10f;
                    _temp.y += 2f;
                    _temp.width = MoveUpButtonWidth;

                    // Move buttons.
                    if (GUI.Button(_temp, moveUpGUI, EditorStyles.miniButtonLeft))
                    {
                        int _destinationIndex = Mathf.Max(1, _index - 1);
                        ArrayUtility.Move(_groups, _index, _destinationIndex);
                    }

                    _temp.x += _temp.width;
                    _temp.width = MoveDownButtonWidth;

                    if (GUI.Button(_temp, moveDownGUI, EditorStyles.miniButtonRight))
                    {
                        int _destinationIndex = Mathf.Min(_groups.Length - 1, _index + 1);
                        ArrayUtility.Move(_groups, _index, _destinationIndex);
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
                    if (GUI.Button(_temp, openSceneGUI))
                        _onOpen(_index, _i);
                }

                _temp.x += _temp.width + 2f;
                _temp.width = AddCloseButtonWidth;

                // Add / Close button.
                using (var _scope = EnhancedGUI.GUIColor.Scope(_color))
                {
                    if (_isLoaded)
                    {
                        if (GUI.Button(_temp, closeSceneGUI))
                            _onClose(_index, _i);
                    }
                    else if (GUI.Button(_temp, addSceneGUI))
                        _onAdd(_index, _i);
                }

                _temp.x += _temp.width + 5f;
                _temp.xMax = _position.xMax - (PlayButtonWidth + EnhancedEditorGUIUtility.IconWidth + 10f);

                // Element name.
                string _elementName = _getElementName(_index, _i);
                EditorGUI.LabelField(_temp, _elementName);

                _temp.x += _temp.width + EnhancedEditorGUIUtility.IconWidth + 5f;
                _temp.width = PlayButtonWidth;

                // Play button.
                if (EnhancedEditorGUI.IconButton(_temp, playSceneGUI))
                    _onPlay(_index, _i);

                _temp.x -= 17f;
                _temp.width = EnhancedEditorGUIUtility.IconWidth;

                // Menu button.
                if (EditorGUI.DropdownButton(_temp, menuGUI, FocusType.Passive, EditorStyles.label))
                {
                    GenericMenu _menu = new GenericMenu();
                    _menu.AddItem(new GUIContent("Select Asset"), false, () => { });

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
            foreach (var _group in Database.sceneGroups)
            {
                bool _isGroupVisible = false;

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
            foreach (var _group in Database.bundleGroups)
            {
                bool _isGroupVisible = false;

                foreach (var _scene in _group.Bundles)
                {
                    bool _isVisible = _scene.SceneBundle.name.ToLower().Contains(_searchFilter);
                    _scene.IsVisible = _isVisible;

                    if (_isVisible)
                        _isGroupVisible = true;
                }

                _group.IsVisible = _isGroupVisible;
            }
        }
        #endregion

        #region Scene Management Delegates
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

        private void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            Database.UpdateLoadedScenes();
        }
        #endregion

        #region Core Scene
        // -------------------------------------------
        // Core Scene
        // -------------------------------------------

        private const float EnableWidth = 50;
        private const float ToggleWidth = 15;
        private const float HelpBoxHeight = 36;

        private readonly GUIContent coreScenePathGUI = new GUIContent("Core scene path", "Path of the scene to load when entering play mode");
        private readonly GUIContent isEnableGUI = new GUIContent("Enable", "Is the core scene loading enable");

        /// <summary>
        /// ID used do display a help box at the bottom of the window :
        /// 
        /// • -1  :  Registration Error
        /// • 0   :  Copy path info
        /// • 1   :  Autoload scene registered
        /// • 2   :  Scene autoload disabled
        /// </summary>
        private int messageID = 0;

        // -----------------------


        /// <summary>
        /// This method has two purposes:
        /// 
        /// • First, when Unity starts, set registered core scene if one,
        /// as Unity do not keep it in cache.
        /// 
        /// • Second, if the associated option is enabled, when entering play mode,
        /// register active scenes to load them as soon as loading ends.
        /// 
        /// Sene loading cannot be performed before entering play mode,
        /// and once in, active scene informations will not be available anymore.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialization()
        {
            // When entering play mode, register active scenes path to load them,
            // if the associated option is enabled.
            /*if (EditorApplication.isPlayingOrWillChangePlaymode && LoadSceneHandlerSettings(out SceneHandler _settings)
                && _settings.IsCoreSceneEnabled && !IsCoreSceneLoaded(out int _coreIndex))
            {
                EditorApplication.ExitPlaymode();
                EditorApplication.playModeStateChanged += (mode) =>
                {
                    // First, open core scene now play mode has been exited.
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_settings.CoreScene), OpenSceneMode.Additive);

                    Scene _coreScene = EditorSceneManager.GetSceneAt(_coreIndex);
                    int _sceneCount = EditorSceneManager.sceneCount;

                    // Reorder scenes to set Core one on top of the hierarchy,
                    // and set first following scene as active one.
                    if (_sceneCount > 1)
                    {
                        for (int _i = 0; _i < _sceneCount; _i++)
                        {
                            Scene _scene = EditorSceneManager.GetSceneAt(_i);
                            if (_scene.path != _coreScene.path && _coreIndex > _i)
                            {
                                EditorSceneManager.MoveSceneBefore(_coreScene, _scene);
                                _coreIndex = _i;
                            }
                        }
                    }

                    EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneAt(0));

                    // Finally, re-enter play mode.
                    EditorApplication.EnterPlaymode();
                };
            }

            // ----- Local Method ----- //

            bool IsCoreSceneLoaded(out int _index)
            {
                for (_index = 0; _index < EditorSceneManager.sceneCount; _index++)
                {
                    if (EditorSceneManager.GetSceneAt(_index).name == _settings.CoreScene.name)
                        return true;
                }

                return false;
            }*/
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
