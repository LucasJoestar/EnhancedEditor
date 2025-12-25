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
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor toolbar extension used to easily select prefabs from the project and place them in the scene.
    /// </summary>
	public sealed class SceneDesigner : EditorWindow {
        #region Styles
        private static class Styles {
            public static readonly GUIStyle Label = new GUIStyle(EditorStyles.whiteLabel);
            public static readonly GUIContent ToolbarMainButtonGUI = new GUIContent() {
                image = EditorGUIUtility.IconContent("PreMatCube").image,
                tooltip = "Create Object"
            };
            public static readonly GUIContent ToolbarVerticeAreaButtonGUI = new GUIContent() {
                image = EditorGUIUtility.IconContent("PolygonCollider2D Icon").image,
                tooltip = "Create Vertice Area"
            };
        }
        #endregion

        #region Folder & Asset
        [Serializable]
        private sealed class Folder {
            public string Name = string.Empty;
            public bool Foldout = false;

            public List<Folder> Folders = new List<Folder>();
            public List<Asset>  Assets  = new List<Asset>();

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public Folder(string _name, bool _open) {
                Name    = _name;
                Foldout = _open;
            }

            // -------------------------------------------
            // Utility
            // -------------------------------------------

            public void RegisterAsset(string[] _directories, string _fullPath, int _index, List<string> _openFolders) {
                if (_index == (_directories.Length - 1)) {

                    // Register new asset.
                    foreach (Asset _asset in Assets) {
                        if (_asset.Path.Equals(_fullPath, StringComparison.Ordinal))
                            return;
                    }

                    Assets.Add(new Asset(_fullPath));
                    return;
                }

                // Sub folder.
                string _directory = _directories[_index];
                foreach (Folder _folder in Folders) {

                    if (_folder.Name.Equals(_directory, StringComparison.Ordinal)) {
                        _folder.RegisterAsset(_directories, _fullPath, _index + 1, _openFolders);
                        return;
                    }
                }

                Folder _newFolder = new Folder(_directory, _openFolders.Contains(_directory));
                Folders.Add(_newFolder);

                _newFolder.RegisterAsset(_directories, _fullPath, _index + 1, _openFolders);
            }
        }

        [Serializable]
        private sealed class Asset {
            public string Name = string.Empty;
            public string Path = string.Empty;

            public Texture Icon = null;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public Asset(string _path) {
                Name = $" {System.IO.Path.GetFileNameWithoutExtension(_path)}";
                Path = _path;
            }
        }
        #endregion

        #region Mesh Infos
        private sealed class MeshInfo {
            public Material[] Materials  = null;
            public Transform Transform   = null;

            public SpriteRenderer Sprite = null;
            public Mesh Mesh = null;

            public Bounds Bounds = new Bounds();
            public bool IsSprite = false;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public MeshInfo(MeshFilter _mesh) : this(_mesh, _mesh.sharedMesh, null, false) {

                if (_mesh.TryGetComponent(out MeshRenderer _meshRenderer)) {
                    Materials = _meshRenderer.sharedMaterials;
                }

                Bounds = _mesh.GetComponent<Renderer>().bounds;
            }

            public MeshInfo(SkinnedMeshRenderer _mesh) : this(_mesh, _mesh.sharedMesh, null, false) {
                Materials = _mesh.sharedMaterials;
                Bounds    = _mesh.GetComponent<Renderer>().bounds;
            }

            public MeshInfo(SpriteRenderer _sprite) : this(_sprite, null, _sprite, true) {
                Materials = _sprite.sharedMaterials;
                Bounds    = _sprite.bounds;
            }

            private MeshInfo(Component _component, Mesh _mesh, SpriteRenderer _sprite, bool _isSprite) {
                Transform = _component.transform;
                IsSprite  = _isSprite;

                Mesh   = _mesh;
                Sprite = _sprite;
            }
        }
        #endregion

        #region Save Data
        [Serializable]
        private class SavableData {
            [Serializable]
            public class HistoryAsset {
                public string AssetPath = string.Empty;
                public int Category     = -1;

                [NonSerialized] public GameObject Asset = null;

                // -------------------------------------------
                // Constructor(s)
                // -------------------------------------------

                public HistoryAsset(string _path, int _category, GameObject _asset) {
                    AssetPath = _path;
                    Category  = _category;

                    Asset = _asset;
                    //LoadAsset();
                }

                // -------------------------------------------
                // Utility
                // -------------------------------------------

                public bool LoadAsset() {
                    if (!AssetDatabase.AssetPathExists(AssetPath))
                        return false;

                    if (AssetDatabase.LoadMainAssetAtPath(AssetPath) is GameObject _asset) {
                        Asset = _asset;
                        return true;
                    }

                    return false;
                }
            }

            [Serializable]
            public class Category {
                public List<HistoryAsset> History = new List<HistoryAsset>();
                public List<string> OpenFolders   = new List<string>();
                public string SelectedAssetPath   = string.Empty;

                // -------------------------------------------
                // Utility
                // -------------------------------------------

                public void RefreshHistory() {
                    for (int i = History.Count; i-- > 0;) {
                        if (!History[i].LoadAsset()) {
                            History.RemoveAt(i);
                        }
                    }
                }
            }

            // ===== Content ===== \\

            public List<HistoryAsset> History = new List<HistoryAsset>();
            public List<Category> Categories  = new List<Category>();

            public string SelectedAssetPath = string.Empty;
            public int SelectedCategory     = 0;
            public int EnabledCategory      = -1;

            // -----------------------

            public void RefreshCategories(EnhancedSceneViewEnhancedSettings _settings) {//
                int _count = _settings.Categories.Count + 2;

                while (Categories.Count < _count) {
                    saveData.Categories.Add(new Category());
                }

                while (Categories.Count > _count) {
                    saveData.Categories.RemoveLast();
                }
            }

            public void RefreshHistory() {
                for (int i = Categories.Count; i-- > 0;) {
                    Categories[i].RefreshHistory();
                }

                for (int i = History.Count; i-- > 0;) {
                    if (!History[i].LoadAsset()) {
                        History.RemoveAt(i);
                    }
                }
            }
        }
        #endregion

        // --- Dockable Window --- \\

        #region Global Members
        private const int HistoryMaxCount = 25;
        private const string SaveDataKey  = "SceneDesignerData";

        private static readonly SavableData saveData = new SavableData();
        private static readonly List<Folder> roots   = new List<Folder>();

        private static GameObject selectedAsset = null;

        // -----------------------

        public static bool IsPlacingTemplate {
            get { return EnabledCategory != -1; }
        }

        public static int EnabledCategory {
            get { return saveData.EnabledCategory; }
            private set {
                saveData.EnabledCategory = value;
                SaveData();
            }
        }

        public static int CategoryCount {
            get { return saveData.Categories.Count; }
        }

        public static string SelectedAssetPath {
            get { return saveData.SelectedAssetPath; }
            private set {
                saveData.SelectedAssetPath = value;
                SaveData();
            }
        }

        // -------------------------------------------
        // Initialization
        // -------------------------------------------

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // Loads session values.
            string _dataJson = EditorPrefs.GetString(SaveDataKey, string.Empty);
            if (!string.IsNullOrEmpty(_dataJson)) {

                EditorJsonUtility.FromJsonOverwrite(_dataJson, saveData);
                saveData.RefreshHistory();
            }

            saveData.RefreshCategories(EnhancedSceneViewEnhancedSettings.Settings);
            SetEnableCategory(EnabledCategory);

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
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
        private const string UndoRecordTitle = "Scene Designer Window Change";

        private static readonly GUIContent[] tabsGUI = new GUIContent[] {
            new GUIContent("Scene Designer"),
            new GUIContent("Settings"),
        };

        private static readonly GUIContent generalHeaderGUI = new GUIContent("GENERAL", "General settings");

        private static SceneDesigner instance = null;

        [SerializeField] private int selectedTabIndex = 0;

        // -----------------------

        private void OnEnable() {
            titleContent.image = EditorGUIUtility.FindTexture("Grid.PaintTool");
            instance = this;
        }

        private void OnGUI() {
            Undo.RecordObject(this, UndoRecordTitle);
            GUILayout.Space(10f);

            selectedTabIndex = EnhancedEditorGUILayout.CenteredToolbar(selectedTabIndex, tabsGUI, GUI.ToolbarButtonSize.FitToContents, GUILayout.Height(ToolbarHeight));

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck   = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);

                    switch (selectedTabIndex) {

                        case 0:
                            DrawSceneDesigner();
                            break;

                        case 1:
                            EnhancedSceneViewEnhancedSettings.DrawSceneDesignerSettings(generalHeaderGUI);
                            break;

                        default:
                            break;
                    }

                    // Refresh on change.
                    if (_changeCheck.changed) {
                        EnhancedEditorUserSettings.Instance.Save();
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

        #region Scene Designer
        private const float HistoricalHeight = 75f;
        private const float SelectionMargins = 2f;

        private const string HelpBoxMessage = "Hold the { <color=yellow>Ctrl</color> } key to rotate the object and the { <color=yellow>Shift</color> } key to modify its scale.\n" +
                                              "Reset all Transform values with { <color=yellow>Tab</color> }.";

        private static readonly GUIContent historicalHeaderGUI = new GUIContent("RECENTLY USED", "Recently used assets");
        private static readonly GUIContent placementHeaderGUI  = new GUIContent("PLACEMENT", "Placement additional options");

        private static readonly GUIContent enabledGUI          = new GUIContent("Enabled", "Toggles the Scene Designer activation");
        private static readonly GUIContent removeGUI           = new GUIContent("Remove Object", "Removes this object from the recently used list");
        private static readonly GUIContent clearGUI            = new GUIContent("Clear", "Clear the history and removes all objects from the recently used list");
        private static readonly GUIContent pingGUI             = new GUIContent("Ping Asset", "Pings this asset in the Project Browser window");

        private static readonly Color historicalBackgroundColor = SuperColor.DarkGrey.Get();
        private static readonly Color selectionColor = SuperColor.Sapphire.Get();

        private Vector2 historicalScroll = Vector2.zero;

        // -----------------------

        private void DrawSceneDesigner() {

            // Activation.
            bool _isEnabled = EditorGUILayout.Toggle(enabledGUI, IsPlacingTemplate);
            if (_isEnabled != IsPlacingTemplate) {
                SetEnableCategory(_isEnabled ? saveData.SelectedCategory : -1);
            }

            // Historical.
            ref var _history = ref saveData.History;
            int _count = _history.Count;

            if (_count != 0) {
                GUILayout.Space(5f);
                EnhancedEditorGUILayout.UnderlinedLabel(historicalHeaderGUI, EditorStyles.boldLabel);
                GUILayout.Space(5f);

                using (var _scrollScope = new EditorGUILayout.ScrollViewScope(historicalScroll, false, false, GUILayout.Height(HistoricalHeight + 13f)))
                using (var _scope       = new EditorGUILayout.HorizontalScope(GUILayout.Height(HistoricalHeight))) {
                    historicalScroll = _scrollScope.scrollPosition;

                    EditorGUI.DrawRect(_scope.rect, historicalBackgroundColor);
                    GUILayout.Space(5f);

                    Rect _origin = EditorGUILayout.GetControlRect(false, HistoricalHeight - 5f);
                    Rect _position = new Rect(_origin) {
                        width = _origin.height
                    };

                    // Draw historical.
                    for (int i = _count; i-- > 0;) {

                        var _historyData = _history[i];
                        GameObject _asset = _historyData.Asset;

                        if (_asset == null) {
                            _history.RemoveAt(i);
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
                        if (IsPlacingTemplate && (selectedAsset == _asset)) {
                            Rect _temp = new Rect(_position);

                            _temp.xMin -= SelectionMargins;
                            _temp.xMax += SelectionMargins;
                            _temp.yMin -= SelectionMargins;
                            _temp.yMax += SelectionMargins;

                            EditorGUI.DrawRect(_temp, selectionColor);
                        }

                        // Error can be thrown at startup.
                        try {
                            EditorGUI.DrawPreviewTexture(_position, _icon);
                        } catch (NullReferenceException) { }

                        if (EnhancedEditorGUIUtility.ContextClick(_position)) {

                            // Menu.
                            int _index = i;

                            GenericMenu _menu = new GenericMenu();
                            _menu.AddItem(pingGUI,   false, () => EditorGUIUtility.PingObject(_asset));
                            _menu.AddItem(removeGUI, false, () => RemoveHistoryObject(_index));

                            _menu.ShowAsContext();
                        }
                         else if (EnhancedEditorGUIUtility.MouseDown(_position)) {

                            // Selection.
                            SelectAsset(_asset, _historyData.Category, false, true);
                        }

                        if (i != 0) {
                            _position.x += _position.width + 7f;
                        }
                    }

                    GUILayoutUtility.GetRect((_position.xMax - _origin.xMax) - 60f, 1f);
                    EditorGUILayout.LabelField("");
                }

                using (var scope = new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(clearGUI, EditorStyles.miniButtonRight, GUILayout.Width(85f))) {
                        ClearHistory();
                    }
                }
            }

            GUILayout.Space(10f);

            // Options.
            EnhancedEditorGUILayout.UnderlinedLabel(placementHeaderGUI, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            EnhancedSceneViewEnhancedSettings.DrawPlacementSettings();

            GUILayout.Space(5f);
            EnhancedEditorGUILayout.HorizontalLine(SuperColor.DarkGrey.Get(), GUILayout.Height(1f));
            GUILayout.Space(2f);
            
            using (var _scope = EnhancedGUI.GUIStyleRichText.Scope(EditorStyles.helpBox, true)) {
                EditorGUILayout.HelpBox(HelpBoxMessage, UnityEditor.MessageType.Info);
            }
        }
        #endregion

        // --- Scene GUI --- \\

        #region Scene GUI
        private const EventModifiers RotateModifier = EventModifiers.Control;
        private const EventModifiers ScaleModifier  = EventModifiers.Shift;

        private const KeyCode DisableKey = KeyCode.Escape;
        private const KeyCode ResetKey   = KeyCode.Tab;

        private const int ControlID = 167213;

        private static List<MeshInfo> meshInfos = new List<MeshInfo>();
        private static GameObject newInstance   = null;

        private static Vector3 worldPosition = Vector3.zero;
        private static Quaternion rotation   = Quaternion.identity;
        private static Vector3 scale         = Vector3.one;

        private static Bounds assetBounds = default;
        private static Mesh quadMesh = null;

        private static Vector3[] areaRectangleVerticeBuffer = new Vector3[4];
        private static List<Vector3> areaVertices = new List<Vector3>();

        // -----------------------

        public static bool IsDrawingArea {
            get { return EnabledCategory == (CategoryCount - 1); }
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        private static void OnSceneGUI(SceneView _sceneView) {
            
            // Trigger.
            if (IsDrawingArea) {
                OnSceneGUIDrawArea(_sceneView);
                return;
            }

            // Template placer.
            if (IsPlacingTemplate) {
                OnSceneGUIPlaceTemplate(_sceneView);
                return;
            }
        }

        private static void OnSceneGUIDrawArea(SceneView _sceneView) {
            EnhancedSceneViewEnhancedSettings _settings = EnhancedSceneViewEnhancedSettings.Settings;
            Event _event = Event.current;

            // Alter cursor in scene view.
            Rect _cursorRect = new Rect(Vector2.zero, SceneView.lastActiveSceneView.position.size);
            _cursorRect.yMin += EnhancedEditorToolbar.ToolbarHeight - 5f;

            EditorGUIUtility.AddCursorRect(_cursorRect, MouseCursor.Link);
            bool _isValidPosition = (mouseOverWindow == _sceneView) && _cursorRect.Contains(_event.mousePosition);

            // Get current cursor world position.
            Vector3 _worldPosition;
            if (_isValidPosition) {
                GetMouseWorldPosition(_event, _settings, out _worldPosition);
            } else {
                _worldPosition = worldPosition;
            }

            Color _verticeColor = VerticeAreaUtility.AreaPreviewPointColor;
            Color _lineColor    = VerticeAreaUtility.AreaPreviewLineColor;
            float _verticeSize  = VerticeAreaUtility.AreaPointHandlesSize;
            float _lineSize     = VerticeAreaUtility.AreaLineRectSize;

            // Draw preview.
            if (_isValidPosition) {
                using (var _scope = EnhancedGUI.HandlesColor.Scope(_verticeColor)) {
                    Handles.SphereHandleCap(0, _worldPosition, Quaternion.identity, _verticeSize, EventType.Repaint);
                }
            }

            ref List<Vector3> _vertices = ref areaVertices;

            // Hot key.
            if (_event.isKey) {

                switch (_event.keyCode) {

                    // Reset rotation and scale.
                    case ResetKey:
                        _vertices.RemoveLast();
                        break;

                    // Disable.
                    case DisableKey:
                        SetEnableCategory(-1);
                        return;

                    default:
                        break;
                }
            }

            // Mouse input management.
            if (_isValidPosition && MouseDown(_event)) {

                // Complete on loop - create instance and set area vertices.
                if (_vertices.SafeFirst(out Vector3 _origin) && ((_origin - _worldPosition).magnitude < _verticeSize)) {

                    Transform _transform = CreateObject(_event, _settings);

                    if (_transform.TryGetComponentInChildren(out IVerticeArea _area)) {

                        Vector3 _position = Vector3.zero;
                        for (int i = _vertices.Count; i-- > 0;) {
                            _position += _vertices[i];
                        }

                        _position /= _vertices.Count;
                        for (int i = _vertices.Count; i-- > 0;) {
                            _vertices[i] -= _position;
                        }

                        _transform.position = _position;

                        _area.SetAreaVertices(_vertices);
                        _area.Pin(true);
                    }

                    _vertices.Clear();
                } else {
                    // Register new vertice.
                    _vertices.Add(_worldPosition);
                    _event.Use();
                }
            }

            MouseUp(_event);

            // Repaint on mouse movement.
            if (_event.type == EventType.MouseMove) {
                _sceneView.Repaint();
            }

            var _zTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            // Draw Area.
            int _verticeCount = _vertices.Count;
            if (_verticeCount == 0) {
                return;
            }

            using (var _scope = EnhancedGUI.HandlesColor.Scope(_verticeColor)) {
                for (int i = _vertices.Count; i-- > 0;) {
                    Handles.SphereHandleCap(0, _vertices[i], Quaternion.identity, _verticeSize, EventType.Repaint);
                }
            }

            ref Vector3[] _buffer = ref areaRectangleVerticeBuffer;

            using (var _scope = EnhancedGUI.HandlesColor.Scope(_lineColor)) {
                for (int i = _vertices.Count; i-- > 1;) {

                    Vector3 _pointA = _vertices[i];
                    Vector3 _pointB = _vertices[i - 1];

                    Vector3 _right = (_pointA - _pointB).normalized;
                    Quaternion _rotation = Quaternion.FromToRotation(Vector3.right, _right);

                    Vector3 _offsetA = new Vector3(0f, _lineSize, 0f).Rotate(_rotation);
                    Vector3 _offsetB = new Vector3(0f, 0f, _lineSize).Rotate(_rotation);

                    DrawRectangle(ref _buffer, _pointA, _pointB, _offsetA, _lineColor);
                    DrawRectangle(ref _buffer, _pointA, _pointB, _offsetB, _lineColor);
                }
            }

            Handles.zTest = _zTest;

            // ----- Local Method ----- \\

            static void DrawRectangle(ref Vector3[] _verticeBuffer, Vector3 _pointA, Vector3 _pointB, Vector3 _offset, Color _lineColor) {
                _verticeBuffer[0] = _pointA + _offset;
                _verticeBuffer[1] = _pointB + _offset;
                _verticeBuffer[2] = _pointB - _offset;
                _verticeBuffer[3] = _pointA - _offset;

                Handles.DrawSolidRectangleWithOutline(_verticeBuffer, _lineColor, _lineColor);
            }
        }

        private static void OnSceneGUIPlaceTemplate(SceneView _sceneView) {
            // Destroyed asset management.
            if (selectedAsset == null) {
                return;
            }

            EnhancedSceneViewEnhancedSettings _settings = EnhancedSceneViewEnhancedSettings.Settings;
            Event _event = Event.current;

            bool _isRotating = _event.modifiers.HasFlagUnsafe(RotateModifier);
            bool _isScaling  = _event.modifiers.HasFlagUnsafe(ScaleModifier);

            if ((mouseOverWindow != _sceneView) && !_isRotating && !_isScaling)
                return;

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
                        SetEnableCategory(-1);
                        return;

                    default:
                        break;
                }
            }

            // Create and place a new instance on click.
            if (MouseDown(_event) && !_isRotating && !_isScaling) {
                CreateObject(_event, _settings);
            }

            MouseUp(_event);

            // Get the asset preview position in world space according to the user mouse position.
            if (!_isRotating && !_isScaling) {
                GetMouseWorldPosition(_event, _settings, out worldPosition);
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

            // Repaint on mouse movement.
            if (_event.type == EventType.MouseMove) {
                _sceneView.Repaint();
            }

            // Draw the selected mesh on camera.
            if (((GUIUtility.hotControl != 0) && !_isRotating && !_isScaling) || (SceneView.lastActiveSceneView != _sceneView))
                return;

            Camera _camera = _sceneView.camera;
            Transform _assetTransform = selectedAsset.transform;
            Matrix4x4 _matrix = new Matrix4x4();

            foreach (MeshInfo _mesh in meshInfos) {

                Quaternion _parentRotation = _assetTransform.rotation * rotation;
                Quaternion _localRotation  = _mesh.Transform.rotation * Quaternion.Inverse(_assetTransform.rotation);
                Quaternion _rotation       = _parentRotation * _localRotation;

                Vector3 _offset = _parentRotation * Vector3.Scale(_assetTransform.InverseTransformPoint(_mesh.Transform.position), scale);
                Vector3 _scale  = Vector3.Scale(_mesh.Transform.lossyScale, scale);

                // Set the matrix to use for preview
                Vector3 _position = worldPosition + _offset;
                _matrix.SetTRS(_position, _rotation, _scale);

                Material[] _materials = _mesh.Materials;

                for (int i = _materials.Length; i-- > 0;) {
                    Material _material = _materials[i];

                    if (_mesh.IsSprite) {
                        DrawSprite(_mesh.Sprite, _matrix, _position, _material, 2, _camera, i);
                    } else {
                        Graphics.DrawMesh(_mesh.Mesh, _matrix, _material, 2, _camera, i);
                    }
                }
            }
        }

        // -------------------------------------------
        // Placing
        // -------------------------------------------

        private static bool MouseDown(Event _event) {
            return (GUIUtility.hotControl == 0) && (_event.type == EventType.MouseDown) && (_event.button == 0);
        }

        private static bool MouseUp(Event _event) {
            if ((GUIUtility.hotControl == ControlID) && ((_event.type == EventType.Used) || (_event.type == EventType.MouseUp)) && (newInstance != null)) {

                GUIUtility.hotControl = 0;
                _event.Use();

                newInstance = null;
                return true;
            }

            return false;
        }

        private static Transform CreateObject(Event _event, EnhancedSceneViewEnhancedSettings _settings) {

            GameObject _instance = EnhancedEditorUtility.CreateObject(selectedAsset, selectedAsset.name, Selection.activeGameObject, false);
            Transform _transform = _instance.transform;

            _transform.position   = worldPosition;
            _transform.rotation  *= rotation * _settings.RandomRotation;
            _transform.localScale = Vector3.Scale(_transform.localScale, scale * _settings.RandomScale);

            GUIUtility.hotControl = ControlID;

            Undo.RegisterCreatedObjectUndo(_instance, "New Object Instantiation");
            newInstance = _instance;

            _event.Use();
            return _transform;
        }

        private static bool GetMouseWorldPosition(Event _event, EnhancedSceneViewEnhancedSettings _settings, out Vector3 _position) {

            Ray _worldRay = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
            if (Physics.Raycast(_worldRay, out RaycastHit _hit, 1000f, _settings.SnapMask)) {

                // Get the hit normal, rounded to the nearest int for each axis
                Vector3 _roundedNormal = new Vector3(Mathf.RoundToInt(_hit.normal.x),
                                                         Mathf.RoundToInt(_hit.normal.y),
                                                         Mathf.RoundToInt(_hit.normal.z));

                // Set the preview position relative to the virtual collider
                //worldPosition = (_hit.point - Vector3.Scale(assetBounds.center, scale)) + Vector3.Scale(assetBounds.extents, Vector3.Scale(_roundedNormal, scale));
                //Debug.Log("Position => " + worldPosition.ToStringX(5) + " - " + assetBounds.center.ToStringX(5) + " - " + assetBounds.extents.ToStringX(5));

                _position = _hit.point;
                return true;
            }

            _position = _worldRay.origin + (_worldRay.direction * 25f);
            return false;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> enabled state.
        /// </summary>
        /// <param name="_enableCategory">Index of the currently enable category</param>
        public static void SetEnableCategory(int _enableCategory, bool _reloadFromHistory = false) {

            if ((_enableCategory != -1) && (_reloadFromHistory || (selectedAsset == null))) {

                string _assetPath = saveData.Categories[_enableCategory].SelectedAssetPath;

                if (!string.IsNullOrEmpty(_assetPath)) {
                    SelectAsset(_assetPath, _enableCategory, false, false);
                }

                if (_enableCategory != EnabledCategory) {
                    SceneDesignerQuickAccess.GetWindow(_enableCategory);
                }

                return;
            }

            EnabledCategory = _enableCategory;
            if (IsDrawingArea) {
                areaVertices.Clear();
            }

            // Repaint.
            if (instance != null) {
                instance.Repaint();
            }

            EnhancedEditorToolbar.Repaint();
        }

        /// <inheritdoc cref="SelectAsset(string, int, bool, bool)"/>
        /// <param name="_asset">New selected asset.</param>
        public static void SelectAsset(GameObject _asset, int _category, bool _refreshGlobalHistory = true, bool _refreshCategoryHistory = true) {
            string _path = AssetDatabase.GetAssetPath(_asset);

            if (!string.IsNullOrEmpty(_path)) {
                SelectAsset(_path, _category, _refreshGlobalHistory, _refreshCategoryHistory);
            }
        }

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> currently selected asset.
        /// </summary>
        /// <param name="_assetPath">New selected asset path.</param>
        public static void SelectAsset(string _assetPath, int _category, bool _refreshGlobalHistory = true, bool _refreshCategoryHistory = true) {
            GameObject _asset = AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath);

            if (_asset == null)
                return;

            Transform _transform = _asset.transform;

            // Get mesh infos.
            SkinnedMeshRenderer[] _meshRenderers   = _asset.GetComponentsInChildren<SkinnedMeshRenderer>();
            SpriteRenderer[]      _spriteRenderers = _asset.GetComponentsInChildren<SpriteRenderer>();
            MeshFilter[]          _meshFilters     = _asset.GetComponentsInChildren<MeshFilter>();

            meshInfos.Clear();

            foreach (SkinnedMeshRenderer _mesh in _meshRenderers) {
                meshInfos.Add(new MeshInfo(_mesh));
            }

            foreach (SpriteRenderer _sprite in _spriteRenderers) {
                meshInfos.Add(new MeshInfo(_sprite));
            }

            foreach (MeshFilter _mesh in _meshFilters) {
                meshInfos.Add(new MeshInfo(_mesh));
            }

            // Setup bounds.
            assetBounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (MeshInfo _mesh in meshInfos) {
                Bounds _bounds  = _mesh.Bounds;
                _bounds.center -= _transform.position;

                assetBounds.Encapsulate(_bounds);
            }

            // Select.
            saveData.SelectedCategory = _category;
            saveData.Categories[_category].SelectedAssetPath = _assetPath;

            SelectedAssetPath = _assetPath;
            selectedAsset     = _asset;

            // History.
            if (_refreshGlobalHistory) {
                RegisterInHistory(saveData.History, HistoryMaxCount);
            }
            if (_refreshCategoryHistory) {
                RegisterInHistory(saveData.Categories[_category].History, EnhancedSceneViewEnhancedSettings.Settings.QuickHistoryMaxCount);
            }

            SetEnableCategory(_category);
            SaveData();

            // Repaint.
            if (instance != null) {
                instance.Repaint();
            }

            // ----- Local Method ----- \\

            void RegisterInHistory(List<SavableData.HistoryAsset> _history, int _maxCount) {
                int _index = -1;

                // Find index.
                for (int i = _history.Count; i-- > 0;) {
                    if (_history[i].Asset == _asset) {

                        _index = i;
                        break;
                    }
                }

                // Register.
                switch (_index) {
                    case -1:
                        _history.Add(new SavableData.HistoryAsset(_assetPath, _category, _asset));
                        break;

                    default:
                        _history.Move(_index, _history.Count - 1);
                        break;
                }

                // Remove excess.
                while (_history.Count > _maxCount) {
                    _history.RemoveFirst();
                }
            }
        }

        // -------------------------------------------
        // Sprite
        // -------------------------------------------

        public static void DrawSprite(SpriteRenderer _sprite, Matrix4x4 _matrix, Vector3 _position, Material _material, int _layer, Camera _camera, int _subMeshIndex) {
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
            Mesh _mesh = new Mesh {
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

                uv = new Vector2[] {
                                new Vector2(0, 0),
                                new Vector2(0, 1),
                                new Vector2(1, 1),
                                new Vector2(1, 0),
                            }
            };

            return _mesh;
        }
        #endregion

        // --- Utility --- \\

        #region Utility
        private static List<string> folderBuffer = new List<string>() { string.Empty };

        // -----------------------

        /// <summary>
        /// Refreshes all categories folder content and asset.
        /// </summary>
        public static void RefreshAllFolders() {
            var _settings = EnhancedSceneViewEnhancedSettings.Settings;

            // Save data.
            saveData.RefreshCategories(_settings);

            // Main.
            RefreshCategoryFolders(0, ref _settings.Folders);

            // Additional.
            ref List<string> _folderSpan = ref folderBuffer;
            ref var _categorySpan = ref _settings.Categories.Array;

            int _count = _categorySpan.Length;

            for (int i = 0; i < _count; i++) {
                _folderSpan[0] = _categorySpan[i].Folder;
                RefreshCategoryFolders(i + 1, ref _folderSpan);
            }

            // Vertice area.
            _folderSpan[0] = _settings.VerticeAreaFolder;
            RefreshCategoryFolders(_count + 1, ref _folderSpan);
        }

        /// <inheritdoc cref="RefreshCategoryFolders(int, ref List{string})"/>
        public static void RefreshCategoryFolders(int _category) {

            var _settings = EnhancedSceneViewEnhancedSettings.Settings;

            if (_category == 0) {
                RefreshCategoryFolders(0, ref _settings.Folders);
                return;
            }

            ref List<string> _folderSpan = ref folderBuffer;
            _folderSpan[0] = (_category > _settings.Categories.Count) ? _settings.VerticeAreaFolder : _settings.Categories.Array[_category - 1].Folder;

            RefreshCategoryFolders(_category, ref _folderSpan);
        }

        /// <summary>
        /// Refreshes all folders of a specific category.
        /// </summary>
        public static void RefreshCategoryFolders(int _category, ref List<string> _folders) {

            while (roots.Count <= _category) {
                roots.Add(new Folder("Root", true));
            }

            Folder _root = roots[_category];
            _root.Folders.Clear();
            _root.Assets .Clear();

            int _count = _folders.Count;
            if (_count == 0)
                return;

            string[] _pathHelpers = new string[_count];
            for (int i = 0; i < _count; i++) {
                string _folder = _folders[i];
                if (string.IsNullOrEmpty(_folder))
                    return;

                string _fullPath = Path.Combine("Assets", _folder);
                _pathHelpers[i] = _fullPath;
            }

            // Load all objects.
            string _filter = $"t:{typeof(GameObject).Name}";
            string[] _assets = Array.ConvertAll(AssetDatabase.FindAssets(_filter, _pathHelpers), AssetDatabase.GUIDToAssetPath);

            for (int i = 0; i < _count; i++) {
                string _base = _folders[i].Split('/', '\\')[0];
                _pathHelpers[i] = string.IsNullOrEmpty(_base)
                                ? InternalEditorUtility.GetAssetsFolder()
                                : _base;
            }

            // Register each asset.
            List<string> _openFolders = saveData.Categories[_category].OpenFolders;

            foreach (string _path in _assets) {

                // Only register IVerticeArea objects.
                if (_category == (CategoryCount - 1)) {
                    GameObject _asset = AssetDatabase.LoadMainAssetAtPath(_path) as GameObject;

                    if (!_asset.TryGetComponentInChildren<IVerticeArea>(out _))
                        continue;
                }

                string[] _directories = _path.Split('/', '\\');
                int _index = 0;

                while (Array.IndexOf(_pathHelpers, _directories[_index].Trim()) == -1) {
                    _index++;
                }

                _root.RegisterAsset(_directories, _path, _index, _openFolders);
            }

            // Remove additional top folders.
            ref List<Folder> _subFolders = ref _root.Folders;

            while ((_subFolders.Count == 1) && (_subFolders[0].Assets.Count == 0)) {
                _subFolders.ReplaceBy(_subFolders[0].Folders);
            }
        }

        /// <summary>
        /// Removes the object at a given index from the recently used list.
        /// </summary>
        /// <param name="_index">Index of the item to remove.</param>
        public static void RemoveHistoryObject(int _index) {
            saveData.History.RemoveAt(_index);
            SaveData();
        }

        /// <summary>
        /// Clears and remove all object from the recently used list.
        /// </summary>
        public static void ClearHistory() {
            saveData.History.Clear();
            SaveData();
        }

        /// <summary>
        /// Registers and updates the current foldout state of a given folder.
        /// </summary>
        public static void RegisterOpenFolder(string _folderName, int _category, bool _foldout) {
            var _folders = saveData.Categories[_category].OpenFolders;

            if (_foldout) {
                _folders.AddIfNotExists(_folderName);
            } else {
                _folders.Remove(_folderName);
            }
            
            SaveData();
        }

        /// <summary>
        /// Saves all available session data.
        /// </summary>
        public static void SaveData() {
            EditorPrefs.SetString(SaveDataKey, EditorJsonUtility.ToJson(saveData));
        }
        #endregion

        // --- Sub Windows --- \\

        public sealed class SceneDesignerQuickAccess : EditorWindow {
            #region Window GUI
            public const float WindowHeightOffset = 52f;
            public const float WindowWidth = 400f;
            public const float LineHeight  = 18f;

            /// <summary>
            /// Creates and shows a new <see cref="SceneDesignerQuickAccess"/> window instance on screen.
            /// </summary>
            /// <returns><see cref="SceneDesignerQuickAccess"/> window instance on screen.</returns>
            public static SceneDesignerQuickAccess GetWindow(int _category) {
                SceneDesignerQuickAccess _window = CreateInstance<SceneDesignerQuickAccess>();

                Vector2 _position = SceneView.lastActiveSceneView.position.position;
                Rect    _lastRect = toolbarRect[_category];

                _position.x += _lastRect.position.x - 1f;
                _position.y += _lastRect.height + 51f;

                // Setup.
                _window.category   = _category;
                _window.historyMax = Mathf.Max(0, _window.History.Count - EnhancedSceneViewEnhancedSettings.Settings.QuickHistoryMaxCount);

                RefreshCategoryFolders(_category);

                // Display.
                Vector2 _size = new Vector2(WindowWidth, WindowHeightOffset + (LineHeight * _window.History.Count));
                _window.position = new Rect(_position, _size);

                _window.ShowPopup();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private static readonly GUIContent maximizeGUI = new GUIContent(string.Empty, "Opens the settings in another window");
            private static readonly GUIContent selectGUI   = new GUIContent("( Select new Prefab template )", "Opens an utility window to select a new Prefab for template");

            private new bool hasFocus = false;

            private int category = -1;
            int historyMax = 0;

            private List<SavableData.HistoryAsset> History {
                get { return saveData.Categories[category].History; }
            }

            // -----------------------

            private void OnEnable() {
                maximizeGUI.image = EditorGUIUtility.IconContent("winbtn_win_rest").image;
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

                // Picker button.
                if (GUILayout.Button(selectGUI, EditorStyles.miniButton)) {
                    SceneDesignerObjectPicker.GetWindow(category);
                    Close();
                }

                // Separtor.
                Rect _position = GUILayoutUtility.GetLastRect();

                _position.y += _position.height + 1f;
                _position.height = 1f;

                EnhancedEditorGUI.HorizontalLine(_position, SuperColor.Black.Get());
                GUILayout.Space(5f);

                // History.
                var _history = History;

                for (int i = _history.Count; i-- > historyMax;) {
                    GameObject _asset = _history[i].Asset;

                    Rect _fullRect = EditorGUILayout.GetControlRect(true, GUILayout.Height(LineHeight - EditorGUIUtility.standardVerticalSpacing));
                    EnhancedEditorGUI.BackgroundLine(_fullRect, i, _asset == selectedAsset, true);

                    Rect _labelRect = new Rect(_fullRect){
                        xMin = _fullRect.x + 20f
                    };

                    GUI.Label(_labelRect, _asset.name, EditorStyles.label);

                    if (_fullRect.Event(out Event _event) == EventType.MouseDown) {
                        switch (_event.clickCount) {

                            // Select.
                            case 1:
                                SelectAsset(_asset, category, true, false);
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

                // Utility buttons.
                GUILayout.Space(2f);
                using (var _horizontalScope = new EditorGUILayout.HorizontalScope()) {

                    // Preferences button.
                    _position = new Rect(_horizontalScope.rect) {
                        xMin = _horizontalScope.rect.xMax - 28f,
                        height = 20f
                    };

                    EnhancedEditorSettings.DrawUserSettingsButton(_position);

                    _position.x -= 30f;

                    if (EnhancedEditorGUI.IconButton(_position, maximizeGUI)) {
                        SceneDesigner.GetWindow();
                        return;
                    }

                    _position.xMin = 5f;
                    _position.width = position.width;

                    GUILayout.Label("");
                }

                GUILayout.Space(5f);
            }

            private void OnLostFocus() {
                if (!PreviewWindow.HasFocus) {
                    Close();
                }

                hasFocus = false;
            }
            #endregion

            #region Toolbar Extension
            private static readonly List<Rect> toolbarRect = new List<Rect>();

            // -----------------------

            [EditorToolbarLeftExtension(Order = 100)]
            #pragma warning disable
            private static void ToolbarExtension() {
                // Main.
                DrawCategory(0, Styles.ToolbarMainButtonGUI);

                // Additional categories.
                ref var _categories = ref EnhancedSceneViewEnhancedSettings.Settings.Categories.Array;
                int _count = _categories.Length;

                for (int i = 0; i < _count; i++) {
                    var _category = _categories[i];

                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_category.Icon, $"Create {_category.Name}");
                    DrawCategory(i + 1, _label);
                }

                // Vertice area.
                GUILayout.Space(10f);
                DrawCategory(_count + 1, Styles.ToolbarVerticeAreaButtonGUI);
            }

            private static void DrawCategory(int _index, GUIContent _label) {
                // Toggle.
                int _result = EnhancedEditorToolbar.DropdownToggle(EnabledCategory == _index, _label, GUILayout.Width(32f));

                if (Event.current.type == EventType.Repaint) {
                    Rect _rect = GUILayoutUtility.GetLastRect();

                    while (toolbarRect.Count <= _index) {
                        toolbarRect.Add(default);
                    }

                    toolbarRect[_index] = _rect;
                }

                switch (_result) {

                    // Enable toggle.
                    case 0:
                        SetEnableCategory((EnabledCategory == _index) ? -1 : _index, true);
                        break;

                    // Asset selection.
                    case 1:
                        SceneDesignerQuickAccess.GetWindow(_index);
                        break;

                    default:
                        break;
                }
            }
            #endregion
        }

        public sealed class SceneDesignerObjectPicker : EditorWindow {
            #region Window GUI
            private static readonly Vector2 windowSize = new Vector2(400f, 300f);

            /// <summary>
            /// Creates and shows a new <see cref="SceneDesignerObjectPicker"/> window instance on screen.
            /// </summary>
            /// <returns><see cref="SceneDesignerObjectPicker"/> window instance on screen.</returns>
            public static SceneDesignerObjectPicker GetWindow(int _category) {
                SceneDesignerObjectPicker _window = GetWindow<SceneDesignerObjectPicker>(true, "Select", true);
                //_window.position = new Rect(_window.position.position, windowSize);

                _window.category = _category;

                _window.ShowUtility();
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

            private static Vector2 scroll = new Vector2();
            private new bool hasFocus = false;
            private int category = -1;

            // -----------------------

            private void OnEnable() {
                RefreshAllFolders();
            }

            private void OnGUI() {

                using (var _scope = new GUILayout.ScrollViewScope(scroll)) {
                    scroll = _scope.scrollPosition;

                    // Content.
                    using (var _verticalScope = new GUILayout.VerticalScope()) {
                        // No asset message.
                        if (roots[category].Folders.Count == 0) {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox(NoAssetMessage, UnityEditor.MessageType.Info, true);

                            GUILayout.FlexibleSpace();
                            return;
                        }

                        GUILayout.Space(5f);

                        int _index = 0;
                        DrawFolder(roots[category], ref _index);

                        GUILayout.Space(5f);
                    }
                }

                Repaint();
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

                        bool _foldout = EditorGUI.Foldout(_position, _subfolder.Foldout, _label, true);
                        if (_foldout != _subfolder.Foldout) {

                            _subfolder.Foldout = _foldout;
                            RegisterOpenFolder(_subfolder.Name, category, _foldout);
                        }
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
                                    SelectAsset(_asset.Path, category, true, true);
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

                Rect _background = new Rect(_position) {
                    x = 0f,
                    width = position.width
                };

                _index++;

                // Line background.
                bool _selected = _isAsset && (SelectedAssetPath == _path);
                EnhancedEditorGUI.BackgroundLine(_background, _index, _selected, true);

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
        }

        private sealed class PreviewWindow : EditorWindow {
            #region Content
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
                Event _event = Event.current;
                Vector2 _position = GUIUtility.GUIToScreenPoint(_event.mousePosition);

                if (!screenPosition.Contains(_position)) {
                    Close();
                    return;
                }

                if (_event.type == EventType.Repaint) {
                    position = new Rect(_position, position.size);
                }

                Rect _temp = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUI.DrawPreviewTexture(_temp, preview);

                Repaint();
            }

            private void OnDisable() {
                HasFocus = false;
                FocusWindowIfItsOpen<SceneDesignerQuickAccess>();
            }
            #endregion
        }
    }
}
