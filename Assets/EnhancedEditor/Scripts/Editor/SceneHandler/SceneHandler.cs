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
    public class SceneHandler : EditorWindow
    {
        #region Window GUI
        /// <summary>
        /// Get currently opened Scene Handler or create a new one.
        /// </summary>
        [MenuItem("Enhanced Editor/Scene Handler", false, 110)]
        public static void Get() => GetWindow<SceneHandler>("Scene Handler", true).Show();

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float IconWidth = 25;
        private const float RefreshButtonWidth = 55;
        private const float ToolbarWidth = 200;

        private const float ToolbarHeight = 25;
        private const float RectHeight = 20;
        private const float Spacing = 10;

        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh and load new project scenes");
        private readonly GUIContent[] toolbar = new GUIContent[]
                                                    {
                                                        new GUIContent("Scene Loader", "Utilies to manages loaded scenes in project"),
                                                        new GUIContent("Core Scene", "Manages project core scene behaviour")
                                                    };

        private readonly float toolbarHeight = EditorGUIUtility.singleLineHeight + 2;

        private readonly AutoManagedResource<SceneHandlerSettings> resource = new AutoManagedResource<SceneHandlerSettings>();

        private SceneHandlerSettings settings => resource.GetResource();
        private SceneData[] sceneDatas = new SceneData[] { };

        private Vector2 scroll = new Vector2();
        private float scrollHeight = 0;
        private int toolbarIndex = 0;

        // -----------------------

        private void OnEnable()
        {
            // Refresh scenes.
            sceneDatas = settings.RefreshScenes();

            // Initialize icons & menu.
            tokenIcons = new GUIContent[]
            {
                EditorGUIUtility.IconContent("FreeToken.png"),
                EditorGUIUtility.IconContent("AlexisToken.png"),
                EditorGUIUtility.IconContent("LucasToken.png"),
                EditorGUIUtility.IconContent("WilliamToken.png"),
                EditorGUIUtility.IconContent("MatthieuToken.png"),
            };

            tagIcon = EditorGUIUtility.IconContent("d_FilterByLabel", "Scene Tag");
            dropdownIcon = EditorGUIUtility.IconContent("d_icon dropdown", "Scene Tag");
            loadedIcon = EditorGUIUtility.IconContent("d_FilterSelectedOnly", "Loaded scene");

            openSceneMenu = new GenericMenu();
            openSceneMenu.AddItem(new GUIContent("Single"), false, () => OpenScene(OpenSceneMode.Single));
            openSceneMenu.AddItem(new GUIContent("Additive"), false, () => OpenScene(OpenSceneMode.Additive));
        }

        // Add tags to this thing.
        private void OnGUI()
        {
            // Initialize GUIStyles if not loaded yet.
            if (!areStylesInitialized)
            {
                areStylesInitialized = true;

                tagStyle = new GUIStyle(EditorStyles.boldLabel);
                tagStyle.fontSize = TagStyleFontSize;

                sceneLabelStyle = new GUIStyle(GUI.skin.button);
                sceneLabelStyle.alignment = TextAnchor.MiddleLeft;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.GetControlRect();
            EditorGUILayout.EndHorizontal();

            // Refresh button.
            Rect _rect = new Rect(0, 0, RefreshButtonWidth, toolbarHeight);
            if (GUI.Button(_rect, refreshGUI, EditorStyles.toolbarButton))
            {
                sceneDatas = settings.RefreshScenes();
            }

            // Scroll initialization.
            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Tab toolbar.
            _rect = EditorGUILayout.GetControlRect(true, ToolbarHeight);
            _rect.width += 6;
            float _scrollStartY = _rect.yMax;

            toolbarIndex = GUI.Toolbar(new Rect((_rect.width - ToolbarWidth + 2) / 2f, _rect.y, ToolbarWidth, _rect.height), toolbarIndex, toolbar);

            // Draw selected tab.
            _rect.y += _rect.height + Spacing;
            _rect.height = RectHeight;

            switch (toolbarIndex)
            {
                case 0:
                    DrawSceneLoader(ref _rect);
                    break;

                case 1:
                    DrawCoreScene(ref _rect);
                    break;

                default:
                    break;
            }

            // Ends scroll rect.
            // Use only repaint event as Layout and Ignore do not calculate rect
            // and distort double columns height.
            if (Event.current.type == EventType.Repaint)
                scrollHeight = _rect.yMax - _scrollStartY;

            EditorGUILayout.GetControlRect(false, scrollHeight);
            EditorGUILayout.EndScrollView();

            // Repaint for custom button mouse over.
            Repaint();
        }
        #endregion

        #region Scene Loader
        // -------------------------------------------
        // Scene Loader
        // -------------------------------------------

        private const float LoadedIconWidth = 20;
        private const float DropdownIconWidth = 15;
        private const float TokenIconWidth = 75;

        private const float SceneHeightMargin = 3;

        private const float DoubleColumnWidth = 700;
        private const float SceneLabelWidth = 150;
        private const float SceneButtonWidth = 70;
        private const float SceneButtonMargin = 11;

        private const int TagStyleFontSize = 13;

        private readonly GUIContent createTagGUI = new GUIContent("Create Tag", "Create a new tag and assign it to this scene");
        private readonly GUIContent openSceneGUI = new GUIContent("OPEN", "Load this scene");
        private readonly GUIContent closeSceneGUI = new GUIContent("CLOSE", "Close this scene");

        private readonly Color oddColor = EnhancedEditorGUIUtility.GUIOddColor;
        private readonly Color sceneLabelColor = new Color(.7f, .7f, .7f);
        private readonly Color openSceneColor = new Color(.35f, .9f, .35f);
        private readonly Color closeSceneColor = new Color(.9f, .35f, .35f);
        private readonly Color loadedColor = new Color(.0f, 1f, .0f);

        private GUIContent[] tokenIcons = new GUIContent[] { };
        private GUIContent tagIcon = null;
        private GUIContent dropdownIcon = null;
        private GUIContent loadedIcon = null;

        private GUIStyle tagStyle = null;
        private GUIStyle sceneLabelStyle = null;
        private bool areStylesInitialized = false;

        private GenericMenu openSceneMenu = null;
        private int openSceneIndex = 0;

        // -----------------------

        private void DrawSceneLoader(ref Rect _rect)
        {
            // Double column rect utility variables.
            bool _isDoubleColumn = _rect.width > DoubleColumnWidth;
            bool _isDouble = true;
            bool _isOdd = false;
            float _columnStartY = _rect.y;

            // Draw each scene informations.
            string _actualTag = SceneData.DefaultTag;
            for (int _i = 0; _i < sceneDatas.Length; _i++)
            {
                // Draw tag name if not default one.
                string _tag = sceneDatas[_i].Tag;
                if (_tag != _actualTag)
                {
                    // Spacing from previous category.
                    if (_i > 0)
                    {
                        // Double column break.
                        if (_isDoubleColumn)
                        {
                            Rect _separatorRect = new Rect(_rect.width / 2f, _columnStartY, 1, _rect.yMax - _columnStartY);
                            if (_isDouble)
                                _separatorRect.height -= _rect.height + (SceneHeightMargin * 2);
                            else
                                _rect.y += _rect.height + (SceneHeightMargin * 2);

                            EditorGUI.DrawRect(_separatorRect, SuperColor.SmokyBlack.Get());
                        }

                        _rect.y += Spacing;
                    }

                    _actualTag = _tag;
                    _rect.x = Spacing;

                    EditorGUI.LabelField(_rect, _actualTag, tagStyle);
                    _rect.y += Spacing * 2.5f;

                    // Update double column settings.
                    if (_isDoubleColumn)
                    {
                        _isDouble = true;
                        _isOdd = false;
                        _columnStartY = _rect.y;
                    }
                }

                // Draw scene within rect depending on state.
                if (_isDoubleColumn)
                {
                    float _midScreen = _rect.width * .5f;
                    _isDouble = !_isDouble;
                    if (!_isDouble)
                        _isOdd = !_isOdd;

                    if (_isDouble)
                    {
                        // Set alignment to right.
                        TextAnchor _previous = EditorStyles.label.alignment;
                        EditorStyles.label.alignment = TextAnchor.MiddleRight;
                        sceneLabelStyle.alignment = TextAnchor.MiddleRight;

                        // Draw scene at window right side.
                        DrawScene(_i,
                              new Rect(_rect.width - IconWidth - 5, _rect.y, IconWidth, _rect.height),
                              new Rect(_rect.width - IconWidth - SceneLabelWidth - (Spacing * .5f), _rect.y, SceneLabelWidth, _rect.height),
                              new Rect(_rect.width - IconWidth - SceneLabelWidth - TokenIconWidth - (Spacing * 1.5f), _rect.y, TokenIconWidth, _rect.height),
                              new Rect(_midScreen + Spacing, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_midScreen + Spacing - SceneButtonMargin, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_midScreen + SceneButtonWidth + (Spacing * 1.5f), _rect.y, LoadedIconWidth, _rect.height),
                              new Rect(_midScreen + Spacing * 1.5f, _rect.y, DropdownIconWidth, _rect.height));

                        if (_i < sceneDatas.Length - 1)
                            _rect.y += _rect.height + (SceneHeightMargin * 2);

                        // Reset alignment.
                        EditorStyles.label.alignment = _previous;
                        sceneLabelStyle.alignment = _previous;
                    }
                    else
                    {
                        // Odd color background.
                        if (_isOdd)
                            EditorGUI.DrawRect(new Rect(0, _rect.y - SceneHeightMargin, _rect.width, _rect.height + (SceneHeightMargin * 2)), oddColor);

                        // Draw scene at window left side.
                        DrawScene(_i,
                              new Rect(5, _rect.y, IconWidth, _rect.height),
                              new Rect(IconWidth + (Spacing * .5f), _rect.y, SceneLabelWidth, _rect.height),
                              new Rect(IconWidth + SceneLabelWidth + (Spacing * 1.5f), _rect.y, TokenIconWidth, _rect.height),
                              new Rect(_midScreen - SceneButtonWidth - Spacing, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_midScreen - SceneButtonWidth - Spacing + SceneButtonMargin, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_midScreen - SceneButtonWidth - LoadedIconWidth - (Spacing * 1.5f), _rect.y, LoadedIconWidth, _rect.height),
                              new Rect(_midScreen - DropdownIconWidth - (Spacing * 1.5f), _rect.y, DropdownIconWidth, _rect.height));
                    }
                }
                else
                {
                    // Odd color background.
                    if ((_i % 2) == 1)
                        EditorGUI.DrawRect(new Rect(0, _rect.y - SceneHeightMargin, _rect.width, _rect.height + (SceneHeightMargin * 2)), oddColor);

                    // Draw scene all window long.
                    DrawScene(_i,
                              new Rect(5, _rect.y, IconWidth, _rect.height),
                              new Rect(IconWidth + (Spacing * .5f), _rect.y, SceneLabelWidth, _rect.height),
                              new Rect(IconWidth + SceneLabelWidth + (Spacing * 1.5f), _rect.y, TokenIconWidth, _rect.height),
                              new Rect(_rect.width - SceneButtonWidth - Spacing, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_rect.width - SceneButtonWidth - Spacing + SceneButtonMargin, _rect.y, SceneButtonWidth, _rect.height),
                              new Rect(_rect.width - SceneButtonWidth - LoadedIconWidth - (Spacing * 1.5f), _rect.y, LoadedIconWidth, _rect.height),
                              new Rect(_rect.width - DropdownIconWidth - (Spacing * 1.5f), _rect.y, DropdownIconWidth, _rect.height));

                    if (_i < sceneDatas.Length - 1)
                        _rect.y += _rect.height + (SceneHeightMargin * 2);
                }
            }

            // Column separator.
            if (_isDoubleColumn)
                EditorGUI.DrawRect(new Rect(_rect.width / 2f, _columnStartY, 1, _rect.yMax - _columnStartY), SuperColor.SmokyBlack.Get());
        }

        public void DrawScene(int _sceneIndex, Rect _tagRect, Rect _labelRect, Rect _tokenRect, Rect _buttonRect, Rect _buttonLabelRect, Rect _loadedRect, Rect _dropdownRect)
        {
            SceneData _scene = sceneDatas[_sceneIndex];

            // Tag button.
            EnhancedEditorGUIUtility.PushGUIColor(_tagRect.Contains(Event.current.mousePosition) ? SuperColor.Silver.Get() : GUI.color);
            if (GUI.Button(_tagRect, tagIcon, EditorStyles.label))
            {
                // Open menu to set tag.
                GenericMenu _tagMenu = new GenericMenu();
                for (int _j = 0; _j < settings.TagsCount; _j++)
                {
                    string _tag = settings.GetTag(_j);
                    if (_tag != _scene.Tag)
                    {
                        int _index = _j;
                        _tagMenu.AddItem(new GUIContent(_tag), false, () => settings.SetTag(_scene, settings.GetTag(_index)));
                    }
                }

                _tagMenu.AddSeparator(string.Empty);
                _tagMenu.AddItem(createTagGUI, false, () => CreateSceneTagWindow.GetWindow(settings, _scene));
                _tagMenu.ShowAsContext();
            }
            EnhancedEditorGUIUtility.PopGUIColor();

            // Scene label.
            EnhancedEditorGUIUtility.PushGUIColor(sceneLabelColor);
            EditorGUI.LabelField(_labelRect, _scene.Name, sceneLabelStyle);
            EnhancedEditorGUIUtility.PopGUIColor();

            // Draw token here.
            if (GUI.Button(_tokenRect, tokenIcons[(int)_scene.Token], EditorStyles.label))
            {
                // Open menu to set tag.
                GenericMenu _tagMenu = new GenericMenu();
                for (int _j = 0; _j < Enum.GetValues(typeof(SceneToken)).Length; _j++)
                {
                    SceneToken _value = (SceneToken)_j;
                    _tagMenu.AddItem(new GUIContent(_value.ToString()), _scene.Token == _value, () => settings.SetToken(_scene, _value));
                }

                _tagMenu.DropDown(_tokenRect);
            }

            // Loaded scene informations:
            //   • Draw a checkmark as visual feedback
            //   • Button to close the scene.
            if (_scene.IsLoaded)
            {
                EnhancedEditorGUIUtility.PushGUIColor(closeSceneColor);
                if (GUI.Button(_buttonRect, closeSceneGUI))
                {
                    EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(AssetDatabase.GUIDToAssetPath(_scene.GUID)), true);
                    _scene.IsLoaded = false;
                }
                EnhancedEditorGUIUtility.PopGUIColor();

                EnhancedEditorGUIUtility.PushGUIColor(loadedColor);
                EditorGUI.LabelField(_loadedRect, loadedIcon);
                EnhancedEditorGUIUtility.PopGUIColor();
            }
            else
            {
                // Button to load (additively or destructively) the scene,
                // with a dropdown icon next to it.
                EnhancedEditorGUIUtility.PushGUIColor(openSceneColor);
                if (GUI.Button(_buttonRect, GUIContent.none))
                {
                    openSceneIndex = _sceneIndex;
                    openSceneMenu.DropDown(_buttonRect);
                }

                EditorGUI.LabelField(_buttonLabelRect, openSceneGUI);
                EditorGUI.LabelField(_dropdownRect, dropdownIcon);
                EnhancedEditorGUIUtility.PopGUIColor();
            }
        }

        // -----------------------

        private void OpenScene(OpenSceneMode _mode)
        {
            if (_mode == OpenSceneMode.Single)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                for (int _i = 0; _i < sceneDatas.Length; _i++)
                {
                    if (_i != openSceneIndex)
                        sceneDatas[_i].IsLoaded = false;
                }
            }

            EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(sceneDatas[openSceneIndex].GUID), _mode);
            sceneDatas[openSceneIndex].IsLoaded = true;
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
        
        private void DrawCoreScene(ref Rect _rect)
        {
            // Top right window - Enable toggle.
            EditorGUI.LabelField(new Rect(_rect.width - EnableWidth - ToggleWidth - Spacing, _rect.y, EnableWidth, _rect.height), isEnableGUI);
            bool _enable = EditorGUI.Toggle(new Rect(_rect.width - ToggleWidth - Spacing, _rect.y, ToggleWidth, _rect.height), settings.IsEnable);
            if (_enable != settings.IsEnable)
            {
                if (!_enable)
                {
                    messageID = 2;
                }
                else if (settings.CoreScene)
                {
                    messageID = 1;
                }
                else
                    messageID = -1;

                settings.EnableCoreScene(_enable);
            }

            _rect.x = Spacing;
            _rect.y += _rect.height + (Spacing * .5f);
            _rect.width = _rect.width - (Spacing * 2);

            // Scene registration.
            EditorGUI.BeginChangeCheck();
            SceneAsset _coreScene = (SceneAsset)EditorGUI.ObjectField(_rect, coreScenePathGUI, settings.CoreScene, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
                settings.SetCoreScene(_coreScene);

            _rect.y += _rect.height + (Spacing * .5f);
            _rect.height = HelpBoxHeight;

            // Display scene load informative message.
            switch (messageID)
            {
                case -1:
                    EditorGUI.HelpBox(_rect, "Core scene is enabled but no scene is assigned.",
                                             UnityEditor.MessageType.Warning);
                    break;

                case 1:
                    EditorGUI.HelpBox(_rect, "Core scene successfully loaded.",
                                             UnityEditor.MessageType.Info);
                    break;

                case 2:
                    EditorGUI.HelpBox(_rect, "Core scene disabled.",
                                             UnityEditor.MessageType.Info);
                    break;

                // Display nothing.
                default:
                    break;
            }
        }

        // -----------------------

        /// <summary>
        /// This method has two purposes:
        /// 
        /// • First, when Unity starts, set registered core scene if one,
        /// as Unity do not keep it in cache.
        /// 
        /// • Second, if the associated option is enabled, when entering play mode,
        /// register active scenes to load them as soon as loading ends.

        /// Sene loading cannot be performed before entering play mode,
        /// and once in, active scene informations will not be available anymore.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialization()
        {
            // When entering play mode, register active scenes path to load them,
            // if the associated option is enabled.
            if (EditorApplication.isPlayingOrWillChangePlaymode && LoadSceneHandlerSettings(out SceneHandlerSettings _settings)
                && _settings.IsEnable && !IsCoreSceneLoaded(out int _coreIndex))
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
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Loads project <see cref="SceneHandlerSettings"/> asset.
        /// </summary>
        private static bool LoadSceneHandlerSettings(out SceneHandlerSettings _settings)
        {
            SceneHandlerSettings[] _allSettings = EnhancedEditorUtility.LoadAssets<SceneHandlerSettings>();
            if (_allSettings.Length == 0)
            {
                _settings = null;
                return false;
            }

            _settings = _allSettings[0];
            return true;
        }
        #endregion

        #region Create Tag Window
        private class CreateSceneTagWindow : EditorWindow
        {
            SceneHandlerSettings settings = null;
            SceneData scene = null;
            string tagValue = "New Tag";

            // -----------------------

            public static void GetWindow(SceneHandlerSettings _settings, SceneData _scene)
            {
                CreateSceneTagWindow _window = GetWindow<CreateSceneTagWindow>(true, "New Scene Tag", true);
                _window.settings = _settings;
                _window.scene = _scene;

                _window.minSize = new Vector2(250, 70);
                _window.maxSize = new Vector2(250, 70);

                _window.ShowUtility();
            }

            private void OnGUI()
            {
                Rect _rect = new Rect(5, 5, 40, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_rect, "Tag:");
                _rect.x += 50;

                _rect.width = position.width - _rect.x - 5;
                tagValue = EditorGUI.TextField(_rect, tagValue);

                string _value = tagValue.Trim();
                if (string.IsNullOrEmpty(_value))
                {
                    _rect.x = 5;
                    _rect.y += _rect.height + 5;
                    _rect.height = 35;
                    _rect.width = position.width - 10;

                    EditorGUI.HelpBox(_rect, "Tag cannot be null or empty!", UnityEditor.MessageType.Error);
                }
                else if (settings.DoesTagExist(_value))
                {
                    _rect.x = 5;
                    _rect.y += _rect.height + 5;
                    _rect.height = 35;
                    _rect.width = position.width - 10;

                    EditorGUI.HelpBox(_rect, "Similar Tag already exist.", UnityEditor.MessageType.Error);
                }
                else
                {
                    _rect.x = position.width - 55;
                    _rect.y += _rect.height + 10;
                    _rect.width = 50;
                    _rect.height = 25;

                    if (GUI.Button(_rect, "OK"))
                    {
                        settings.AddTag(scene, _value);
                        Close();
                    }
                }
            }
        }
        #endregion
    }
}
