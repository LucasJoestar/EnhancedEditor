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
	public static class TimeFactor
    {
        #region Global Members
        public static readonly GUIContent[] buttonsGUI = new GUIContent[]
                                                                {
                                                                    new GUIContent("<<", ""),
                                                                    new GUIContent("X", ""),
                                                                    new GUIContent(">>", ""),
                                                                };

        // -----------------------

        static TimeFactor()
        {

        }
        #endregion

        #region GUI
        //[EditorToolbarRightExtension(Order = 75)]
        #pragma warning disable IDE0051
        private static void GUI()
        {
            GUILayout.Space(25f);
            EnhancedEditorToolbar.ButtonGroup(buttonsGUI, GUILayout.Width(96f));
        }
        #endregion
    }
}
