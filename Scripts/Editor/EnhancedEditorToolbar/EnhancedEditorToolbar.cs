// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  https://github.com/Unity-Technologies/UnityCsReference
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/EditorToolbar.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UIServiceEditor/UnityMainToolbar.cs
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/Toolbar.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUIView.cs
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/WindowBackendManager.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/External/MirroredPackageSources/com.unity.ui/Core/IMGUIContainer.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/External/MirroredPackageSources/com.unity.ui/Core/VisualElement.cs
//
// ============================================================================ //

#if UNITY_2021_1_OR_NEWER
#define SCENEVIEW_TOOLBAR
#elif UNITY_2020_1_OR_NEWER
#define EDITOR_TOOLBAR
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Extends the main editor toolbar to add custom controls in it.
    /// <para/>
    /// You can create your own custom controls using the <see cref="EditorToolbarLeftExtension"/>
    /// and <see cref="EditorToolbarRightExtension"/> attributes on your methods.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedEditorToolbar
    {
        #region Global Members
        #if SCENEVIEW_TOOLBAR
        private const string FoldoutKey = "EnhancedToolbarFoldout";

        private static bool foldout = false;
        #elif EDITOR_TOOLBAR
        private static readonly BindingFlags reflectionFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;

        private static readonly Type toolbarType = editorAssembly.GetType("UnityEditor.Toolbar");
        private static readonly Type guiViewType = editorAssembly.GetType("UnityEditor.GUIView");
        private static readonly Type iWindowBackendType = editorAssembly.GetType("UnityEditor.IWindowBackend");

        private static readonly PropertyInfo windowBackendInfo = guiViewType.GetProperty("windowBackend", reflectionFlags);
        private static readonly PropertyInfo visualTreeInfo = iWindowBackendType.GetProperty("visualTree", reflectionFlags);
        private static readonly FieldInfo onGUIHandlerInfo = typeof(IMGUIContainer).GetField("m_OnGUIHandler", reflectionFlags);
        private static readonly MethodInfo repaintInfo = typeof(VisualElement).GetMethod("MarkDirtyRepaint");

        private static readonly FieldInfo toolCountInfo = toolbarType.GetField("k_ToolCount", reflectionFlags);

        private static readonly int toolCount = 7;
        private static ScriptableObject toolbar = null;

        private static Action repaintDelegate = null;
        #endif

        private static readonly Action[] toolbarLeftExtensions = new Action[] { };
        private static readonly Action[] toolbarRightExtensions = new Action[] { };

        // -----------------------

        static EnhancedEditorToolbar()
        {
            #if UNITY_2020_1_OR_NEWER
            // Get all toolbar extensions.
            toolbarLeftExtensions = GetExtensions<EditorToolbarLeftExtension>(false);
            toolbarRightExtensions = GetExtensions<EditorToolbarRightExtension>(true);

            #if SCENEVIEW_TOOLBAR
            // From the version 2021.1 or Unity, the editor toolbar uses GUIElements instead of classic GUI controls.
            // While no fix has been found, simply draw the controls on top of the SceneView window.
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            // Load foldout value.
            SetFoldout(EditorPrefs.GetBool(FoldoutKey, foldout));
            #else
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            if (toolCountInfo != null)
            {
                toolCount = (int)toolCountInfo.GetValue(null);
            }
            #endif

            // ----- Local Method ----- \\
            
            Action[] GetExtensions<T>(bool _sortAscending) where T : EditorToolbarExtension
            {
                // Get matching extension methods.
                var _methods = TypeCache.GetMethodsWithAttribute<T>();
                List<MethodInfo> _extensions = new List<MethodInfo>();

                foreach (var _method in _methods)
                {
                    if (_method.IsStatic && (_method.GetParameters().Length == 0))
                        _extensions.Add(_method);
                }

                // Sort all extensions by their order.
                _extensions.Sort((a, b) =>
                {
                    var _aAttribute = a.GetCustomAttribute<T>();
                    var _bAttribute = b.GetCustomAttribute<T>();

                    int _comparison = _sortAscending
                                    ? _aAttribute.Order.CompareTo(_bAttribute.Order)
                                    : _bAttribute.Order.CompareTo(_aAttribute.Order);

                    return _comparison;
                });

                // Then store their delegate.
                return Array.ConvertAll(_extensions.ToArray(), a =>
                {
                    return a.CreateDelegate(typeof(Action)) as Action;
                });
            }
            #endif
        }
        #endregion

        #region Update
        private static void Update()
        {
            #if SCENEVIEW_TOOLBAR
            // Nothing to see here...
            #elif EDITOR_TOOLBAR
            // Subscribe the toolbar GUI callback every time its reference gets lost.
            // Cannot be done on initialization as some editor elements may not be initialized yet.
            if (!toolbar)
            {
                var _toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                if (_toolbars.Length == 0)
                    return;

                // Load the main toolbar and get its gui handler callback.
                // Could use the onGUIHandler delegate from its IWindowModel interface, if it wasn't readonly.
                toolbar = _toolbars[0] as ScriptableObject;

                object _windowBackend = windowBackendInfo.GetValue(toolbar);
                VisualElement _visualTree = visualTreeInfo.GetValue(_windowBackend) as VisualElement;
                IMGUIContainer _container = _visualTree[0] as IMGUIContainer;
                Action _handler = onGUIHandlerInfo.GetValue(_container) as Action;

                _handler -= OnGUI;
                _handler += OnGUI;

                // As the delegate isn't the direct reference, we now need to set it back.
                onGUIHandlerInfo.SetValue(_container, _handler);

                // Get toolbar repaint delegate.
                repaintDelegate = repaintInfo.CreateDelegate(typeof(Action), _visualTree) as Action;
            }
            #endif
        }
        #endregion

        #region Toolbar GUI
        #if EDITOR_TOOLBAR
        private const float Space = 8f;

        private const float StandardButtonWidth = 32f;
        private const float PivotButtonsWidth = 128f;
        private const float PlayPauseStopWidth = 140f;
        private const float DropdownWidth = 80f;

        private const float ControlHeight = 22f;
        private const float VerticalPadding = 4f;

        // -----------------------

        private static void OnGUI()
        {
            // Position calculs.
            float _width = EnhancedEditorGUIUtility.ScreenWidth;
            float _playModePos = Mathf.RoundToInt((_width - PlayPauseStopWidth) * .5f);

            Rect _leftPosition = new Rect()
            {
                x = Space + (StandardButtonWidth * toolCount) + Space + PivotButtonsWidth + Space + StandardButtonWidth + 3f,
                xMax = _playModePos - Space,
                y = VerticalPadding,
                height = ControlHeight
            };

            Rect _rightPosition = new Rect()
            {
                x = _playModePos + (StandardButtonWidth * 3f) + Space,
                xMax = _width - (((DropdownWidth + Space) * 3f) + ((StandardButtonWidth + Space) * 2f) + StandardButtonWidth),
                y = VerticalPadding,
                height = ControlHeight
            };

            // Left side extensions.
            if (_leftPosition.width > 0f)
            {
                using (var _scope = new GUILayout.AreaScope(_leftPosition))
                using (var _horizontalScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    foreach (var _extension in toolbarLeftExtensions)
                    {
                        GUILayout.Space(5f);
                        _extension();
                    }
                }
            }

            // Right side extensions.
            if (_rightPosition.width > 0f)
            {
                using (var _scope = new GUILayout.AreaScope(_rightPosition))
                using (var _horizontalScope = new GUILayout.HorizontalScope())
                {
                    foreach (var _extension in toolbarRightExtensions)
                    {
                        _extension();
                        GUILayout.Space(5f);
                    }
                }
            }
        }
        #endif
        #endregion

        #region Scene View GUI
        #if SCENEVIEW_TOOLBAR
        private const float ToolbarHeight = 25f;
        private const float ToolbarFoldoutWidth = 25f;
        private const float ToolbarHeaderWidth = 97f;
        private const float ToolbarRightSpace = 60f;
        private const float ToolbarFoldoutSpeed = 999f;

        private static readonly GUIContent foldGUI = new GUIContent(string.Empty, "Hides the Enhanced Editor toolbar.");
        private static readonly GUIContent unfolddGUI = new GUIContent(string.Empty, "Shows the Enhanced Editor toolbar.");
        private static readonly GUIContent headerGUI = new GUIContent("Enhanced Toolbar");

        private static float toolbarWidth = ToolbarFoldoutWidth;
        private static double lastUpdateTime = 0f;

        // -----------------------

        private static void OnSceneGUI(SceneView _view)
        {
            // Icons loading.
            if (foldGUI.image == null)
            {
                foldGUI.image = EditorGUIUtility.IconContent("Animation.FirstKey@2x").image;
                unfolddGUI.image = EditorGUIUtility.IconContent("Animation.LastKey@2x").image;
            }

            // Toolbar width update.
            float _targetWidth = foldout
                               ? ToolbarFoldoutWidth
                               : Screen.width;

            if (toolbarWidth != _targetWidth)
            {
                double _time = EditorApplication.timeSinceStartup;
                float _difference = (float)(_time - lastUpdateTime);

                lastUpdateTime = _time;
                toolbarWidth = Mathf.MoveTowards(toolbarWidth, _targetWidth, _difference * ToolbarFoldoutSpeed);

                _view.Repaint();
            }

            // Draw controls inside a 2D GUI group.
            Handles.BeginGUI();

            using (var _area = new GUILayout.AreaScope(new Rect(-1f, 0f, toolbarWidth, ToolbarHeight)))
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Foldout button.
                GUIContent _buttonGUI = foldout
                                      ? unfolddGUI
                                      : foldGUI;

                if (Button(_buttonGUI, GUILayout.Width(ToolbarFoldoutWidth - 1f)))
                {
                    SetFoldout(!foldout);
                }

                GUILayout.Label(headerGUI, EditorStyles.miniLabel, GUILayout.Width(ToolbarHeaderWidth));

                // Left side extensions.
                foreach (var _extension in toolbarLeftExtensions)
                {
                    _extension();
                }

                GUILayout.FlexibleSpace();

                // Right side extensions.
                foreach (var _extension in toolbarRightExtensions)
                {
                    _extension();
                }

                GUILayout.Space(ToolbarRightSpace);
            }

            Handles.EndGUI();
        }

        private static void SetFoldout(bool _foldout)
        {
            // Set foldout value.
            foldout = _foldout;
            EditorPrefs.SetBool(FoldoutKey, _foldout);

            lastUpdateTime = EditorApplication.timeSinceStartup;
        }
        #endif
        #endregion

        #region GUI Utility
        private static readonly GUIContent dropdownGUI = new GUIContent();

        // -----------------------

        /// <summary>
        /// Draws a toolbar-like button. Use this to draw buttons on the editor toolbar in your own extensions.
        /// </summary>
        /// <param name="_label">Text, image and tooltip to be displayed.</param>
        /// <param name="_options">An optional list of layout options that specify extra layout properties.</param>
        /// <returns>True when the users clicks the button, false otherwise.</returns>
        public static bool Button(GUIContent _label, params GUILayoutOption[] _options)
        {
            #if SCENEVIEW_TOOLBAR
            return GUILayout.Button(_label, EditorStyles.toolbarButton, _options);
            #else
            // First, draw the button background as the toolbar appropriate style cannot display any label.
            Rect _position = EditorGUILayout.GetControlRect(_options);
            bool _result = GUI.Button(_position, GUIContent.none, EnhancedEditorStyles.ToolbarControl);

            // Then, draw the label over it.
            GUIStyle _labelStyle = EnhancedEditorStyles.ToolbarLabel;
            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter))
            {
                GUI.Label(_position, _label, _labelStyle);
            }

            return _result;
            #endif
        }

        /// <summary>
        /// Draws a group of buttons together, like the ones in the editor toolbar.
        /// </summary>
        /// <param name="_labels">Texts, images and tooltips to be displayed</param>
        /// <returns>Index of the button the users clicked on if any, -1 otherwise.</returns>
        public static int ButtonGroup(GUIContent[] _labels)
        {
            int _result = -1;

            #if SCENEVIEW_TOOLBAR
            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(EditorStyles.toolbarButton, TextAnchor.MiddleCenter))
            {
                for (int _i = 0; _i < _labels.Length; _i++)
                {
                    // Select the appropriate background style depending on the button position.
                    GUIContent _label = _labels[_i];
                    if (GUILayout.Button(_label, EditorStyles.toolbarButton))
                        _result = _i;
                }
            }
            #else
            GUIStyle _labelStyle = EnhancedEditorStyles.ToolbarLabel;
            Rect _position = EditorGUILayout.GetControlRect(GUILayout.Width(0f));

            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter))
            {
                for (int _i = 0; _i < _labels.Length; _i++)
                {
                    // Select the appropriate background style depending on the button position.
                    GUIContent _content = _labels[_i];
                    GUIStyle _style = (_i == 0)
                                    ? EnhancedEditorStyles.ToolbarCommandLeft
                                    : ((_i == (_labels.Length - 1)) ? EnhancedEditorStyles.ToolbarCommandRight
                                                                    : EnhancedEditorStyles.ToolbarCommandMid);

                    _position.width = _labelStyle.CalcSize(_content).x + 7f;
                    EditorGUILayout.GetControlRect(GUILayout.Width(_position.width - 3f));

                    if (GUI.Button(_position, GUIContent.none, _style))
                        _result = _i;

                    // Draw the label over the background.
                    GUI.Label(_position, _content, _labelStyle);
                    _position.x += _position.width;
                }
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Draws a toggle next to a dropdown.
        /// </summary>
        /// <param name="toggle">Toggle button value.</param>
        /// <param name="_label">Text, image and tooltip to be displayed on the toggle button.</param>
        /// <param name="_options"><inheritdoc cref="Button(GUIContent, GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <returns>0 if the toggle value has changed, 1 if the user clicked on the dropdown, and -1 otherwise.</returns>
        public static int DropdownToggle(bool toggle, GUIContent _label, params GUILayoutOption[] _options)
        {
            int _result = -1;

            if (dropdownGUI.image == null)
                dropdownGUI.image = EditorGUIUtility.IconContent("dropdown").image;

            #if SCENEVIEW_TOOLBAR
            using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                GUILayout.Toggle(toggle, _label, EditorStyles.toolbarButton, _options);

                if (_changeCheck.changed)
                    _result = 0;
            }

            if (EditorGUILayout.DropdownButton(GUIContent.none, FocusType.Passive, EditorStyles.toolbarDropDown, GUILayout.Width(16f)))
            {
                _result = 1;
            }
            #else
            // Toggle button.
            Rect _position = EditorGUILayout.GetControlRect(_options);
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                GUI.Toggle(_position, toggle, GUIContent.none, EnhancedEditorStyles.ToolbarCommandLeft);

                if (_changeCheck.changed)
                    _result = 0;
            }

            GUIStyle _labelStyle = EnhancedEditorStyles.ToolbarLabel;
            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter))
            {
                GUI.Label(_position, _label, _labelStyle);
            }

            // Dropdown.
            _position = EditorGUILayout.GetControlRect(GUILayout.Width(12f));
            _position.xMin -= 3f;

            if (EditorGUI.DropdownButton(_position, GUIContent.none, FocusType.Passive, EnhancedEditorStyles.ToolbarCommandRight))
                _result = 1;

            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter))
            {
                GUI.Label(_position, dropdownGUI, _labelStyle);
            }
            #endif

            return _result;
        }

        /// <summary>
        /// Repaints the main editor toolbar.
        /// </summary>
        public static void Repaint()
        {
            #if EDITOR_TOOLBAR
            repaintDelegate();
            #endif
        }
        #endregion
    }
}
