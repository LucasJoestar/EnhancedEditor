// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor toolbar extension that automatically saves assets and open scene(s) at regular intervals.
    /// </summary>
    public static class EditorAutosave
    {
        #region Global Members
        private const float ButtonEnabledWidth = 60f;
        private const float ButtonDisabledWidth = 32f;
        private const float MinSaveInterval = 5f;
        private const double UpdateMaxInterval = .5f;

        private const string EnabledKey = "AutosaveEnabled";
        private const string RemainingTimeKey = "AutosaveRemainingTime";

        private const string EnableTooltip = "Toggle Assets & Open Scene(s) Autosave\n\nCurrently enabled (next save in less than {0} seconds).";

        private static readonly GUIContent enableGUI = new GUIContent(string.Empty, EnableTooltip);
        private static readonly GUIContent disableGUI = new GUIContent(string.Empty, "Toggle Assets & Open Scene(s) Autosave\n\nCurrently disabled.");

        private static float saveInterval = 0f;
        private static float saveRemainingTime = 0f;
        private static double lastTimeCheckup = 0f;
        private static bool isEnabled = false;

        // -----------------------

        static EditorAutosave()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            enableGUI.image = EditorGUIUtility.FindTexture("Record On");
            disableGUI.image = EditorGUIUtility.FindTexture("SaveAs");

            saveInterval = EnhancedEditorSettings.Settings.UserSettings.AutosaveInterval + .5f;

            // Loads session values.
            isEnabled = SessionState.GetBool(EnabledKey, isEnabled);
            saveRemainingTime = SessionState.GetFloat(RemainingTimeKey, saveRemainingTime);
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Set the time interval between two autosave.
        /// </summary>
        /// <param name="_saveInterval">New autosave time interval (in seconds).</param>
        public static void SetSaveInterval(float _saveInterval)
        {
            _saveInterval = Mathf.Max(MinSaveInterval, _saveInterval);
            saveInterval = saveRemainingTime
                         = _saveInterval + .5f;
        }

        private static void Update()
        {
            // Do not save in play mode, as it wouldn't be any useful.
            if (isEnabled && !EditorApplication.isPlaying && InternalEditorUtility.isApplicationActive)
            {
                double _timeSinceStartup = EditorApplication.timeSinceStartup;
                float _ellipse = (float)Math.Min(UpdateMaxInterval, _timeSinceStartup - lastTimeCheckup);
                lastTimeCheckup = _timeSinceStartup;

                saveRemainingTime -= _ellipse;
                if (saveRemainingTime < 0f)
                {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();

                    saveRemainingTime = saveInterval;
                }

                // Save this time value to keep it between recompilations.
                SessionState.SetFloat(RemainingTimeKey, saveRemainingTime);
            }
        }
        #endregion

        #region GUI Draw
        [EditorToolbarLeftExtension(Order = -25)]
        #pragma warning disable IDE0051
        private static void OnGUI()
        {
            // Select the appropriate options depending on the activation state.
            GUIContent _label;
            GUILayoutOption _width;

            if (isEnabled)
            {
                string _time = saveRemainingTime.ToString("##0");
                enableGUI.text = EditorApplication.isPlaying ? string.Empty : $" {_time}s";
                enableGUI.tooltip = string.Format(EnableTooltip, _time);

                _label = enableGUI;
                _width = GUILayout.Width(ButtonEnabledWidth);

                // Constantly repaint to properly display the remaining time.
                EnhancedEditorToolbar.Repaint();
            }
            else
            {
                _label = disableGUI;
                _width = GUILayout.Width(ButtonDisabledWidth);
            }

            // Disable this button while the application is playing, as no save would be performed.
            using (var _scope = EnhancedGUI.GUIEnabled.Scope(!Application.isPlaying))
            {
                // Enable / disable autosave and save its value.
                if (EnhancedEditorToolbar.Button(_label, _width))
                {
                    isEnabled = !isEnabled;
                    SessionState.SetBool(EnabledKey, isEnabled);
                }
            }

            GUILayout.Space(20f);
        }
        #endregion
    }

    class PlayModeButtons : VisualElement
    {
        const float k_ImguiOverrideWidth = 240f;

        readonly VisualElement m_PlayButton;
        readonly VisualElement m_PauseButton;
        readonly Button m_StepButton;
        readonly VisualElement m_UIElementsRoot;
        readonly IMGUIContainer m_ImguiOverride;

        public PlayModeButtons()
        {
            Debug.Log("In");

            name = "PlayMode";

            Add(m_UIElementsRoot = new VisualElement());
            m_UIElementsRoot.style.flexDirection = FlexDirection.Row;

            m_UIElementsRoot.Add(m_PlayButton = new VisualElement { name = "Play" });
            //m_PlayButton.RegisterValueChangedCallback(OnPlayButtonValueChanged);

            m_UIElementsRoot.Add(m_PauseButton = new VisualElement { name = "Pause" });
            //m_PauseButton.RegisterValueChangedCallback(OnPauseButtonValueChanged);

            m_UIElementsRoot.Add(m_StepButton = new ToolbarButton { name = "Step" });
            //EditorToolbarUtility.AddIconElement(m_StepButton);
            m_StepButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            m_StepButton.clicked += OnStepButtonClicked;

            //EditorToolbarUtility.SetupChildrenAsButtonStrip(m_UIElementsRoot);

            Add(m_ImguiOverride = new IMGUIContainer());
            m_ImguiOverride.style.display = DisplayStyle.None;
            m_ImguiOverride.style.width = k_ImguiOverrideWidth;

            UpdatePlayState();
            UpdatePauseState();
            UpdateStepState();

            //Immediately after a domain reload, Modes might be initialized after the toolbar so we wait a frame to check it
            EditorApplication.delayCall += () =>
            {
                CheckAvailability();
                CheckImguiOverride();
            };

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
            ModeService.modeChanged += OnModeChanged;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;
            ModeService.modeChanged -= OnModeChanged;
        }

        void OnModeChanged(ModeService.ModeChangedArgs args)
        {
            CheckAvailability();
            CheckImguiOverride();
        }

        void CheckAvailability()
        {
        }

        void CheckImguiOverride()
        {
        }

        void OnPlayButtonValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                EditorApplication.EnterPlaymode();
            }
            else
            {
                EditorApplication.ExitPlaymode();
            }
        }

        void OnPauseButtonValueChanged(ChangeEvent<bool> evt)
        {
            EditorApplication.isPaused = evt.newValue;
        }

        void OnStepButtonClicked()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.Step();
            }
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            UpdatePlayState();
            UpdateStepState();
        }

        void OnPauseStateChanged(PauseState state)
        {
            UpdatePauseState();
        }

        void UpdatePlayState()
        {
            //m_PlayButton.value = EditorApplication.isPlayingOrWillChangePlaymode;
        }

        void UpdatePauseState()
        {
            //m_PauseButton.value = EditorApplication.isPaused;
        }

        void UpdateStepState()
        {
            m_StepButton.SetEnabled(EditorApplication.isPlaying);
        }

        static void OverrideGUIHandler()
        {
        }
    }
}
