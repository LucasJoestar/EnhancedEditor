// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="RequiredAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            EnhancedEditorGUI.DrawRequiredHelpBox(_position, _property, out _height);
            return false;
        }

        public override void OnContextMenu(GenericMenu _menu, SerializedProperty _property)
        {
            _menu.AddItem(new GUIContent("Get Reference", "Get a reference of this property."), false, () =>
            {
                if (EnhancedEditorGUIUtility.FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _type))
                {
                    _property.objectReferenceValue = ((Component)_property.serializedObject.targetObject).GetComponent(_type.FieldType);
                    _property.serializedObject.ApplyModifiedProperties();
                }
            });
        }
        #endregion
    }
}
