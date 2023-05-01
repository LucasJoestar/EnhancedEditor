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
using UnityEditorInternal;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Base class to derive any <see cref="Object"/> custom editor from (instead of <see cref="UnityEditor.Editor"/>),
    /// performing additional operations related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    public class UnityObjectEditor : UnityEditor.Editor {
        #region Method Drawer Group
        private class MethodDrawerGroup {
            public MethodDrawer[] MethodDrawers = new MethodDrawer[] { };

            // -----------------------

            public void DrawMethodDrawers(bool _isOnTop) {
                Rect _position = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);

                // Pre GUI callback.
                foreach (MethodDrawer _drawer in MethodDrawers) {
                    if ((_isOnTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnBeforeGUI())
                        return;
                }

                // Method GUI.
                foreach (MethodDrawer _drawer in MethodDrawers) {
                    if ((_isOnTop == _drawer.Attribute.IsDrawnOnTop) && _drawer.OnGUI())
                        break;
                }

                // Post GUI callback.
                foreach (MethodDrawer _drawer in MethodDrawers) {
                    if (_isOnTop == _drawer.Attribute.IsDrawnOnTop)
                        _drawer.OnAfterGUI();
                }

                // Context click menu. 
                _position.height = GUILayoutUtility.GetLastRect().yMax - _position.y;

                if (EnhancedEditorGUIUtility.ContextClick(_position)) {
                    GenericMenu _menu = new GenericMenu();
                    foreach (MethodDrawer _drawer in MethodDrawers)
                        _drawer.OnContextMenu(_menu);

                    if (_menu.GetItemCount() > 0)
                        _menu.ShowAsContext();
                }
            }
        }
        #endregion

        #region Editor Content
        private const string ScriptPropertyName     = "m_Script";
        private const string ChronosPropertyName    = "chronos";
        protected const float SaveValueButtonWidth = 100f;

        private const BindingFlags MethodInfoFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        protected static readonly GUIContent saveValueGUI = new GUIContent(" Save Values", "Automatically save this object values and apply them on play mode exit.");
        protected static readonly GUIContent loadValueGUI = new GUIContent(" Load Values", "Loads this object saved values.");

        private static readonly Color saveButtonColor = new Color(.25f, 1f, .25f);
        private static readonly Color loadButtonColor = SuperColor.Orange.Get();

        private static readonly Dictionary<UnityObjectEditor, Type> editors = new Dictionary<UnityObjectEditor, Type>();
        private readonly List<SerializedProperty> properties = new List<SerializedProperty>();

        private UnityObjectDrawer[] objectDrawers = new UnityObjectDrawer[] { };
        private MethodDrawerGroup[] methodDrawerGroups = new MethodDrawerGroup[] { };

        /// <summary>
        /// Indicates if the data of the editing object can be saved.
        /// </summary>
        protected virtual bool CanSaveData {
            get { return false; }
        }

        // -----------------------

        protected virtual void OnEnable() {
            try {
                if (!serializedObject.targetObject) {
                    DestroyImmediate(this);
                    return;
                }
            } catch {
                DestroyImmediate(this);
                return;
            }

            // Registration.
            editors.Add(this, target.GetType());

            saveValueGUI.image = EditorGUIUtility.FindTexture("SaveAs");
            loadValueGUI.image = EditorGUIUtility.FindTexture("SaveAs");

            // Get properties.
            SerializedProperty _property = serializedObject.GetIterator();
            if (_property.NextVisible(true)) {

                // First property (script) should always be drawn first.
                List<Pair<SerializedProperty, int>> _properties = new List<Pair<SerializedProperty, int>> {
                    new Pair<SerializedProperty, int>(_property.Copy(), int.MinValue)
                };

                int _order = 0;

                while (_property.NextVisible(false)) {

                    if (EnhancedEditorUtility.FindSerializedPropertyField(_property, out FieldInfo _field) && _field.IsDefined(typeof(PropertyOrderAttribute), true)) {
                        PropertyOrderAttribute _attribute = _field.GetCustomAttribute<PropertyOrderAttribute>();
                        _order = _attribute.PropertyOrder;
                    }

                    int _index = _properties.FindIndex(p => p.Second > _order);
                    if (_index == -1) {
                        _index = _properties.Count;
                    }

                    _properties.Insert(_index, new Pair<SerializedProperty, int>(_property.Copy(), _order));
                };
                
                // Register ordered properties.
                properties.AddRange(_properties.ConvertAll(p => p.First));
            }

            // Get all class attributes from editing object, then create and enable a drawer for each of them.
            Type _type = serializedObject.targetObject.GetType();
            {
                var _attributes = _type.GetCustomAttributes(typeof(EnhancedClassAttribute), true) as EnhancedClassAttribute[];

                foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetObjectDrawers()) {
                    foreach (EnhancedClassAttribute _attribute in _attributes) {
                        if (_pair.Value == _attribute.GetType()) {
                            UnityObjectDrawer _customDrawer = UnityObjectDrawer.CreateInstance(_pair.Key, serializedObject, _attribute);
                            ArrayUtility.Add(ref objectDrawers, _customDrawer);
                        }
                    }
                }
            }

            // Do the same for method attributes.
            {
                MethodInfo[] _allMethods = _type.GetMethods(MethodInfoFlags);
                var _methodDrawers = EnhancedDrawerUtility.GetMethodDrawers();

                foreach (MethodInfo _method in _allMethods) {
                    var _attributes = _method.GetCustomAttributes(typeof(EnhancedMethodAttribute), true) as EnhancedMethodAttribute[];

                    // Skip methods without any desired attribute.
                    if (_attributes.Length == 0)
                        continue;

                    MethodDrawerGroup _group = new MethodDrawerGroup();

                    foreach (KeyValuePair<Type, Type> _pair in _methodDrawers) {
                        foreach (EnhancedMethodAttribute _attribute in _attributes) {
                            if (_pair.Value == _attribute.GetType()) {
                                MethodDrawer _customDrawer = MethodDrawer.CreateInstance(_pair.Key, serializedObject, _attribute, _method);
                                ArrayUtility.Add(ref _group.MethodDrawers, _customDrawer);
                            }
                        }
                    }

                    Array.Sort(_group.MethodDrawers, (a, b) => a.Attribute.Order.CompareTo(b.Attribute.Order));
                    ArrayUtility.Add(ref methodDrawerGroups, _group);
                }
            }

            // Add the EnhancedEditor GameObject-extending component if none is attached.
            if (!Application.isPlaying && ExtendedBehaviourEnhancedSettings.AutomaticSetup) {
                foreach (var _object in serializedObject.targetObjects) {
                    // If this object is a prefab, only add the component to the origin asset to avoid troubles with the prefab override system.
                    if (PrefabUtility.IsPartOfPrefabInstance(_object) || PrefabUtility.IsAddedComponentOverride(_object) || PrefabUtility.IsPartOfImmutablePrefab(_object))
                        continue;

                    if (_object is Component _component) {
                        var _behaviour = _component.gameObject.AddComponentIfNone<ExtendedBehaviour>();

                        do { }
                        while (ComponentUtility.MoveComponentUp(_behaviour));
                    }
                }
            }
        }

        public override void OnInspectorGUI() {
            try {
                // Top method drawers.
                foreach (MethodDrawerGroup _group in methodDrawerGroups) {
                    _group.DrawMethodDrawers(true);
                }

                // Inspector.
                bool _drawInspector = true;
                foreach (UnityObjectDrawer _drawer in objectDrawers) {
                    if (_drawer.OnInspectorGUI()) {
                        _drawInspector = false;
                        break;
                    }
                }

                if (_drawInspector) {

                    if (properties.Count != 0) {

                        serializedObject.Update();
                        int _startIndex = 0;

                        // First property is script type, so display it as readonly.
                        if (properties[0].propertyPath == ScriptPropertyName) {

                            _startIndex++;
                            EnhancedEditorGUILayout.ReadonlyField(properties[0], true);
                        }

                        if ((properties.Count > 2) && (properties[1].propertyPath == ChronosPropertyName)) {

                            _startIndex++;
                            EnhancedEditorGUILayout.ReadonlyField(properties[1], true);
                        }

                        if (DrawInspectorGUI()) {
                            for (int i = _startIndex; i < properties.Count; i++) {
                                SerializedProperty _property = properties[i];
                                EditorGUILayout.PropertyField(_property, true);
                            }
                        }

                        serializedObject.ApplyModifiedProperties();

                    } else {

                        // Default inspector.
                        base.OnInspectorGUI();
                    }

                    OnAfterInspectorGUI();
                }

                GUILayout.Space(10f);

                // Bottom method drawers.
                foreach (MethodDrawerGroup _group in methodDrawerGroups) {
                    _group.DrawMethodDrawers(false);
                }
            } catch (InvalidOperationException e) {
                this.LogException(e);
            }
        }

        protected virtual void OnDisable() {

            // Unegistration.
            editors.Remove(this);

            foreach (var _drawer in objectDrawers) {
                _drawer.OnDisable();
            }
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        /// <summary>
        /// Override this to implement your own GUI version of this script.
        /// </summary>
        /// <returns>True to draw the default inspector, false otherwise.</returns>
        protected virtual bool DrawInspectorGUI() {
            return true;
        }
        
        /// <summary>
        /// Called after this object inspector GUI was drawn.
        /// </summary>
        protected virtual void OnAfterInspectorGUI() { }

        /// <summary>
        /// Saves an object data.
        /// </summary>
        /// <param name="_object"><see cref="Object"/> to save data.</param>
        /// <returns>Object serialized data.</returns>
        protected virtual PlayModeObjectData SaveData(Object _object) {
            return new PlayModeObjectData(_object);
        }

        // -------------------------------------------
        // Save
        // -------------------------------------------

        /// <summary>
        /// Draw buttons to save / load properties.
        /// </summary>
        protected void SaveLoadButtonGUILayout() {

            if (Application.isPlaying) {

                // Save properties.
                SaveButtonGUI(GetSaveLoadButtonPosition());

            } else if (PlayModeDataSave.Contain(target)) {

                // Load properties.
                LoadButtonGUI(GetSaveLoadButtonPosition());
            }
        }

        /// <inheritdoc cref="SaveLoadButtonGUILayout"/>
        protected void SaveLoadButtonGUI(Rect _position) {

            if (Application.isPlaying) {

                // Save properties.
                SaveButtonGUI(_position);

            } else if (PlayModeDataSave.Contain(target)) {

                // Load properties.
                LoadButtonGUI(_position);
            }
        }

        /// <summary>
        /// Draw button to save properties.
        /// </summary>
        protected void SaveButtonGUI(Rect _position) {

            using (var _scope = EnhancedGUI.GUIContentColor.Scope(saveButtonColor)) {
                if (EnhancedEditorGUI.IconDropShadowButton(_position, saveValueGUI)) {

                    foreach (Object _target in targets) {

                        MenuCommand _command = new MenuCommand(_target);
                        SaveData(_command);
                    }
                }
            }
        }

        /// <summary>
        /// Draw button to load properties.
        /// </summary>
        protected void LoadButtonGUI(Rect _position) {

            using (var _scope = EnhancedGUI.GUIContentColor.Scope(loadButtonColor)) {
                if (EnhancedEditorGUI.IconDropShadowButton(_position, loadValueGUI)) {

                    foreach (Object _target in targets) {

                        MenuCommand _command = new MenuCommand(_target);
                        LoadData(_command);
                    }
                }
            }
        }

        // -----------------------

        private Rect GetSaveLoadButtonPosition() {

            Rect _position = EditorGUILayout.GetControlRect(true, 20f);
            _position.xMin = _position.xMax - SaveValueButtonWidth;

            return _position;
        }
        #endregion

        #region Context
        private const int ContextMenuOrder = 900;
        private const string SaveDataMenu = "CONTEXT/Component/Save Data";
        private const string LoadDataMenu = "CONTEXT/Component/Load Data";

        // -------------------------------------------
        // Save
        // -------------------------------------------

        /// <summary>
        /// Saves this context object data.
        /// </summary>
        [MenuItem(SaveDataMenu, false, ContextMenuOrder)]
        public static void SaveData(MenuCommand _command) {

            Object _context = _command.context;

            if (CanSaveObjectData(_context, out UnityObjectEditor _editor)) {

                PlayModeObjectData _data = _editor.SaveData(_context);
                PlayModeDataSave.SaveData(_data);
            }
        }

        /// <summary>
        /// Get if this context object data can be saved.
        /// </summary>
        [MenuItem(SaveDataMenu, true, ContextMenuOrder)]
        public static bool SaveDataValidate(MenuCommand _command) {
            return CanSaveObjectData(_command.context, out _) && Application.isPlaying;
        }

        // -------------------------------------------
        // Load
        // -------------------------------------------

        /// <summary>
        /// Loads this context object data.
        /// </summary>
        [MenuItem(LoadDataMenu, false, ContextMenuOrder)]
        public static void LoadData(MenuCommand _command) {

            Object _context = _command.context;

            if (CanSaveObjectData(_context, out UnityObjectEditor _editor)) {

                PlayModeDataSave.LoadData(_context);
            }
        }

        /// <summary>
        /// Get if this context object data can be loaded.
        /// </summary>
        [MenuItem(LoadDataMenu, true, ContextMenuOrder)]
        public static bool LoadDataValidate(MenuCommand _command) {
            Object _context = _command.context;
            return CanSaveObjectData(_context, out _) && PlayModeDataSave.Contain(_context);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static bool CanSaveObjectData(Object _object, out UnityObjectEditor _editor) {

            // Null object.
            if (_object == null) {
                _editor = null;
                return false;
            }

            Type _type = _object.GetType();

            // Find matching active editor.
            foreach (var _temp in editors) {
                if (_type.IsSameOrSubclass(_temp.Value)) {

                    _editor = _temp.Key;
                    return _editor.CanSaveData;
                }
            }

            _editor = null;
            return false;
        }
        #endregion
    }
}
