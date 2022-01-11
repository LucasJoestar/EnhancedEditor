// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="SerializedInterface"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializedInterface), true)]
	public class SerializedInterfacePropertyDrawer : EnhancedPropertyEditor
    {
        #region Drawer Content
        private static readonly Dictionary<string, Type> interfaceInfos = new Dictionary<string, Type>();

        // -----------------------

        protected override float GetDefaultHeight(SerializedProperty _property, GUIContent _label) {
            SerializedProperty _gameObjectProperty = _property.FindPropertyRelative("gameObject");

            Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
            float _height = _position.height + EnhancedEditorGUI.GetRequiredExtraHeight(_position, _label, _gameObjectProperty.objectReferenceValue);

            return _height;
        }

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Register this property to cache its interface type.
            string _key = _property.propertyPath;

            if (!interfaceInfos.TryGetValue(_key, out Type _interfaceType))
            {
                _interfaceType = EnhancedEditorUtility.GetFieldInfoType(fieldInfo);
                interfaceInfos.Add(_key, _interfaceType);
            }

            // Required field help box.
            SerializedProperty _gameObjectProperty = _property.FindPropertyRelative("gameObject");
            _position.height = EditorGUIUtility.singleLineHeight;

            float _extraHeight = EnhancedEditorGUI.RequiredHelpBox(_position, _label, _gameObjectProperty.objectReferenceValue);
            Rect _temp = new Rect(_position)
            {
                height = _extraHeight
            };

            using (var _scope = new EditorGUI.PropertyScope(_temp, GUIContent.none, _property)) { }
            _position.y += _extraHeight;

            // Interface picker.
            EnhancedEditorGUI.PickerField(_position, _gameObjectProperty, _label, _interfaceType);

            float _height = _position.height + _extraHeight;
            return _height;
        }
        #endregion
    }
}
