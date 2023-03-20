// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Use this attribute on any <see cref="UnityEngine.Object"/> class to toggle its gizmos visibility.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScriptGizmosAttribute : EnhancedClassAttribute {
        #region Global Members
        /// <summary>
        /// Whether to display this script gizmos or not.
        /// </summary>
        public readonly bool ShowGizmos = true;

        /// <summary>
        /// Whether to display this script icon in the SceneView or not.
        /// </summary>
        public readonly bool ShowIcon = true;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_showIcon"><inheritdoc cref="ShowIcon" path="/summary"/></param>
        /// <param name="_showGizmos"><inheritdoc cref="ShowGizmos" path="/summary"/></param>
        /// <inheritdoc cref="ScriptGizmosAttribute"/>
        public ScriptGizmosAttribute(bool _showIcon, bool _showGizmos = true) {
            ShowIcon = _showIcon;
            ShowGizmos = _showGizmos;
        }
        #endregion
    }
}
