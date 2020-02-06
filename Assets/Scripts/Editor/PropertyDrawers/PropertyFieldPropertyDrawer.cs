using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor
{
    [CustomPropertyDrawer(typeof(PropertyFieldAttribute))]
    public class PropertyFieldPropertyDrawer : PropertyDrawer
    {
        #region Fields
        /// <summary>
        /// Used to indicate if editing field value changed during previous frame.
        /// </summary>
        private bool _isChanged = false;
        #endregion

        #region Methods
        /********************************
         *****   ORIGINAL METHODS   *****
         *******************************/

        /// <summary>
        /// Set editing field linked property.
        /// </summary>
        /// <param name="_property">SerializedProperty of the editing field.</param>
        private void SetProperty(SerializedProperty _property)
        {
            PropertyFieldAttribute _attribute = (PropertyFieldAttribute)attribute;

            // Get linked property name
            string _propertyName = string.IsNullOrEmpty(_attribute.PropertyName) ?
                                    (char.ToUpper(_property.name[0]) + _property.name.Substring(1)) :
                                    _attribute.PropertyName;

            // Indicates if a property was set or not
            bool _isSet = false;

            // Set property for each editing object
            foreach (Object _target in _property.serializedObject.targetObjects)
            {
                if (_target == null) continue;

                // Get field value
                FieldInfo _info = _target.GetType().GetField(_property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_info == null)
                {
                    continue;
                }

                // Try to get property from each inherited class
                Type _type = _target.GetType();
                while (_type != null)
                {
                    PropertyInfo _propertyInfo = _type.GetProperty(_propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                    if (_propertyInfo != null)
                    {
                        // Try to set property and catch mismatching type exception
                        try
                        {
                            object _value = _info.GetValue(_target);
                            _propertyInfo.SetValue(_target, _value);
                        }
                        catch (ArgumentException)
                        {
                            Debug.LogWarning($"Field value of \"{_property.name}\" could not be assigned to property \"{_propertyName}\" on \"{_target.GetType()}\" script from \"{_target.name}\".");
                        }
                        _isSet = true;
                        break;
                    }

                    _type = _type.BaseType;
                }

                // Debug if no property was set on the object
                if (!_isSet)
                {
                    Debug.LogWarning($"Property \"{_propertyName}\" could not be found in the \"{_target.GetType()}\" script from \"{_target.name}\".");
                }
            }
        }


        /*****************************
         *****   UNITY METHODS   *****
         ****************************/

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // If value changed, set linked property
            if (_isChanged)
            {
                SetProperty(_property);
                _isChanged = false;
            }

            // Draw property field and set boolean if value changed ;
            // The boolean is needed because the editing field
            // value is not changed instantly, so we can only get its
            // new value by reflection during the next one...
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(_position, _property, _label);
            
            if (EditorGUI.EndChangeCheck()) _isChanged = true;
        }
        #endregion
    }
}
