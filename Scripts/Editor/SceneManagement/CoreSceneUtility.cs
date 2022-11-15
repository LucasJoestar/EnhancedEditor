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
    /// <see cref="EnhancedEditorProjectSetting"/> for the core scene system.
    /// </summary>
    [Serializable]
    public class CoreSceneProjectSetting : EnhancedEditorProjectSetting {
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

        /// <inheritdoc cref="CoreSceneProjectSetting"/>
        public CoreSceneProjectSetting(int _guid) : base(_guid) { }
        #endregion
    }

    /// <summary>
    /// Core scene related static class utility.
    /// </summary>
    public static class CoreSceneUtility {
        #region Project Settings
        private const string CoreSceneMessage = "The Core Scene system allows to always load a specific scene first when entering play mode in the editor.";
        private static readonly int settingGUID = "AutomaticSetup".GetHashCode();

        private static readonly GUIContent coreSceneHeaderGUI = new GUIContent("Core Scene System", "Settings related to the Core Scene system.");
        private static readonly GUIContent coreSceneGUI = new GUIContent("Core Scene", "The core scene to load when entering play mode.");
        private static readonly GUIContent isCoreSceneEnabledGUI = new GUIContent("Enabled", "Enables / Disables to core scene system.");

        public static CoreSceneProjectSetting CoreSceneSetting {
            get {
                EnhancedEditorSettings _editorSettings = EnhancedEditorSettings.Settings;

                if (!_editorSettings.GetProjectSetting(settingGUID, out CoreSceneProjectSetting _setting)) {
                    _setting = new CoreSceneProjectSetting(settingGUID);
                    _editorSettings.AddProjectSetting(_setting);
                }

                return _setting;
            }
        }

        public static SceneAsset CoreScene {
            get { return CoreSceneSetting.CoreScene; }
        }

        public static bool IsCoreSceneEnabled {
            get { return CoreSceneSetting.IsCoreSceneEnabled; }
        }

        // -----------------------

        [EnhancedEditorProjectSettings(Order = 25)]
        private static void DrawProjectSetting() {
            CoreSceneProjectSetting _setting = CoreSceneSetting;

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
