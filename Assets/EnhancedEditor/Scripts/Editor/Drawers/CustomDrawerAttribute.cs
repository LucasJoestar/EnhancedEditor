// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Tells an Enhanced Drawer class which attribute type it's a drawer for.
    /// <para/>
    /// When you make a custom drawer for an <see cref="EnhancedPropertyAttribute"/>, an <see cref="EnhancedMethodAttribute"/> or an <see cref="EnhancedClassAttribute"/>,
    /// you need to put this attribute on the drawer class to specify its target type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomDrawerAttribute : Attribute
    {
        #region Global Members
        /// <summary>
        /// The custom attribute type this is a drawer for.
        /// <br/>For instance, if this drawer is for script variables with a specific <see cref="EnhancedPropertyAttribute"/>,
        /// the type should be that attribute.
        /// </summary>
        public readonly Type TargetType = null;

        // -----------------------

        /// <param name="_targetType"><inheritdoc cref="TargetType" path="/summary"/></param>
        /// <inheritdoc cref="CustomDrawerAttribute"/>
        public CustomDrawerAttribute(Type _targetType)
        {
            TargetType = _targetType;
        }
        #endregion
    }
}
