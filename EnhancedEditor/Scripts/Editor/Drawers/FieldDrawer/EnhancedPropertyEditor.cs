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
    /// <see cref="EnhancedEditor"/> internal class used to manage multiple property drawers
    /// for a single field, and performing additional operations.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnhancedPropertyAttribute), true)]
    internal sealed class EnhancedPropertyEditor : PropertyDrawer
    {
        #region Property Infos
        private class PropertyInfos
        {
            public EnhancedPropertyDrawer[] PropertyDrawers = new EnhancedPropertyDrawer[] { };
            public float Height = EditorGUIUtility.singleLineHeight;
        }
        #endregion

        #region Drawer Content
        // To do.
        private static readonly Dictionary<string, PropertyInfos> propertyInfos = new Dictionary<string, PropertyInfos>();

        internal static List<EnhancedPropertyEditor> enhancedEditors = new List<EnhancedPropertyEditor>();
        internal string propertyPath = string.Empty;

        private EnhancedPropertyDrawer[] propertyDrawers = null;
        private float propertyHeight = EditorGUIUtility.singleLineHeight;

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            // Repaint on next frame if not yet initialized.
            if (propertyDrawers == null)
            {
                EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
            }

            return propertyHeight;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Initialization.
            if (propertyDrawers == null)
                Initialize(_property);

            float _yOrigin = _position.y;
            _position.height = EditorGUIUtility.singleLineHeight;

            // For some unknown reasons, the property label may be set to an empty string when using certain APIs (like GetPropertyHeight).
            // To ensure its viability, get it from another reference.
            GUIContent _tempLabel = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            if (_tempLabel.text == _label.text)
            {
                _label = _tempLabel;
            }

            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                // Pre GUI callback.
                foreach (EnhancedPropertyDrawer _drawer in propertyDrawers)
                {
                    if (_drawer.OnBeforeGUI(_position, _property, _label, out float _height))
                    {
                        IncreasePosition(_height);
                        CalculateFullHeight();

                        return;
                    }

                    IncreasePosition(_height);
                }

                // Property GUI.
                bool _isDrawn = false;
                foreach (EnhancedPropertyDrawer _drawer in propertyDrawers)
                {
                    if (_drawer.OnGUI(_position, _property, _label, out float _height))
                    {
                        IncreasePosition(_height);
                        _isDrawn = true;

                        break;
                    }

                    IncreasePosition(_height);
                }

                // If no specific property field has been drawn, draw default one.
                if (!_isDrawn)
                {
                    _position.height = EditorGUI.GetPropertyHeight(_property, true);
                    EditorGUI.PropertyField(_position, _property, _label, true);

                    IncreasePosition(_position.height);
                    _position.height = EditorGUIUtility.singleLineHeight;
                }

                // Post GUI callback.
                foreach (EnhancedPropertyDrawer _drawer in propertyDrawers)
                {
                    _drawer.OnAfterGUI(_position, _property, _label, out float _height);
                    IncreasePosition(_height);
                }

                // On property value changed callback.
                if (_changeCheck.changed)
                {
                    _property.serializedObject.ApplyModifiedProperties();
                    _property.serializedObject.Update();

                    foreach (EnhancedPropertyDrawer _drawer in propertyDrawers)
                    {
                        _drawer.OnValueChanged();
                    }
                }
            }

            CalculateFullHeight();

            // Context click menu. 
            _position.y = _yOrigin;
            _position.height = propertyHeight;

            if (EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = new GenericMenu();
                OnContextMenu(_menu);

                if (_menu.GetItemCount() > 0)
                    _menu.ShowAsContext();
            }

            // ----- Local Methods ----- \\

            void IncreasePosition(float _height)
            {
                if (_height != 0f)
                    _position.y += _height + EditorGUIUtility.standardVerticalSpacing;
            }

            void CalculateFullHeight()
            {
                propertyHeight = _position.y - _yOrigin;
                if (propertyHeight != 0f)
                {
                    propertyHeight -= EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }
        #endregion

        #region Utility
        internal void OnContextMenu(GenericMenu _menu)
        {
            foreach (EnhancedPropertyDrawer _drawer in propertyDrawers)
                _drawer.OnContextMenu(_menu);
        }

        private void Initialize(SerializedProperty _property)
        {
            // To do.
            /*PropertyInfos _infos = new PropertyInfos();
            propertyInfos.Add(_property.propertyPath, _infos);*/

            // -----------------------

            propertyPath = _property.propertyPath;

            // Register this property drawer.
            int _index = enhancedEditors.FindIndex(d => d.propertyPath == propertyPath);
            if (_index > -1)
            {
                enhancedEditors[_index] = this;
            }
            else
            {
                enhancedEditors.Add(this);
            }

            // Get all enhanced attributes from the target field, and create their respective drawer.
            var _attributes = fieldInfo.GetCustomAttributes(typeof(EnhancedPropertyAttribute), true) as EnhancedPropertyAttribute[];
            propertyDrawers = new EnhancedPropertyDrawer[] { };
            
            foreach (EnhancedPropertyAttribute _attribute in _attributes)
            {
                foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetPropertyDrawers())
                {
                    if (_pair.Value == _attribute.GetType())
                    {
                        EnhancedPropertyDrawer _customDrawer = EnhancedPropertyDrawer.CreateInstance(_pair.Key, _property, _attribute, fieldInfo);
                        ArrayUtility.Add(ref propertyDrawers, _customDrawer);

                        break;
                    }
                }
            }

            // Sort the drawers by their order.
            Array.Sort(propertyDrawers, (a, b) => a.Attribute.order.CompareTo(b.Attribute.order));
        }
        #endregion
    }
}
