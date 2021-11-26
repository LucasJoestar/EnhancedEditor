// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Base class to derive any custom <see cref="EnhancedMethodAttribute"/> drawer from.
    /// <para/>
    /// Use this to draw additional GUI elements to represent your method in the inspector.
    /// </summary>
	public abstract class MethodDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="MethodDrawer"/>,
        /// with a specific <see cref="EnhancedMethodAttribute"/> target.
        /// </summary>
        /// <param name="_type">Drawer class type to create (must inherit from <see cref="MethodDrawer"/>).</param>.
        /// <param name="_serializedObject"><inheritdoc cref="SerializedObject" path="/summary"/></param>
        /// <param name="_attribute"><inheritdoc cref="Attribute" path="/summary"/></param>
        /// <param name="_methodInfo"><inheritdoc cref="MethodInfo" path="/summary"/></param>.
        /// <returns>Newly created <see cref="MethodDrawer"/> instance.</returns>
        internal static MethodDrawer CreateInstance(Type _type, SerializedObject _serializedObject, EnhancedMethodAttribute _attribute, MethodInfo _methodInfo)
        {
            MethodDrawer _drawer = Activator.CreateInstance(_type) as MethodDrawer;
            _drawer.SerializedObject = _serializedObject;
            _drawer.Attribute = _attribute;
            _drawer.MethodInfo = _methodInfo;
            _drawer.Label = new GUIContent(ObjectNames.NicifyVariableName(_methodInfo.Name), _attribute.Tooltip);

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
        /// The <see cref="EnhancedMethodAttribute"/> associated with this drawer.
        /// </summary>
        public EnhancedMethodAttribute Attribute { get; private set; } = null;

        /// <summary>
        /// The reflection <see cref="System.Reflection.MethodInfo"/> for the method this drawer represents.
        /// </summary>
        public MethodInfo MethodInfo { get; private set; } = null;
        
        /// <summary>
        /// Label associated with this method.
        /// </summary>
        public GUIContent Label { get; private set; } = null;

        // -----------------------

        /// <summary>
        /// Called when this object is created and initialized.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Called before this method main GUI representation.
        /// <br/>Use this to draw additional GUI element(s) above, like a specific message or a feedback.
        /// </summary>
        /// <returns>True to stop drawing this method and prevent its associated GUI representation from being drawn, false otherwise.</returns>
        public virtual bool OnBeforeGUI()
        {
            return false;
        }

        /// <summary>
        /// Use this to implement this method main GUI representation.
        /// </summary>
        /// <returns>True to prevent any other GUI representations of this method from being drawn, false otherwise.</returns>
        public virtual bool OnGUI()
        {
            return false;
        }

        /// <summary>
        /// Called after this method main GUI representation.
        /// <br/>Use this to draw additional GUI element(s) below, like a specific message or a feedback.
        /// </summary>
        public virtual void OnAfterGUI() { }

        /// <summary>
        /// Allows you to add new item(s) to the <see cref="GenericMenu"/> displayed on this method context click.
        /// </summary>
        /// <param name="_menu">Menu to add item(s) to.</param>
        public virtual void OnContextMenu(GenericMenu _menu) { }
        #endregion
    }
}
