// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;
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
        internal class PropertyInfos
        {
            public readonly bool RequireConstantRepaint = false;

            public EnhancedPropertyDrawer[] PropertyDrawers = null;
            public float Height = EditorGUIUtility.singleLineHeight;

            // -----------------------

            public PropertyInfos(SerializedProperty _property, FieldInfo _fieldInfo)
            {
                // Get all enhanced attributes from the target field, and create their respective drawer.
                var _attributes = _fieldInfo.GetCustomAttributes(typeof(EnhancedPropertyAttribute), true) as EnhancedPropertyAttribute[];
                PropertyDrawers = new EnhancedPropertyDrawer[] { };

                foreach (EnhancedPropertyAttribute _attribute in _attributes)
                {
                    foreach (KeyValuePair<Type, Type> _pair in EnhancedDrawerUtility.GetPropertyDrawers())
                    {
                        if (_pair.Value == _attribute.GetType())
                        {
                            EnhancedPropertyDrawer _customDrawer = EnhancedPropertyDrawer.CreateInstance(_pair.Key, _property, _attribute, _fieldInfo);
                            ArrayUtility.Add(ref PropertyDrawers, _customDrawer);

                            if (!RequireConstantRepaint)
                                RequireConstantRepaint = _customDrawer.RequireConstantRepaint;

                            break;
                        }
                    }
                }

                // Sort the drawers by their order.
                Array.Sort(PropertyDrawers, (a, b) => a.Attribute.order.CompareTo(b.Attribute.order));
            }

            // -----------------------

            public void OnContextMenu(GenericMenu _menu)
            {
                foreach (EnhancedPropertyDrawer _drawer in PropertyDrawers)
                    _drawer.OnContextMenu(_menu);
            }
        }
        #endregion

        #region Drawer Content
        internal static readonly Dictionary<string, PropertyInfos> propertyInfos = new Dictionary<string, PropertyInfos>();

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            string _path = _property.propertyPath;

            // Repaint on next frame if not yet initialized.
            if (!propertyInfos.ContainsKey(_path))
            {
                EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
                return EditorGUIUtility.singleLineHeight;
            }

            return propertyInfos[_path].Height;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            string _path = _property.propertyPath;

            // Property initialization.
            if (!propertyInfos.ContainsKey(_path))
            {
                // Data clear.
                if (propertyInfos.Count > 100)
                {
                    propertyInfos.Clear();
                    GC.Collect();
                }

                PropertyInfos _newInfos = new PropertyInfos(_property, fieldInfo);
                propertyInfos.Add(_path, _newInfos);
            }

            float _yOrigin = _position.y;
            _position.height = EditorGUIUtility.singleLineHeight;

            // For some unknown reasons, the property label may be set to an empty string when using certain APIs (like GetPropertyHeight).
            // To ensure its viability, get it from another reference.
            GUIContent _tempLabel = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            if (_tempLabel.text == _label.text)
            {
                _label = _tempLabel;
            }

            var _infos = propertyInfos[_path];

            // Constantly repaint for the drawers who need it.
            if (_infos.RequireConstantRepaint)
                EnhancedEditorGUIUtility.Repaint(_property.serializedObject);

            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                // Pre GUI callback.
                foreach (EnhancedPropertyDrawer _drawer in _infos.PropertyDrawers)
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
                foreach (EnhancedPropertyDrawer _drawer in _infos.PropertyDrawers)
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
                foreach (EnhancedPropertyDrawer _drawer in _infos.PropertyDrawers)
                {
                    _drawer.OnAfterGUI(_position, _property, _label, out float _height);
                    IncreasePosition(_height);
                }

                // On property value changed callback.
                if (_changeCheck.changed)
                {
                    _property.serializedObject.ApplyModifiedProperties();
                    _property.serializedObject.Update();

                    foreach (EnhancedPropertyDrawer _drawer in _infos.PropertyDrawers)
                    {
                        _drawer.OnValueChanged();
                    }
                }
            }

            CalculateFullHeight();

            // Context click menu. 
            _position.y = _yOrigin;
            _position.height = _infos.Height;

            if (EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = new GenericMenu();
                _infos.OnContextMenu(_menu);

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
                float _height = _position.y - _yOrigin;
                if (_height != 0f)
                {
                    _height -= EditorGUIUtility.standardVerticalSpacing;
                }

                _infos.Height = _height;
            }
        }
        #endregion
    }
}
