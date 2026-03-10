// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="EnhancedSceneView"/>-related user settings.
    /// </summary>
    [Serializable]
    [MovedFrom(true, "EnhancedEditor.Editor", "EnhancedEditor.Editor", "SceneDesignerEnhancedSettings")]
    public sealed class EnhancedSceneViewEnhancedSettings : EnhancedSettings {
        [Serializable]
        public sealed class TemplateCategory {
            #region Content
            [Tooltip("Name of this category")]
            public string Name = "New Category";

            [Tooltip("Root folder of all templates associated with this category")]
            [Enhanced, Folder] public string Folder = string.Empty;

            [Tooltip("Displayed icon for this category")]
            public Texture Icon = null;
            #endregion
        }

        #region Global Members
        [Tooltip("Core category template folders")]
        [Enhanced, Folder] public List<string> Folders = new List<string>();

        [Tooltip("Additional template categories")]
        [Enhanced, DisplayName("Additional Categories")]
        public BlockArray<TemplateCategory> Categories = new BlockArray<TemplateCategory>(true, true, false);

        [Tooltip("Special category for drawing vertices area")]
        [Enhanced, Folder] public string VerticeAreaFolder = string.Empty;

        // --- General --- \\

        [Tooltip("Draws a wire box in the Scene View around all currently selected objects")]
        public bool DrawSelectionWire = true;
        [Tooltip("Draws a wire box in the Scene View around the object currently under the mouse position")]
        public bool DrawHoverWire     = true;

        [Tooltip("Color of the wire box drawned around the current selection")]
        [Enhanced, ShowIf(nameof(DrawSelectionWire))] public Color SelectionWireColor = SuperColor.White.Get();
        [Tooltip("Color of the wire box drawned around the current hover")]
        [Enhanced, ShowIf(nameof(DrawHoverWire))]     public Color HoverWireColor     = SuperColor.Crimson.Get();

        // --- Scene Editor --- \\

        [Tooltip("Maximum amount of displayed elements in the quick history dropdown window")]
        [SerializeField] public int QuickHistoryMaxCount = 5;

        [Tooltip("Layer mask used to detect colliders for placing objects in the scene")]
        [SerializeField] public LayerMask SnapMask       = ~0;

        [SerializeField] private Vector2 rotationRange   = Vector2.zero;
        [SerializeField] private Vector2 scaleRange      = Vector2.one;

        [SerializeField] private bool randomRotation     = false;
        [SerializeField] private bool randomScale        = false;

        // -----------------------

        /// <summary>
        /// Get a random scale used to place an object.
        /// </summary>
        public float RandomScale {
            get {
                if (!randomScale) {
                    return 1f;
                }

                return scaleRange.Random();
            }
        }

        /// <summary>
        /// Get a random rotation used to place an object.
        /// </summary>
        public Quaternion RandomRotation {
            get {
                if (!randomRotation) {
                    return Quaternion.identity;
                }

                return Quaternion.AngleAxis(rotationRange.Random(), Vector3.up);
            }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="EnhancedSceneViewEnhancedSettings"/>
        public EnhancedSceneViewEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Scene View Settings
        public const string PreferencesPath                 = EnhancedEditorSettings.UserSettingsPath + "/Scene Designer";
        public const string PreferencesLabel                = "Scene Designer";

        public static readonly string[] PreferencesKeywords = new string[] {
                                                                "Enhanced",
                                                                "Editor",
                                                                "Designer",
                                                                "Scene View",
                                                            };

        private static readonly GUIContent placementHeaderGUI = new GUIContent("Placement", "Placement related settings");

        private static readonly GUIContent sceneDesignerFoldersGUI = new GUIContent("Main Template Folders",
                                                                                    "All folders displayed in the main category to select templates to place in the scene using the Scene Designer");

        private static readonly GUIContent rotationGUI = new GUIContent("Random Rotation", "Toggles prefab rotation override");
        private static readonly GUIContent scaleGUI    = new GUIContent("Random Scale",    "Toggles prefab scale override - value is used as percent of the prefab original scale");

        private static readonly ReorderableList mainFolderList = new ReorderableList(null, typeof(string)) {
            drawElementCallback = DrawMainElementCallback,
            drawHeaderCallback  = DrawMainHeaderCallback,
        };

        private static readonly int settingsGUID = "EnhancedEditorScriptableSceneDesignerSetting".GetStableHashCode();
        private static EnhancedSceneViewEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty        = null;

        // -----------------------

        /// <inheritdoc cref="EnhancedSceneViewEnhancedSettings"/>
        public static EnhancedSceneViewEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty == null) || (settingsProperty.serializedObject != _userSettings.SerializedObject))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new EnhancedSceneViewEnhancedSettings(settingsGUID);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -------------------------------------------
        // Settings
        // -------------------------------------------

        public static EditorWindow OpenUserSettings() {
            return SettingsService.OpenUserPreferences(PreferencesPath);
        }

        [SettingsProvider]
        private static SettingsProvider CreateUserSettingsProvider() {
            SettingsProvider _provider = new SettingsProvider(PreferencesPath, SettingsScope.User) {
                label       = PreferencesLabel,
                keywords    = PreferencesKeywords,
                guiHandler  = DrawPreferenceSettings,
            };

            return _provider;
        }

        internal static void DrawPreferenceSettings(string _searchContext = "") {
            GUILayout.Space(10f);

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(15f);

                using (var _verticalScope = new GUILayout.VerticalScope()) {

                    DrawCategorySettings();
                    GUILayout.Space(10f);

                    EnhancedEditorGUILayout.UnderlinedLabel(placementHeaderGUI, EditorStyles.boldLabel);
                    GUILayout.Space(5f);

                    DrawPlacementSettings();
                }
            }
        }

        internal static void DrawCategorySettings() {
            var _settings = Settings;

            using (var _verticalScope = new GUILayout.VerticalScope())
            using (var _changeCheck   = new EditorGUI.ChangeCheckScope()) {

                settingsProperty.serializedObject.Update();

                // Main category.
                using (var _horizontalScope = new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Space(3f);

                    mainFolderList.list = _settings.Folders;
                    mainFolderList.DoLayoutList();

                    GUILayout.Space(3f);
                }

                GUILayout.Space(5f);

                // Additional categories.
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("Categories"));
                GUILayout.Space(5f);

                // Vertice area.
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("VerticeAreaFolder"));

                // Category refresh.
                if (_changeCheck.changed) {
                    SceneDesigner.RefreshAllFolders();
                }

                settingsProperty.serializedObject.ApplyModifiedProperties();

                // Save on change.
                if (_changeCheck.changed) {
                    EnhancedEditorSettings.UserSettings.Save();
                }
            }
        }

        internal static void DrawSceneDesignerSettings(GUIContent _label) {
            DrawCategorySettings();

            GUILayout.Space(10f);

            EnhancedEditorGUILayout.UnderlinedLabel(_label, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            DrawGeneralSettings(false);
        }

        internal static void DrawPlacementSettings() {
            var _ = Settings;
            settingsProperty.serializedObject.Update();

            EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("QuickHistoryMaxCount"));
            EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("SnapMask"));
            GUILayout.Space(5f);

            // Scale.
            DrawField(scaleGUI,    settingsProperty.FindPropertyRelative("randomScale"),    settingsProperty.FindPropertyRelative("scaleRange"),    20f);
            DrawField(rotationGUI, settingsProperty.FindPropertyRelative("randomRotation"), settingsProperty.FindPropertyRelative("rotationRange"), 360f);

            settingsProperty.serializedObject.ApplyModifiedProperties();

            // ----- Local Method ----- \\

            static void DrawField(GUIContent _label, SerializedProperty _toggleProperty, SerializedProperty _rangeProperty, float maxValue) {
                Rect _position = EditorGUILayout.GetControlRect();
                Rect _temp = EditorGUI.PrefixLabel(_position, _label);

                _temp.width = 25f;
                EditorGUI.PropertyField(_temp, _toggleProperty, GUIContent.none);

                if (_toggleProperty.boolValue) {
                    _temp.x = _temp.xMax;
                    _temp.xMax = _position.xMax;

                    EnhancedEditorGUI.MinMaxField(_temp, _rangeProperty, GUIContent.none, 0f, maxValue);
                }
            }
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        private static void DrawMainHeaderCallback(Rect _position) {
            EditorGUI.LabelField(_position, sceneDesignerFoldersGUI);
        }

        private static void DrawMainElementCallback(Rect _position, int _index, bool _isActive, bool _isFocused) {
            _position.yMin += EditorGUIUtility.standardVerticalSpacing;
            settings.Folders[_index] = EnhancedEditorGUI.FolderField(_position, settings.Folders[_index]);
        }
        #endregion

        #region Editor Settings
        private static readonly GUIContent generalHeaderGUI = new GUIContent("Scene View", "Enhanced scene view related settings");
        private static readonly GUIContent editGUI          = new GUIContent("Edit Scene Designer", "Opens the scene designer editor specific settings panel");

        // -----------------------

        [EnhancedEditorUserSettings(Order = 50)]
        private static void DrawGeneralSettings() {
            GUILayout.Space(10f);
            DrawGeneralSettings(true);

            GUILayout.Space(5f);
            if (GUILayout.Button(editGUI, EditorStyles.miniButtonLeft, GUILayout.Width(150f), GUILayout.Height(15f))) {
                OpenUserSettings();
            }
        }

        internal static void DrawGeneralSettings(bool _drawHeader) {
            var _ = Settings;

            if (_drawHeader) {
                EnhancedEditorGUILayout.UnderlinedLabel(generalHeaderGUI, EditorStyles.boldLabel);
                GUILayout.Space(5f);
            }

            // Selection and hover wires.
            settingsProperty.serializedObject.Update();

            SerializedProperty _drawSelection = settingsProperty.FindPropertyRelative("DrawSelectionWire");
            SerializedProperty _drawHover     = settingsProperty.FindPropertyRelative("DrawHoverWire");

            EditorGUILayout.PropertyField(_drawSelection);
            EditorGUILayout.PropertyField(_drawHover);

            if (_drawSelection.boolValue || _drawHover.boolValue) {
                GUILayout.Space(5f);

                if (_drawSelection.boolValue) {
                    EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("SelectionWireColor"));
                }

                if (_drawHover.boolValue) {
                    EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("HoverWireColor"));
                }
            }

            settingsProperty.serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
