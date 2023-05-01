// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="Enum"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Enum), true)]
    public class EnumPropertyDrawer : EnhancedPropertyEditor {
        #region Enum Infos
        protected class EnumInfos {
            public GUIContent[] Names = new GUIContent[0];
            public int[] Values = new int[0];

            // -----------------------

            public EnumInfos(List<EnumValueInfo> _values) {

                int _count = _values.Count;

                Names = new GUIContent[_count];
                Values = new int[_count];

                for (int i = 0; i < _count; i++) {
                    Names[i] = new GUIContent(ObjectNames.NicifyVariableName(_values[i].Name), _values[i].Tooltip);
                    Values[i] = _values[i].Value;

                    if (Names[i].text == "_") {
                        Names[i].text = string.Empty;
                    }
                }
            }
        }
        #endregion

        #region Drawer Content
        private static readonly Dictionary<Type, EnumInfos> enums = new Dictionary<Type, EnumInfos>();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            if (!EnhancedEditorUtility.GetSerializedPropertyType(_property, out Type _type)) {
                EditorGUI.PropertyField(_position, _property, _label);
                return _height;
            }

            // Register enum.
            if (!enums.TryGetValue(_type, out EnumInfos _infos)) {
                _infos = new EnumInfos(EnumUtility.GetNames(_type));
                enums.Add(_type, _infos);
            }

            // Flags popup.
            if (_type.IsDefined(typeof(FlagsAttribute), true)) {

                FlagFieldAttribute _attribute = fieldInfo.GetCustomAttribute<FlagFieldAttribute>(true);

                if ((_attribute == null) || _attribute.MaskField) {

                    EditorGUI.PropertyField(_position, _property, _label);
                    return _height;
                }
            }

            DrawPopup(_position, _property, _infos, _label);
            return _height;
        }

        /// <summary>
        /// Draws this enum popup.
        /// </summary>
        /// <param name="_infos">Infos on this enum displayed names and values.</param>
        /// <inheritdoc cref="OnEnhancedGUI(Rect, SerializedProperty, GUIContent)"/>
        protected virtual void DrawPopup(Rect _position, SerializedProperty _property, EnumInfos _infos, GUIContent _label) {
            EditorGUI.IntPopup(_position, _property, _infos.Names, _infos.Values, _label);
        }
        #endregion
    }
}
