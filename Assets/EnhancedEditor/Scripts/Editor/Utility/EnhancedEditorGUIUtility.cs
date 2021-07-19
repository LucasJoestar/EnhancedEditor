// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multiple GUI utility methods and fields related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EnhancedEditorGUIUtility
    {
        #region Global Members
        /// <summary>
        /// Width of the foldout displayed at the right of the asset preview fields.
        /// </summary>
        public const float AssetPreviewFoldoutWidth = 25f;

        /// <summary>
        /// Default height used to draw help boxes.
        /// </summary>
        public const float DefaultHelpBoxHeight = 38f;

        /// <summary>
        /// Space on each side of the Sections between label and horizontal lines (in pixels).
        /// </summary>
        public const float SpaceAroundSectionLabel = 5f;

        /// <summary>
        /// Separator width used for fields draw.
        /// </summary>
        public const float StandardHorizontalSeparator = 5f;

        /// <summary>
        /// Height used to draw tags. Equivalent to <see cref="EnhancedEditorStyles.CNCountBadge"/> height.
        /// </summary>
        public const float TagHeight = 16f;

        /// <summary>
        /// Size (height and width) of the icon drawn using <see cref="EnhancedEditorStyles.OlMinus"/>.
        /// Equivalent to the size of <see cref="EnhancedEditorStyles.OlPlus"/>.
        /// </summary>
        public const float OlMinusSize = 17f;

        public const float BoxLeftOffset = -13;
        public const float BoxRightOffset = 15;

        public static readonly Color GUIBackgroundProColor = new Color32(56, 56, 56, 255);
        public static readonly Color GUIBackgroundStandardColor = new Color32(194, 194, 194, 255);

        public static readonly Color GUIOddColor = new Color(.195f, .195f, .195f);
        public static readonly Color GUISelectedColor = new Color(0f, .5f, 1f, .25f);

        public static readonly Color GUILinkColor = new Color(.25f, .5f, 1f);
        #endregion

        #region GUI Contents
        /// <summary>
        /// Browse icon, looking like a magnifying glass.
        /// </summary>
        public static GUIContent BrowseIcon => GetContent(browseIcon);
        private static GUIContent browseIcon = null;

        private static bool areContentInitialized = false;

        // -----------------------

        private static GUIContent GetContent(GUIContent _content)
        {
            if (!areContentInitialized)
            {
                browseIcon = EditorGUIUtility.IconContent("BrowseIcon.png");

                areContentInitialized = true;
            }

            return _content;
        }
        #endregion

        #region GUI Shortcuts

        #region Color
        private static List<Color> guiColors = new List<Color>();
        private static List<Color> guiBackgroundColors = new List<Color>();
        private static List<Color> guiContentColors = new List<Color>();

        // -----------------------

        /// <summary>
        /// Push the current <see cref="GUI.color"/> to the buffer and set its new value.
        /// </summary>
        /// <param name="_color">New GUI color.</param>
        public static void PushGUIColor(Color _color)
        {
            guiColors.Add(GUI.color);
            GUI.color = _color;
        }

        /// <summary>
        /// Push the current <see cref="GUI.backgroundColor"/> to the buffer and set its new value.
        /// </summary>
        /// <param name="_color">New GUI background color.</param>
        public static void PushGUIBackgroundColor(Color _color)
        {
            guiBackgroundColors.Add(GUI.color);
            GUI.backgroundColor = _color;
        }

        /// <summary>
        /// Push the current <see cref="GUI.contentColor"/> to the buffer and set its new value.
        /// </summary>
        /// <param name="_color">New GUI content color.</param>
        public static void PushGUIContentColor(Color _color)
        {
            guiContentColors.Add(GUI.color);
            GUI.contentColor = _color;
        }

        /// <summary>
        /// Revert <see cref="GUI.color"/> to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopGUIColor()
        {
            if (guiColors.Count > 0)
            {
                int _index = guiColors.Count - 1;
                GUI.color = guiColors[_index];
                guiColors.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more GUI color than you are pushing!");
        }

        /// <summary>
        /// Revert <see cref="GUI.backgroundColor"/> to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopGUIBackgroundColor()
        {
            if (guiBackgroundColors.Count > 0)
            {
                int _index = guiBackgroundColors.Count - 1;
                GUI.backgroundColor = guiBackgroundColors[_index];
                guiBackgroundColors.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more GUI background color than you are pushing!");
        }

        /// <summary>
        /// Revert <see cref="GUI.contentColor"/> to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopGUIContentColor()
        {
            if (guiContentColors.Count > 0)
            {
                int _index = guiContentColors.Count - 1;
                GUI.contentColor = guiContentColors[_index];
                guiContentColors.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more GUI content color than you are pushing!");
        }

        /// <summary>
        /// Get the editor GUI background color.
        /// </summary>
        /// <returns>GUI Background color, depending if using pro skin or not.</returns>
        public static Color GetGUIBackgroundColor()
        {
            return EditorGUIUtility.isProSkin ? GUIBackgroundProColor : GUIBackgroundStandardColor;
        }
        #endregion

        #region Handles Color
        private static List<Color> handlesColors = new List<Color>();

        // -----------------------

        /// <summary>
        /// Push the current <see cref="Handles.color"/> to the buffer and set its new value.
        /// </summary>
        /// <param name="_color">New Handles color.</param>
        public static void PushHandlesColor(Color _color)
        {
            handlesColors.Add(Handles.color);
            Handles.color = _color;
        }

        /// <summary>
        /// Revert <see cref="GUI.color"/> to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopHandlesColor()
        {
            if (handlesColors.Count > 0)
            {
                int _index = handlesColors.Count - 1;
                Handles.color = handlesColors[_index];
                handlesColors.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more Handles color than you are pushing!");
        }
        #endregion

        #region Styles
        private static Dictionary<GUIStyle, List<TextAnchor>> styleAnchors = new Dictionary<GUIStyle, List<TextAnchor>>();

        // -----------------------

        /// <summary>
        /// Push the current style anchor to the buffer and set its new value.
        /// </summary>
        /// <param name="_style">Style to set anchor.</param>
        /// <param name="_anchor">New style anchor.</param>
        public static void PushAnchor(GUIStyle _style, TextAnchor _anchor)
        {
            if (!styleAnchors.ContainsKey(_style))
                styleAnchors.Add(_style, new List<TextAnchor>());

            styleAnchors[_style].Add(_style.alignment);
            _style.alignment = _anchor;
        }

        /// <summary>
        /// Revert a style anchor to the previous pushed-in-buffer one.
        /// </summary>
        /// <param name="_style">Style to revert anchor.</param>
        public static void PopAnchor(GUIStyle _style)
        {
            if (!styleAnchors.ContainsKey(_style))
            {
                Debug.LogError($"You are popping more anchor than you are pushing! ({_style.name})");
                return;
            }

            List<TextAnchor> _anchors = styleAnchors[_style];
            int _index = _anchors.Count - 1;

            _style.alignment = _anchors[_index];
            _anchors.RemoveAt(_index);

            if (_index == 0)
                styleAnchors.Remove(_style);
        }
        #endregion

        #region Enable State
        private static List<bool> guiEnableStates = new List<bool>();

        // -----------------------

        /// <summary>
        /// Push the current <see cref="GUI.enabled"/> state to the buffer and set its new value.
        /// </summary>
        /// <param name="_color">Should GUI be enabled?.</param>
        public static void PushEnable(bool _isEnable)
        {
            guiEnableStates.Add(GUI.enabled);
            GUI.enabled = _isEnable;
        }

        /// <summary>
        /// Revert <see cref="GUI.enabled"/> state to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopEnable()
        {
            if (guiEnableStates.Count > 0)
            {
                int _index = guiEnableStates.Count - 1;
                GUI.enabled = guiEnableStates[_index];
                guiEnableStates.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more GUI enabling than you are pushing!");
        }
        #endregion

        #region Label Width
        private static List<float> labelWidths = new List<float>();

        // -----------------------

        /// <summary>
        /// Push the current <see cref="EditorGUIUtility.labelWidth"/> to the buffer and set its new value.
        /// </summary>
        /// <param name="_width">New label width.</param>
        public static void PushLabelWidth(float _width)
        {
            labelWidths.Add(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = _width;
        }

        /// <summary>
        /// Revert <see cref="EditorGUIUtility.labelWidth"/> to the previous pushed-in-buffer value.
        /// </summary>
        public static void PopLabelWidth()
        {
            if (labelWidths.Count > 0)
            {
                int _index = labelWidths.Count - 1;
                EditorGUIUtility.labelWidth = labelWidths[_index];
                labelWidths.RemoveAt(_index);
            }
            else
                Debug.LogError("You are popping more label width than you are pushing!");
        }
        #endregion

        #endregion

        #region GUIContent
        /// <summary>
        /// Get appropriated GUIContent label from a serialized property.
        /// </summary>
        /// <param name="_property">Property to get label from.</param>
        /// <returns>Label associated with the property.</returns>
        public static GUIContent GetPropertyLabel(SerializedProperty _property)
        {
            return new GUIContent(ObjectNames.NicifyVariableName(_property.name), _property.tooltip);
        }
        #endregion

        #region Serialized Property

        #region Ceil / Floor Value
        /// <summary>
        /// Ceils a <see cref="SerializedProperty"/> value.
        /// </summary>
        /// <param name="_property">Property to ceil value.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void CeilValue(SerializedProperty _property, float _maxValue)
        {
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

                // Do nothing.
                default:
                    break;
            }
        }

        /// <summary>
        /// Floor a <see cref="SerializedProperty"/> value.
        /// </summary>
        /// <param name="_property">Property to floor value.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void FloorValue(SerializedProperty _property, float _minValue)
        {
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    _property.intValue = (int)Mathf.Max(_property.intValue, _minValue);
                    break;

                case SerializedPropertyType.Float:
                    _property.floatValue = Mathf.Max(_property.floatValue, _minValue);
                    break;

                case SerializedPropertyType.Enum:
                    _property.enumValueIndex = (int)Mathf.Max(_property.enumValueIndex, _minValue);
                    break;

                // Do nothing.
                default:
                    break;
            }
        }
        #endregion

        #region Get Single Value
        /// <summary>
        /// Get a property value as single.
        /// </summary>
        /// <param name="_property">Property to get value from.</param>
        /// <param name="_value">Property value (0 if property type is not compatible).</param>
        /// <returns>False if property value cannot be converted as single, true otherwise.</returns>
        public static bool GetPropertyValueAsSingle(SerializedProperty _property, out float _value)
        {
            // Get property value if can be converted as single.
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Enum:
                    _value = _property.enumValueIndex;
                    break;

                case SerializedPropertyType.Integer:
                    _value = _property.intValue;
                    break;

                case SerializedPropertyType.Float:
                    _value = _property.floatValue;
                    break;

                // Not matching type.
                default:
                    _value = 0;
                    return false;
            }

            return true;
        }
        #endregion

        #endregion

        #region Conditions
        private static readonly BindingFlags getMemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
                                                            | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.InvokeMethod;

        // -----------------------

        /// <summary>
        /// Get the <see cref="MemberInfo"/> to use as condition from a given type.
        /// <para/>
        /// Note that for a member to be used as a condition, it must be or a boolean
        /// (whether a field or a proeprty), or a method returning a boolean without any parameter.
        /// </summary>
        /// <param name="_type">Type to get member from.</param>
        /// <param name="_memberName">Name of the member to find.</param>
        /// <param name="_member">Matching found member in type.</param>
        /// <returns>True if successfully found matching member, false otherwise.</returns>
        public static bool GetConditionMember(Type _type, string _memberName, out MemberInfo _member)
        {
            MemberInfo[] _members = _type.GetMember(_memberName, getMemberFlags);

            // Get matching member of boolean return type.
            for (int _i = 0; _i < _members.Length; _i++)
            {
                _member = _members[_i];
                switch (_member.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo _field = (FieldInfo)_member;
                        if (_field.FieldType == typeof(bool))
                        {
                            return true;
                        }
                        break;

                    case MemberTypes.Method:
                        MethodInfo _method = (MethodInfo)_member;
                        if ((_method.ReturnType == typeof(bool)) && (_method.GetParameters().Length == 0))
                        {
                            return true;
                        }
                        break;

                    case MemberTypes.Property:
                        PropertyInfo _property = (PropertyInfo)_member;
                        if (_property.PropertyType == typeof(bool))
                        {
                            return true;
                        }
                        break;

                    default:
                        break;
                }
            }

            _member = null;
            return false;
        }

        /// <summary>
        /// Get a given condition is fulfilled.
        /// Should be used with <see cref="GetConditionMember(Type, string, out MemberInfo, out MemberTypes)"/>.
        /// </summary>
        /// <param name="_member">Member to check condition.</param>
        /// <param name="_target">Type object to get member value.</param>
        /// <param name="_condition">Condition type to use.</param>
        /// <returns>True if the condition is fulfilled, false otherwise.</returns>
        public static bool IsConditionFulfilled(MemberInfo _member, Object _target, ConditionType _condition)
        {
            bool _isValid = false;
            if (_member is FieldInfo _field)
            {
                _isValid = (bool)_field.GetValue(_target);
            }
            else if (_member is MethodInfo _method)
            {
                _isValid = (bool)_method.Invoke(_target, null);
            }
            else if (_member is PropertyInfo _property)
            {
                _isValid = (bool)_property.GetValue(_target);
            }

            // Interpret condition validation.
            switch (_condition)
            {
                case ConditionType.True:
                    return _isValid;

                case ConditionType.False:
                    return !_isValid;

                default:
                    return _isValid;
            }
        }
        #endregion

        #region Reflection

        #region Get Value
        /// <summary>
        /// Get a <see cref="SerializedObject"/> field or property value as single.
        /// </summary>
        /// <param name="_object">Object to get value from.</param>
        /// <param name="_memberName">Name of the member to get value (must be a field or a property).</param>
        /// <param name="_value">Member value.</param>
        /// <returns>False if member value cannot be converted as single, true otherwise.</returns>
        public static bool GetFieldOrPropertyValueAsSingle(SerializedObject _object, string _memberName, out float _value)
        {
            if (GetFieldOrPropertyValue(_object, _memberName, out object _memberValue))
            {
                // Catch non single value exception.
                try
                {
                    _value = Convert.ToSingle(_memberValue);
                    return true;
                }
                catch (Exception _exception)
                {
                    if (!(_exception is FormatException) && !(_exception is InvalidCastException))
                        throw _exception;
                }
            }

            _value = 0;
            return false;
        }

        /// <summary>
        /// Get a <see cref="SerializedObject"/> field or property value.
        /// </summary>
        /// <param name="_object">Object to get value from.</param>
        /// <param name="_memberName">Name of the member to get value (must be a field or a property).</param>
        /// <param name="_value">Member value.</param>
        /// <returns>True is member was found, false otherwise.</returns>
        public static bool GetFieldOrPropertyValue(SerializedObject _object, string _memberName, out object _value)
        {
            // Field value.
            if (FindSerializedObjectField(_object, _memberName, out FieldInfo _field))
            {
                _value = _field.GetValue(_object.targetObject);
                return true;
            }

            // Property value.
            if (FindSerializedObjectProperty(_object, _memberName, out PropertyInfo _property))
            {
                _value = _property.GetValue(_object.targetObject);
                return true;
            }

            _value = 0;
            return false;
        }

        /// <summary>
        /// Get a <see cref="SerializedObject"/> field or property values.
        /// </summary>
        /// <param name="_object">Object to get values from.</param>
        /// <param name="_memberName">Name of the member to get values (must be a field or a property).</param>
        /// <param name="_values">Member values.</param>
        /// <returns>True is member was found, false otherwise.</returns>
        public static bool GetFieldOrPropertyValues(SerializedObject _object, string _memberName, out object[] _values)
        {
            // Field value.
            if (FindSerializedObjectField(_object, _memberName, out FieldInfo _field))
            {
                _values = new object[_object.targetObjects.Length];
                for (int _i = 0; _i < _values.Length; _i++)
                {
                    Object _target = _object.targetObjects[_i];
                    _values[_i] = _field.GetValue(_target);
                }
                
                return true;
            }

            // Property value.
            if (FindSerializedObjectProperty(_object, _memberName, out PropertyInfo _property))
            {
                _values = new object[_object.targetObjects.Length];
                for (int _i = 0; _i < _values.Length; _i++)
                {
                    Object _target = _object.targetObjects[_i];
                    _values[_i] = _property.GetValue(_target);
                }

                return true;
            }

            _values = null;
            return false;
        }
        #endregion

        #region Find Member
        /// <summary>
        /// Retrieves a field from a given serialized object.
        /// </summary>
        /// <param name="_object">Object to get field from.</param>
        /// <param name="_fieldPath">Path of the field to find.</param>
        /// <param name="_field">Found <see cref="FieldInfo"/></param>
        /// <returns>True if successfully found <see cref="FieldInfo"/>, false otherwise.</returns>
        public static bool FindSerializedObjectField(SerializedObject _object, string _fieldPath, out FieldInfo _field)
        {
            Type _type = _object.targetObject.GetType();
            _field = null;

            string[] _fields = _fieldPath.Split('.');
            for (int _i = 0; _i < _fields.Length; _i++)
            {
                _field = _type.GetField(_fields[_i],
                              BindingFlags.GetField | BindingFlags.Instance |
                              BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static);

                if (_field == null)
                    return false;

                // there are only two container field type that can be serialized:
                // Array and List<T>
                if (_field.FieldType.IsArray)
                {
                    _type = _field.FieldType.GetElementType();
                    _i += 2;
                    continue;
                }

                if (_field.FieldType.IsGenericType)
                {
                    _type = _field.FieldType.GetGenericArguments()[0];
                    _i += 2;
                    continue;
                }

                _type = _field.FieldType;
            }

            return true;
        }

        /// <summary>
        /// Retrieves a field from a given serialized object.
        /// </summary>
        /// <param name="_object">Object to get field from.</param>
        /// <param name="_propertyName">Name of the property to find.</param>
        /// <param name="_property">Found <see cref="PropertyInfo"/>.</param>
        /// <returns>True if successfully found <see cref="PropertyInfo"/>, false otherwise.</returns>
        public static bool FindSerializedObjectProperty(SerializedObject _object, string _propertyName, out PropertyInfo _property)
        {
            Type _type = _object.targetObject.GetType();
            _property = null;

            while (_type != null)
            {
                _property = _type.GetProperty(_propertyName,
                              BindingFlags.GetProperty | BindingFlags.Instance |
                              BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static);

                if (_property != null)
                    return true;

                _type = _type.BaseType;
            }

            return false;
        }
        #endregion

        #region Type
        /// <summary>
        /// Get the object type of a given property.
        /// </summary>
        /// <param name="_property">Property to get type.</param>
        /// <returns>Property object type.</returns>
        public static Type GetPropertyType(SerializedProperty _property)
        {
            if (FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _field))
            {
                return GetFieldInfoType(_field);
            }

            return null;
        }

        /// <summary>
        /// Get the real type of a given <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="_field">Field to get type from.</param>
        /// <returns>Field related type.</returns>
        public static Type GetFieldInfoType(FieldInfo _field)
        {
            Type _type = _field.FieldType;
            if (_type.IsArray)
            {
                _type = _type.GetElementType();
            }
            else if (_type.IsGenericType)
            {
                _type = _type.GetGenericArguments()[0];
            }

            return _type;
        }
        #endregion

        #region Set Property Value
        /// <summary>
        /// Set the value of a property (not a field) from a serialized object.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get value used
        /// to set property value.</param>
        /// <param name="_propertyName">Name of the property to set.</param>
        public static void SetPropertyValue(SerializedProperty _property, string _propertyName)
        {
            if (GetFieldOrPropertyValue(_property.serializedObject, _property.name, out object _value))
                SetPropertyValue(_property.serializedObject, _propertyName, _value);
        }

        /// <summary>
        /// Set the value of a property (not a field) from a serialized object.
        /// </summary>
        /// <param name="_object"><see cref="SerializedObject"/> to set property value.</param>
        /// <param name="_propertyName">Name of the property to set.</param>
        /// <param name="_value">Property value.</param>
        public static void SetPropertyValue(SerializedObject _object, string _propertyName, object _value)
        {
            if (FindSerializedObjectProperty(_object, _propertyName, out PropertyInfo _propertyInfo))
            {
                foreach (Object _target in _object.targetObjects)
                {
                    // Catch mismatching type exception.
                    try
                    {
                        _propertyInfo.SetValue(_target, _value);
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogWarning($"Value \"{_value}\" could not be assigned to property \"{_propertyName}\" on \"{_target.GetType()}\" script from \"{_target.name}\".");
                    }
                }

            }
            else
                Debug.LogWarning($"Property \"{_propertyName}\" could not be found in the \"{_object.targetObject.GetType()}\" script from \"{_object.targetObject.name}\".");
        }
        #endregion

        #endregion

        #region GUI Utility
        /// <summary>
        /// Repaints all editors of a given <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_object">Object to repaint associated editor(s).</param>
        public static void Repaint(SerializedObject _object)
        {
            foreach (UnityEditor.Editor _editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (_editor.serializedObject == _object)
                    _editor.Repaint();
            }
        }
        #endregion
    }
}
