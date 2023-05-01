// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using LogColumnType = EnhancedEditor.Editor.EnhancedConsoleWindow.LogColumnType;
using LogType = EnhancedEditor.Editor.DefaultConsoleLogFilter.Type;
using LogEntry = EnhancedEditor.Editor.EnhancedConsoleWindow.OriginalLogEntry;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Wrapper used to ignore specific stack calls from a console log.
    /// </summary>
    [Serializable]
    public class ConsoleLogIgnoredStackCall {
        #region Global Members
        [Enhanced, Duo("UseNamespace", 20f)] public string Namespace = "Namespace";
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseNamespace = true;

        [Enhanced, Duo("UseClass", 20f)] public string Class = "Class";
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseClass = true;

        [Enhanced, Duo("UseMethod", 20f)] public string Method = "Method";
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseMethod = true;

        // -----------------------

        public ConsoleLogIgnoredStackCall() { }

        public ConsoleLogIgnoredStackCall(string _namespace, string _class, string _method, bool _useNamespace = true, bool _useClass = true, bool _useMethod = true) {
            Namespace = _namespace;
            Class = _class;
            Method = _method;

            UseNamespace = _useNamespace;
            UseClass = _useClass;
            UseMethod = _useMethod;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedConsoleWindow"/>-related <see cref="ScriptableObject"/> settings wrapper.
    /// <br/> Used to create a <see cref="SerializedObject"/> from it and draw its settings using a <see cref="SerializedProperty"/>.
    /// </summary>
    [Serializable]
    public class EnhancedConsoleEnhancedSettings : EnhancedSettings {
        #region Global Members
        [SerializeField, HideInInspector] internal int selectedTabIndex = 0;
        [SerializeField, HideInInspector] internal bool enabled = true;

        public BlockArray<LogColumnType> Columns = new BlockArray<LogColumnType>(false, true, true) {
            LogColumnType.Type,
            LogColumnType.Namespace,
            LogColumnType.Class,
            LogColumnType.Method,
            LogColumnType.Log,
            LogColumnType.Object,
            LogColumnType.File,

            LogColumnType.Frame,
            LogColumnType.Timestamp,
        };

        public BlockArray<CustomConsoleLogFilter> CustomFilters = new BlockArray<CustomConsoleLogFilter>() {
            new CustomConsoleLogFilter()
        };

        public BlockArray<DefaultConsoleLogFilter> DefaultFilters = new BlockArray<DefaultConsoleLogFilter>(false, false);

        public BlockArray<ConsoleLogIgnoredStackCall> IgnoredStackCalls = new BlockArray<ConsoleLogIgnoredStackCall>() {
            new ConsoleLogIgnoredStackCall("EnhancedEditor", "Debug", "Log", true, true, false),
            new ConsoleLogIgnoredStackCall("EnhancedEditor", "UnityObjectDebugLogger", "Log", true, true, false),

            new ConsoleLogIgnoredStackCall("UnityEngine", "Debug", "Log", true, true, false),
            new ConsoleLogIgnoredStackCall("UnityEngine", "Logger", "Log", true, true, false),
        };

        /// <summary>
        /// The total amount of configured filters.
        /// </summary>
        public int FilterCount {
            get { return CustomFilters.Count + DefaultFilters.Count; }
        }

        // -----------------------

        /// <inheritdoc cref="EnhancedConsoleEnhancedSettings"/>
        public EnhancedConsoleEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Behaviour
        private const int DefaultFilterCount = 7;

        // -----------------------

        private bool Initialize() {
            // Default filter initialization, used to correctly be able to call the Resources.Load method,
            // which can't be called from a ScriptableObject constructor.
            if (DefaultFilters.Count == DefaultFilterCount) {
                return false;
            }

            Array.Resize(ref DefaultFilters.Array, DefaultFilterCount);

            DefaultFilters[0] = new DefaultConsoleLogFilter(LogType.Log) {
                Name = "Log",
                Color = new Color(1f, 1f, 1f, .7f),
                UseColor = false,
                FilterIcon = ConsoleLogFilterIcon.Grey,
            };

            DefaultFilters[1] = new DefaultConsoleLogFilter(LogType.Warning) {
                Name = "Warning",
                Color = new Color(.5f, .45f, .1f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Pumpkin,
            };

            DefaultFilters[2] = new DefaultConsoleLogFilter(LogType.Error) {
                Name = "Error",
                Color = new Color(.7f, .15f, .15f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Strawberry,
            };

            DefaultFilters[3] = new DefaultConsoleLogFilter(LogType.Assert) {
                Name = "Assert",
                Color = new Color(.8f, .5f, .2f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Peach,
            };

            DefaultFilters[4] = new DefaultConsoleLogFilter(LogType.Exception) {
                Name = "Exception",
                Color = new Color(.75f, .2f, .7f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Grape,
            };

            DefaultFilters[5] = new DefaultConsoleLogFilter(LogType.External) {
                Name = "External",
                Color = new Color(.25f, .5f, .7f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Blueberry,
            };

            DefaultFilters[6] = new DefaultConsoleLogFilter(LogType.Compilation) {
                Name = "Compilation",
                Color = new Color(.7f, .25f, .4f, .7f),
                UseColor = true,
                FilterIcon = ConsoleLogFilterIcon.Radish,
            };

            return true;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if a specific stack call should be ignored.
        /// </summary>
        public bool IgnoreCall(string _namespace, string _class, string _method) {
            foreach (ConsoleLogIgnoredStackCall _ignore in IgnoredStackCalls) {
                if ((_ignore.UseNamespace   || _ignore.UseClass || _ignore.UseMethod)
                 && (!_ignore.UseNamespace  || _namespace.Contains(_ignore.Namespace))
                 && (!_ignore.UseClass      || _class.Contains(_ignore.Class))
                 && (!_ignore.UseMethod     || _method.Contains(_ignore.Method))) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the log filter at the given index.
        /// <br/> You can use <see cref="FilterCount"/> to get the total amount of defined filters.
        /// </summary>
        /// <param name="_index">The index to get the filter at.</param>
        /// <returns>The log filter at the given index.</returns>
        public ConsoleLogFilter GetFilterAt(int _index) {
            if (_index < CustomFilters.Count) {
                return CustomFilters[_index];
            }

            _index -= CustomFilters.Count;
            return DefaultFilters[_index];
        }

        /// <summary>
        /// Get the best matching <see cref="ConsoleLogFilter"/> for a specific <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="_entry">The log to get the associated filter.</param>
        /// <returns>The best matching filter for this log.</returns>
        internal ConsoleLogFilter GetBestFilter(LogEntry _entry) {
            // Search in all custom filters first for the first matching one.
            foreach (CustomConsoleLogFilter _filter in CustomFilters) {
                if (_filter.Match(_entry)) {
                    return _filter;
                }
            }

            // Then only in the default filters (use a reverse loop to get external types first).
            for (int i = DefaultFilters.Count; i-- > 1;) {
                DefaultConsoleLogFilter _filter = DefaultFilters[i];

                if (_filter.Match(_entry)) {
                    return _filter;
                }
            }

            return DefaultFilters[0];
        }

        /// <summary>
        /// Resets all filters displayed count.
        /// </summary>
        internal void ResetFiltersDisplayedCount() {
            foreach (CustomConsoleLogFilter _filter in CustomFilters) {
                _filter.DisplayedCount = 0;
            }

            foreach (DefaultConsoleLogFilter _filter in DefaultFilters) {
                _filter.DisplayedCount = 0;
            }
        }

        /// <summary>
        /// Copies the values of another <see cref="EnhancedConsoleEnhancedSettings"/>.
        /// </summary>
        /// <param name="_settings">The setting values to copy.</param>
        public void CopyValues(EnhancedConsoleEnhancedSettings _settings) {
            selectedTabIndex = _settings.selectedTabIndex;

            Columns = _settings.Columns;
            CustomFilters = _settings.CustomFilters;
            DefaultFilters = _settings.DefaultFilters;
            IgnoredStackCalls = _settings.IgnoredStackCalls;
        }
        #endregion

        #region Settings
        private const float ToolbarWidth = 375f;
        private const float ToolbarHeight = 25f;
        private const float ButtonWidth = 55f;

        public const string PreferencesFileExtension = "txt";
        public const string UndoRecordTitle = "Enhanced Console Preferences change";

        public const string PreferencesPath = EnhancedEditorSettings.UserSettingsPath + "/Enhanced Console";
        public const string PreferencesLabel = "Enhanced Console";

        public static readonly string[] PreferencesKeywords = new string[] {
                                                                "Enhanced",
                                                                "Editor",
                                                                "Console",
                                                                "Log",
                                                                "Stack Trace",
                                                            };

        private static readonly GUIContent importGUI = new GUIContent("Import", "Import console preferences from a file.");
        private static readonly GUIContent exportGUI = new GUIContent("Export", "Export these console preferences into a file.");

        private static readonly GUIContent[] tabsGUI = new GUIContent[] {
            new GUIContent("Colmuns", "Edit the order of the console detail columns"),
            new GUIContent("Custom Filters", "Create and edit your own filters for the console logs"),
            new GUIContent("Default Filters", "Edit the default console log filters"),
            new GUIContent("Ignored Calls", "Configure the calls to ignore from the console logs stack"),
        };

        private static readonly int settingsGUID = "EnhancedEditorConsoleSetting".GetHashCode();
        private static EnhancedConsoleEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty = null;

        /// <inheritdoc cref="EnhancedConsoleEnhancedSettings"/>
        public static EnhancedConsoleEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty.serializedObject != _userSettings.SerializedObject))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new EnhancedConsoleEnhancedSettings(settingsGUID);
                    settingsProperty = _userSettings.AddSetting(settings);

                    if (settings.Initialize()) {
                        _userSettings.Save();
                    }
                }

                return settings;
            }
        }

        /// <summary>
        /// Whether the enhanced console is currently enabled or not.
        /// </summary>
        public static bool Enabled {
            get { return Settings.enabled; }
            set {
                Settings.enabled = value;
                EnhancedEditorUserSettings.Instance.Save();
            }
        }

        // -----------------------

        public static EditorWindow OpenUserSettings() {
            EditorWindow _preferences = SettingsService.OpenUserPreferences(PreferencesPath);
            return _preferences;
        }

        [SettingsProvider]
        private static SettingsProvider CreateUserSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(PreferencesPath, SettingsScope.User) {
                label = PreferencesLabel,
                keywords = PreferencesKeywords,
                guiHandler = DrawSettings,
            };

            return _provider;
        }

        private static void DrawSettings(string _searchContext) {
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();

                // Import / export logs.
                if (GUILayout.Button(importGUI, EditorStyles.miniButtonLeft, GUILayout.Width(ButtonWidth))) {
                    string _path = EditorUtility.OpenFilePanel("Import Preferences", string.Empty, PreferencesFileExtension);
                    if (!string.IsNullOrEmpty(_path)) {
                        try {
                            string _json = File.ReadAllText(_path);

                            // To check if the json could be successfully loaded, resize the preferences columns to 0.
                            EnhancedConsoleEnhancedSettings _temp = new EnhancedConsoleEnhancedSettings(settingsGUID); { _temp.Columns.Array = new LogColumnType[0]; }
                            EditorJsonUtility.FromJsonOverwrite(_json, _temp);

                            // Then, if it was not resized, it means that the json was in an incorrect format.
                            if (_temp.Columns.Count == 0) {
                                throw new ArgumentException();
                            }

                            settings.CopyValues(_temp);
                        } catch (Exception e) when ((e is ArgumentException) || (e is IOException)) {
                            EditorUtility.DisplayDialog("Preferences Import", "Could not load any setting from the selected preferences file.\n\n" +
                                                        "Please select another file and try again.", "Ok");
                        }
                    }
                }

                if (GUILayout.Button(exportGUI, EditorStyles.miniButtonRight, GUILayout.Width(ButtonWidth))) {
                    string _path = EditorUtility.SaveFilePanel("Export Preferences", string.Empty, "EnhancedConsolePreferences", PreferencesFileExtension);
                    if (!string.IsNullOrEmpty(_path)) {
                        File.WriteAllText(_path, EditorJsonUtility.ToJson(Settings, true));
                    }
                }
            }

            GUILayout.Space(10f);

            EnhancedConsoleEnhancedSettings _setting = Settings;
            settingsProperty.serializedObject.Update();

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(15f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    _setting.selectedTabIndex = EnhancedEditorGUILayout.CenteredToolbar(_setting.selectedTabIndex, tabsGUI, GUI.ToolbarButtonSize.Fixed,
                                                                                            GUILayout.Width(ToolbarWidth), GUILayout.Height(ToolbarHeight));

                    GUILayout.Space(10f);

                    switch (_setting.selectedTabIndex) {
                        case 0:
                            EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("Columns"));

                            if (_changeCheck.changed) {
                                settingsProperty.serializedObject.ApplyModifiedProperties();
                                EnhancedConsoleWindow.GetWindow(false).SortColumns(_setting);
                            }

                            break;

                        case 1:
                            EnhancedEditorGUILayout.EnhancedPropertyField(settingsProperty.FindPropertyRelative("CustomFilters"));
                            break;

                        case 2:
                            EnhancedEditorGUILayout.EnhancedPropertyField(settingsProperty.FindPropertyRelative("DefaultFilters"));
                            break;

                        case 3:
                            EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("IgnoredStackCalls"));
                            break;

                        default:
                            break;
                    }

                    settingsProperty.serializedObject.ApplyModifiedProperties();

                    // Refresh on change.
                    if (_changeCheck.changed) {
                        EnhancedEditorUserSettings.Instance.Save();
                        EnhancedConsoleWindow.GetWindow(false).RefreshFilters();
                    }
                }
            }
        }
        #endregion
    }
}
