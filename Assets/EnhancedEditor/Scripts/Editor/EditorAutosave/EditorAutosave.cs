// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor toolbar extension automatically saving assets and open scene(s) at regular interval.
    /// </summary>
    public static class EditorAutosave
    {
        #region Global Members
        private const float ButtonEnabledWidth = 60f;
        private const float ButtonDisabledWidth = 32f;
        private const float SaveInterval = 300.5f;
        private const double MaxInterval = .5f;

        private const string EnableKey = "AutosaveEnabled";
        private const string RemainingTimeKey = "AutosaveRemainingTime";

        private const string EnableTooltip = "Toggle Assets & Open Scene(s) Autosave\n\nCurrently enabled (next save in less than {0} seconds).";

        private static readonly GUIContent enableGUI = new GUIContent(string.Empty, EnableTooltip);
        private static readonly GUIContent disableGUI = new GUIContent(string.Empty, "Toggle Assets & Open Scene(s) Autosave\n\nCurrently disabled.");

        private static bool isEnabled = false;

        private static float saveRemainingTime = 0f;
        private static double lastTimeCheckup = 0f;

        // -----------------------

        static EditorAutosave()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            enableGUI.image = EditorGUIUtility.FindTexture("Record On");
            disableGUI.image = EditorGUIUtility.FindTexture("SaveAs");

            // Loads saved values.
            isEnabled = SessionState.GetBool(EnableKey, isEnabled);
            saveRemainingTime = SessionState.GetFloat(RemainingTimeKey, saveRemainingTime);
        }
        #endregion

        #region Update
        private static void Update()
        {
            // Do not save in play mode, as it wouldn't be any useful.
            if (isEnabled && !EditorApplication.isPlaying && InternalEditorUtility.isApplicationActive)
            {
                double _timeSinceStartup = EditorApplication.timeSinceStartup;
                float _ellipse = (float)Math.Min(MaxInterval, _timeSinceStartup - lastTimeCheckup);
                lastTimeCheckup = _timeSinceStartup;

                saveRemainingTime -= _ellipse;
                if (saveRemainingTime < 0f)
                {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();

                    saveRemainingTime = SaveInterval;
                }

                // Save value to keep it between recompilations.
                SessionState.SetFloat(RemainingTimeKey, saveRemainingTime);
            }
        }
        #endregion

        #region GUI
        [EditorToolbarLeftExtension(Order = -25)]
        #pragma warning disable IDE0051
        private static void OnGUI()
        {
            // Select appropriate content depending on activation state.
            GUIContent _content;
            GUILayoutOption _width;

            if (isEnabled)
            {
                string _time = saveRemainingTime.ToString("##0");
                enableGUI.text = EditorApplication.isPlaying ? string.Empty : $" {_time}s";
                enableGUI.tooltip = string.Format(EnableTooltip, _time);

                _content = enableGUI;
                _width = GUILayout.Width(ButtonEnabledWidth);

                // Repaint to properly display remaining time.
                EnhancedEditorToolbar.Repaint();
            }
            else
            {
                _content = disableGUI;
                _width = GUILayout.Width(ButtonDisabledWidth);
            }

            // Disable button while application is playing, as no save would be perform.
            bool _isPlaying = Application.isPlaying;
            if (_isPlaying)
                EnhancedEditorGUIUtility.PushEnable(false);

            // Enable / disable autosave and save its value.
            if (EnhancedEditorToolbar.Button(_content, _width))
            {
                isEnabled = !isEnabled;
                SessionState.SetBool(EnableKey, isEnabled);
            }

            if (_isPlaying)
                EnhancedEditorGUIUtility.PopEnable();

            GUILayout.Space(25f);
        }
        #endregion
    }
}
