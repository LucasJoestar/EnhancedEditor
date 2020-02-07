using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multiple utility methods and properties related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public static class EditorGUIUtilityEnhanced
    {
        #region Fields / Properties

        #endregion

        #region Methods

        #region Reflection
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
        #endregion

        #endregion
    }
}
