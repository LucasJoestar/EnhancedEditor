using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multiple utility methods and properties related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EditorGUIUtilityEnhanced
    {
        #region Methods

        #region Serialized Properties Value
        /*************************************
         ***   SERIALIZED PROPERTY VALUE   ***
         ************************************/

        /// <summary>
        /// Get the single value of a property.
        /// </summary>
        /// <param name="_property">Property to get value from.</param>
        /// <param name="_value">Property value (0 if property type is not compatible).</param>
        /// <returns>Returns false if single value could not be get from property, true otherwise.</returns>
        public static bool GetPropertyValueAsSingle(SerializedProperty _property, out float _value)
        {
            // Try to get property value
            // If property type is not compatible with single, return false
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

                default:
                    // Not matching type
                    _value = 0;
                    return false;
            }

            // If get property value correctly, return true
            return true;
        }
        #endregion

        #region Reflection
        /****************************************
         ***   SERIALIZED OBJECT REFLECTION   ***
         ***************************************/

        /// <summary>
        /// Get single type value of a field or a property from a serialized object.
        /// </summary>
        /// <param name="_object">Serialized object to get value from.</param>
        /// <param name="_memberName">Name of the member to get value (field or property).</param>
        /// <param name="_value">Get member value.</param>
        /// <returns>Returns false if member value cannot be converted to single, true otherwise.</returns>
        public static bool GetSerializedObjectFieldOrPropertyValueAsSingle(SerializedObject _object, string _memberName, out float _value)
        {
            // Get member value
            object _memberValue = GetSerializedObjectFieldOrPropertyValue(_object, _memberName);

            if (_memberName != null)
            {
                // Try to convert indicated maximum variable value to single
                try
                {
                    _value = Convert.ToSingle(_memberValue);
                    return true;
                }
                // If wrong type, return false
                catch (Exception _exception)
                {
                    if (!(_exception is FormatException) && !(_exception is InvalidCastException)) throw;
                }
            }

            // Set value to zero and return false
            _value = 0;
            return false;
        }

        /// <summary>
        /// Get the value of a field or a property from a serialized object.
        /// </summary>
        /// <param name="_object">Serialized object to get value from.</param>
        /// <param name="_memberName">Name of the member to get value (field or property).</param>
        /// <returns>Returns field or property value from given name or null if nothing is found.</returns>
        public static object GetSerializedObjectFieldOrPropertyValue(SerializedObject _object, string _memberName)
        {
            // Try to find field
            FieldInfo _field = FindSerializedObjectField(_object, _memberName);
            if (_field != null) return _field.GetValue(_object.targetObject);

            // Try to find property
            PropertyInfo _property = FindSerializedObjectProperty(_object, _memberName);
            if (_property != null) return _property.GetValue(_object.targetObject);

            // If nothing is found, return null
            return null;
        }


        /// <summary>
        /// Retrieves a field from a given serialized object.
        /// </summary>
        /// <param name="_object">Serialized object to get field from.</param>
        /// <param name="_fieldName">Name of the field to find.</param>
        /// <returns>Returns the field info from the given field name if founded, null otherwise.</returns>
        public static FieldInfo FindSerializedObjectField(SerializedObject _object, string _fieldName)
        {
            Type _type = _object.targetObject.GetType();
            FieldInfo _info = null;

            while (_type != null)
            {
                _info = _type.GetField(_fieldName,
                              BindingFlags.GetField | BindingFlags.Instance |
                              BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static);

                if (_info != null) break;
                _type = _type.BaseType;
            }

            return _info;
        }

        /// <summary>
        /// Retrieves a property from a given serialized object.
        /// </summary>
        /// <param name="_object">Serialized object to get property from.</param>
        /// <param name="_propertyName">Name of the property to find.</param>
        /// <returns>Returns the property info from the given property name if founded, null otherwise.</returns>
        public static PropertyInfo FindSerializedObjectProperty(SerializedObject _object, string _propertyName)
        {
            Type _type = _object.targetObject.GetType();
            PropertyInfo _info = null;

            while (_type != null)
            {
                _info = _type.GetProperty(_propertyName,
                              BindingFlags.GetProperty | BindingFlags.Instance |
                              BindingFlags.Public | BindingFlags.NonPublic |
                              BindingFlags.Static);

                if (_info != null) break;
                _type = _type.BaseType;
            }

            return _info;
        }


        /// <summary>
        /// Set the value of a property (not a field) from a serialized object.
        /// </summary>
        /// <param name="_property">Serialized property used to get value from and find property.</param>
        /// <param name="_propertyName">Name of the property to set.</param>
        public static void SetPropertyValue(SerializedProperty _property, string _propertyName) => SetPropertyValue(_property.serializedObject, _propertyName, GetSerializedObjectFieldOrPropertyValue(_property.serializedObject, _property.name));

        /// <summary>
        /// Set the value of a property (not a field) from a serialized object.
        /// </summary>
        /// <param name="_object">Serialized object to find property from.</param>
        /// <param name="_propertyName">Name of the property to set.</param>
        /// <param name="_value">Property value.</param>
        public static void SetPropertyValue(SerializedObject _object, string _propertyName, object _value)
        {
            // Get property info
            PropertyInfo _propertyInfo = FindSerializedObjectProperty(_object, _propertyName);

            // If not null, set property for each editing object
            if (_propertyInfo != null)
            {
                foreach (Object _target in _object.targetObjects)
                {
                    // Try to set property and catch mismatching type exception
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
            // Debug if property wasn't found
            else Debug.LogWarning($"Property \"{_propertyName}\" could not be found in the \"{_object.targetObject.GetType()}\" script from \"{_object.targetObject.name}\".");
        }
        #endregion

        #endregion
    }
}
