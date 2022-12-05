// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="EnhancedEditor"/>-related shared project settings.
    /// <para/>
    /// Note that these settings should not be called from a <see cref="ScriptableObject"/> constructor, due to Unity preferences limitations.
    /// </summary>
    [NonEditable("Please use the Preferences/Project Settings windows to edit these settings.")]
    public class EnhancedEditorProjectSettings : EnhancedEditorSettings {
        #region Global Members
        private const string DefaultSettingsDirectory = "EnhancedEditor/Editor/Settings/";
        private static EnhancedEditorProjectSettings instance = null;

        /// <inheritdoc cref="EnhancedEditorProjectSettings"/>
        public static EnhancedEditorProjectSettings Instance {
            get {
                if ((instance == null) && !EnhancedEditorUtility.LoadMainAsset(out instance)) {
                    string _path = Path.Combine(Application.dataPath, DefaultSettingsDirectory);
                    if (!Directory.Exists(_path)) {
                        Directory.CreateDirectory(_path);
                    }

                    _path = Path.Combine("Assets", DefaultSettingsDirectory, $"EnhancedEditorSettings.asset");
                    instance = CreateInstance<EnhancedEditorProjectSettings>();

                    AssetDatabase.CreateAsset(instance, _path);
                    AssetDatabase.SaveAssets();
                }

                return instance;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Opens the Project Settings window with the <see cref="EnhancedEditor"/> settings already selected.
        /// </summary>
        [MenuItem(MenuPath + "Project Settings", false, -50), Button(SuperColor.Green, IsDrawnOnTop = false)]
        public static EditorWindow OpenSettings() {
            EditorWindow _projectSettings = SettingsService.OpenProjectSettings(ProjectSettingsPath);
            return _projectSettings;
        }

        public override void Save() {
            EnhancedEditorUtility.SaveAsset(this);
        }
        #endregion
    }
}
