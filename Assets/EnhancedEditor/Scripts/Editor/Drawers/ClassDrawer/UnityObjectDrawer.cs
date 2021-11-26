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
    /// Base class to derive any custom <see cref="EnhancedClassAttribute"/> drawer from.
    /// <para/>
    /// Use this to create custom drawers for class attributes inheriting from <see cref="EnhancedClassAttribute"/>.
    /// </summary>
    public abstract class UnityObjectDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="UnityObjectDrawer"/>,
        /// with a specific <see cref="UnityEditor.SerializedObject"/> target.
        /// </summary>
        /// <param name="_type">Drawer class type to create (must inherit from <see cref="UnityObjectDrawer"/>).</param>.
        /// <param name="_serializedObject"><inheritdoc cref="SerializedObject" path="/summary"/></param>
        /// <param name="_attribute"><inheritdoc cref="Attribute" path="/summary"/></param>
        /// <returns>Newly created <see cref="UnityObjectDrawer"/> instance.</returns>
        internal static UnityObjectDrawer CreateInstance(Type _type, SerializedObject _serializedObject, EnhancedClassAttribute _attribute)
        {
            UnityObjectDrawer _drawer = Activator.CreateInstance(_type) as UnityObjectDrawer;
            _drawer.SerializedObject = _serializedObject;
            _drawer.Attribute = _attribute;

            _drawer.OnEnable();

            return _drawer;
        }
        #endregion

        #region Drawer Content
        /// <summary>
        /// A <see cref="UnityEditor.SerializedObject"/> representing the object(s) being inspected.
        /// </summary>
        public SerializedObject SerializedObject { get; private set; } = null;

        /// <summary>
        /// The <see cref="EnhancedClassAttribute"/> associated with this drawer.
        /// </summary>
        public EnhancedClassAttribute Attribute { get; private set; } = null;

        // -----------------------

        /// <summary>
        /// Called when this object is created and initialized.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Use this to alter the editing object(s) inspector.
        /// <br/>The return value determines whether the rest of the inspector should continue being drawn or not.
        /// </summary>
        /// <returns>True to stop drawing this object inspector, false otherwise.</returns>
        public virtual bool OnInspectorGUI()
        {
            return false;
        }

        /// <summary>
        /// Called when this object goes out of scope.
        /// </summary>
        public virtual void OnDisable() { }
        #endregion
    }
}
