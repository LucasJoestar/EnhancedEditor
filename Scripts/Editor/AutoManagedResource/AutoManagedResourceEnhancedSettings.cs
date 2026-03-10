// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="AutoManagedResource{T}"/> related project settings.
    /// </summary>
    public static class AutoManagedResourceEnhancedSettings {
        #region Project Settings
        public const string AutoManagedResourceDefaultDirectory = "EnhancedEditor/AutoManagedResources";
        private const string DirectoryPanelTitle                = "Auto-Managed Resources Default Directory";

        private static readonly GUIContent directoryGUI = new GUIContent("Managed Resource Dir.",
                                                                         "Directory in the project where are created all auto-managed resources.");

        private static readonly int settingsGUID       = "AutoManagedResourceDirectory".GetStableHashCode();
        private static FolderEnhancedSettings settings = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="AutoManagedResourceEnhancedSettings"/>
        public static FolderEnhancedSettings Settings {
            get {
                EnhancedEditorProjectSettings _projectSettings = EnhancedEditorProjectSettings.Instance;

                if ((settings == null) && !_projectSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new FolderEnhancedSettings(settingsGUID, AutoManagedResourceDefaultDirectory, true);
                    _projectSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -------------------------------------------
        // Drawer
        // -------------------------------------------

        [EnhancedEditorProjectSettings(Order = 10)]
        private static void DrawSettings() {
            FolderEnhancedSettings _settings = Settings;
            _settings.Folder = EnhancedEditorGUILayout.FolderField(directoryGUI, _settings.Folder, false, DirectoryPanelTitle);
        }
        #endregion
    }
}
