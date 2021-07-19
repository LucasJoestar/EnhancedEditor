// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Base class to derive any custom <see cref="EnhancedClassAttribute"/> drawers from.
    /// <para/>
    /// Use this to create custom drawers for class attributes inheriting from <see cref="EnhancedClassAttribute"/>.
    /// </summary>
    public abstract class UnityObjectDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="UnityObjectDrawer"/>
        /// with a specific <see cref="UnityEditor.SerializedObject"/> target.
        /// </summary>
        /// <param name="_type">Class type to create. Must inherit from <see cref="UnityObjectDrawer"/>!</param>.
        /// <param name="_serializedObject">Target editing <see cref="UnityEditor.SerializedObject"/>.</param>
        /// <param name="_attribute">Attribute to create a drawer for.</param>
        /// <returns>Newly created <see cref="UnityObjectDrawer"/> instance.</returns>
        internal static UnityObjectDrawer CreateInstance(Type _type, SerializedObject _serializedObject, EnhancedClassAttribute _attribute)
        {
            UnityObjectDrawer _drawer = (UnityObjectDrawer)Activator.CreateInstance(_type);
            _drawer.SerializedObject = _serializedObject;
            _drawer.Attribute = _attribute;

            return _drawer;
        }
        #endregion

        #region Drawer Content
        /// <summary>
        /// A <see cref="UnityEditor.SerializedObject"/> representing the object(s) being inspected.
        /// </summary>
        public SerializedObject SerializedObject { get; private set; } = null;

        /// <summary>
        /// <see cref="EnhancedClassAttribute"/> associated with this drawer.
        /// </summary>
        public EnhancedClassAttribute Attribute { get; private set; } = null;

        // -----------------------

        /// <summary>
        /// Use this function to alter editing object(s) inspector.
        /// Return value determines if inspector should continue being drawn or not.
        /// </summary>
        /// <returns>True to continue drawing inspector, false otherwise.</returns>
        public virtual bool OnInspectorGUI() => true;

        /// <summary>
        /// This function is called when the editor for target object(s) is loaded.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// This function is called when the editor for target object(s) goes out of scope.
        /// </summary>
        public virtual void OnDisable() { }
        #endregion
    }
}
