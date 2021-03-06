﻿using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Auto laid out version of <see cref="EditorGUIEnhanced"/>.
    /// </summary>
    public static class EditorGUILayoutEnhanced
    {
        #region Property Drawers

        #region Asset Preview
        /*********************************
         *******   ASSET PREVIEW   *******
         ********************************/

        /// <summary>
        /// Draw an property field with an asset preview below it.
        /// </summary>
        /// <param name="_property">Property to display and draw asset preview.</param>
        /// <param name="_foldout">The shown asset preview foldout state.</param>
        /// <param name="_previewAspect">Aspect of the displayed asset preview.</param>
        /// <returns>Returns new asset preview foldout state.</returns>
        public static bool AssetPreviewField(SerializedProperty _property, bool _foldout, float _previewAspect) => AssetPreviewField(_property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _foldout, _previewAspect);

        /// <summary>
        /// Draw an property field with an asset preview below it.
        /// </summary>
        /// <param name="_property">Property to display and draw asset preview.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_foldout">The shown asset preview foldout state.</param>
        /// <param name="_previewAspect">Aspect of the displayed asset preview.</param>
        /// <returns>Returns new asset preview foldout state.</returns>
        public static bool AssetPreviewField(SerializedProperty _property, GUIContent _label, bool _foldout, float _previewAspect)
        {
            Rect _rect = EditorGUILayout.GetControlRect(true, EditorGUIUtilityEnhanced.GetAssetPreviewPropertyHeight(_property, _foldout, _previewAspect));

            return EditorGUIEnhanced.AssetPreviewField(_rect, _property, _label, _foldout);
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
        /// <param name="_property">Serialized property to edit.</param>
        /// <param name="_propertyName">Name of the associated property (must be in the same script as the property field).</param>
        public static void PropertyField(SerializedProperty _property, string _propertyName) => PropertyField(_property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _propertyName);

        /// <summary>
        /// Draw a serialized property field associated with another property ;
        /// when setting field value, associated property will be set automatically.
        /// Usefull for clamping value or calling event on inspector edit.
        /// </summary>
        /// <param name="_property">Serialized property to edit.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_propertyName">Name of the associated property (must be in the same script as the property field).</param>
        public static void PropertyField(SerializedProperty _property, GUIContent _label, string _propertyName)
        {
            // Get rect
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            // Draw field
            EditorGUIEnhanced.PropertyField(_position, _property, _label, _propertyName);
        }
        #endregion

        #region Progress Bar
        /********************************
         *******   PROGRESS BAR   *******
         *******************************/

        /********** EDITABLE **********/

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, string _maxValueVariableName, Color _color, float _height) => EditorGUIEnhanced.EditableProgressBar(EditorGUILayout.GetControlRect(false, _height), _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void EditableProgressBar(GUIContent _label, SerializedProperty _property, string _maxValueVariableName, Color _color, float _height) => EditorGUIEnhanced.EditableProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void EditableProgressBar(SerializedProperty _property, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.EditableProgressBar(EditorGUILayout.GetControlRect(false, _height), _property, _maxValue, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void EditableProgressBar(GUIContent _label, SerializedProperty _property, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.EditableProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _property, _maxValue, _color);

        /// <summary>
        /// Draw an editable progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_value">Bar progression value.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <returns>Returns new progress bar value.</returns>
        public static float EditableProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.EditableProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _value, _maxValue, _color);

        /********** READ ONLY **********/

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, string _maxValueVariableName, Color _color, float _height) => EditorGUIEnhanced.ProgressBar(EditorGUILayout.GetControlRect(false, _height), _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValueVariableName">Name of the variable used as maximum value (must be in the same script as the property).</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void ProgressBar(GUIContent _label, SerializedProperty _property, string _maxValueVariableName, Color _color, float _height) => EditorGUIEnhanced.ProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _property, _maxValueVariableName, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void ProgressBar(SerializedProperty _property, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.ProgressBar(EditorGUILayout.GetControlRect(false, _height), _property, _maxValue, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_property">Property used as value in bar progression.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void ProgressBar(GUIContent _label, SerializedProperty _property, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.ProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _property, _maxValue, _color);

        /// <summary>
        /// Draw a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed in the middle of the bar.</param>
        /// <param name="_value">Bar progression value.</param>
        /// <param name="_maxValue">Bar maximum value.</param>
        /// <param name="_color">Bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        public static void ProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, float _height) => EditorGUIEnhanced.ProgressBar(EditorGUILayout.GetControlRect(false, _height), _label, _value, _maxValue, _color);
        #endregion

        #region Min Max
        /*****************************
         ********   MIN MAX   ********
         ****************************/

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_property">Editing property.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(SerializedProperty _property, float _minLimit, float _maxLimit) => EditorGUIEnhanced.MinMaxField(EditorGUILayout.GetControlRect(false), _property, _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_property">Editing property.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit) => EditorGUIEnhanced.MinMaxField(EditorGUILayout.GetControlRect(false), _property, _label, _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_vector">Vector used to store min & max values in.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(GUIContent _label, Vector2 _vector, float _minLimit, float _maxLimit) => EditorGUIEnhanced.MinMaxField(EditorGUILayout.GetControlRect(false), _label, _vector, _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field for a vector2.
        /// </summary>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_vector">Vector used to store min & max values in.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(GUIContent _label, Vector2Int _vector, float _minLimit, float _maxLimit) => EditorGUIEnhanced.MinMaxField(EditorGUILayout.GetControlRect(false), _label, _vector, _minLimit, _maxLimit);

        /// <summary>
        /// Draw a min max slider field.
        /// </summary>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_minValue">Editing minimum value.</param>
        /// <param name="_maxValue">Editing maximum value.</param>
        /// <param name="_minLimit">Min allowed limit value.</param>
        /// <param name="_maxLimit">Max allowed limit value.</param>
        public static void MinMaxField(GUIContent _label, ref float _minValue, ref float _maxValue, float _minLimit, float _maxLimit) => EditorGUIEnhanced.MinMaxField(EditorGUILayout.GetControlRect(false), _label, ref _minValue, ref _maxValue, _minLimit, _maxLimit);
        #endregion

        #region Max
        /*****************************
         **********   MAX   **********
         ****************************/

        /// <summary>
        /// Restrain a property value to a maximum one.
        /// </summary>
        /// <param name="_property">Editing property.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_maxValue">Property max value.</param>
        public static void MaxField(SerializedProperty _property, float _maxValue) => MaxField(_property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), _maxValue);

        /// <summary>
        /// Restrain a property value to a maximum one.
        /// </summary>
        /// <param name="_property">Editing property.</param>
        /// <param name="_label">Displayed label.</param>
        /// <param name="_maxValue">Property max value.</param>
        public static void MaxField(SerializedProperty _property, GUIContent _label, float _maxValue)
        {
            Rect _rect = EditorGUILayout.GetControlRect(true);

            EditorGUIEnhanced.MaxField(_rect, _property, _label, _maxValue);
        }
        #endregion

        #region Required
        /****************************
         *******   REQUIRED   *******
         ***************************/

        /// <summary>
        /// Draw a required property field.
        /// </summary>
        /// <param name="_property">Property to draw and check validity.</param>
        public static void RequiredProperty(SerializedProperty _property) => RequiredProperty(_property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property));

        /// <summary>
        /// Draw a required property field.
        /// </summary>
        /// <param name="_property">Property to draw and check validity.</param>
        /// <param name="_label">Label to display before property.</param>
        public static void RequiredProperty(SerializedProperty _property, GUIContent _label)
        {
            Rect _rect = EditorGUILayout.GetControlRect(true, EditorGUIUtilityEnhanced.GetRequiredPropertyHeight(_property));

            EditorGUIEnhanced.RequiredProperty(_rect, _property, _label);
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
        public static void ReadOnlyProperty(SerializedProperty _property) => ReadOnlyProperty(_property, EditorGUIUtilityEnhanced.GetPropertyLabel(_property), false);

        /// <summary>
        /// Draw a readonly property field.
        /// </summary>
        /// <param name="_position">Rect to draw field in.</param>
        /// <param name="_property">Property to display.</param>
        /// <param name="_label">Label to display before property.</param>
        /// <param name="_useRadioToggle">If property is boolean type, indicates if using radio toggle or standard one.</param>
        public static void ReadOnlyProperty(SerializedProperty _property, GUIContent _label, bool _useRadioToggle = false)
        {
            // Get field rect
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            // Draw it
            EditorGUIEnhanced.ReadOnlyProperty(_position, _property, _label, _useRadioToggle);
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
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        public static void Section(GUIContent _label, float _lineWidth) => Section(_label, _lineWidth, SectionAttribute.DefaultHeightSpace);

        /// <summary>
        /// Draw a section, a header-like usefull to order your editor.
        /// </summary>
        /// <param name="_label">Label to display.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        /// <param name="_heightSpace">Space on top and bottom of the section (in pixels).</param>
        public static void Section(GUIContent _label, float _lineWidth, float _heightSpace)
        {
            // Get section rect
            Rect _position = EditorGUILayout.GetControlRect(true, (EditorGUIUtility.singleLineHeight + _heightSpace) * 2);

            // Draw it
            EditorGUIEnhanced.Section(_position, _label, _lineWidth);
        }


        /*******************************
         *****   HORIZONTAL LINE   *****
         ******************************/

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        /// <param name="_color">Color of the line.</param>
        public static void HorizontalLine(Color _color) => HorizontalLine(HorizontalLineAttribute.DefaultHeight, 0, _color);

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        /// <param name="_height">Line height.</param>
        /// <param name="_color">Color of the line.</param>
        public static void HorizontalLine(float _height, Color _color) => HorizontalLine(_height, 0, _color);

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        /// <param name="_height">Line height.</param>
        /// <param name="_width">Line width ; 0 is full width.</param>
        /// <param name="_color">Color of the line.</param>
        public static void HorizontalLine(float _height, float _width, Color _color)
        {
            // Get line rect
            Rect _position = EditorGUILayout.GetControlRect(false, _height + EditorGUIUtility.singleLineHeight);

            _position.y += (EditorGUIUtility.singleLineHeight / 2f) - 1;
            _position.height = _height;

            // Draw line
            EditorGUIEnhanced.HorizontalLine(_position, _width, _color);
        }
        #endregion
    }
}
