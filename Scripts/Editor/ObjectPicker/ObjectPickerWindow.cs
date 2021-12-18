// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ObjectSelector.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ObjectListArea.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ObjectListLocalGroup.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/EditorCache.cs
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="GameObject"/> and <see cref="Component"/> picker window, allowing to only select objects with some specific components and / or interfaces.
    /// </summary>
    public class ObjectPickerWindow : EditorWindow
    {
        #region Styles
        private static class Styles
        {
            public static readonly GUIStyle PickerTabStyle = new GUIStyle("ObjectPickerTab");
            public static readonly GUIStyle PickerBackgroundStyle = new GUIStyle("ProjectBrowserIconAreaBg");
            public static readonly GUIStyle ObjectLabelStyle = new GUIStyle("ProjectBrowserGridLabel");
            public static readonly GUIStyle PreviewDropShadowStyle = new GUIStyle("ProjectBrowserTextureIconDropShadow");
            public static readonly GUIStyle InspectorBackgroundStyle = new GUIStyle("PopupCurveSwatchBackground");
            public static readonly GUIStyle BottomResizeStyle = new GUIStyle("WindowBottomResize");
            public static readonly GUIStyle PreviewBackgroundStyle = new GUIStyle("ObjectPickerPreviewBackground");
            public static readonly GUIStyle SmallStatusStyle = new GUIStyle("ObjectPickerSmallStatus");
        }
        #endregion

        #region Object Info
        private struct ObjectInfo
        {
            public GameObject Object;
            public string Name;
            public bool IsVisible;

            // -----------------------

            public ObjectInfo(GameObject _object, string _name)
            {
                Object = _object;
                Name = _name;
                IsVisible = true;
            }

            public ObjectInfo(GameObject _object) : this(_object, _object.name) { }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and shows a new <see cref="Component"/> picker window, allowing to only select objects with some specific components and / or interfaces.
        /// </summary>
        /// <param name="_controlID">Picker associated control id. Use the same id to get the newly selected object with <see cref="GetSelectedObject(int, out Component)"/>).</param>
        /// <param name="_objectType">Selected object type.</param>
        /// <inheritdoc cref="GetWindow(int, GameObject, Type[], bool, Action{GameObject}"/>
        public static ObjectPickerWindow GetWindow(int _controlID, Component _selectedObject, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects,
                                                   Action<Component> _onSelectObject = null)
        {
            if (!ArrayUtility.Contains(_requiredTypes, _objectType))
            {
                ArrayUtility.Add(ref _requiredTypes, _objectType);
            }

            GameObject _gameObject = (_selectedObject != null)
                                   ? _selectedObject.gameObject
                                   : null;


            ObjectPickerWindow _window = GetWindow(_controlID, _gameObject, _requiredTypes, _allowSceneObjects, (go) =>
            {
                Component _component = GetSelectedComponent(go);
                _onSelectObject?.Invoke(_component);
            });

            objectType = _objectType;
            return _window;
        }

        /// <summary>
        /// Creates and shows a new <see cref="GameObject"/> picker window, allowing to only select objects with some specific components and / or interfaces.
        /// </summary>
        /// <param name="_controlID">Picker associated control id. Use the same id to get the newly selected object with <see cref="GetSelectedObject(int, out GameObject)"/>).</param>
        /// <param name="_selectedObject">The currently selected object in picker.</param>
        /// <param name="_requiredTypes">Only the objects possessing all of these required components will be displayed
        /// (must either be a component or an interface).</param>
        /// <param name="_allowSceneObjects">Allow or not to assign scene objects.</param>
        /// <param name="_onSelectObject">Callback for whenever a new object is selected by the user.</param>
        /// <returns><see cref="ObjectPickerWindow"/> instance on screen.</returns>
        public static ObjectPickerWindow GetWindow(int _controlID, GameObject _selectedObject, Type[] _requiredTypes, bool _allowSceneObjects,
                                                   Action<GameObject> _onSelectObject = null)
        {
            ObjectPickerWindow _window = GetWindow<ObjectPickerWindow>(true, string.Empty, true);
            _window.minSize = new Vector2(200f, 335f);

            _window.Initialize(_controlID, _selectedObject, _requiredTypes, _allowSceneObjects, _onSelectObject);
            _window.Show();

            objectType = null;
            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float SizeSliderWidth = 50f;
        private const float SizeSliderMinValue = 1f;
        private const float SizeSliderMaxValue = 5f;

        private const string DataKey = "EnhancedObjectPicker";
        private const string SearchFieldControlName = "SearchFilter";

        private static readonly ObjectInfo nullObject = new ObjectInfo(null, "None");
        private static readonly GUIContent[] tabsGUI = new GUIContent[]
                                                    {
                                                        new GUIContent("Assets", "All matching asset objects."),
                                                        new GUIContent("Scene", "All matching scene objects.")
                                                    };

        private static int controlID = 0;
        private static bool hasSelectedObject = false;
        private static GameObject selectedObject = null;
        private static Type objectType = null;

        private static UnityEditor.Editor objectEditor = null;

        private ObjectInfo[] assetObjects = new ObjectInfo[] { };
        private ObjectInfo[] sceneObjects = new ObjectInfo[] { };
        private bool allowSceneObjects = true;

        private Type[] requiredTypes = null;
        private Action<GameObject> onSelectObject = null;

        [SerializeField] private float sizeSlider = 1f;
        [SerializeField] private float inspectorHeight = 0f;
        [SerializeField] private float lastWideInspectorHeight = WideInspectorMinimumHeight;

        private string searchFilter = string.Empty;
        private bool doFocusSearchField = true;

        private Vector2 scroll = new Vector2();
        private int selectedTab = 0;

        // -----------------------

        private void OnEnable()
        {
            // Load values.
            string _data = EditorPrefs.GetString(DataKey, JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(_data, this);

            ClearEditor();
        }

        private void OnGUI()
        {
            // Search field.
            Rect _toolbarRect;
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _toolbarRect = EditorGUILayout.GetControlRect();
            }

            // Tab selection.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Using an array iteration instead of two direct toggles allows to easily add new tabs whenever we want.
                int _selectedTab = selectedTab;
                for (int _i = 0; _i < tabsGUI.Length; _i++)
                {
                    // Simply ignore scene tab in this case.
                    if (!allowSceneObjects && (_i == 1))
                        continue;

                    GUIContent _tabGUI = tabsGUI[_i];
                    bool _isSelected = _selectedTab == _i;

                    if (GUILayout.Toggle(_isSelected, _tabGUI, Styles.PickerTabStyle))
                    {
                        _selectedTab = _i;
                    }
                }

                if (_selectedTab != selectedTab)
                {
                    selectedTab = _selectedTab;
                    scroll = Vector2.zero;
                }

                GUILayout.FlexibleSpace();

                // Size slider.
                sizeSlider = GUILayout.HorizontalSlider(sizeSlider, SizeSliderMinValue, SizeSliderMaxValue, GUILayout.Width(SizeSliderWidth));

                // Use this to draw the bottom line.
                GUILayout.Label(GUIContent.none, Styles.PickerTabStyle, GUILayout.ExpandWidth(true));
            }

            // Background color.
            Rect _position = new Rect(0f, GUILayoutUtility.GetLastRect().yMax, position.width, position.height);
            GUI.Label(_position, GUIContent.none, Styles.PickerBackgroundStyle);

            // Object picker.
            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                switch (selectedTab)
                {
                    case 0:
                        DrawPicker(assetObjects, _position.position);
                        break;

                    case 1:
                        DrawPicker(sceneObjects, _position.position);
                        break;

                    default:
                        break;
                }
            }

            // Inspector.
            DrawInspector();

            // Finally, draw the search field to leave keyboard inputs for selecting objects.
            string _searchFilter = EnhancedEditorGUI.ToolbarSearchField(SearchFieldControlName, _toolbarRect, searchFilter);
            if (_searchFilter != searchFilter)
            {
                searchFilter = _searchFilter;

                FilterObjects(assetObjects);
                FilterObjects(sceneObjects);
            }

            // Focus this search field once it has been drawn.
            if (doFocusSearchField)
            {
                GUI.FocusControl(SearchFieldControlName);
                doFocusSearchField = false;
            }
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDisable()
        {
            ClearEditor();

            // Save values.
            string _data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(DataKey, _data);
        }
        #endregion

        #region Initialization
        private void Initialize(int _controlID, GameObject _selectedObject, Type[] _requiredTypes, bool _allowSceneObjects, Action<GameObject> _onSelectObject)
        {
            // Only keep eligible types.
            for (int _i = _requiredTypes.Length; _i-- > 0;)
            {
                Type _type = _requiredTypes[_i];
                if (!EnhancedEditorUtility.IsComponentOrInterface(_type))
                {
                    ArrayUtility.RemoveAt(ref _requiredTypes, _i);
                }
            }

            // Title.
            string _title = "Select ";
            if (_requiredTypes.Length == 0)
            {
                _title += "GameObject";
            }
            else if (_requiredTypes.Length == 1)
            {
                _title += _requiredTypes[0].Name;
            }
            else
            {
                _title += $"Object with {_requiredTypes[0].Name}";
                for (int _i = 1; _i < _requiredTypes.Length; _i++)
                {
                    _title += $", {_requiredTypes[_i].Name}";
                }
            }

            titleContent = new GUIContent(_title);

            // Settings.
            controlID = _controlID;
            selectedObject = _selectedObject;
            requiredTypes = _requiredTypes;
            onSelectObject = _onSelectObject;
            allowSceneObjects = _allowSceneObjects;

            CreateEditor();

            // Objects.
            assetObjects = GetMatchingObjects(EnhancedEditorUtility.LoadAssets<GameObject>());
            if (_allowSceneObjects)
            {
                sceneObjects = GetMatchingObjects(FindObjectsOfType<GameObject>());
                selectedTab = Array.Exists(assetObjects, o => o.Object == selectedObject)
                            ? 0
                            : 1;
            }
            else
            {
                sceneObjects = new ObjectInfo[] { };
                selectedTab = 0;
            }
        }

        private ObjectInfo[] GetMatchingObjects(GameObject[] _objects)
        {
            List<ObjectInfo> _matchingObjects = new List<ObjectInfo>();
            foreach (GameObject _gameObject in _objects)
            {
                if (IsObjectValid(_gameObject))
                {
                    ObjectInfo _object = new ObjectInfo(_gameObject);
                    _matchingObjects.Add(_object);
                }
            }

            _matchingObjects.Sort((a, b) => a.Name.CompareTo(b.Name));
            _matchingObjects.Insert(0, nullObject);

            return _matchingObjects.ToArray();

            // ----- Local Method ----- \\

            bool IsObjectValid(GameObject _object)
            {
                foreach (Type _type in requiredTypes)
                {
                    if (!_object.GetComponent(_type))
                        return false;
                }

                return true;
            }
        }
        #endregion

        #region GUI Draw
        private const float GridObjectSize = 24f;
        private const float ListObjectSize = 16f;

        private const float GridMininimumSpacing = 15f;
        private const float GridVerticalSpacing = 30f;

        private const float PickerMinimumHeight = 250f;
        private const float WideInspectorMinimumHeight = 85f;
        private const float BottomResizeWidth = 32f;
        private const float ResizeCursorOffset = 100f;

        private static readonly Color gridPreviewSelectedColor = new Color(0.175f, 0.235f, .37f, .2f);
        private static readonly Color gridContentSelectedColor = new Color(0.85f, 0.9f, 1f);
        private static readonly Color gridLabelSelectedColor = new Color(0f, .95f, 1f);
        private static readonly Color inspectorSeparatorColor = new Color(.25f, .25f, .25f);

        private static readonly RectOffset dropShadowBackgroundOffset = new RectOffset(1, 1, 1, 1);
        private int inspectorResizeState = 0;
        private bool doFocusSelection = false;

        // -----------------------

        private void DrawPicker(ObjectInfo[] _objects, Vector2 _origin)
        {
            bool _focusSelection = doFocusSelection && (Event.current.type == EventType.Repaint);
            int _selectedIndex = -1;

            // Display the objects as a list if the slider value is at its minimum, and as a grid otherwise.
            if (sizeSlider == SizeSliderMinValue)
            {
                using (var _scope = new EditorGUI.IndentLevelScope())
                {
                    for (int _i = 0; _i < _objects.Length; _i++)
                    {
                        if (!_objects[_i].IsVisible)
                            continue;

                        GameObject _object = _objects[_i].Object;
                        bool _isSelected = selectedObject == _object;

                        if (_isSelected)
                            _selectedIndex = _i;

                        // Background color.
                        Rect _position = EditorGUILayout.GetControlRect(true, ListObjectSize, GUILayout.MaxWidth(position.width - 10f));
                        Rect _temp = new Rect(_position)
                        {
                            x = 0f,
                            width = position.width
                        };

                        EnhancedEditorGUI.BackgroundLine(_temp, _isSelected, _i);

                        // Draw null object (first index) without any icon.
                        if (_i == 0)
                        {
                            _position.xMin = 18f;
                            EditorGUI.LabelField(_position, _objects[_i].Name);
                        }
                        else
                        {
                            // Don't store the object content as it can need multiple calls for Unity to properly load it, and is already cached internally.
                            EditorGUI.LabelField(_position, EditorGUIUtility.ObjectContent(_object, typeof(GameObject)));
                        }

                        // Select on click.
                        ObjectSelection(_temp, _object);

                        // Scroll focus.
                        if (_focusSelection && _isSelected)
                            Focus(_temp);
                    }
                }

                // Selected object switch.
                int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                switch (_switch)
                {
                    case -1:
                        if (!SelectFirstObject())
                        {
                            for (int _i = _selectedIndex; _i-- > 0;)
                            {
                                if (DoSelectObject(_i))
                                    break;
                            }
                        }
                        break;

                    case 1:
                        if (!SelectFirstObject())
                        {
                            for (int _i = _selectedIndex + 1; _i < _objects.Length; _i++)
                            {
                                if (DoSelectObject(_i))
                                    break;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                // Position calculs.
                float _size = GridObjectSize * sizeSlider;
                Rect _position = EditorGUILayout.GetControlRect(false, 5f);

                float _gridCount = Mathf.Max(1f, Mathf.Floor(_position.width / (_size + GridMininimumSpacing)));
                float _spacing = (_position.width - (_size * _gridCount)) / (_gridCount + 1f);
                int _index = -1;

                Rect _temp = new Rect(_position)
                {
                    y = _position.yMax,
                    width = _size,
                    height = _size
                };

                // Draw all objects as a grid.
                while (_index < _objects.Length)
                {
                    if (_index == _objects.Length - 1)
                        break;

                    _temp.x = _position.x + _spacing;

                    // Line.
                    for (int _i = 0; _i < _gridCount; _i++)
                    {
                        // Increment index.
                        _index++;
                        if (_index == _objects.Length)
                        {
                            if (_i == 0)
                                _temp.y -= _temp.height + GridVerticalSpacing;

                            break;
                        }

                        if (!_objects[_index].IsVisible)
                        {
                            _i--;
                            continue;
                        }

                        GameObject _object = _objects[_index].Object;
                        bool _isSelected = selectedObject == _object;

                        if (_isSelected)
                            _selectedIndex = _index;

                        // Do not draw null object preview.
                        if (_index > 0)
                        {
                            // Catch preview null ref exception.
                            try
                            {
                                Texture2D _preview = AssetPreview.GetAssetPreview(_object);
                                if (_preview != null)
                                {
                                    // Drop shadow background.
                                    if (Event.current.type == EventType.Repaint)
                                    {
                                        Rect _dropShadowPos = dropShadowBackgroundOffset.Remove(Styles.PreviewDropShadowStyle.border.Add(_temp));
                                        Styles.PreviewDropShadowStyle.Draw(_dropShadowPos, GUIContent.none, false, false, false, false);
                                    }

                                    // Preview and selected feedback.
                                    EditorGUI.DrawPreviewTexture(_temp, _preview);
                                    if (selectedObject == _object)
                                    {
                                        EditorGUI.DrawRect(_temp, gridPreviewSelectedColor);
                                    }
                                }
                                else
                                {
                                    Color _color = _isSelected
                                                 ? gridContentSelectedColor
                                                 : Color.white;

                                    using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(EditorStyles.label, TextAnchor.MiddleCenter))
                                    using (var _colorScope = EnhancedGUI.GUIColor.Scope(_color))
                                    {
                                        GUI.Label(_temp, EditorGUIUtility.ObjectContent(_object, typeof(GameObject)).image, EditorStyles.label);
                                    }
                                }
                            }
                            catch (NullReferenceException) { }
                        }

                        Rect _labelPos = new Rect(_temp)
                        {
                            y = _temp.yMax + 2f,
                            height = Styles.ObjectLabelStyle.lineHeight
                        };

                        // Only draw the label on repaint event, as the method GUIStyle.Draw is not allowed to be called during another event.
                        if (Event.current.type == EventType.Repaint)
                        {
                            string _label = _objects[_index].Name;
                            int _length = (int)_labelPos.width / 7;

                            if (_label.Length > _length)
                            {
                                _label = $"{_label.Remove(_length)}...";
                            }

                            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
                            Color _color = _isSelected
                                         ? gridLabelSelectedColor
                                         : Color.white;

                            _labelPos.width = Styles.ObjectLabelStyle.CalcSize(_labelGUI).x + 2f;
                            _labelPos.x += (_temp.width - _labelPos.width) * .5f;

                            using (var _scope = EnhancedGUI.GUIBackgroundColor.Scope(_color))
                            {
                                Styles.ObjectLabelStyle.Draw(_labelPos, _labelGUI, false, false, _isSelected, true);
                            }
                        }

                        _temp.yMax += _labelPos.height + 4f;

                        // Select on click.
                        ObjectSelection(_temp, _object);

                        // Scroll focus.
                        if (_focusSelection && _isSelected)
                            Focus(_temp);

                        // Position calculs.
                        _temp.x += _temp.width + _spacing;
                        _temp.yMax -= _labelPos.height + 4f;
                    }

                    _temp.y += _temp.height + GridVerticalSpacing;
                }

                // Vertical object switch.
                int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                switch (_switch)
                {
                    case -1:
                        if (!SelectFirstObject())
                        {
                            int _count = (int)_gridCount;
                            _index = _selectedIndex;

                            for (int _i = _selectedIndex; _i-- > 0;)
                            {
                                if (_objects[_i].IsVisible)
                                {
                                    _index = _i;
                                    _count--;

                                    if (_count == 0)
                                        break;
                                }
                            }

                            SelectObject(_objects[_index].Object);
                        }
                        break;

                    case 1:
                        if (!SelectFirstObject())
                        {
                            int _count = (int)_gridCount;
                            _index = _selectedIndex;

                            for (int _i = _selectedIndex + 1; _i < _objects.Length; _i++)
                            {
                                if (_objects[_i].IsVisible)
                                {
                                    _index = _i;
                                    _count--;

                                    if (_count == 0)
                                        break;
                                }
                            }

                            SelectObject(_objects[_index].Object);
                        }
                        break;

                    default:
                        break;
                }

                // Horizontal object switch.
                if (GUIUtility.keyboardControl == 0)
                {
                    _switch = EnhancedEditorGUIUtility.HorizontalSelectionKeys();
                    switch (_switch)
                    {
                        case -1:
                            if (!SelectFirstObject())
                            {
                                for (int _i = _selectedIndex; _i-- > 0;)
                                {
                                    if (DoSelectObject(_i))
                                        break;
                                }
                            }
                            break;

                        case 1:
                            if (!SelectFirstObject())
                            {
                                for (int _i = _selectedIndex + 1; _i < _objects.Length; _i++)
                                {
                                    if (DoSelectObject(_i))
                                        break;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }

                float _height = EnhancedEditorGUI.ManageDynamicControlHeight(GUIContent.none, _temp.y - _position.yMax);
                GUILayout.Space(_height);
            }

            // ----- Local Methods ----- \\

            void ObjectSelection(Rect _position, GameObject _object)
            {
                if (EnhancedEditorGUIUtility.MouseDown(_position))
                {
                    SelectObject(_object);
                    if (Event.current.clickCount > 1)
                    {
                        Close();
                    }
                }
            }

            bool SelectFirstObject()
            {
                // Only select the first object if the selected one is not visible.
                if (_selectedIndex != -1)
                    return false;

                _selectedIndex = 0;
                for (int _i = 1; _i < _objects.Length; _i++)
                {
                    if (_objects[_i].IsVisible)
                    {
                        _selectedIndex = _i;
                        break;
                    }
                }

                SelectObject(_objects[_selectedIndex].Object);
                return true;
            }

            bool DoSelectObject(int _index)
            {
                if (_objects[_index].IsVisible)
                {
                    SelectObject(_objects[_index].Object);
                    return true;
                }

                return false;
            }

            void Focus(Rect _position)
            {
                Vector2 _areaSize = position.size - _origin;
                _areaSize.y -= (inspectorHeight != 0f)
                             ? inspectorHeight
                             : 22f;

                scroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(scroll, _position, _areaSize);
                doFocusSelection = false;
                Repaint();
            }
        }

        private void DrawInspector()
        {
            // Inspector background and separator.
            Rect _position = EditorGUILayout.GetControlRect();
            Rect _temp = new Rect(_position)
            {
                x = 0f,
                width = position.width,
                y = _position.y - 2f,
                yMax = position.height
            };

            GUI.Box(_temp, GUIContent.none, Styles.InspectorBackgroundStyle);

            _temp.height = 1f;
            EditorGUI.DrawRect(_temp, inspectorSeparatorColor);

            // Inspector resize icon.
            Rect _resizePos = new Rect()
            {
                x = (position.width - BottomResizeWidth) * .5f,
                y = _temp.y + 2f,
                width = BottomResizeWidth,
                height = Styles.BottomResizeStyle.fixedHeight
            };

            GUI.Label(_resizePos, GUIContent.none, Styles.BottomResizeStyle);

            // Inspector resize.
            Event _event = Event.current;
            int _controlID = EnhancedEditorGUIUtility.GetControlID(FocusType.Passive);
            EventType _eventType = _event.GetTypeForControl(_controlID);

            switch (inspectorResizeState)
            {
                // Not resizing yet.
                case 0:
                {
                    _temp.y -= 1f;
                    _temp.height = 23f;

                    EditorGUIUtility.AddCursorRect(_temp, MouseCursor.ResizeVertical);
                    if ((_eventType == EventType.MouseDown) && _temp.Contains(_event.mousePosition))
                    {
                        // Prepare resize.
                        GUIUtility.hotControl = _controlID;
                        inspectorResizeState = 1;

                        _event.Use();
                    }
                }
                break;

                // Preparing to resize.
                case 1:
                {
                    _temp.y -= 1f;
                    _temp.yMax = position.height;

                    EditorGUIUtility.AddCursorRect(_temp, MouseCursor.ResizeVertical);
                    if (_eventType == EventType.MouseDrag)
                    {
                        // Start resize.
                        GUIUtility.hotControl = _controlID;
                        GUI.changed = true;

                        inspectorResizeState = 2;
                        _event.Use();
                    }
                    else if (_eventType == EventType.MouseUp)
                    {
                        // Switch inspector wide mode.
                        inspectorHeight = (inspectorHeight == 0f)
                                        ? lastWideInspectorHeight
                                        : 0f;

                        GUIUtility.hotControl = 0;
                        inspectorResizeState = 0;

                        _event.Use();
                    }
                }
                break;

                // Resizing.
                case 2:
                    _temp.y -= ResizeCursorOffset + 1f;
                    _temp.yMax = position.height + ResizeCursorOffset;

                    EditorGUIUtility.AddCursorRect(_temp, MouseCursor.ResizeVertical);
                    if (_eventType == EventType.MouseDrag)
                    {
                        // Resize inspector height.
                        inspectorHeight = Mathf.Clamp(position.height - _event.mousePosition.y, 0f, position.height - PickerMinimumHeight);
                        if (inspectorHeight < WideInspectorMinimumHeight)
                        {
                            inspectorHeight = (inspectorHeight > (WideInspectorMinimumHeight * .5f))
                                            ? WideInspectorMinimumHeight
                                            : 0f;
                        }

                        // Save last wide inspector height.
                        if (inspectorHeight >= WideInspectorMinimumHeight)
                            lastWideInspectorHeight = inspectorHeight;

                        GUIUtility.hotControl = _controlID;
                        GUI.changed = true;

                        _event.Use();
                    }
                    else if (_eventType == EventType.MouseUp)
                    {
                        // Stop resize.
                        GUIUtility.hotControl = 0;
                        inspectorResizeState = 0;

                        _event.Use();
                    }
                    break;

                default:
                    break;
            }

            // Inspector.
            bool _isWideInspector = inspectorHeight != 0f;
            string _label;

            if (selectedObject != null)
            {
                string _type = ObjectNames.NicifyVariableName(selectedObject.GetType().Name);
                string _path = AssetDatabase.GetAssetPath(selectedObject);

                _label = _isWideInspector
                       ? $"{selectedObject.name}\n{_type}\n{_path}"
                       : $"{selectedObject.name} ({_type})     {_path}";
            }
            else
            {
                _label = nullObject.Name;
            }

            // Reserve inspector area.
            if (_isWideInspector)
            {
                // Make sure that the height of the inspector is never too large, in case the user resizes the window.
                inspectorHeight = Mathf.Min(inspectorHeight, position.height - PickerMinimumHeight);

                GUILayout.Space(inspectorHeight - 20f);
                Rect _previewPos = new Rect(_position)
                {
                    y = _position.y + 3f,
                    yMax = position.height - 5f
                };

                _previewPos.width = _previewPos.height;

                // Preview.
                if (objectEditor != null && objectEditor.HasPreviewGUI())
                {
                    objectEditor.OnPreviewGUI(_previewPos, Styles.PreviewBackgroundStyle);
                }

                // Update label position.
                _position.xMin = _previewPos.xMax;
                _position.y += 5f;
                _position.yMax = position.height - 20f;
            }

            using (var _align = EnhancedGUI.GUIStyleAlignment.Scope(Styles.SmallStatusStyle, TextAnchor.MiddleLeft))
            using (var _scope = new EditorGUI.IndentLevelScope())
            {
                _position = EditorGUI.IndentedRect(_position);

                GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
                EditorGUI.DropShadowLabel(_position, _labelGUI, Styles.SmallStatusStyle);
            }
        }
        #endregion

        #region Get Selected Object
        /// <inheritdoc cref="GetSelectedObject(int, out GameObject)"/>
        public static bool GetSelectedObject(int _id, out Component _object)
        {
            if (!GetSelectedObject(_id, out GameObject _gameObject) || (objectType == null))
            {
                _object = null;
                return false;
            }

            _object = GetSelectedComponent(_gameObject);
            return true;
        }

        /// <summary>
        /// Get the newly selected object by the user for a specific control id.
        /// </summary>
        /// <param name="_controlID">Control id to get selected object for.</param>
        /// <param name="_object">Newly selected object.</param>
        /// <returns>True if the user selected a new object, false otherwise.</returns>
        public static bool GetSelectedObject(int _controlID, out GameObject _object)
        {
            if (hasSelectedObject && (_controlID == controlID))
            {
                _object = selectedObject;
                hasSelectedObject = false;

                GUI.changed = true;
                return true;
            }

            _object = null;
            return false;
        }
        #endregion

        #region Utility
        private static Component GetSelectedComponent(GameObject _object)
        {
            Component _component = _object?.GetComponent(objectType);
            return _component;
        }

        private void FilterObjects(ObjectInfo[] _objects)
        {
            string _searchFilter = searchFilter.ToLower();
            for (int _i = 1; _i < _objects.Length; _i++)
            {
                _objects[_i].IsVisible = _objects[_i].Name.ToLower().Contains(_searchFilter);
            }
        }

        private void SelectObject(GameObject _object)
        {
            selectedObject = _object;
            hasSelectedObject = true;
            doFocusSelection = true;

            onSelectObject?.Invoke(_object);

            ClearEditor();
            CreateEditor();

            InternalEditorUtility.RepaintAllViews();
        }

        private void ClearEditor()
        {
            if (objectEditor != null)
                DestroyImmediate(objectEditor);
        }

        private void CreateEditor()
        {
            if (selectedObject != null)
                objectEditor = UnityEditor.Editor.CreateEditor(selectedObject);
        }
        #endregion
    }
}
