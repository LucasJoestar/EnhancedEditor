// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Linq;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Enhanced <see cref="GameObject"/> / <see cref="Component"/> picker,
    /// restricting selection to objects with specific component(s) and / or interface(s).
    /// </summary>
	public class PickerAttribute : EnhancedPropertyAttribute
    {
        #region Global Members
        public readonly Type[] RequiredTypes = new Type[] { };

        // -----------------------

        /// <inheritdoc cref="PickerAttribute(Type[])"/>
        public PickerAttribute() { }

        /// <inheritdoc cref="PickerAttribute"/>
        /// <param name="_requiredComponents">Required component and / or interface types.</param>
        public PickerAttribute(params Type[] _requiredComponents) 
        {
            RequiredTypes = _requiredComponents.Where(t => t.IsSubclassOf(typeof(Component)) || t.IsInterface).ToArray();
        }
        #endregion
    }
}
