// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    public class Shapeshifter : EditorWindow
    {
        #region Content
        [MenuItem("CONTEXT/Object/Shapeshifter")]
        public static void GetShapeshifter(MenuCommand _command) => GetWindow<Shapeshifter>(true, "Shapeshifter").Show(_command.context);

        // -------------------------------------------
        // Shapeshifter Window
        // -------------------------------------------

        private readonly GUIContent buttonGUI = new GUIContent("X", "Shapeshift this class as this new type");
        private readonly GUIContent[] toolbar = new GUIContent[] { new GUIContent("PARENT", "Shift this component to one of its parents"),
                                                                   new GUIContent("CHILD", "Shift this component to one of its children")};
        private int toolbarIndex = 0;

        private Type[] parentTypes = new Type[] { };
        private Type[] childTypes = new Type[] { };
        private Object target = null;

        // -----------------------

        public void Show(Object _target)
        {
            // Can only perform shapeshifting on MonoBehaviours & non StateMachineBehaviours ScriptableObjects.
            if (!(_target is MonoBehaviour) && (!(_target is ScriptableObject) || (_target is StateMachineBehaviour)))
            {
                // No no no no no.
                EditorUtility.DisplayDialog("Shapeshifter Error",
                                            "Shapeshift can only be applied to Monobehaviours and ScriptableObjects.\n\n" +
                                            "StateMachineBehaviours are not compatible either.",
                                            "Ok");

                Close();
                return;
            }

            // Get all base and derived class types from the inspecting class.
            Type _type = _target.GetType();

            Type _base = _type.BaseType;
            while ((_base != null) && (_base != typeof(MonoBehaviour)) && (_base != typeof(ScriptableObject)))
            {
                if (!_base.IsAbstract)
                    ArrayUtility.Add(ref parentTypes, _base);

                _base = _base.BaseType;
            }

            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int _i = 0; _i < _assemblies.Length; _i++)
            {
                Type[] _allTypes = _assemblies[_i].GetTypes();
                for (int _j = 0; _j < _allTypes.Length; _j++)
                {
                    Type _child = _allTypes[_j];
                    if (_child.IsSubclassOf(_type) && !_child.IsAbstract)
                        ArrayUtility.Add(ref childTypes, _allTypes[_j]);
                }
            }

            // Show window.
            target = _target;
            Show();
        }

        // -----------------------

        private void OnGUI()
        {
            toolbarIndex = GUILayout.Toolbar(toolbarIndex, toolbar);
            EditorGUILayout.Space(10);

            Type[] _list;
            switch (toolbarIndex)
            {
                case 0:
                    _list = parentTypes;
                    break;

                case 1:
                    _list = childTypes;
                    break;

                default:
                    _list = new Type[] { };
                    break;
            }

            // Draw each available class for shapeshifting.
            string _section = string.Empty;
            Rect _rect;
            Type _type;

            for (int _i = 0; _i < _list.Length; _i++)
            {
                // Draw namespace section.
                _type = _list[_i];
                string _assembly = _type.Namespace;
                if (_section != _assembly)
                {
                    if (!string.IsNullOrEmpty(_section))
                    {
                        EditorGUILayout.Space(10);
                        _rect = EditorGUILayout.GetControlRect(false, 1);
                        _rect.x += 25;
                        _rect.width -= 50;

                        EditorGUI.DrawRect(_rect, Color.gray);
                        EditorGUILayout.Space(10);
                    }

                    _section = _assembly;

                    EditorGUILayout.LabelField(_section.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.Space(10);
                }

                // Selection of the class new type.
                _rect = EditorGUILayout.GetControlRect();
                _rect.width -= 30;

                EditorGUI.LabelField(_rect, _type.Name);

                _rect.x += _rect.width + 5;
                _rect.width = 25;
                if (GUI.Button(_rect, buttonGUI))
                {
                    Shapeshift(_type);
                    return;
                }
            }
        }

        private void Shapeshift(Type _shape)
        {
            bool _isScriptableObject = target is ScriptableObject;

            object _newShape;
            if (_isScriptableObject)
            {
                _newShape = CreateInstance(_shape);
            }
            else
                _newShape = ((Component)target).gameObject.AddComponent(_shape);

            // Copy all available field values.
            FieldInfo[] _originFields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] _fields = _shape.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int _i = 0; _i < _fields.Length; _i++)
            {
                bool _hasFound = false;
                for (int _j = 0; _j < _originFields.Length; _j++)
                {
                    if (_originFields[_j].Name == _fields[_i].Name)
                    {
                        _hasFound = true;
                        break;
                    }
                }

                if (_hasFound)
                    _fields[_i].SetValue(_newShape, _fields[_i].GetValue(target));
            }

            // Replace previous object and select it.
            if (_isScriptableObject)
            {
                string _path = AssetDatabase.GetAssetPath(target.GetInstanceID());
                AssetDatabase.CreateAsset((Object)_newShape, _path);
            }
            else
                DestroyImmediate(target);

            Selection.activeObject = (Object)_newShape;
            Close();
        }
        #endregion
    }
}
