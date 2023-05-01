// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
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
            AssemblyReloadEvents.beforeAssemblyReload   -= OnBeforeAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload    -= OnAfterAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload    -= ToggleGizmos;

            AssemblyReloadEvents.beforeAssemblyReload   += OnBeforeAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload    += OnAfterAssembliesReload;
            AssemblyReloadEvents.afterAssemblyReload    += ToggleGizmos;

            #if LOCALIZATION_ENABLED
            // Project locale setup.
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
        /// Get a unique id used to reference a specific <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="_property">The <see cref="SerializedProperty"/> to get an associated id.</param>
        /// <returns>The id associated with the given <see cref="SerializedProperty"/>.</returns>
        public static string GetSerializedPropertyID(SerializedProperty _property) {
            return _property.propertyPath + _property.serializedObject.targetObject.GetInstanceID().ToString();
        }

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
        /// <param name="_type">Property object type.</param>
        /// <returns>True if the property object type could be found, false otherwise.</returns>
        public static bool GetSerializedPropertyType(SerializedProperty _property, out Type _type) {
            if (FindSerializedPropertyField(_property, out FieldInfo _field)) {
                _type = _field.FieldType;
                _type = _type.IsGenericType ? _type.GetGenericTypeDefinition()
                                            : _type.IsArray ? _type.GetElementType()
                                                            : _field.FieldType;

                return true;
            }

            //Debug.LogWarning($"SerializedProperty type at path \'{_property.propertyPath}\' could not be found");

            _type = null;
            return false;
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

            if (_paths.Length == 0) {
                return false;
            }

            // Get component field base class.
            while ((_type.GetField(_paths[0], FieldFlags) == null) && (_type.BaseType != null)) {
                _type = _type.BaseType;
            }

            for (int i = 0; i < _paths.Length - _ignoreCount; i++) {
                _field = _type.GetField(_paths[i], FieldFlags);

                if (_field == null) {
                    return false;
                }

                // Collection.
                if (_field.FieldType.IsArray && ((i + 2) < _paths.Length)) {
                    _value = (_field.GetValue(_value) as Array).GetValue(GetPathIndex(_paths[i + 2]));
                    _type = (_value != null)
                          ? _value.GetType()
                          : _field.FieldType.GetElementType();

                    i += 2;
                    continue;
                }

                // List.
                if (_field.FieldType.IsGenericType && (_field.FieldType.GetGenericTypeDefinition() == typeof(List<>)) && ((i + 2) < _paths.Length)) {

                    _value = (_field.GetValue(_value) as IList)[GetPathIndex(_paths[i + 2])];
                    _type = (_value != null)
                          ? _value.GetType()
                          : _field.FieldType.GetElementType();

                    i += 2;
                    continue;
                }

                if (_field != null) {
                    _value = _field.GetValue(_value);
                    _type = (_value != null)
                          ? _value.GetType()
                          : _field.FieldType;
                } else {
                    // Not found.
                    _field = null;
                    return false;
                }
            }

            return true;

            // ----- Local Method ----- \\

            int GetPathIndex(string _path) {
                // String is "data[0]", where 0 is the object index.
                int _startIndex = _path.IndexOf('[') + 1;
                string _index = _path.Substring(_startIndex, _path.IndexOf(']') - _startIndex).ToString();

                return int.Parse(_index);
            }
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
            } else {

                while (_type.IsGenericType) {
                    _type = _type.GetGenericArguments()[0];
                }
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
        /// <typeparam name="T"><inheritdoc cref="FindAssetsGUID(Type)" path="/param[@name='_type']"/></typeparam>
        /// <inheritdoc cref="FindAssetsGUID(Type)"/>
        public static string[] FindAssetsGUID<T>() where T : Object {
            return FindAssetsGUID(typeof(T));
        }

        /// <summary>
        /// Finds the guid of all assets of a specific type in the project.
        /// </summary>
        /// <param name="_type">Asset type to find.</param>
        /// <returns>All found assets guid.</returns>
        public static string[] FindAssetsGUID(Type _type) {
            string _filter = $"t:{_type.Name}";
            return AssetDatabase.FindAssets(_filter);
        }

        /// <typeparam name="T"><inheritdoc cref="FindAssets(Type)" path="/param[@name='_type']"/></typeparam>
        /// <inheritdoc cref="FindAssets(Type)"/>
        public static string[] FindAssets<T>() where T : Object {
            return FindAssets(typeof(T));
        }

        /// <summary>
        /// Finds all assets of a specific type in the project.
        /// </summary>
        /// <param name="_type">Asset type to find.</param>
        /// <returns>All found assets path.</returns>
        public static string[] FindAssets(Type _type) {
            string[] _guids = FindAssetsGUID(_type);
            return Array.ConvertAll(_guids, AssetDatabase.GUIDToAssetPath);
        }

        /// <typeparam name="T"><inheritdoc cref="LoadAssets(Type)" path="/param[@name='_type']"/></typeparam>
        /// <inheritdoc cref="LoadAssets(Type)"/>
        public static T[] LoadAssets<T>() where T : Object {
            string[] _guids = FindAssetsGUID<T>();
            return Array.ConvertAll(_guids, (a) => {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(a));
            });
        }

        /// <summary>
        /// Loads all assets of a specific type in the project.
        /// </summary>
        /// <param name="_type">Asset type to load.</param>
        /// <returns>Collection of all loaded assets.</returns>
        public static Object[] LoadAssets(Type _type) {
            string[] _guids = FindAssetsGUID(_type);
            return Array.ConvertAll(_guids, (a) => {
                return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(a), _type);
            });
        }

        /// <typeparam name="T"><inheritdoc cref="LoadMainAsset(Type, out Object)" path="/param[@name='_type']"/></typeparam>
        /// <inheritdoc cref="LoadMainAsset(Type, out Object)"/>
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
        /// Loads the first asset of a specific type that could be found in the project.
        /// </summary>
        /// <param name="_type">Asset type to load.</param>
        /// <param name="_asset">Loaded asset.</param>
        /// <returns>True if an asset of this type could be found and loaded, false otherwise.</returns>
        public static bool LoadMainAsset(Type _type, out Object _asset) {
            string[] _guids = FindAssetsGUID(_type);
            if (_guids.Length != 0) {
                string _guid = _guids[0];
                _asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guid), _type);

                return true;
            }

            _asset = null;
            return false;
        }

        /// <summary>
        /// Get the GUID from a specific asset <see cref="Object"/>.
        /// </summary>
        /// <param name="_asset">The asset to get the GUID.</param>
        /// <returns>The GUID of the given asset.</returns>
        public static string GetAssetGUID(Object _asset) {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_asset));
        }

        // -----------------------

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

        #region Gizmos
        private const BindingFlags InstanceFlags    = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags      = BindingFlags.Static   | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type annotationUtilityType  = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnnotationUtility");
        private static readonly Type annotationType         = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Annotation");

        private static readonly MethodInfo getAnnotationsMethod     = annotationUtilityType.GetMethod("GetAnnotations", StaticFlags);
        private static readonly MethodInfo setGizmoEnabledMethod    = annotationUtilityType.GetMethod("SetGizmoEnabled", StaticFlags);
        private static readonly MethodInfo setIconEnabledMethod     = annotationUtilityType.GetMethod("SetIconEnabled", StaticFlags);

        private static readonly FieldInfo classIDField      = annotationType.GetField("classID", InstanceFlags);
        private static readonly FieldInfo scriptClassField  = annotationType.GetField("scriptClass", InstanceFlags);

        private static readonly object[] setGizmoEnabledArguments   = new object[4];
        private static readonly object[] setIconEnabledArguments    = new object[3];

        // -----------------------

        private static void ToggleGizmos() {

            IEnumerable _annotations = (IEnumerable)getAnnotationsMethod.Invoke(null, null);
            var _types = TypeCache.GetTypesWithAttribute<ScriptGizmosAttribute>().ToList();

            foreach (var _annotation in _annotations) {

                string _scriptClass = (string)scriptClassField.GetValue(_annotation);
                if (string.IsNullOrEmpty(_scriptClass)) {
                    continue;
                }

                Type _type = _types.Find(t => t.Name == _scriptClass);
                if (_type == null) {
                    continue;
                }

                bool _showGizmos = true;
                bool _showIcon = true;

                if (_type.IsDefined(typeof(ScriptGizmosAttribute), false)) {

                    ScriptGizmosAttribute _attribute = _type.GetCustomAttribute<ScriptGizmosAttribute>();
                    _showGizmos = _attribute.ShowGizmos;
                    _showIcon = _attribute.ShowIcon;
                }

                int _classID = (int)classIDField.GetValue(_annotation);

                // Calling this method forces the associated script to unfold in this inspector,
                // so until a solution is found, we won't call it anymore.
                bool _setGizmos = false;
                if (_setGizmos) {
                    setGizmoEnabledArguments[0] = _classID;
                    setGizmoEnabledArguments[1] = _scriptClass;
                    setGizmoEnabledArguments[2] = _showGizmos ? 1 : 0;
                    setGizmoEnabledArguments[3] = false;

                    setGizmoEnabledMethod.Invoke(null, setGizmoEnabledArguments);
                }

                setIconEnabledArguments[0] = _classID;
                setIconEnabledArguments[1] = _scriptClass;
                setIconEnabledArguments[2] = _showIcon ? 1 : 0;

                setIconEnabledMethod.Invoke(null, setIconEnabledArguments);
            }
        }
        #endregion

        #region Various
        /// <summary>
        /// Is a specific type either a <see cref="Component"/> or an interface?
        /// </summary>
        /// <param name="_type">Type to check.</param>
        /// <returns>True if this type is a component or an interface, false otherwise.</returns>
        public static bool IsComponentOrInterface(Type _type) {
            bool _isPickable = _type.IsInterface || _type.IsSameOrSubclass(typeof(Component));
            return _isPickable;
        }

        /// <summary>
        /// Is a specific type a scene object, that is either a <see cref="Component"/> or a <see cref="GameObject"/>?
        /// </summary>
        /// <param name="_type">Type to check.</param>
        /// <returns>True if this type is either a component or a game object, false otherwise.</returns>
        public static bool IsSceneObject(Type _type) {
            bool _isPickable = (_type == typeof(GameObject)) || _type.IsSameOrSubclass(typeof(Component));
            return _isPickable;
        }

        // -----------------------

        /// <inheritdoc cref="CreateObject(string, GameObject, Vector3, Type[])"/>
        public static GameObject CreateObject(string _name, params Type[] _components) {
            return CreateObject(_name, null, _components);
        }

        /// <inheritdoc cref="CreateObject(string, GameObject, Vector3, Type[])"/>
        public static GameObject CreateObject(string _name, GameObject _context, params Type[] _components) {
            GameObject _object = new GameObject(_name);
            return CreateObject(_object, _context, _components);
        }

        /// <inheritdoc cref="CreateObject(string, GameObject, Vector3, Type[])"/>
        public static GameObject CreateObject(GameObject _prefab, string _name, GameObject _context, params Type[] _components) {
            GameObject _object = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
            _object.name = _name;

            return CreateObject(_object, _context, _components);
        }

        /// <summary>
        /// Creates a new <see cref="GameObject"/> in the active scene.
        /// </summary>
        /// <param name="_name">Name of the object to create.</param>
        /// <param name="_context">Context object.</param>
        /// <param name="_position">World position of the object.</param>
        /// <param name="_components">Base component(s) of this object.</param>
        /// <returns>Newly created object instance.</returns>
        public static GameObject CreateObject(string _name, GameObject _context, Vector3 _position, params Type[] _components) {
            GameObject _object = CreateObject(_name, _context, _components);
            _object.transform.position = _position;

            return _object;
        }

        private static GameObject CreateObject(GameObject _object, GameObject _context, params Type[] _components) {

            for (int i = 0; i < _components.Length; i++) {
                CreateComponent(_object, _components[i]);
            }

            if (_context != null) {
                GameObjectUtility.SetParentAndAlign(_object, _context);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(_object);

            Undo.RegisterCreatedObjectUndo(_object, "Create " + _object.name);

            EditorGUIUtility.PingObject(_object);
            Selection.activeObject = _object;

            return _object;
        }

        // -----------------------

        /// <inheritdoc cref="CreateComponent(GameObject, Type, bool)"/>
        public static T CreateComponent<T>(GameObject _object, bool _allowDuplicate = false) where T : class {
            return CreateComponent(_object, typeof(T), _allowDuplicate) as T;
        }

        /// <summary>
        /// Creates, adds and setups a specific <see cref="Component"/> type on a <see cref="GameObject"/>.
        /// </summary>
        /// <param name="_object"><see cref="GameObject"/> on which to add the component.</param>
        /// <param name="_componentType">Type of the component to create.</param>
        /// <param name="_allowDuplicate">If true, create the component even in another one is already on the same object..</param>
        /// <returns>The created <see cref="Component"/>.</returns>
        public static Component CreateComponent(GameObject _object, Type _componentType, bool _allowDuplicate = false) {

            if (!_allowDuplicate && _object.TryGetComponent(_componentType, out Component _component)) {
                return _component;
            }

            _component = Undo.AddComponent(_object, _componentType);
            Preset[] _presets = Preset.GetDefaultPresetsForObject(_component);

            if (_presets.Length != 0) {
                _presets[0].ApplyTo(_component);
            }

            return _component;
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
