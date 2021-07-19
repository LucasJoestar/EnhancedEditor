// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="EnhancedEditor"/> internal class managing all property drawers
    /// and origin of the additional operations performed.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnhancedPropertyAttribute), true)]
    internal sealed class EnhancedPropertyEditor : PropertyDrawer
    {
        #region Drawer Content
        private EnhancedPropertyDrawer[] propertyDrawers = new EnhancedPropertyDrawer[] { };
        private bool isInitialized = false;

        private float propertyHeight = 0f;

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            // Draw property out of screen for initilization to get its supposed height.
            if (!isInitialized)
                OnGUI(new Rect(Screen.width, Screen.height, 0f, 0f), _property, _label);

            return propertyHeight;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (!isInitialized)
                Initialize(_property);

            // Field context click menu. 
            Event _event = Event.current;
            if ((_event.type == EventType.MouseDown) && (_event.button == 1) && _position.Contains(_event.mousePosition))
            {
                GenericMenu _menu = new GenericMenu();
                for (int _i = 0; _i < propertyDrawers.Length; _i++)
                    propertyDrawers[_i].OnContextMenu(_menu, _property);

                if (_menu.GetItemCount() > 0)
                {
                    _menu.ShowAsContext();
                    _event.Use();
                }
            }

            float _height;
            _position.height = EditorGUIUtility.singleLineHeight;
            propertyHeight = _position.y;

            // Pre GUI callback.
            for (int _i = 0; _i < propertyDrawers.Length; _i++)
            {
                if (propertyDrawers[_i].OnBeforeGUI(_position, _property, _label, out _height))
                {
                    if (_height == 0f)
                        _height -= EditorGUIUtility.standardVerticalSpacing;

                    propertyHeight = (_position.y + _height) - propertyHeight;
                    return;
                }

                IncreasePosition(ref _position, _height);
            }

            // Property GUI.
            bool _isDrawn = false;
            EditorGUI.BeginChangeCheck();

            for (int _i = 0; _i < propertyDrawers.Length; _i++)
            {
                EnhancedPropertyDrawer _drawer = propertyDrawers[_i];
                if (_drawer.OnGUI(_position, _property, _label, out _height))
                {
                    IncreasePosition(ref _position, _height);
                    _isDrawn = true;

                    break;
                }

                IncreasePosition(ref _position, _height);
            }

            // If no specific property field has been drawn, draw default one.
            if (!_isDrawn)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                IncreasePosition(ref _position, EditorGUIUtility.singleLineHeight);
            }

            // On property value changed callback.
            if (EditorGUI.EndChangeCheck())
            {
                for (int _i = 0; _i < propertyDrawers.Length; _i++)
                {
                    EnhancedPropertyDrawer _drawer = propertyDrawers[_i];

                    _property.serializedObject.ApplyModifiedProperties();
                    _drawer.OnValueChanged(_property);
                }
            }

            // Post GUI callback.
            for (int _i = 0; _i < propertyDrawers.Length; _i++)
            {
                EnhancedPropertyDrawer _drawer = propertyDrawers[_i];

                _drawer.OnAfterGUI(_position, _property, _label, out _height);
                IncreasePosition(ref _position, _height);
            }

            propertyHeight = _position.y - (propertyHeight + EditorGUIUtility.standardVerticalSpacing);
        }
        #endregion

        #region Utility
        private void Initialize(SerializedProperty _property)
        {
            // Get all compatible attributes from editing field and create their drawer.
            var _attributes = fieldInfo.GetCustomAttributes(typeof(EnhancedPropertyAttribute), true) as EnhancedPropertyAttribute[];

            propertyDrawers = new EnhancedPropertyDrawer[] { };
            foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetPropertyDrawers())
            {
                foreach (EnhancedPropertyAttribute _attribute in _attributes)
                {
                    if (_pair.Value == _attribute.GetType())
                    {
                        EnhancedPropertyDrawer _customDrawer = EnhancedPropertyDrawer.CreateInstance(_pair.Key, _property, _attribute, fieldInfo);
                        UnityEditor.ArrayUtility.Add(ref propertyDrawers, _customDrawer);

                        break;
                    }
                }
            }

            // Sort by order.
            Array.Sort(propertyDrawers, (a, b) => a.Attribute.order.CompareTo(b.Attribute.order));
            isInitialized = true;
        }

        private void IncreasePosition(ref Rect _position, float _height)
        {
            if (_height != 0f)
                _position.y += _height + EditorGUIUtility.standardVerticalSpacing;
        }
        #endregion
    }
}
