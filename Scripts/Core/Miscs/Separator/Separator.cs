// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Utility <see cref="Component"/> used to create an empty spacing in the inspector.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(InternalUtility.MenuPath + "Separator")]
    public class Separator : MonoBehaviour { }
}
