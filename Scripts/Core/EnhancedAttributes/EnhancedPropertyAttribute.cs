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

    /// <summary>
    /// New attribute required to be used on every field using at least one other attribute
    /// starting from the 2021.1 version of Unity.
    /// <para/>
    /// As from this version multiple drawers for a single field causes multiple errors
    /// and strange behaviours, a single drawer is used for this attribute exclusively.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class EnhancedAttribute : PropertyAttribute { }
}
