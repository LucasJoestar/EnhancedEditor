// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Allows you to add you own menu items on the context menu of a <see cref="SerializedProperty"/>.
    /// <para/>
    /// The method must accept a <see cref="GenericMenu"/> and a <see cref="SerializedProperty"/> as arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SerializedPropertyMenuAttribute : Attribute { }
}
