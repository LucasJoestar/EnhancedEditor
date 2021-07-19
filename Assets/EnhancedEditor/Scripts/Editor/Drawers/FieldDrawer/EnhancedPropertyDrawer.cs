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
    /// Base class to derive any custom property drawer from (instead of <see cref="PropertyDrawer"/>),
    /// performing additional operations related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
    public abstract class EnhancedPropertyDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="EnhancedPropertyDrawer"/>
        /// with a specific <see cref="EnhancedPropertyAttribute"/> target.
        /// </summary>
        /// <param name="_type">Class type to create. Must inherit from <see cref="EnhancedPropertyDrawer"/>!</param>.
        /// <param name="_property">Target editing property associated with the attribute.</param>.
        /// <param name="_attribute">Attribute to create a drawer for.</param>.
        /// <param name="_fieldInfo">The reflection <see cref="System.Reflection.FieldInfo"/> for the member the associated property represents.</param>.
        /// <returns>Newly created <see cref="EnhancedPropertyDrawer"/> instance.</returns>
        internal static EnhancedPropertyDrawer CreateInstance(Type _type, SerializedProperty _property, EnhancedPropertyAttribute _attribute, FieldInfo _fieldInfo)
        {
            EnhancedPropertyDrawer _drawer = (EnhancedPropertyDrawer)Activator.CreateInstance(_type);
            _drawer.Attribute = _attribute;
            _drawer.Property = _property;
            _drawer.FieldInfo = _fieldInfo;

            _drawer.OnEnable(_property);

            SceneView.duringSceneGui -= _drawer.OnSceneGUI;
            SceneView.duringSceneGui += _drawer.OnSceneGUI;

            return _drawer;
        }

        // -----------------------

        private void OnSceneGUI(SceneView _scene)
        {
            foreach (UnityEditor.Editor _editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (_editor.serializedObject == Property.serializedObject)
                {
                    OnSceneGUI(Property, _scene);
                    return;
                }
            }

            // Remove callback if no active editor match target object.
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        #endregion

        #region Drawer Content
        /// <summary>
        /// <see cref="EnhancedPropertyAttribute"/> associated with this drawer.
        /// </summary>
        public EnhancedPropertyAttribute Attribute { get; private set; } = null;

        /// <summary>
        /// <see cref="SerializedProperty"/> associated with this drawer.
        /// </summary>
        public SerializedProperty Property { get; private set; } = null;

        /// <summary>
        /// The reflection <see cref="System.Reflection.FieldInfo"/> for the member the associated property represents.
        /// </summary>
        public FieldInfo FieldInfo { get; private set; } = null;

        // -----------------------

        /// <summary>
        /// Called when this drawer is created and initialized.
        /// </summary>
        public virtual void OnEnable(SerializedProperty _property) { }

        /// <summary>
        /// Called before this property field is drawn.
        /// <para/>
        /// Use this to draw additional GUI element(s) above this property field
        /// (like a specific message or a feedback).
        /// </summary>
        /// <param name="_position">Rect to draw within.
        /// Note that height is set to <see cref="EditorGUIUtility.singleLineHeight"/>
        /// and can be modified as desired.</param>
        /// <param name="_property">Property to draw above.</param>
        /// <param name="_label">Label of the property.</param>
        /// <param name="_height">Total height of the drawn GUI elements.
        /// Indicated value will be used to extend property height.</param>
        /// <returns>True if the property drawer should be stopped
        /// (this will prevent associated property field from being drawn), false otherwise.</returns>
        public virtual bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
            return false;
        }

        /// <summary>
        /// Use this to override the way the property will be drawn.
        /// </summary>
        /// <param name="_position">Rect to draw within.
        /// Note that height is set to <see cref="EditorGUIUtility.singleLineHeight"/>
        /// and can be modified as desired.</param>
        /// <param name="_property">Property to draw.</param>
        /// <param name="_label">Label of the property.</param>
        /// <param name="_height">Total height of the drawn GUI elements.
        /// Indicated value will be used to extend property height.</param>
        /// <returns>True if the property has been drawn
        /// (should always be true when override), false otherwise.</returns>
        public virtual bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
            return false;
        }

        /// <summary>
        /// Use this to draw additional GUI element(s) in the <see cref="SceneView"/> and to use <see cref="Handles"/>.
        /// </summary>
        /// <param name="_property">Associated property.</param>
        /// <param name="_scene">Scene to draw in.</param>
        public virtual void OnSceneGUI(SerializedProperty _property, SceneView _scene) { }

        /// <summary>
        /// Called after this property field has been drawn.
        /// <para/>
        /// Use this to draw additional GUI element(s) below this property field
        /// (like a specific message or a feedback).
        /// </summary>
        /// <param name="_position">Rect to draw within.
        /// Note that height is set to <see cref="EditorGUIUtility.singleLineHeight"/>
        /// and can be modified as desired.</param>
        /// <param name="_property">Property to draw below.</param>
        /// <param name="_label">Label of the property.</param>
        /// <param name="_height">Total height of the drawn GUI elements.
        /// Indicated value will be used to extend property height.</param>
        public virtual void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
        }

        /// <summary>
        /// Called whenever the property value has been changed.
        /// </summary>
        /// <param name="_property">Changed property.</param>
        public virtual void OnValueChanged(SerializedProperty _property) { }

        /// <summary>
        /// Use this to add item(s) to the menu displayed on field context click.
        /// </summary>
        /// <param name="_menu">Menu to add item(s) to.</param>
        /// <param name="_property">Associated property.</param>
        public virtual void OnContextMenu(GenericMenu _menu, SerializedProperty _property) { }
        #endregion
    }
}
