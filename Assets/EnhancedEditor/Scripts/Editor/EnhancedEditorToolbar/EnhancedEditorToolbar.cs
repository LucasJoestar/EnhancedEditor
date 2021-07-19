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
using UnityEngine.UIElements;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Extends the editor toolbar to add custom controls in it.
    /// <para/>
    /// You can create your own custom controls with the <see cref="EditorToolbarLeftExtension"/> and <see cref="EditorToolbarRightExtension"/> attributes.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedEditorToolbar
    {
        #region Global Members
        private static readonly BindingFlags reflectionFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly Type guiViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
        private static readonly Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly Type iWindowBackendType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");

        private static readonly PropertyInfo windowBackendInfo = guiViewType.GetProperty("windowBackend", reflectionFlags);
        private static readonly PropertyInfo visualTreeInfo = iWindowBackendType.GetProperty("visualTree", reflectionFlags);
        private static readonly FieldInfo onGUIHandlerInfo = typeof(IMGUIContainer).GetField("m_OnGUIHandler", reflectionFlags);

        private static readonly FieldInfo toolCountInfo = toolbarType.GetField("k_ToolCount", reflectionFlags);
        private static readonly int toolCount = 7;

        private static readonly Action[] toolbarLeftExtensions = new Action[] { };
        private static readonly Action[] toolbarRightExtensions = new Action[] { };

        private static VisualElement visualTree = null;
        private static Action repaintDelegate = null;

        private static ScriptableObject toolbar = null;

        // -----------------------

        static EnhancedEditorToolbar()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            if (toolCountInfo != null)
            {
                toolCount = (int)toolCountInfo.GetValue(null);
            }

            // Get all toolbar extensions.
            toolbarLeftExtensions = GetExtensions<EditorToolbarLeftExtension>(false);
            toolbarRightExtensions = GetExtensions<EditorToolbarRightExtension>(true);

            // ----- Local Method ----- //
            Action[] GetExtensions<T>(bool _sortAscending) where T : EditorToolbarExtension
            {
                // Get matching methods.
                var _attributes = TypeCache.GetMethodsWithAttribute<T>();
                List<MethodInfo> _extensions = new List<MethodInfo>();

                for (int _i = 0; _i < _attributes.Count; _i++)
                {
                    MethodInfo _method = _attributes[_i];
                    if (_method.IsStatic && (_method.GetParameters().Length == 0))
                        _extensions.Add(_method);
                }

                // Sort extensions by their order.
                _extensions.Sort((a, b) =>
                {
                    var _aAttribute = a.GetCustomAttribute<EditorToolbarLeftExtension>();
                    var _bAttribute = b.GetCustomAttribute<EditorToolbarLeftExtension>();

                    return _sortAscending
                            ? _aAttribute.Order.CompareTo(_bAttribute.Order)
                            : _bAttribute.Order.CompareTo(_aAttribute.Order);
                });

                // Then store their delegate.
                return Array.ConvertAll(_extensions.ToArray(), a =>
                {
                    return a.CreateDelegate(typeof(Action)) as Action;
                });
            }
        }
        #endregion

        #region Update
        private static void Update()
        {
            // Subscribe toolbar GUI callback everytime its refrence is lost. 
            if (toolbar == null)
            {
                var _toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                if (_toolbars.Length == 0)
                    return;

                // Load the main toolbar and get its gui handler callback.
                // Could use the onGUIHandler delegate from its IWindowModel interface if it wasn't readonly.
                toolbar = _toolbars[0] as ScriptableObject;

                object _windowBackend = windowBackendInfo.GetValue(toolbar);
                visualTree = visualTreeInfo.GetValue(_windowBackend) as VisualElement;
                IMGUIContainer _container = visualTree[0] as IMGUIContainer;
                Action _handler = onGUIHandlerInfo.GetValue(_container) as Action;

                _handler -= OnGUI;
                _handler += OnGUI;

                // As the delegate isn't the direct reference, we now need to set it back.
                onGUIHandlerInfo.SetValue(_container, _handler);

                // Get toolbar repaint delegate.
                repaintDelegate = typeof(VisualElement).GetMethod("MarkDirtyRepaint").CreateDelegate(typeof(Action), visualTree) as Action;
            }
        }
        #endregion

        #region GUI
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
            // Calculate all rects.
            float _playModePos = Mathf.RoundToInt((Screen.width - PlayPauseStopWidth) * .5f);

            Rect _leftPosition = new Rect()
            {
                x = Space + (StandardButtonWidth * toolCount) + Space + PivotButtonsWidth + Space + StandardButtonWidth + 3f,
                xMax = _playModePos - Space,
                y = VerticalPadding,
                height = ControlHeight,
            };

            Rect _rightPosition = new Rect()
            {
                x = _playModePos + (StandardButtonWidth * 3f) + Space,
                xMax = Screen.width - (((DropdownWidth + Space) * 3f) + ((StandardButtonWidth + Space) * 2f) + StandardButtonWidth),
                y = VerticalPadding,
                height = ControlHeight
            };

            // Left side extensions.
            if (_leftPosition.width > 0f)
            {
                GUILayout.BeginArea(_leftPosition);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                foreach (var _extension in toolbarLeftExtensions)
                {
                    GUILayout.Space(5f);
                    _extension();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            // Right side extensions.
            if (_rightPosition.width > 0f)
            {
                GUILayout.BeginArea(_rightPosition);
                GUILayout.BeginHorizontal();

                foreach (var _extension in toolbarRightExtensions)
                {
                    _extension();
                    GUILayout.Space(5f);
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        // -----------------------

        //[ToolbarLeftExtension]
        public static void LeftExtension()
        {
            Button(new GUIContent("Left Space"), GUILayout.ExpandWidth(true));
        }

        //[ToolbarRightExtension]
        public static void Rightxtension()
        {
            Button(new GUIContent("Right Space"), GUILayout.ExpandWidth(true));
        }
        #endregion

        #region GUI Utility
        /// <summary>
        /// Draws a toolbar like button. Use this to draw buttons for the editor toolbar in your own extensions.
        /// </summary>
        /// <param name="_content">Content to display.</param>
        /// <param name="_options">An optional list of layout options that specify extra layout properties.</param>
        /// <returns>True when the users clicks the button, false otherwise.</returns>
        public static bool Button(GUIContent _content, params GUILayoutOption[] _options)
        {
            // First, draw the button background as the appropriate style cannot display any label.
            Rect _position = EditorGUILayout.GetControlRect(_options);
            bool _result = GUI.Button(_position, GUIContent.none, EnhancedEditorStyles.ToolbarControl);

            // Then, draw toolbar label above.
            GUIStyle _labelStyle = EnhancedEditorStyles.ToolbarLabel;
            EnhancedEditorGUIUtility.PushAnchor(_labelStyle, TextAnchor.MiddleCenter);

            // Somehow label is not automatically aligned with the toolbar, so increment vertical position.
            _position.y += 2f;
            GUI.Label(_position, _content, _labelStyle);

            EnhancedEditorGUIUtility.PopAnchor(_labelStyle);

            return _result;
        }

        /// <summary>
        /// Draws a group of buttons together, like the ones in the editor toolbar.
        /// </summary>
        /// <param name="_contents">Contents to display on buttons.</param>
        /// <param name="_options">An optional list of layout options that specify extra layout properties.</param>
        /// <returns>Index of the button the users clicked on if any, -1 otherwise.</returns>
        public static int ButtonGroup(GUIContent[] _contents, params GUILayoutOption[] _options)
        {
            int _result = -1;

            Rect _position = EditorGUILayout.GetControlRect(_options);
            _position.width = Mathf.Round(_position.width / _contents.Length);

            GUIStyle _labelStyle = EnhancedEditorStyles.ToolbarLabel;
            EnhancedEditorGUIUtility.PushAnchor(_labelStyle, TextAnchor.MiddleCenter);

            for (int _i = 0; _i < _contents.Length; _i++)
            {
                // Select appropriate background style depending on position.
                GUIContent _content = _contents[_i];
                GUIStyle _style = (_i == 0) ? EnhancedEditorStyles.ToolbarCommandLeft
                                            : ((_i == (_contents.Length - 1)) ? EnhancedEditorStyles.ToolbarCommandRight
                                                                              : EnhancedEditorStyles.ToolbarCommandMid);

                if (GUI.Button(_position, GUIContent.none, _style))
                    _result = _i;

                // Draw label over background.
                _position.y += 2f;
                GUI.Label(_position, _content, _labelStyle);

                // Increment position for next button.
                _position.x += _position.width;
                _position.y -= 2f;
            }

            EnhancedEditorGUIUtility.PopAnchor(_labelStyle);

            return _result;
        }

        /// <summary>
        /// Repaints the main editor toolbar.
        /// </summary>
        public static void Repaint()
        {
            repaintDelegate();
        }
        #endregion
    }
}
