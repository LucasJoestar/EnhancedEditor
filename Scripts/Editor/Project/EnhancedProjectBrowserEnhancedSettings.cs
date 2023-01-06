// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="EnhancedProjectBrowser"/>-related user settings.
    /// </summary>
    [Serializable]
    public class EnhancedProjectBrowserEnhancedSettings : EnhancedSettings {
        #region Global Members
        [SerializeField, Tooltip("Icon used to draw each default folders")]
        [Enhanced, ValidationMember("FolderColor")] private Texture defaultFolderIcon = null;

        [SerializeField, Tooltip("Icon used to draw all default open folders")]
        [Enhanced, ValidationMember("FolderColor")] private Texture defaultOpenFolderIcon = null;

        [SerializeField, Tooltip("Icon used to draw all default empty folders")]
        [Enhanced, ValidationMember("FolderColor")] private Texture defaultEmptyFolderIcon = null;

        [Space(5f)]

        [SerializeField, Tooltip("Color used to draw all default folder icons")]
        [Enhanced, ValidationMember("FolderColor")] private SuperColor folderColor = SuperColor.Aquamarine;

        [SerializeField, Tooltip("Toggles the Enhanced Project Browser activation")]
        [Enhanced, ValidationMember("Enabled")] private bool enabled = false;

        // -----------------------

        /// <summary>
        /// Icon used to draw each default folders.
        /// </summary>
        public Texture DefaultFolderIcon {
            get {
                if (defaultFolderIcon == null) {
                    defaultFolderIcon = EditorGUIUtility.IconContent("Folder Icon").image;
                }

                return defaultFolderIcon;
            }
            set {
                defaultFolderIcon = value;
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>
        /// Icon used to draw all default open folders.
        /// </summary>
        public Texture DefaultOpenFolderIcon {
            get {
                if (defaultOpenFolderIcon == null) {
                    defaultOpenFolderIcon = EditorGUIUtility.IconContent("FolderOpened Icon").image;
                }

                return defaultOpenFolderIcon;
            }
            set {
                defaultOpenFolderIcon = value;
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>
        /// Icon used to draw all default empty folders.
        /// </summary>
        public Texture DefaultEmptyFolderIcon {
            get {
                if (defaultEmptyFolderIcon == null) {
                    defaultEmptyFolderIcon = EditorGUIUtility.IconContent("FolderEmpty Icon").image;
                }

                return defaultEmptyFolderIcon;
            }
            set {
                defaultEmptyFolderIcon = value;
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>
        /// Color used to draw all default folder icons.
        /// </summary>
        public SuperColor FolderColor {
            get { return folderColor; }
            set {
                folderColor = value;
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>
        /// Toggles the Enhanced Project Browser activation.
        /// </summary>
        public bool Enabled {
            get { return enabled; }
            set {
                enabled = value;
                EditorApplication.RepaintProjectWindow();
            }
        }

        // -----------------------

        /// <inheritdoc cref="EnhancedProjectBrowserEnhancedSettings"/>
        public EnhancedProjectBrowserEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Settings
        private static readonly GUIContent headerGUI = new GUIContent("Enhanced Project Browser",
                                                                      "All enhanced project browser related settings.");

        private static readonly int settingsGUID = "EnhancedProjectBrowserSettings".GetHashCode();
        private static EnhancedProjectBrowserEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty = null;

        /// <inheritdoc cref="EnhancedProjectBrowserEnhancedSettings"/>
        public static EnhancedProjectBrowserEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty.serializedObject != _userSettings.SerializedObject))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new EnhancedProjectBrowserEnhancedSettings(settingsGUID);
                    settingsProperty = _userSettings.AddSetting(settings);
                }
                
                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorUserSettings(Order = 60)]
        private static void DrawSettings() {
            var _ = Settings;

            GUILayout.Space(10f);

            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI);
            GUILayout.Space(5f);

            EnhancedEditorGUILayout.BlockField(settingsProperty, false);
        }
        #endregion
    }
}
