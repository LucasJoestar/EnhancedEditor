// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
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
        #region Property Infos
        private struct PropertyInfos
        {
            public Type InterfaceType;
            public float Height;

            // -----------------------

            public PropertyInfos(FieldInfo _fieldInfo)
            {
                InterfaceType = EnhancedEditorUtility.GetFieldInfoType(_fieldInfo);
                Height = 0f;
            }
        }
        #endregion

        #region Drawer Content
        private static readonly Dictionary<string, PropertyInfos> propertyInfos = new Dictionary<string, PropertyInfos>();

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            string _key = _property.propertyPath;

            // Only calculate the height if it has not been cached.
            if (!propertyInfos.TryGetValue(_key, out PropertyInfos _infos))
            {
                SerializedProperty _gameObjectProperty = _property.FindPropertyRelative("gameObject");
                _infos = new PropertyInfos(fieldInfo);

                Rect _position = EnhancedEditorGUIUtility.GetViewControlRect();
                float _height =_infos.Height
                              = _position.height + EnhancedEditorGUI.GetRequiredExtraHeight(_position, _label, _gameObjectProperty.objectReferenceValue);

                propertyInfos.Add(_key, _infos);
                return _height;
            }

            return _infos.Height;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Register this property to cache its infos.
            string _key = _property.propertyPath;

            if (!propertyInfos.TryGetValue(_key, out PropertyInfos _infos))
            {
                _infos = new PropertyInfos(fieldInfo);
                propertyInfos.Add(_key, _infos);
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
            EnhancedEditorGUI.PickerField(_position, _gameObjectProperty, _label, _infos.InterfaceType);

            _infos.Height = _position.height + _extraHeight;
            propertyInfos[_key] = _infos;
        }
        #endregion
    }
}
