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
    /// Base class to derive any custom method drawer from,
    /// allowing to draw GUI elements to represent a class method in the editor.
    /// </summary>
	public abstract class MethodDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="MethodDrawer"/>
        /// with a specific <see cref="EnhancedMethodAttribute"/> target.
        /// </summary>
        /// <param name="_type">Class type to create. Must inherit from <see cref="MethodDrawer"/>!</param>.
        /// <param name="_serializedObject">Target editing <see cref="UnityEditor.SerializedObject"/>.</param>.
        /// <param name="_attribute">Attribute to create a drawer for.</param>.
        /// <param name="_methodInfo">The reflection <see cref="System.Reflection.MethodInfo"/> for the method this drawer represents.</param>.
        /// <returns>Newly created <see cref="MethodDrawer"/> instance.</returns>
        internal static MethodDrawer CreateInstance(Type _type, SerializedObject _serializedObject, EnhancedMethodAttribute _attribute, MethodInfo _methodInfo)
        {
            MethodDrawer _drawer = (MethodDrawer)Activator.CreateInstance(_type);
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
        /// <see cref="EnhancedMethodAttribute"/> associated with this drawer.
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
        /// Called when this drawer is created and initialized.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Called before this method GUI is drawn.
        /// <para/>
        /// Use this to draw additional GUI element(s) above this method representation
        /// (like a specific message or a feedback).
        /// </summary>
        /// <param name="_label">Label of the method.</param>
        /// <returns>True if the method drawer should be stopped
        /// (this will prevent associated method representation from being drawn), false otherwise.</returns>
        public virtual bool OnBeforeGUI(GUIContent _label)
        {
            return false;
        }

        /// <summary>
        /// Use this to specify how to draw this method on GUI.
        /// </summary>
        /// <param name="_methodInfo">Associated member reflection <see cref="System.Reflection.MethodInfo"/>.</param>
        /// <param name="_label">Label of the method.</param>
        /// <returns>True if the property has been drawn
        /// (should always be true when override), false otherwise.</returns>
        public virtual bool OnGUI(MethodInfo _methodInfo, GUIContent _label)
        {
            return false;
        }

        /// <summary>
        /// Called after this method GUI has been drawn.
        /// <para/>
        /// Use this to draw additional GUI element(s) below this method representation
        /// (like a specific message or a feedback).
        /// </summary>
        /// <param name="_label">Label of the method.</param>
        public virtual void OnAfterGUI(GUIContent _label) { }

        /// <summary>
        /// Use this to add item(s) to the menu displayed on method context click.
        /// </summary>
        /// <param name="_menu">Menu to add item(s) to.</param>
        /// <param name="_methodInfo">Associated member reflection <see cref="System.Reflection.MethodInfo"/>.</param>
        public virtual void OnContextMenu(GenericMenu _menu, MethodInfo _methodInfo) { }
        #endregion
    }
}
