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

        #region Asset Preview
        /*********************************
         *******   ASSET PREVIEW   *******
         ********************************/

        /// <summary>
        /// Draw an property field with an asset preview below it.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to display and draw asset preview.</param>
        /// <param name="_foldout">The shown asset preview foldout state.</param>
        /// <returns>Returns new asset preview foldout state.</returns>
        public static bool AssetPreviewField(Rect _position, SerializedProperty _property, bool _foldout) => AssetPreviewField(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _foldout);

        /// <summary>
        /// Draw an property field with an asset preview below it.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to display and draw asset preview.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_foldout">The shown asset preview foldout state.</param>
        /// <returns>Returns new asset preview foldout state.</returns>
        public static bool AssetPreviewField(Rect _position, SerializedProperty _property, GUIContent _label, bool _foldout)
        {
            Texture2D _texture = EditorGUIUtilityEnhanced.GetAssetPreview(_property);

            // Display property field
            Rect _rect = new Rect()
            {
                x = _position.x,
                y = _position.y,
                width = _position.width - (_texture ? 25 : 0),
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.PropertyField(_rect, _property, _label);

            // If no texture, draw nothing
            if (!_texture)
            {
                // If property is not valid, display informative box
                if (_property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    _position.y += _rect.height + EditorGUIUtility.standardVerticalSpacing;
                    _position.height = EditorGUIUtilityEnhanced.DefaultHelpBoxHeight;

                    EditorGUI.HelpBox(_position, "Asset Preview attribute can only be used with object reference type fields !", MessageType.Error);
                }
                return false;
            }

            // Display foldout button next to property field
            _rect.x += _position.width - 5;
            _rect.width = _position.width - _rect.width;

            _foldout = EditorGUI.Foldout(_rect, _foldout, GUIContent.none);

            // If visible & assigned, display asset preview
            if (_foldout)
            {
                float _space = _rect.height + EditorGUIUtility.standardVerticalSpacing;
                float _aspect = _position.height - _space;

                _position.x += _position.width - 25 - _aspect;
                _position.y += _space;

                _position.width = _aspect;
                _position.height = _aspect;

                EditorGUI.DrawPreviewTexture(_position, _texture);
            }

            return _foldout;
        }
        #endregion

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
        public static void PropertyField(Rect _position, SerializedProperty _property, string _propertyName) => PropertyField(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _propertyName);

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
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color) => EditableProgressBar(_position, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _property, _maxValueVariableName, _color);

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
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color) => EditableProgressBar(_position, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _property, _maxValue, _color);

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
        public static void ProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color) => ProgressBar(_position, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _property, _maxValueVariableName, _color);

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
        public static void ProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color) => ProgressBar(_position, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _property, _maxValue, _color);

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

        #region Min Max
        /*****************************
         ********   MIN MAX   ********
         ****************************/

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_property">Editing property.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(Rect _position, SerializedProperty _property, float _minLimit, float _maxLimit) => MinMaxField(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_property">Editing property.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit)
        {
            switch (_property.propertyType)
            {
                // Draw Vector2 slider
                case SerializedPropertyType.Vector2:
                {
                    Vector2 _vector = _property.vector2Value;

                    EditorGUI.BeginChangeCheck();
                    MinMaxField(_position, _label, ref _vector.x, ref _vector.y, _minLimit, _maxLimit);

                    if (EditorGUI.EndChangeCheck())
                        _property.vector2Value = _vector;
                    break;
                }

                // Draw Vector2Int slider
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2 _vector = _property.vector2IntValue;

                    EditorGUI.BeginChangeCheck();
                    MinMaxField(_position, _label, ref _vector.x, ref _vector.y, _minLimit, _maxLimit);

                    if (EditorGUI.EndChangeCheck())
                        _property.vector2IntValue = new Vector2Int((int)_vector.x, (int)_vector.y);
                    break;
                }

                // Draw informative box
                default:
                    EditorGUI.HelpBox(_position, "Min Max fields can only be used with Vector2 !", MessageType.Error);
                    return;
            }
        }

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_vector">Vector used to store min & max values in.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(Rect _position, GUIContent _label, Vector2 _vector, float _minLimit, float _maxLimit) => MinMaxField(_position, _label, ref _vector.x, ref _vector.y, _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_vector">Vector used to store min & max values in.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(Rect _position, GUIContent _label, Vector2Int _vector, float _minLimit, float _maxLimit)
        {
            Vector2 _newVector = _vector;
            EditorGUI.BeginChangeCheck();
            MinMaxField(_position, _label, ref _newVector.x, ref _newVector.y, _minLimit, _maxLimit);

            if (EditorGUI.EndChangeCheck())
            {
                _vector.x = (int)_newVector.x;
                _vector.y = (int)_newVector.y;
            }
        }

        /// <summary>
        /// Draw a min max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_minValue">Editing minimum value.</param>
        /// <param name="_maxValue">Editing maximum value.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(Rect _position, GUIContent _label, ref float _minValue, ref float _maxValue, float _minLimit, float _maxLimit)
        {
            // Calcul rects
            Rect _labelRect = new Rect()
            {
                x = _position.x,
                y = _position.y,
                width = EditorGUIUtility.labelWidth,
                height = _position.height
            };

            float _fieldWidth = (_position.width - _labelRect.width) / 5f;

            Rect _fieldRect = new Rect()
            {
                x = _labelRect.xMax,
                y = _position.y,
                width = _fieldWidth - EditorGUIUtilityEnhanced.StandardHorizontalSeparator,
                height = _position.height
            };

            Rect _sliderRect = new Rect()
            {
                x = _fieldRect.xMax + EditorGUIUtilityEnhanced.StandardHorizontalSeparator,
                y = _position.y,
                width = _fieldWidth * 3,
                height = _position.height
            };

            // Draw label
            EditorGUI.LabelField(_labelRect, _label);

            // Draw min value field
            float _newMinValue = EditorGUI.FloatField(_fieldRect, _minValue);
            if (_newMinValue != _minValue)
                _minValue = Mathf.Clamp(_newMinValue, _minLimit, _maxValue);

            // Draw min max slider
            EditorGUI.MinMaxSlider(_sliderRect, ref _minValue, ref _maxValue, _minLimit, _maxLimit);

            // Draw max value field
            _fieldRect.x = _sliderRect.xMax + EditorGUIUtilityEnhanced.StandardHorizontalSeparator;

            float _newMaxValue = EditorGUI.FloatField(_fieldRect, _maxValue);
            if (_newMaxValue != _maxValue)
                _maxValue = Mathf.Clamp(_newMaxValue, _minValue, _maxLimit);
        }
        #endregion

        #region Max
        /*****************************
         **********   MAX   **********
         ****************************/

        /// <summary>
        /// Restrain a property value to a maximum one.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_property">Editing property.</param>
        /// <param name="_maxValue">Property max value.</param>
        public static void MaxField(Rect _position, SerializedProperty _property, float _maxValue) => MaxField(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _maxValue);

        /// <summary>
        /// Restrain a property value to a maximum one.
        /// </summary>
        /// <param name="_position">Rect to draw field.</param>
        /// <param name="_property">Editing property.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_maxValue">Property max value.</param>
        public static void MaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);

            if (EditorGUI.EndChangeCheck())
            {
                // Restrain value when changed
                switch (_property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        _property.intValue = (int)Mathf.Min(_property.intValue, _maxValue);
                        break;

                    case SerializedPropertyType.Float:
                        _property.floatValue = Mathf.Min(_property.floatValue, _maxValue);
                        break;

                    case SerializedPropertyType.Enum:
                        _property.enumValueIndex = (int)Mathf.Min(_property.enumValueIndex, _maxValue);
                        break;

                    default:
                        // Do nothing
                        break;
                }
            }
        }
        #endregion

        #region Required
        /****************************
         *******   REQUIRED   *******
         ***************************/

        /// <summary>
        /// Draw a required property field.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to draw and check validity.</param>
        public static void RequiredProperty(Rect _position, SerializedProperty _property) => RequiredProperty(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property));

        /// <summary>
        /// Draw a required property field.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to draw and check validity.</param>
        /// <param name="_label">Label to display before property.</param>
        public static void RequiredProperty(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            Rect _rect = new Rect(_position.x, _position.y, _position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(_rect, _property, _label);
            if (!EditorGUIUtilityEnhanced.IsPropertyRequired(_property)) return;

            _rect.y += _rect.height + EditorGUIUtility.standardVerticalSpacing;
            _rect.height = EditorGUIUtilityEnhanced.DefaultHelpBoxHeight;

            EditorGUI.HelpBox(_rect, "Keep in mind to set a reference to this field !", MessageType.Error);
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
        public static void ReadOnlyProperty(Rect _position, SerializedProperty _property) => ReadOnlyProperty(_position, _property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), false);

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
