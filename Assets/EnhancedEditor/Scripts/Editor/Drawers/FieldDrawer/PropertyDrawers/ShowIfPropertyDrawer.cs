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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="ShowIfAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(ShowIfAttribute))]
	public class ShowIfPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private bool isValid = false;

        private MemberInfo member = null;
        private ConditionType conditionType = 0;

        // -----------------------

        public override void OnEnable(SerializedProperty _property)
        {
            ShowIfAttribute _attribute = (ShowIfAttribute)Attribute;
            conditionType = _attribute.Type;

            Type _type = _property.serializedObject.targetObject.GetType();
            isValid = EnhancedEditorGUIUtility.GetConditionMember(_type, _attribute.ConditionMemberName, out member);
        }

        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            if (!isValid)
            {
                _position.height = _height = EnhancedEditorGUIUtility.DefaultHelpBoxHeight;
                _position = EditorGUI.IndentedRect(_position);

                EditorGUI.HelpBox(_position, $"\"{_label.text}\" show specified condition cannot be found! " +
                                             $"Please specify a valid member returning a boolean.", UnityEditor.MessageType.Error);
                return false;
            }

            _height = 0f;
            return !EnhancedEditorGUIUtility.IsConditionFulfilled(member, _property.serializedObject.targetObject, conditionType);
        }
        #endregion
    }
}
