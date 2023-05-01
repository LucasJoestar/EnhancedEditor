// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Allows you to draw your own extension controls on the main editor toolbar.
    /// <br/>Extensions can be drawn whether on the left or on the right side of the play mode buttons.
    /// <para/>
    /// The methods must be static, with a <see cref="SceneView"/>, a <see cref="GenericMenu"/> and a <see cref="RaycastHit"/> as arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SceneViewContextMenuItemAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// The delegate used for to call this attribute.
        /// </summary>
        /// <param name="_scene">The context <see cref="SceneView"/>.</param>
        /// <param name="_menu">Displayed <see cref="GenericMenu"/> to add items to.</param>
        /// <param name="_hit">The <see cref="RaycastHit"/> from the user mouse in world space.</param>
        public delegate void Delegate(SceneView _scene, GenericMenu _menu, RaycastHit _hit);

        // -----------------------

        /// <summary>
        /// Order in which to call this item.
        /// </summary>
        public int Order { get; set; } = 0;
        #endregion
    }
}
