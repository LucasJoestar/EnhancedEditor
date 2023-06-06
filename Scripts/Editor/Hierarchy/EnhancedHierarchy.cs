// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  Uses reflection to know which object is selected,
//  because of the internal drag system.
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/TreeView/TreeViewController.cs
//
// ============================================================================ //

#if UNITY_2021_1_OR_NEWER
#define UNITY_2021
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_2021
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Static class used to draw over the hierarchy window for better layout and icons.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedHierarchy {
        /// <summary>
        /// Wrapper built around a <see cref="HierarchyProperty"/> and a <see cref="GameObject"/>,
        /// with none of them guaranteed to be valid.
        /// </summary>
        private class HierarchyObject {
            #region Global Members
            public readonly int ID = 0;

            private readonly HierarchyProperty property     = null;
            private readonly GameObject gameObject          = null;
            private readonly ExtendedBehaviour behaviour    = null;

            private readonly Texture icon                   = null;

            private readonly bool hasGameObject = false;
            private readonly bool hasBehaviour  = false;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public HierarchyObject(int _id) {
                ID = _id;

                property = new HierarchyProperty(HierarchyType.GameObjects);
                gameObject = EditorUtility.InstanceIDToObject(_id) as GameObject;

                hasGameObject = gameObject != null;

                // Extended behaviour.
                if (hasGameObject) {
                    behaviour = gameObject.GetExtendedBehaviour();
                    hasBehaviour = behaviour != null;

                    icon = PrefabUtility.GetIconForGameObject(gameObject);
                    EnhancedHierarchyEnhancedSettings.Settings.ReplaceGameObjectIcon(ref icon);
                }

                if (!property.Find(_id, treeView.expandedIDs.ToArray())) {
                    //Debug.Log("Not Found => " + _id);
                }
            }
            #endregion

            #region Utility
            // -------------------------------------------
            // Properties
            // -------------------------------------------

            public string Name {
                get {
                    if (property.isValid) {
                        return property.name;
                    }

                    if (hasGameObject) {
                        return gameObject.name;
                    }

                    return "Unknown";
                }
            }

            public bool IsSceneHeader {
                get {
                    return property.isValid
                         ? property.isSceneHeader
                         : false;
                }
            }

            public bool IsHierarchyHeader {
                get {
                    return hasBehaviour && behaviour.isHeader;
                }
            }

            public bool IsSelected {
                get {
                    return IsSelected(ID);
                }
            }

            public bool IsExpanded {
                get {
                    return treeView.expandedIDs.Contains(ID);
                }
            }

            public bool IsPrefabRoot {
                get {
                    return hasGameObject && PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
                }
            }

            public bool HasChildren {
                get {
                    if (property.isValid) {
                        return property.hasChildren;
                    }

                    if (hasGameObject) {
                        return gameObject.transform.childCount != 0;
                    }

                    return false;
                }
            }

            public Color LabelColor {
                get {
                    return (hasGameObject && !IsSelected && PrefabUtility.IsPartOfAnyPrefab(gameObject))
                     ? labelPrefabColor
                     : labelDefaultColor;
                }
            }

            public Texture Icon {
                get {
                    return icon;
                }
            }

            // -------------------------------------------
            // Methods
            // -------------------------------------------

            public bool GetGameObject(out GameObject _gameObject) {
                _gameObject = gameObject;
                return hasGameObject;
            }

            public bool GetBehaviour(out ExtendedBehaviour _behaviour) {
                _behaviour = behaviour;
                return hasBehaviour;
            }

            public bool GetScene(out Scene _scene) {
                if (property.isValid) {
                    // GetScene might throw an exception (e.g. in prefab mode).
                    try {
                        _scene = property.GetScene();
                        return true;
                    } catch (NullReferenceException) { }
                }

                _scene = default;
                return false;
            }

            public bool IsActiveInHierarchy(Rect _position) {
                bool _isPrefabRoot = !isSearchFilter && !property.isValid && IsRootObject(_position) && (PrefabStageUtility.GetCurrentPrefabStage() != null);
                return !hasGameObject || (gameObject.activeInHierarchy && !_isPrefabRoot);
            }

            public bool IsRootObject(Rect _position) {
                return hasGameObject && (_position.x == (LeftMargin + IndentWidth));
            }
            #endregion
        }

        #region Styles
        private static class Styles {
            public static readonly GUIStyle Label = new GUIStyle(EditorStyles.whiteLabel);

            // WhiteBoldLabel is displayed black (kinda strange), so use the WhiteLabel instead.
            public static readonly GUIStyle Header = new GUIStyle(EditorStyles.whiteLabel){
                fontStyle = FontStyle.Bold,
            };

            public static readonly GUIContent OpenPrefabGUI = EditorGUIUtility.IconContent("ArrowNavigationRight");
        }
        #endregion

        #region Initialization
        static EnhancedHierarchy() {
            EditorApplication.hierarchyWindowItemOnGUI  += OnHierarchyItemGUI;
            EditorApplication.hierarchyChanged          += OnHierarchyChanged;
            EditorApplication.playModeStateChanged      += OnPlayModeStateChanged;
            EditorApplication.update                    += OnUpdate;

            RefreshTreeView();
        }
        #endregion

        #region Editor GUI
        private const float LeftMargin      = 32f;
        private const float RootPositionX   = 60f;
        private const float OpenPrefabWidth = 20f;
        private const float IndentWidth     = 14f;
        private const float IconWidth       = 16f;

        private const string HierarchyWindowTitle = "Hierarchy";

        private static readonly Gradient headerOutlineGrad  = new Gradient() {
            colorKeys = new GradientColorKey[] { new GradientColorKey(SuperColor.DarkGrey.Get(), 1f) },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .2f), new GradientAlphaKey(1f, 1f) },
        };

        private static readonly Dictionary<int, HierarchyObject> objects = new Dictionary<int, HierarchyObject>();

        private static readonly List<Rect> indentPositions  = new List<Rect>() { Rect.zero };
        private static readonly Vector2 GradientMargins     = new Vector2(-15f, 22f);

        private static readonly Color dragPreviewColor      = new Color(1f, 1f, 1f, .1f);
        private static readonly Color selectionColor        = new Color(1f, 1f, 1f, .15f);
        private static readonly Color hoverColor            = new Color(1f, 1f, 1f, .07f);

        private static readonly Color labelDefaultColor     = new Color(.8f, .8f, .8f, 1f);
        private static readonly Color labelPrefabColor      = new Color(.48f, .67f, .94f, 1f);

        private static bool isHierarchyFocused      = false;

        private static TreeViewState treeView       = null;
        private static object treeViewController    = null;
        private static List<int> dragSelection      = null;

        private static bool isSearchFilter = false;
        private static bool enabled = true;

        // -------------------------------------------
        // GUI
        // -------------------------------------------

        private static void OnHierarchyItemGUI(int _id, Rect _position) {
            // Activation.
            EnhancedHierarchyEnhancedSettings _settings = EnhancedHierarchyEnhancedSettings.Settings;
            if (!_settings.Enabled || !enabled) {
                return;
            }

            if (treeView == null) {
                RefreshTreeView();
            }

            // Ignore null items.
            if ((_id == 0) || (treeView == null)) {
                return;
            }

            try {
                // Registration.
                if (!objects.TryGetValue(_id, out HierarchyObject _object)) {
                    _object = new HierarchyObject(_id);
                    objects.Add(_id, _object);
                }

                // Scene type icon.
                if (_object.IsSceneHeader) {
                    DrawSceneHeader(_position, _object);
                } else {
                    DrawObject(_position, _object);
                }

                // Indent.
                DrawItemIndent(_position, _object);
            } catch (Exception) {
                objects.Remove(_id);
            }
        }

        private static void DrawSceneHeader(Rect _position, HierarchyObject _object) {
            if (_object.GetScene(out Scene _scene) && _scene.IsValid() && _scene.IsCoreScene()) {
                // Setup.
                _position.x -= 2f;
                _position.y -= 1f;
                _position.width = 21f;
                _position.height += 1f;

                EditorGUI.LabelField(_position, EnhancedHierarchyEnhancedSettings.Settings.CoreSceneIcon);
            }
        }

        private static void DrawObject(Rect _position, HierarchyObject _object) {
            // Positions.
            Rect _full = new Rect(0f, _position.y, _position.xMax + OpenPrefabWidth, _position.height);
            Rect _temp = new Rect(_position);
            Rect _open = new Rect(_position) {
                x = _position.xMax,
                width = OpenPrefabWidth,
            };

            Rect _background = new Rect(_full) {
                xMin = LeftMargin,
            };

            // Leave some space for the open prefab button.
            if (_object.IsPrefabRoot && _open.Contains(Event.current.mousePosition)) {
                _background.xMax = _position.xMax;
            }

            // Line background.
            bool _selected = _object.IsSelected;
            bool _isOdd = ((int)(_position.y / _position.height) % 2) == 0;

            Color _backgroundColor = _isOdd ? EnhancedEditorGUIUtility.GUIPeerLineColor : EnhancedEditorGUIUtility.GUIThemeBackgroundColor;
            EditorGUI.DrawRect(_background, _backgroundColor);

            // Feedback background.
            if (isHierarchyFocused && _selected) {
                _backgroundColor = EnhancedEditorGUIUtility.GUISelectedColor;
                EditorGUI.DrawRect(_background, _backgroundColor);
            } else if ((DragAndDrop.visualMode == DragAndDropVisualMode.Move) && _full.Contains(Event.current.mousePosition)) {
                EditorGUI.DrawRect(_background, dragPreviewColor);
            } else if (_selected) {
                EditorGUI.DrawRect(_background, selectionColor);
            } else if (_full.Contains(Event.current.mousePosition)) {
                EditorGUI.DrawRect(_background, hoverColor);
            }

            // Foldout.
            if (_object.HasChildren) {
                _temp.x -= 14f;
                _temp.width = 15f;

                EditorGUI.Foldout(_temp, _object.IsExpanded, GUIContent.none);
            }

            // Enable color.
            Color _guiColor = GUI.color;

            if (!_object.IsActiveInHierarchy(_position)) {
                _guiColor.a = .5f;
            }

            // Content.
            using (var _colorScope = EnhancedGUI.GUIColor.Scope(_guiColor)) {

                if (_object.IsHierarchyHeader) {
                    DrawHierarchyHeader(_position, _object);
                } else {
                    DrawHierarchyObject(_position, _object);
                }
            }
            
            // Open prefab button.
            if (_object.IsPrefabRoot && !_open.Contains(Event.current.mousePosition)) {

                _temp = new Rect(_open) {
                    x = _position.xMax - 1f,
                    y = _position.y,
                    width = 18f,
                };

                EditorGUI.LabelField(_temp, Styles.OpenPrefabGUI);
            }
        }

        private static void DrawHierarchyHeader(Rect _position, HierarchyObject _object) {
            // Header.
            float _x = _position.x - RootPositionX;
            int _indent = (_x != 0f)
                        ? (int)(_x / IndentWidth)
                        : 0;

            _position.xMin += GradientMargins.x;
            _position.xMax += GradientMargins.y;

            HierarchyStyle _style = GetHierarchyStyle(_object);
            Gradient _gradient = _style.HeaderBackground;

            // Background gradient.
            if (_style.OverrideHeaderBackground || EnhancedHierarchyEnhancedSettings.Settings.GetHeaderGradient(_indent, out _gradient)) {

                EnhancedEditorGUI.DrawGradient(_position, _gradient);

                // Outline.
                if (EnhancedHierarchyEnhancedSettings.Settings.HeaderOutline) {
                    Rect _temp = new Rect(_position) {
                        xMin = _position.x - 20f,
                        height = 1f,
                    };

                    EnhancedEditorGUI.DrawGradient(_temp, headerOutlineGrad);

                    _temp.y = _position.yMax - 1f;
                    EnhancedEditorGUI.DrawGradient(_temp, headerOutlineGrad);
                }
            }

            // Label.
            GUIStyle _labelStyle = Styles.Header;
            Color _color = _style.UseHeaderColor
                         ? _style.HeaderColor
                         : _object.LabelColor;

            using (var _scope = EnhancedGUI.GUIContentColor.Scope(_color))
            using (var _alignmentScope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter)) {
                    EditorGUI.DropShadowLabel(_position, EnhancedEditorGUIUtility.GetLabelGUI(_object.Name), _labelStyle);
            }
        }

        private static void DrawHierarchyObject(Rect _position, HierarchyObject _object) {
            Rect _temp = new Rect(_position){
                x = _position.x - 1f,
                width = IconWidth
            };

            HierarchyStyle _style = GetHierarchyStyle(_object);

            // Background gradient.
            if (_style.UseBackground) {
                EnhancedEditorGUI.DrawGradient(_position, _style.Background);
            }

            Texture _icon = _style.Icon;
            if (_icon == null) {
                _icon = _object.Icon;
            }

            Color _color = _style.UseIconColor
                         ? _style.IconColor
                         : GUI.contentColor;

            // Icon.
            using (var _scope = EnhancedGUI.GUIContentColor.Scope(_color)) {
                GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_icon, string.Empty);
                EditorGUI.LabelField(_temp, _label);
            }

            _temp = new Rect() {
                x = _temp.xMax + 1f,
                xMax = _position.xMax,

                y = _temp.y - 2f,
                height = 20f,
            };

            _color = _style.UseLabelColor
                   ? _style.LabelColor
                   : _object.LabelColor;

            // Label.
            using (var _scope = EnhancedGUI.GUIContentColor.Scope(_color)) {
                EditorGUI.LabelField(_temp, EnhancedEditorGUIUtility.GetLabelGUI(_object.Name), Styles.Label);
            }
        }

        private static void DrawItemIndent(Rect _position, HierarchyObject _object) {
            // Indent position.
            Rect _indentPosition = new Rect(){
                x = _position.x - 20f,
                y = _position.y + (_position.height / 2f),
                width = 19f,
                height = 1f
            };

            // Item foldout shift.
            if (_object.HasChildren) {
                _indentPosition.width -= 11f;
            }

            // Indent dotted lines.
            if (Event.current.type == EventType.Repaint) {
                // Ignore scene headers.
                if (!_object.IsSceneHeader && !_object.IsRootObject(_position)) {
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

        // -------------------------------------------
        // Button
        // -------------------------------------------

        private static void OnUpdate() {
            // Window focus.
            EditorWindow _focus =  EditorWindow.focusedWindow;
            if (_focus != null) {
                isHierarchyFocused = _focus.titleContent.text == HierarchyWindowTitle;
            }

            isSearchFilter = IsSearchFilter();

            // Update the drag selection on update,
            // as the referenced list may be created again.
            dragSelection = GetDragSelection();
        }

        private static void OnHierarchyChanged() {
            Reset();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            Reset();

            // The call of a HierarchyProperty when exiting play mode may cause the Editor to crash,
            // so temporarily disable the enhanced hierarchy during the operation.
            enabled = _state != PlayModeStateChange.ExitingPlayMode;
        }
        #endregion

        #region Utility
        private const BindingFlags Flags                            = BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type hierarchyType                  = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchy");
        private static readonly Type hierarchyWindowType            = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        private static readonly Type treeViewControllerType         = typeof(EditorWindow).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

        private static readonly FieldInfo treeViewField             = hierarchyType.GetField("m_TreeViewState", BindingFlags.Instance | Flags);
        private static readonly FieldInfo treeViewControllerField   = hierarchyType.GetField("m_TreeView", BindingFlags.Instance | Flags);
        private static readonly FieldInfo searchFilterField         = hierarchyType.GetField("m_SearchFilter", BindingFlags.Instance | Flags);

        private static readonly FieldInfo lastHierarchyField        = hierarchyWindowType.GetField("s_LastInteractedHierarchy", BindingFlags.Static | Flags);
        private static readonly FieldInfo hierarchyField            = hierarchyWindowType.GetField("m_SceneHierarchy", BindingFlags.Instance | Flags);

        private static readonly FieldInfo dragSelectionField        = treeViewControllerType.GetField("m_DragSelection", BindingFlags.Instance | Flags);

        #if UNITY_2021
        private static readonly Type integerCacheType               = treeViewControllerType.GetNestedType("IntegerCache", Flags);
        private static readonly FieldInfo dragListField             = integerCacheType.GetField("m_List", BindingFlags.Instance | Flags);
        #endif

        private static readonly HierarchyStyle tempStyle = new HierarchyStyle();

        // -----------------------

        private static void RefreshTreeView() {
            try {
                if (GetHierarchy(out var _hierarchy)) {

                    treeView = treeViewField.GetValue(_hierarchy) as TreeViewState;
                    treeViewController = treeViewControllerField.GetValue(_hierarchy);
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private static List<int> GetDragSelection() {
            if (treeViewController == null) {
                return null;
            }

            var _dragSelection = dragSelectionField.GetValue(treeViewController);

            #if UNITY_2021
            return dragListField.GetValue(_dragSelection) as List<int>;
            #else
            return _dragSelection as List<int>;
            #endif
        }

        private static HierarchyStyle GetHierarchyStyle(HierarchyObject _object) {
            tempStyle.Reset();

            if (_object.GetGameObject(out GameObject _gameObject) && EnhancedHierarchyEnhancedSettings.Settings.GetLayerStyle(_gameObject.layer, out HierarchyStyle _style)) {
                tempStyle.Copy(_style);
            }

            if (_object.GetBehaviour(out ExtendedBehaviour _behaviour) && _behaviour.overrideHierarchyStyle) {
                tempStyle.Copy(_behaviour.hierarchyStyle);
            }

            return tempStyle;
        }

        private static bool IsSelected(int _id) {
            return ((dragSelection != null) && (dragSelection.Count != 0))
                 ? dragSelection.Contains(_id)
                 : treeView.selectedIDs.Contains(_id);
        }

        private static bool IsSearchFilter() {
            if (!GetHierarchy(out object _hierarchy)) {
                return false;
            } 
            
            object _searchFilter = searchFilterField.GetValue(_hierarchy);
            if (_searchFilter == null) {
                return false;
            }

            return !string.IsNullOrEmpty(_searchFilter.ToString());
        }

        private static bool GetHierarchy(out object _hierarchy) {
            try {
                var _hierarchyWindow = lastHierarchyField.GetValue(null);

                if (_hierarchyWindow != null) {
                    _hierarchy = hierarchyField.GetValue(_hierarchyWindow);
                    return _hierarchy != null;
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }

            _hierarchy = null;
            return false;
        }

        internal static void Reset() {
            objects.Clear();
            treeView = null;
        }
        #endregion
    }
}
