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

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for methods with the attribute <see cref="ButtonAttribute"/> (inherit from <see cref="MethodDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ButtonAttribute))]
	public class ButtonDrawer : MethodDrawer
    {
        #region Drawer Content
        private const float LabelWidthCoef = .33f;
        private const float ButtonHeight = 25f;

        private ParameterInfo[] parameters = null;
        private GUIContent[] parametersGUI = null;
        private object[] parameterValues = null;

        private bool useCondition = false;

        // -----------------------

        public override void OnEnable()
        {
            // Parameter informations.
            parameters = MethodInfo.GetParameters();
            parameterValues = new object[parameters.Length];
            parametersGUI = new GUIContent[parameters.Length];

            for (int _i = 0; _i < parameters.Length; _i++)
            {
                ParameterInfo _parameter = parameters[_i];
                parametersGUI[_i] = new GUIContent(ObjectNames.NicifyVariableName(_parameter.Name));

                Type _type = _parameter.ParameterType;
                if (_parameter.HasDefaultValue)
                {
                    parameterValues[_i] = _parameter.DefaultValue;
                }
                else if (_type.IsValueType)
                {
                    parameterValues[_i] = Activator.CreateInstance(_type);
                }
            }
                
            // Condition member.
            ButtonAttribute _attribute = Attribute as ButtonAttribute;
            useCondition = !string.IsNullOrEmpty(_attribute.ConditionMember.Name) && _attribute.ConditionMember.GetValue(SerializedObject, out _);
        }

        public override bool OnGUI()
        {
            ButtonAttribute _attribute = Attribute as ButtonAttribute;
            bool _isEnable = _attribute.Mode.IsActive() &&
                            (!useCondition || (_attribute.ConditionMember.GetValue(SerializedObject, out bool _value) && (_value == _attribute.ConditionType.Get())));

            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                // Pack all button GUI controls within a nice box group.
                float _size = Mathf.Max(EditorStyles.label.CalcSize(Label).x + 20f, EnhancedEditorGUIUtility.ScreenWidth - 250f);
                using (var _verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(_size)))
                {
                    using (EnhancedGUI.GUIEnabled.Scope(_isEnable))
                    using (EnhancedGUI.GUIColor.Scope(_attribute.Color))
                    {
                        if (GUILayout.Button(Label, GUILayout.Height(ButtonHeight)))
                        {
                            foreach (Object _target in SerializedObject.targetObjects)
                            {
                                MethodInfo.Invoke(_target, parameterValues);
                            }
                        }
                    }

                    // Adjust the label width according to the size of the button.
                    using (var _labelScope = EnhancedEditorGUI.GUILabelWidth.Scope(_size * LabelWidthCoef))
                    {
                        for (int _i = 0; _i < parameters.Length; _i++)
                            DrawParameterField(_i);
                    }
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(3f);
            return true;
        }
        #endregion

        #region Parameter Utility
        private static readonly Dictionary<Type, int> parameterTypes = new Dictionary<Type, int>
        {
            { typeof(bool), 0 },
            { typeof(int), 1 },
            { typeof(float), 2 },
            { typeof(string), 3 },
            { typeof(char), 4 },
            { typeof(Color), 5 },
            { typeof(LayerMask), 6 },
            { typeof(Vector2), 7 },
            { typeof(Vector3), 8 },
            { typeof(Vector4), 9 },
            { typeof(Rect), 10 },
            { typeof(Vector2Int), 11 },
            { typeof(Vector3Int), 12 },
            { typeof(RectInt), 13 },
            { typeof(Quaternion), 14 },
            { typeof(Bounds), 15 },
            { typeof(BoundsInt), 16 },
            { typeof(AnimationCurve), 17 },
            { typeof(Gradient), 18 }
        };

        private void DrawParameterField(int _index)
        {
            ParameterInfo _parameter = parameters[_index];
            GUIContent _name = parametersGUI[_index];
            ref object _value = ref parameterValues[_index];

            Type _type = _parameter.ParameterType;
            if (parameterTypes.ContainsKey(_parameter.ParameterType))
            {
                switch (parameterTypes[_type])
                {
                    // Boolean.
                    case 0:
                        _value = EditorGUILayout.Toggle(_name, (bool)_value);
                        break;

                    // Int.
                    case 1:
                        _value = EditorGUILayout.IntField(_name, (int)_value);
                        break;

                    // Float.
                    case 2:
                        _value = EditorGUILayout.FloatField(_name, (float)_value);
                        break;

                    // String.
                    case 3:
                        _value = EditorGUILayout.TextField(_name, (string)_value);
                        break;

                    // Char.
                    case 4:
                        _value = EditorGUILayout.TextField(_name, (string)_value);
                        break;
                    // Color.
                    case 5:
                        _value = EditorGUILayout.ColorField(_name, (Color)_value);
                        break;

                    // LayerMask.
                    case 6:
                        _value = EditorGUILayout.LayerField(_name, (LayerMask)_value);
                        break;

                    // Vector2.
                    case 7:
                        _value = EditorGUILayout.Vector2Field(_name, (Vector2)_value);
                        break;

                    // Vector3.
                    case 8:
                        _value = EditorGUILayout.Vector3Field(_name, (Vector3)_value);
                        break;

                    // Vector4.
                    case 9:
                        _value = EditorGUILayout.Vector4Field(_name, (Vector4)_value);
                        break;

                    // Rect.
                    case 10:
                        _value = EditorGUILayout.RectField(_name, (Rect)_value);
                        break;

                    // Vector2Int.
                    case 11:
                        _value = EditorGUILayout.Vector2IntField(_name, (Vector2Int)_value);
                        break;

                    // Vector3Int.
                    case 12:
                        _value = EditorGUILayout.Vector3IntField(_name, (Vector3Int)_value);
                        break;

                    // RectInt.
                    case 13:
                        _value = EditorGUILayout.RectIntField(_name, (RectInt)_value);
                        break;

                    // Quaternion.
                    case 14:
                        _value = Quaternion.Euler(EditorGUILayout.Vector3Field(_name, ((Quaternion)_value).eulerAngles));
                        break;

                    // Bounds.
                    case 15:
                        _value = EditorGUILayout.BoundsField(_name, (Bounds)_value);
                        break;

                    // BoundsInt.
                    case 16:
                        _value = EditorGUILayout.BoundsIntField(_name, (BoundsInt)_value);
                        break;

                    // AnimationCurve.
                    case 17:
                        _value = EditorGUILayout.CurveField(_name, (AnimationCurve)_value);
                        break;

                    // Gradient.
                    case 18:
                        _value = EditorGUILayout.GradientField(_name, (Gradient)_value);
                        break;
                }
            }
            else if (_type.IsSubclassOf(typeof(Object)))
            {
                _value = EditorGUILayout.ObjectField(_name, _value as Object, _parameter.ParameterType, true);
            }
            else if (_type.IsSubclassOf(typeof(Enum)))
            {
                _value = EditorGUILayout.Popup(_name, (int)_value, Enum.GetNames(_parameter.ParameterType));
            }
            else
            {
                EditorGUILayout.HelpBox($"Cannot draw parameter of type \"{_type}\"!", UnityEditor.MessageType.Error);
            }
        }
        #endregion
    }
}
