// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Makes this field a <see cref="GameObject"/> / <see cref="Component"/> object picker,
    /// constraining this value to objects with specific components and / or interfaces.
    /// </summary>
    public class PickerAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        /// <summary>
        /// Only the objects possessing all of these required components will be assignable
        /// (must either be a component or an interface).
        /// </summary>
        public readonly Type[] RequiredTypes = new Type[] { };

        // -----------------------

        /// <inheritdoc cref="PickerAttribute(Type[])"/>
        public PickerAttribute() { }

        /// <param name="_requiredComponents"><inheritdoc cref="RequiredTypes" path="/summary"/></param>
        /// <inheritdoc cref="PickerAttribute"/>
        public PickerAttribute(params Type[] _requiredComponents) 
        {
            RequiredTypes = _requiredComponents;
        }
        #endregion
    }
}
