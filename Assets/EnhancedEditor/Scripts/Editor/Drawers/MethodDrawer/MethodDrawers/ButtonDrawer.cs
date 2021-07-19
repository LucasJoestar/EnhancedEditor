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
    /// Special drawer (inheriting from <see cref="MethodDrawer"/>) for methods with attribute <see cref="ButtonAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(ButtonAttribute))]
	public class ButtonDrawer : MethodDrawer
    {
        #region Drawer Content
        public const float ButtonHeight = 25f;

        /// <summary>
        /// <see cref="ParameterInfo"/> of the associated method.
        /// </summary>
        public ParameterInfo[] Parameters { get; private set; } = null;

        /// <summary>
        /// <see cref="GUIContent"/> of associated method parameters.
        /// </summary>
        public GUIContent[] ParametersGUI { get; protected set; } = null;

        /// <summary>
        /// Parameter values used to invoke associated method.
        /// </summary>
        public object[] ParameterValues { get; protected set; } = null;

        /// <summary>
        /// Condition member to be validated to enable this button.
        /// </summary>
        public MemberInfo ConditionMember { get; private set; } = null;

        /// <summary>
        /// Does this button require a condition to be validated?
        /// </summary>
        public bool UseCondition { get; private set; } = false;

        // -----------------------

        public override void OnEnable()
        {
            Parameters = MethodInfo.GetParameters();
            ParameterValues = new object[Parameters.Length];
            ParametersGUI = new GUIContent[Parameters.Length];

            for (int _i = 0; _i < Parameters.Length; _i++)
            {
                ParameterInfo _parameter = Parameters[_i];
                ParametersGUI[_i] = new GUIContent(ObjectNames.NicifyVariableName(_parameter.Name));

                Type _type = _parameter.ParameterType;
                if (_parameter.HasDefaultValue)
                {
                    ParameterValues[_i] = _parameter.DefaultValue;
                }
                else if (_type.IsValueType)
                {
                    ParameterValues[_i] = Activator.CreateInstance(_type);
                }
            }
                
            // Get condition member if using one.
            ButtonAttribute _attribute = Attribute as ButtonAttribute;
            if (!string.IsNullOrEmpty(_attribute.ConditionMemberName))
            {
                UseCondition = EnhancedEditorGUIUtility.GetConditionMember(SerializedObject.targetObject.GetType(), _attribute.ConditionMemberName, out MemberInfo _condition);
                ConditionMember = _condition;
            }
        }

        public override bool OnGUI(MethodInfo _methodInfo, GUIContent _label)
        {
            ButtonAttribute _attribute = Attribute as ButtonAttribute;
            bool _isEnable = _attribute.Mode.IsActive() &&
                            (!UseCondition || EnhancedEditorGUIUtility.IsConditionFulfilled(ConditionMember, SerializedObject.targetObject, _attribute.ConditionType));

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Begin a box vertical group with adjusted width according to label size.
            float _size = Mathf.Max(EditorStyles.label.CalcSize(Label).x + 20f, Screen.width - 250f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(_size));

            EnhancedEditorGUIUtility.PushGUIColor(_attribute.Color);
            EnhancedEditorGUIUtility.PushEnable(_isEnable);

            if (GUILayout.Button(Label, GUILayout.Height(ButtonHeight)))
            {
                foreach (Object _target in SerializedObject.targetObjects)
                {
                    MethodInfo.Invoke(_target, ParameterValues);
                }
            }

            EnhancedEditorGUIUtility.PopEnable();
            EnhancedEditorGUIUtility.PopGUIColor();

            // Adjust paramter fields label width according to button width.
            EnhancedEditorGUIUtility.PushLabelWidth(_size * .33f);

            for (int _i = 0; _i < Parameters.Length; _i++)
                DrawParameterField(_i);

            EnhancedEditorGUIUtility.PopLabelWidth();

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3f);
            return true;
        }
        #endregion

        #region Extra Callbacks
        /// <summary>
        /// Called whenever an associated parameter value has changed.
        /// </summary>
        /// <param name="_parameterValue">New parameter value.</param>
        /// <param name="_type">Parameter type.</param>
        public virtual void OnParameterValueChanged(ref object _parameterValue, Type _type) { }
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
            ParameterInfo _parameter = Parameters[_index];
            GUIContent _name = ParametersGUI[_index];
            object _value = ParameterValues[_index];

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
                        _value = EditorGUILayout.Vector3Field(_name, (Vector4)_value);
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

            // Invoke callback on parameter value change.
            if (_value != ParameterValues[_index])
            {
                ParameterValues[_index] = _value;
                OnParameterValueChanged(ref ParameterValues[_index], _type);
            }
        }
        #endregion
    }
}
