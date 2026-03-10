// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

using ContextMenuDelegate = EnhancedEditor.Editor.SceneViewContextMenuItemAttribute.Delegate;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="SceneView"/> utility class, adding multiple shortcuts and menu options.
    /// </summary>
    [InitializeOnLoad]
    #pragma warning disable
    public static class EnhancedSceneView {
        #region Initialization
        static EnhancedSceneView() {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui   -= OnSceneGUI;
            SceneView.duringSceneGui   += OnSceneGUI;

            GetContextMenuDelegates();
            OnSelectionChanged();
        }
        #endregion

        #region Scene GUI
        private const float MaxCastDistance = 1000f;
        private static bool isContextClick  = false;

        private static readonly List<SkinnedMeshRenderer> skinnedMeshRendererBuffer = new List<SkinnedMeshRenderer>();
        private static readonly List<MeshRenderer>        meshRendererBuffer        = new List<MeshRenderer>();

        private static readonly List<Transform> selectionObjects = new List<Transform>();
        private static readonly List<Bounds>    selectionBounds  = new List<Bounds>();

        private static GameObject underMouseGameObject = null;
        private static Bounds     underMouseBounds     = default;

        // -----------------------

        private static void OnSceneGUI(SceneView _sceneView) {

            EnhancedSceneViewEnhancedSettings _settings = EnhancedSceneViewEnhancedSettings.Settings;
            Event _event = Event.current;

            DrawWireBox(_sceneView, _settings, _event);

            if (EditorWindow.mouseOverWindow != _sceneView) {
                return;
            }

            switch (_event.type) {

                // Prepare menu on mouse down.
                case EventType.MouseDown:
                    isContextClick = true;
                    break;

                case EventType.MouseUp:
                    // Context menu.
                    if (isContextClick && (_event.button == 1)) {

                        switch (_event.modifiers) {

                            // Standard create menu
                            case EventModifiers.Control:
                                MenuCommand _command = new MenuCommand(Selection.activeGameObject);
                                EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition, Vector2.one), "GameObject/", _command);
                                break;

                            // Custom menu.
                            case EventModifiers.None:
                            default:

                                // Require hit target.
                                if (!Physics.Raycast(HandleUtility.GUIPointToWorldRay(_event.mousePosition), out RaycastHit _hit, MaxCastDistance)) {
                                    return;
                                }

                                GenericMenu _menu = new GenericMenu();

                                foreach (ContextMenuDelegate _delegate in contextMenuDelegates) {
                                    _delegate(_sceneView, _menu, _hit);
                                }

                                _menu.ShowAsContext();
                                break;

                            // Ignore.
                            case EventModifiers.Alt:
                            case EventModifiers.Shift:
                            case EventModifiers.Command:
                            case EventModifiers.Numeric:
                            case EventModifiers.CapsLock:
                            case EventModifiers.FunctionKey:
                                return;
                        }

                        _event.Use();
                        _sceneView.Repaint();
                    }

                    isContextClick = false;
                    break;

                // Cancel menu when mouse moved.
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    isContextClick = false;
                    break;

                default:
                    break;
            }
        }

        // -------------------------------------------
        // View
        // -------------------------------------------

        public static void DrawWireBox(SceneView _sceneView, EnhancedSceneViewEnhancedSettings _settings, Event _event) {

            // Current selection wire box.
            if (_settings.DrawSelectionWire) {

                List<Bounds> _selectionBounds = selectionBounds;
                int _count = _selectionBounds.Count;

                if (_count != 0) {
                    using (var _scope = EnhancedGUI.HandlesColor.Scope(_settings.SelectionWireColor)) {

                        for (int i = _count; i-- > 0;) {
                            Bounds _bounds = _selectionBounds[i];
                            try {
                                Handles.DrawWireCube(_bounds.center + selectionObjects[i].position, _bounds.size);
                            } catch (MissingReferenceException) { }
                        }
                    }
                }
            }

            if (!_settings.DrawHoverWire || SceneDesigner.IsPlacingTemplate || (GUIUtility.hotControl != 0) || (EditorWindow.mouseOverWindow != _sceneView))
                return;

            // Mouse hover wire box.
            if (_event.type == EventType.MouseMove) {
                GameObject _go = HandleUtility.PickGameObject(_event.mousePosition, true);

                if (_go != null) {

                    if (underMouseGameObject != _go) {

                        if (PrefabUtility.IsPartOfPrefabInstance(_go)) {
                            _go = PrefabUtility.GetNearestPrefabInstanceRoot(_go);
                        }

                        underMouseBounds = GetObjectBounds(_go, skinnedMeshRendererBuffer, meshRendererBuffer);
                        underMouseBounds.extents -= .025f.ToVector3();
                    }
                }

                underMouseGameObject = _go;
            }

            switch (Tools.current) {

                case Tool.View:
                case Tool.Move:
                case Tool.Rotate:
                case Tool.Scale:
                case Tool.Rect:
                case Tool.Transform:
                case Tool.Custom:
                case Tool.None:

                    if (underMouseGameObject != null) {
                        using (var _scope = EnhancedGUI.HandlesColor.Scope(_settings.HoverWireColor)) {
                            Handles.DrawWireCube(underMouseBounds.center, underMouseBounds.size);
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static void OnSelectionChanged() {
            List<Transform> _selectionObjects = selectionObjects;
            List<Bounds>    _selectionBounds  = selectionBounds;

            _selectionObjects.Clear();
            _selectionBounds .Clear();

            GameObject[] _selection = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel);

            for (int i = _selection.Length; i-- > 0;) {

                GameObject _object = _selection[i];
                if (AssetDatabase.Contains(_object))
                    continue;

                Bounds _bounds = GetObjectBounds(_object, skinnedMeshRendererBuffer, meshRendererBuffer);
                _bounds.center -= _object.transform.position;

                _selectionObjects.Add(_object.transform);
                _selectionBounds .Add(_bounds);
            }
        }

        private static Bounds GetObjectBounds(GameObject _gameObject, List<SkinnedMeshRenderer> _skinnedMeshBuffer, List<MeshRenderer> _meshBuffer) {
            Bounds _bounds = new Bounds(_gameObject.transform.position, Vector3.zero);

            _gameObject.GetComponentsInChildren(_skinnedMeshBuffer);
            _gameObject.GetComponentsInChildren(_meshBuffer);

            for (int i = _skinnedMeshBuffer.Count; i-- > 0;) {
                _bounds.Encapsulate(_skinnedMeshBuffer[i].bounds);
            }

            for (int i = _meshBuffer.Count; i-- > 0;) {
                _bounds.Encapsulate(_meshBuffer[i].bounds);
            }

            return _bounds;
        }
        #endregion

        #region Context Menu
        public const string CreateGUI = "Create/";

        private static readonly GUIContent teleportHereGUI = new GUIContent("Teleport Here", "Teleport all selected GameObject(s) to this position");
        private static readonly GUIContent createEmptyGUI  = new GUIContent($"{CreateGUI}Empty", "Creates an empty GameObject at this position");

        private static ContextMenuDelegate[] contextMenuDelegates = new ContextMenuDelegate[0];

        // -------------------------------------------
        // Menu Items
        // -------------------------------------------

        [SceneViewContextMenuItem(Order = 10)]
        private static void TeleportHereItem(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            GameObject[] _selection = Selection.gameObjects;

            if (_selection.Length != 0) {
                _menu.AddItem(teleportHereGUI, false, Teleport);
            } else {
                _menu.AddDisabledItem(teleportHereGUI);
            }

            // ----- Local Method ----- \\

            void Teleport() {
                Vector3 _position = _hit.point;

                for (int i = _selection.Length; i-- > 0;) {
                    Transform _transform = _selection[i].transform;

                    Undo.RecordObject(_transform, "Teleport Object");
                    _transform.position = _position;
                }
            }
        }

        [SceneViewContextMenuItem(Order = -1)]
        private static void CreateObjectItem(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            _menu.AddItem(createEmptyGUI, false, CreateEmpty);
            _menu.AddSeparator(CreateGUI);
            _menu.AddSeparator(string.Empty);

            // ----- Local Method ----- \\

            void CreateEmpty() {
                EnhancedEditorUtility.CreateObject("GameObject", Selection.activeGameObject, _hit.point, false);
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static void GetContextMenuDelegates() {
            // Get matching methods.
            var _methods = TypeCache.GetMethodsWithAttribute<SceneViewContextMenuItemAttribute>();
            List<MethodInfo> _infos = new List<MethodInfo>();

            foreach (var _method in _methods) {
                if (_method.IsStatic) {
                    _infos.Add(_method);
                }
            }

            // Sort all methods by their order.
            _infos.Sort((a, b) => {
                var _aAttribute = a.GetCustomAttribute<SceneViewContextMenuItemAttribute>();
                var _bAttribute = b.GetCustomAttribute<SceneViewContextMenuItemAttribute>();

                return _aAttribute.Order.CompareTo(_bAttribute.Order);
            });

            // Then store their delegate.
            foreach (MethodInfo _info in _infos) {
                try {
                    ContextMenuDelegate _delegate = _info.CreateDelegate(typeof(ContextMenuDelegate)) as ContextMenuDelegate;
                    ArrayUtility.Add(ref contextMenuDelegates, _delegate);
                } catch (ArgumentException) { }
            }
        }
        #endregion

        #region Shortcut
        private const float DefaultDistance = 999;
        private static readonly Collider[] colliderBuffer = new Collider[16];
        private static readonly List<Collider> getColliderBuffer = new List<Collider>();

        // -----------------------

        /// <summary>
        /// Snaps selected object(s) to the nearest collider.
        /// </summary>
        [Shortcut("Enhanced Editor/Snap Object", KeyCode.PageDown)]
        private static void SnapSelection() {

            GameObject[] _selection = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel);
            for (int i = _selection.Length; i-- > 0;) {

                GameObject _gameObject = _selection[i];
                if (AssetDatabase.Contains(_gameObject))
                    continue;

                Collider _closest = null;
                Vector3 _normal   = default;
                float _distance   = DefaultDistance;

                Transform _transform = _gameObject.transform;
                Vector3 _position    = _transform.position;
                int _amount = Physics.OverlapSphereNonAlloc(_position, 10f, colliderBuffer);

                for (int j = 0; j < _amount; j++) {

                    Collider _collider = colliderBuffer[j];
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
                        _normal   = _point - _position;
                        _closest  = _collider;
                    }
                }

                // Extract from any overlapping collider.
                if (!Mathf.Approximately(_distance, DefaultDistance)) {

                    _gameObject.transform.position += _normal;
                    _gameObject.GetComponentsInChildren(getColliderBuffer);

                    foreach (Collider _collider in getColliderBuffer) {

                        if (_collider.isTrigger)
                            continue;

                        if (Physics.ComputePenetration(_collider, _collider.transform.position, _collider.transform.rotation,
                                                       _closest,  _closest .transform.position, _closest .transform.rotation, out _normal, out _distance)) {
                            _gameObject.transform.position += _normal * _distance;
                        }
                    }

                    getColliderBuffer.Clear();
                }
            }
        }
        #endregion
    }
}
