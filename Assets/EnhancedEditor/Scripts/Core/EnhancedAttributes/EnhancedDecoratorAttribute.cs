// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Diagnostics;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Base class to derive all custom decorator attributes from.
    /// <para/>
    /// Decorators are used to draw purely decorative elements around variables in the inspector.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public abstract class EnhancedDecoratorAttribute : PropertyAttribute { }
}
