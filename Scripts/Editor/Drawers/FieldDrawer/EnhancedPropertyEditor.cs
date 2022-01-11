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
    /// Base class to inherit all custom property drawers from instead of <see cref="PropertyDrawer"/>.
    /// <para/>
    /// Performs additional operations used to manage multiple property drawers
    /// for a single field.
    /// </summary>
    #if !UNITY_2021_1_OR_NEWER
    [CustomPropertyDrawer(typeof(EnhancedPropertyAttribute), true)]
    #endif
    [CustomPropertyDrawer(typeof(EnhancedAttribute), false)]
    public class EnhancedPropertyEditor : PropertyDrawer
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

        public override sealed float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            string _path = _property.propertyPath;

            // Repaint on next frame if not yet initialized.
            if (!propertyInfos.ContainsKey(_path))
            {
                float _height = GetDefaultHeight(_property, _label);
                return _height;
            }

            return propertyInfos[_path].Height;
        }

        public override sealed void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
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
                    float _height = OnEnhancedGUI(_position, _property, _label);
                    IncreasePosition(_height);

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

        #region Default Behaviour
        /// <summary>
        /// Override this to specify the height to use for a specific property.
        /// </summary>
        /// <param name="_property"><see cref="SerializedProperty"/> to specify the height for.</param>
        /// <param name="_label">Label displayed in front of the property.</param>
        /// <returns>Total height to be used to draw this property field.</returns>
        protected virtual float GetDefaultHeight(SerializedProperty _property, GUIContent _label)
        {
            EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Replacement method for <see cref="OnGUI(Rect, SerializedProperty, GUIContent)"/>.
        /// <br/> Height has to be specified as returned value.
        /// </summary>
        /// <param name="_position">Rectangle on the screen to draw within.</param>
        /// <param name="_property"><see cref="SerializedProperty"/> to draw a field for.</param>
        /// <param name="_label">Label displayed in front of the field.</param>
        /// <returns>Total height used to draw this property field.</returns>
        protected virtual float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            float _height = _position.height
                          = EditorGUI.GetPropertyHeight(_property, _label, true);

            EditorGUI.PropertyField(_position, _property, _label, true);
            return _height;
        }
        #endregion
    }
}
