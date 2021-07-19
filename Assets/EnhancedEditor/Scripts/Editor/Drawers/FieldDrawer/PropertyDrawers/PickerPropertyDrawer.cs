// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="PickerAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(PickerAttribute))]
	public class PickerPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private Type[] requiredTypes = new Type[] { };
        private bool isValid = true;

        // -----------------------

        public override void OnEnable(SerializedProperty _property)
        {
            PickerAttribute _attribute = Attribute as PickerAttribute;
            requiredTypes = _attribute.RequiredTypes;

            Type _propertyType = EnhancedEditorGUIUtility.GetFieldInfoType(FieldInfo);

            if (_propertyType.IsSubclassOf(typeof(Component)))
            {
                UnityEditor.ArrayUtility.Add(ref requiredTypes, _propertyType);
            }
            else if (_propertyType != typeof(GameObject))
            {
                isValid = false;
            }
        }

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Draw nothing if property is not valid.
            if (!isValid)
            {
                _height = 0f;
                return false;
            }

            _position.height = _height
                             = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.PickerField(_position, _property, _label, requiredTypes);
            return true;
        }
        #endregion
    }
}
