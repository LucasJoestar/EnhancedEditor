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

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor toolbar extension used to easily select prefabs from the project and place them in the scene.
    /// </summary>
	public class SceneDesigner : EditorWindow
    {
        #region Folder & Asset
        private class Folder
        {
            public string Name = string.Empty;
            public bool Foldout = false;

            public List<Folder> Folders = new List<Folder>();
            public List<Asset> Assets = new List<Asset>();

            // -----------------------

            public Folder(string _name)
            {
                Name = _name;
            }

            // -----------------------

            public void RegisterAsset(string[] _directories, string _fullPath, int _index)
            {
                if (_index == (_directories.Length - 1))
                {
                    // Register new asset.
                    if (!Assets.Exists(a => a.Path == _fullPath))
                    {
                        Asset _asset = new Asset(_fullPath);
                        Assets.Add(_asset);
                    }

                    return;
                }

                string _directory = _directories[_index];
                foreach (Folder _folder in Folders)
                {
                    if (_folder.Name == _directory)
                    {
                        _folder.RegisterAsset(_directories, _fullPath, _index + 1);
                        return;
                    }
                }

                Folder _newFolder = new Folder(_directory);
                Folders.Add(_newFolder);

                _newFolder.RegisterAsset(_directories, _fullPath, _index + 1);
            }
        }

        private class Asset
        {
            public string Name = string.Empty;
            public string Path = string.Empty;

            public Texture Icon = null;

            // -----------------------

            public Asset(string _path)
            {
                Name = $" {System.IO.Path.GetFileNameWithoutExtension(_path)}";
                Path = _path;
            }
        }
        #endregion

        #region Mesh Infos
        private class MeshInfo
        {
            public Mesh Mesh = null;
            public Material[] Materials = null;
            public Transform Transform = null;

            public Bounds Bounds = new Bounds();

            // -----------------------

            public MeshInfo(MeshFilter _mesh)
            {
                Mesh = _mesh.sharedMesh;
                Transform = _mesh.transform;

                if (_mesh.TryGetComponent(out MeshRenderer _meshRenderer))
                    Materials = _meshRenderer.sharedMaterials;

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
        private const string EnabledKey = "SceneDesignerEnabled";
        private const string SelectedAssetKey = "SceneDesignerSelectedAsset";

        private static readonly GUIContent toolbarButton = new GUIContent();

        private static bool isEnabled = false;
        private static string selectedAssetPath = string.Empty;

        private static GameObject selectedAsset = null;

        // -----------------------

        static SceneDesigner()
        {
            toolbarButton.image = EditorGUIUtility.IconContent("PreMatCube").image;

            // Loads session values.
            bool _isEnabled = SessionState.GetBool(EnabledKey, isEnabled);

            SelectAsset(SessionState.GetString(SelectedAssetKey, selectedAssetPath));
            SetEnable(_isEnabled);
        }
        #endregion

        #region Behaviour
        private static MeshInfo[] meshInfos = null;
        private static GameObject newInstance = null;

        private static Vector3 worldPosition = default;
        private static Bounds assetBounds = default;

        // -----------------------

        private static void OnSceneGUI(SceneView _sceneView)
        {
            if (mouseOverWindow != _sceneView)
                return;

            Event _event = Event.current;

            // Create and place a new instance on click.
            if ((GUIUtility.hotControl != 0) && (_event.type == EventType.MouseDown) && (_event.button == 0))
            {
                newInstance = PrefabUtility.InstantiatePrefab(selectedAsset) as GameObject;
                newInstance.transform.position = worldPosition;

                EditorGUIUtility.PingObject(newInstance);
                Selection.activeObject = newInstance;

                Undo.RegisterCreatedObjectUndo(newInstance, "New Asset Instantiation");

                _event.Use();
            }
            else if ((_event.type == EventType.Used) && (newInstance != null))
            {
                Selection.activeObject = newInstance;
                newInstance = null;
            }

            // Get the asset preview position in world space according to the user mouse position.
            Ray _worldRay = HandleUtility.GUIPointToWorldRay(_event.mousePosition);
            if (Physics.Raycast(_worldRay, out RaycastHit _hit, 1000f))
            {
                // Get the hit normal, rounded to the nearest int for each axis
                Vector3 _roundedNormal = new Vector3(Mathf.RoundToInt(_hit.normal.x),
                                                     Mathf.RoundToInt(_hit.normal.y),
                                                     Mathf.RoundToInt(_hit.normal.z));

                // Set the preview position relative to the virtual collider
                worldPosition = (_hit.point - assetBounds.center) + Vector3.Scale(assetBounds.extents, _roundedNormal);
            }
            else
            {
                worldPosition = _worldRay.origin + (_worldRay.direction * 25f);
            }

            // Position handles.
            bool doDrawHandles = GUIUtility.hotControl == 0;
            #if SCENEVIEW_TOOLBAR
            doDrawHandles &= _event.mousePosition.y > 25f;
            #endif

            if (doDrawHandles)
            {
                Transform _transform = selectedAsset.transform;

                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.xAxisColor))
                {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.right), 1f, EventType.Repaint);
                }
                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.yAxisColor))
                {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.up), 1f, EventType.Repaint);
                }
                using (var _scope = EnhancedEditorGUI.HandlesColor.Scope(Handles.zAxisColor))
                {
                    Handles.ArrowHandleCap(0, worldPosition, Quaternion.LookRotation(_transform.forward), 1f, EventType.Repaint);
                }
            }

            _sceneView.Repaint();
        }

        private static void DrawMeshes(Camera _camera)
        {
            if ((GUIUtility.hotControl != 0) || (_camera != SceneView.lastActiveSceneView.camera) || !(mouseOverWindow is SceneView))
                return;

            Transform _transform = selectedAsset.transform;
            Matrix4x4 _matrix = new Matrix4x4();

            foreach (MeshInfo _mesh in meshInfos)
            {
                // Set the matrix to use for preview
                _matrix.SetTRS(worldPosition + _transform.InverseTransformPoint(_mesh.Transform.position),
                               _mesh.Transform.rotation,
                               _mesh.Transform.lossyScale);

                for (int _i = 0; _i < _mesh.Materials.Length; _i++)
                {
                    Material _material = _mesh.Materials[_i];
                    Graphics.DrawMesh(_mesh.Mesh, _matrix, _material, 2, _camera, _i);
                }

                Handles.DrawWireCube(worldPosition + _mesh.Bounds.center, _mesh.Bounds.size);
            }
        }

        // -----------------------

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> enabled state.
        /// </summary>
        /// <param name="_isEnable">Should the <see cref="SceneDesigner"/> be enabled?</param>
        public static void SetEnable(bool _isEnable)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Camera.onPreCull -= DrawMeshes;

            if (_isEnable)
            {
                if (string.IsNullOrEmpty(selectedAssetPath))
                {
                    GetWindow();
                    return;
                }

                SceneView.duringSceneGui += OnSceneGUI;
                Camera.onPreCull += DrawMeshes;
            }

            isEnabled = _isEnable;
            SessionState.SetBool(EnabledKey, _isEnable);

            EnhancedEditorToolbar.Repaint();
        }

        /// <inheritdoc cref="SelectAsset(string)"/>
        /// <param name="_asset">New selected asset.</param>
        public static void SelectAsset(GameObject _asset)
        {
            string _path = AssetDatabase.GetAssetPath(_asset);
            if (!string.IsNullOrEmpty(_path))
                SelectAsset(_path);
        }

        /// <summary>
        /// Set the <see cref="SceneDesigner"/> currently selected asset.
        /// </summary>
        /// <param name="_assetPath">New selected asset path.</param>
        public static void SelectAsset(string _assetPath)
        {
            GameObject _asset = AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath);
            if (_asset != null)
            {
                Transform _transform = _asset.transform;
                selectedAsset = _asset;

                MeshFilter[] _meshFilters = selectedAsset.GetComponentsInChildren<MeshFilter>();
                SkinnedMeshRenderer[] _meshRenderers = selectedAsset.GetComponentsInChildren<SkinnedMeshRenderer>();

                meshInfos = Array.ConvertAll(_meshFilters, (m) => new MeshInfo(m));
                ArrayUtility.AddRange(ref meshInfos, Array.ConvertAll(_meshRenderers, (m) => new MeshInfo(m)));

                assetBounds = new Bounds();

                foreach (MeshInfo _mesh in meshInfos)
                {
                    Bounds _bounds = _mesh.Bounds;
                    _bounds.center -= _transform.position;

                    assetBounds.Encapsulate(_bounds);
                }

                selectedAssetPath = _assetPath;
                SessionState.SetString(SelectedAssetKey, _assetPath);

                SetEnable(true);
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and shows a new <see cref="SceneDesigner"/> instance on screen.
        /// </summary>
        /// <returns><see cref="SceneDesigner"/> instance on screen.</returns>
        public static SceneDesigner GetWindow()
        {
            SceneDesigner _window = CreateInstance<SceneDesigner>();

            Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Vector2 _size = new Vector2(400f, 300f);

            _position.x -= 10f;
            _position.y = GUIUtility.GUIToScreenPoint(new Vector2(0f, 22f)).y;

            _window.position = new Rect(_position, _size);

            _window.ShowPopup();
            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string NoAssetMessage = "No asset could be found in the specified folders. " +
                                              "You can edit the scene designers folders using the button on the window top-right corner.";

        private static readonly Color indentColor = SuperColor.Grey.Get();

        private static Folder root = new Folder("Root");
        private static string[] folders = new string[] { };

        private static Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            string[] _folders = EnhancedEditorSettings.Settings.SceneDesignerFolders;
            if (_folders != folders)
            {
                root.Folders.Clear();

                if (_folders.Length == 0)
                    return;

                string[] _pathHelpers = new string[_folders.Length];
                for (int _i = 0; _i < _folders.Length; _i++)
                {
                    string _fullPath = Path.Combine("Assets", _folders[_i]);
                    _pathHelpers[_i] = _fullPath;
                }

                // Load all objects.
                string[] _assets = Array.ConvertAll(AssetDatabase.FindAssets("t:GameObject", _pathHelpers), AssetDatabase.GUIDToAssetPath);

                for (int _i = 0; _i < _folders.Length; _i++)
                {
                    string _base = _folders[_i].Split('/', '\\')[0];
                    _pathHelpers[_i] = string.IsNullOrEmpty(_base)
                                     ? InternalEditorUtility.GetAssetsFolder()
                                     : _base;
                }

                // Register each asset.
                foreach (string _path in _assets)
                {
                    string[] _directories = _path.Split('/', '\\');
                    int _index = 0;

                    while (Array.IndexOf(_pathHelpers, _directories[_index].Trim()) == -1)
                        _index++;

                    root.RegisterAsset(_directories, _path, _index);
                }

                folders = _folders;
            }
        }

        private void OnFocus()
        {
            if (!InternalEditorUtility.isApplicationActive)
                Close();
        }

        private void OnGUI()
        {
            if (!hasFocus && !PreviewWindow.HasFocus)
            {
                Close();
                return;
            }

            bool _drawIndent = Event.current.type == EventType.Repaint;
            int _index = 0;

            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;

                GUILayout.Space(2f);
                using (var _horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    // Preferences button.
                    Rect _position = new Rect(_horizontalScope.rect)
                    {
                        xMin = _horizontalScope.rect.xMax - 25f,
                        height = 20f
                    };

                    EnhancedEditorSettings.DrawPreferencesButton(_position);

                    GUILayout.Space(5f);
                    using (var _verticalScope = new GUILayout.VerticalScope())
                    {
                        // No asset message.
                        if (root.Folders.Count == 0)
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox(NoAssetMessage, UnityEditor.MessageType.Info, true);

                            GUILayout.FlexibleSpace();
                            return;
                        }

                        DrawFolder(root);
                    }
                }
            }

            Repaint();

            // ----- Local Methods ----- \\

            void DrawFolder(Folder _folder)
            {
                Rect _origin = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing));
                Rect _position = default;

                foreach (Folder _subfolder in _folder.Folders)
                {
                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_subfolder.Name, _subfolder.Name);
                    _position = GetRect(false);

                    using (var _noIndent = EnhancedEditorGUI.ZeroIndentScope())
                    {
                        _subfolder.Foldout = EditorGUI.Foldout(_position, _subfolder.Foldout, _label, true);
                    }

                    if (_subfolder.Foldout)
                    {
                        using (var _scope = new EditorGUI.IndentLevelScope())
                        {
                            DrawFolder(_subfolder);
                        }
                    }
                }

                _index = 0;

                foreach (Asset _asset in _folder.Assets)
                {
                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_asset.Name, _asset.Path);
                    _position = GetRect(true, _asset.Path);

                    using (var _noIndent = EnhancedEditorGUI.ZeroIndentScope())
                    {
                        EditorGUI.LabelField(_position, _label);

                        // Mini thumbnail.
                        if (_asset.Icon == null)
                        {
                            GameObject _object = AssetDatabase.LoadAssetAtPath<GameObject>(_asset.Path);
                            Texture2D _icon = AssetPreview.GetAssetPreview(_object);

                            if ((_icon == null) && !AssetPreview.IsLoadingAssetPreview(_object.GetInstanceID()))
                                _icon = AssetPreview.GetMiniThumbnail(_object);

                            _asset.Icon = _icon;
                        }
                        else
                        {
                            Rect _temp = new Rect(_position)
                            {
                                xMin = _position.xMax - (_position.height + 3f),
                                width = _position.height
                            };

                            EditorGUI.DrawPreviewTexture(_temp, _asset.Icon);

                            // Preview window.
                            if (!PreviewWindow.HasFocus && _temp.Contains(Event.current.mousePosition))
                            {
                                _temp.position += position.position;
                                PreviewWindow.GetWindow(_temp, _asset.Icon);
                            }
                        }

                        if (_position.Event(out Event _event) == EventType.MouseDown)
                        {
                            switch (_event.clickCount)
                            {
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
                if (_drawIndent && (EditorGUI.indentLevel != 0))
                {
                    Rect _temp = new Rect()
                    {
                        x = _origin.x - 9f,
                        y = _origin.y,
                        width = 2f,
                        yMax = _position.yMax - 8f
                    };

                    EditorGUI.DrawRect(_temp, indentColor);
                }
            }

            Rect GetRect(bool _isAsset, string _path = "")
            {
                Rect _position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                Rect _background = new Rect(_position)
                {
                    x = 0f,
                    width = position.width
                };

                if (_isAsset)
                {
                    EnhancedEditorGUI.BackgroundLine(_background, selectedAssetPath == _path, _index);
                    _index++;
                }

                // Horizontal indent.
                if (_drawIndent && (EditorGUI.indentLevel != 0))
                {
                    Rect _temp = new Rect()
                    {
                        x = _position.x - 7f,
                        y = _position.y + 8f,
                        width = 7f,
                        height = 2f
                    };

                    EditorGUI.DrawRect(_temp, indentColor);
                }

                return _position;
            }
        }

        private void OnLostFocus()
        {
            if (!PreviewWindow.HasFocus)
                Close();
        }
        #endregion

        #region Toolbar Extension
        [EditorToolbarLeftExtension(Order = 100)]
        private static void ToolbarExtension()
        {
            int _result = EnhancedEditorToolbar.DropdownToggle(isEnabled, toolbarButton, GUILayout.Width(32f));

            switch (_result)
            {
                // Designer enable state toggle.
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
        private class PreviewWindow : EditorWindow
        {
            public static PreviewWindow GetWindow(Rect _screenPosition, Texture _preview)
            {
                PreviewWindow _window = CreateInstance<PreviewWindow>();

                Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 _size = new Vector2(174f, 174f);

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

            private void OnEnable()
            {
                HasFocus = true;
            }

            private void OnGUI()
            {
                Vector2 _position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                if (!screenPosition.Contains(_position))
                {
                    Close();
                    return;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    position = new Rect(_position, position.size);
                }

                Rect _temp = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUI.DrawPreviewTexture(_temp, preview);
                
                Repaint();
            }

            private void OnDisable()
            {
                HasFocus = false;
                FocusWindowIfItsOpen<SceneDesigner>();
            }
        }
        #endregion
    }
}
