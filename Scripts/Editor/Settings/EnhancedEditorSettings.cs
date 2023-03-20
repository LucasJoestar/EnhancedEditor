// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

#if UNITY_2021_1_OR_NEWER
#define SCENEVIEW_TOOLBAR
#elif UNITY_2020_1_OR_NEWER
#define EDITOR_TOOLBAR
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using SettingDelegate = System.Action;

namespace EnhancedEditor.Editor {
    // -------------------------------------------
    // Enhanced Settings
    // -------------------------------------------

    /// <summary>
    /// <see cref="EnhancedEditorProjectSettings"/>-related project settings base class.
    /// <br/> Inherit from this to create your own project settings.
    /// </summary>
    [Serializable]
    public abstract class EnhancedSettings {
        #region Global Members
        /// <summary>
        /// The unique identifier of this setting.
        /// </summary>
        [SerializeField, HideInInspector] public int GUID = 0;

        // -----------------------

        public EnhancedSettings(int _guid) {
            GUID = _guid;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedSettings"/> for a simple <see cref="bool"/>.
    /// </summary>
    [Serializable]
    public class BooleanEnhancedSettings : EnhancedSettings {
        #region Global Members
        /// <summary>
        /// This setting value.
        /// </summary>
        public bool Value =true;

        // -----------------------

        /// <inheritdoc cref="BooleanEnhancedSettings"/>
        public BooleanEnhancedSettings(int _guid, bool _value) : base(_guid) {
            Value = _value;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedSettings"/> for a simple <see cref="float"/>.
    /// </summary>
    [Serializable]
    public class FloatEnhancedSettings : EnhancedSettings {
        #region Global Members
        /// <summary>
        /// This setting value.
        /// </summary>
        public float Value = 0f;

        // -----------------------

        /// <inheritdoc cref="BooleanEnhancedSettings"/>
        public FloatEnhancedSettings(int _guid, float _value) : base(_guid) {
            Value = _value;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedSettings"/> for a folder path.
    /// </summary>
    [Serializable]
    public class FolderEnhancedSettings : EnhancedSettings {
        #region Global Members
        /// <summary>
        /// Default folder value.
        /// </summary>
        public string DefaultValue = string.Empty;

        /// <summary>
        /// If true, restricts this folder to this project only.
        /// </summary>
        public bool InProjectOnly = true;

        /// <summary>
        /// This folder value.
        /// </summary>
        public string Folder = string.Empty;

        // -----------------------

        /// <inheritdoc cref="FolderEnhancedSettings"/>
        public FolderEnhancedSettings(int _guid, string _defaultValue, bool _inProjectOnly) : base(_guid) {
            DefaultValue = _defaultValue;
            InProjectOnly = _inProjectOnly;

            Folder = _defaultValue;
        }
        #endregion
    }

    // -------------------------------------------
    // Editor settings class
    // -------------------------------------------

    /// <summary>
    /// <see cref="EnhancedEditor"/> base settings class, managing both project and user settings.
    /// </summary>
    public abstract class EnhancedEditorSettings : ScriptableObject {
        #region Styles
        internal static class Styles {
            public static readonly GUIContent UserSettingsButtonGUI     = new GUIContent(EditorGUIUtility.FindTexture("d_Settings"),
                                                                                         "Opens the EnhancedEditor user preferences.");

            public static readonly GUIContent ProjectSettingsButtonGUI  = new GUIContent(EditorGUIUtility.FindTexture("d_Settings"),
                                                                                         "Opens the EnhancedEditor project settings.");
        }
        #endregion

        #region Global Members
        /// <inheritdoc cref="EnhancedEditorUserSettings.Instance"/>
        public static EnhancedEditorUserSettings UserSettings {
            get { return EnhancedEditorUserSettings.Instance; }
        }

        /// <inheritdoc cref="EnhancedEditorProjectSettings.Instance"/>
        public static EnhancedEditorProjectSettings ProjectSettings {
            get { return EnhancedEditorProjectSettings.Instance; }
        }

        // -----------------------

        [SerializeReference] internal EnhancedSettings[] settings = new EnhancedSettings[] { };
        #endregion

        #region Behaviour
        private SerializedObject serializedObject = null;

        public SerializedObject SerializedObject {
            get {
                if ((serializedObject == null) || (serializedObject.targetObject == null)) {
                    serializedObject = new SerializedObject(this);
                }

                return serializedObject;
            }
        }

        // -----------------------

        /// <summary>
        /// Get the setting associated with a specific guid.
        /// </summary>
        /// <param name="_guid">The guid to get the associated setting.</param>
        /// <param name="_setting">The setting associated with the guid.</param>
        /// <param name="_property">The <see cref="SerializedProperty"/> associated with this setting.</param>
        /// <returns>True if the associated setting could be found, false otherwise.</returns>
        public virtual bool GetSetting<T>(int _guid, out T _setting, out SerializedProperty _property) where T : EnhancedSettings {
            // References to the SerializedProperty may be lost on undo
            // (because of the SerializedReference attribute), so register a callback on it.
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            int _index = Array.FindIndex(settings, s => s.GUID == _guid);

            if ((_index != -1) && (settings[_index] is T _temp)) {
                _setting = _temp;
                _property = SerializedObject.FindProperty("settings").GetArrayElementAtIndex(_index);

                return true;
            }

            _setting = null;
            _property = null;

            return false;
        }

        /// <summary>
        /// Adds a new setting to these project settings.
        /// </summary>
        /// <param name="_setting">The new setting to add.</param>
        public virtual SerializedProperty AddSetting(EnhancedSettings _setting) {
            ArrayUtility.Add(ref settings, _setting);

            SerializedObject _object = SerializedObject;

            _object.ApplyModifiedPropertiesWithoutUndo();
            _object.Update();

            SerializedProperty _property = _object.FindProperty("settings");
            return _property.GetArrayElementAtIndex(_property.arraySize - 1);
        }

        /// <summary>
        /// Saves these settings.
        /// </summary>
        public abstract void Save();

        // -----------------------

        private void OnUndoRedoPerformed() {
            serializedObject = null;
        }
        #endregion

        #region Menu Navigation
        public const string MenuPath = InternalUtility.MenuItemPath + "Settings/";

        // -----------------------

        /// <summary>
        /// Draws a button to open the Enhanced Editor user settings window.
        /// </summary>
        public static void DrawUserSettingsButton(Rect _position) {
            if (EnhancedEditorGUI.IconButton(_position, Styles.UserSettingsButtonGUI)) {
                EnhancedEditorUserSettings.OpenSettings();
            }
        }

        /// <summary>
        /// Draws a button to open the Enhanced Editor project settings window.
        /// </summary>
        public static void DrawProjectSettingsButton(Rect _position) {
            if (EnhancedEditorGUI.IconButton(_position, Styles.ProjectSettingsButtonGUI)) {
                EnhancedEditorProjectSettings.OpenSettings();
            }
        }
        #endregion

        #region Settings Providers
        public const string UserSettingsPath        = "Preferences/Enhanced Engine";
        public const string ProjectSettingsPath     = "Project/Enhanced Engine";

        public const string UserSettingsGlobalLabel = "Enhanced Engine";
        public const string ProjectSettingsLabel    = "Enhanced Engine";

        public static readonly string[] UserSettingsKeywords    = new string[] {
                                                                    "Enhanced",
                                                                    "Engine",
                                                                    "Editor",
                                                                    "Framework",
                                                                    "Autosave",
                                                                    "Build Pipeline",
                                                                    "Chronos",
                                                                    "Scene Designer",
                                                                };


        public static readonly string[] ProjectSettingsKeywords = new string[] {
                                                                    "Enhanced",
                                                                    "Engine",
                                                                    "Editor",
                                                                    "Framework",
                                                                    "Core Scene",
                                                                    "Resource",
                                                                    "Instance Tracker",
                                                                    "Script Template",
                                                                };

        // -----------------------

        [SettingsProvider]
        private static SettingsProvider CreateUserSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(UserSettingsPath, SettingsScope.User) {
                label = UserSettingsGlobalLabel,
                keywords = UserSettingsKeywords,

                guiHandler = (string _searchContext) => {
                    GUILayout.Space(10f);
                    DrawUserSettings();
                },
            };

            return _provider;
        }

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(ProjectSettingsPath, SettingsScope.Project) {
                label = ProjectSettingsLabel,
                keywords = ProjectSettingsKeywords,

                guiHandler = (string _searchContext) => {
                    GUILayout.Space(10f);
                    DrawProjectSettings();
                },
            };

            return _provider;
        }
        #endregion

        #region GUI Draw
        public const string ProjectSettingsRecordTitle  = "EnhancedEditor Project Settings Change";
        public const string UserSettingsRecordTitle     = "EnhancedEditor User Settings Change";
        public const string EditorPrefsPath             = "EnhancedEditor/";

        private static readonly List<SettingDelegate> userSettingsDrawers = new List<SettingDelegate>();
        private static readonly List<SettingDelegate> projectSettingsDrawers = new List<SettingDelegate>();

        private static readonly GUIContent shortcutsGUI = new GUIContent("Editor Shortcuts", "Edit all Enhanced Editor-related editor shortcuts.");

        // -----------------------

        public static void DrawUserSettings() {
            // Load drawers.
            GetDelegates<EnhancedEditorUserSettingsAttribute>(userSettingsDrawers);
            EnhancedEditorUserSettings _settings = UserSettings;

            // Draw preferences.
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);
                
                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    Undo.RecordObject(_settings, UserSettingsRecordTitle);
                    _settings.SerializedObject.Update();

                    foreach (SettingDelegate _delegate in userSettingsDrawers) {
                        _delegate();
                    }

                    _settings.SerializedObject.ApplyModifiedProperties();

                    if (_changeCheck.changed) {
                        _settings.Save();
                    }

                    #if UNITY_2019_1_OR_NEWER
                    GUILayout.Space(10f);

                    // Editor shortcuts.
                    using (var _horizontalScope = new GUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(shortcutsGUI, GUILayout.MaxWidth(120f))) {
                            EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
                        }
                    }
                    #endif
                }
            }
        }

