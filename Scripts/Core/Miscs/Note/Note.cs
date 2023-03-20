// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Editor utility <see cref="Component"/> with a simple editable note text.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(InternalUtility.MenuPath + "Note")]
    public class Note : MonoBehaviour {
        #region Global Members
        #if UNITY_EDITOR
        [SerializeField, Enhanced, EnhancedTextArea(true)] private string note = string.Empty;
        #endif
        #endregion
    }
}
