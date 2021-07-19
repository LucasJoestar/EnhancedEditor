// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Auto laid out version of <see cref="EnhancedEditorGUI"/>.
    /// </summary>
    public static class EnhancedEditorGUILayout
    {
        #region Property Drawers

        #region Asset Preview
        /// <summary>
        /// Draws a property field with its object reference
        /// asset preview within a foldout below it.
        /// </summary>
        /// <param name="_property">Property to draw with asset preview.</param>
        /// <param name="_previewSize">Size of the asset preview.</param>
        /// <param name="_isUnfolded">Asset preview foldout state.</param>
        /// <returns>New asset preview foldout state.</returns>
        public static bool AssetPreviewField(SerializedProperty _property, float _previewSize, bool _isUnfolded)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            return AssetPreviewField(_property, _label, _previewSize, _isUnfolded);
        }

        /// <summary>
        /// Draws a property field with its object reference
        /// asset preview within a foldout below it.
        /// </summary>
        /// <param name="_property">Property to draw with asset preview.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_previewSize">Size of the asset preview.</param>
        /// <param name="_isUnfolded">Asset preview foldout state.</param>
        /// <returns>New asset preview foldout state.</returns>
        public static bool AssetPreviewField(SerializedProperty _property, GUIContent _label, float _previewSize, bool _isUnfolded)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            _isUnfolded = EnhancedEditorGUI.AssetPreviewField(_position, _property, _label, _previewSize, _isUnfolded, out float _height);

            EditorGUILayout.GetControlRect(true, _height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);
            return _isUnfolded;
        }
        #endregion

        #region Max
        /// <summary>
        /// Restrains a property value so it does not exceed a maximum.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void MaxField(SerializedProperty _property, float _maxValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_property, _label, _maxValue);
        }

        /// <summary>
        /// Restrains a property value so it does not exceed a maximum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void MaxField(SerializedProperty _property, GUIContent _label, float _maxValue)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.MaxField(_position, _property, _label, _maxValue);
        }
        #endregion

        #region Min
        /// <summary>
        /// Restrains a property value so it does not go under a maximum.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void MinField(SerializedProperty _property, float _minValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_property, _label, _minValue);
        }

        /// <summary>
        /// Restrains a property value so it does not go under a maximum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void MinField(SerializedProperty _property, GUIContent _label, float _minValue)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.MinField(_position, _property, _label, _minValue);
        }
        #endregion

        #region Min Max
        /// <summary>
        /// Draws a property as a min-max slider field.
        /// </summary>
        /// <param name="_property">Property to draw as a min-max field.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(SerializedProperty _property, float _minLimit, float _maxLimit)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_property, _label, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Draws a property as a min-max slider field.
        /// </summary>
        /// <param name="_property">Property to draw as a min-max field.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.MinMaxField(_position, _property, _label, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Draws a <see cref="Vector2"/> as a min-max slider field.
        /// </summary>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_value">Editing vector.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        /// <returns>New vector value.</returns>
        public static Vector2 MinMaxField(GUIContent _label, Vector2 _vector, float _minLimit, float _maxLimit)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _vector, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Draws a <see cref="Vector2Int"/> as a min-max slider field.
        /// </summary>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_value">Editing vector.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        /// <returns>New vector value.</returns>
        public static Vector2Int MinMaxField(GUIContent _label, Vector2Int _vector, int _minLimit, int _maxLimit)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _vector, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Draws a min-max slider field.
        /// </summary>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_minValue">Editing minimum value.</param>
        /// <param name="_maxValue">Editing maximum value.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(GUIContent _label, ref float _minValue, ref float _maxValue, float _minLimit, float _maxLimit)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.MinMaxField(_position, _label, ref _minValue, ref _maxValue, _minLimit, _maxLimit);
        }
        #endregion

        #region Property Field
        /// <summary>
        /// Draws a property field associated with a property,
        /// which value will be set whenever value is changed in the inspector.
        /// Usefull for performing additional operations when value is changed.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_propertyName">Name of the associated property to set (must be in the same script as the property field).</param>
        public static void PropertyField(SerializedProperty _property, string _propertyName)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PropertyField(_property, _label, _propertyName);
        }

        /// <summary>
        /// Draws a property field associated with a property,
        /// which value will be set whenever value is changed in the inspector.
        /// Usefull for performing additional operations when value is changed.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_propertyName">Name of the associated property to set (must be in the same script as the property field).</param>
        public static void PropertyField(SerializedProperty _property, GUIContent _label, string _propertyName)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.PropertyField(_position, _property, _label, _propertyName);
        }
        #endregion

        #region Progress Bar

        #region Editable
        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, string _maxValueVariableName, Color _color, float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            EditableProgressBar(_property, _label, _maxValueVariableName, _color, _height);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, GUIContent _label, string _maxValueVariableName, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            EnhancedEditorGUI.EditableProgressBar(_position, _property, _label, _maxValueVariableName, _color);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, float _maxValue, Color _color, float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            EditableProgressBar(_property, _label, _maxValue, _color, _height);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, GUIContent _label, float _maxValue, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            EnhancedEditorGUI.EditableProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_value">Progress bar actual progression value.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        /// <returns>New progress bar value.</returns>
        public static float EditableProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            return EnhancedEditorGUI.EditableProgressBar(_position, _label, _value, _maxValue, _color);
        }
        #endregion

        #region Readonly
        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, string _maxValueVariableName, Color _color, float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValueVariableName, _color, _height);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, string _maxValueVariableName, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            EnhancedEditorGUI.ProgressBar(_position, _property, _label, _maxValueVariableName, _color);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, float _maxValue, Color _color, float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValue, _color, _height);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, float _maxValue, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            EnhancedEditorGUI.ProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_value">Progress bar actual progression value.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Height of the progress bar (in pixels).</param>
        public static void ProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, float _height)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height);
            EnhancedEditorGUI.ProgressBar(_position, _label, _value, _maxValue, _color);
        }
        #endregion

        #endregion

        #region Readonly
        /// <summary>
        /// Draws a readonly property field.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_useRadioToggle">If the property is a boolean, indicates if using a radio toggle or a standard one.</param>
        public static void ReadOnlyProperty(SerializedProperty _property, bool _useRadioToggle = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ReadOnlyProperty(_property, _label, _useRadioToggle);
        }

        /// <summary>
        /// Draws a readonly property field.
        /// </summary>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_useRadioToggle">If the property is a boolean, indicates if using a radio toggle or a standard one.</param>
        public static void ReadOnlyProperty(SerializedProperty _property, GUIContent _label, bool _useRadioToggle = false)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.ReadOnlyProperty(_position, _property, _label, _useRadioToggle);
        }
        #endregion

        #region Required
        /// <summary>
        /// Draws a required property field,
        /// showing an error help box when the property object reference value is null.
        /// </summary>
        /// <param name="_property">Property to draw and check validity.</param>
        public static void RequiredProperty(SerializedProperty _property)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            RequiredProperty(_property, _label);
        }

        /// <summary>
        /// Draws a required property field,
        /// showing an error help box when the property object reference value is null.
        /// </summary>
        /// <param name="_property">Property to draw and check validity.</param>
        /// <param name="_label">Label displayed before property.</param>
        public static void RequiredProperty(SerializedProperty _property, GUIContent _label)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.RequiredProperty(_position, _property, _label, out float _height);

            EditorGUILayout.GetControlRect(true, _height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);
        }
        #endregion

        #endregion

        #region Decorator Drawers

        #region Horizontal Line
        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="_color">Line color.</param>
        public static void HorizontalLine(Color _color)
        {
            HorizontalLine(HorizontalLineAttribute.DefaultHeight, 1f, _color);
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="_height">Line height (in pixels).</param>
        /// <param name="_color">Line color.</param>
        public static void HorizontalLine(float _height, Color _color)
        {
            HorizontalLine(_height, 1f, _color);
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="_height">Line height (in pixels).</param>
        /// <param name="_width">Line width in percent (from 0 to 1).</param>
        /// <param name="_color">Line color.</param>
        public static void HorizontalLine(float _height, float _width, Color _color)
        {
            Rect _position = EditorGUILayout.GetControlRect(false, _height + EditorGUIUtility.singleLineHeight);
            _position.y += (EditorGUIUtility.singleLineHeight / 2f) - 1;
            _position.height = _height;

            EnhancedEditorGUI.HorizontalLine(_position, _width, _color);
        }
        #endregion

        #region Section
        /// <summary>
        /// Draws a section, that is a header-like label usefull for ordering your editor.
        /// </summary>
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        public static void Section(GUIContent _label, float _lineWidth)
        {
            Section(_label, _lineWidth, SectionAttribute.DefaultMargins);
        }

        /// <summary>
        /// Draws a section, that is a header-like label usefull for ordering your editor.
        /// </summary>
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        /// <param name="_heightSpace">Space on top and bottom of the section (in pixels).</param>
        public static void Section(GUIContent _label, float _lineWidth, float _heightSpace)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, (EditorGUIUtility.singleLineHeight + _heightSpace) * 2);
            EnhancedEditorGUI.Section(_position, _label, _lineWidth);
        }
        #endregion

        #endregion

        #region Others
        /// <summary>
        /// Draws a texture on screen.
        /// Height is automatically adjusted.
        /// </summary>
        /// <param name="_texture">Texture to draw.</param>
        public static void Texture(Texture2D _texture)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, 0f);
            float _height = EnhancedEditorGUI.Texture(_position, _texture);

            EditorGUILayout.GetControlRect(true, _height);
        }

        /// <summary>
        /// Draws a clickable label, redirecting to a specified url.
        /// </summary>
        /// <param name="_label">Displayed label redirecting to url.</param>
        /// <param name="_url">Link associated url.</param>
        public static void LinkLabel(GUIContent _label, string _url)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EnhancedEditorGUI.LinkLabel(_position, _label, _url);
        }

        /// <summary>
        /// Draws an underlined label.
        /// </summary>
        /// <param name="_label">Label to display.</param>
        /// <param name="_style">Style used to draw label.</param>
        public static void UnderlinedLabel(GUIContent _label, GUIStyle _style)
        {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 1f);
            EnhancedEditorGUI.UnderlinedLabel(_position, _label, _style);
        }
        #endregion

        public static string ToolbarSearchField(string _content, params GUILayoutOption[] _options)
        {
            Rect _searchRect = EditorGUILayout.GetControlRect(_options);
            Rect _buttonRect = _searchRect;
            _buttonRect.xMin = _buttonRect.xMax - 14f;

            if (!string.IsNullOrEmpty(_content))
            {
                EditorGUIUtility.AddCursorRect(_buttonRect, MouseCursor.Arrow);
                if (Event.current.type == EventType.MouseUp && _buttonRect.Contains(Event.current.mousePosition))
                {
                    _content = string.Empty;
                    Event.current.Use();
                }
            }

            EditorGUIUtility.AddCursorRect(_searchRect, MouseCursor.Text);
            _content = GUI.TextField(_searchRect, _content, EditorStyles.toolbarSearchField);

            if (!string.IsNullOrEmpty(_content))
                GUI.Button(_buttonRect, GUIContent.none, new GUIStyle("ToolbarSeachCancelButton"));

            return _content;
        }

        public static int CenteredToolbar(int _selected, GUIContent[] _contents, params GUILayoutOption[] _options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int _selectedTab = GUILayout.Toolbar(_selected, _contents, _options);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return _selectedTab;
        }

        private static readonly GUIContent sortAscendingGUI = new GUIContent("↑", "Sort in ascending order.");
        private static readonly GUIContent sortDescendingGUI = new GUIContent("↓", "Sort in descending order.");

        public static bool ToolbarSortOptions(ref int _selectedOption, ref bool _doSortAscending, GUIContent[] _displayedOptions, params GUILayoutOption[] _options)
        {
            Rect _position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbar, _options);
            bool _changed = false;

            _position.xMax -= 20f;
            int _sortOption = EditorGUI.Popup(_position, _selectedOption, _displayedOptions, EditorStyles.toolbarDropDown);
            if (_sortOption != _selectedOption)
            {
                _selectedOption = _sortOption;
                _changed = true;
            }

            _position.xMin = _position.xMax;
            _position.xMax += 20f;
            if (GUI.Button(_position, _doSortAscending ? sortAscendingGUI : sortDescendingGUI, EditorStyles.toolbarButton))
            {
                _doSortAscending = !_doSortAscending;
                _changed = true;
            }

            return _changed;
        }
    }
}
