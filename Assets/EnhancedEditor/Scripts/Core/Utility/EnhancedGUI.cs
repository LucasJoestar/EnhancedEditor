// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Contains multiple <see cref="GUI"/>-related methods and variables.
    /// </summary>
	public static class EnhancedGUI
    {
        #region GUI Buffers
        /// <summary>
        /// <see cref="GUI.color"/> buffer system. Use this to dynamically push / pop GUI colors.
        /// </summary>
        public static readonly GUIBuffer<Color> GUIColor = new GUIBuffer<Color>(() => GUI.color,
                                                                                (c) => GUI.color = c, "GUI Color");

        /// <summary>
        /// <see cref="GUI.contentColor"/> buffer system. Use this to dynamically push / pop GUI content colors.
        /// </summary>
        public static readonly GUIBuffer<Color> GUIContentColor = new GUIBuffer<Color>(() => GUI.contentColor,
                                                                                       (c) => GUI.contentColor = c, "GUI Content Color");

        /// <summary>
        /// <see cref="GUI.backgroundColor"/> buffer system. Use this to dynamically push / pop GUI background colors.
        /// </summary>
        public static readonly GUIBuffer<Color> GUIBackgroundColor = new GUIBuffer<Color>(() => GUI.backgroundColor,
                                                                                          (c) => GUI.backgroundColor = c, "GUI Background Color");

        /// <summary>
        /// <see cref="GUI.enabled"/> buffer system. Use this to dynamically push / pop GUI enabled states.
        /// </summary>
        public static readonly GUIBuffer<bool> GUIEnabled = new GUIBuffer<bool>(() => GUI.enabled,
                                                                                (b) => GUI.enabled = b, "GUI Enabled State");

        /// <summary>
        /// <see cref="GUIStyle.fontSize"/> buffer system. Use this to dynamically push / pop font size for various <see cref="GUIStyle"/>.
        /// </summary>
        public static readonly GUIBuffer<GUIStyle, int> GUIStyleFontSize = new GUIBuffer<GUIStyle, int>((s) => s.fontSize,
                                                                                                        (s, f) => s.fontSize = f, "Style Font Size");

        /// <summary>
        /// <see cref="GUIStyle.alignment"/> buffer system. Use this to dynamically push / pop alignment for various <see cref="GUIStyle"/>.
        /// </summary>
        public static readonly GUIBuffer<GUIStyle, TextAnchor> GUIStyleAlignment = new GUIBuffer<GUIStyle, TextAnchor>((s) => s.alignment,
                                                                                                                       (s, a) => s.alignment = a, "Style Alignment");
        #endregion
    }
}
