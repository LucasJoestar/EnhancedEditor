// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="EnhancedProjectBrowser"/>-related user settings.
    /// </summary>
    [Serializable]
    public class EnhancedHierarchyEnhancedSettings : EnhancedSettings {
        [Serializable]
        public class LayerHierarchyStyle {
            #region Global Members
            public LayerMask Layer = 1;

            public HierarchyStyle Style = new HierarchyStyle();
            #endregion
        }

        #region Global Members
        [Space(5f)]

        [SerializeField, Enhanced, Tooltip("Toggles the Enhanced Hierarchy activation")]
        private bool enabled = false;

        [SerializeField, Tooltip("Toggles the Enhanced Hierarchy activation during play mode")]
        [Enhanced, ShowIf("enabled")] private bool enabledDuringPlay = true;

        [Space(5f)]

        [SerializeField, Tooltip("Default override icon to be displayed next to any non-prefab GameObject in the hierarchy")]
        private Texture gameObjectIcon = Resources.Load<Texture>("GameObjectIcon");

        [Space(15f, order = 0), Title("Hierarchy Header", order = 1), Space(3f, order = 2)]

        [SerializeField, Tooltip("Toggle the visibility of the hierarchy headers outline")]
        [Enhanced, DisplayName("Background Outline")] private bool headerOutline = true;

        [Space(10f)]

        [SerializeField, Enhanced, DisplayName("Header Backgrounds"), HelpBox("Gradients displayed on hierarchy headers background, for each indent level", MessageType.Info, false)]
        private BlockArray<Gradient> headerGradients = new BlockArray<Gradient>() {
            new Gradient(){
                colorKeys = new GradientColorKey[]{new GradientColorKey(SuperColor.Indigo.Get(), 1f)},
                alphaKeys = new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.7f, .5f), new GradientAlphaKey(0f, 1f)},
            }
        };

        [Space(15f, order = 0), Title("Layer Settings", order = 1), Space(7f, order = 2)]

        [SerializeField, Enhanced, HelpBox("Hierarchy styles used for GameObjects on specific layers", MessageType.Info, false)]
        private BlockArray<LayerHierarchyStyle> layerStyles = new BlockArray<LayerHierarchyStyle>() {
            new LayerHierarchyStyle()
        };

        // -----------------------

        [SerializeField, HideInInspector] private int coreSceneIconIndex = 5;
        private GUIContent coreSceneIcon = null;

        // -----------------------

        /// <summary>
        /// Core scene special icon.
        /// </summary>
        public GUIContent CoreSceneIcon {
            get {
                if (coreSceneIcon == null) {
                    coreSceneIcon = new GUIContent(EditorGUIUtility.IconContent($"sv_icon_dot{coreSceneIconIndex}_pix16_gizmo"));
                }

                return coreSceneIcon;
            }
        }

        /// <summary>
        /// Whether the Enhanced hierarchy module is enabled or not.
        /// </summary>
        public bool Enabled {
            get { return enabled && (!EditorApplication.isPlaying || enabledDuringPlay); }
        }

        /// <summary>
        /// Whether the headers outline visibility is enabled or not.
        /// </summary>
        public bool HeaderOutline {
            get { return headerOutline; }
        }

        // -----------------------

        /// <inheritdoc cref="EnhancedHierarchyEnhancedSettings"/>
        public EnhancedHierarchyEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Utility
        private const string GameObjectIconName = "d_GameObject Icon";

        // -----------------------

        /// <summary>
        /// Replaces an icon by the default override for any non-prefab <see cref="GameObject"/>, if it marches.
        /// </summary>
        /// <param name="_icon">The icon to replace.</param>
        /// <returns>True if the icon was replaced, false otherwise.</returns>
        public bool ReplaceGameObjectIcon(ref Texture _icon) {
            if ((_icon != null) && (_icon.name == GameObjectIconName) && (gameObjectIcon != null)) {
                _icon = gameObjectIcon;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the header gradient for an object at a specific indent.
        /// </summary>
        /// <param name="_indent">Indent of the object position.</param>
        /// <param name="_gradient">The header gradient for this object.</param>
        /// <returns>True if a matching gradient could be found, false otherwise.</returns>
        public bool GetHeaderGradient(int _indent, out Gradient _gradient) {
            if (headerGradients.Count == 0) {
                _gradient = null;
                return false;
            }

            _gradient = headerGradients[Mathf.Clamp(_indent, 0, headerGradients.Count - 1)];
            return true;
        }

        /// <summary>
        /// Get the override <see cref="HierarchyStyle"/> for a given layer.
        /// </summary>
        /// <param name="_layer">The layer to get the associated <see cref="HierarchyStyle"/>.</param>
        /// <param name="_style">The <see cref="HierarchyStyle"/> associated with the given layer.</param>
        /// <returns>True if any <see cref="HierarchyStyle"/> for this layer could be found, false otherwise.</returns>
        public bool GetLayerStyle(int _layer, out HierarchyStyle _style) {
            foreach (var _layerStyle in layerStyles) {

                if (_layerStyle.Layer.Contains(_layer)) {
                    _style = _layerStyle.Style;
                    return true;
                }
            }

            _style = null;
            return false;
        }

        /// <summary>
        /// Copies the values of another <see cref="EnhancedHierarchyEnhancedSettings"/>.
        /// </summary>
        /// <param name="_settings">The setting values to copy.</param>
        public void CopyValues(EnhancedHierarchyEnhancedSettings _settings) {
            enabled             = _settings.enabled;
            enabledDuringPlay   = _settings.enabledDuringPlay;

            gameObjectIcon      = _settings.gameObjectIcon;
            coreSceneIconIndex  = _settings.coreSceneIconIndex;

            headerOutline       = _settings.headerOutline;
            headerGradients     = _settings.headerGradients;

            layerStyles              = _settings.layerStyles;
        }
        #endregion

        #region Hierarchy Settings
        private const float ButtonWidth     = 55f;

        public const string PreferencesFileExtension        = "txt";
        public const string PreferencesPath                 = EnhancedEditorSettings.UserSettingsPath + "/Hierarchy";
        public const string PreferencesLabel                = "Hierarchy";

        public static readonly string[] PreferencesKeywords = new string[] {
                                                                "Enhanced",
                                                                "Editor",
                                                                "Hierarchy",
                                                                "GameObject",
                                                                "Scene View",
                                                            };

        private static readonly GUIContent importGUI    = new GUIContent("Import", "Import console preferences from a file.");
        private static readonly GUIContent exportGUI    = new GUIContent("Export", "Export these console preferences into a file.");
        private static readonly GUIContent applyGUI     = new GUIContent("Apply Modifications", "Applies any modification made on the hierarchy.");

        private static readonly GUIContent coreIconGUI          = new GUIContent("Core Scene Icon", "Special icon used for the core scene.");
        private static readonly GUIContent[] coreIconPopupGUI   = new GUIContent[] {
            new GUIContent("White"),
            new GUIContent("Blue"),
            new GUIContent("Turquoise"),
            new GUIContent("Green"),
            new GUIContent("Yellow"),
            new GUIContent("Orange"),
            new GUIContent("Red"),
            new GUIContent("Magenta"),
        };

        private static readonly int settingsGUID = "EnhancedHierarchySettings".GetHashCode();
        private static EnhancedHierarchyEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty = null;

        /// <inheritdoc cref="EnhancedHierarchyEnhancedSettings"/>
        public static EnhancedHierarchyEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty.serializedObject != _userSettings.SerializedObject))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new EnhancedHierarchyEnhancedSettings(settingsGUID);
                    settingsProperty = _userSettings.AddSetting(settings);
                }

                return settings;
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
                guiHandler = DrawHierarchySettings,
            };

            return _provider;
        }

        private static void DrawHierarchySettings(string _searchContext) {
            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();

                // Import / export logs.
                if (GUILayout.Button(importGUI, EditorStyles.miniButtonLeft, GUILayout.Width(ButtonWidth))) {
                    string _path = EditorUtility.OpenFilePanel("Import Preferences", string.Empty, PreferencesFileExtension);
                    if (!string.IsNullOrEmpty(_path)) {
                        try {
                            string _json = File.ReadAllText(_path);

                            // To check if the json could be successfully loaded, set its guid to -1.
                            EnhancedHierarchyEnhancedSettings _temp = new EnhancedHierarchyEnhancedSettings(settingsGUID);
                            { _temp.GUID = -1; }

                            EditorJsonUtility.FromJsonOverwrite(_json, _temp);

                            // Then, if the guid wasn't changed, it means that the json was in an incorrect format.
                            if (_temp.GUID == -1) {
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

            EnhancedHierarchyEnhancedSettings _settings = Settings;
            settingsProperty.serializedObject.Update();

            using (var _scope = new GUILayout.HorizontalScope()) {
                GUILayout.Space(15f);

                using (var _verticalScope = new GUILayout.VerticalScope())
                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {

                    // Core scene icon.
                    int _index = EditorGUILayout.Popup(coreIconGUI, _settings.coreSceneIconIndex, coreIconPopupGUI);

                    if (_index != _settings.coreSceneIconIndex) {
                        _settings.coreSceneIconIndex = Mathf.Clamp(_index, 0, 7);
                        _settings.coreSceneIcon = null;
                    }

                    // Settings.
                    EnhancedEditorGUILayout.BlockField(settingsProperty);

                    GUILayout.Space(10f);

                    using (var _horizontalScope = new GUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();

                        // Refresh on change.
                        if (GUILayout.Button(applyGUI, GUILayout.Width(150f)) || _changeCheck.changed) {
                            EnhancedEditorUserSettings.Instance.Save();

                            EnhancedHierarchy.Reset();
                            EditorApplication.RepaintHierarchyWindow();
                        }
                    }
                }
            }
        }
        #endregion

        #region Editor Settings
        private static readonly GUIContent headerGUI = new GUIContent("Enhanced Hierarchy", "Enhanced hierarchy related settings.");
        private static readonly GUIContent editGUI = new GUIContent("Edit Hierarchy", "Opens the enhanced hierarchy specific settings panel.");

        // -----------------------

        [EnhancedEditorUserSettings(Order = 65)]
        private static void DrawGeneralSettings() {
            var _ = Settings;

            GUILayout.Space(10f);

            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI);
            GUILayout.Space(5f);

            using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative("enabled"), false);

                if (_changeCheck.changed) {
                    EditorApplication.RepaintHierarchyWindow();
                }
            }

            GUILayout.Space(5f);
            if (GUILayout.Button(editGUI, EditorStyles.miniButtonLeft, GUILayout.Width(120f), GUILayout.Height(15f))) {
                OpenUserSettings();
            }
        }
        #endregion
    }
}
