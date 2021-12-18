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
    /// <summary>
    /// Editor window available from any <see cref="MonoBehaviour"/> or <see cref="ScriptableObject"/> menu,
    /// allowing users to shape this object into one of its parent or child classes while keeping all of its assigned values.
    /// </summary>
    public class ShapeshifterWindow : EditorWindow
    {
        #region Styles
        private static class Styles
        {
            public static readonly GUIStyle TabStyle = new GUIStyle("ObjectPickerTab");
            public static readonly GUIStyle BackgroundStyle = new GUIStyle("ProjectBrowserIconAreaBg");
        }
        #endregion

        #region Shape Type
        [Serializable]
        private class ShapeType : IComparable<ShapeType>
        {
            public Type Type = null;

            public bool IsVisible = true;
            public bool IsSelected = false;

            // -----------------------

            public ShapeType(Type _type)
            {
                Type = _type;
            }

            // -----------------------

            int IComparable<ShapeType>.CompareTo(ShapeType _other)
            {
                int _comparison = Type.FullName.CompareTo(_other.Type.FullName);
                return _comparison;
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and shows a new instance of the <see cref="ShapeshifterWindow"/> window
        /// associated with a specific <see cref="MenuCommand"/>.
        /// </summary>
        /// <param name="_command">Command associated with this shapeshifter instance.</param>
        /// <returns><see cref="ShapeshifterWindow"/> instance on screen.</returns>
        [MenuItem("CONTEXT/MonoBehaviour/Shapeshifter")]
        [MenuItem("CONTEXT/ScriptableObject/Shapeshifter")]
        public static ShapeshifterWindow GetWindow(MenuCommand _command)
        {
            ShapeshifterWindow _window = GetWindow<ShapeshifterWindow>(true, "Shapeshifter");
            if (_window.Initialize(_command.context))
            {
                _window.minSize = new Vector2(225f, 300f);

                _window.Show();
                return _window;
            }

            return null;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string UndoRecordTitle = "Shapeshifter Changes";

        private static readonly GUIContent selectGUI = new GUIContent(string.Empty, "Shapeshift this object into a new instance of this class.");
        private static readonly GUIContent[] tabsGUI = new GUIContent[]
                                                    {
                                                        new GUIContent("Parents", "All available parent types for shapeshiting."),
                                                        new GUIContent("Children", "All available child types for shapeshiting.")
                                                    };

        private Object target = null;

        [SerializeField] private ShapeType[] parentTypes = new ShapeType[] { };
        [SerializeField] private ShapeType[] childTypes = new ShapeType[] { };

        [SerializeField] private int selectedTab = 0;
        [SerializeField] private string searchFilter = string.Empty;

        private Vector2 scroll = new Vector2();
        private int ignoredTab = -1;

        // -----------------------

        private void OnGUI()
        {
            // Close the window if it has not been initialized.
            if (target == null)
            {
                Close();
                return;
            }

            Undo.RecordObject(this, UndoRecordTitle);

            // Search field.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter);
                if (_searchFilter != searchFilter)
                {
                    searchFilter = _searchFilter;
                    FilterTypes();
                }
            }

            // Tab selection.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Using an array iteration instead of two direct toggles allows to easily add new tabs whenever we want.
                int _selectedTab = selectedTab;
                for (int _i = 0; _i < tabsGUI.Length; _i++)
                {
                    // Skip ignored tab.
                    if (ignoredTab == _i)
                        continue;

                    GUIContent _tabGUI = tabsGUI[_i];
                    bool _isSelected = _selectedTab == _i;

                    if (GUILayout.Toggle(_isSelected, _tabGUI, Styles.TabStyle))
                    {
                        _selectedTab = _i;
                    }
                }

                if (_selectedTab != selectedTab)
                {
                    selectedTab = _selectedTab;
                    scroll = Vector2.zero;
                }

                // Use this to draw the bottom line.
                GUILayout.Label(GUIContent.none, Styles.TabStyle, GUILayout.ExpandWidth(true));
            }

            // Background color.
            Rect _position = new Rect(0f, GUILayoutUtility.GetLastRect().yMax, position.width, position.height);
            GUI.Label(_position, GUIContent.none, Styles.BackgroundStyle);

            // Draw shapeshiftable types.
            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                Rect _fullPosition = new Rect(Vector2.zero, position.size);

                switch (selectedTab)
                {
                    // Parent types.
                    case 0:
                        DrawTypes(parentTypes);

                        if (EnhancedEditorGUIUtility.DeselectionClick(_position))
                        {
                            foreach (ShapeType _shapeType in parentTypes)
                                _shapeType.IsSelected = false;
                        }
                        break;

                    // Child types.
                    case 1:
                        DrawTypes(childTypes);

                        if (EnhancedEditorGUIUtility.DeselectionClick(_position))
                        {
                            foreach (ShapeType _shapeType in childTypes)
                                _shapeType.IsSelected = false;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void OnLostFocus()
        {
            Close();
        }
        #endregion

        #region GUI Draw
        private static readonly EditorColor peerLineColor = new EditorColor(new Color(.75f, .75f, .75f),
                                                                            new Color(.25f, .25f, .25f));

        // -----------------------

        private void DrawTypes(ShapeType[] _types)
        {
            // Draw each available class for shapeshifting.
            using (var _scope = new EditorGUI.IndentLevelScope())
            {
                string _namespace = string.Empty;
                int _index = 0;

                foreach (ShapeType _type in _types)
                {
                    if (!_type.IsVisible)
                        continue;

                    Type _targetType = _type.Type;

                    // Namespace header.
                    if (_targetType.Namespace != _namespace)
                    {
                        if (!string.IsNullOrEmpty(_namespace))
                            GUILayout.Space(5f);

                        _namespace = _targetType.Namespace;

                        using (EnhancedEditorGUI.ZeroIndentScope())
                        {
                            Rect _temp = EditorGUILayout.GetControlRect();
                            _temp.xMin += 3f;

                            EnhancedEditorGUI.UnderlinedLabel(_temp, $"{_namespace}:", EditorStyles.boldLabel);
                            GUILayout.Space(3f);
                        }
                    }

                    Rect _position = EditorGUILayout.GetControlRect();
                    Rect _fullPosition = new Rect(_position)
                    {
                        x = 0f,
                        width = position.width
                    };

                    // Background line.
                    EnhancedEditorGUI.BackgroundLine(_fullPosition, _type.IsSelected, _index, EnhancedEditorGUIUtility.GUISelectedColor, peerLineColor);
                    _index++;

                    _position.width -= EnhancedEditorGUIUtility.IconWidth + 10f;

                    // Type label.
                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_targetType.Name, _targetType.FullName);
                    EditorGUI.LabelField(_position, _label);

                    _position.x += _position.width + 5f;
                    _position.y -= 1f;

                    _position.width = EnhancedEditorGUIUtility.IconWidth + 2f;
                    _position.height += 2f;

                    // Shapeshift button.
                    if (EnhancedEditorGUI.IconButton(_position, selectGUI, 2f))
                    {
                        Shapeshift(_targetType);
                        break;
                    }

                    // Selection.
                    if (EnhancedEditorGUIUtility.MouseDown(_fullPosition))
                    {
                        switch (Event.current.clickCount)
                        {
                            // Select on click.
                            case 1:
                                SelectType(_types, _type);
                                break;

                            // Shapeshift on double click.
                            case 2:
                                Shapeshift(_targetType);
                                break;

                            default:
                                break;
                        }
                    }

                    if (_type.IsSelected)
                    {
                        Event _event = Event.current;
                        if ((_event.type == EventType.KeyDown) && (_event.keyCode == KeyCode.Return))
                        {
                            _event.Use();

                            // When using a key event, the scope began with GUILayout.ScrollViewScope
                            // is somehow lost, so let's begin it again to avoid any error.
                            if (!Shapeshift(_targetType))
                            {
                                GUILayout.BeginScrollView(scroll);
                            }

                            break;
                        }

                        int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                        if (_switch != 0)
                        {
                            _switch = Mathf.Clamp(_switch, 0, _types.Length - 1);

                            ShapeType _switchType = _types[_switch];
                            SelectType(_types, _switchType);
                        }
                    }
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Initializes this shapeshifter instance with a specific target object.
        /// </summary>
        /// <param name="_target">Target object to shapeshift.</param>
        public bool Initialize(Object _target)
        {
            // Shapeshifting can only be performed on MonoBehaviour and ScriptableObject instances.
            if (!(_target is MonoBehaviour) && !(_target is ScriptableObject))
            {
                EditorUtility.DisplayDialog("Shapeshifter Not Available",
                                            "Shapeshifting can only be performed on Monobehaviour and ScriptableObject instances.",
                                            "OK");

                Close();
                return false;
            }

            target = _target;
            parentTypes = new ShapeType[] { };
            childTypes = new ShapeType[] { };

            Type _targetType = _target.GetType();
            Type _baseType = _targetType.BaseType;

            while ((_baseType != null) && (_baseType != typeof(MonoBehaviour)) && (_baseType != typeof(ScriptableObject)))
            {
                // Register all parent types.
                if (!_baseType.IsAbstract)
                {
                    ShapeType _parentType = new ShapeType(_baseType);
                    ArrayUtility.Add(ref parentTypes, _parentType);
                }

                _baseType = _baseType.BaseType;
            }

            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var _assembly in _assemblies)
            {
                Type[] _types = _assembly.GetTypes();
                foreach (var _type in _types)
                {
                    // Register all child types.
                    if (!_type.IsAbstract && _type.IsSubclassOf(_targetType))
                    {
                        ShapeType _childType = new ShapeType(_type);
                        ArrayUtility.Add(ref childTypes, _childType);
                    }
                }
            }

            // No shapeshifting available.
            if ((parentTypes.Length == 0) && (childTypes.Length == 0))
            {
                Close();
                EditorUtility.DisplayDialog("No Shapeshifting Available",
                                            "No shapeshiting could be found for the selected object type.\n\n" +
                                            "The shapeshifter can only be used to shape a class instance into one of its parent or child type.",
                                            "OK");

                return false;
            }

            // Initialization.
            selectGUI.image = EditorGUIUtility.FindTexture("RotateTool");

            ignoredTab = (parentTypes.Length == 0)
                       ? 0
                       : ((childTypes.Length == 0)
                       ? 1
                       : -1);

            selectedTab = (ignoredTab == 0)
                        ? 1
                        : 0;

            Array.Sort(parentTypes);
            Array.Sort(childTypes);

            return true;
        }

        private bool Shapeshift(Type _newType)
        {
            if (!EditorUtility.DisplayDialog("Shapeshifting Confirmation",
                                             $"Are you sure you want to shape this object instance into a new {_newType.Name} instance?\n\n" +
                                             "All references to this object will be lost.",
                                             "Yes", "Cancel"))
            {
                Focus();
                return false;
            }

            Object _newShape;

            if (target is ScriptableObject)
            {
                _newShape = CreateInstance(_newType);

                string _path = AssetDatabase.GetAssetPath(target);
                AssetDatabase.CreateAsset(_newShape, _path);
                AssetDatabase.Refresh();
            }
            else
            {
                _newShape = Undo.AddComponent((target as Component).gameObject, _newType);
            }

            EditorUtility.CopySerialized(target, _newShape);

            Undo.DestroyObjectImmediate(target);

            Selection.activeObject = _newShape;
            EditorUtility.SetDirty(_newShape);

            Close();
            return true;
        }

        // -----------------------

        private void FilterTypes()
        {
            string _searchFilter = searchFilter.ToLower();

            foreach (ShapeType _type in parentTypes)
            {
                bool _isVisible = _type.Type.Name.ToLower().Contains(_searchFilter);
                _type.IsVisible = _isVisible;
            }

            foreach (ShapeType _type in childTypes)
            {
                bool _isVisible = _type.Type.Name.ToLower().Contains(_searchFilter);
                _type.IsVisible = _isVisible;
            }
        }

        private void SelectType(ShapeType[] _types, ShapeType _selectedType)
        {
            foreach (ShapeType _shapeType in _types)
                _shapeType.IsSelected = false;

            _selectedType.IsSelected = true;
        }
        #endregion
    }
}
