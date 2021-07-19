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
    /// Picker window for <see cref="GameObject"/>s with components of required type(s).
    /// </summary>
    public class GameObjectPicker : EditorWindow
    {
        #region Object Infos
        private struct ObjectInfos
        {
            public GameObject Object;
            public string Name;

            public bool IsVisible;

            // -----------------------

            public ObjectInfos(GameObject _object, string _name)
            {
                Object = _object;
                Name = _name;

                IsVisible = true;
            }

            public ObjectInfos(GameObject _object) : this(_object, _object.name) { }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and show a new <see cref="GameObject"/> picker window.
        /// </summary>
        /// <param name="_requiredTypes">Required component(s) on selectable <see cref="GameObject"/>s.</param>
        /// <param name="_onSelect">Callback on object selection.</param>
        /// <param name="_allowSceneObjects">Are scene objects selectable from the window?</param>
        public static GameObjectPicker GetWindow(Type[] _requiredTypes, Action<GameObject> _onSelect, bool _allowSceneObjects = true)
        {
            GameObjectPicker _window = GetWindow<GameObjectPicker>(true, "GameObject Picker");
            _window.Initialize(_requiredTypes, _onSelect, _allowSceneObjects);
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        public const float LineHeight = 14f;

        private readonly Color selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private readonly GUIContent[] tabs = new GUIContent[]
                                                    {
                                                        new GUIContent("Assets", "All matching asset objects."),
                                                        new GUIContent("Scene", "All matching scene objects.")
                                                    };

        private ObjectInfos[] assetObjects = new ObjectInfos[] { };
        private ObjectInfos[] sceneObjects = new ObjectInfos[] { };

        private Type[] requiredTypes = null;
        private Action<GameObject> onSelect = null;

        private string searchFilter = string.Empty;
        private int selectedObject = -1;

        private int selectedTab = 1;
        private Vector2 scroll = new Vector2();

        private readonly double doubleClickMaxInterval = .5f;
        private double lastClickTime = 0f;

        // -----------------------

        private void OnGUI()
        {
            // Toolbar with search filter.
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter);
            if (_searchFilter != searchFilter)
            {
                searchFilter = _searchFilter;

                UpdateVisibility(assetObjects);
                UpdateVisibility(sceneObjects);
            }

            EditorGUILayout.EndHorizontal();

            // Object selection.
            int _selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            if (_selectedTab != selectedTab)
            {
                selectedTab = _selectedTab;
                selectedObject = -1;

                scroll = Vector2.zero;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (selectedTab)
            {
                case 0:
                    DrawObjectSelector(assetObjects);
                    break;

                case 1:
                    DrawObjectSelector(sceneObjects);
                    break;

                default:
                    break;
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Initialization
        private void Initialize(Type[] _requiredTypes, Action<GameObject> _onSelect, bool _allowSceneObjects)
        {
            requiredTypes = _requiredTypes;
            onSelect = _onSelect;

            assetObjects = GetMatchingObjects(EnhancedEditorUtility.LoadAssets<GameObject>());
            sceneObjects = _allowSceneObjects ? GetMatchingObjects(FindObjectsOfType<GameObject>())
                                              : new ObjectInfos[] { };
        }

        private ObjectInfos[] GetMatchingObjects(GameObject[] _objects)
        {
            List<ObjectInfos> _list = new List<ObjectInfos>() { new ObjectInfos(null, "None") };
            for (int _i = _objects.Length; _i-- > 0;)
            {
                GameObject _object = _objects[_i];
                if (IsObjectValid(_object))
                {
                    _list.Add(new ObjectInfos(_object));
                }
            }

            ObjectInfos[] _result = _list.ToArray();
            Array.Sort(_result, (a, b) => a.Name.CompareTo(b.Name));

            return _result;

            // ----- Local Methods ----- //

            bool IsObjectValid(GameObject _object)
            {
                for (int _j = 0; _j < requiredTypes.Length; _j++)
                {
                    if (!_object.GetComponent(requiredTypes[_j]))
                        return false;
                }

                return true;
            }
        }
        #endregion

        #region Utility
        private void DrawObjectSelector(ObjectInfos[] _objects)
        {
            // Draw available objects.
            EditorGUI.indentLevel++;

            for (int _i = 0; _i < _objects.Length; _i++)
            {
                ObjectInfos _object = _objects[_i];
                if (_object.IsVisible)
                {
                    DrawObjet(_object, _i + 1);
                }
            }

            EditorGUI.indentLevel--;

            // Deselect on empty space click.
            Event _current = Event.current;
            if (_current.type == EventType.MouseDown)
            {
                selectedObject = -1;
                _current.Use();
            }
        }

        private void DrawObjet(ObjectInfos _object, int _index)
        {
            // Selected background.
            Rect _position = EditorGUILayout.GetControlRect(true, LineHeight);
            if (selectedObject == _index)
            {
                EditorGUI.DrawRect(_position, selectedColor);
            }

            if (GUI.Button(_position, string.Empty, EditorStyles.label))
            {
                onSelect(_object.Object);

                // Close window on double click.
                double _time = EditorApplication.timeSinceStartup;
                if ((selectedObject == _index) && ((_time - lastClickTime) < doubleClickMaxInterval))
                {
                    Close();
                }

                selectedObject = _index;
                lastClickTime = _time;
            }

            if (_object.Object)
            {
                EditorGUI.LabelField(_position, EditorGUIUtility.ObjectContent(_object.Object, typeof(GameObject)));
            }
            else
            {
                _position.xMin += 15f;
                EditorGUI.LabelField(_position, _object.Name);
            }
        }

        private void UpdateVisibility(ObjectInfos[] _objects)
        {
            for (int _i = 0; _i < _objects.Length; _i++)
            {
                _objects[_i].IsVisible = _objects[_i].Name.ToLower().Contains(searchFilter.ToLower());
            }
        }
        #endregion
    }
}
