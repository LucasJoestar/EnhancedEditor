// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

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
            /// <summary>
            /// The directory where to build and look for existing builds of the game from the <see cref="BuildPipelineWindow"/>.
            /// </summary>
            public string BuildDirectory = string.Empty;

            /// <summary>
            /// Time interval (in seconds) between two autosave.
            /// <br/> Autosave can be toggled from the main editor toolbar.
            /// </summary>
            public int AutosaveInterval = AutosaveDefaultInterval;
        }

        // -----------------------

        /// <inheritdoc cref="EditorUserSettings"/>
        [NonSerialized] public EditorUserSettings UserSettings = new EditorUserSettings();

        /// <summary>
        /// The directory in the project where are created all auto-managed resources.
        /// </summary>
        public string AutoManagedResourceDirectory = AutoManagedResourceDefaultDirectory;

        /// <summary>
        /// The directory in the project where are created all instance trackers (must be in an Editor folder).
        /// </summary>
        public string InstanceTrackerDirectory = InstanceTrackerDefaultDirectory;
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

        public void SaveSettings()
        {
            // Use EditorPrefs for user-dependant settings.
            string _data = JsonUtility.ToJson(UserSettings);

            EditorPrefs.SetString(PrefsKey, _data);
            EditorUtility.SetDirty(this);

            EditorAutosave.SetSaveInterval(UserSettings.AutosaveInterval);
        }

        private void OnEnable()
        {
            // Load and initialize settings.
            string _data = EditorPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(_data))
            {
                JsonUtility.FromJsonOverwrite(_data, UserSettings);
            }

            if (string.IsNullOrEmpty(UserSettings.BuildDirectory))
            {
                UserSettings.BuildDirectory = BuildDefaultDirectory;
                SaveSettings();
            }
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
                    EnhancedEditorSettings _settings = Settings;
                    GUILayout.Space(10f);

                    using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        DrawPreferences(_settings);

                        if (_changeCheck.changed)
                            _settings.SaveSettings();
                    }
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
        private const string BuildDirectoryPanelTitle = "Build Directory";

        private static readonly GUIContent localHeader = new GUIContent("User Settings:", "User-dependant settings.");
        private static readonly GUIContent globalHeader = new GUIContent("Global", "Project global settings, shared between users.");

        private static readonly GUIContent autoManagedResourceDirectoryGUI = new GUIContent("Managed Resource Dir.",
                                                                                           "Directory in the project where are created all auto-managed resources.");

        private static readonly GUIContent instanceTrackerDirectoryGUI = new GUIContent("Instance Tracker Dir.",
                                                                                       "Directory in the project where are created all instance trackers.");

        private static readonly GUIContent buildDirectoryGUI = new GUIContent("Build Directory",
                                                                             "Directory where to build and look for existing builds of the game from the BuildPipelineWindow.");

        private static readonly GUIContent autosaveIntervalGUI = new GUIContent("Autosave Interval",
                                                                               "Time interval (in seconds) between two autosave. Autosave can be toggled from the main editor toolbar.");

        // -----------------------

        private static void DrawPreferences(EnhancedEditorSettings _preferences)
        {
            Undo.RecordObject(_preferences, UndoRecordTitle);

            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(10f);
                using (var _verticalScope = new GUILayout.VerticalScope())
                {
                    DrawUserSettings(_preferences.UserSettings);

                    GUILayout.Space(15f);

                    DrawGlobalSettings(_preferences);
                }
            }
        }

        private static void DrawUserSettings(EditorUserSettings _settings)
        {
            EnhancedEditorGUILayout.UnderlinedLabel(localHeader, EditorStyles.boldLabel);
            GUILayout.Space(2f);

            using (var _scope = new EditorGUI.IndentLevelScope())
            {
                // Build dirctory.
                _settings.BuildDirectory = EnhancedEditorGUILayout.FolderField(buildDirectoryGUI, _settings.BuildDirectory, true, BuildDirectoryPanelTitle);

                // Autosave interval.
                _settings.AutosaveInterval = EnhancedEditorGUILayout.MinField(autosaveIntervalGUI, _settings.AutosaveInterval, 5);
            }
        }

        private static void DrawGlobalSettings(EnhancedEditorSettings _preferences)
        {
            EditorGUILayout.LabelField(globalHeader, EditorStyles.boldLabel);

            // Auto-managed resource directory.
            _preferences.AutoManagedResourceDirectory = EnhancedEditorGUILayout.FolderField(autoManagedResourceDirectoryGUI,
                                                                                            _preferences.AutoManagedResourceDirectory, false,
                                                                                            AutoManagedResourceDirectoryPanelTitle);

            // Instance trackers dirctory.
            _preferences.InstanceTrackerDirectory = EnhancedEditorGUILayout.EditorFolderField(instanceTrackerDirectoryGUI,
                                                                                              _preferences.InstanceTrackerDirectory,
                                                                                              InstanceTrackerDirectoryPanelTitle);
        }
        #endregion
    }
}
