// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple editor utility methods.
    /// </summary>
	public static class EnhancedEditorUtility
    {
        #region Color Picker
        private static readonly BindingFlags colorPickerFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object[] colorPickerArgs = new object[] { null, null, null, true, false };

        // -----------------------

        /// <summary>
        /// Opens the editor built-in color picker window.
        /// </summary>
        /// <param name="_color">The color the color picker shows.</param>
        /// <param name="_callback">Callback for whenever the user selects a new color in the color picker.</param>
        public static void ColorPicker(Color _color, Action<Color> _callback)
        {
            colorPickerArgs[1] = _callback;
            colorPickerArgs[2] = _color;

            EditorWindow _colorPicker = ScriptableObject.CreateInstance("ColorPicker") as EditorWindow;
            _colorPicker.GetType().InvokeMember("Show", colorPickerFlags, null, _colorPicker, colorPickerArgs);
        }
        #endregion

        #region Serialized Object
        private static readonly BindingFlags getFieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetField;

        // -----------------------

        /// <summary>
        /// Retrieves a specific field from a <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_object"><see cref="SerializedObject"/> to get this field from.</param>
        /// <param name="_fieldPath">Full path of this field (like <see cref="SerializedProperty.propertyPath"/>).</param>
        /// <param name="_field">Found <see cref="FieldInfo"/>.</param>
        /// <returns>True if this field was successfully found, false otherwise.</returns>
        public static bool FindSerializedObjectField(SerializedObject _object, string _fieldPath, out FieldInfo _field)
        {
            Type _type = _object.targetObject.GetType();
            _field = null;

            string[] _fields = _fieldPath.Split('.');
            for (int _i = 0; _i < _fields.Length; _i++)
            {
                _field = _type.GetField(_fields[_i], getFieldFlags);

                if (_field == null)
                    return false;

                // Serialized containers management:
                // Array.
                if (_field.FieldType.IsArray)
                {
                    _type = _field.FieldType.GetElementType();
                    _i += 2;

                    continue;
                }

                // List.
                if (_field.FieldType.IsGenericType && (_field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    _type = _field.FieldType.GetGenericArguments()[0];
                    _i += 2;

                    continue;
                }

                _type = _field.FieldType;
            }

            return true;
        }
        #endregion

        #region Serialized Property
        /// <summary>
        /// Get the object type of a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get type from.</param>
        /// <returns>Property object type.</returns>
        public static Type GetSerializedPropertyType(SerializedProperty _property)
        {
            if (FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _field))
            {
                return GetFieldInfoType(_field);
            }

            return null;
        }

        /// <summary>
        /// Get a <see cref="SerializedProperty"/> value as single (for <see cref="float"/>, <see cref="int"/> and <see cref="Enum"/> property types). 
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get value from.</param>
        /// <param name="_value">Property value as single (0 if the property type is not compatible).</param>
        /// <returns>True if the property value has been fully converted as single, false otherwise.</returns>
        public static bool GetSerializedPropertyValueAsSingle(SerializedProperty _property, out float _value)
        {
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    _value = _property.intValue;
                    break;

                case SerializedPropertyType.Float:
                    _value = _property.floatValue;
                    break;

                case SerializedPropertyType.Enum:
                    _value = _property.enumValueIndex;
                    break;

                // Not matching type.
                default:
                    _value = 0f;
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Set a <see cref="SerializedProperty"/> value if it can be converted as single
        /// (for <see cref="float"/>, <see cref="int"/> and <see cref="Enum"/> property types). 
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to set value.</param>
        /// <param name="_value">New property value.</param>
        /// <returns>True if the property value has been successfully set, false otherwise.</returns>
        public static bool SetSerializedPropertyValueAsSingle(SerializedProperty _property, float _value)
        {
            switch (_property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    _property.intValue = (int)_value;
                    break;

                case SerializedPropertyType.Float:
                    _property.floatValue = _value;
                    break;

                case SerializedPropertyType.Enum:
                    _property.enumValueIndex = (int)_value;
                    break;

                // Not matching type.
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ceils a <see cref="SerializedProperty"/> value, so that is does not exceed a specific maximum.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to ceil value.</param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void CeilSerializedPropertyValue(SerializedProperty _property, float _maxValue)
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
        /// Floors a <see cref="SerializedProperty"/> value, so that it does not go under a specific minimum.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to floor value.</param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void FloorSerializedPropertyValue(SerializedProperty _property, float _minValue)
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

        #region Field Reflection
        /// <summary>
        /// Get the actual type of a specific <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="_field"><see cref="FieldInfo"/> to get type.</param>
        /// <returns>Field object type.</returns>
        public static Type GetFieldInfoType(FieldInfo _field)
        {
            Type _type = _field.FieldType;
            if (_type.IsArray)
            {
                _type = _type.GetElementType();

                if (_type.IsGenericType)
                    _type = _type.GetGenericArguments()[0];
            }
            else if (_type.IsGenericType)
            {
                _type = _type.GetGenericArguments()[0];
            }

            return _type;
        }
        #endregion

        #region Assets
        /// <summary>
        /// Finds the guid of all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>All found assets guid.</returns>
        public static string[] FindAssetsGUID<T>() where T : Object
        {
            string _filter = $"t:{typeof(T).Name}";
            return AssetDatabase.FindAssets(_filter);
        }

        /// <summary>
        /// Finds all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>All found assets path.</returns>
        public static string[] FindAssets<T>() where T : Object
        {
            string[] _guids = FindAssetsGUID<T>();
            return Array.ConvertAll(_guids, AssetDatabase.GUIDToAssetPath);
        }

        /// <summary>
        /// Loads all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <returns>Array of all loaded assets.</returns>
        public static T[] LoadAssets<T>() where T : Object
        {
            string[] _guids = FindAssetsGUID<T>();
            return Array.ConvertAll(_guids, (a) =>
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(a));
            });
        }

        /// <summary>
        /// Loads the first asset of a specific type that could be found in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <param name="_asset">Loaded asset.</param>
        /// <returns>True if an asset of this type could be found and loaded, false otherwise.</returns>
        public static bool LoadMainAsset<T>(out T _asset) where T : Object
        {
            string[] _guids = FindAssetsGUID<T>();
            if (_guids.Length != 0)
            {
                string _guid = _guids[0];
                _asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(_guid));

                return true;
            }

            _asset = null;
            return false;
        }

        /// <summary>
        /// Saves a specific asset in the database.
        /// </summary>
        /// <param name="_asset">Database asset to save.</param>
        public static void SaveAsset(Object _asset) {
            string _path = AssetDatabase.GetAssetPath(_asset);
            if (string.IsNullOrEmpty(_path))
                return;

            EditorUtility.SetDirty(_asset);
            AssetModificationProcessor.saveAssetPath = _path;
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region Various
        /// <summary>
        /// Is a specific type either a <see cref="Component"/> or an interface?
        /// </summary>
        /// <param name="_type">Type to check.</param>
        /// <returns>True if this type is a component or an interface, false otherwise.</returns>
        public static bool IsComponentOrInterface(Type _type)
        {
            bool _isPickable = _type.IsInterface || _type.IsSubclassOf(typeof(Component));
            return _isPickable;
        }

        /// <summary>
        /// Is a specific type a scene object, that is either a <see cref="Component"/> or a <see cref="GameObject"/>?
        /// </summary>
        /// <param name="_type">Type to check.</param>
        /// <returns>True if this type is either a component or a game object, false otherwise.</returns>
        public static bool IsSceneObject(Type _type)
        {
            bool _isPickable = (_type == typeof(GameObject)) || _type.IsSubclassOf(typeof(Component));
            return _isPickable;
        }
        #endregion
    }
}