        public static void DrawProjectSettings() {
            // Load drawers.
            GetDelegates<EnhancedEditorProjectSettingsAttribute>(projectSettingsDrawers);
            EnhancedEditorProjectSettings _settings = ProjectSettings;

            // Draw project settings.
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    Undo.RecordObject(_settings, ProjectSettingsRecordTitle);
                    _settings.SerializedObject.Update();

                    foreach (SettingDelegate _delegate in projectSettingsDrawers) {
                        _delegate();
                    }

                    _settings.SerializedObject.ApplyModifiedProperties();

                    if (_changeCheck.changed) {
                        _settings.Save();
                    }
                }
            }
        }

        // -----------------------

        private static void GetDelegates<T>(List<SettingDelegate> _delegates) where T : EnhancedEditorSettingsAttribute {
            if (_delegates.Count != 0) {
                return;
            }

            var _methods = TypeCache.GetMethodsWithAttribute<T>();
            List<MethodInfo> _drawers = new List<MethodInfo>();

            foreach (var _method in _methods) {
                if (_method.IsStatic && (_method.GetParameters().Length == 0)) {
                    _drawers.Add(_method);
                }
            }

            _drawers.Sort((a, b) => {
                return a.GetCustomAttribute<T>().Order.CompareTo(b.GetCustomAttribute<T>().Order);
            });

            foreach (MethodInfo _drawer in _drawers) {
                _delegates.Add(_drawer.CreateDelegate(typeof(SettingDelegate)) as SettingDelegate);
            }
        }
        #endregion
    }
}
