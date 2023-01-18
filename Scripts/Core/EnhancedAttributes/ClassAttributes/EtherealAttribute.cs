// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Use this attribute on any class to hide it from the EnhancedSystem public functionalities.
    /// <br/> Can also be used on an enum member to not display it inside an <see cref="EnumValues{Enum, T}"/>
    /// <para/>
    /// For exemple, any class with this attribute won't be displayed on the list using <see cref="SerializedType{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EtherealAttribute : EnhancedClassAttribute { }
}
