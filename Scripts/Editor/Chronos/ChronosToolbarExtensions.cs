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

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Main editor toolbar extensions allowing to easily increase and decrease the game time scale.
    /// </summary>
	public static class ChronosToolbarExtensions
    {
        #region Global Members
        public static readonly GUIContent[] buttonsGUI = new GUIContent[]
                                                                {
                                                                    new GUIContent(string.Empty, "Decreases the game time scale."),
                                                                    new GUIContent(string.Empty, "Resets the game time scale."),
                                                                    new GUIContent(string.Empty, "Increases the game time scale."),
                                                                };

        // -----------------------

        static ChronosToolbarExtensions()
        {
            string _hourglassIcon = EditorGUIUtility.isProSkin
                                  ? "Hourglass_dark.png"
                                  : "Hourglass.png";

            buttonsGUI[0].image = EditorGUIUtility.FindTexture("Profiler.FirstFrame");
            buttonsGUI[1].image = EditorGUIUtility.IconContent(_hourglassIcon).image;
            buttonsGUI[2].image = EditorGUIUtility.FindTexture("Profiler.LastFrame");

            Chronos.Reset();
        }
        #endregion

        #region GUI Draw
        [EditorToolbarRightExtension(Order = 75)]
        #pragma warning disable IDE0051
        private static void GUI()
        {
            #if EDITOR_TOOLBAR
            GUILayout.Space(25f);
            #endif
            
            buttonsGUI[1].text = (Time.timeScale == 1f)
                               ? string.Empty
                               : $"x{Time.timeScale:0.##}";

            int _click = EnhancedEditorToolbar.ButtonGroup(buttonsGUI);
            switch (_click)
            {
                // Slow down.
                case 0:
                    Chronos.Decrease();
                    break;

                // Reset time scale.
                case 1:
                    Chronos.Reset();
                    break;

                // Speed up.
                case 2:
                    Chronos.Increase();
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Shortcuts
        #if UNITY_2019_1_OR_NEWER
        private const string ShortcutBasePath = "Enhanced Editor/";

        // -----------------------

        [Shortcut(ShortcutBasePath + "Increase Time Scale", KeyCode.KeypadPlus)]
        private static void IncreaseShortcut()
        {
            if (UseShortcuts())
            {
                Chronos.Increase();
            }
        }

        [Shortcut(ShortcutBasePath + "Reset Time Scale", KeyCode.KeypadMultiply)]
        private static void ResetShortcut()
        {
            if (UseShortcuts())
            {
                Chronos.Reset();
            }
        }

        [Shortcut(ShortcutBasePath + "Decrease Time Scale", KeyCode.KeypadMinus)]
        private static void DecreaseShortcut()
        {
            if (UseShortcuts())
            {
                Chronos.Decrease();
            }
        }

        // -----------------------

        private static bool UseShortcuts()
        {
            bool _useShortcuts = Application.isFocused || (EditorWindow.focusedWindow == SceneView.lastActiveSceneView);
            return _useShortcuts;
        }
        #endif
        #endregion
    }
}
