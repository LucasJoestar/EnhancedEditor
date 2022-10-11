// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if LOCALIZATION_ENABLED
using UnityEngine.Localization.Settings;
#endif

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Contains multiple editor utility methods.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedEditorUtility {
        #region Global Members
        static EnhancedEditorUtility() {
            // Assembly reloading.
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssembliesReload;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssembliesReload;

            #if LOCALIZATION_ENABLED
            // Project locale setup.
            var _settings = EnhancedEditorSettings.Settings.UserSettings;

            if (LocalizationSettings.SelectedLocale == null) {
                LocalizationSettings.SelectedLocale = LocalizationSettings.ProjectLocale;
            }
            #endif
        }
        #endregion

        #region Color Picker
        private const BindingFlags ColorPickerFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object[] colorPickerArgs = new object[] { null, null, null, true, false };

        // -----------------------

        /// <summary>
        /// Opens the editor built-in color picker window.
        /// </summary>
        /// <param name="_color">The color the color picker shows.</param>
        /// <param name="_callback">Callback for whenever the user selects a new color in the color picker.</param>
        public static void ColorPicker(Color _color, Action<Color> _callback) {
            colorPickerArgs[1] = _callback;
            colorPickerArgs[2] = _color;

            EditorWindow _colorPicker = ScriptableObject.CreateInstance("ColorPicker") as EditorWindow;
            _colorPicker.GetType().InvokeMember("Show", ColorPickerFlags, null, _colorPicker, colorPickerArgs);
        }
        #endregion

        #region Serialized Property
        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // -----------------------

        /// <summary>
        /// Get the the type name of the actual value of a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get the type name from.</param>
        /// <returns>The property value type name.</returns>
        public static string GetSerializedPropertyValueTypeName(SerializedProperty _property) {
            string _name = _property.type;
            int _index = _name.IndexOf('<');

            if (_index == -1) {
                return _name;
            }

            _index++;
            return _name.Substring(_index, _name.Length - (_index + 1));
        }

        /// <summary>
        /// Get the object type of a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get type from.</param>
        /// <returns>Property object type.</returns>
        public static Type GetSerializedPropertyType(SerializedProperty _property) {
            if (FindSerializedPropertyField(_property, out FieldInfo _field)) {
                Type _type = _field.FieldType;
                return _type.IsGenericType ? _type.GetGenericTypeDefinition() : _field.FieldType;
            }

            return null;
        }

        /// <summary>
        /// Get a <see cref="SerializedProperty"/> value as single (for <see cref="float"/>, <see cref="int"/> and <see cref="Enum"/> property types). 
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get value from.</param>
        /// <param name="_value">Property value as single (0 if the property type is not compatible).</param>
        /// <returns>True if the property value has been fully converted as single, false otherwise.</returns>
        public static bool GetSerializedPropertyValueAsSingle(SerializedProperty _property, out float _value) {
            switch (_property.propertyType) {
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
        public static bool SetSerializedPropertyValueAsSingle(SerializedProperty _property, float _value) {
            switch (_property.propertyType) {
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
        public static void CeilSerializedPropertyValue(SerializedProperty _property, float _maxValue) {
            switch (_property.propertyType) {
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
        public static void FloorSerializedPropertyValue(SerializedProperty _property, float _minValue) {
            switch (_property.propertyType) {
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

        /// <summary>
        /// Finds the <see cref="SerializedProperty"/> with a specific name from another source <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_sourceProperty">The source <see cref="SerializedProperty"/> to get the other property from.</param>
        /// <param name="_propertyName">The name of the property to find.</param>
        /// <param name="_property">The found <see cref="SerializedProperty"/> (null if none)</param>
        /// <returns>True if the property was successfully found, false otherwise.</returns>
        public static bool FindSerializedProperty(SerializedProperty _sourceProperty, string _propertyName, out SerializedProperty _property) {
            SerializedObject _object = _sourceProperty.serializedObject;

            string[] _fullPath = _sourceProperty.propertyPath.Split('.');
            string _path = string.Empty;
            int _count = 0;

            while (_count < _fullPath.Length) {
                _property = _object.FindProperty($"{_path}{_propertyName}");

                if (_property != null) {
                    return true;
                }

                if (_count == 0) {
                    _path = $"{_fullPath[_count]}.";
                } else {
                    _path = $"{_path}{_fullPath[_count]}.";
                }

                _count++;
            }

            _property = null;
            return false;
        }

        /// <summary>
        /// Retrieves a specific field from a <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to get this field from.</param>
        /// <param name="_field">Found <see cref="FieldInfo"/>.</param>
        /// <returns>True if this field was successfully found, false otherwise.</returns>
        public static bool FindSerializedPropertyField(SerializedProperty _property, out FieldInfo _field) {
            return FindSerializedPropertyField(_property, 0, out _field, out _, out _);
        }

        // -----------------------

        internal static bool FindSerializedPropertyField(SerializedProperty _property, int _ignoreCount, out FieldInfo _field, out Type _type, out object _value) {
            string[] _paths = _property.propertyPath.Split('.');

            _value = _property.serializedObject.targetObject;
            _type = _value.GetType();
            _field = null;

            for (int i = 0; i < _paths.Length - _ignoreCount; i++) {
                _field = _type.GetField(_paths[i], FieldFlags);

                if (_field == null) {
                    return false;
                }

                // Array and Lists.
                if (_field.FieldType.IsArray) {
                    // String is "data[0]", where 0 is the object index.
                    string _index = _paths[i + 2][5].ToString();

                    _value = (_field.GetValue(_value) as Array).GetValue(int.Parse(_index));
                    _type = _value.GetType();

                    i += 2;
                    continue;
                }

                if (_field.FieldType.IsGenericType) {

                    if ((i + 1) < (_paths.Length - _ignoreCount)) {
                        _type = _field.FieldType.GetGenericArguments()[0];
                        i += 2;
                    }

                    continue;
                }

                if (_field != null) {
                    _type = _field.FieldType;
                    _value = _field.GetValue(_value);
                } else {
                    // Not found.
                    _field = null;
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Field Reflection
        /// <summary>
        /// Get the actual content or generic type of a specific <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="_field"><see cref="FieldInfo"/> to get type.</param>
        /// <returns>Field object type.</returns>
        public static Type GetFieldInfoType(FieldInfo _field) {
            Type _type = _field.FieldType;
            if (_type.IsArray) {
                _type = _type.GetElementType();

                if (_type.IsGenericType)
                    _type = _type.GetGenericArguments()[0];
            } else if (_type.IsGenericType) {
                _type = _type.GetGenericArguments()[0];
            }

            return _type;
        }
        #endregion

        #region Project
        /// <summary>
        /// Get the path of the selected folder in the project window.
        /// </summary>
        /// <returns>Selected folder path.</returns>
        public static string GetProjectSelectedFolderPath() {
            string _path = GetProjectPath();
            _path = Path.Combine(_path, AssetDatabase.GetAssetPath(Selection.activeObject));

            if (!Directory.Exists(_path)) {
                _path = Path.GetDirectoryName(_path);
            }

            return _path;
        }

        /// <summary>
        /// Get the path of the project (similar to <see cref="Application.dataPath"/> but without the '/Assets' folder).
        /// </summary>
        /// <returns></returns>
        public static string GetProjectPath() {
            string _assetFolder = UnityEditorInternal.InternalEditorUtility.GetAssetsFolder();
            string _path = Application.dataPath;

            return _path.Remove(_path.Length - _assetFolder.Length, _assetFolder.Length);
        }
        #endregion

        #region Assets
        /// <summary>
        /// Finds the guid of all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>All found assets guid.</returns>
        public static string[] FindAssetsGUID<T>() where T : Object {
            string _filter = $"t:{typeof(T).Name}";
            return AssetDatabase.FindAssets(_filter);
        }

        /// <summary>
        /// Finds all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>All found assets path.</returns>
        public static string[] FindAssets<T>() where T : Object {
            string[] _guids = FindAssetsGUID<T>();
            return Array.ConvertAll(_guids, AssetDatabase.GUIDToAssetPath);
        }

        /// <summary>
        /// Loads all assets of a specific type in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <returns>Array of all loaded assets.</returns>
        public static T[] LoadAssets<T>() where T : Object {
            string[] _guids = FindAssetsGUID<T>();
            return Array.ConvertAll(_guids, (a) => {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(a));
            });
        }

        /// <summary>
        /// Loads the first asset of a specific type that could be found in the project.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <param name="_asset">Loaded asset.</param>
        /// <returns>True if an asset of this type could be found and loaded, false otherwise.</returns>
        public static bool LoadMainAsset<T>(out T _asset) where T : Object {
            string[] _guids = FindAssetsGUID<T>();
            if (_guids.Length != 0) {
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
        public static bool IsComponentOrInterface(Type _type) {
            bool _isPickable = _type.IsInterface || _type.IsSubclassOf(typeof(Component));
            return _isPickable;
        }

        /// <summary>
        /// Is a specific type a scene object, that is either a <see cref="Component"/> or a <see cref="GameObject"/>?
        /// </summary>
        /// <param name="_type">Type to check.</param>
        /// <returns>True if this type is either a component or a game object, false otherwise.</returns>
        public static bool IsSceneObject(Type _type) {
            bool _isPickable = (_type == typeof(GameObject)) || _type.IsSubclassOf(typeof(Component));
            return _isPickable;
        }
        #endregion

        #region Menu Command
        /// <summary>
        /// Ping the selected object in the project or hierarchy window.
        /// </summary>
        [MenuItem("CONTEXT/ScriptableObject/Ping Asset", false, 499)]
        [MenuItem("CONTEXT/Component/Ping Game Object", false, 999)]
        public static void PingObject(MenuCommand _command) {
            EditorGUIUtility.PingObject(_command.context);
        }
        #endregion

        #region Assembly Reload
        private const string ReloadingAssembliesKey = "IsReloadingAssemblies";

        /// <summary>
        /// True once <see cref="AssemblyReloadEvents.beforeAssemblyReload"/> is called, false after <see cref=" AssemblyReloadEvents.afterAssemblyReload"/>.
        /// </summary>
        public static bool isReloadingAssemblies => SessionState.GetBool(ReloadingAssembliesKey, false);

        // -----------------------

        private static void OnBeforeAssembliesReload() {
            SessionState.SetBool(ReloadingAssembliesKey, true);
        }

        private static void OnAfterAssembliesReload() {
            SessionState.SetBool(ReloadingAssembliesKey, false);
        }
        #endregion
    }
}
