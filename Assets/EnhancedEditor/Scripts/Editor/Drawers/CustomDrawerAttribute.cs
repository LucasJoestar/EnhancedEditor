// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Tells an Enhanced Drawer class which run-time type it's a drawer for.
    /// <para/>
    /// When you make a custom drawer for an <see cref="EnhancedPropertyAttribute"/>, an <see cref="EnhancedMethodAttribute"/> or an <see cref="EnhancedClassAttribute"/>,
    /// you need to put this attribute on the associated editor class.
    /// </summary>
    public class CustomDrawerAttribute : Attribute
    {
        #region Global Members
        public readonly Type TargetType = null;

        // -----------------------

        /// <inheritdoc cref="CustomDrawerAttribute"/>
        /// <param name="_targetType">Defines which object type the custom drawer class can draw.</param>
        public CustomDrawerAttribute(Type _targetType)
        {
            TargetType = _targetType;
        }
        #endregion
    }
}
