// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// 
    /// </summary>
	public static class Chronos
    {
        #region Global Members
        public static readonly GUIContent[] buttonsGUI = new GUIContent[]
                                                                {
                                                                    new GUIContent("", ""),
                                                                    new GUIContent("", ""),
                                                                    new GUIContent("", ""),
                                                                };

        // -----------------------

        static Chronos()
        {
            buttonsGUI[0].image = EditorGUIUtility.FindTexture("Profiler.FirstFrame");
            buttonsGUI[1].image = EditorGUIUtility.FindTexture("UnityEditor.AnimationWindow");
            buttonsGUI[2].image = EditorGUIUtility.FindTexture("Profiler.LastFrame");
        }
        #endregion

        #region GUI Draw
        [EditorToolbarRightExtension(Order = 75)]
        #pragma warning disable IDE0051
        private static void GUI()
        {
            GUILayout.Space(25f);
            EnhancedEditorToolbar.ButtonGroup(buttonsGUI, GUILayout.Width(96f));
        }
        #endregion
    }
}
