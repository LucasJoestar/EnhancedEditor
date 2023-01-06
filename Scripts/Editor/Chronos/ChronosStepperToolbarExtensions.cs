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

using UnityEditor;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.ShortcutManagement;
#endif

namespace EnhancedEditor.Editor {
    /// <summary>
    /// <see cref="ChronosStepper"/> main editor toolbar extension class, with some additional shortcuts.
    /// </summary>
	public static class ChronosStepperToolbarExtensions {
        #region Styles
        private static class Styles {
            public static readonly GUIContent[] ButtonsGUI = new GUIContent[] {
                new GUIContent(EditorGUIUtility.FindTexture("Profiler.FirstFrame"),                                     "Decreases the game time scale."),
                new GUIContent(Resources.Load<Texture>(EditorGUIUtility.isProSkin ? "Hourglass_dark" : "Hourglass"),    "Resets the game time scale."),
                new GUIContent(EditorGUIUtility.FindTexture("Profiler.LastFrame"),                                      "Increases the game time scale."),
            };
        }
        #endregion

        #region Toolbar Extension
        [EditorToolbarRightExtension(Order = 75)]
        private static void GUI() {
            #if EDITOR_TOOLBAR
            GUILayout.Space(25f);
            #endif

            float _chronos = Time.timeScale;
            Styles.ButtonsGUI[1].text = (_chronos == 1f)
                                      ? string.Empty
                                      : $"x{_chronos:0.##}";

            int _click = EnhancedEditorToolbar.ButtonGroup(Styles.ButtonsGUI);
            switch (_click) {
                // Slow down.
                case 0:
                    ChronosStepper.Decrease();
                    break;

                // Reset time scale.
                case 1:
                    ChronosStepper.Reset();
                    break;

                // Speed up.
                case 2:
                    ChronosStepper.Increase();
                    break;

                default:
                    break;
            }

            // Constantly repaint while in play mode to correctly display the current time scale.
            if (Application.isPlaying) {
                EnhancedEditorToolbar.Repaint();
            }
        }
        #endregion

        #region Shortcuts
        #if UNITY_2019_1_OR_NEWER
        private const string ShortcutBasePath = "Enhanced Editor/";

        private static bool UseShortcuts {
            get {
                return Application.isFocused || (EditorWindow.focusedWindow == SceneView.lastActiveSceneView);
            }
        }

        // -----------------------

        [Shortcut(ShortcutBasePath + "Increase Time Scale", KeyCode.KeypadPlus)]
        private static void IncreaseShortcut() {
            if (UseShortcuts) {
                ChronosStepper.Increase();
            }
        }

        [Shortcut(ShortcutBasePath + "Reset Time Scale", KeyCode.KeypadMultiply)]
        private static void ResetShortcut() {
            if (UseShortcuts) {
                ChronosStepper.Reset();
            }
        }

        [Shortcut(ShortcutBasePath + "Decrease Time Scale", KeyCode.KeypadMinus)]
        private static void DecreaseShortcut() {
            if (UseShortcuts) {
                ChronosStepper.Decrease();
            }
        }
        #endif
        #endregion
    }
}
