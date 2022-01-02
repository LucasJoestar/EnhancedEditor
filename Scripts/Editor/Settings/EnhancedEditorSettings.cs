// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="EnhancedEditor"/>-related preferences settings.
    /// <para/>
    /// Note that these settings should not be called from a <see cref="ScriptableObject"/> constructor, due to Unity preferences limitations.
    /// </summary>
    [NonEditable("Please use the Preferences window to edit these settings.")]
	public class EnhancedEditorSettings : ScriptableObject
    {
        #region Settings Data
        public const string AutoManagedResourceDefaultDirectory = "EnhancedEditor/AutoManagedResources";
        public const string InstanceTrackerDefaultDirectory = "EnhancedEditor/Editor/InstanceTrackers";
        public const string ScriptTemplateDefaultDirectory = "EnhancedEditor/Editor/ScriptTemplates";
        public const int AutosaveDefaultInterval = 300;

        public static string BuildDefaultDirectory
        {
            get
            {
                string _directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds");
                return _directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        /// <summary>
        /// User-dependant <see cref="EnhancedEditor"/> settings.
        /// </summary>
        [Serializable]
        public class EditorUserSettings
        {
            [SerializeField] private string buildDirectory = string.Empty;

            /// <summary>
            /// The directory where to build and look for existing builds of the game from the <see cref="BuildPipelineWindow"/>.
            /// </summary>
            public string BuildDirectory
            {
                get
                {
                    if (string.IsNullOrEmpty(buildDirectory))
                    {
                        buildDirectory = BuildDefaultDirectory;
                    }

                    return buildDirectory;
                }
                set
                {
                    buildDirectory = value;
                }
            }

            /// <summary>
            /// Time interval (in seconds) between two autosave.
            /// <br/> Autosave can be toggled from the main editor toolbar.
            /// </summary>
            public int AutosaveInterval = AutosaveDefaultInterval;

            #if ENABLE_INPUT_SYSTEM
            // Chronos-related inputs.
            [SerializeField] internal InputAction increaseTimeScale = new InputAction();
            [SerializeField] internal InputAction resetTimeScale = new InputAction();
            [SerializeField] internal InputAction decreaseTimeScale = new InputAction();
            #endif
        }

        // -----------------------

        /// <inheritdoc cref="EditorUserSettings"/>
        public EditorUserSettings UserSettings = new EditorUserSettings();

        /// <summary>
        /// The directory in the project where are created all auto-managed resources.
        /// </summary>
        public string AutoManagedResourceDirectory = AutoManagedResourceDefaultDirectory;

        /// <summary>
        /// The directory in the project where are created all instance trackers (must be in an Editor folder).
        /// </summary>
        public string InstanceTrackerDirectory = InstanceTrackerDefaultDirectory;

        /// <summary>
        /// The directory in the project where are stored all script templates.
        /// </summary>
        public string ScriptTemplateDirectory = ScriptTemplateDefaultDirectory;

        /// <summary>
        /// All folders used to select objects to place in the scene using the <see cref="SceneDesigner"/>.
        /// </summary>
        [Folder] public string[] SceneDesignerFolders = new string[] { };

        /// <summary>
        /// The core scene to load when entering play mode.
        /// <br/> Works only in editor.
        /// </summary>
        public SceneAsset CoreScene = new SceneAsset();

        /// <summary>
        /// Indicates whether the core scene is enabled and should be loaded when entering play mode.
        /// </summary>
        public bool IsCoreSceneEnabled = false;
        #endregion

        #region Behaviour
        private const string DefaultSettingsDirectory = "EnhancedEditor/Editor/Settings/";
        private const string PrefsKey = "EnhancedEditorPreferences";

        private static EnhancedEditorSettings settings = null;

        /// <inheritdoc cref="EnhancedEditorSettings"/>
        public static EnhancedEditorSettings Settings
        {
            get
            {
                if ((settings == null) && !EnhancedEditorUtility.LoadMainAsset(out settings))
                {
                    string _path = Path.Combine(Application.dataPath, DefaultSettingsDirectory);
                    if (!Directory.Exists(_path))
                        Directory.CreateDirectory(_path);

                    _path = Path.Combine("Assets", DefaultSettingsDirectory, $"EnhancedEditorSettings.asset");
                    settings = CreateInstance<EnhancedEditorSettings>();

                    AssetDatabase.CreateAsset(settings, _path);
                    AssetDatabase.SaveAssets();
                }

                return settings;
            }
        }

        // -----------------------

        private void OnEnable()
        {
            // Load and initialize settings.
            string _data = EditorPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(_data))
            {
                JsonUtility.FromJsonOverwrite(_data, UserSettings);
            }
            else
            {
                UserSettings = new EditorUserSettings();

                #if ENABLE_INPUT_SYSTEM
                UserSettings.increaseTimeScale.AddBinding(Keyboard.current.numpadPlusKey);
                UserSettings.resetTimeScale.AddBinding(Keyboard.current.numpadMultiplyKey);
                UserSettings.decreaseTimeScale.AddBinding(Keyboard.current.numpadMinusKey);
                #endif
            }

            if (string.IsNullOrEmpty(UserSettings.BuildDirectory))
            {
                UserSettings.BuildDirectory = BuildDefaultDirectory;
                SaveSettings();
            }
        }

        private void OnValidate()
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            // Use EditorPrefs for user-dependant settings.
            string _data = JsonUtility.ToJson(UserSettings);

            EditorPrefs.SetString(PrefsKey, _data);
            EditorUtility.SetDirty(this);

            EditorAutosave.SetSaveInterval(UserSettings.AutosaveInterval);

            #if ENABLE_INPUT_SYSTEM
            // Save chronos inputs.
            PlayerPrefs.SetString(Chronos.IncreaseInputKey, JsonUtility.ToJson(UserSettings.increaseTimeScale));
            PlayerPrefs.SetString(Chronos.ResetInputKey, JsonUtility.ToJson(UserSettings.resetTimeScale));
            PlayerPrefs.SetString(Chronos.DecreaseInputKey, JsonUtility.ToJson(UserSettings.decreaseTimeScale));
            #endif
        }
        #endregion

        #region Settings Provider
        public const string PreferencesPath = "Preferences/EnhancedEditor";
        public const string PreferencesLabel = "Enhanced Editor";
        public static readonly string[] Keywords = new string[]
                                                        {
                                                            "Enhanced",
                                                            "Editor",
                                                            "Build Pipeline",
                                                            "Resource",
                                                            "Instance Tracker",
                                                            "Autosave"
                                                        };

        // -----------------------

        [SettingsProvider]
        #pragma warning disable IDE0051
        private static SettingsProvider CreateSettingsProvider()
        {
            SettingsProvider _provider = new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                label = PreferencesLabel,
                keywords = Keywords,
                
                guiHandler = (string _searchContext) =>
                {
                    GUILayout.Space(10f);

                    EnhancedEditorSettings _settings = Settings;
                    DrawPreferences(_settings);
                },
            };

            return _provider;
        }
        #endregion

        #region Menu Navigation
        private static readonly GUIContent toolbarButtonGUI = new GUIContent(string.Empty, "Opens the EnhancedEditor preferences settings.");

        // -----------------------

        /// <summary>
        /// Opens the Preferences window with the <see cref="EnhancedEditor"/> settings already selected.
        /// </summary>
        [MenuItem(InternalUtility.MenuItemPath + "Preferences", false, -50), Button(SuperColor.Green, IsDrawnOnTop = false)]
        public static EditorWindow OpenPreferencesSettings()
        {
            EditorWindow _preferences = SettingsService.OpenUserPreferences(PreferencesPath);
            return _preferences;
        }

        /// <summary>
        /// Main editor toolbar extension used to open the <see cref="EnhancedEditor"/> preferences settings.
        /// </summary>
        [EditorToolbarRightExtension(Order = 500)]
        #pragma warning disable IDE0051
        private static void OpenPreferencesToolbarExtension()
        {
            if (toolbarButtonGUI.image == null)
                toolbarButtonGUI.image = EditorGUIUtility.FindTexture("d_Settings");

            GUILayout.FlexibleSpace();
            if (EnhancedEditorToolbar.Button(toolbarButtonGUI, GUILayout.Width(32f)))
            {
                OpenPreferencesSettings();
            }

            GUILayout.Space(25f);
        }
        #endregion

        #region GUI Draw
        private const string UndoRecordTitle = "EnhancedEditor Settings Change";

        private const string AutoManagedResourceDirectoryPanelTitle = "Auto-Managed Resources Default Directory";
        private const string InstanceTrackerDirectoryPanelTitle = "Instance Trackers Directory";
        private const string ScriptTemplateDirectoryPanelTitle = "Script Templates Directory";
        private const string BuildDirectoryPanelTitle = "Build Directory";

        private const string CoreSceneMessage = "The Core Scene system allows to always load a specific scene first when entering play mode in the editor.";

        private static readonly GUIContent localHeaderGUI = new GUIContent("User Settings:", "User-dependant settings.");
        private static readonly GUIContent globalHeaderGUI = new GUIContent("Global Settings:", "Project global settings, shared between users.");
        private static readonly GUIContent coreSceneHeaderGUI = new GUIContent("Core Scene System", "Settings related to the Core Scene system.");
        private static readonly GUIContent chronosHeaderGUI = new GUIContent("Chronos Runtime Inputs", "Input shortcuts related to the chronos tool.");
        private static readonly GUIContent shortcutsGUI = new GUIContent("Editor Shortcuts", "Edit all Enhanced Editor-related editor shortcuts.");

        private static readonly GUIContent autoManagedResourceDirectoryGUI = new GUIContent("Managed Resource Dir.",
                                                                                            "Directory in the project where are created all auto-managed resources.");

        private static readonly GUIContent instanceTrackerDirectoryGUI = new GUIContent("Instance Tracker Dir.",
                                                                                        "Directory in the project where are created all instance trackers.");

        private static readonly GUIContent scriptTemplateDirectoryGUI = new GUIContent("Script Template Dir.",
                                                                                       "Directory in the project where are stored all script templates.");

        private static readonly GUIContent buildDirectoryGUI = new GUIContent("Build Directory",
                                                                              "Directory where to build and look for existing builds of the game from the BuildPipelineWindow.");

        private static readonly GUIContent autosaveIntervalGUI = new GUIContent("Autosave Interval",
                                                                                "Time interval (in seconds) between two autosave. Autosave can be toggled from the main editor toolbar.");

        private static readonly GUIContent sceneDesignerFoldersGUI = new GUIContent("Scene Designer Folders",
                                                                                    "All folders displayed to select objects to place in the scene using the Scene Designer.");

        private static readonly GUIContent coreSceneGUI = new GUIContent("Core Scene", "The core scene to load when entering play mode.");
        private static readonly GUIContent isCoreSceneEnabledGUI = new GUIContent("Enabled", "Enables / Disables to core scene system.");

        private static SerializedProperty sceneDesignerFoldersProperty = null;

        #if ENABLE_INPUT_SYSTEM
        private static SerializedProperty increaseTimeScaleProperty = null;
        private static SerializedProperty resetTimeScaleProperty = null;
        private static SerializedProperty decreaseTimeScaleProperty = null;
        #endif

        // -----------------------

        private static void DrawPreferences(EnhancedEditorSettings _settings)
        {
            Undo.RecordObject(_settings, UndoRecordTitle);

            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(7f);
                using (var _verticalScope = new GUILayout.VerticalScope())
                {
                    DrawUserSettings(_settings.UserSettings);

                    GUILayout.Space(15f);
                    DrawGlobalSettings(_settings);
                }
            }
        }

        private static void DrawUserSettings(EditorUserSettings _settings)
        {
            EnhancedEditorGUILayout.UnderlinedLabel(localHeaderGUI, EditorStyles.boldLabel);
            GUILayout.Space(2f);

            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(15f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                {

                    // Build dirctory.
                    _settings.BuildDirectory = EnhancedEditorGUILayout.FolderField(buildDirectoryGUI, _settings.BuildDirectory, true, BuildDirectoryPanelTitle);

                    // Autosave interval.
                    _settings.AutosaveInterval = EnhancedEditorGUILayout.MinField(autosaveIntervalGUI, _settings.AutosaveInterval, 5);

                    #if ENABLE_INPUT_SYSTEM
                    // Properties initialization.
                    if (increaseTimeScaleProperty == null)
                    {
                        SerializedObject _serializedObject = new SerializedObject(settings);
                        SerializedProperty _userSettings = _serializedObject.FindProperty("UserSettings");

                        increaseTimeScaleProperty = _userSettings.FindPropertyRelative("increaseTimeScale");
                        resetTimeScaleProperty = _userSettings.FindPropertyRelative("resetTimeScale");
                        decreaseTimeScaleProperty = _userSettings.FindPropertyRelative("decreaseTimeScale");
                    }

                    GUILayout.Space(10f);

                    // Chronos inputs.
                    EditorGUILayout.LabelField(chronosHeaderGUI, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(increaseTimeScaleProperty);
                    EditorGUILayout.PropertyField(resetTimeScaleProperty);
                    EditorGUILayout.PropertyField(decreaseTimeScaleProperty);
                    #endif

                    #if UNITY_2019_1_OR_NEWER
                    GUILayout.Space(5f);

                    // Editor shortcuts.
                    using (var _horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(shortcutsGUI, GUILayout.MaxWidth(120f)))
                        {
                            EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
                        }
                    }
                    #endif
                }
            }
        }

        private static void DrawGlobalSettings(EnhancedEditorSettings _settings)
        {
            EnhancedEditorGUILayout.UnderlinedLabel(globalHeaderGUI, EditorStyles.boldLabel);
            GUILayout.Space(3f);

            // Auto-managed resource directory.
            _settings.AutoManagedResourceDirectory = EnhancedEditorGUILayout.FolderField(autoManagedResourceDirectoryGUI,
                                                                                         _settings.AutoManagedResourceDirectory, false,
                                                                                         AutoManagedResourceDirectoryPanelTitle);

            // Instance trackers dirctory.
            _settings.InstanceTrackerDirectory = EnhancedEditorGUILayout.EditorFolderField(instanceTrackerDirectoryGUI,
                                                                                           _settings.InstanceTrackerDirectory,
                                                                                           InstanceTrackerDirectoryPanelTitle);

            // Script templates dirctory.
            _settings.ScriptTemplateDirectory = EnhancedEditorGUILayout.FolderField(scriptTemplateDirectoryGUI,
                                                                                    _settings.ScriptTemplateDirectory, false,
                                                                                    ScriptTemplateDirectoryPanelTitle);

            GUILayout.Space(5f);

            // Scene Designer folders.
            if (sceneDesignerFoldersProperty == null)
            {
                SerializedObject _serializedObject = new SerializedObject(settings);
                sceneDesignerFoldersProperty = _serializedObject.FindProperty("SceneDesignerFolders");
            }

            EditorGUILayout.PropertyField(sceneDesignerFoldersProperty);

            GUILayout.Space(15f);

            // Core scene system.
            EditorGUILayout.LabelField(coreSceneHeaderGUI, EditorStyles.boldLabel);

            EnhancedEditorGUILayout.SceneAssetField(coreSceneGUI, _settings.CoreScene);
            if (!string.IsNullOrEmpty(_settings.CoreScene.guid))
            {
                _settings.IsCoreSceneEnabled = EditorGUILayout.Toggle(isCoreSceneEnabledGUI, _settings.IsCoreSceneEnabled);
            }

            EditorGUILayout.HelpBox(CoreSceneMessage, UnityEditor.MessageType.Info);
        }
        #endregion
    }
}
