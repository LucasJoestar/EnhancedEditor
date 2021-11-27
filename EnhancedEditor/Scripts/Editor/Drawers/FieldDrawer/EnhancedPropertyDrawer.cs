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
    /// Base class to derive any custom <see cref="EnhancedPropertyAttribute"/> drawer from (instead of <see cref="PropertyDrawer"/>).
    /// <para/>
    /// Use this to create custom drawers for field attributes inheriting from <see cref="EnhancedPropertyAttribute"/>.
    /// </summary>
    public abstract class EnhancedPropertyDrawer
    {
        #region Internal Behaviour
        /// <summary>
        /// Creates a new instance of a <see cref="EnhancedPropertyDrawer"/>,
        /// with a specific <see cref="EnhancedPropertyAttribute"/> target.
        /// </summary>
        /// <param name="_type">Drawer class type to create (must inherit from <see cref="EnhancedPropertyDrawer"/>).</param>.
        /// <param name="_property"><inheritdoc cref="SerializedProperty" path="/summary"/></param>.
        /// <param name="_attribute"><inheritdoc cref="Attribute" path="/summary"/></param>.
        /// <param name="_fieldInfo"><inheritdoc cref="FieldInfo" path="/summary"/>.</param>.
        /// <returns>Newly created <see cref="EnhancedPropertyDrawer"/> instance.</returns>
        internal static EnhancedPropertyDrawer CreateInstance(Type _type, SerializedProperty _property, EnhancedPropertyAttribute _attribute, FieldInfo _fieldInfo)
        {
            EnhancedPropertyDrawer _drawer = Activator.CreateInstance(_type) as EnhancedPropertyDrawer;
            _drawer.Attribute = _attribute;
            _drawer.SerializedProperty = _property;
            _drawer.FieldInfo = _fieldInfo;

            _drawer.OnEnable();

            SceneView.duringSceneGui -= _drawer.DuringSceneGUI;
            SceneView.duringSceneGui += _drawer.DuringSceneGUI;

            return _drawer;
        }

        // -----------------------

        private void DuringSceneGUI(SceneView _scene)
        {
            foreach (UnityEditor.Editor _editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (_editor.serializedObject == SerializedProperty.serializedObject)
                {
                    OnSceneGUI(_scene);
                    return;
                }
            }

            // Unsubscribe from the event if no active editor could match the target object.
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
        #endregion

        #region Drawer Content
        /// <summary>
        /// A <see cref="UnityEditor.SerializedProperty"/> representing the property associated with this drawer.
        /// </summary>
        public SerializedProperty SerializedProperty { get; private set; } = null;

        /// <summary>
        /// The <see cref="EnhancedPropertyAttribute"/> associated with this drawer.
        /// </summary>
        public EnhancedPropertyAttribute Attribute { get; private set; } = null;

        /// <summary>
        /// The reflection <see cref="System.Reflection.FieldInfo"/> for the member this property represents.
        /// </summary>
        public FieldInfo FieldInfo { get; private set; } = null;

        // -----------------------

        /// <summary>
        /// Called when this object is created and initialized.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Called before this property field is drawn.
        /// <br/>Use this to draw additional GUI element(s) above, like a specific message or a feedback.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_property']"/></param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_label']"/></param>
        /// <param name="_height"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_height']"/></param>
        /// <returns>True to stop drawing this property and prevent its associated field from being drawn, false otherwise.</returns>
        public virtual bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
            return false;
        }

        /// <summary>
        /// Use this to override the way this property is drawn.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_property']"/></param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_label']"/></param>
        /// <param name="_height"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_height']"/></param>
        /// <returns>True to prevent any other property field of this property from being drawn, false otherwise.</returns>
        public virtual bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
            return false;
        }

        /// <summary>
        /// Called after this property field has been drawn.
        /// <br/>Use this to draw additional GUI element(s) below, like a specific message or a feedback.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_property']"/></param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_label']"/></param>
        /// <param name="_height"><inheritdoc cref="DocumentationMethod(Rect, SerializedProperty, GUIContent, out float)" path="/param[@name='_height']"/></param>
        public virtual void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = 0f;
        }

        /// <summary>
        /// Called whenever this property value is changed in the inspector.
        /// </summary>
        public virtual void OnValueChanged() { }

        /// <summary>
        /// Allows you to add new item(s) to the <see cref="GenericMenu"/> displayed on this field context click.
        /// </summary>
        /// <param name="_menu">Menu to add item(s) to.</param>
        public virtual void OnContextMenu(GenericMenu _menu) { }

        /// <summary>
        /// Use this to draw additional GUI element(s) in the <see cref="SceneView"/> (and allows you to use <see cref="Handles"/>).
        /// </summary>
        /// <param name="_scene">Scene to draw in.</param>
        public virtual void OnSceneGUI(SceneView _scene) { }
        #endregion

        #region Documentation
        /// <summary>
        /// This method is for documentation only, used by inheriting its parameters documentation to centralize it in one place.
        /// </summary>
        /// <param name="_position">Rectangle on the screen to draw within.
        /// <br/>Note that the height is set to <see cref="EditorGUIUtility.singleLineHeight"/>, and can be modified as desired.</param>
        /// <param name="_property"><inheritdoc cref="SerializedProperty" path="/summary"/></param>
        /// <param name="_label">Label to be displayed in front of this property field.</param>
        /// <param name="_height">The total height used to draw your GUI controls. This will be used to increment the current GUI position.</param>
        private static void DocumentationMethod(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) => _height = 0f;
        #endregion
    }
}
