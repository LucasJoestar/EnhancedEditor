// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="AutoManagedResource{T}"/> related project settings.
    /// </summary>
    public static class AutoManagedResourceEnhancedSettings {
        #region Project Settings
        public const string AutoManagedResourceDefaultDirectory = "EnhancedEditor/AutoManagedResources";
        private const string DirectoryPanelTitle = "Auto-Managed Resources Default Directory";

        private static readonly GUIContent directoryGUI = new GUIContent("Managed Resource Dir.",
                                                                         "Directory in the project where are created all auto-managed resources.");

        private static readonly int settingsGUID = "AutoManagedResourceDirectory".GetHashCode();
        private static FolderEnhancedSettings settings = null;

        /// <inheritdoc cref="AutoManagedResourceEnhancedSettings"/>
        public static FolderEnhancedSettings Settings {
            get {
                EnhancedEditorProjectSettings projectSettings = EnhancedEditorProjectSettings.Instance;

                if ((settings == null) && !projectSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new FolderEnhancedSettings(settingsGUID, AutoManagedResourceDefaultDirectory, true);
                    projectSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorProjectSettings(Order = 10)]
        private static void DrawSettings() {
            FolderEnhancedSettings _settings = Settings;
            _settings.Folder = EnhancedEditorGUILayout.FolderField(directoryGUI, _settings.Folder, false, DirectoryPanelTitle);
        }
        #endregion
    }
}
