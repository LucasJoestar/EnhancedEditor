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
    /// <see cref="EnhancedSettings"/> for the core scene system.
    /// </summary>
    [Serializable]
    public class CoreSceneEnhancedSettings : EnhancedSettings {
        #region Global Members
        /// <summary>
        /// The core scene to load when entering play mode.
        /// <br/> Works only in editor.
        /// </summary>
        public SceneAsset CoreScene = new SceneAsset();

        /// <summary>
        /// Indicates whether the core scene is enabled and should be loaded when entering play mode.
        /// </summary>
        public bool IsCoreSceneEnabled = false;

        // -----------------------

        /// <inheritdoc cref="CoreSceneEnhancedSettings"/>
        public CoreSceneEnhancedSettings(int _guid) : base(_guid) { }
        #endregion

        #region Project Settings
        private const string CoreSceneMessage = "The Core Scene system allows to always load a specific scene first when entering play mode in the editor.";

        private static readonly GUIContent coreSceneHeaderGUI       = new GUIContent("Core Scene System", "Settings related to the Core Scene system.");
        private static readonly GUIContent coreSceneGUI             = new GUIContent("Core Scene", "The core scene to load when entering play mode.");
        private static readonly GUIContent isCoreSceneEnabledGUI    = new GUIContent("Enabled", "Enables / Disables to core scene system.");


        private static readonly int settingsGUID = "CoreSceneSettings".GetHashCode();
        private static CoreSceneEnhancedSettings settings = null;

        /// <inheritdoc cref="CoreSceneEnhancedSettings"/>
        public static CoreSceneEnhancedSettings Settings {
            get {
                EnhancedEditorProjectSettings _projectSettings = EnhancedEditorProjectSettings.Instance;

                if ((settings == null) && !_projectSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new CoreSceneEnhancedSettings(settingsGUID);
                    _projectSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        [EnhancedEditorProjectSettings(Order = 25)]
        private static void DrawSettings() {
            CoreSceneEnhancedSettings _setting = Settings;

            GUILayout.Space(15f);

            // Core scene system.
            EnhancedEditorGUILayout.UnderlinedLabel(coreSceneHeaderGUI);
            GUILayout.Space(2f);

            EnhancedEditorGUILayout.SceneAssetField(coreSceneGUI, _setting.CoreScene);

            if (!string.IsNullOrEmpty(_setting.CoreScene.guid)) {
                _setting.IsCoreSceneEnabled = EditorGUILayout.Toggle(isCoreSceneEnabledGUI, _setting.IsCoreSceneEnabled);
            }

            EditorGUILayout.HelpBox(CoreSceneMessage, UnityEditor.MessageType.Info);
        }
        #endregion
    }
}
