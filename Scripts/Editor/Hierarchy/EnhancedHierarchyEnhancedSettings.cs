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
    public class EnhancedHierarchyEnhancedSettings : EnhancedSettings {
        #region Global Members
        //[SerializeField, Tooltip("Color used to draw all default folder icons")]
        //[Enhanced, ValidationMember("FolderColor")] private SuperColor folderColor = SuperColor.Aquamarine;

        [SerializeField, Tooltip("Toggles the Enhanced Hierarchy activation")]
        [Enhanced, ValidationMember("Enabled")] private bool enabled = false;

        // -----------------------

        /// <summary>
        /// Color used to draw all default folder icons.
        /// </summary>
        /*public SuperColor FolderColor {
            get { return folderColor; }
            set {
                folderColor = value;
                EditorApplication.RepaintHierarchyWindow();
            }
        }*/

        /// <summary>
        /// Toggles the Enhanced Project Browser activation.
        /// </summary>
        public bool Enabled {
            get { return enabled; }
            set {
                enabled = value;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        // -----------------------

        /// <inheritdoc cref="EnhancedHierarchyEnhancedSettings"/>
        public EnhancedHierarchyEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Settings
        private static readonly GUIContent headerGUI = new GUIContent("Enhanced Hierarchy",
                                                                      "All enhanced hierarchy related settings.");

        private static readonly int settingsGUID = "EnhancedHierarchySettings".GetHashCode();
        private static EnhancedHierarchyEnhancedSettings settings = null;
        private static SerializedProperty settingsProperty = null;

        /// <inheritdoc cref="EnhancedHierarchyEnhancedSettings"/>
        public static EnhancedHierarchyEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if (((settings == null) || (settingsProperty.serializedObject.targetObject == null))
                   && !_userSettings.GetSetting(settingsGUID, out settings, out settingsProperty)) {

                    settings = new EnhancedHierarchyEnhancedSettings(settingsGUID);
                    settingsProperty = _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorUserSettings(Order = 65)]
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
