﻿// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

namespace EnhancedEditor {
    /// <summary>
    /// Used to simulate <see cref="UnityEditor.MessageType"/> without needing to use the UnityEditor namespace.
    /// </summary>
    public enum MessageType {
        None    = 0,

        Info    = 1,
        Warning = 2,
        Error   = 3,
    }
}
