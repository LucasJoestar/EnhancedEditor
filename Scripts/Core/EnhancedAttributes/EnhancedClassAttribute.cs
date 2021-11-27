// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Diagnostics;

namespace EnhancedEditor
{
    /// <summary>
    /// Base class to derive all custom class attributes from.
    /// Use this to create custom attributes for your <see cref="UnityEngine.Object"/> script classes.
    /// <para/>
    /// A custom attribute can be hooked up with a custom drawer
    /// to get callbacks when the editor script is being drawn in the inspector.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class EnhancedClassAttribute : Attribute { }
}
