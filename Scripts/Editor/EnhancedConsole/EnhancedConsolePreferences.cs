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
        [Enhanced, Duo("UseNamespace")] public string Namespace = "Namespace";
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseNamespace = true;

        [Enhanced, Duo("UseClass")] public string Class = "Class";
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseClass = true;

        [Enhanced, Duo("UseMethod")] public string Method = "Method";
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
    public class EnhancedConsolePreferences : ScriptableObject {
        #region Content
        [SerializeField, HideInInspector] internal int selectedTabIndex = 0;

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
        /// Saves these preferences settings.
        /// </summary>
        public void Save() {
            EditorPrefs.SetString(EditorPrefsKey, EditorJsonUtility.ToJson(this));
        }
        #endregion

        #region User Preferences
        private const float ToolbarWidth = 375f;
        private const float ToolbarHeight = 25f;
        private const float ButtonWidth = 55f;

        public const string PreferencesFileExtension = ".txt";
        public const string UndoRecordTitle = "Enhanced Console Preferences change";

        public const string EditorPrefsKey = EnhancedEditorSettings.EditorPrefsPath + "EnhancedConsole";
        public const string PreferencesPath = EnhancedEditorSettings.PreferencesPath + "/Console";
        public const string PreferencesLabel = "Console";

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


        private static EnhancedConsolePreferences preferences = null;
        private static SerializedObject serializedObject = null;

        /// <summary>
        /// <see cref="EnhancedConsoleWindow"/>-related user preferences settings.
        /// </summary>
        public static EnhancedConsolePreferences Preferences {
            get {
                if (preferences == null) {
                    preferences = CreateInstance<EnhancedConsolePreferences>();
                    serializedObject = new SerializedObject(preferences);

                    string _json = EditorPrefs.GetString(EditorPrefsKey, string.Empty);
                    if (!string.IsNullOrEmpty(_json)) {
                        EditorJsonUtility.FromJsonOverwrite(_json, preferences);
                    }

                    if (preferences.Initialize()) {
                        EditorPrefs.SetString(EditorPrefsKey, EditorJsonUtility.ToJson(preferences));
                    }
                }

                return preferences;
            } set {
                if (value == null) {
                    return;
                }

                preferences = value;
                serializedObject = new SerializedObject(value);
                SavePreferences(value);
            }
        }

        // -----------------------

        public static EditorWindow OpenPreferences() {
            EditorWindow _preferences = SettingsService.OpenUserPreferences(PreferencesPath);
            return _preferences;
        }

        [SettingsProvider]
        private static SettingsProvider CreateUserSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(PreferencesPath, SettingsScope.User) {
                label = PreferencesLabel,
                keywords = PreferencesKeywords,
                guiHandler = DrawPreferences,
            };

            return _provider;
        }

        private static void DrawPreferences(string _searchContext) {
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();

                // Import / export logs.
                if (GUILayout.Button(importGUI, EditorStyles.miniButtonLeft, GUILayout.Width(ButtonWidth))) {
                    string _path = EditorUtility.OpenFilePanel("Import Preferences", string.Empty, PreferencesFileExtension);
                    if (!string.IsNullOrEmpty(_path)) {
                        try {
                            string _json = File.ReadAllText(_path);

                            // To check if the json could be successfully loaded, resize the preferences columns to 0.
                            EnhancedConsolePreferences _temp = CreateInstance<EnhancedConsolePreferences>(); { _temp.Columns.Array = new LogColumnType[0]; }
                            EditorJsonUtility.FromJsonOverwrite(_json, _temp);

                            // Then, if it was not resized, it means that the json was in an incorrect format.
                            if (_temp.Columns.Count == 0) {
                                throw new ArgumentException();
                            }

                            Preferences = _temp;
                        } catch (Exception e) when ((e is ArgumentException) || (e is IOException)) {
                            EditorUtility.DisplayDialog("Preferences Import", "Could not load any setting from the selected preferences file.\n\n" +
                                                        "Please select another file and try again.", "Ok");
                        }
                    }
                }

                if (GUILayout.Button(exportGUI, EditorStyles.miniButtonRight, GUILayout.Width(ButtonWidth))) {
                    string _path = EditorUtility.SaveFilePanel("Export Preferences", string.Empty, "EnhancedConsolePreferences", PreferencesFileExtension);
                    if (!string.IsNullOrEmpty(_path)) {
                        File.WriteAllText(_path, EditorJsonUtility.ToJson(Preferences, true));
                    }
                }
            }

            GUILayout.Space(10f);

            EnhancedConsolePreferences _preferences = Preferences;
            serializedObject.UpdateIfRequiredOrScript();

            Undo.RecordObject(_preferences, UndoRecordTitle);

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(15f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                    _preferences.selectedTabIndex = EnhancedEditorGUILayout.CenteredToolbar(_preferences.selectedTabIndex, tabsGUI, GUI.ToolbarButtonSize.Fixed,
                                                                                            GUILayout.Width(ToolbarWidth), GUILayout.Height(ToolbarHeight));

                    GUILayout.Space(10f);

                    switch (_preferences.selectedTabIndex) {
                        case 0:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("Columns"));

                            if (_changeCheck.changed) {
                                serializedObject.ApplyModifiedProperties();
                                EnhancedConsoleWindow.GetWindow(false).SortColumns(_preferences);
                            }

                            break;

                        case 1:
                            EnhancedEditorGUILayout.EnhancedPropertyField(serializedObject.FindProperty("CustomFilters"));
                            break;

                        case 2:
                            EnhancedEditorGUILayout.EnhancedPropertyField(serializedObject.FindProperty("DefaultFilters"));
                            break;

                        case 3:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("IgnoredStackCalls"));
                            break;

                        default:
                            break;
                    }

                    serializedObject.ApplyModifiedProperties();

                    // Serialize on change.
                    if (_changeCheck.changed) {
                        SavePreferences(_preferences);
                    }
                }
            }
        }

        private static void SavePreferences(EnhancedConsolePreferences _preferences) {
            string _json = EditorJsonUtility.ToJson(_preferences);
            EditorPrefs.SetString(EditorPrefsKey, _json);

            EnhancedConsoleWindow.GetWindow(false).RefreshFilters();
        }
        #endregion
    }
}