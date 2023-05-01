// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using ContextMenuDelegate = EnhancedEditor.Editor.SceneViewContextMenuItemAttribute.Delegate;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="SceneView"/> utility class, adding multiple shortcuts and menu options.
    /// </summary>
    [InitializeOnLoad]
    #pragma warning disable
    public static class SceneViewUtility {
        #region Global Members
        private static ContextMenuDelegate[] contextMenuDelegates = new ContextMenuDelegate[] { };

        // -----------------------

        static SceneViewUtility() {
            SceneView.duringSceneGui += OnSceneGUI;

            GetContextMenuDelegates();
        }
        #endregion

        #region Scene GUI
        private const float MaxCastDistance = 1000f;
        private static bool isContextClick = false;

        // -----------------------

        private static void OnSceneGUI(SceneView _sceneView) {
            if (EditorWindow.mouseOverWindow != _sceneView) {
                return;
            }

            Event _event = Event.current;

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

        // -----------------------

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

        #region Context Menu
        public const string CreateGUI = "Create/";

        private static readonly GUIContent createEmptyGUI = new GUIContent($"{CreateGUI}Empty", "Creates an empty GameObject at this position");
        private static readonly GUIContent teleportHereGUI = new GUIContent("Teleport Here", "Teleport all selected GameObject(s) to this position");

        // -----------------------

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

                foreach (GameObject _gameObject in _selection) {
                    Transform _transform = _gameObject.transform;

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
                EnhancedEditorUtility.CreateObject("GameObject", Selection.activeGameObject, _hit.point);
            }
        }
        #endregion
    }
}
