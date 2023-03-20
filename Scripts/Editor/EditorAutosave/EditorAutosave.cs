// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

#if UNITY_2021_1_OR_NEWER
#define SCENEVIEW_TOOLBAR
#elif UNITY_2020_1_OR_NEWER
#define EDITOR_TOOLBAR
#endif

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor toolbar extension that automatically saves assets and open scene(s) at regular intervals.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorAutosave {
        #region Global Members
        private const float DefaultSaveInterval = 300f;

        private const float ButtonEnabledWidth = 60f;
        private const float ButtonDisabledWidth = 32f;
        private const float MinSaveInterval = 5f;
        private const float UpdateMaxInterval = .5f;

        private const string EnabledKey = "AutosaveEnabled";
        private const string RemainingTimeKey = "AutosaveRemainingTime";

        private const string EnableTooltip = "Toggle Assets & Open Scene(s) Autosave\n\nCurrently enabled (next save in less than {0} seconds).";

        private static readonly GUIContent enableGUI = new GUIContent(string.Empty, EnableTooltip);
        private static readonly GUIContent disableGUI = new GUIContent(string.Empty, "Toggle Assets & Open Scene(s) Autosave\n\nCurrently disabled.");

        private static readonly int settingsGUID = "EnhancedEditorAutosaveSetting".GetHashCode();
        private static FloatEnhancedSettings settings = null;

        private static float saveInterval = 0f;
        private static float saveRemainingTime = 0f;
        private static bool isEnabled = false;

        /// <summary>
        /// Autosave-related user settings.
        /// </summary>
        public static FloatEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new FloatEnhancedSettings(settingsGUID, DefaultSaveInterval);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -----------------------

        static EditorAutosave() {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            enableGUI.image = EditorGUIUtility.FindTexture("Record On");
            disableGUI.image = EditorGUIUtility.FindTexture("SaveAs");

            saveInterval = Settings.Value + UpdateMaxInterval;

            // Loads session values.
            isEnabled = SessionState.GetBool(EnabledKey, isEnabled);
            saveRemainingTime = SessionState.GetFloat(RemainingTimeKey, saveRemainingTime);
        }
        #endregion

        #region Behaviour
        private static void Update() {
            // Do not save in play mode, as it wouldn't be any useful.
            if (isEnabled && !EditorApplication.isPlaying && InternalEditorUtility.isApplicationActive) {

                float _deltaTime = Math.Min(UpdateMaxInterval, ChronosUtility.RealDeltaTime);

                saveRemainingTime -= _deltaTime;
                if (saveRemainingTime < 0f) {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();

                    saveRemainingTime = saveInterval;
                }

                // Save this time value to keep it between recompilations.
                SessionState.SetFloat(RemainingTimeKey, saveRemainingTime);
            }
        }
        #endregion

        #region User Settings
        private static readonly GUIContent autosaveIntervalGUI = new GUIContent("Autosave Interval",
                                                                                "Time interval (in seconds) between two autosave. " +
                                                                                "Autosave can be toggled from the main editor toolbar.");

        // -----------------------

        [EnhancedEditorUserSettings(Order = 10)]
        private static void DrawSettings() {
            // Autosave interval.
            float _interval = saveInterval - UpdateMaxInterval;
            float _newInterval = EnhancedEditorGUILayout.MinField(autosaveIntervalGUI, _interval, MinSaveInterval);

            if (_newInterval != _interval) {
                _newInterval = Mathf.Max(MinSaveInterval, _newInterval);
                saveInterval = saveRemainingTime
                             = _newInterval + UpdateMaxInterval;

                settings.Value = _newInterval;
                GUI.changed = true;
            }
        }
        #endregion

        #region Toolbar Extension
        [EditorToolbarLeftExtension(Order = -25)]
        #pragma warning disable IDE0051
        private static void OnGUI() {
            #if SCENEVIEW_TOOLBAR
            GUILayout.Space(10f);
            #endif

            // Select the appropriate options depending on the activation state.
            GUIContent _label;
            GUILayoutOption _width;

            if (isEnabled) {
                string _time = saveRemainingTime.ToString("##0");
                enableGUI.text = EditorApplication.isPlaying ? string.Empty : $" {_time}s";
                enableGUI.tooltip = string.Format(EnableTooltip, _time);

                _label = enableGUI;
                _width = GUILayout.Width(ButtonEnabledWidth);

                // Constantly repaint to properly display the remaining time.
                EnhancedEditorToolbar.Repaint();
            } else {
                _label = disableGUI;
                _width = GUILayout.Width(ButtonDisabledWidth);
            }

            // Disable this button while the application is playing, as no save would be performed.
            using (var _scope = EnhancedGUI.GUIEnabled.Scope(!Application.isPlaying)) {
                // Enable / disable autosave and save its value.
                if (EnhancedEditorToolbar.Button(_label, _width)) {
                    isEnabled = !isEnabled;
                    SessionState.SetBool(EnabledKey, isEnabled);
                }
            }

            #if EDITOR_TOOLBAR
            GUILayout.Space(20f);
            #endif
        }
        #endregion
    }
}
