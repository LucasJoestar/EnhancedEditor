using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contain a bunch of useful methods for GUI related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EditorGUIEnhanced
    {
        #region Fields / Properties
        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Indicates if the user is currently dragging a progress bar.
        /// </summary>
        private static bool     isDraggingProgressBar =     false;
        #endregion

        #region Methods

        #region Property Drawers

        #region Property Field
        /**********************************
         *******   PROPERTY FIELD   *******
         *********************************/

        /// <summary>
        /// Draw a serialized property field associated with another property ;
        /// when setting field value, associated property will be set automatically.
        /// Usefull for clamping value or calling event on inspector edit.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Serialized property to edit.</param>
        /// <param name="_propertyName">Name of the associated property (must be in the same script as the property field).</param>
        public static void PropertyField(Rect _position, SerializedProperty _property, string _propertyName) => PropertyField(_position, _property, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), _propertyName);

        /// <summary>
        /// Draw a serialized property field associated with another property ;
        /// when setting field value, associated property will be set automatically.
        /// Usefull for clamping value or calling event on inspector edit.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Serialized property to edit.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_propertyName">Name of the associated property (must be in the same script as the property field).</param>
        public static void PropertyField(Rect _position, SerializedProperty _property, GUIContent _label, string _propertyName)
        {
            // Draw property field and new value by reflection
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);

            if (EditorGUI.EndChangeCheck())
            {
                _property.serializedObject.ApplyModifiedProperties();
                EditorGUIUtilityEnhanced.SetPropertyValue(_property, _propertyName);
            }
        }
        #endregion

        #region Progress Bar
        /********************************
         *******   PROGRESS BAR   *******
         *******************************/

        /********** UTILITY **********/

        /// <summary>
        /// Try to get a progress bar max value or display error box.
        /// </summary>
        /// <param name="_position">Rect to draw error box if needed.</param>
        /// <param name="_property">Object to get variable value from.</param>
        /// <param name="_maxValueVariableName">Name of the variable acting as maximum value.</param>
        /// <param name="_maxValue">Max value.</param>
        /// <returns>Returns false if variable type is incompatible, true otherwise.</returns>
        private static bool TryGetProgressBarMaxValue(Rect _position, SerializedProperty _property, string _maxValueVariableName, out float _maxValue)
        {
            // Try to get progress bar max value
            if (EditorGUIUtilityEnhanced.GetSerializedObjectFieldOrPropertyValueAsSingle(_property.serializedObject, _maxValueVariableName, out _maxValue)) return true;

            // If variable type is not comptatible, show error box
            EditorGUI.HelpBox(_position, $"Error : {_property.name} field | \"{_maxValueVariableName}\" value cannot be converted to single !", MessageType.Error);
            return false;
        }

        /// <summary>
        /// Try to get a progress bar value or display error box.
        /// </summary>
        /// <param name="_position">Rect to draw error box if needed.</param>
        /// <param name="_property">Property to get value from.</param>
        /// <param name="_value">Value of the property.</param>
        /// <returns>Returns false if property type is incompatible, true otherwise.</returns>
        private static bool TryGetProgressBarValue(Rect _position, SerializedProperty _property, out float _value)
        {
            // Try to get progress bar value
            if (EditorGUIUtilityEnhanced.GetPropertyValueAsSingle(_property, out _value)) return true;

            // If property type is not comptatible, show error box
            EditorGUI.HelpBox(_position, $"Error : {_property.name} field | Progress bar cannot be used with \"{_property.propertyType}\" variable type !", MessageType.Error);
            return false;
        }

        /********** EDITABLE **********/

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color) => EditableProgressBar(_position, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        public static void EditableProgressBar(Rect _position, GUIContent _label, SerializedProperty _property, string _maxValueVariableName, Color _color)
        {
            // Convert indicated maximum variable value to single, or display error box
            if (!TryGetProgressBarMaxValue(_position, _property, _maxValueVariableName, out float _maxValue)) return;

            // Draw editable progress bar
            EditableProgressBar(_position, _label, _property, _maxValue, _color);
        }

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color) => EditableProgressBar(_position, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), _property, _maxValue, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        public static void EditableProgressBar(Rect _position, GUIContent _label, SerializedProperty _property, float _maxValue, Color _color)
        {
            // Convert property value to single, or display error box
            if (!TryGetProgressBarValue(_position, _property, out float _value)) return;

            // Draw editable progress bar
            float _newValue = EditableProgressBar(_position, _label, _value, _maxValue, _color);

            // If value changed, set it
            if (_value != _newValue)
            {
                switch (_property.propertyType)
                {
                    case SerializedPropertyType.Enum:
                        _property.enumValueIndex = Mathf.RoundToInt(_newValue);
                        break;

                    case SerializedPropertyType.Integer:
                        _property.intValue = Mathf.RoundToInt(_newValue);
                        break;

                    case SerializedPropertyType.Float:
                        _property.floatValue = _newValue;
                        break;
                }
            }
        }

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_value">Bar progression value.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <returns>Returns new progress bar value.</returns>
        public static float EditableProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color)
        {
            // First, draw progress bar
            ProgressBar(_position, _label, _value, _maxValue, _color);

            // Then, return new value
            return MakeEditableProgressBar(_position, _value, _maxValue);
        }

        /// <summary>
        /// Make editable progress bar.
        /// </summary>
        /// <param name="_position">Progress bar position.</param>
        /// <param name="_value">Progress bar actual value.</param>
        /// <param name="_maxValue">Progress bar max value.</param>
        /// <returns>Returns new progress bar value.</returns>
        private static float MakeEditableProgressBar(Rect _position, float _value, float _maxValue)
        {
            // Get current event and a hot control
            Event _event = Event.current;
            int _controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Allow user to drag bar value if not already doing it
            if (!isDraggingProgressBar)
            {
                _position.x += (_position.width * Mathf.Clamp(_value / _maxValue, 0, 1)) - 5;
                _position.width = 10;

                // Change cursor when at the edge of filled bar
                EditorGUIUtility.AddCursorRect(_position, MouseCursor.ResizeHorizontal);

                if ((_event.GetTypeForControl(_controlID) == EventType.MouseDown) && _position.Contains(_event.mousePosition))
                {
                    // Set using hot control and use event
                    GUIUtility.hotControl = _controlID;
                    _event.Use();

                    isDraggingProgressBar = true;
                }
            }
            // If dragging it, change editing value on mouse move
            else
            {
                // Change cursor while mouse is in property rect
                EditorGUIUtility.AddCursorRect(_position, MouseCursor.ResizeHorizontal);

                // While dragging, set variable value
                if (_event.GetTypeForControl(_controlID) == EventType.MouseDrag)
                {
                    // Set using hot control and use event
                    GUIUtility.hotControl = _controlID;
                    _event.Use();

                    // Return clamped value on mouse position
                    float _newValue = (float)Math.Round(((_event.mousePosition.x - _position.x) / _position.width) * _maxValue, 2);
                    return Mathf.Clamp(_newValue, 0, _maxValue);
                }
                // Stop dragging when releasing mouse
                if (_event.GetTypeForControl(_controlID) == EventType.MouseUp)
                {
                    // Reset hot control and use event
                    GUIUtility.hotControl = 0;
                    _event.Use();

                    isDraggingProgressBar = false;
                }
            }

            // Return value if not changed
            return _value;
        }

        /********** READ ONLY **********/

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color) => ProgressBar(_position, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        public static void ProgressBar(Rect _position, GUIContent _label, SerializedProperty _property, string _maxValueVariableName, Color _color)
        {
            // Convert indicated maximum variable value to single, or display error box
            if (!TryGetProgressBarMaxValue(_position, _property, _maxValueVariableName, out float _maxValue)) return;

            // Draw progress bar
            ProgressBar(_position, _label, _property, _maxValue, _color);
        }

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color) => ProgressBar(_position, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), _property, _maxValue, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        public static void ProgressBar(Rect _position, GUIContent _label, SerializedProperty _property, float _maxValue, Color _color)
        {
            // Convert property value to single, or display error box
            if (!TryGetProgressBarValue(_position, _property, out float _value)) return;

            // Draw progress bar
            ProgressBar(_position, _label, _value, _maxValue, _color);
        }

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw progress bar.</param>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_value">Bar progression value.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        public static void ProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color)
        {
            // Let's draw the bar
            // First, draw filled bar
            Rect _barRect = new Rect(_position.x, _position.y, _position.width * Mathf.Clamp(_value / _maxValue, 0, 1), _position.height);
            EditorGUI.DrawRect(_barRect, _color);

            // Now, draw empty bar if not full
            if (_barRect.width < _position.width)
            {
                _barRect.x += _barRect.width;
                _barRect.width = _position.width - _barRect.width;
                EditorGUI.DrawRect(_barRect, SuperColor.SmokyBlack.GetColor());
            }

            // Now let's draw the label ; first, get label value
            _label.text = $"[{_label.text}]{(_position.height > EditorGUIUtility.singleLineHeight * 2 ? "\n" : " ")}{_value.ToString("0.##")} / {_maxValue}";

            // Then resize rect position
            Vector2 _labelSize = EditorStyles.boldLabel.CalcSize(_label);
            _position.x += _position.x + ((_position.width - _labelSize.x) / 2f);
            _position.y += ((_position.height - _labelSize.y) / 2f) - 2;
            _position.size = _labelSize;

            // And finaly draw it in a shadow style
            EditorGUI.DropShadowLabel(_position, _label);
        }
        #endregion

        #region Readonly
        /****************************
         *******   READONLY   *******
         ***************************/

        /// <summary>
        /// Draw a readonly property field.
        /// </summary>
        /// <param name="_position">Rect to draw field in</param>
        /// <param name="_property">Property to display</param>
        public static void ReadOnlyProperty(Rect _position, SerializedProperty _property) => ReadOnlyProperty(_position, _property, new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip), false);

        /// <summary>
        /// Draw a readonly property field.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to display.</param>
        /// <param name="_label">Label to display before property.</param>
        /// <param name="_useRadioToggle">If property is boolean type, indicates if using radio toggle or standard one.</param>
        public static void ReadOnlyProperty(Rect _position, SerializedProperty _property, GUIContent _label, bool _useRadioToggle = false)
        {
            GUI.enabled = false;

            // Draw radio field if needed
            if ((_property.propertyType == SerializedPropertyType.Boolean) && _useRadioToggle)
            {
                EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
            }
            else EditorGUI.PropertyField(_position, _property, _label);

            GUI.enabled = true;
        }
        #endregion

        #endregion

        #region Decorator Drawers
        /***************************
         *******   SECTION   *******
         **************************/

        /// <summary>
        /// Draw a section, a header-like usefull to order your editor.
        /// </summary>
        /// <param name="_position">Rect to draw the section.</param>
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        public static void Section(Rect _position, GUIContent _label, float _lineWidth)
        {
            // Get label size, plus line and section width
            Vector2 _labelSize = EditorGUIUtilityEnhanced.SectionLabelStyle.CalcSize(_label);
            float _sectionWidth = Mathf.Min(_position.width, _labelSize.x + (EditorGUIUtilityEnhanced.SpaceAroundSectionLabel * 2) + (_lineWidth * 2));
            _lineWidth = ((_sectionWidth - _labelSize.x) / 2f) - EditorGUIUtilityEnhanced.SpaceAroundSectionLabel;

            // Set position at the middle of the area
            _position.y += Mathf.Max(0, (_position.height - _labelSize.y) / 2f);

            // Draw horizontal line if enough space
            if (_lineWidth > 0)
            {
                Rect _lineRect = new Rect()
                {
                    x = _position.x + ((_position.width - _sectionWidth) / 2f),
                    y = _position.y + _labelSize.y / 2f,
                    height = 2f,
                    width = _lineWidth
                };

                // Set label x position
                _position.x = _lineRect.x + _lineWidth + EditorGUIUtilityEnhanced.SpaceAroundSectionLabel;

                // Draw lines around label
                EditorGUI.DrawRect(_lineRect, EditorGUIUtilityEnhanced.SectionLabelStyle.normal.textColor);
                _lineRect.x += _lineWidth + _labelSize.x + (EditorGUIUtilityEnhanced.SpaceAroundSectionLabel * 2);
                EditorGUI.DrawRect(_lineRect, EditorGUIUtilityEnhanced.SectionLabelStyle.normal.textColor);
            }
            // Set maximum available space surrounding the label
            else if (_position.width > _labelSize.x)
            {
                _position.x += (_position.width - _labelSize.x) / 2f;
            }

            // Draw label
            EditorGUI.LabelField(_position, _label, EditorGUIUtilityEnhanced.SectionLabelStyle);
        }


        /*******************************
         *****   HORIZONTAL LINE   *****
         ******************************/

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        /// <param name="_position">Rect to draw the line.</param>
        /// <param name="_width">Line width ; 0 is full width.</param>
        /// <param name="_color">Color of the line.</param>
        public static void HorizontalLine(Rect _position, float _width, Color _color)
        {
            // Specify line width if needed
            if ((_width > 0) && (_width < _position.width))
            {
                _position.x += (_position.width - _width) / 2f;
                _position.width = _width;
            }

            // Draw rect at position of specified color
            EditorGUI.DrawRect(_position, _color);
        }
        #endregion

        #endregion
    }
}
