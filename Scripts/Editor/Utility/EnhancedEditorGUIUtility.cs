// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple editor-related GUI utility methods, variables and properties.
    /// </summary>
    public static class EnhancedEditorGUIUtility
    {
        #region Global Editor Variables
        /// <summary>
        /// Default size (for both width and height) used to draw an asset preview (in pixels).
        /// </summary>
        public const float AssetPreviewDefaultSize = 64f;

        /// <summary>
        /// Width used to draw foldout buttons (in pixels).
        /// </summary>
        public const float FoldoutWidth = 15f;

        /// <summary>
        /// Width used to draw various icons (in pixels).
        /// </summary>
        public const float IconWidth = 20f;

        /// <summary>
        /// Default width of the lines surrounding the label of a section (in pixels).
        /// </summary>
        public const float SectionDefaultLineWidth = 50f;

        /// <summary>
        /// Space on both sides of a section between its label and the horizontal lines (in pixels).
        /// </summary>
        public const float SectionLabelMargins = 5f;

        /// <summary>
        /// Size of scroll bars drawn in GUI.
        /// </summary>
        public const float ScrollSize = 13f;

        /// <summary>
        /// Size (both width and height) of the icons drawn using the styles
        /// <see cref="EnhancedEditorStyles.OlMinus"/> and <see cref="EnhancedEditorStyles.OlPlus"/>.
        /// </summary>
        public const float OlStyleSize = 16f;
        #endregion

        #region Color
        /// <summary>
        /// Color used to draw peer background lines.
        /// </summary>
        public static readonly EditorColor GUIPeerLineColor = new EditorColor(new Color(.75f, .75f, .75f),
                                                                              new Color(.195f, .195f, .195f));

        /// <summary>
        /// Color used for various selected GUI controls.
        /// </summary>
        public static readonly EditorColor GUISelectedColor = new EditorColor(new Color(0f, .5f, 1f, .28f),
                                                                              new Color(0f, .5f, 1f, .25f));

        /// <summary>
        /// Color used for link labels, when the mouse is not hover.
        /// </summary>
        public static readonly EditorColor LinkLabelNormalColor = new EditorColor(new Color(0f, .235f, .533f, 1f),
                                                                                  new Color(.506f, .706f, 1f, 1f));
        /// <summary>
        /// Color used for link labels, when the mouse is hover.
        /// </summary>
        public static readonly EditorColor LinkLabelActiveColor = new EditorColor(new Color(.12f, .53f, 1f, 1f),
                                                                                  new Color(.9f, .9f, .9f, 1f));

        /// <summary>
        /// Editor GUI background color used in dark theme.
        /// </summary>
        public static readonly Color DarkThemeGUIBackgroundColor = new Color32(56, 56, 56, 255);

        /// <summary>
        /// Editor GUI background color used in light theme.
        /// </summary>
        public static readonly Color LightThemeGUIBackgroundColor = new Color32(194, 194, 194, 255);

        /// <summary>
        /// Current editor GUI background color, depending on whether currently using the light theme or the dark theme.
        /// </summary>
        public static Color GUIThemeBackgroundColor
        {
            get
            {
                Color _color = EditorGUIUtility.isProSkin
                                ? DarkThemeGUIBackgroundColor
                                : LightThemeGUIBackgroundColor;

                return _color;
            }
        }
        #endregion

        #region GUI Content
        private static readonly GUIContent propertylabelGUI = new GUIContent(GUIContent.none);
        private static readonly GUIContent labelGUI = new GUIContent(GUIContent.none);
        private static readonly GUIContent helpBoxGUI = new GUIContent();

        // -----------------------

        /// <summary>
        /// Get the <see cref="GUIContent"/> label associated with a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get label from.</param>
        /// <returns>Label associated with the property.</returns>
        public static GUIContent GetPropertyLabel(SerializedProperty _property)
        {
            string _name = ObjectNames.NicifyVariableName(_property.name);

            propertylabelGUI.text = _name;
            propertylabelGUI.tooltip = _property.tooltip;

            return propertylabelGUI;
        }

        /// <inheritdoc cref="GetLabelGUI(string, string)"/>
        public static GUIContent GetLabelGUI(string _label)
        {
            return GetLabelGUI(_label, string.Empty);
        }

        /// <summary>
        /// Get a cached <see cref="GUIContent"/> for a specific label.
        /// </summary>
        /// <param name="_label"><see cref="GUIContent"/> text label.</param>
        /// <param name="_tooltip"><see cref="GUIContent"/> tooltip.</param>
        /// <returns><see cref="GUIContent"/> to use.</returns>
        public static GUIContent GetLabelGUI(string _label, string _tooltip)
        {
            labelGUI.text = _label;
            labelGUI.tooltip = _tooltip;
            labelGUI.image = null;

            return labelGUI;
        }

        /// <summary>
        /// Get the height to use to draw a help box with a specific message, type and width.
        /// </summary>
        /// <param name="_message">The message text.</param>
        /// <param name="_messageType">The type of message.</param>
        /// <param name="_width">The total width to use to draw the help box.</param>
        /// <returns>Total height that will be used to draw this help box.</returns>
        public static float GetHelpBoxHeight(string _message, UnityEditor.MessageType _messageType, float _width)
        {
            GUIContent _label;
            if (_messageType != UnityEditor.MessageType.None)
            {
                _label = helpBoxGUI;
                _label.text = _message;

                // Load an icon that will help calculate the help box height.
                if (_label.image == null)
                    _label.image = EditorGUIUtility.FindTexture("console.infoicon");
            }
            else
            {
                _label = GetLabelGUI(_message);
            }

            float _height = EditorStyles.helpBox.CalcHeight(_label, _width);
            return _height;
        }
        #endregion

        #region Rect and Position
        /// <summary>
        /// The current screen width, scaled by the device dpi.
        /// </summary>
        public static float ScreenWidth
        {
            get
            {
                float _width = Mathf.RoundToInt(Screen.width / ScreenScale);
                return _width;
            }
        }

        /// <summary>
        /// The current screen height, scaled by the device dpi.
        /// </summary>
        public static float ScreenHeight
        {
            get
            {
                float _width = Mathf.RoundToInt(Screen.height / ScreenScale);
                return _width;
            }
        }

        /// <summary>
        /// The actual editor scale depending on the screen device dpi.
        /// </summary>
        public static float ScreenScale
        {
            get
            {
                float _scale = Screen.dpi / 96f;
                return _scale;
            }
        }

        /// <summary>
        /// Get this view default rect for an Editor control.
        /// <br/>
        /// Note that its y position is always equal to 0.
        /// </summary>
        /// <returns>The default rect to use for an Editor control on this view.</returns>
        public static Rect GetViewControlRect()
        {
            // Left offset is equal to 18 pixels, and right offset to 5.
            Rect _position = new Rect()
            {
                x = 18f,
                y = 0f,
                width = EditorGUIUtility.currentViewWidth - 18f - 5f,
                height = EditorGUIUtility.singleLineHeight
            };

            return _position;
        }

        /// <summary>
        /// Moves a scroll to focus on a specific position in view.
        /// </summary>
        /// <param name="_scroll">The current position of the scroll.</param>
        /// <param name="_position">Position on screen to focus.</param>
        /// <param name="_scrollAreaSize">The size of the scroll area on screen.</param>
        /// <returns>Focused scroll value on position.</returns>
        public static Vector2 FocusScrollOnPosition(Vector2 _scroll, Rect _position, Vector2 _scrollAreaSize)
        {
            // Horizontal focus.
            if (_scroll.x > _position.x)
            {
                _scroll.x = _position.x;
            }
            else if ((_scroll.x + _scrollAreaSize.x) < _position.xMax)
            {
                _scroll.x = _position.xMax - _scrollAreaSize.x;
            }

            // Vertical focus.
            if (_scroll.y > _position.y)
            {
                _scroll.y = _position.y;
            }
            else if ((_scroll.y + _scrollAreaSize.y) < _position.yMax)
            {
                _scroll.y = _position.yMax - _scrollAreaSize.y;
            }

            return _scroll;
        }
        #endregion

        #region Control ID
        private static readonly List<int> controlsID = new List<int>();

        private static int controldIDIndex = -1;
        private static int activeIDCount = 0;

        // -----------------------

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(FocusType _focusType)
        {
            GUIContent _label = GUIContent.none;
            return GetControlID(_label, _focusType);
        }

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(FocusType _focusType, Rect _position)
        {
            GUIContent _label = GUIContent.none;
            return GetControlID(_label, _focusType, _position); ;
        }

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(GUIContent _label, FocusType _focusType)
        {
            Rect _position = Rect.zero;
            return GetControlID(_label, _focusType, _position);
        }

        /// <summary>
        /// Works pretty much like <see cref="GUIUtility.GetControlID(GUIContent, FocusType, Rect)"/>,
        /// except that the ID is guaranteed to never equal -1.
        /// <para/>
        /// <inheritdoc cref="GUIUtility.GetControlID(GUIContent, FocusType, Rect)" path="/summary"/>
        /// </summary>
        /// <param name="_label">Label acting as a hint to ensure correct matching of IDs to controls.</param>
        /// <param name="_focusType">Control <see cref="FocusType"/>.</param>
        /// <param name="_position">Rectangle on the screen where the control is drawn.</param>
        /// <returns>Guaranteed not equal to -1 control ID.</returns>
        public static int GetControlID(GUIContent _label, FocusType _focusType, Rect _position)
        {
            int _id = GUIUtility.GetControlID(_label, _focusType, _position);
            return DoControlID(_id);
        }

        /// <inheritdoc cref="GetControlID(int, FocusType, Rect)"/>
        public static int GetControlID(int _hint, FocusType _focusType)
        {
            Rect _position = Rect.zero;
            return GetControlID(_hint, _focusType, _position);
        }

        /// <summary>
        /// Works pretty much like <see cref="GUIUtility.GetControlID(int, FocusType, Rect)"/>,
        /// except that the ID is guaranteed to never equal -1.
        /// <para/>
        /// <inheritdoc cref="GUIUtility.GetControlID(int, FocusType, Rect)" path="/summary"/>
        /// </summary>
        /// <param name="_hint">Hint to help ensure correct matching of IDs to controls.</param>
        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(int _hint, FocusType _focusType, Rect _position)
        {
            int _id = GUIUtility.GetControlID(_hint, _focusType, _position);
            return DoControlID(_id);
        }

        // -----------------------

        private static int DoControlID(int _id)
        {
            /* Sometimes, with some special events (like EventType.Used), the returned id may be equal to -1.
             * However, this unwanted behaviour breaks many enhanced controls (like dynamic height ones),
             * as all following ids are then also equal to -1, and their associated registered value cannot be properly retrieved.
             * 
             * So let's store all previous event ids, and replace undesired values with them.
             * (You can reactivate the following debugs if you want to understand the default behaviour).
            */

            //Debug.Log("ID => " + _id + " | " + Event.current.type);

            if (_id == -1)
            {
                // Unregistered id.
                if (controlsID.Count == 0)
                    return -1;

                // Invalid.
                controldIDIndex = (controldIDIndex < activeIDCount)
                                ? (controldIDIndex + 1)
                                : 0;

                _id = controlsID[controldIDIndex];
            }
            else if ((controldIDIndex != -1) && (_id < controlsID[controldIDIndex]))
            {
                // Restart.
                activeIDCount = controldIDIndex;

                controldIDIndex = 0;
                controlsID[controldIDIndex] = _id;
            }
            else
            {
                // Increment.
                controldIDIndex++;
                if (controldIDIndex == controlsID.Count)
                {
                    controlsID.Add(_id);
                }
                else
                {
                    controlsID[controldIDIndex] = _id;
                }
            }

            //Debug.LogWarning("ID => " + _id + " | " + Event.current.type);

            return _id;
        }
        #endregion

        #region Event and Clicks
        private static Vector2 mouseDownPosition = new Vector2();

        // -----------------------

        /// <summary>
        /// Did the user just performed a context click on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect)" path="/param[@name='_position']"/></param>
        /// <returns>True if the user performed a context click here, false otherwise.</returns>
        public static bool ContextClick(Rect _position)
        {
            if (_position.Event(out Event _event) == EventType.ContextClick)
            {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just pressed a mouse button on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect)" path="/param[@name='_position']"/></param>
        /// <returns>True if the user clicked here, false otherwise.</returns>
        public static bool MouseDown(Rect _position)
        {
            if (_position.Event(out Event _event) == EventType.MouseDown)
            {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just released the main mouse button on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect)" path="/param[@name='_position']"/></param>
        /// <returns>True if the user released this mouse button here, false otherwise.</returns>
        public static bool MainMouseUp(Rect _position)
        {
            if ((_position.Event(out Event _event) == EventType.MouseUp) && (_event.button == 0))
            {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just performed a click to deselect element(s) on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect)" path="/param[@name='_position']"/></param>
        /// <returns>True if the user performed a deselection click here, false otherwise.</returns>
        public static bool DeselectionClick(Rect _position)
        {
            if ((_position.Event(out Event _event) == EventType.MouseDown) && !_event.control && !_event.shift && (_event.button == 0))
            {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just pressed a vertical key on the keyboard?
        /// <br/> Works with up and down arrows.
        /// </summary>
        /// <returns>-1 for up, 1 for down, 0 otherwise.</returns>
        public static int VerticalKeys()
        {
            Event _event = Event.current;

            if (_event.type == EventType.KeyDown)
            {
                switch (_event.keyCode)
                {
                    case KeyCode.UpArrow:
                        _event.Use();
                        return -1;

                    case KeyCode.DownArrow:
                        _event.Use();
                        return 1;

                    default:
                        break;
                }
            }

            return 0;
        }

        /// <summary>
        /// Did the user just pressed a horizontal key on the keyboard?
        /// <br/> Works with left and right arrows.
        /// </summary>
        /// <returns>-1 for left, 1 for right, 0 otherwise.</returns>
        public static int HorizontalSelectionKeys()
        {
            Event _event = Event.current;

            if (_event.type == EventType.KeyDown)
            {
                switch (_event.keyCode)
                {
                    case KeyCode.LeftArrow:
                        _event.Use();
                        return -1;

                    case KeyCode.RightArrow:
                        _event.Use();
                        return 1;

                    default:
                        break;
                }
            }

            return 0;
        }

        /// <summary>
        /// Selects multiple elements of an array according to the user's mouse inputs.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <param name="_array"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_array']"/></param>
        /// <param name="_index">Index of the current array element.</param>
        /// <param name="_isSelected"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_isSelected']"/></param>
        /// <param name="_onSelect"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_onSelect']"/></param>
        /// <returns>True if the user performed a click to select element(s) here, false otherwise.</returns>
        public static bool MultiSelectionClick(Rect _position, Array _array, int _index, Predicate<int> _isSelected, Action<int, bool> _onSelect)
        {
            // Only select on mouse down or on selected element mouse up.
            bool _isMouseUpSelection = false;
            if (_position.Event(out Event _event) != EventType.MouseDown)
            {
                if ((_position.Event() != EventType.MouseUp) || (_event.button != 0) || (_event.mousePosition != mouseDownPosition))
                {
                    if (_event.type == EventType.MouseDrag)
                        mouseDownPosition = Vector2.zero;

                    return false;
                }

                _isMouseUpSelection = true;
            }

            // Mouse up helper update.
            if (!_isMouseUpSelection && _isSelected(_index))
            {
                mouseDownPosition = _event.mousePosition;
                return true;
            }
            else
                mouseDownPosition = Vector2.zero;

            if (_event.shift)
            {
                int _firstIndex = -1;
                int _lastIndex = -1;

                // Find first index.
                for (int _i = 0; _i < _array.Length; _i++)
                {
                    if (_isSelected(_i) || (_i == _index))
                    {
                        _firstIndex = _i;
                        break;
                    }
                }

                // Find last index.
                for (int _i = _array.Length; _i-- > 0;)
                {
                    if (_isSelected(_i) || (_i == _index))
                    {
                        _lastIndex = _i + 1;
                        break;
                    }
                }

                // Select all elements between indexes.
                if (_index == _firstIndex)
                {
                    for (int _i = _lastIndex; _i-- > _firstIndex;)
                    {
                        _onSelect(_i, true);
                    }
                }
                else
                {
                    for (int _i = _firstIndex; _i < _lastIndex; _i++)
                    {
                        _onSelect(_i, true);
                    }
                }
            }
            else if (_event.control)
            {
                // Inverse selected state.
                bool _selected = _isSelected(_index);
                _onSelect(_index, !_selected);
            }
            else if (!_isSelected(_index) || (_event.button == 0))
            {
                // Unselect every element except this one.
                for (int _i = 0; _i < _array.Length; _i++)
                {
                    bool _selected = _i == _index;
                    _onSelect(_i, _selected);
                }
            }

            _event.Use();
            return true;
        }

        /// <summary>
        /// Selects multiple elements of an array according to the user's vertical inputs.
        /// </summary>
        /// <param name="_array"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_array']"/></param>
        /// <param name="_isSelected"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_isSelected']"/></param>
        /// <param name="_canBeSelected">Used to know if a specific element can be selected.</param>
        /// <param name="_onSelect"><inheritdoc cref="DocumentationMethod(Rect, Array, Predicate{int}, Action{int, bool})" path="/param[@name='_onSelect']"/></param>
        /// <param name="_lastSelectedIndex">Index of the last selected element. Use -1 if no element is currently selected.</param>
        /// <returns>True if the user pressed a key to select element(s), from otherwise.</returns>
        public static bool VerticalMultiSelectionKeys(Array _array, Predicate<int> _isSelected, Predicate<int> _canBeSelected, Action<int, bool> _onSelect, int _lastSelectedIndex)
        {
            // Do not select anything on an empty array.
            if (_array.Length == 0)
                return false;

            if ((_lastSelectedIndex > -1) && ((_lastSelectedIndex >= _array.Length) || !_canBeSelected(_lastSelectedIndex)))
                _lastSelectedIndex = -1;

            // Get selection value.
            int _switch = VerticalKeys();
            switch (_switch)
            {
                case -1:
                {
                    // Find first selected element if none.
                    if (_lastSelectedIndex > -1)
                        break;

                    for (int _i = 0; _i < _array.Length; _i++)
                    {
                        if (_isSelected(_i))
                        {
                            _lastSelectedIndex = _i;
                            break;
                        }
                    }
                }
                break;

                case 1:
                {
                    // Find last selected element if none.
                    if (_lastSelectedIndex > -1)
                        break;

                    for (int _i = _array.Length; _i-- > 0;)
                    {
                        if (_isSelected(_i))
                        {
                            _lastSelectedIndex = _i;
                            break;
                        }
                    }
                }
                break;

                default:
                    return false;
            }

            // If no element is selected, then simply selected one.
            if (_lastSelectedIndex < 0)
            {
                int _index, _limit;
                if (_switch == 1)
                {
                    _index = _array.Length - 1;
                    _limit = -1;
                }
                else
                {
                    _index = 0;
                    _limit = _array.Length;
                }

                for (int _i = _index; _i != _limit; _i -= _switch)
                {
                    if (_canBeSelected(_i))
                    {
                        _onSelect(_index, true);
                        break;
                    }
                }

                return true;
            }

            Event _event = Event.current;
            if (_event.shift || _event.control)
            {
                // Multi-selection.
                int _firstIndex = -1;
                int _lastIndex = -1;

                // Find first index.
                for (int _i = 0; _i < _array.Length; _i++)
                {
                    if (_isSelected(_i))
                    {
                        _firstIndex = _i;
                        break;
                    }
                }

                // Find last index.
                for (int _i = _array.Length; _i-- > 0;)
                {
                    if (_isSelected(_i))
                    {
                        _lastIndex = _i + 1;
                        break;
                    }
                }

                // Select elements.
                if (_switch == -1)
                {
                    bool _increase = _lastSelectedIndex == _firstIndex;
                    if (_increase)
                    {
                        int _index = GetNewSelectedIndex();
                        for (int _i = _lastIndex; _i-- > _index;)
                            _onSelect(_i, true);
                    }
                    else
                    {
                        for (int _i = _firstIndex; _i < _lastSelectedIndex; _i++)
                            _onSelect(_i, true);

                        for (int _i = _lastSelectedIndex; _i < _lastIndex; _i++)
                            _onSelect(_i, false);
                    }
                }
                else
                {
                    bool _increase = _lastSelectedIndex == (_lastIndex - 1);
                    if (_increase)
                    {
                        int _index = GetNewSelectedIndex() + 1;
                        for (int _i = _firstIndex; _i < _index; _i++)
                            _onSelect(_i, true);
                    }
                    else
                    {
                        for (int _i = _firstIndex; _i < (_lastSelectedIndex + 1); _i++)
                            _onSelect(_i, false);

                        for (int _i = _lastIndex; _i-- > (_lastSelectedIndex + 1);)
                            _onSelect(_i, true);
                    }
                }
            }
            else
            {
                // Select this element only.
                int _index = GetNewSelectedIndex();

                for (int _i = 0; _i < _array.Length; _i++)
                {
                    bool _selected = _i == _index;
                    _onSelect(_i, _selected);
                }
            }

            return true;

            // ----- Local Method ----- \\

            int GetNewSelectedIndex()
            {
                int _index = _lastSelectedIndex;
                while (true)
                {
                    _index += _switch;
                    if ((_index == -1) || (_index == _array.Length))
                    {
                        _index = _lastSelectedIndex;
                        break;
                    }

                    if (_canBeSelected(_index))
                        break;
                }

                return _index;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Repaints all editors associated with a specific <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_object"><see cref="SerializedObject"/> to repaint associated editor(s).</param>
        public static void Repaint(SerializedObject _object)
        {
            UnityEditor.Editor[] _editors = ActiveEditorTracker.sharedTracker.activeEditors;
            foreach (UnityEditor.Editor _editor in _editors)
            {
                if (_editor.serializedObject == _object)
                {
                    _editor.Repaint();
                }
            }
        }
        #endregion

        #region Documentation
        /// <summary>
        /// This method is for documentation only, used by inheriting its parameters documentation to centralize it in one place.
        /// </summary>
        /// <param name="_position">Rectangle on the screen where to check for user actions.</param>
        /// <param name="_array">Array of all selectable elements.</param>
        /// <param name="_isSelected">Used to know if a specific element is selected.</param>
        /// <param name="_onSelect">Callback when selecting an element.</param>
        internal static void DocumentationMethod(Rect _position, Array _array, Predicate<int> _isSelected, Action<int, bool> _onSelect) { }
        #endregion
    }
}
