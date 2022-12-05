// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Static class used to draw over the hierarchy window for better layout and icons.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedHierarchy {
        #region Global Members
        private static readonly Dictionary<int, HierarchyProperty> itemInfos = new Dictionary<int, HierarchyProperty>();

        // -----------------------

        static EnhancedHierarchy() {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Editor GUI
        private static readonly List<Rect> indentPositions = new List<Rect>() { Rect.zero };

        private static readonly GUIContent coreSceneIcon = EditorGUIUtility.IconContent("sv_icon_dot13_pix16_gizmo");

        // -----------------------

        private static void OnHierarchyItemGUI(int _id, Rect _position) {
            // Activation.
            EnhancedHierarchyEnhancedSettings _settings = EnhancedHierarchyEnhancedSettings.Settings;
            if (!_settings.Enabled) {
                return;
            }

            // Ignore null items.
            if (_id == 0) {
                return;
            }

            if (!itemInfos.TryGetValue(_id, out HierarchyProperty _item)) {
                // Item registration.
                _item = new HierarchyProperty(HierarchyType.GameObjects);

                if (!_item.Find(_id, null)) {
                    Debug.Log("Not Found => " + _id);
                    return;
                }

                itemInfos.Add(_id, _item);
            }

            // Scene type icon.
            if (_item.isSceneHeader) {
                if (_item.GetScene().IsCoreScene()) {
                    _position.x -= 40f;
                    _position.width = 20f;

                    EditorGUI.LabelField(_position, coreSceneIcon);
                }
            } else {
                Rect _temp = new Rect(_position){
                    x = 0f,
                    width = Screen.width,
                };

                //EditorGUI.DrawRect(_temp, Color.green);
            }
            // Indent.
            DrawItemIndent(_position, _item);

            // Icon.
            DrawItemIcon(_id, _position);
        }

        private static void DrawItemIndent(Rect _position, HierarchyProperty _item) {
            // Indent position.
            Rect _indentPosition = new Rect(){
                x = _position.x - 20f,
                y = _position.y + (_position.height / 2f),
                width = 19f,
                height = 1f
            };

            // Item foldout shift.
            if (_item.hasChildren) {
                _indentPosition.width -= 11f;
            }

            // Indent dotted lines.
            if (Event.current.type == EventType.Repaint) {
                // Ignore scene headers.
                if (!_item.isSceneHeader) {
                    EnhancedEditorGUI.HorizontalDottedLine(_indentPosition, 1f, 1f);

                    while (_position.x < indentPositions.Last().x) {
                        indentPositions.RemoveLast();
                    }

                    // Vertical line.
                    if (!indentPositions.Last(out Rect _lastIndentPosition) || (_lastIndentPosition.y >= _position.y)) {
                        _lastIndentPosition = new Rect(_position.x, 0f, 1f, 1f);
                    }

                    _indentPosition = new Rect() {
                        x = _indentPosition.x - 2f,
                        y = _lastIndentPosition.y + 8f,
                        yMax = _indentPosition.y + 2f,
                        width = 1f
                    };

                    if (_position.x != _lastIndentPosition.x) {
                        _indentPosition.yMin += 8f;
                    }

                    EnhancedEditorGUI.VerticalDottedLine(_indentPosition, 1f, 1f);

                    // Only keep the last position for the same indent value.
                    if (_position.x == _lastIndentPosition.x) {
                        indentPositions.RemoveLast();
                    }
                } else {
                    _position.y += 2f;
                }

                indentPositions.Add(_position);
            }
        }

        private static void DrawItemIcon(int _id, Rect _position) {
            GameObject _object = EditorUtility.InstanceIDToObject(_id) as GameObject;

            if (!ReferenceEquals(_object, null) && _object.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                Texture _icon = _behaviour.icon;

                GUIContent _content = EnhancedEditorGUIUtility.GetLabelGUI(string.Empty);
                _content.image = _icon;

                _position.xMin = _position.xMax - 16f;

                EditorGUI.LabelField(_position, _content);
            }
        }
        #endregion

        #region Utility
        private static void OnHierarchyChanged() {
            itemInfos.Clear();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            itemInfos.Clear();
        }
        #endregion
    }
}
