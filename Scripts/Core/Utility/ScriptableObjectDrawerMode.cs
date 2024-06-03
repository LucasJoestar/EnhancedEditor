// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Enum value used to determine how a <see cref="ScriptableObject"/> is drawn in the editor.
    /// </summary>
    [Flags]
    public enum ScriptableObjectDrawerMode {
        None    = 0,

        Button  = 1,
        Content = 2,
    }
}
