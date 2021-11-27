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
    /// Base class to derive all custom property attributes from.
    /// Use this to create and assign multiple custom attributes to your script variables.
    /// <para/>
    /// A custom attribute can be hooked up with a custom drawer
    /// to customize the way the variable is drawn in the inspector.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class EnhancedPropertyAttribute : PropertyAttribute { }
}
