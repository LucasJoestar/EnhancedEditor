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
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using SettingDelegate = System.Action;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="EnhancedEditorSettings"/>-related project settings base class.
    /// <br/> Inherit from this to create your own project settings.
    /// </summary>
    [Serializable]
    public abstract class EnhancedEditorProjectSetting {
        #region Global Members
        /// <summary>
        /// The unique identifier of this setting.
        /// </summary>
        public int GUID = 0;

        // -----------------------

        public EnhancedEditorProjectSetting(int _guid) {
            GUID = _guid;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedEditorProjectSetting"/> for a simple <see cref="bool"/>.
    /// </summary>
    [Serializable]
    public class BooleanProjectSetting : EnhancedEditorProjectSetting {
        #region Global Members
        /// <summary>
        /// This setting value.
        /// </summary>
        public bool Value =true;

        // -----------------------

        /// <inheritdoc cref="BooleanProjectSetting"/>
        public BooleanProjectSetting(int _guid, bool _value) : base(_guid) {
            Value = _value;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedEditorProjectSetting"/> for a folder path.
    /// </summary>
    [Serializable]
    public class FolderProjectSetting : EnhancedEditorProjectSetting {
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

        /// <inheritdoc cref="FolderProjectSetting"/>
        public FolderProjectSetting(int _guid, string _defaultValue, bool _inProjectOnly) : base(_guid) {
            DefaultValue = _defaultValue;
            InProjectOnly = _inProjectOnly;

            Folder = _defaultValue;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedEditor"/>-related preferences settings.
    /// <para/>
    /// Note that these settings should not be called from a <see cref="ScriptableObject"/> constructor, due to Unity preferences limitations.
    /// </summary>
    [NonEditable("Please use the Preferences/Project Settings windows to edit these settings.")]
    #pragma warning disable IDE0051
    public class EnhancedEditorSettings : ScriptableObject {
        #region Styles
        private static class Styles {
            public static readonly GUIContent PreferencesButtonGUI = new GUIContent(EditorGUIUtility.FindTexture("d_Settings"), "Opens the EnhancedEditor user preferences.");
            public static readonly GUIContent ProjectSettingsButtonGUI = new GUIContent(EditorGUIUtility.FindTexture("d_Settings"), "Opens the EnhancedEditor project settings.");
        }
        #endregion

        #region Global Members
        private const string DefaultSettingsDirectory = "EnhancedEditor/Editor/Settings/";

        private static EnhancedEditorSettings settings = null;

        /// <inheritdoc cref="EnhancedEditorSettings"/>
        public static EnhancedEditorSettings Settings {
            get {
                if ((settings == null) && !EnhancedEditorUtility.LoadMainAsset(out settings)) {
                    string _path = Path.Combine(Application.dataPath, DefaultSettingsDirectory);
                    if (!Directory.Exists(_path)) {
                        Directory.CreateDirectory(_path);
                    }

                    _path = Path.Combine("Assets", DefaultSettingsDirectory, $"EnhancedEditorSettings.asset");
                    settings = CreateInstance<EnhancedEditorSettings>();

                    AssetDatabase.CreateAsset(settings, _path);
                    AssetDatabase.SaveAssets();
                }

                return settings;
            }
        }

        [SerializeReference] private EnhancedEditorProjectSetting[] projectSettings = new EnhancedEditorProjectSetting[] { };
        #endregion

        #region Settings Utility
        /// <summary>
        /// Get the setting associated with a specific guid.
        /// </summary>
        /// <param name="_guid">The guid to get the associated setting.</param>
        /// <param name="_setting">The setting associated with the guid.</param>
        /// <returns>True if the associated setting could be found, false otherwise.</returns>
        public bool GetProjectSetting<T>(int _guid, out T _setting) where T : EnhancedEditorProjectSetting {
            int _index = Array.FindIndex(projectSettings, s => s.GUID == _guid);

            if ((_index != -1) && (projectSettings[_index] is T _temp)) {
                _setting = _temp;
                return true;
            }

            _setting = null;
            return false;
        }

        /// <summary>
        /// Adds a new project setting to these settings.
        /// </summary>
        /// <param name="_setting">The new project setting to add.</param>
        public void AddProjectSetting(EnhancedEditorProjectSetting _setting) {
            ArrayUtility.Add(ref projectSettings, _setting);
        }
        #endregion

        #region Menu Navigation
        public const string MenuPath = InternalUtility.MenuItemPath + "Settings/";

        // -----------------------

        /// <summary>
        /// Opens the Preferences window with the <see cref="EnhancedEditor"/> settings already selected.
        /// </summary>
        [MenuItem(MenuPath + "Preferences", false, -50), Button(SuperColor.Green, IsDrawnOnTop = false)]
        public static EditorWindow OpenPreferences() {
            EditorWindow _preferences = SettingsService.OpenUserPreferences(PreferencesPath);
            return _preferences;
        }

        /// <summary>
        /// Opens the Project Settings window with the <see cref="EnhancedEditor"/> settings already selected.
        /// </summary>
        [MenuItem(MenuPath + "Project Settings", false, -50), Button(SuperColor.Green, IsDrawnOnTop = false)]
        public static EditorWindow OpenProjectSettings() {
            EditorWindow _projectSettings = SettingsService.OpenProjectSettings(ProjectSettingsPath);
            return _projectSettings;
        }

        /// <summary>
        /// Main editor toolbar extension used to open the <see cref="EnhancedEditor"/> preferences settings.
        /// </summary>
        [EditorToolbarRightExtension(Order = 500)]
        private static void OpenPreferencesToolbarExtension() {
            #if SCENEVIEW_TOOLBAR
            GUILayout.Space(10f);
            #elif EDITOR_TOOLBAR
            GUILayout.FlexibleSpace();
            #endif

            if (EnhancedEditorToolbar.Button(Styles.PreferencesButtonGUI, GUILayout.Width(32f))) {
                OpenPreferences();
            }

            #if EDITOR_TOOLBAR
            GUILayout.Space(25f);
            #endif
        }

        // -----------------------

        /// <summary>
        /// Draws a button to open the Enhanced Editor preferences window.
        /// </summary>
        public static void DrawPreferencesButton(Rect _position) {
            if (EnhancedEditorGUI.IconButton(_position, Styles.PreferencesButtonGUI)) {
                OpenPreferences();
            }
        }

        /// <summary>
        /// Draws a button to open the Enhanced Editor project settings window.
        /// </summary>
        public static void DrawProjectSettingsButton(Rect _position) {
            if (EnhancedEditorGUI.IconButton(_position, Styles.ProjectSettingsButtonGUI)) {
                OpenPreferences();
            }
        }
        #endregion

        #region Settings Providers
        public const string ProjectSettingsPath = "Project/Enhanced Editor";
        public const string PreferencesPath = "Preferences/Enhanced Editor";

        public const string ProjectSettingsLabel = "Enhanced Editor";
        public const string PreferencesGlobalLabel = "Enhanced Editor";

        public static readonly string[] PreferencesKeywords = new string[] {
                                                                "Enhanced",
                                                                "Editor",
                                                                "Autosave",
                                                                "Build Pipeline",
                                                                "Chronos",
                                                                "Scene Designer",
                                                            };


        public static readonly string[] ProjectSettingsKeywords = new string[] {
                                                                    "Enhanced",
                                                                    "Editor",
                                                                    "Core Scene",
                                                                    "Resource",
                                                                    "Instance Tracker",
                                                                    "Script Template",
                                                                };

        // -----------------------

        [SettingsProvider]
        private static SettingsProvider CreateUserSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(PreferencesPath, SettingsScope.User) {
                label = PreferencesGlobalLabel,
                keywords = PreferencesKeywords,

                guiHandler = (string _searchContext) => {
                    GUILayout.Space(10f);
                    DrawUserPreferences();
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
        public const string ProjectSettingsRecordTitle = "EnhancedEditor Settings Change";
        public const string EditorPrefsPath = "EnhancedEditor/";

        private static List<SettingDelegate> preferencesDrawers = new List<SettingDelegate>();
        private static List<SettingDelegate> projectSettingsDrawers = new List<SettingDelegate>();

        private static readonly GUIContent shortcutsGUI = new GUIContent("Editor Shortcuts", "Edit all Enhanced Editor-related editor shortcuts.");

        // -----------------------

        public static void DrawUserPreferences() {
            // Load drawers.
            GetDelegates<EnhancedEditorPreferencesAttribute>(preferencesDrawers);

            // Draw preferences.
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);

                using (var _verticalScope = new GUILayout.VerticalScope()) {
                    foreach (SettingDelegate _delegate in preferencesDrawers) {
                        _delegate();
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

            EnhancedEditorSettings _settings = Settings;

            // Draw project settings.
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(10f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    Undo.RecordObject(_settings, ProjectSettingsRecordTitle);

                    foreach (SettingDelegate _delegate in projectSettingsDrawers) {
                        _delegate();
                    }

                    if (_changeCheck.changed) {
                        EnhancedEditorUtility.SaveAsset(_settings);
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
