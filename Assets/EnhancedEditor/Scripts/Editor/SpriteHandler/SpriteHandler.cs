// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    public class SpriteHandler : EditorWindow
    {
        #region Fields
        private const string EditorPrefsKey = "SpritesEditorPrefs";

        [SerializeField] private int mode = 0;
        private readonly GUIContent[] modes = new GUIContent[]
                                                {
                                                new GUIContent("General", "General sprite parameters helper."),
                                                new GUIContent("Color Manager", "Manage all loaded scene(s) sprites color."),
                                                new GUIContent("Sprite Scraper", "Create new sprites from multiple ones.")
                                                };
        #endregion

        #region Handler Systems
        [MenuItem("Enhanced Editor/Sprite Handler", false, 120)]
        public static void GetWindow() => GetWindow<SpriteHandler>("Sprite Handler").Show();

        // -------------------------------------------
        // General Mode
        // -------------------------------------------

        private const string DescriptionMessage = "If you select a GameObject in the scene hierarchy, the following commands will affect all its SpriteRenderers" +
                                                    "(with the ones in children, if the associated option is enabled) ; pretty useful to increment multiple sprites" +
                                                    "order in layer at once.";

        private const string SortingOrderUndoName = "Increment Sprites Sorting Order";

        private readonly GUIContent childrenSpritesGUI = new GUIContent("Use Children Sprites", "Should the commands automatically affect selected GameObject(s)" +
                                                                               "children sprites or not.");
        private readonly GUIContent sortingOrderIncrementGUI = new GUIContent("Sorting Order Increment value", "Value used to increment sprites and sprite masks" +
                                                                               "sorting order.");
        private readonly GUIContent incrementButtonGUI = new GUIContent("Increment Sorting Order", "Increment selected GameObject(s)" +
                                                                               "related sprites and sprite masks sorting order.");

        [SerializeField] private bool doUseChildrenSprites = true;
        [SerializeField] private int sortingOrderIncrementValue = 0;

        // ----------------

        public void DrawGeneralMode()
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox(DescriptionMessage, UnityEditor.MessageType.Info, true);

            doUseChildrenSprites = EditorGUILayout.Toggle(childrenSpritesGUI, doUseChildrenSprites);
            GUILayout.Space(10);
            sortingOrderIncrementValue = EditorGUILayout.IntField(sortingOrderIncrementGUI, sortingOrderIncrementValue, GUILayout.Width(200));

            // Increment button.
            if ((Selection.gameObjects.Length > 0) && GUILayout.Button(incrementButtonGUI, GUILayout.Height(25), GUILayout.Width(200)))
            {
                List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
                List<SpriteMask> _masks = new List<SpriteMask>();
                if (doUseChildrenSprites)
                {
                    for (int _i = 0; _i < Selection.gameObjects.Length; _i++)
                    {
                        _sprites.AddRange(Selection.gameObjects[_i].GetComponentsInChildren<SpriteRenderer>());
                        _masks.AddRange(Selection.gameObjects[_i].GetComponentsInChildren<SpriteMask>());
                    }
                }
                else
                {
                    for (int _i = 0; _i < Selection.gameObjects.Length; _i++)
                    {
                        _sprites.AddRange(Selection.gameObjects[_i].GetComponents<SpriteRenderer>());
                        _masks.AddRange(Selection.gameObjects[_i].GetComponents<SpriteMask>());
                    }
                }

                // Increment all sprites and masks sorting order.
                if (_sprites.Count > 0)
                {
                    Undo.RecordObjects(_sprites.ToArray(), SortingOrderUndoName);
                    for (int _i = 0; _i < _sprites.Count; _i++)
                    {
                        _sprites[_i].sortingOrder += sortingOrderIncrementValue;
                    }
                }

                if (_masks.Count > 0)
                {
                    Undo.RecordObjects(_masks.ToArray(), SortingOrderUndoName);
                    for (int _i = 0; _i < _masks.Count; _i++)
                    {
                        _masks[_i].backSortingOrder += sortingOrderIncrementValue;
                        _masks[_i].frontSortingOrder += sortingOrderIncrementValue;
                    }
                }
            }
        }

        // -------------------------------------------
        // Color Managemer
        // -------------------------------------------

        private const string ResetMessage = "Are you sure you want to reset all of your color groups ? This action cannot be undone.";
        private const string LoadMessage = "Are you sure you want to load saved color groups ? Actual color groups will be override ; this action cannot be undone.";

        private readonly GUIContent rangeGUI = new GUIContent("Range", "Range loaded groups by name.");
        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Load open scenes missing sprites.");
        private readonly GUIContent resetGUI = new GUIContent("Reset", "Clear all groups and reload all sprites from zero point.");
        private readonly GUIContent loadGUI = new GUIContent("Load", "Load saved color groups of opened scene");
        private readonly GUIContent saveGUI = new GUIContent("Save", "Save actual color groups");

        private int selectedGroup = -1;
        private bool isInFusionMode = false;
        private ColorGroup[] colorGroups = new ColorGroup[] { };

        [SerializeField] private List<string> sceneGroups = new List<string>();
        [SerializeField] private List<string> sceneGroupsName = new List<string>();

        // ----------------

        public void DrawColorManager()
        {
            // Get non-assignated color groups and associate them with matching one if found
            for (int _i = 0; _i < colorGroups.Length; _i++)
            {
                colorGroups[_i].Sprites = colorGroups[_i].Sprites.Where(s => (s != null) && s.enabled).ToList();

                SpriteRenderer[] _differents = colorGroups[_i].Sprites.Where(s => s.color != colorGroups[_i].Color).ToArray();

                if (_differents.Length > 0)
                {
                    if (_differents.Length == colorGroups[_i].Sprites.Count)
                    {
                        colorGroups[_i].Color = _differents[0].color;
                    }

                    else
                    {
                        foreach (SpriteRenderer _sprite in _differents)
                        {
                            colorGroups[_i].Sprites.Remove(_sprite);
                            LoadSprite(_sprite);
                        }
                    }
                }
                if (colorGroups[_i].Sprites.Count == 0)
                {
                    colorGroups = colorGroups.Where(g => g != colorGroups[_i]).ToArray();
                    Repaint();
                }
            }

            // Draw color groups editor
            GUILayout.Space(5);

            EditorGUILayout.LabelField(new GUIContent("Level Colors", "All colors of the loaded sprites in the level"), EditorStyles.boldLabel);

            if (colorGroups.Length == 0)
            {
                GUILayout.Space(5);

                EditorGUILayout.HelpBox(new GUIContent("No sprites loaded !", "Click on the \"Load Sprites\" button to get sprites of the scene, or add some to load them next"));
                return;
            }

            // Draw all folders & color groups !!
            #region Color Groups
            ColorGroup _colorGroup;

            for (int _i = 0; _i < colorGroups.Length; _i++)
            {
                _colorGroup = colorGroups[_i];

                if (selectedGroup == _i) GUI.backgroundColor = new Color(.7f, .7f, .7f);
                else GUI.backgroundColor = Color.white;

                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal(selectedGroup == _i ? "HelpBox" : EditorStyles.inspectorDefaultMargins);

                // Draw fusion mode buttons
                if (!_colorGroup.isSelected && (isInFusionMode || (Selection.gameObjects.Length > 0)))
                {
                    Color _original = GUI.color;
                    GUI.color = isInFusionMode ? new Color(0, .75f, 0, 1) : new Color(.5f, .5f, .8f, 1);

                    if (GUILayout.Button("F", GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        if (isInFusionMode)
                        {
                            ColorGroup[] _groups = colorGroups.Where(g => g.isSelected).ToArray();

                            foreach (ColorGroup _group in _groups)
                            {
                                Undo.RecordObjects(_group.Sprites.ToArray(), "fusion sprite groups color");

                                foreach (SpriteRenderer _sprite in _group.Sprites)
                                {
                                    _sprite.color = _colorGroup.Color;
                                }
                                _colorGroup.Sprites.AddRange(_group.Sprites);
                            }

                            colorGroups = colorGroups.Except(_groups).ToArray();
                            isInFusionMode = false;

                            Repaint();
                        }
                        else
                        {
                            SpriteRenderer[] _sprites = Selection.gameObjects.ToList().SelectMany(s => doUseChildrenSprites ? s.GetComponentsInChildren<SpriteRenderer>() : s.GetComponents<SpriteRenderer>()).ToArray();

                            Undo.RecordObjects(_sprites, "fusion sprite groups color");

                            foreach (SpriteRenderer _sprite in _sprites)
                            {
                                _sprite.color = _colorGroup.Color;
                            }

                            _colorGroup.Sprites.AddRange(_sprites);
                        }
                    }

                    GUI.color = _original;
                }
                else GUILayout.Space(28);

                // Draw color group
                EditorGUI.BeginChangeCheck();
                _colorGroup.Name = EditorGUILayout.TextField(_colorGroup.Name, GUILayout.Width(250));
                if (EditorGUI.EndChangeCheck()) selectedGroup = _i;

                if (GUILayout.Button($"({_colorGroup.Sprites.Count})", EditorStyles.label, GUILayout.Width(50)))
                {
                    foreach (SpriteRenderer _sprite in _colorGroup.Sprites)
                    {
                        Debug.Log($"CG \"{_colorGroup.Name}\" => {_sprite.name}");
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                _colorGroup.Color = EditorGUILayout.ColorField(_colorGroup.Color);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedGroup = _i;

                    Undo.RecordObjects(_colorGroup.Sprites.ToArray(), "change sprites group color");
                    _colorGroup.Sprites.ForEach(s => s.color = _colorGroup.Color);
                }

                bool _selected = EditorGUILayout.Toggle(_colorGroup.isSelected, GUILayout.Width(15));
                if (_selected != _colorGroup.isSelected)
                {
                    _colorGroup.isSelected = _selected;
                    if (_selected)
                    {
                        if (!isInFusionMode) isInFusionMode = true;
                    }
                    else if (!colorGroups.Any(g => g.isSelected)) isInFusionMode = false;
                }

                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(10);
        }

        private void DrawColorManagerToolbar()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(rangeGUI, EditorStyles.toolbarButton))
            {
                colorGroups = colorGroups.OrderBy(g => g.Color.grayscale).ToArray();
                colorGroups = colorGroups.OrderBy(g => g.Name).ToArray();
            }

            if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton))
                LoadSprites();

            GUILayout.Space(15);

            // Reset
            if (GUILayout.Button(resetGUI, EditorStyles.toolbarButton) && EditorUtility.DisplayDialog(ConfirmDialogTitle, ResetMessage, ConfirmDialogOK, ConfirmDialogCancel))
            {
                colorGroups = new ColorGroup[] { };
                LoadSprites();
                Repaint();
            }

            GUILayout.Space(10);

            // Load
            if (GUILayout.Button(loadGUI, EditorStyles.toolbarButton) && EditorUtility.DisplayDialog(ConfirmDialogTitle, LoadMessage, ConfirmDialogOK, ConfirmDialogCancel))
            {
                string _sceneName = EditorSceneManager.GetActiveScene().name;
                for (int _i = 0; _i < sceneGroupsName.Count; _i++)
                {
                    if (sceneGroupsName[_i] == _sceneName)
                    {
                        string[] _groups = sceneGroups[_i].Split('\n');
                        string[] _groupColor = new string[] { };
                        string _groupName = string.Empty;

                        colorGroups = new ColorGroup[_groups.Length - 1];

                        for (int _j = 0; _j < _groups.Length - 1; _j++)
                        {
                            _groupName = _groups[_j].Split('|')[0];
                            _groupColor = _groups[_j].Split('|')[1].Split('#');

                            colorGroups[_j] = new ColorGroup(new Color(float.Parse(_groupColor[0]), float.Parse(_groupColor[1]), float.Parse(_groupColor[2]), float.Parse(_groupColor[3])), _groupName);
                        }

                        LoadSprites();
                        break;
                    }
                }
            }

            // Save
            if (GUILayout.Button(saveGUI, EditorStyles.toolbarButton))
            {
                string _file = string.Empty;
                foreach (ColorGroup _group in colorGroups)
                {
                    _file += $"{_group.Name}|{_group.Color.r}#{_group.Color.g}#{_group.Color.b}#{_group.Color.a}\n";
                }

                bool _isSaved = false;
                string _sceneName = EditorSceneManager.GetActiveScene().name;
                for (int _i = 0; _i < sceneGroupsName.Count; _i++)
                {
                    if (sceneGroupsName[_i] == _sceneName)
                    {
                        sceneGroups[_i] = _file;
                        _isSaved = true;
                        break;
                    }
                }

                if (!_isSaved)
                {
                    sceneGroupsName.Add(_sceneName);
                    sceneGroups.Add(_file);
                }
            }
        }

        // ----------------

        public void LoadSprites()
        {
            SpriteRenderer[] _sprites = FindObjectsOfType<SpriteRenderer>();
            foreach (SpriteRenderer _sprite in _sprites)
            {
                if (_sprite.enabled)
                    LoadSprite(_sprite);
            }
        }

        private void LoadSprite(SpriteRenderer _sprite)
        {
            ColorGroup _matching = colorGroups.Where(c => c.Color == _sprite.color).FirstOrDefault();
            if (_matching == null)
            {
                colorGroups = colorGroups.Append(new ColorGroup(_sprite.color, new SpriteRenderer[] { _sprite })).OrderBy(g => g.Name).ToArray();
            }
            else if (!_matching.Sprites.Contains(_sprite))
            {
                _matching.Sprites.Add(_sprite);
            }
        }

        // -------------------------------------------
        // Sprite Scraper
        // -------------------------------------------

        private const string TextureExtension = ".png";
        private const string SaveAsMenuPath = "File/Save As...";
        private const string BrowseIconName = "BrowseIcon.png";

        private const string CleanButton = "Clean";
        private const string CleanMessage = "Are you sure you want to clean your template list ?";

        private const string RemoveUnusedButton = "Remove Unused";
        private const string RemoveUnusedMessage = "Are you sure you want to remove all unused (disabled) templates from the list ?";

        private const string ConfirmDialogTitle = "Confirm this action";
        private const string ConfirmDialogOK = "Yes";
        private const string ConfirmDialogCancel = "Cancel";

        private const string TextureFolderPanel = "Select a folder where to save Sprite Scraper created textures.";
        private const string InvalidFolderMessage = "Invalid folder. Please select a directory from your project Assets folder.";
        private const string NoUnlitMessage = "No unlit material assign. Disabling light bake option will not be taken into account.";
        private const string NoTemplateMessage = "No template selected. You must assign a template to create sprite from !";

        private const int toggleWidth = 13;
        private const int tmpSeparator = 5;
        private const int tmpBakeWidth = 50;
        private const int tmpPOTWidth = 80;

        [SerializeField] private int scraperMode = 0;
        private readonly GUIContent[] scraperModes = new GUIContent[]
                                                                {
                                                                new GUIContent("New Scene", "Create a new scene with all templates replaced by new sprites."),
                                                                new GUIContent("Merge Scraps", "Create new sprites based on each template."),
                                                                new GUIContent("Merge (No Save)", "Create new sprite based on each template, but do not save textures " +
                                                                                                    "(Useful for test only).")
                                                                };

        private readonly GUIContent settingsGUI = new GUIContent("Settings", "Show / Hide sprite scraper settings.");
        private readonly GUIContent saveTextureFolderGUI = new GUIContent("Save Texture Folder", "Folder where to save sprite created textures.");
        private readonly GUIContent unlitMaterialGUI = new GUIContent("Unlit Material", "Material used when light bake option is disabled.");

        private readonly GUIContent templatesHeaderGUI = new GUIContent("Templates", "Sprite Scraper templates.");
        private readonly GUIContent templateRootParamGUI = new GUIContent("Template Root", "Root transform of the sprites to use as template");
        private readonly GUIContent bakeLightParam1GUI = new GUIContent("Bake", "Should lights be baked with template or not.");
        private readonly GUIContent bakeLightParam2GUI = new GUIContent("Lights", "Should lights be baked with template or not.");
        private readonly GUIContent PowerOfTwoParam1GUI = new GUIContent("Power of Two", "Should created sprite use a power of two dimensions texture or not" +
                                                                                            "(You can disable this if using the created texture in a sprite atlas).");
        private readonly GUIContent PowerOfTwoParam2GUI = new GUIContent("Texture", "Should created sprite use a power of two dimensions texture or not" +
                                                                                            "(You can disable this if using the created texture in a sprite atlas).");

        private readonly GUIContent layerParamGUI = new GUIContent("Layer", "Created sprite layer.");
        private readonly GUIContent orderInLayerParamGUI = new GUIContent("Order in Layer", "Created sprite order in layer.");
        private readonly GUIContent materialParamGUI = new GUIContent("Material", "Created sprite assigned material (Null if default).");

        private readonly GUIContent mergeScrapsGUI = new GUIContent("Merge Scraps", "Merge all templates scraps into new sprites according to selected mode.");

        private readonly Color separatorColor = new Color(.7f, .7f, .7f);
        private readonly Color disableElementColor = new Color(.4f, .4f, .4f, .5f);

        // Cannot initialize a GUIStyle outside of a OnGUI method,
        // so check if initialization is required or not.
        private bool areGUIStylesInitialized = false;
        private GUIStyle headerInfoStyle = null;

        private bool areSettingsUnfolded = false;
        private bool doDisplayInvalidFolderMessage = false;
        private bool doDisplayNoTemplateMessage = false;

        [SerializeField] private string saveTextureFolder = "Assets/";
        [SerializeField] private Material unlitMaterial = null;

        // Templates
        [SerializeField] private List<SpriteTemplate> templates = new List<SpriteTemplate>();
        private ReorderableList templatesList = null;

        // ----------------

        public void DrawSpriteScraper()
        {
            // GUIStyles initialization.
            if (!areGUIStylesInitialized)
            {
                areGUIStylesInitialized = true;

                headerInfoStyle = new GUIStyle(EditorStyles.label);
                headerInfoStyle.alignment = TextAnchor.MiddleCenter;
            }

            // Sprite Scraper mode
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            scraperMode = GUILayout.Toolbar(scraperMode, scraperModes, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents, GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            // Settings
            areSettingsUnfolded = EditorGUILayout.Foldout(areSettingsUnfolded, settingsGUI, true);
            if (areSettingsUnfolded)
            {
                EditorGUI.indentLevel++;

                // Save texture folder.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(saveTextureFolderGUI, saveTextureFolder);

                Rect _rect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
                _rect.Set
                (
                    _rect.x - 2,
                    _rect.y - 1,
                    _rect.width + 2,
                    _rect.height + 1
                );
                if (GUI.Button(_rect, GUIContent.none))
                {
                    string _folder = EditorUtility.OpenFolderPanel(TextureFolderPanel, string.Empty, string.Empty);
                    if (_folder.Contains(Application.dataPath))
                    {
                        doDisplayInvalidFolderMessage = false;
                        saveTextureFolder = _folder.Remove(0, Application.dataPath.Length - 6) + Path.AltDirectorySeparatorChar;
                    }
                    else
                        doDisplayInvalidFolderMessage = true;
                }

                _rect.width = 30;
                _rect.x -= (_rect.width / 2) - 2;
                _rect.y += 1;
                EditorGUI.LabelField(_rect, EditorGUIUtility.IconContent(BrowseIconName));

                EditorGUILayout.EndHorizontal();
                if (doDisplayInvalidFolderMessage)
                {
                    EditorGUILayout.HelpBox(InvalidFolderMessage, UnityEditor.MessageType.Error);
                }

                // Unlit material.
                unlitMaterial = (Material)EditorGUILayout.ObjectField(unlitMaterialGUI, unlitMaterial, typeof(Material), false);


                EditorGUI.indentLevel--;
            }

            if (!unlitMaterial)
            {
                EditorGUILayout.HelpBox(NoUnlitMessage, UnityEditor.MessageType.Warning);
            }

            GUILayout.Space(5);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.grey);
            GUILayout.Space(5);

            templatesList.DoLayoutList();

            // List cleaner buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CleanButton) && EditorUtility.DisplayDialog(ConfirmDialogTitle, CleanMessage, ConfirmDialogOK, ConfirmDialogCancel))
            {
                templates.Clear();
            }
            if (GUILayout.Button(RemoveUnusedButton) && EditorUtility.DisplayDialog(ConfirmDialogTitle, RemoveUnusedMessage, ConfirmDialogOK, ConfirmDialogCancel))
            {
                for (int _i = 0; _i < templates.Count; _i++)
                {
                    if (!templates[_i].IsSelected)
                    {
                        templates.RemoveAt(_i);
                        _i--;
                    }
                }
            }

            GUILayout.EndHorizontal();

            // Merge scraps into beautiful artworks.
            if (GUILayout.Button(mergeScrapsGUI, GUILayout.MaxWidth(100), GUILayout.Height(25)))
            {
                #region Scraper
                if (templates.Count > 0)
                {
                    doDisplayNoTemplateMessage = false;

                    // Create and configure camera used to capture templates.
                    Camera _renderCamera = new GameObject().AddComponent<Camera>();
                    Rect _viewport = new Rect(0, 0, 1, 1);
                    _renderCamera.orthographic = true;
                    _renderCamera.clearFlags = CameraClearFlags.SolidColor;
                    _renderCamera.backgroundColor = Color.clear;
                    _renderCamera.cullingMask = ~2;
                    _renderCamera.allowDynamicResolution = true;

                    Renderer[] _allRenderers = FindObjectsOfType<Renderer>();
                    GameObject[] _artworks = new GameObject[templates.Count];
                    int[] _artworksLayer = new int[templates.Count];

                    for (int _n = 0; _n < templates.Count; _n++)
                    {
                        if (!templates[_n].IsSelected || !templates[_n].Root)
                            continue;

                        SpriteTemplate _template = templates[_n];
                        SpriteRenderer[] _sprites = _template.Root.GetComponentsInChildren<SpriteRenderer>();
                        SpriteMask[] _masks = _template.Root.GetComponentsInChildren<SpriteMask>();

                        // Set template bounds volume entirely in camera view.
                        Bounds _bounds = new Bounds(_sprites[0].bounds.center, _sprites[0].bounds.size);
                        for (int _i = 1; _i < _sprites.Length; _i++)
                        {
                            _bounds.Encapsulate(_sprites[_i].bounds);
                        }

                        _renderCamera.orthographicSize = 5;
                        _renderCamera.transform.position = new Vector3(_bounds.center.x, _bounds.center.y, _bounds.center.z - 10);
                        while (!(_viewport.Contains(_renderCamera.WorldToViewportPoint(_bounds.min)) && _viewport.Contains(_renderCamera.WorldToViewportPoint(_bounds.max))))
                        {
                            _renderCamera.orthographicSize++;
                        }

                        // Set all visible renderers on a different layer (not visible anymore).
                        Plane[] _frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(_renderCamera);
                        Dictionary<GameObject, int> _renderers = new Dictionary<GameObject, int>();
                        for (int _i = 0; _i < _allRenderers.Length; _i++)
                        {
                            if (GeometryUtility.TestPlanesAABB(_frustrumPlanes, _allRenderers[_i].bounds) &&
                                !_sprites.Contains(_allRenderers[_i]) && !_masks.Contains(_allRenderers[_i]) &&
                                !_renderers.ContainsKey(_allRenderers[_i].gameObject))
                            {
                                _renderers.Add(_allRenderers[_i].gameObject, _allRenderers[_i].gameObject.layer);
                                _allRenderers[_i].gameObject.layer = 1;
                            }
                        }

                        // Get artwork texture information and capture template.
                        Vector2 _offset = _renderCamera.WorldToScreenPoint(_bounds.min);
                        Vector2 _boundsSize = (Vector2)_renderCamera.WorldToScreenPoint(_bounds.max) - _offset;
                        Vector2Int _size;
                        if (_template.DoUsePowerOfTwoTexture)
                        {
                            _size = new Vector2Int(Mathf.NextPowerOfTwo((int)_boundsSize.x), Mathf.NextPowerOfTwo((int)_boundsSize.y));
                        }
                        else
                            _size = new Vector2Int((int)_boundsSize.x, (int)_boundsSize.y);

                        RenderTexture _renderTexture = new RenderTexture(_renderCamera.pixelWidth, _renderCamera.pixelHeight, 32);
                        RenderTexture.active = _renderTexture;
                        _renderCamera.targetTexture = _renderTexture;

                        // Set template sprites material to unlit if not baking light.
                        Material[] _spritesMaterial = null;
                        bool _doNotBakeLight = !_template.DoBakeLights && unlitMaterial;
                        if (_doNotBakeLight)
                        {
                            _spritesMaterial = new Material[_sprites.Length];
                            for (int _i = 0; _i < _sprites.Length; _i++)
                            {
                                _spritesMaterial[_i] = _sprites[_i].sharedMaterial;
                                _sprites[_i].material = unlitMaterial;
                            }
                        }

                        _renderCamera.Render();

                        // Restore template sprites material.
                        if (_doNotBakeLight)
                        {
                            for (int _i = 0; _i < _sprites.Length; _i++)
                            {
                                _sprites[_i].material = _spritesMaterial[_i];
                            }
                        }

                        // Reset renderers layer.
                        foreach (KeyValuePair<GameObject, int> _renderer in _renderers)
                        {
                            _renderer.Key.layer = _renderer.Value;
                        }

                        // Create full transparent texture, then paint capture on it.
                        var _capture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false);
                        Color[] _colors = _capture.GetPixels();
                        for (int _i = 0; _i < _colors.Length; _i++)
                        {
                            _colors[_i] = Color.clear;
                        }
                        _capture.SetPixels(_colors);

                        // Reversed Y starting position
                        _capture.ReadPixels(new Rect(_offset.x, _renderCamera.pixelHeight - _offset.y - _boundsSize.y, _boundsSize.x, _boundsSize.y), 0, 0);
                        _capture.Apply();

                        // Create artwork based on template.
                        SpriteRenderer _artwork = new GameObject(_template.Root.name).AddComponent<SpriteRenderer>();
                        Vector2 _pivot = new Vector2((_boundsSize.x * .5f) / _size.x, 0);
                        float _pixelPerUnit = _renderCamera.pixelHeight / (_renderCamera.orthographicSize * 2f);

                        _artwork.gameObject.layer = 1;
                        _artwork.sortingOrder = _template.OrderInLayer;
                        _artwork.transform.position = new Vector3(_bounds.min.x + _bounds.extents.x, _bounds.min.y, _bounds.center.z);

                        if (_template.Material)
                            _artwork.material = _template.Material;

                        _artworks[_n] = _artwork.gameObject;
                        _artworksLayer[_n] = _template.Layer;

                        // Save texture according to selected mode.
                        if (scraperMode < 2)
                        {
                            string _path = Application.dataPath + saveTextureFolder.Remove(0, 6);
                            if (!Directory.Exists(_path))
                                Directory.CreateDirectory(_path);

                            // Get unique file path.
                            _path += _artwork.name + TextureExtension;
                            if (File.Exists(_path))
                            {
                                string _originalPath = _path.Insert(_path.Length - 4, "_");
                                int _number = 0;

                                do
                                {
                                    _path = _originalPath.Insert(_originalPath.Length - 4, _number.ToString());
                                    _number++;
                                } while (File.Exists(_path));
                            }

                            // Save texture as PNG.
                            File.WriteAllBytes(_path, _capture.EncodeToPNG());

                            _path = _path.Remove(0, Application.dataPath.Length - 6);

                            AssetDatabase.ImportAsset(_path, ImportAssetOptions.ForceUpdate);

                            TextureImporter _importer = (TextureImporter)AssetImporter.GetAtPath(_path);
                            TextureImporterSettings _settings = new TextureImporterSettings();

                            _importer.textureType = TextureImporterType.Sprite;
                            _importer.spritePixelsPerUnit = _pixelPerUnit;

                            _importer.ReadTextureSettings(_settings);
                            _settings.spriteAlignment = (int)SpriteAlignment.Custom;
                            _importer.SetTextureSettings(_settings);

                            _importer.spritePivot = _pivot;
                            _importer.SaveAndReimport();

                            _artwork.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_path);
                        }
                        else
                            _artwork.sprite = Sprite.Create(_capture, new Rect(0, 0, _size.x, _size.y), _pivot, _pixelPerUnit);
                    }

                    // Replace templates by artworks on new scene mode.
                    if (scraperMode == 0)
                    {
                        for (int _i = 0; _i < _artworks.Length; _i++)
                        {
                            if (_artworks[_i] && templates[_i].Root)
                            {
                                _artworks[_i].transform.SetParent(templates[_i].Root.parent);

                                try
                                {
                                    DestroyImmediate(templates[_i].Root.gameObject);
                                }
                                catch (InvalidOperationException)
                                {
                                    GameObject _prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(templates[_i].Root.gameObject);
                                    PrefabUtility.UnpackPrefabInstance(_prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                                    DestroyImmediate(templates[_i].Root.gameObject);
                                }
                            }
                        }

                        // Save scene as new one.
                        templates.Clear();
                        EditorApplication.ExecuteMenuItem(SaveAsMenuPath);
                    }

                    // Set artowrks layer.
                    for (int _i = 0; _i < _artworks.Length; _i++)
                    {
                        if (_artworks[_i])
                            _artworks[_i].layer = _artworksLayer[_i];
                    }

                    // Clean capture tools.
                    DestroyImmediate(_renderCamera.gameObject);
                    RenderTexture.active = null;
                }
                else
                    doDisplayNoTemplateMessage = true;
                #endregion
            }

            if (doDisplayNoTemplateMessage)
                EditorGUILayout.HelpBox(NoTemplateMessage, UnityEditor.MessageType.Error);
        }

        // ----------------

        private void DrawTemplatesHeader(Rect _rect)
        {
            Rect _fieldRect = new Rect()
            {
                x = -1,
                y = _rect.y + EditorGUIUtility.singleLineHeight,
                width = Screen.width,
                height = _rect.height - EditorGUIUtility.singleLineHeight + 5
            };
            // Draw header background color & separators.
            EditorGUI.LabelField(_fieldRect, GUIContent.none, ReorderableList.defaultBehaviours.boxBackground);


            _fieldRect.height = 1;
            EditorGUI.DrawRect(_fieldRect, separatorColor);
            _fieldRect.y = _rect.yMax;
            EditorGUI.DrawRect(_fieldRect, separatorColor);

            // Header
            _fieldRect.Set
            (
                _rect.x,
                _rect.y,
                _rect.width,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(_fieldRect, templatesHeaderGUI, EditorStyles.boldLabel);

            // Parameter infos
            _fieldRect.x = _rect.xMax - tmpPOTWidth - tmpSeparator;
            _fieldRect.y += EditorGUIUtility.singleLineHeight;
            _fieldRect.width = tmpPOTWidth;

            EditorGUI.LabelField(_fieldRect, PowerOfTwoParam1GUI, headerInfoStyle);
            _fieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(_fieldRect, PowerOfTwoParam2GUI, headerInfoStyle);

            _fieldRect.x -= tmpBakeWidth + tmpSeparator;
            _fieldRect.y -= EditorGUIUtility.singleLineHeight;
            _fieldRect.width = tmpBakeWidth;

            EditorGUI.LabelField(_fieldRect, bakeLightParam1GUI, headerInfoStyle);
            _fieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(_fieldRect, bakeLightParam2GUI, headerInfoStyle);

            _fieldRect.width = _fieldRect.x - (_rect.x + toggleWidth + tmpSeparator);
            _fieldRect.x = _fieldRect.x - _fieldRect.width;
            _fieldRect.y -= EditorGUIUtility.singleLineHeight * .5f;

            EditorGUI.LabelField(_fieldRect, templateRootParamGUI);
        }

        private void DrawTemplatesElement(Rect _rect, int _index, bool _isActive, bool _isFocus)
        {
            // Global settings.
            Rect _fieldRect = new Rect()
            {
                x = _rect.x,
                y = _rect.y,
                width = toggleWidth,
                height = EditorGUIUtility.singleLineHeight
            };
            templates[_index].IsSelected = EditorGUI.Toggle(_fieldRect, templates[_index].IsSelected);

            _fieldRect.x = _rect.xMax - tmpSeparator - ((tmpPOTWidth + toggleWidth) / 2f);
            templates[_index].DoUsePowerOfTwoTexture = EditorGUI.Toggle(_fieldRect, templates[_index].DoUsePowerOfTwoTexture);

            _fieldRect.x = _rect.xMax - tmpPOTWidth - (tmpSeparator * 2) - ((tmpBakeWidth + toggleWidth) / 2f);
            templates[_index].DoBakeLights = EditorGUI.Toggle(_fieldRect, templates[_index].DoBakeLights);

            _fieldRect.x = _rect.x + toggleWidth + tmpSeparator;
            _fieldRect.width = (_rect.xMax - tmpPOTWidth - tmpBakeWidth - (tmpSeparator * 3)) - _fieldRect.x;
            templates[_index].Root = (Transform)EditorGUI.ObjectField(_fieldRect, templates[_index].Root, typeof(Transform), true);

            // Additional settings.
            _fieldRect.x = _rect.xMax - toggleWidth - 3;
            _fieldRect.width = toggleWidth;

            templates[_index].AreSettingsUnfolded = EditorGUI.Foldout(_fieldRect, templates[_index].AreSettingsUnfolded, GUIContent.none);
            if (templates[_index].AreSettingsUnfolded)
            {
                _fieldRect.width = _rect.width * .8f;
                _fieldRect.x = _rect.xMax - _fieldRect.width;
                _fieldRect.y += EditorGUIUtility.singleLineHeight + 5;

                templates[_index].Layer = EditorGUI.LayerField(_fieldRect, layerParamGUI, templates[_index].Layer);
                _fieldRect.y += EditorGUIUtility.singleLineHeight;

                templates[_index].OrderInLayer = EditorGUI.IntField(_fieldRect, orderInLayerParamGUI, templates[_index].OrderInLayer);
                _fieldRect.y += EditorGUIUtility.singleLineHeight;

                templates[_index].Material = (Material)EditorGUI.ObjectField(_fieldRect, materialParamGUI, templates[_index].Material, typeof(Material), false);

                // Draw separator.
                _fieldRect.Set
                (
                    _rect.x + 17,
                    _rect.yMax - 3,
                    _rect.width - 50,
                    1
                );
                EditorGUI.DrawRect(_fieldRect, separatorColor);
            }

            // Color element in grey if not selected.
            if (!templates[_index].IsSelected)
            {
                _fieldRect.Set
                (
                    -1,
                    _rect.y - 2,
                    Screen.width,
                    _rect.height
                );

                EditorGUI.DrawRect(_fieldRect, disableElementColor);
            }
        }

        private float GetTemplatesElementHeight(int _index)
        {
            float _height = EditorGUIUtility.singleLineHeight + 3;
            if (templates[_index].AreSettingsUnfolded)
                _height *= 4;

            return _height;
        }

        private void OnTemplatesAdd(ReorderableList _list)
        {
            if (templates.Count > 0)
            {
                SpriteTemplate _lastTemplate = templates[templates.Count - 1];
                SpriteTemplate _template = new SpriteTemplate()
                {
                    DoBakeLights = _lastTemplate.DoBakeLights,
                    DoUsePowerOfTwoTexture = _lastTemplate.DoUsePowerOfTwoTexture,

                    Layer = _lastTemplate.Layer,
                    OrderInLayer = _lastTemplate.OrderInLayer,
                    Material = _lastTemplate.Material,
                };

                templates.Add(_template);
            }
            else
                templates.Add(new SpriteTemplate());
        }
        #endregion

        #region Editor Window
        private void OnEnable()
        {
            // Load settings.
            string _prefs = EditorPrefs.GetString(EditorPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(_prefs))
                JsonUtility.FromJsonOverwrite(_prefs, this);

            // Load sprites on enable.
            LoadSprites();

            // Remove obsolete saved scene groups.
            string[] _allScenes = EnhancedEditorUtility.FindAssets<SceneAsset>();
            for (int _i = 0; _i < _allScenes.Length; _i++)
            {
                _allScenes[_i] = Path.GetFileNameWithoutExtension(_allScenes[_i]);
            }
            for (int _i = 0; _i < sceneGroupsName.Count; _i++)
            {
                if (!_allScenes.Contains(sceneGroupsName[_i]))
                {
                    sceneGroups.RemoveAt(_i);
                    sceneGroupsName.RemoveAt(_i);

                    _i--;
                }
            }

            // Initialize templates list.
            templatesList = new ReorderableList(templates, typeof(SpriteTemplate), true, true, true, true)
            {
                headerHeight = (EditorGUIUtility.singleLineHeight * 3) + 3,
                elementHeightCallback = GetTemplatesElementHeight,
                drawHeaderCallback = DrawTemplatesHeader,
                drawElementCallback = DrawTemplatesElement,
                onAddCallback = OnTemplatesAdd
            };
        }

        private void OnDisable()
        {
            // Save settings.
            EditorPrefs.SetString(EditorPrefsKey, JsonUtility.ToJson(this));
        }

        // -----------------------

        private Vector2 scrollbar = new Vector2();

        private void OnGUI()
        {
            // Toolbar draw.
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            switch (mode)
            {
                case 0:
                    break;

                case 1:
                    DrawColorManagerToolbar();
                    break;

                case 2:
                    break;

                default:
                    break;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            mode = GUILayout.Toolbar(mode, modes, GUILayout.Height(25), GUILayout.ExpandWidth(true));

            scrollbar = EditorGUILayout.BeginScrollView(scrollbar);
            switch (mode)
            {
                case 0:
                    DrawGeneralMode();
                    break;

                case 1:
                    DrawColorManager();
                    break;

                case 2:
                    DrawSpriteScraper();
                    break;

                default:
                    break;
            }
            EditorGUILayout.EndScrollView();
        }
        #endregion
    }

    [Serializable]
    public class SpriteTemplate
    {
        #region Fields
        public Transform Root = null;
        public bool IsSelected = true;
        public bool DoBakeLights = true;
        public bool DoUsePowerOfTwoTexture = true;

        public bool AreSettingsUnfolded = false;
        public int Layer = 0;
        public int OrderInLayer = 0;
        public Material Material = null;
        #endregion
    }

    [Serializable]
    public class ColorGroup
    {
        #region Fields
        public string Name = "? New Color Group ?";
        public bool isSelected = false;
        public Color Color;

        public List<SpriteRenderer> Sprites = new List<SpriteRenderer>();
        #endregion

        #region Constructors
        public ColorGroup(Color _color, string _name)
        {
            Color = _color;
            Name = _name;
            Sprites = new List<SpriteRenderer>();
        }

        public ColorGroup(Color _color, IEnumerable<SpriteRenderer> _sprites)
        {
            Color = _color;
            Sprites = new List<SpriteRenderer>(_sprites);
        }
        #endregion
    }
}
