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

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="SceneDesigner"/>-related <see cref="EnhancedSettings"/> class.
    /// </summary>
    [Serializable]
    public class SceneDesignerEnhancedSettings : EnhancedSettings {
        #region Global Members
        [Folder] public List<string> Folders = new List<string>();

        // -----------------------

        /// <inheritdoc cref="SceneDesignerEnhancedSettings"/>
        public SceneDesignerEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Settings
        private static readonly GUIContent sceneDesignerFoldersGUI = new GUIContent("Scene Designer Folders",
                                                                                    "All folders displayed to select objects to place in the scene using the Scene Designer.");

        private static readonly ReorderableList folderList = new ReorderableList(null, typeof(string)) {
            drawHeaderCallback = DrawHeaderCallback,
            drawElementCallback = DrawElementCallback,
        };

        private static readonly int settingsGUID = "EnhancedEditorScriptableSceneDesignerSetting".GetHashCode();
        private static SceneDesignerEnhancedSettings settings = null;

        /// <inheritdoc cref="SceneDesignerEnhancedSettings"/>
        public static SceneDesignerEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
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
        private static void DrawSettings() {
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
	public class SceneDesigner : EditorWindow {
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
            public Material[] Materials = null;
            public Transform Transform = null;

            public Bounds Bounds = new Bounds();

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public MeshInfo(MeshFilter _mesh) {
                Mesh = _mesh.sharedMesh;
                Transform = _mesh.transform;

                if (_mesh.TryGetComponent(out MeshRenderer _meshRenderer)) {
                    Materials = _meshRenderer.sharedMaterials;
                }

                Bounds = _mesh.GetComponent<Renderer>().bounds;
            }

            public MeshInfo(SkinnedMeshRenderer _mesh) {
                Mesh = _mesh.sharedMesh;
                Materials = _mesh.sharedMaterials;
                Transform = _mesh.transform;

                Bounds = _mesh.GetComponent<Renderer>().bounds;
            }
        }
        #endregion

        #region Global Members
        private const string EnabledKey         = "SceneDesignerEnabled";
        private const string SelectedAssetKey   = "SceneDesignerSelectedAsset";

        private static bool isEnabled = false;
        private static string selectedAssetPath = string.Empty;

        private static GameObject selectedAsset = null;

        // -----------------------

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // Loads session values.
            bool _isEnabled = SessionState.GetBool(EnabledKey, isEnabled);

            SelectAsset(SessionState.GetString(SelectedAssetKey, selectedAssetPath));
            SetEnable(_isEnabled);
        }
        #endregion

        #region Behaviour
        private const EventModifiers RotateModifier = EventModifiers.Control;
        private const EventModifiers ScaleModifier  = EventModifiers.Shift;

        private const KeyCode DisableKey = KeyCode.Escape;
        private const KeyCode ResetKey   = KeyCode.Tab;

        private static MeshInfo[] meshInfos = null;
        private static GameObject newInstance = null;

        private static Vector3 worldPosition = Vector3.zero;
        private static Quaternion rotation   = Quaternion.identity;
        private static Vector3 scale         = Vector3.one;

        private static Bounds assetBounds = default;

        // -----------------------

        private static void OnSceneGUI(SceneView _sceneView) {

            Event _event = Event.current;
            bool _isRotating = _event.modifiers.HasFlag(RotateModifier);
            bool _isScaling  = _event.modifiers.HasFlag(ScaleModifier);

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

                newInstance = PrefabUtility.InstantiatePrefab(selectedAsset) as GameObject;
                Transform _transform = newInstance.transform;

                _transform.position = worldPosition;
                _transform.rotation *= rotation;
                _transform.localScale = Vector3.Scale(_transform.localScale, scale);

                EditorGUIUtility.PingObject(newInstance);
                Selection.activeObject = newInstance;

                Undo.RegisterCreatedObjectUndo(newInstance, "New Asset Instantiation");

                _event.Use();
            } else if ((_event.type == EventType.Used) && (newInstance != null)) {

                Selection.activeObject = newInstance;
                newInstance = null;
            }

            // Get the asset preview position in world space according to the user mouse position.
            if (!_isRotating && !_isScaling) {

                Ray _worldRay = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
                if (Physics.Raycast(_worldRay, out RaycastHit _hit, 1000f)) {

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

                Quaternion _rotation = _mesh.Transform.rotation * rotation;
                Quaternion _local = _assetTransform.rotation * rotation;

                Vector3 _offset = _local * Vector3.Scale(_assetTransform.InverseTransformPoint(_mesh.Transform.position), scale);
                Vector3 _scale = Vector3.Scale(_mesh.Transform.lossyScale, scale);

                // Set the matrix to use for preview
                _matrix.SetTRS(worldPosition + _offset, _rotation, _scale);

                for (int _i = 0; _i < _mesh.Materials.Length; _i++) {
                    Material _material = _mesh.Materials[_i];
                    Graphics.DrawMesh(_mesh.Mesh, _matrix, _material, 2, _camera, _i);
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
                    GetWindow();
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
        public static void SelectAsset(GameObject _asset) {
            string _path = AssetDatabase.GetAssetPath(_asset);

            if (!string.IsNullOrEmpty(_path)) {
                SelectAsset(_path);
            }
        }

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> currently selected asset.
        /// </summary>
        /// <param name="_assetPath">New selected asset path.</param>
        public static void SelectAsset(string _assetPath) {
            GameObject _asset = AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath);

            if (_asset == null) {
                return;
            }

            Transform _transform = _asset.transform;

            // Get mesh infos.
            MeshFilter[] _meshFilters = _asset.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] _meshRenderers = _asset.GetComponentsInChildren<SkinnedMeshRenderer>();

            meshInfos = Array.ConvertAll(_meshFilters, (m) => new MeshInfo(m));
            ArrayUtility.AddRange(ref meshInfos, Array.ConvertAll(_meshRenderers, (m) => new MeshInfo(m)));

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

            SessionState.SetString(SelectedAssetKey, _assetPath);

            SetEnable(true);
        }
        #endregion

        #region Window GUI
        private static readonly Vector2 windowSize = new Vector2(400f, 300f);

        /// <summary>
        /// Creates and shows a new <see cref="SceneDesigner"/> window instance on screen.
        /// </summary>
        /// <returns><see cref="SceneDesigner"/> window instance on screen.</returns>
        public static SceneDesigner GetWindow() {
            SceneDesigner _window = CreateInstance<SceneDesigner>();

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
        private static readonly Folder root = new Folder("Root");

        private static Vector2 scroll = new Vector2();
        private new bool hasFocus = false;

        // -----------------------

        private void OnEnable() {
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
                    Rect _position = new Rect(_horizontalScope.rect) {
                        xMin = _horizontalScope.rect.xMax - 28f,
                        height = 20f
                    };

                    EnhancedEditorSettings.DrawUserSettingsButton(_position);

                    _position.xMin = 5f;
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

                        Rect _temp = new Rect(_position) {
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
                        Rect _temp = new Rect(_position) {
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
                                SelectAsset(_asset.Path);
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
                Rect _temp = new Rect() {
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

            Rect _background = new Rect(_position) {
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
                Rect _temp = new Rect() {
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
        private static void ToolbarExtension() {
            int _result = EnhancedEditorToolbar.DropdownToggle(isEnabled, Styles.ToolbarButtonGUI, GUILayout.Width(32f));

            switch (_result) {

                // Enbled toggle.
                case 0:
                    SetEnable(!isEnabled);
                    break;

                // Asset selection.
                case 1:
                    GetWindow();
                    break;

                default:
                    break;
            }
        }
        #endregion

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
                        _distance   = _pointDistance;
                        _normal     = _point - _position;
                        _closest    = _collider;
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
    }
}
