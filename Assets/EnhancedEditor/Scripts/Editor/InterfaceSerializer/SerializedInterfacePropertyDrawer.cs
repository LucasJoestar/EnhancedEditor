// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="SerializedInterface"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializedInterface), true)]
	public class SerializedInterfacePropertyDrawer : PropertyDrawer
    {
        #region Drawer Content
        private SerializedProperty objectProperty = null;
        private Type interfaceType = null;

        private float height = EditorGUIUtility.singleLineHeight;
        private bool isInitialized = false;

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (!isInitialized)
            {
                objectProperty = _property.FindPropertyRelative("Object");
                interfaceType = EnhancedEditorGUIUtility.GetFieldInfoType(fieldInfo);

                isInitialized = true;
            }

            // Required field help box.
            if (EnhancedEditorGUI.DrawRequiredHelpBox(_position, objectProperty, out float _height))
            {
                _height += EditorGUIUtility.standardVerticalSpacing;
            }

            EnhancedEditorGUI.PickerField(_position, objectProperty, _label, interfaceType);

            // As usual, only update height on repaint event as this is the only one reliable on layout values.
            if (Event.current.type == EventType.Repaint)
            {
                height = _position.height + _height;
            }
        }
        #endregion
    }
}
