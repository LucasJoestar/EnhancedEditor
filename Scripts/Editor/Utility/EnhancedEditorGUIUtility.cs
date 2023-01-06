// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// All available valide command names from <see cref="Event.commandName"/>.
    /// </summary>
    public enum ValidateCommand {
        None = 0,
        Copy,
        Cut,
        Paste,
        Delete,
        SoftDelete,
        Duplicate,
        FrameSelected,
        FrameSelectedWithLock,
        SelectAll,
        Find,
        FocusProjectWindow,

        Unknown = 31
    }

    /// <summary>
    /// Contains multiple editor-related GUI utility methods, variables and properties.
    /// </summary>
    public static class EnhancedEditorGUIUtility {
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
        /// Width used to draw various mini icons (in pixels).
        /// </summary>
        public const float MiniIconWidth = 18f;

        /// <summary>
        /// Default width of the lines surrounding the label of a section (in pixels).
        /// </summary>
        public const float SectionDefaultLineWidth = 50f;

        /// <summary>
        /// Space on both sides of a section between its label and the horizontal lines (in pixels).
        /// </summary>
        public const float SectionLabelMargins = 5f;

        /// <summary>
        /// Size of logScroll bars drawn in GUI.
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
                                                                              new Color(.1f, .6f, 1f, .4f));

        /// <summary>
        /// Color used for various GUI feedback controls.
        /// </summary>
        public static readonly EditorColor GUIFeedbackColor = new EditorColor(new Color(.2f, .341f, .85f),
                                                                              new Color(.2f, .341f, .85f));

        /// <summary>
        /// Color used to draw gui splitters, separating two layouts.
        /// </summary>
        public static readonly EditorColor GUISplitterColor = new EditorColor(new Color(.13f, .13f, .13f),
                                                                              new Color(.13f, .13f, .13f));

        /// <summary>
        /// Color used to draw background bar controls.
        /// </summary>
        public static readonly EditorColor GUIBarColor = new EditorColor(new Color(.21f, .21f, .21f),
                                                                         new Color(.21f, .21f, .21f));

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
        public static Color GUIThemeBackgroundColor {
            get {
                Color _color = EditorGUIUtility.isProSkin
                                ? DarkThemeGUIBackgroundColor
                                : LightThemeGUIBackgroundColor;

                return _color;
            }
        }
        #endregion

        #region GUI Content
        private const int PropertyLabelCacheLimit = 500;

        private static readonly GUIContent labelGUI = new GUIContent(GUIContent.none);
        private static readonly GUIContent helpBoxGUI = new GUIContent();

        private static readonly Dictionary<string, GUIContent> propertyLabelGUI = new Dictionary<string, GUIContent>();

        // -----------------------

        /// <summary>
        /// Get the <see cref="GUIContent"/> label associated with a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get label from.</param>
        /// <returns>Label associated with the property.</returns>
        public static GUIContent GetPropertyLabel(SerializedProperty _property) {
            // Cache label.
            string _id = EnhancedEditorUtility.GetSerializedPropertyID(_property);

            if (!propertyLabelGUI.TryGetValue(_id, out GUIContent _label)) {
                DisplayNameAttribute _attribute;

                // Clear cache.
                if (propertyLabelGUI.Count > PropertyLabelCacheLimit) {
                    propertyLabelGUI.Clear();
                }

                if (EnhancedEditorUtility.FindSerializedPropertyField(_property, out FieldInfo _info) && (_attribute = _info.GetCustomAttribute<DisplayNameAttribute>()) != null) {
                    if ((_attribute.NameMember != null) && _attribute.NameMember.Value.GetValue(_property, out string _name)) {
                        _attribute.SetName(_name);
                    }

                    GUIContent _displayLabel = _attribute.Label;
                    _label = new GUIContent(_displayLabel.text, string.IsNullOrEmpty(_displayLabel.tooltip) ? _property.tooltip : _displayLabel.tooltip);
                } else {
                    _label = new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip);
                }

                propertyLabelGUI.Add(_id, _label);
            }

            return _label;
        }

        /// <inheritdoc cref="GetLabelGUI(string, string)"/>
        public static GUIContent GetLabelGUI(string _label) {
            return GetLabelGUI(_label, string.Empty);
        }

        /// <summary>
        /// Get a cached <see cref="GUIContent"/> for a specific label.
        /// </summary>
        /// <param name="_label"><see cref="GUIContent"/> text label.</param>
        /// <param name="_tooltip"><see cref="GUIContent"/> tooltip.</param>
        /// <returns><see cref="GUIContent"/> to use.</returns>
        public static GUIContent GetLabelGUI(string _label, string _tooltip) {
            labelGUI.text = _label;
            labelGUI.tooltip = _tooltip;
            labelGUI.image = null;

            return labelGUI;
        }

        /// <summary>
        /// Get a cached <see cref="GUIContent"/> for a specific image.
        /// </summary>
        /// <param name="_image">This <see cref="GUIContent"/> image.</param>
        /// <param name="_tooltip"><see cref="GUIContent"/> tooltip.</param>
        /// <returns><see cref="GUIContent"/> to use.</returns>
        public static GUIContent GetLabelGUI(Texture _image, string _tooltip) {
            labelGUI.image = _image;
            labelGUI.tooltip = _tooltip;
            labelGUI.text = string.Empty;

            return labelGUI;
        }

        /// <summary>
        /// Get the height to use to draw a help box with a specific message, type and width.
        /// </summary>
        /// <param name="_message">The message text.</param>
        /// <param name="_messageType">The type of message.</param>
        /// <param name="_width">The total width to use to draw the help box.</param>
        /// <returns>Total height that will be used to draw this help box.</returns>
        public static float GetHelpBoxHeight(string _message, UnityEditor.MessageType _messageType, float _width) {
            GUIContent _label;
            if (_messageType != UnityEditor.MessageType.None) {
                _label = helpBoxGUI;
                _label.text = _message;

                // Load an Icon that will help calculate the help box height.
                if (_label.image == null)
                    _label.image = EditorGUIUtility.FindTexture("console.infoicon");
            } else {
                _label = GetLabelGUI(_message);
            }

            float _height = EditorStyles.helpBox.CalcHeight(_label, _width);
            return _height;
        }

        /// <summary>
        /// Calculates the total height used to draw a specific <see cref="GUIStyle"/> in a fixed line amount.
        /// </summary>
        /// <param name="_style">The <see cref="GUIStyle"/> to get the line height.</param>
        /// <param name="_lineCount">Total line count to get the associated height.</param>
        /// <returns>The height used to draw the given line amount with this style.</returns>
        public static float CalculLineHeight(GUIStyle _style, int _lineCount) {
            float _height = _style.CalcHeight(GUIContent.none, 100f);
            float _padding = _style.padding.vertical;

            return ((_height - _padding) * _lineCount) + _padding;
        }

        /// <summary>
        /// Safely loads an Icon content without logging an error if not found.
        /// </summary>
        /// <param name="_icon">The name of the Icon to load.</param>
        /// <param name="_text">Optional Icon text.</param>
        /// <returns>The Icon loaded <see cref="GUIContent"/>.</returns>
        public static GUIContent SafeIconContent(string _icon, string _text = "") {
            bool _enabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;

            GUIContent _content = new GUIContent();

            try {
                _content = EditorGUIUtility.IconContent(_icon, _text);
            } catch (Exception) { }

            Debug.unityLogger.logEnabled = _enabled;
            return _content;
        }
        #endregion

        #region GUI Calculs
        /// <inheritdoc cref="ClampLabelWidth(string, float, GUIStyle)"/>
        public static string ClampLabelWidth(string _label, float _maxWidth) {
            return ClampLabelWidth(_label, _maxWidth, EditorStyles.label);
        }

        /// <inheritdoc cref="ClampLabelWidth(GUIContent, float, GUIStyle)"/>
        public static string ClampLabelWidth(string _label, float _maxWidth, GUIStyle _style) {
            return ClampLabelWidth(GetLabelGUI(_label), _maxWidth, _style).text;
        }

        /// <inheritdoc cref="ClampLabelWidth(GUIContent, float, GUIStyle)"/>
        public static GUIContent ClampLabelWidth(GUIContent _label, float _maxWidth) {
            return ClampLabelWidth(_label, _maxWidth, EditorStyles.label);
        }

        /// <summary>
        /// Clamps the length of a specific label so that it does not exceed a specific width.
        /// </summary>
        /// <param name="_label">The label to clamp.</param>
        /// <param name="_maxWidth">Maximum allowed width.</param>
        /// <param name="_style">The <see cref="GUIStyle"/> used to draw the label.</param>
        /// <returns>The clamped label.</returns>
        public static GUIContent ClampLabelWidth(GUIContent _label, float _maxWidth, GUIStyle _style) {
            int _count = 0;
            float _width = 0f;
            string _text = _label.text;

            float _emptyWidth = _style.CalcSize(GUIContent.none).x;

            while ((_width < _maxWidth) && (_count < _text.Length)) {
                _label.text = _text[_count].ToString();
                _width += _style.CalcSize(_label).x - _emptyWidth;

                _count++;
            }

            _label.text = _text.Substring(0, _count);
            return _label;
        }
        #endregion

        #region Rect and Position
        /// <summary>
        /// The current screen width, scaled by the device dpi.
        /// </summary>
        public static float ScreenWidth {
            get {
                float _width = Mathf.RoundToInt(Screen.width / ScreenScale);
                return _width;
            }
        }

        /// <summary>
        /// The current screen height, scaled by the device dpi.
        /// </summary>
        public static float ScreenHeight {
            get {
                float _width = Mathf.RoundToInt(Screen.height / ScreenScale);
                return _width;
            }
        }

        /// <summary>
        /// The actual editor scale depending on the screen device dpi.
        /// </summary>
        public static float ScreenScale {
            get {
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
        public static Rect GetViewControlRect() {
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
        /// Moves a logScroll to focus on a specific position in view.
        /// </summary>
        /// <param name="_scroll">The current position of the logScroll.</param>
        /// <param name="_position">Position on screen to focus.</param>
        /// <param name="_scrollAreaSize">The size of the logScroll area on screen.</param>
        /// <returns>Focused logScroll value on position.</returns>
        public static Vector2 FocusScrollOnPosition(Vector2 _scroll, Rect _position, Vector2 _scrollAreaSize) {
            // Horizontal focus.
            if (_scroll.x > _position.x) {
                _scroll.x = _position.x;
            } else if ((_scroll.x + _scrollAreaSize.x) < _position.xMax) {
                _scroll.x = _position.xMax - _scrollAreaSize.x;
            }

            // Vertical focus.
            if (_scroll.y > _position.y) {
                _scroll.y = _position.y;
            } else if ((_scroll.y + _scrollAreaSize.y) < _position.yMax) {
                _scroll.y = _position.yMax - _scrollAreaSize.y;
            }

            return _scroll;
        }

        /// <summary>
        /// Get a control rect for the current layout without allocating any more space.
        /// </summary>
        /// <returns>New rect of the current layout.</returns>
        public static Rect GetControlRect() {
            return EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);
        }
        #endregion

        #region Event and Clicks
        public const float ResizeHandlerExtent = 3f;

        private static readonly int resizeControlID = "ReduceSize".GetHashCode();
        private static Vector2 mouseDownPosition = new Vector2();

        // -----------------------

        /// <summary>
        /// Get the current <see cref="Event.commandName"/> type on this position.
        /// </summary>
        /// <returns>This position event command type</returns>
        public static ValidateCommand ValidateCommand(Rect _position, out Event _event) {
            if (_position.Event(out _event) != EventType.ValidateCommand) {
                return Editor.ValidateCommand.None;
            }

            return ValidateCommand(out _event);
        }

        /// <summary>
        /// Get the current <see cref="Event.commandName"/> type.
        /// </summary>
        /// <returns>The event command type</returns>
        public static ValidateCommand ValidateCommand(out Event _event) {
            _event = Event.current;

            if (_event.type != EventType.ValidateCommand) {
                return Editor.ValidateCommand.None;
            }

            switch (_event.commandName) {
                case "Copy":
                    return Editor.ValidateCommand.Copy;

                case "Cut":
                    return Editor.ValidateCommand.Cut;

                case "Paste":
                    return Editor.ValidateCommand.Paste;

                case "Delete":
                    return Editor.ValidateCommand.Delete;

                case "SoftDelete":
                    return Editor.ValidateCommand.SoftDelete;

                case "Duplicate":
                    return Editor.ValidateCommand.Duplicate;

                case "FrameSelected":
                    return Editor.ValidateCommand.FrameSelected;

                case "FrameSelectedWithLock":
                    return Editor.ValidateCommand.FrameSelectedWithLock;

                case "SelectAll":
                    return Editor.ValidateCommand.SelectAll;

                case "Find":
                    return Editor.ValidateCommand.Find;

                case "FocusProjectWindow":
                    return Editor.ValidateCommand.FocusProjectWindow;

                default:
                    break;
            }

            return Editor.ValidateCommand.Unknown;
        }

        /// <summary>
        /// Did the user just performed a context click on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <returns>True if the user performed a context click here, false otherwise.</returns>
        public static bool ContextClick(Rect _position) {
            if (_position.Event(out Event _event) == EventType.ContextClick) {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just performed a double click on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <returns>True if the user performed a double click here, false otherwise.</returns>
        public static bool DoubleClick(Rect _position) {
            if ((_position.Event(out Event _event) == EventType.MouseDown) && (_event.button == 0) && (_event.clickCount > 1)) {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just pressed a mouse button on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <returns>True if the user clicked here, false otherwise.</returns>
        public static bool MouseDown(Rect _position) {
            if (_position.Event(out Event _event) == EventType.MouseDown) {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just released the main mouse button on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <returns>True if the user released this mouse button here, false otherwise.</returns>
        public static bool MainMouseUp(Rect _position) {
            if ((_position.Event(out Event _event) == EventType.MouseUp) && (_event.button == 0)) {
                _event.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Did the user just performed a click to deselect element(s) on this position?
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <returns>True if the user performed a deselection click here, false otherwise.</returns>
        public static bool DeselectionClick(Rect _position) {
            if ((_position.Event(out Event _event) == EventType.MouseDown) && !_event.control && !_event.shift && (_event.button == 0)) {
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
        public static int VerticalKeys() {
            Event _event = Event.current;

            if (_event.type == EventType.KeyDown) {
                switch (_event.keyCode) {
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
        public static int HorizontalSelectionKeys() {
            Event _event = Event.current;

            if (_event.type == EventType.KeyDown) {
                switch (_event.keyCode) {
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
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_position']"/></param>
        /// <param name="_collection"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_collection']"/></param>
        /// <param name="_index">Index of the current array element.</param>
        /// <param name="_canBeSelected"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_canBeSelected']"/></param>
        /// <param name="_isSelected"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_isSelected']"/></param>
        /// <param name="_onSelect"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_onSelect']"/></param>
        /// <returns>True if the user performed a click to select element(s) here, false otherwise.</returns>
        public static bool MultiSelectionClick(Rect _position, IList _collection, int _index, Predicate<int> _isSelected, Predicate<int> _canBeSelected, Action<int, bool> _onSelect) {
            // Only select on mouse down or on selected element mouse up.
            bool _isMouseUpSelection = false;
            if (_position.Event(out Event _event) != EventType.MouseDown) {
                if ((_position.Event() != EventType.MouseUp) || (_event.button != 0) || (_event.mousePosition != mouseDownPosition)) {
                    if (_event.type == EventType.MouseDrag)
                        mouseDownPosition = Vector2.zero;

                    return false;
                }

                _isMouseUpSelection = true;
            }

            // Mouse up helper update.
            if (!_isMouseUpSelection && _isSelected(_index)) {
                mouseDownPosition = _event.mousePosition;
                _event.Use();
                return true;
            } else
                mouseDownPosition = Vector2.zero;

            if (_event.shift) {
                int _firstIndex = -1;
                int _lastIndex = -1;

                // Find first index.
                for (int _i = 0; _i < _collection.Count; _i++) {
                    if (_isSelected(_i) || (_i == _index)) {
                        _firstIndex = _i;
                        break;
                    }
                }

                // Find last index.
                for (int _i = _collection.Count; _i-- > 0;) {
                    if (_isSelected(_i) || (_i == _index)) {
                        _lastIndex = _i + 1;
                        break;
                    }
                }

                // Select all elements between indexes.
                if (_index == _firstIndex) {
                    for (int _i = _lastIndex; _i-- > _firstIndex;) {
                        if (_canBeSelected(_i)) {
                            _onSelect(_i, true);
                        }
                    }
                } else {
                    for (int _i = _firstIndex; _i < _lastIndex; _i++) {
                        if (_canBeSelected(_i)) {
                            _onSelect(_i, true);
                        }
                    }
                }
            } else if (_event.control) {
                // Inverse selected state.
                bool _selected = _isSelected(_index);
                _onSelect(_index, !_selected);
            } else if (!_isSelected(_index) || (_event.button == 0)) {
                // Unselect every element except this one.
                for (int _i = 0; _i < _collection.Count; _i++) {
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
        /// <param name="_collection"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_collection']"/></param>
        /// <param name="_isSelected"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_isSelected']"/></param>
        /// <param name="_canBeSelected"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_canBeSelected']"/></param>
        /// <param name="_onSelect"><inheritdoc cref="DocumentationMethod(Rect, IList, Predicate{int}, Predicate{int}, Action{int, bool})" path="/param[@name='_onSelect']"/></param>
        /// <param name="_lastSelectedIndex">Index of the last selected element. Use -1 if no element is currently selected.</param>
        /// <returns>True if the user pressed a key to select element(s), from otherwise.</returns>
        public static bool VerticalMultiSelectionKeys(IList _collection, Predicate<int> _isSelected, Predicate<int> _canBeSelected, Action<int, bool> _onSelect, int _lastSelectedIndex) {
            // Do not select anything on an empty array.
            if (_collection.Count == 0)
                return false;

            if ((_lastSelectedIndex > -1) && ((_lastSelectedIndex >= _collection.Count) || !_canBeSelected(_lastSelectedIndex)))
                _lastSelectedIndex = -1;

            // Get selection value.
            int _switch = VerticalKeys();
            switch (_switch) {
                case -1: {
                    // Find first selected element if none.
                    if (_lastSelectedIndex > -1)
                        break;

                    for (int _i = 0; _i < _collection.Count; _i++) {
                        if (_isSelected(_i)) {
                            _lastSelectedIndex = _i;
                            break;
                        }
                    }
                }
                break;

                case 1: {
                    // Find last selected element if none.
                    if (_lastSelectedIndex > -1)
                        break;

                    for (int _i = _collection.Count; _i-- > 0;) {
                        if (_isSelected(_i)) {
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
            if (_lastSelectedIndex < 0) {
                int _index, _limit;
                if (_switch == 1) {
                    _index = _collection.Count - 1;
                    _limit = -1;
                } else {
                    _index = 0;
                    _limit = _collection.Count;
                }

                for (int _i = _index; _i != _limit; _i -= _switch) {
                    if (_canBeSelected(_i)) {
                        _onSelect(_index, true);
                        break;
                    }
                }

                return true;
            }

            Event _event = Event.current;
            if (_event.shift || _event.control) {
                // Multi-selection.
                int _firstIndex = -1;
                int _lastIndex = -1;

                // Find first index.
                for (int _i = 0; _i < _collection.Count; _i++) {
                    if (_isSelected(_i)) {
                        _firstIndex = _i;
                        break;
                    }
                }

                // Find last index.
                for (int _i = _collection.Count; _i-- > 0;) {
                    if (_isSelected(_i)) {
                        _lastIndex = _i + 1;
                        break;
                    }
                }

                // Select elements.
                if (_switch == -1) {
                    bool _increase = _lastSelectedIndex == _firstIndex;
                    if (_increase) {
                        int _index = GetNewSelectedIndex();
                        for (int _i = _lastIndex; _i-- > _index;) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, true);
                            }
                        }
                    } else {
                        for (int _i = _firstIndex; _i < _lastSelectedIndex; _i++) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, true);
                            }
                        }

                        for (int _i = _lastSelectedIndex; _i < _lastIndex; _i++) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, false);
                            }
                        }
                    }
                } else {
                    bool _increase = _lastSelectedIndex == (_lastIndex - 1);
                    if (_increase) {
                        int _index = GetNewSelectedIndex() + 1;
                        for (int _i = _firstIndex; _i < _index; _i++) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, true);
                            }
                        }
                    } else {
                        for (int _i = _firstIndex; _i < (_lastSelectedIndex + 1); _i++) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, false);
                            }
                        }

                        for (int _i = _lastIndex; _i-- > (_lastSelectedIndex + 1);) {
                            if (_canBeSelected(_i)) {
                                _onSelect(_i, true);
                            }
                        }
                    }
                }
            } else {
                // Select this element only.
                int _index = GetNewSelectedIndex();

                for (int _i = 0; _i < _collection.Count; _i++) {
                    bool _selected = _i == _index;
                    _onSelect(_i, _selected);
                }
            }

            return true;

            // ----- Local Method ----- \\

            int GetNewSelectedIndex() {
                int _index = _lastSelectedIndex;
                while (true) {
                    _index += _switch;
                    if ((_index == -1) || (_index == _collection.Count)) {
                        _index = _lastSelectedIndex;
                        break;
                    }

                    if (_canBeSelected(_index))
                        break;
                }

                return _index;
            }
        }

        /// <summary>
        /// Makes an horizontally resizable area from its left border.
        /// </summary>
        /// <param name="_area">The total resizable area position on screen.</param>
        /// <param name="_minWidth">Minimum allowed width of the area.</param>
        /// <param name="_maxWidth">Maximum allowed width of the area.</param>
        /// <returns>The new width of the area. Make sure to only modify it when <see cref="GUI.changed"/> is set to true.</returns>
        public static float HorizontalSplitterHandle(Rect _area, float _minWidth, float  _maxWidth) {
            int _controlID = GetControlID(resizeControlID, FocusType.Passive);
            Rect _splitter = new Rect(_area) {
                x = _area.x - ResizeHandlerExtent,
                width = ResizeHandlerExtent * 2f
            };

            EditorGUIUtility.AddCursorRect(_splitter, MouseCursor.ResizeHorizontal, _controlID);
            return Mathf.Clamp(SplitterHandle(_controlID, _area, _splitter).width, _minWidth, _maxWidth);
        }

        /// <summary>
        /// Makes a vertically resizable area from its top border.
        /// </summary>
        /// <param name="_area">The total resizable area position on screen.</param>
        /// <param name="_minHeight">Minimum allowed height of the area.</param>
        /// <param name="_maxHeight">Maximum allowed height of the area.</param>
        /// <returns>The new height of the area. Make sure to only modify it when <see cref="GUI.changed"/> is set to true.</returns>
        public static float VerticalSplitterHandle(Rect _area, float _minHeight, float _maxHeight) {
            // Event and state control.
            int _controlID = GetControlID(resizeControlID, FocusType.Passive);
            Rect _splitter = new Rect(_area) {
                yMin = _area.y - ResizeHandlerExtent,
                height = ResizeHandlerExtent * 2f
            };

            EditorGUIUtility.AddCursorRect(_splitter, MouseCursor.ResizeVertical, _controlID);
            return Mathf.Clamp(SplitterHandle(_controlID, _area, _splitter).height, _minHeight, _maxHeight);
        }

        // -----------------------

        private static Rect SplitterHandle(int _controlID, Rect _area, Rect _splitter) {
            Event _event = Event.current;

            switch (_event.GetTypeForControl(_controlID)) {
                // Prepare resize.
                case EventType.MouseDown:
                    if (_splitter.Contains(_event.mousePosition) && (GUIUtility.hotControl == 0) && (_event.button == 0)) {
                        GUIUtility.hotControl = _controlID;
                        GUIUtility.keyboardControl = 0;

                        _event.Use();
                    }
                    break;

                // ReduceSize.
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == _controlID) {
                        _area.size = _area.max - _event.mousePosition;

                        GUI.changed = true;
                        _event.Use();
                    }
                    break;

                // Stop resize.
                case EventType.MouseUp:
                    if ((GUIUtility.hotControl == _controlID) && (_event.button == 0)) {
                        GUIUtility.hotControl = 0;
                        _event.Use();
                    }
                    break;
            }

            return _area;
        }
        #endregion

        #region Control ID
        private static readonly List<int> controlsID = new List<int>();

        private static int controldIDIndex = -1;
        private static int activeIDCount = 0;

        private static int lastID = 0;

        // -----------------------

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(FocusType _focusType) {
            GUIContent _label = GUIContent.none;
            return GetControlID(_label, _focusType);
        }

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(FocusType _focusType, Rect _position) {
            GUIContent _label = GUIContent.none;
            return GetControlID(_label, _focusType, _position);
            ;
        }

        /// <inheritdoc cref="GetControlID(GUIContent, FocusType, Rect)"/>
        public static int GetControlID(GUIContent _label, FocusType _focusType) {
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
        public static int GetControlID(GUIContent _label, FocusType _focusType, Rect _position) {
            int _id = GUIUtility.GetControlID(_label, _focusType, _position);
            return DoControlID(_id);
        }

        /// <inheritdoc cref="GetControlID(int, FocusType, Rect)"/>
        public static int GetControlID(int _hint, FocusType _focusType) {
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
        public static int GetControlID(int _hint, FocusType _focusType, Rect _position) {
            int _id = GUIUtility.GetControlID(_hint, _focusType, _position);
            return DoControlID(_id);
        }

        // -----------------------

        private static int DoControlID(int _id) {
            /* Sometimes, with some special events (like EventType.Used), the returned id may be equal to -1.
             * However, this unwanted behaviour breaks many enhanced controls (like dynamic height ones),
             * as all following ids are then also equal to -1, and their associated registered value cannot be properly retrieved.
             * 
             * So let's store all previous event ids, and replace undesired values with them.
             * (You can reactivate the following debugs if you want to understand the default behaviour).
            */

            //Debug.Log("ID => " + _id + " | " + Event.current.type);

            if (_id == -1) {
                // Unregistered id.
                if (controlsID.Count == 0)
                    return -1;

                // Invalid.
                controldIDIndex = (controldIDIndex < activeIDCount)
                                ? (controldIDIndex + 1)
                                : 0;

                _id = controlsID[controldIDIndex];
            } else if ((controldIDIndex != -1) && (_id < controlsID[controldIDIndex])) {
                // Restart.
                activeIDCount = controldIDIndex;

                controldIDIndex = 0;
                controlsID[controldIDIndex] = _id;
            } else {
                // Increment.
                controldIDIndex++;
                if (controldIDIndex == controlsID.Count) {
                    controlsID.Add(_id);
                } else {
                    controlsID[controldIDIndex] = _id;
                }
            }

            //Debug.LogWarning("ID => " + _id + " | " + Event.current.type);

            lastID = _id;
            return _id;
        }

        /// <summary>
        /// Get the id returned by the last control request.
        /// </summary>
        /// <returns></returns>
        public static int GetLastControlID() {
            return lastID;
        }
        #endregion

        #region Utility
        private static readonly MethodInfo getActiveFolderPathMethodInfo = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath",
                                                                                                               BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Get the path of the currently selected folder. If multiple are selected, returns the first one.
        /// </summary>
        /// <returns>The path of the first currently selected folder.</returns>
        public static string GetActiveFolderPath() {
            return getActiveFolderPathMethodInfo.Invoke(null, null) as string;
        }

        /// <summary>
        /// Repaints all editors associated with a specific <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_object"><see cref="SerializedObject"/> to repaint associated editor(s).</param>
        public static void Repaint(SerializedObject _object) {
            UnityEditor.Editor[] _editors = ActiveEditorTracker.sharedTracker.activeEditors;
            foreach (UnityEditor.Editor _editor in _editors) {
                if (_editor.serializedObject == _object) {
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
        /// <param name="_collection">Collection of all selectable elements.</param>
        /// <param name="_isSelected">Used to know if a specific element is selected.</param>
        /// <param name="_canBeSelected">Used to know if a specific element can be selected.</param>
        /// <param name="_onSelect">Callback when selecting an element.</param>
        internal static void DocumentationMethod(Rect _position, IList _collection, Predicate<int> _isSelected, Predicate<int> _canBeSelected, Action<int, bool> _onSelect) { }
        #endregion
    }
}
