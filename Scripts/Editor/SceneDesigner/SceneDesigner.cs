// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

#if UNITY_2021_1_OR_NEWER
#define SCENEVIEW_TOOLBAR
#elif UNITY_2020_1_OR_NEWER
#define EDITOR_TOOLBAR
#endif

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditorInternal;
using UnityEngine;

using Random = UnityEngine.Random;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="SceneDesigner"/>-related <see cref="EnhancedSettings"/> class.
    /// </summary>
    [Serializable]
    public sealed class SceneDesignerEnhancedSettings : EnhancedSettings {
        #region Global Members
        [Enhanced, Folder] public List<string> Folders  = new List<string>();

        // --- Scene Editor --- \\

        [SerializeField] public LayerMask LayerMask     = ~0;

        [SerializeField] private bool randomScale       = false;
        [SerializeField] private bool randomRotation    = false;
        [SerializeField] private Vector2 scaleRange     = Vector2.one;
        [SerializeField] private Vector2 rotationRange  = Vector2.zero;

        public float Scale {
            get {
                if (!randomScale)
                    return 1f;

                return Random.Range(scaleRange.x, scaleRange.y);
            }
        }

        public Quaternion Rotation {
            get {
                if (!randomRotation)
                    return Quaternion.identity;

                return Quaternion.AngleAxis(Random.Range(rotationRange.x, rotationRange.y), Vector3.up);
            }
        }

        // -----------------------

        /// <inheritdoc cref="SceneDesignerEnhancedSettings"/>
        public SceneDesignerEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Settings
        private static readonly GUIContent sceneDesignerFoldersGUI = new GUIContent("Scene Designer Folders",
                                                                                    "All folders displayed to select objects to place in the scene using the Scene Designer.");

        private static readonly GUIContent rotationGUI = new GUIContent("Random Rotation", "Toggles prefab rotation override");
        private static readonly GUIContent scaleGUI    = new GUIContent("Random Scale",    "Toggles prefab scale override - value is used as percent of the prefab original scale");

        private static readonly ReorderableList folderList = new ReorderableList(null, typeof(string)) {
            drawHeaderCallback = DrawHeaderCallback,
            drawElementCallback = DrawElementCallback,
        };

        private static readonly int settingsGUID = "EnhancedEditorScriptableSceneDesignerSetting".GetStableHashCode();
        private static SceneDesignerEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty    = null;

        /// <inheritdoc cref="SceneDesignerEnhancedSettings"/>
        public static SceneDesignerEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty == null) || (settingsProperty.serializedObject != _userSettings.SerializedObject))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {
                    settings = new SceneDesignerEnhancedSettings(settingsGUID);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -------------------------------------------
        // Draw
        // -------------------------------------------

        [EnhancedEditorUserSettings(Order = 50)]
        internal static void DrawFolderSettings() {
            var _ = Settings;

            GUILayout.Space(5f);

            using (var _scope = new EditorGUI.ChangeCheckScope()) {
                folderList.list = settings.Folders;
                folderList.DoLayoutList();

                // Save on change.
                if (_scope.changed) {
                    SceneDesigner.RefreshFolders();
                }
            }
        }

        internal static void DrawSceneEditorSettings() {
            SceneDesignerEnhancedSettings _settings = Settings;
            settingsProperty.serializedObject.Update();

            EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("LayerMask"));
            GUILayout.Space(5f);

            // Scale.
            DrawField(scaleGUI, settingsProperty.FindPropertyRelative("randomScale"), settingsProperty.FindPropertyRelative("scaleRange"), 20f);
            DrawField(rotationGUI, settingsProperty.FindPropertyRelative("randomRotation"), settingsProperty.FindPropertyRelative("rotationRange"), 360f);

            settingsProperty.serializedObject.ApplyModifiedProperties();

            // ----- Local Method ----- \\

            static void DrawField(GUIContent _label, SerializedProperty _toggleProperty, SerializedProperty _rangeProperty, float maxValue) {
                Rect _position = EditorGUILayout.GetControlRect();
                Rect _temp = EditorGUI.PrefixLabel(_position, _label);

                _temp.width = 25f;
                EditorGUI.PropertyField(_temp, _toggleProperty, GUIContent.none);

                if (_toggleProperty.boolValue) {
                    _temp.x = _temp.xMax;
                    _temp.xMax = _position.xMax;

                    EnhancedEditorGUI.MinMaxField(_temp, _rangeProperty, GUIContent.none, 0f, maxValue);
                }
            }
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        private static void DrawHeaderCallback(Rect _position) {
            EditorGUI.LabelField(_position, sceneDesignerFoldersGUI);
        }

        private static void DrawElementCallback(Rect _position, int _index, bool _isActive, bool _isFocused) {
            _position.yMin += EditorGUIUtility.standardVerticalSpacing;
            settings.Folders[_index] = EnhancedEditorGUI.FolderField(_position, settings.Folders[_index]);
        }
        #endregion
    }

    /// <summary>
    /// Editor toolbar extension used to easily select prefabs from the project and place them in the scene.
    /// </summary>
	public sealed class SceneDesigner : EditorWindow {
        #region Styles
        private static class Styles {
            public static readonly GUIStyle Label = new GUIStyle(EditorStyles.whiteLabel);
            public static readonly GUIContent ToolbarButtonGUI = EditorGUIUtility.IconContent("PreMatCube");
        }
        #endregion

        #region Folder & Asset
        private class Folder {
            public string Name = string.Empty;
            public bool Foldout = false;

            public List<Folder> Folders = new List<Folder>();
            public List<Asset> Assets = new List<Asset>();

            // -----------------------

            public Folder(string _name) {
                Name = _name;
            }

            // -----------------------

            public void RegisterAsset(string[] _directories, string _fullPath, int _index) {
                if (_index == (_directories.Length - 1)) {

                    // Register new asset.
                    if (!Assets.Exists(a => a.Path == _fullPath)) {
                        Asset _asset = new Asset(_fullPath);
                        Assets.Add(_asset);
                    }

                    return;
                }

                string _directory = _directories[_index];
                foreach (Folder _folder in Folders) {

                    if (_folder.Name == _directory) {
                        _folder.RegisterAsset(_directories, _fullPath, _index + 1);
                        return;
                    }
                }

                Folder _newFolder = new Folder(_directory);
                Folders.Add(_newFolder);

                _newFolder.RegisterAsset(_directories, _fullPath, _index + 1);
            }
        }

        private class Asset {
            public string Name = string.Empty;
            public string Path = string.Empty;

            public Texture Icon = null;

            // -----------------------

            public Asset(string _path) {
                Name = $" {System.IO.Path.GetFileNameWithoutExtension(_path)}";
                Path = _path;
            }
        }
        #endregion

        #region Mesh Infos
        private class MeshInfo {
            public Mesh Mesh = null;
            public SpriteRenderer Sprite = null;
            public Material[] Materials = null;
            public Transform Transform = null;

            public Bounds Bounds = new Bounds();
            public bool IsSprite = false;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public MeshInfo(MeshFilter _mesh) {
                Mesh = _mesh.sharedMesh;
                Transform = _mesh.transform;
                IsSprite = false;

                if (_mesh.TryGetComponent(out MeshRenderer _meshRenderer)) {
                    Materials = _meshRenderer.sharedMaterials;
                }

                Bounds = _mesh.GetComponent<Renderer>().bounds;
            }

            public MeshInfo(SkinnedMeshRenderer _mesh) {
                Mesh = _mesh.sharedMesh;
                Materials = _mesh.sharedMaterials;
                Transform = _mesh.transform;
                IsSprite = false;

                Bounds = _mesh.GetComponent<Renderer>().bounds;
            }

            public MeshInfo(SpriteRenderer _sprite) {
                Sprite = _sprite;
                Transform = _sprite.transform;
                IsSprite = true;

                Materials = _sprite.sharedMaterials;
                Bounds = _sprite.bounds;
            }
        }
        #endregion

        // ----- Content ----- \\

        #region Global Members
        private const string EnabledKey         = "SceneDesignerEnabled";
        private const string SelectedAssetKey   = "SceneDesignerSelectedAsset";
        private const string HistoricalKey      = "SceneDesignerHistorical";

        private const int HistoricalMaxCount    = 25;

        private static bool isEnabled = false;
        private static string selectedAssetPath = string.Empty;

        private static readonly Folder root = new Folder("Root");

        private static readonly List<GameObject> historical = new List<GameObject>();
        private static GameObject selectedAsset = null;

        // -----------------------

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // Loads session values.
            bool _isEnabled = SessionState.GetBool(EnabledKey, isEnabled);

            string _historicalJson = SessionState.GetString(HistoricalKey, string.Empty);
            if (!string.IsNullOrEmpty(_historicalJson)) {
                EditorJsonUtility.FromJsonOverwrite(_historicalJson, historical);
            }

            SelectAsset(SessionState.GetString(SelectedAssetKey, selectedAssetPath), false);
            SetEnable(_isEnabled);
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and shows a new <see cref="SceneDesigner"/> window instance on screen.
        /// </summary>
        /// <returns><see cref="SceneDesigner"/> window instance on screen.</returns>
        public static SceneDesigner GetWindow(bool focus = true) {
            SceneDesigner _window = GetWindow<SceneDesigner>(false, "Scene Designer", focus);
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float ToolbarHeight = 25f;

        private static readonly GUIContent[] tabsGUI = new GUIContent[]
        {
            new GUIContent("Scene Editor"),
            new GUIContent("Settings"),
        };

        private static SceneDesigner instance = null;

        [SerializeField] private int selectedTabIndex = 0;

        // -----------------------

        private void OnEnable() {
            titleContent.image = EditorGUIUtility.FindTexture("Grid.PaintTool");
            instance = this;
        }

        private void OnGUI() {
            GUILayout.Space(10f);
            selectedTabIndex = EnhancedEditorGUILayout.CenteredToolbar(selectedTabIndex, tabsGUI, GUI.ToolbarButtonSize.FitToContents, GUILayout.Height(ToolbarHeight));

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);

                    switch (selectedTabIndex) {
                        case 0:

                            DrawSceneEditor();

                            /*EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("Columns"));

                            if (_changeCheck.changed)
                            {
                                settingsProperty.serializedObject.ApplyModifiedProperties();
                                EnhancedConsoleWindow.GetWindow(false).SortColumns(_setting);
                            }*/

                            break;

                        case 1:
                            //EnhancedEditorGUILayout.EnhancedPropertyField(settingsProperty.FindPropertyRelative("CustomFilters"));
                            DrawSettings();

                            break;

                        default:
                            break;
                    }

                    //settingsProperty.serializedObject.ApplyModifiedProperties();

                    // Refresh on change.
                    if (_changeCheck.changed) {
                        EnhancedEditorUserSettings.Instance.Save();
                        EnhancedConsoleWindow.GetWindow(false).RefreshFilters();
                    }
                }

                GUILayout.Space(5f);
            }
        }

        private void OnDisable() {
            if (instance == this) {
                instance = null;
            }
        }
        #endregion

        #region Scene Editor
        private const float HistoricalHeight = 75f;
        private const float SelectionMargins = 2f;

        private static readonly GUIContent historicalHeaderGUI = new GUIContent("RECENTLY USED", "Recently used assets");
        private static readonly GUIContent placementHeaderGUI  = new GUIContent("PLACEMENT", "Placement additional options");

        private static readonly GUIContent enabledGUI    = new GUIContent("Enabled", "Toggles the Scene Designer activation");

        private static readonly Color historicalBackgroundColor = SuperColor.DarkGrey.Get();
        private static readonly Color selectionColor = SuperColor.Sapphire.Get();

        private Vector2 historicalScroll = Vector2.zero;

        // -----------------------

        private void DrawSceneEditor() {
            GUILayout.Space(10f);

            // Activation.
            bool _isEnabled = EditorGUILayout.Toggle(enabledGUI, isEnabled);
            if (_isEnabled != isEnabled) {
                SetEnable(_isEnabled);
            }

            // Historical.
            if (historical.Count != 0) {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField(historicalHeaderGUI, EditorStyles.boldLabel);
                GUILayout.Space(5f);

                using (var scrollScope = new EditorGUILayout.ScrollViewScope(historicalScroll, false, false, GUILayout.Height(HistoricalHeight + 13f)))
                using (var scope = new EditorGUILayout.HorizontalScope(GUILayout.Height(HistoricalHeight))) {
                    historicalScroll = scrollScope.scrollPosition;

                    EditorGUI.DrawRect(scope.rect, historicalBackgroundColor);
                    GUILayout.Space(5f);

                    Rect _origin = EditorGUILayout.GetControlRect(false, HistoricalHeight - 5f);
                    Rect _position = new Rect(_origin)
                    {
                        width = _origin.height
                    };

                    // Draw historical.
                    for (int i = historical.Count; i-- > 0;) {
                        GameObject _asset = historical[i];
                        if (_asset == null) {
                            historical.RemoveAt(i);
                            continue;
                        }

                        Texture2D _icon = AssetPreview.GetAssetPreview(_asset);

                        if ((_icon == null) && !AssetPreview.IsLoadingAssetPreview(_asset.GetInstanceID())) {
                            _icon = AssetPreview.GetMiniThumbnail(_asset);

                            if (_icon == null) {
                                continue;
                            }
                        }

                        // Selection outline.
                        if (isEnabled && (selectedAsset == _asset)) {
                            Rect _temp = new Rect(_position);

                            _temp.xMin -= SelectionMargins;
                            _temp.xMax += SelectionMargins;
                            _temp.yMin -= SelectionMargins;
                            _temp.yMax += SelectionMargins;

                            EditorGUI.DrawRect(_temp, selectionColor);
                        }

                        EditorGUI.DrawPreviewTexture(_position, _icon);
                        if (EnhancedEditorGUIUtility.MouseDown(_position)) {
                            SelectAsset(_asset, false);
                        }

                        _position.x += _position.width + 7f;
                    }

                    GUILayoutUtility.GetRect((_position.xMax - _origin.xMax) - 60f, 1f);
                    EditorGUILayout.LabelField("");
                }
            }

            GUILayout.Space(10f);

            // Options.
            EditorGUILayout.LabelField(placementHeaderGUI, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            SceneDesignerEnhancedSettings.DrawSceneEditorSettings();
        }
        #endregion

        #region Settings
        private void DrawSettings() {
            SceneDesignerEnhancedSettings.DrawFolderSettings();
        }
        #endregion

        // ----- Scene Drawer ----- \\

        #region Scene Drawer
        private const EventModifiers RotateModifier = EventModifiers.Control;
        private const EventModifiers ScaleModifier  = EventModifiers.Shift;

        private const KeyCode DisableKey = KeyCode.Escape;
        private const KeyCode ResetKey   = KeyCode.Tab;

        private const int ControlID = 167213;

        private static MeshInfo[] meshInfos = null;
        private static GameObject newInstance = null;
        private static GameObject selection = null;

        private static Vector3 worldPosition = Vector3.zero;
        private static Quaternion rotation   = Quaternion.identity;
        private static Vector3 scale         = Vector3.one;

        private static Bounds assetBounds = default;
        private static Mesh quadMesh = null;

        // -----------------------

        private static void OnSceneGUI(SceneView _sceneView) {

            SceneDesignerEnhancedSettings _settings = SceneDesignerEnhancedSettings.Settings;
            Event _event = Event.current;
            bool _isRotating = _event.modifiers.HasFlagUnsafe(RotateModifier);
            bool _isScaling  = _event.modifiers.HasFlagUnsafe(ScaleModifier);

            if ((mouseOverWindow != _sceneView) && !_isRotating && !_isScaling)
                return;

            // Destroyed asset management.
            if (selectedAsset == null) {
                return;
            }

            // Hot key.
            if (_event.isKey) {

                switch (_event.keyCode) {

                    // Reset rotation and scale.
                    case ResetKey:
                        rotation = Quaternion.identity;
                        scale = Vector3.one;
                        break;

                    // Disable.
                    case DisableKey:
                        SetEnable(false);
                        return;

                    default:
                        break;
                }
            }

            // Create and place a new instance on click.
            if ((GUIUtility.hotControl != 0) && (_event.type == EventType.MouseDown) && (_event.button == 0) && !_isRotating && !_isScaling) {

                newInstance = EnhancedEditorUtility.CreateObject(selectedAsset, selectedAsset.name, Selection.activeGameObject, false);
                Transform _transform = newInstance.transform;

                _transform.position = worldPosition;
                _transform.rotation *= rotation * _settings.Rotation;
                _transform.localScale = Vector3.Scale(_transform.localScale, scale * _settings.Scale);

                selection = Selection.activeGameObject;
                GUIUtility.hotControl = ControlID;

                Undo.RegisterCreatedObjectUndo(newInstance, "New Asset Instantiation");

                _event.Use();
            } else if ((GUIUtility.hotControl == ControlID) && ((_event.type == EventType.Used) || (_event.type == EventType.MouseUp)) && (newInstance != null)) {

                //Selection.activeObject = selection;
                _event.Use();

                newInstance = null;
                selection = null;

                GUIUtility.hotControl = 0;
            }

            // Get the asset preview position in world space according to the user mouse position.
            if (!_isRotating && !_isScaling) {

                Ray _worldRay = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
                if (Physics.Raycast(_worldRay, out RaycastHit _hit, 1000f, _settings.LayerMask)) {

                    // Get the hit normal, rounded to the nearest int for each axis
                    Vector3 _roundedNormal = new Vector3(Mathf.RoundToInt(_hit.normal.x),
                                                         Mathf.RoundToInt(_hit.normal.y),
                                                         Mathf.RoundToInt(_hit.normal.z));

                    // Set the preview position relative to the virtual collider
                    worldPosition = (_hit.point - Vector3.Scale(assetBounds.center, scale)) + Vector3.Scale(assetBounds.extents, Vector3.Scale(_roundedNormal, scale));
                } else {
                    worldPosition = _worldRay.origin + (_worldRay.direction * 25f);
                }
            }

            // Position handles.
            bool _drawHandles = GUIUtility.hotControl == 0;
            #if SCENEVIEW_TOOLBAR
            _drawHandles &= _event.mousePosition.y > 25f;
            #endif

            if (_isRotating) {

                // Rotation.
                rotation = Handles.RotationHandle(rotation, worldPosition);
            } else if (_isScaling) {

                // Scale.
                scale = Handles.ScaleHandle(scale, worldPosition, rotation, 1f);

            } else if (_drawHandles) {

                // Position.
                Transform _transform = selectedAsset.transform;

                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.xAxisColor)) {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.right), 1f, EventType.Repaint);
                }
                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.yAxisColor)) {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.up), 1f, EventType.Repaint);
                }
                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.zAxisColor)) {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.forward), 1f, EventType.Repaint);
                }
            }

            _sceneView.Repaint();

            // Draw the selected mesh on camera.
            if (((GUIUtility.hotControl != 0) && !_isRotating && !_isScaling) || (SceneView.lastActiveSceneView != _sceneView))
                return;

            Camera _camera = _sceneView.camera;
            Transform _assetTransform = selectedAsset.transform;
            Matrix4x4 _matrix = new Matrix4x4();

            foreach (MeshInfo _mesh in meshInfos) {

                Quaternion _parentRotation = _assetTransform.rotation * rotation;
                Quaternion _localRotation  = _mesh.Transform.rotation * Quaternion.Inverse(_assetTransform.rotation);
                Quaternion _rotation = _parentRotation * _localRotation;

                Vector3 _offset = _parentRotation * Vector3.Scale(_assetTransform.InverseTransformPoint(_mesh.Transform.position), scale);
                Vector3 _scale = Vector3.Scale(_mesh.Transform.lossyScale, scale);

                // Set the matrix to use for preview
                Vector3 _position = worldPosition + _offset;
                _matrix.SetTRS(_position, _rotation, _scale);

                for (int _i = 0; _i < _mesh.Materials.Length; _i++) {
                    Material _material = _mesh.Materials[_i];

                    if (_mesh.IsSprite) {
                        Draw(_mesh.Sprite, _matrix, _position, _material, 2, _camera, _i);
                    } else {
                        Graphics.DrawMesh(_mesh.Mesh, _matrix, _material, 2, _camera, _i);
                    }
                }
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> enabled state.
        /// </summary>
        /// <param name="_isEnable">Should the <see cref="SceneDesigner"/> be enabled?</param>
        public static void SetEnable(bool _isEnable) {
            SceneView.duringSceneGui -= OnSceneGUI;

            if (_isEnable) {
                if (string.IsNullOrEmpty(selectedAssetPath)) {
                    SceneDesignerQuickAccess.GetWindow();
                    return;
                }

                SceneView.duringSceneGui += OnSceneGUI;
            }

            isEnabled = _isEnable;
            SessionState.SetBool(EnabledKey, _isEnable);

            EnhancedEditorToolbar.Repaint();
        }

        /// <inheritdoc cref="SelectAsset(string)"/>
        /// <param name="_asset">New selected asset.</param>
        public static void SelectAsset(GameObject _asset, bool _refreshHistorical = true) {
            string _path = AssetDatabase.GetAssetPath(_asset);

            if (!string.IsNullOrEmpty(_path)) {
                SelectAsset(_path, _refreshHistorical);
            }
        }

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> currently selected asset.
        /// </summary>
        /// <param name="_assetPath">New selected asset path.</param>
        public static void SelectAsset(string _assetPath, bool _refreshHistorical = true) {
            GameObject _asset = AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath);

            if (_asset == null) {
                return;
            }

            Transform _transform = _asset.transform;

            // Get mesh infos.
            MeshFilter[] _meshFilters = _asset.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] _meshRenderers = _asset.GetComponentsInChildren<SkinnedMeshRenderer>();
            SpriteRenderer[] _spriteRenderers = _asset.GetComponentsInChildren<SpriteRenderer>();

            meshInfos = Array.ConvertAll(_meshFilters, (m) => new MeshInfo(m));
            ArrayUtility.AddRange(ref meshInfos, Array.ConvertAll(_meshRenderers, (m) => new MeshInfo(m)));
            ArrayUtility.AddRange(ref meshInfos, Array.ConvertAll(_spriteRenderers, (m) => new MeshInfo(m)));

            // Setup bounds.
            assetBounds = new Bounds();

            foreach (MeshInfo _mesh in meshInfos) {
                Bounds _bounds = _mesh.Bounds;
                _bounds.center -= _transform.position;

                assetBounds.Encapsulate(_bounds);
            }

            // Select.
            selectedAssetPath = _assetPath;
            selectedAsset = _asset;

            // Historical.
            if (_refreshHistorical) {
                int _index = historical.IndexOf(_asset);
                if (_index == 0)
                    return;

                if (_index != -1) {
                    historical.Remove(_asset);
                }

                historical.Add(_asset);
            }

            while (historical.Count > HistoricalMaxCount) {
                historical.RemoveAt(0);
            }

            // Repaint.
            if (instance != null) {
                instance.Repaint();
            }

            SessionState.SetString(SelectedAssetKey, _assetPath);
            SessionState.SetString(HistoricalKey, EditorJsonUtility.ToJson(historical));

            SetEnable(true);
        }

        // --------------

        public static void Draw(SpriteRenderer _sprite, Matrix4x4 _matrix, Vector3 _position, Material _material, int _layer, Camera _camera, int _subMeshIndex) {
            if (quadMesh == null) {
                quadMesh = CreateQuad();
            }

            var _mpb = new MaterialPropertyBlock();
            _mpb.SetTexture("_MainTex", _sprite.sprite.texture);
            _mpb.SetColor("_Color", _sprite.color);

            Vector3 _scale = Vector3.Scale(_matrix.lossyScale, _sprite.bounds.size);
            _position     += _matrix.rotation * _sprite.bounds.center;

            if (_sprite.flipX)
                _scale.x *= -1f;

            if (_sprite.flipY)
                _scale.y *= -1f;

            _matrix = Matrix4x4.TRS(_position, _matrix.rotation, _scale);
            Graphics.DrawMesh(quadMesh, _matrix, _material, _layer, _camera, _subMeshIndex, _mpb);
        }

        private static Mesh CreateQuad() {
            var _mesh = new Mesh {
                vertices = new Vector3[] {
                                        new Vector3(-.5f, -.5f, 0),
                                        new Vector3(-.5f, +.5f, 0),
                                        new Vector3(+.5f, +.5f, 0),
                                        new Vector3(+.5f, -.5f, 0),
                                    },

                normals = new Vector3[] {
                                    Vector3.forward,
                                    Vector3.forward,
                                    Vector3.forward,
                                    Vector3.forward,
                                },

                triangles = new int[] { 0, 1, 2, 2, 3, 0 },

                uv = new Vector2[]
                            {
                                new Vector2(0, 0),
                                new Vector2(0, 1),
                                new Vector2(1, 1),
                                new Vector2(1, 0),
                            }
            };

            return _mesh;
        }
        #endregion

        // ----- Preview ----- \\

        #region Preview Window
        /// <summary>
        /// Preview <see cref="SceneDesigner"/> asset window.
        /// </summary>
        private class PreviewWindow : EditorWindow {
            public static PreviewWindow GetWindow(Rect _screenPosition, Texture _preview) {
                PreviewWindow _window = CreateInstance<PreviewWindow>();

                Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 _size = new Vector2(128f, 128f);

                _window.screenPosition = _screenPosition;
                _window.preview = _preview;

                _window.position = new Rect(_position, _size);
                _window.ShowPopup();

                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            [NonSerialized] public static bool HasFocus = false;

            private Rect screenPosition = default;
            private Texture preview = null;

            // -----------------------

            private void OnEnable() {
                HasFocus = true;
            }

            private void OnGUI() {
                Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                if (!screenPosition.Contains(_position)) {
                    Close();
                    return;
                }

                if (Event.current.type == EventType.Repaint) {
                    position = new Rect(_position, position.size);
                }

                Rect _temp = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUI.DrawPreviewTexture(_temp, preview);

                Repaint();
            }

            private void OnDisable() {
                HasFocus = false;
                FocusWindowIfItsOpen<SceneDesigner>();
            }
        }
        #endregion

        // ----- Utility ----- \\

        #region Shortcut
        private const float DefaultDistance = 999;
        private static readonly Collider[] _colliderBuffer = new Collider[16];

        // -----------------------

        /// <summary>
        /// Snaps selected objects to the nearest collider.
        /// </summary>
        [Shortcut("Enhanced Editor/Snap Object", KeyCode.PageDown)]
        private static void SnapSelection() {

            foreach (GameObject _gameObject in Selection.gameObjects) {
                Collider _closest = null;
                Vector3 _normal = default;
                float _distance = DefaultDistance;

                Transform _transform = _gameObject.transform;
                Vector3 _position = _transform.position;
                int _amount = Physics.OverlapSphereNonAlloc(_position, 10f, _colliderBuffer);

                for (int i = 0; i < _amount; i++) {

                    Collider _collider = _colliderBuffer[i];
                    if (_collider.isTrigger || _collider.transform.IsChildOf(_transform))
                        continue;

                    Vector3 _point;

                    if ((_collider is MeshCollider) && Physics.Raycast(_position, -_transform.up, out RaycastHit hit, 10f)) {
                        _point = hit.point;
                    } else {
                        _point = _collider.ClosestPoint(_position);
                    }

                    float _pointDistance = (_point - _position).sqrMagnitude;

                    // Get nearest collider.
                    if (_pointDistance < _distance) {
                        _distance = _pointDistance;
                        _normal = _point - _position;
                        _closest = _collider;
                    }
                }

                // Extract from any overlapping collider.
                if (!Mathf.Approximately(_distance, DefaultDistance)) {

                    _gameObject.transform.position += _normal;
                    foreach (Collider _collider in _gameObject.GetComponentsInChildren<Collider>()) {

                        if (_collider.isTrigger) {
                            continue;
                        }

                        if (Physics.ComputePenetration(_collider, _collider.transform.position, _collider.transform.rotation,
                                                       _closest, _closest.transform.position, _closest.transform.rotation, out _normal, out _distance)) {
                            _gameObject.transform.position += _normal * _distance;
                        }
                    }
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Refreshes folders content and assets.
        /// </summary>
        public static void RefreshFolders() {
            var _settings = SceneDesignerEnhancedSettings.Settings;
            root.Folders.Clear();

            int _count = _settings.Folders.Count;
            if (_count == 0)
                return;

            string[] _pathHelpers = new string[_count];
            for (int _i = 0; _i < _count; _i++) {
                string _fullPath = Path.Combine("Assets", _settings.Folders[_i]);
                _pathHelpers[_i] = _fullPath;
            }

            // Load all objects.
            string[] _assets = Array.ConvertAll(AssetDatabase.FindAssets("t:GameObject", _pathHelpers), AssetDatabase.GUIDToAssetPath);

            for (int _i = 0; _i < _count; _i++) {
                string _base = _settings.Folders[_i].Split('/', '\\')[0];
                _pathHelpers[_i] = string.IsNullOrEmpty(_base)
                                 ? InternalEditorUtility.GetAssetsFolder()
                                 : _base;
            }

            // Register each asset.
            foreach (string _path in _assets) {
                string[] _directories = _path.Split('/', '\\');
                int _index = 0;

                while (Array.IndexOf(_pathHelpers, _directories[_index].Trim()) == -1) {
                    _index++;
                }

                root.RegisterAsset(_directories, _path, _index);
            }
        }
        #endregion

        public sealed class SceneDesignerQuickAccess : EditorWindow {
            #region Window GUI
            private static readonly Vector2 windowSize = new Vector2(400f, 300f);

            /// <summary>
            /// Creates and shows a new <see cref="SceneDesignerQuickAccess"/> window instance on screen.
            /// </summary>
            /// <returns><see cref="SceneDesignerQuickAccess"/> window instance on screen.</returns>
            public static SceneDesignerQuickAccess GetWindow() {
                SceneDesignerQuickAccess _window = CreateInstance<SceneDesignerQuickAccess>();

                Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 _size = windowSize;

                _position.x -= 10f;
                _position.y = GUIUtility.GUIToScreenPoint(new Vector2(0f, 22f)).y;

                _window.position = new Rect(_position, _size);

                _window.ShowPopup();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const float LineHeight  = 14f;
            private const string NoAssetMessage = "No asset could be found in the specified folders. " +
                                                  "You can edit the scene designers folders using the button on the window top-right corner.";

            private static readonly Color headerColor   = new Color(.9f, .9f, .9f, 1f);
            private static readonly Color prefabColor   = new Color(.48f, .67f, .94f, 1f);
            private static readonly Color hoverColor    = new Color(1f, 1f, 1f, .07f);

            private static readonly GUIContent headerGUI = new GUIContent("Game Objects & Prefabs:");
            private static readonly GUIContent maximizeGUI = new GUIContent(string.Empty, "Open the settings in another window");

            private static Vector2 scroll = new Vector2();
            private new bool hasFocus = false;

            // -----------------------

            private void OnEnable() {
                maximizeGUI.image = EditorGUIUtility.IconContent("winbtn_win_max@2x").image;
                RefreshFolders();
            }

            private void OnFocus() {
                if (!InternalEditorUtility.isApplicationActive) {
                    //Close();
                    //return;
                }

                hasFocus = true;
            }

            private void OnGUI() {
                // Close if not focused.
                if (!hasFocus && !PreviewWindow.HasFocus) {
                    Close();
                    return;
                }

                using (var _scope = new GUILayout.ScrollViewScope(scroll)) {
                    scroll = _scope.scrollPosition;

                    GUILayout.Space(2f);
                    using (var _horizontalScope = new EditorGUILayout.HorizontalScope()) {

                        // Preferences button.
                        Rect _position = new Rect(_horizontalScope.rect)
                        {
                            xMin = _horizontalScope.rect.xMax - 28f,
                            height = 20f
                        };

                        EnhancedEditorSettings.DrawUserSettingsButton(_position);

                        _position.x -= 32f;

                        if (EnhancedEditorGUI.IconButton(_position, maximizeGUI)) {
                            SceneDesigner.GetWindow();
                            return;
                        }

                        _position.xMin = 5f;
                        _position.width = position.width;

                        EnhancedEditorGUI.UnderlinedLabel(_position, headerGUI);

                        GUILayout.Space(5f);

                        // Content.
                        using (var _verticalScope = new GUILayout.VerticalScope()) {
                            // No asset message.
                            if (root.Folders.Count == 0) {
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.HelpBox(NoAssetMessage, UnityEditor.MessageType.Info, true);

                                GUILayout.FlexibleSpace();
                                return;
                            }

                            GUILayout.Space(30f);

                            int _index = 0;
                            DrawFolder(root, ref _index);

                            GUILayout.Space(5f);
                        }
                    }
                }

                Repaint();
            }

            private void OnLostFocus() {
                if (!PreviewWindow.HasFocus) {
                    Close();
                }

                hasFocus = false;
            }

            // -------------------------------------------
            // GUI
            // -------------------------------------------

            private void DrawFolder(Folder _folder, ref int _index) {
                Rect _origin = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing));
                Rect _position = new Rect(_origin);

                // Folders on top.
                foreach (Folder _subfolder in _folder.Folders) {

                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_subfolder.Name, _subfolder.Name);
                    _position = GetRect(ref _index, false);

                    using (var _noIndent = EnhancedEditorGUI.ZeroIndentScope())
                    using (var _scope = EnhancedGUI.GUIContentColor.Scope(headerColor)) {
                        _subfolder.Foldout = EditorGUI.Foldout(_position, _subfolder.Foldout, _label, true);
                    }

                    if (_subfolder.Foldout) {
                        using (var _scope = new EditorGUI.IndentLevelScope()) {
                            DrawFolder(_subfolder, ref _index);
                        }
                    }
                }

                // Assets.
                foreach (Asset _asset in _folder.Assets) {
                    _position = GetRect(ref _index, true, _asset.Path);

                    using (var _noIndent = EnhancedEditorGUI.ZeroIndentScope()) {

                        // Label.
                        using (var _scope = EnhancedGUI.GUIContentColor.Scope(prefabColor)) {

                            Rect _temp = new Rect(_position)
                            {
                                y = _position.y - 1f,
                                height = _position.height + 2f,
                            };

                            EditorGUI.LabelField(_temp, EnhancedEditorGUIUtility.GetLabelGUI(_asset.Name, _asset.Path), Styles.Label);
                        }

                        // Mini thumbnail.
                        if (_asset.Icon == null) {
                            GameObject _object = AssetDatabase.LoadAssetAtPath<GameObject>(_asset.Path);
                            Texture2D _icon = AssetPreview.GetAssetPreview(_object);

                            if ((_icon == null) && !AssetPreview.IsLoadingAssetPreview(_object.GetInstanceID())) {
                                _icon = AssetPreview.GetMiniThumbnail(_object);
                            }

                            _asset.Icon = _icon;
                        } else {
                            Rect _temp = new Rect(_position)
                            {
                                x = _position.xMax - (_position.height + 3f),
                                y = _position.y + 1f,
                                width  = LineHeight,
                                height = LineHeight
                            };

                            EditorGUI.DrawPreviewTexture(_temp, _asset.Icon);

                            // Preview window.
                            if (!PreviewWindow.HasFocus && _temp.Contains(Event.current.mousePosition)) {

                                _temp.position += position.position - scroll;
                                PreviewWindow.GetWindow(_temp, _asset.Icon);
                            }
                        }

                        if (_position.Event(out Event _event) == EventType.MouseDown) {
                            switch (_event.clickCount) {

                                // Select.
                                case 1:
                                    SelectAsset(_asset.Path, true);
                                    Repaint();
                                    break;

                                // Close.
                                case 2:
                                    Close();
                                    break;

                                default:
                                    break;
                            }

                            _event.Use();
                        }
                    }
                }

                // Vertical indent.
                if ((Event.current.type == EventType.Repaint) && (EditorGUI.indentLevel != 0)) {
                    Rect _temp = new Rect()
                    {
                        x = _origin.x - 9f,
                        y = _origin.y - 3,
                        yMax = _position.yMax - 7f,
                        width = 2f,
                    };

                    EnhancedEditorGUI.VerticalDottedLine(_temp, 1f, 1f);
                }
            }

            private Rect GetRect(ref int _index, bool _isAsset, string _path = "") {

                // Position.
                Rect _position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, LineHeight));
                _position.yMin -= EditorGUIUtility.standardVerticalSpacing;

                Rect _background = new Rect(_position)
                {
                    x = 0f,
                    width = position.width
                };

                _index++;

                // Line background.
                bool _selected = _isAsset && (selectedAssetPath == _path);
                bool _isOdd = (_index % 2) != 0;

                Color _backgroundColor = _isOdd ? EnhancedEditorGUIUtility.GUIPeerLineColor : EnhancedEditorGUIUtility.GUIThemeBackgroundColor;
                EditorGUI.DrawRect(_background, _backgroundColor);

                // Feedback background.
                if (_selected) {
                    _backgroundColor = EnhancedEditorGUIUtility.GUISelectedColor;
                    EditorGUI.DrawRect(_background, _backgroundColor);
                } else if (_background.Contains(Event.current.mousePosition)) {
                    EditorGUI.DrawRect(_background, hoverColor);
                }

                // Horizontal indent.
                if ((Event.current.type == EventType.Repaint) && (EditorGUI.indentLevel != 0)) {
                    Rect _temp = new Rect()
                    {
                        x = _position.x - 9f,
                        y = _position.y + 9f,
                        xMax = _position.x + 2f,
                        height = 2f,
                    };

                    EnhancedEditorGUI.HorizontalDottedLine(_temp, 1f, 1f);
                }

                return _position;
            }
            #endregion

            #region Toolbar Extension
            [EditorToolbarLeftExtension(Order = 100)]
            #pragma warning disable
            private static void ToolbarExtension() {
                int _result = EnhancedEditorToolbar.DropdownToggle(isEnabled, Styles.ToolbarButtonGUI, GUILayout.Width(32f));

                switch (_result) {

                    // Enbled toggle.
                    case 0:
                        SetEnable(!isEnabled);
                        break;

                    // Asset selection.
                    case 1:
                        SceneDesignerQuickAccess.GetWindow();
                        break;

                    default:
                        break;
                }
            }
            #endregion
        }
    }
}
