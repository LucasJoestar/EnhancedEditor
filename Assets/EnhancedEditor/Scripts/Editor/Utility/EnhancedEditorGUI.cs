// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains a bunch of useful GUI-related methods for the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EnhancedEditorGUI
    {
        #region Property Drawers

        #region Asset Preview
        /// <summary>
        /// Draws a property field with its object reference
        /// asset preview within a foldout below it.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw with asset preview.</param>
        /// <param name="_previewSize">Size of the asset preview.</param>
        /// <param name="_isUnfolded">Asset preview foldout state.</param>
        /// <param name="_height">Total height of the drawn gui elements.</param>
        /// <returns>New asset preview foldout state.</returns>
        public static bool AssetPreviewField(Rect _position, SerializedProperty _property, float _previewSize, bool _isUnfolded, out float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            return AssetPreviewField(_position, _property, _label, _previewSize, _isUnfolded, out _height);
        }

        /// <summary>
        /// Draws a property field with its object reference
        /// asset preview within a foldout below it.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw with asset preview.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_previewSize">Size of the asset preview.</param>
        /// <param name="_isUnfolded">Asset preview foldout state.</param>
        /// <param name="_height">Total height of the drawn gui elements.</param>
        /// <returns>New asset preview foldout state.</returns>
        public static bool AssetPreviewField(Rect _position, SerializedProperty _property, GUIContent _label, float _previewSize, bool _isUnfolded, out float _height)
        {
            _height = _position.height;

            // Display informative box of property is not compatible.
            if (_property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(_position, _property, _label);

                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
                _position.height = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;

                _height += EditorGUIUtility.standardVerticalSpacing + EnhancedEditorGUIUtility.DefaultHelpBoxHeight;

                EditorGUI.HelpBox(_position, "Asset Preview attribute can only be used with object reference type field!", UnityEditor.MessageType.Error);
                return false;
            }

            // No object reference means no asset preview.
            if (!_property.objectReferenceValue)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return _isUnfolded;
            }

            _position.width -= EnhancedEditorGUIUtility.AssetPreviewFoldoutWidth;
            EditorGUI.PropertyField(_position, _property, _label);

            // Preview foldout.
            _position.x += _position.width + EnhancedEditorGUIUtility.AssetPreviewFoldoutWidth;
            _position.width = EnhancedEditorGUIUtility.AssetPreviewFoldoutWidth;

            _isUnfolded = EditorGUI.Foldout(_position, _isUnfolded, GUIContent.none);

            // Asset preview.
            if (_isUnfolded)
            {
                _position.Set
                (
                    _position.x - _position.width - _previewSize,
                    _position.y + _position.height + EditorGUIUtility.standardVerticalSpacing,
                    _previewSize,
                    _previewSize
                );

                _height += EditorGUIUtility.standardVerticalSpacing + _previewSize;

                // Catch preview null ref exception.
                try
                {
                    EditorGUI.DrawPreviewTexture(_position, AssetPreview.GetAssetPreview(_property.objectReferenceValue));
                }
                catch (NullReferenceException) { }
            }

            return _isUnfolded;
        }
        #endregion

        #region Max
        /// <summary>
        /// Restrains a property value so it does not exceed a maximum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void MaxField(Rect _position, SerializedProperty _property, float _maxValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_position, _property, _label, _maxValue);
        }

        /// <summary>
        /// Restrains a property value so it does not exceed a maximum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void MaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);

            // Restrain value when changed.
            if (EditorGUI.EndChangeCheck())
                EnhancedEditorGUIUtility.CeilValue(_property, _maxValue);
        }
        #endregion

        #region Min
        /// <summary>
        /// Restrains a property value so it does not go under a minimum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void MinField(Rect _position, SerializedProperty _property, float _minValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_position, _property, _label, _minValue);
        }

        /// <summary>
        /// Restrains a property value so it does not go under a maximum.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void MinField(Rect _position, SerializedProperty _property, GUIContent _label, float _minValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);

            // Restrain value when changed.
            if (EditorGUI.EndChangeCheck())
                EnhancedEditorGUIUtility.FloorValue(_property, _minValue);
        }
        #endregion

        #region Min Max
        /// <summary>
        /// Draws a property as a min-max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a min-max field.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(Rect _position, SerializedProperty _property, float _minLimit, float _maxLimit)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_position, _property, _label, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Draws a property as a min-max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a min-max field.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit)
        {
            switch (_property.propertyType)
            {
                // Vector2 slider.
                case SerializedPropertyType.Vector2:
                {
                    Vector2 _value = _property.vector2Value;
                    EditorGUI.BeginChangeCheck();

                    MinMaxField(_position, _label, ref _value.x, ref _value.y, _minLimit, _maxLimit);
                    if (EditorGUI.EndChangeCheck())
                        _property.vector2Value = _value;

                    break;
                }

                // Vector2Int slider.
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2 _value = _property.vector2IntValue;
                    EditorGUI.BeginChangeCheck();

                    MinMaxField(_position, _label, ref _value.x, ref _value.y, _minLimit, _maxLimit);
                    if (EditorGUI.EndChangeCheck())
                        _property.vector2IntValue = new Vector2Int((int)_value.x, (int)_value.y);

                    break;
                }

                // Error info box.
                default:
                    EditorGUI.HelpBox(_position, "Min Max fields can only be used with Vector2!", UnityEditor.MessageType.Error);
                    return;
            }
        }

        /// <summary>
        /// Draws a <see cref="Vector2"/> as a min-max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_value">Editing vector.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        /// <returns>New vector value.</returns>
        public static Vector2 MinMaxField(Rect _position, GUIContent _label, Vector2 _value, float _minLimit, float _maxLimit)
        {
            MinMaxField(_position, _label, ref _value.x, ref _value.y, _minLimit, _maxLimit);
            return _value;
        }

        /// <summary>
        /// Draws a <see cref="Vector2Int"/> as a min-max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_value">Editing vector.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        /// <returns>New vector value.</returns>
        public static Vector2Int MinMaxField(Rect _position, GUIContent _label, Vector2Int _value, int _minLimit, int _maxLimit)
        {
            Vector2 _newValue = _value;
            EditorGUI.BeginChangeCheck();

            MinMaxField(_position, _label, ref _newValue.x, ref _newValue.y, _minLimit, _maxLimit);

            if (EditorGUI.EndChangeCheck())
                _value.Set((int)_newValue.x, (int)_newValue.y);

            return _value;
        }

        /// <summary>
        /// Draws a min-max slider field.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_minValue">Editing minimum value.</param>
        /// <param name="_maxValue">Editing maximum value.</param>
        /// <param name="_minLimit">Minimum field allowed value.</param>
        /// <param name="_maxLimit">Maximum field allowed value.</param>
        public static void MinMaxField(Rect _position, GUIContent _label, ref float _minValue, ref float _maxValue, float _minLimit, float _maxLimit)
        {
            float _width = _position.xMax;
            if (_label != GUIContent.none)
            {
                // Property label.
                _position.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(_position, _label);

                _position.x = _position.xMax + EnhancedEditorGUIUtility.StandardHorizontalSeparator;
            }

            // Min value float field.
            _position.width = EditorGUIUtility.fieldWidth;

            float _newMinValue = EditorGUI.FloatField(_position, _minValue);
            if (_newMinValue != _minValue)
                _minValue = Mathf.Clamp(_newMinValue, _minLimit, _maxValue);

            // Min-Max slider.
            _position.x = _position.xMax + EnhancedEditorGUIUtility.StandardHorizontalSeparator;
            _position.width = (_width - _position.x)
                            - (_position.width + EnhancedEditorGUIUtility.StandardHorizontalSeparator);
           
            EditorGUI.MinMaxSlider(_position, ref _minValue, ref _maxValue, _minLimit, _maxLimit);

            // Max value float field.
            _position.x = _position.xMax + EnhancedEditorGUIUtility.StandardHorizontalSeparator;
            _position.width = EditorGUIUtility.fieldWidth;

            float _newMaxValue = EditorGUI.FloatField(_position, _maxValue);
            if (_newMaxValue != _maxValue)
                _maxValue = Mathf.Clamp(_newMaxValue, _minValue, _maxLimit);
        }
        #endregion

        #region Enhanced Slider
        public static void EnhancedSliderField(Rect _position, SerializedProperty _property, GUIContent _label, float _minValue, float _maxValue, float _precision, out float _height)
        {
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    // Global slider.
                    int _min = (int)_minValue;
                    int _max = (int)_maxValue;
                    int _range = (int)((_max - _min) / _precision);

                    _position.height = _height
                                     = EditorGUIUtility.singleLineHeight;

                    EditorGUI.IntSlider(_position, _property, _min, _max, _label);

                    // Precision slider.
                    _min = Mathf.Max(_min, _property.intValue - _range);
                    _max = Mathf.Min(_max, _property.intValue + _range);

                    float _spacing = _position.height + EditorGUIUtility.standardVerticalSpacing;
                    _position.y += _spacing;
                    _height += _spacing;

                    _position.xMin = _position.x + EditorGUIUtility.labelWidth;
                    EditorGUI.IntSlider(_position, _property, _min, _max, GUIContent.none);

                    break;
                }

                case SerializedPropertyType.Float:
                {
                    // Global slider.
                    float _range = (_maxValue - _minValue) / _precision;

                    _position.height = _height
                                     = EditorGUIUtility.singleLineHeight;

                    EditorGUI.Slider(_position, _property, _minValue, _maxValue, _label);

                    // Precision slider.
                    _minValue = Mathf.Max(_minValue, _property.floatValue - _range);
                    _maxValue = Mathf.Min(_maxValue, _property.floatValue + _range);

                    float _spacing = _position.height + EditorGUIUtility.standardVerticalSpacing;
                    _position.y += _spacing;
                    _height += _spacing;

                    _position.xMin = _position.x + EditorGUIUtility.labelWidth;
                    EditorGUI.Slider(_position, _property, _minValue, _maxValue, GUIContent.none);

                    break;
                }

                default:
                    _position.height = _height
                                     = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;

                    EditorGUI.HelpBox(_position, $"Enhanced Slider can only be used with Int and Float fields!", UnityEditor.MessageType.Error);
                    break;
            }
        }
        #endregion

        #region Picker
        /// <summary>
        /// Draws a picker for a <see cref="GameObject"/> or a <see cref="Component"/>,
        /// constraining its value to have some specific component(s).
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw with a picker.</param>
        /// <param name="_label">Label displayed before field.</param>
        /// <param name="_requiredTypes">Required components type.</param>
        public static void PickerField(Rect _position, SerializedProperty _property, GUIContent _label, Type[] _requiredTypes)
        {
            // Object field.
            Rect _rect = new Rect(_position.x, _position.y, _position.width - 28f, _position.height);
            Object _currentObject = _property.objectReferenceValue;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_rect, _property, _label);

            // Check if assigned object matches target types, or reset it.
            if (EditorGUI.EndChangeCheck())
            {
                _property.serializedObject.ApplyModifiedProperties();
                if (_property.objectReferenceValue != null)
                {
                    GameObject _object = (_property.objectReferenceValue is GameObject _gameObject)
                                        ? _gameObject
                                        : (_property.objectReferenceValue as Component).gameObject;

                    foreach (Type _type in _requiredTypes)
                    {
                        // Reverse to previous value if new one does not match.
                        if (!_object.GetComponent(_type))
                        {
                            SetPickerObject(_currentObject);
                            EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
                            break;
                        }
                    }
                }
            }

            // Specialized search panel.
            _rect.xMin = _rect.xMax + 3f;
            _rect.xMax = _position.xMax;

            bool _allowSceneObjects = !EditorUtility.IsPersistent(_property.serializedObject.targetObject);
            if (GUI.Button(_rect, EnhancedEditorGUIUtility.BrowseIcon))
            {
                GameObjectPicker.GetWindow(_requiredTypes, SetPickerObject, _allowSceneObjects);
            }


            // ----- Local Method -----

            void SetPickerObject(Object _object)
            {
                _property.objectReferenceValue = _object;
                _property.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        /// <param name="_requiredType">Required component type.</param>
        public static void PickerField(Rect _position, SerializedProperty _property, GUIContent _label, Type _requiredType)
        {
            PickerField(_position, _property, _label, new Type[] { _requiredType });
        }
        #endregion

        #region Property Field
        /// <summary>
        /// Draws a property field associated with a property,
        /// which value will be set whenever value is changed in the inspector.
        /// Usefull for performing additional operations when value is changed.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_propertyName">Name of the associated property to set (must be in the same script as the property field).</param>
        public static void PropertyField(Rect _position, SerializedProperty _property, string _propertyName)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PropertyField(_position, _property, _label, _propertyName);
        }

        /// <summary>
        /// Draws a property field associated with a property,
        /// which value will be set whenever value is changed in the inspector.
        /// Usefull for performing additional operations when value is changed.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_propertyName">Name of the associated property to set (must be in the same script as the property field).</param>
        public static void PropertyField(Rect _position, SerializedProperty _property, GUIContent _label, string _propertyName)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);

            if (EditorGUI.EndChangeCheck())
            {
                _property.serializedObject.ApplyModifiedProperties();
                EnhancedEditorGUIUtility.SetPropertyValue(_property, _propertyName);
            }
        }
        #endregion

        #region Progress Bar

        #region Editable
        private static bool isDraggingProgressBar = false;

        // -----------------------

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            EditableProgressBar(_position, _property, _label, _maxValueVariableName, _color);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, string _maxValueVariableName, Color _color)
        {
            if (TryGetProgressBarMaxValue(_position, _property, _maxValueVariableName, out float _maxValue))
                EditableProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            EditableProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        public static void EditableProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue, Color _color)
        {
            if (TryGetProgressBarValue(_position, _property, out float _value))
            {
                float _newValue = EditableProgressBar(_position, _label, _value, _maxValue, _color);
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
        }

        /// <summary>
        /// Draws an editable progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_value">Progress bar actual progression value.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <returns>New progress bar value.</returns>
        public static float EditableProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color)
        {
            ProgressBar(_position, _label, _value, _maxValue, _color);
            return MakeEditableProgressBar(_position, _value, _maxValue);
        }

        // -----------------------

        private static float MakeEditableProgressBar(Rect _position, float _value, float _maxValue)
        {
            Event _event = Event.current;
            int _controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Allow user to drag progress bar actual value.
            if (!isDraggingProgressBar)
            {
                _position.x += (_position.width * Mathf.Clamp(_value / _maxValue, 0, 1)) - 5;
                _position.width = 10;

                // Change cursor when at the edge of filled bar.
                EditorGUIUtility.AddCursorRect(_position, MouseCursor.ResizeHorizontal);
                if ((_event.GetTypeForControl(_controlID) == EventType.MouseDown) && _position.Contains(_event.mousePosition))
                {
                    isDraggingProgressBar = true;

                    GUIUtility.hotControl = _controlID;
                    _event.Use();
                }
            }
            else
            {
                EditorGUIUtility.AddCursorRect(_position, MouseCursor.ResizeHorizontal);
                if (_event.GetTypeForControl(_controlID) == EventType.MouseDrag)
                {
                    // Update progress value on drag.
                    GUIUtility.hotControl = _controlID;
                    _event.Use();

                    _value = (float)Math.Round(((_event.mousePosition.x - _position.x) / _position.width) * _maxValue, 2);
                    _value = Mathf.Clamp(_value, 0, _maxValue);
                }
                else if (_event.GetTypeForControl(_controlID) == EventType.MouseUp)
                {
                    // Stop dragging on mouse button release.
                    isDraggingProgressBar = false;

                    GUIUtility.hotControl = 0;
                    _event.Use();
                }
            }

            return _value;
        }
        #endregion

        #region Readonly
        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, string _maxValueVariableName, Color _color)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_position, _property, _label, _maxValueVariableName, _color);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable to use as progress bar maximum value
        /// (must be in the same script as the property).</param>
        /// <param name="_color">Progress bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, string _maxValueVariableName, Color _color)
        {
            if (TryGetProgressBarMaxValue(_position, _property, _maxValueVariableName, out float _maxValue))
                ProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_position, _property, _label, _maxValue, _color);
        }

        /// <summary>
        /// Draws a property as a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_property">Property to draw as a progress bar.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue, Color _color)
        {
            if (TryGetProgressBarValue(_position, _property, out float _value))
                ProgressBar(_position, _label, _value, _maxValue, _color);
        }

        /// <summary>
        /// Draws a progress bar.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_value">Progress bar actual progression value.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        public static void ProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color)
        {
            // First, draw filled bar portion.
            Rect _barRect = new Rect(_position.x, _position.y, _position.width * Mathf.Clamp(_value / _maxValue, 0, 1), _position.height);
            EditorGUI.DrawRect(_barRect, _color);

            // Then, draw empty portion (if not fully filled).
            if (_barRect.width < _position.width)
            {
                _barRect.x += _barRect.width;
                _barRect.width = _position.width - _barRect.width;

                EditorGUI.DrawRect(_barRect, SuperColor.SmokyBlack.Get());
            }

            _label.text = $"[{_label.text}]" +
                          $"{((_position.height > EditorGUIUtility.singleLineHeight * 2) ? "\n" : " ")}" +
                          $"{_value.ToString("### ### ### ##0.##")} / {_maxValue.ToString("### ### ### ##0.##")}";

            // Draws middle-centered label in shadow style.
            EditorGUI.DropShadowLabel(_position, _label, EnhancedEditorStyles.BoldCenteredLabel);
        }
        #endregion

        #region Utility
        private static bool TryGetProgressBarValue(Rect _position, SerializedProperty _property, out float _value)
        {
            if (!EnhancedEditorGUIUtility.GetPropertyValueAsSingle(_property, out _value))
            {
                string _message = $"Error on \"{_property.name}\"field; Progress bar cannot be used with \"{_property.propertyType}\" variable type!";
                EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);

                return false;
            }

            return true;
        }

        private static bool TryGetProgressBarMaxValue(Rect _position, SerializedProperty _property, string _maxValueVariableName, out float _maxValue)
        {
            if (!EnhancedEditorGUIUtility.GetFieldOrPropertyValueAsSingle(_property.serializedObject, _maxValueVariableName, out _maxValue))
            {
                string _message = $"Error on \"{_property.name}\" field; \"{_maxValueVariableName}\" value cannot be converted as single!";
                EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);

                return false;
            }

            return true;
        }
        #endregion

        #endregion

        #region Readonly
        /// <summary>
        /// Draws a readonly property field.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_useRadioToggle">If the property is a boolean, indicates if using a radio toggle or a standard one.</param>
        public static void ReadOnlyProperty(Rect _position, SerializedProperty _property, bool _useRadioToggle = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ReadOnlyProperty(_position, _property, _label, _useRadioToggle);
        }

        /// <summary>
        /// Draws a readonly property field.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_useRadioToggle">If the property is a boolean, indicates if using a radio toggle or a standard one.</param>
        public static void ReadOnlyProperty(Rect _position, SerializedProperty _property, GUIContent _label, bool _useRadioToggle = false)
        {
            EnhancedEditorGUIUtility.PushEnable(false);
            if ((_property.propertyType == SerializedPropertyType.Boolean) && _useRadioToggle)
            {
                EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
            }
            else
                EditorGUI.PropertyField(_position, _property, _label);

            EnhancedEditorGUIUtility.PopEnable();
        }
        #endregion

        #region Required
        /// <summary>
        /// Draws a required property field,
        /// showing an error help box when the property object reference value is null.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw and check validity.</param>
        /// <param name="_height">Total height of the drawn gui elements.</param>
        /// <returns>True if property is required and help box has been drawn, false otherwise.</returns>
        public static bool RequiredProperty(Rect _position, SerializedProperty _property, out float _height)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            return RequiredProperty(_position, _property, _label, out _height);
        }

        /// <summary>
        /// Draws a required property field,
        /// showing an error help box when the property object reference value is null.
        /// </summary>
        /// <param name="_position">Rect to draw field within.</param>
        /// <param name="_property">Property to draw and check validity.</param>
        /// <param name="_label">Label displayed before property.</param>
        /// <param name="_height">Total height of the drawn gui elements.</param>
        /// <returns>True if property is required and help box has been drawn, false otherwise.</returns>
        public static bool RequiredProperty(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            float _startHeight = _position.y;
            _position.height = EditorGUIUtility.singleLineHeight;

            bool _isRequired;
            if (_isRequired = DrawRequiredHelpBox(_position, _property, out _height))
            {
                _height += EditorGUIUtility.standardVerticalSpacing;
                _position.y += _height;

                _height += _position.height;
            }
            else
                _height = _position.height;

            EditorGUI.PropertyField(_position, _property, _label);

            // Context menu get reference shortcut.
            Event _event = Event.current;
            _position.Set(_position.x, _startHeight, _position.width, _height);
            if ((_event.type == EventType.ContextClick) && _position.Contains(_event.mousePosition))
            {
                GenericMenu _menu = new GenericMenu();
                _menu.AddItem(new GUIContent("Get Reference", "Get a reference of this property."), false, () =>
                {
                    if (EnhancedEditorGUIUtility.FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _type))
                    {
                        _property.objectReferenceValue = ((Component)_property.serializedObject.targetObject).GetComponent(_type.FieldType);
                        _property.serializedObject.ApplyModifiedProperties();
                    }
                });

                _menu.ShowAsContext();
                _event.Use();
            }

            return _isRequired;
        }

        // -----------------------

        public static bool DrawRequiredHelpBox(Rect _position, SerializedProperty _property, out float _height)
        {
            bool _isRequired = (_property.propertyType == SerializedPropertyType.ObjectReference) && !_property.objectReferenceValue;
            if (_isRequired)
            {
                _position = EditorGUI.IndentedRect(_position);

                _height = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;
                _position.height = _height;

                EditorGUI.HelpBox(_position, "Keep in mind to set a reference to this field!", UnityEditor.MessageType.Error);
            }
            else
                _height = 0;

            return _isRequired;
        }
        #endregion

        #endregion

        #region Decorator Drawers

        #region Horizontal Line
        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_width">Line width in percent (from 0 to 1).</param>
        /// <param name="_color">Line color.</param>
        public static void HorizontalLine(Rect _position, float _width, Color _color)
        {
            _position = EditorGUI.IndentedRect(_position);
            if (_width < 1)
            {
                _width *= Screen.width;
                _position.width = _width;
            }

            EditorGUI.DrawRect(_position, _color);
        }
        #endregion

        #region Section
        /// <summary>
        /// Draws a section, that is a header-like label usefull for ordering your editor.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        public static void Section(Rect _position, GUIContent _label, float _lineWidth)
        {
            _position = EditorGUI.IndentedRect(_position);

            Vector2 _labelSize = EditorStyles.boldLabel.CalcSize(_label);
            float _sectionWidth = Mathf.Min(_position.width, _labelSize.x + (EnhancedEditorGUIUtility.SpaceAroundSectionLabel * 2) + (_lineWidth * 2));

            // Draws label surrounding horizontal lines (if there is enough space).
            _lineWidth = ((_sectionWidth - _labelSize.x) / 2f) - EnhancedEditorGUIUtility.SpaceAroundSectionLabel;
            if (_lineWidth > 0)
            {
                float _heightShift = Mathf.Max(0, (_position.height - _labelSize.y) / 2f);
                Rect _lineRect = new Rect()
                {
                    x = _position.x + ((_position.width - _sectionWidth) / 2f),
                    y = _position.y + _heightShift + (_labelSize.y / 2f),
                    height = 2f,
                    width = _lineWidth
                };

                EditorGUI.DrawRect(_lineRect, EditorStyles.boldLabel.normal.textColor);
                _lineRect.x += _lineWidth + _labelSize.x + (EnhancedEditorGUIUtility.SpaceAroundSectionLabel * 2);
                EditorGUI.DrawRect(_lineRect, EditorStyles.boldLabel.normal.textColor);
            }

            // Section label.
            EditorGUI.LabelField(_position, _label, EnhancedEditorStyles.BoldCenteredLabel);
        }
        #endregion

        #endregion

        #region Multi Tags
        private static readonly GUIContent modifyTagNameGUI = new GUIContent("Modify Tag name", "Modify this tag name");
        private static readonly GUIContent modifyTagColorGUI = new GUIContent("Modify Tag color", "Modify this tag color");

        // -----------------------

        public static void TagField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            TagObject _tag;

            if (_property.hasMultipleDifferentValues)
            {
                _tag = TagObject.UnknownTag;
            }
            else
            {
                if (!EnhancedEditorGUIUtility.GetFieldOrPropertyValue(_property.serializedObject, _property.propertyPath, out object _value))
                {
                    throw new NullReferenceException($"{typeof(TagGroup)} property could not be found.");
                }

                Tag _t = (Tag)_value;
                MultiTagsUtility.GetTag(_t.ID, out _tag);
            }

            DoTagField(_position, _position, _tag, new TagObject[] { _tag }, null, OnChangeTag);

            // ----- Local Method ----- //

            void OnChangeTag(long _previousID, long _newID)
            {
                
            }
        }

        /// <summary>
        /// Draws a readonly field for an array of <see cref="TagObject"/>.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_tags">Tags to draw.</param>
        /// <returns>Total height used to draw this field. Use this to increment your layout position.</returns>
        public static float TagGroupField(Rect _position, TagObject[] _tags)
        {
            float _height = TagGroupField(_position, _tags, null, null, null);
            return _height;
        }

        public static float TagGroupField(Rect _position, SerializedProperty _property)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            return TagGroupField(_position, _property, _label);
        }

        /// <summary>
        /// Draws a readonly field for an array of <see cref="TagObject"/>.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_tags">Tags to draw.</param>
        /// <returns>Total height used to draw this field. Use this to increment your layout position.</returns>
        public static float TagGroupField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Got to retrieve tags on each draw as tags in common would not be displayed correctly when using undo with cached values
            // (can only record Unity objects). Using reflection is required to do so, and so as Linq to avoid indigest code blocks.
            if (!EnhancedEditorGUIUtility.GetFieldOrPropertyValues(_property.serializedObject, _property.propertyPath, out object[] _values))
            {
                throw new NullReferenceException($"{typeof(TagGroup)} property could not be found.");
            }

            // Only display tags in common from editing objects.
            TagGroup[] _tagGroups = Array.ConvertAll(_values, (v) => v as TagGroup);
            TagObject[] _tagObjects;

            if (_property.hasMultipleDifferentValues)
            {
                Tag[] _tags = _tagGroups.Select(t => t.Tags).Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToArray());
                _tagObjects = MultiTagsUtility.GetTags(_tags);
            }
            else
            {
                _tagObjects = MultiTagsUtility.GetTags(_tagGroups[0].Tags);
            }

            // Prefix label.
            if (_label != GUIContent.none)
            {
                EditorGUI.PrefixLabel(_position, _label);
                _position.xMin += EditorGUIUtility.labelWidth;
            }

            float _height = TagGroupField(_position, _tagObjects, OnAddTag, OnRemoveTag, OnChangeTag);
            return _height;

            // ----- Local Methods ----- //

            void OnAddTag(long _id)
            {
                Undo.RecordObjects(_property.serializedObject.targetObjects, "add tag");

                for (int _i = 0; _i < _tagGroups.Length; _i++)
                {
                    Object _target = _property.serializedObject.targetObjects[_i];
                    TagGroup _group = _tagGroups[_i];

                    _group.AddTag(new Tag(_id));
                    MultiTagsUtility.SortTagsByName(_group);

                    EditorUtility.SetDirty(_target);
                }
            }

            void OnRemoveTag(long _id)
            {
                Undo.RecordObjects(_property.serializedObject.targetObjects, "remove tag");

                for (int _i = 0; _i < _tagGroups.Length; _i++)
                {
                    Object _target = _property.serializedObject.targetObjects[_i];
                    TagGroup _group = _tagGroups[_i];

                    _group.RemoveTag(new Tag(_id));

                    EditorUtility.SetDirty(_target);
                }
            }

            void OnChangeTag(long _previousID, long _newID)
            {
                Undo.RecordObjects(_property.serializedObject.targetObjects, "change tag");

                for (int _i = 0; _i < _tagGroups.Length; _i++)
                {
                    Object _target = _property.serializedObject.targetObjects[_i];
                    TagGroup _group = _tagGroups[_i];

                    for (int _j = 0; _j < _group.Length; _j++)
                    {
                        if (_group[_j].ID == _previousID)
                        {
                            _group.Tags[_i].ID = _newID;
                            MultiTagsUtility.SortTagsByName(_group);

                            break;
                        }
                    }

                    EditorUtility.SetDirty(_target);
                }
            }
        }

        /// <summary>
        /// Draws a field for an array of <see cref="TagObject"/>.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_tags">Tags to draw.</param>
        /// <param name="_onAddTag">Callback when adding a new tag.
        /// Set this to null if you don't want users to be able to add any tag.</param>
        /// <param name="_onRemoveTag">Callback when removing a tag.
        /// Set this to null if you don't want users to be able to remove tags.</param>
        /// <param name="_onChangeTag">Callback when changing a tag value (previous id as first parameter, new as second).
        /// Set this to null if you don't want users to be able to change them.</param>
        /// <returns>Total height used to draw this field. Use this to increment your layout position.</returns>
        public static float TagGroupField(Rect _position, TagObject[] _tags, Action<long> _onAddTag, Action<long> _onRemoveTag, Action<long, long> _onChangeTag)
        {
            Event _event = Event.current;
            Rect _rect = new Rect(_position);

            // Draw tags.
            for (int _i = 0; _i < _tags.Length; _i++)
            {
                TagObject _tag = _tags[_i];

                _rect = DoTagField(_position, _rect, _tag, _tags, _onRemoveTag, _onChangeTag);
                _rect.x += _rect.width + 5f;
            }

            // Add tag button.
            bool _canAddTag = _onAddTag != null;
            if (_canAddTag)
            {
                _rect.y -= 1f;
                _rect.width = EnhancedEditorGUIUtility.OlMinusSize;

                if ((_rect.xMax > (_position.xMax - 5f)) && (_rect.x > _position.x))
                {
                    _rect.x = _position.x;
                    _rect.y += _rect.height + 5f;
                }

                if (GUI.Button(_rect, string.Empty, EnhancedEditorStyles.OlPlus))
                {
                    GenericMenu _menu = GetTagMenu(_tags, _onAddTag);
                    _menu.DropDown(_rect);
                }
            }

            float _height = (_rect.yMax - _position.y) + 1f;
            return _height;
        }

        /// <summary>
        /// Draws a field for a <see cref="TagObject"/>.
        /// </summary>
        /// <param name="_position">Total rect used to draw within.</param>
        /// <param name="_tagRect">Rect to draw this tag within.</param>
        /// <param name="_tag">Tag to draw.</param>
        /// <param name="_notDisplayedTags">Tags that will not be displayed in the select menu.</param>
        /// <param name="_onRemoveTag">Callback when removing the tag.
        /// Set this to null if you don't want users to be able to remove it.</param>
        /// <param name="_onChangeTag">Callback when changing tag value (previous id as first parameter, new as second).
        /// Set this to null if you don't want users to be able to change it.</param>
        /// <returns>Rect used to draw this field. Use this to increment your layout position.</returns>
        private static Rect DoTagField(Rect _position, Rect _tagRect, TagObject _tag, TagObject[] _notDisplayedTags, Action<long> _onRemoveTag, Action<long, long> _onChangeTag)
        {
            Event _event = Event.current;
            GUIContent _content = new GUIContent(_tag.Name);

            // Calcul rects.
            Rect _rect = new Rect(_tagRect)
            {
                width = EditorStyles.boldLabel.CalcSize(_content).x + 10f,
                height = EnhancedEditorGUIUtility.TagHeight
            };

            bool _canRemoveTag = _onRemoveTag != null;
            bool _canChangeTag = _onChangeTag != null;

            if (_canRemoveTag)
                _rect.width += EnhancedEditorGUIUtility.OlMinusSize;

            if ((_rect.xMax > (_position.xMax - 5f)) && (_rect.x > _position.x))
            {
                _rect.x = _position.x;
                _rect.y += _rect.height + 5f;
            }

            _tagRect = _rect;

            // Draw tag.
            EnhancedEditorGUIUtility.PushGUIColor(_tag.Color);
            GUI.Label(_rect, string.Empty, EnhancedEditorStyles.CNCountBadge);
            EnhancedEditorGUIUtility.PopGUIColor();

            _rect.xMin += 5f;
            GUI.Label(_rect, _content, EditorStyles.boldLabel);

            // Remove tag button.
            if (_canRemoveTag)
            {
                Rect _buttonRect = new Rect(_rect);
                _buttonRect.xMin = _buttonRect.xMax - 5f - EnhancedEditorGUIUtility.OlMinusSize;
                _buttonRect.y -= 1f;

                if (GUI.Button(_buttonRect, string.Empty, EnhancedEditorStyles.OlMinus))
                {
                    _onRemoveTag.Invoke(_tag.ID);
                }
            }

            // Context click & tag modification.
            if (_rect.Contains(_event.mousePosition))
            {
                if (_event.type == EventType.ContextClick)
                {
                    GenericMenu _menu = new GenericMenu();
                    _menu.AddItem(modifyTagNameGUI, false, () =>
                    {
                        ModifyTagNameWindow.GetWindow(_tag);
                    });

                    _menu.AddItem(modifyTagColorGUI, false, () =>
                    {
                        EnhancedEditorUtility.ColorPicker(_tag.Color, (Color _color) =>
                        {
                            MultiTagsUtility.SetTagColor(_tag.ID, _color);
                        });
                    });

                    _menu.DropDown(_rect);
                    _event.Use();
                }
                else if (_canChangeTag && (_event.type == EventType.MouseDown) && _event.button == 0)
                {
                    GenericMenu _menu = GetTagMenu(_notDisplayedTags, (long _id) =>
                    {
                        _onChangeTag?.Invoke(_tag.ID, _id);
                    });

                    _menu.DropDown(_rect);
                    _event.Use();
                }
            }

            return _tagRect;
        }

        // -----------------------

        /// <summary>
        /// Get context menu used to select a tag.
        /// <para/>
        /// Used to select another tag or add one to a group.
        /// </summary>
        /// <param name="_notDisplayedTags">Tags that will not be displayed in the menu.</param>
        /// <param name="_onSelectTag">Callback when selecting a tag.</param>
        /// <returns><see cref="GenericMenu"/> to select a tag. You can display it as you want.</returns>
        public static GenericMenu GetTagMenu(TagObject[] _notDisplayedTags, Action<long> _onSelectTag)
        {
            List<TagObject> _tags = new List<TagObject>(MultiTagsUtility.GetAllTags());
            foreach (var _tag in _notDisplayedTags)
                _tags.Remove(_tag);

            GenericMenu _menu = new GenericMenu();
            foreach (var _tag in _tags)
            {
                _menu.AddItem(new GUIContent(_tag.Name.Replace('_', '/')), false, () =>
                {
                    _onSelectTag?.Invoke(_tag.ID);
                });
            }

            _menu.AddSeparator(string.Empty);
            _menu.AddItem(new GUIContent("Create new Tag"), false, () =>
            {
                CreateTagWindow.GetWindow(_onSelectTag);
            });
            _menu.AddItem(new GUIContent("Open Multi-Tags Window"), false, () =>
            {
                MultiTagsWindow.GetWindow();
            });

            return _menu;
        }
        #endregion

        #region Others
        /// <summary>
        /// Draws a texture within a rect.
        /// Height is automatically adjusted.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_texture">Texture to draw.</param>
        /// <returns>Total height used to draw the texture.</returns>
        public static float Texture(Rect _position, Texture2D _texture)
        {
            float _ratio = _texture.height / (float)_texture.width;
            float _height = _position.height
                          = EditorGUIUtility.currentViewWidth * _ratio;

            GUI.Label(_position, _texture);
            return _height;
        }

        /// <summary>
        /// Draws a clickable label, redirecting to a specified url.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Displayed label redirecting to url.</param>
        /// <param name="_url">Link associated url.</param>
        public static void LinkLabel(Rect _position, GUIContent _label, string _url)
        {
            Color _color = _position.Contains(Event.current.mousePosition)
                                            ? Color.white
                                            : EnhancedEditorGUIUtility.GUILinkColor;

            EnhancedEditorGUIUtility.PushGUIColor(_color);
            EditorGUIUtility.AddCursorRect(_position, MouseCursor.Link);

            if (GUI.Button(_position, GUIContent.none, EditorStyles.label))
            {
                Application.OpenURL(_url);
            }

            UnderlinedLabel(_position, _label, EnhancedEditorStyles.BoldWordWrappedLabel);
            EnhancedEditorGUIUtility.PopGUIColor();
        }

        /// <summary>
        /// Draws an underlined label.
        /// </summary>
        /// <param name="_position">Rect to draw within.</param>
        /// <param name="_label">Label to display.</param>
        /// <param name="_style">Style used to draw label.</param>
        public static void UnderlinedLabel(Rect _position, GUIContent _label, GUIStyle _style)
        {
            _position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(_position, _label, _style);

            _position.y += _position.height;
            _position.height = 1f;
            _position.width = _style.CalcSize(_label).x;

            HorizontalLine(_position, 1f, SuperColor.Grey.Get());
        }
        #endregion
    }
}
