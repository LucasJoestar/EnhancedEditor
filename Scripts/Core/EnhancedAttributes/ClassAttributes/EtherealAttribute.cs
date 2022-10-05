// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Use this attribute on any class to hide it from the EnhancedSystem public functionalities.
    /// <para/>
    /// For exemple, any class with this attribute won't be displayed on the list using <see cref="SerializedType{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EtherealAttribute : EnhancedClassAttribute { }
}
