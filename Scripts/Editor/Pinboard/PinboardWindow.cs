// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;
using PinObject = EnhancedEditor.Editor.Pinboard.PinObject;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// The pinboard window can be used to store and give quick access to your favorite assets and folders in the project.
    /// </summary>
	public class PinboardWindow : EditorWindow
    {
        #region Indent Level
        private struct IndentLevel
        {
            public int Indent;
            public Vector2 StartPosition;
            public float YMax;
            public bool IsDrawn;

            // -----------------------

            public IndentLevel(int _indent)
            {
                Indent = _indent;
                StartPosition = new Vector2(0f, Mathf.Infinity);
                YMax = StartPosition.y;
                IsDrawn = true;
            }

            public IndentLevel(int _indent, Vector2 _startPosition, float _yMax)
            {
                Indent = _indent;
                StartPosition = _startPosition;
                YMax = _yMax;
                IsDrawn = false;
            }

            public IndentLevel(int _indent, Vector2 _startPosition) : this(_indent, _startPosition, _startPosition.y) { }
        }
        #endregion

        #region Sorting Structs
        private struct SortLevel
        {
            public int Index;
            public int Indent;

            public List<SortFolder> Folders;

            // -----------------------

            public SortLevel(int _index, int _indent)
            {
                Index = _index;
                Indent = _indent;

                Folders = new List<SortFolder>();
            }
        }

        private struct SortFolder
        {
            public PinObject Object;
            public int ChildCount;

            // -----------------------

            public SortFolder(PinObject _object)
            {
                Object = _object;
                ChildCount = 0;
            }
        }
        #endregion

        #region Comparers
        private abstract class PinObjectComparer : IComparer<PinObject>
        {
            public static bool DoSortAscending = true;

            // -----------------------

            public int Compare(PinObject _a, PinObject _b)
            {
                int _compare = GetCompareValue(_a, _b);

                return DoSortAscending
                       ? _compare
                       : -_compare;
            }

            public abstract int GetCompareValue(PinObject _a, PinObject _b);
        }

        private class TypeComparer : PinObjectComparer
        {
            public static readonly TypeComparer Comparer = new TypeComparer();

            // -----------------------

            public override int GetCompareValue(PinObject _a, PinObject _b)
            {
                int _compare = (_a.Type != _b.Type)
                             ? _a.Type.CompareTo(_b.Type)
                             : NameComparer.Comparer.GetCompareValue(_a, _b);

                return _compare;
            }
        }

        private class NameComparer : PinObjectComparer
        {
            public static readonly NameComparer Comparer = new NameComparer();

            // -----------------------

            public override int GetCompareValue(PinObject _a, PinObject _b)
            {
                int _compare = _a.Asset.name.CompareTo(_b.Asset.name);
                return _compare;
            }
        }

        private class PathComparer : PinObjectComparer
        {
            public static readonly PathComparer Comparer = new PathComparer();

            // -----------------------

            public override int GetCompareValue(PinObject _a, PinObject _b)
            {
                int _compare = AssetDatabase.GetAssetPath(_a.Asset).CompareTo(AssetDatabase.GetAssetPath(_b.Asset));
                return _compare;
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="PinboardWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="PinboardWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Pinboard", false, 20)]
        public static PinboardWindow GetWindow()
        {
            PinboardWindow _window = GetWindow<PinboardWindow>("Pinboard");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float CreateButtonWidth = 105f;
        private const float SortOptionsWidth = 120f;
        private const float AssetPathWidthCoef = .35f;
        private const float SeparatorHeight = 3f;

        private const string UndoRecordTitle = "Pinboard Change";
        private const string RenameControlName = "RenameFolder";
        private const string StartDragTitle = "Dragging PinObject(s)";
        private const string PinObjectDataType = "PinObject";

        private static readonly AutoManagedResource<Pinboard> resource = new AutoManagedResource<Pinboard>();

        private readonly GUIContent createFolderGUI = new GUIContent(" Create Folder", "Creates a new folder at the root of the pinboard.");
        private readonly GUIContent[] sortOptionsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Sort by type", "Sort the assets by their type."),
                                                                new GUIContent("Sort by name", "Sort the assets by their name."),
                                                                new GUIContent("Sort by path", "Sort the assets by their path.")
                                                            };

        private readonly EditorColor highlightColor = new EditorColor(new Color(.4f, .4f, .4f, .23f), new Color(.5f, .5f, .5f, .25f));
        private readonly EditorColor separatorColor = new EditorColor(new Color(0f, .5f, 1f, .55f), new Color(0f, .3f, 1f, .65f));
        private readonly Color indentColor = Color.grey;

        /// <summary>
        /// Pinboard related <see cref="ScriptableObject"/>, used to store all data.
        /// </summary>
        public Pinboard Pinboard => resource.GetResource();

        [SerializeField] private string searchFilter = string.Empty;
        [SerializeField] private int selectedSortOption = 0;
        [SerializeField] private bool doSortAscending = true;

        private int lastSelectedPinObject = -1;

        private GenericMenu contextMenu = null;
        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            titleContent.image = EditorGUIUtility.IconContent("pin").image;
            createFolderGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");

            contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove", "Remove the selected element(s) from the pinboard."), false, RemoveSelectedObjects);
        }

        private void OnGUI()
        {
            Undo.RecordObject(this, UndoRecordTitle);
            Undo.RecordObject(Pinboard, UndoRecordTitle);

            // Toolbar.
            DrawToolbar();

            // Pinned Objects.
            Rect _origin = EditorGUILayout.GetControlRect(false, -EditorGUIUtility.standardVerticalSpacing);
            using (var _scope = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scope.scrollPosition;
                DrawPinObjects(_origin);

                // Empty area operations.
                Rect _position = EditorGUILayout.GetControlRect(false, 5f);
                _position.y -= SeparatorHeight;
                _position.yMax = Mathf.Max(_position.yMax, position.height);

                EventOperations(_position, _position, null, -1, 0);
                if (EnhancedEditorGUIUtility.DeselectionClick(_position))
                {
                    foreach (PinObject _object in Pinboard.PinObjects)
                        _object.IsSelected = false;

                    lastSelectedPinObject = -1;
                }
            }

            // Indicate that a drag and drop operation is available inside the window.
            Event _event = Event.current;
            if ((_event.type == EventType.DragUpdated) && (_event.mousePosition.y > _origin.y))
            {
                DragAndDrop.visualMode = ((DragAndDrop.objectReferences.Length == 0) || AssetDatabase.Contains(DragAndDrop.objectReferences[0]))
                                       ? DragAndDropVisualMode.Copy
                                       : DragAndDropVisualMode.None;

                _event.Use();
            }
            else if (_event.type == EventType.KeyDown)
            {
                switch (_event.keyCode)
                {
                    // Remove object
                    case KeyCode.Delete:
                    {
                        RemoveSelectedObjects();
                        _event.Use();
                    }
                    break;

                    // Ping object.
                    case KeyCode.Return:
                    {
                        foreach (PinObject _object in Pinboard.PinObjects)
                        {
                            if (_object.IsSelected)
                            {
                                PingObject(_object);
                                _event.Use();

                                break;
                            }
                        }
                    }
                    break;
                }
            }

            // Constantly repaint to correctly display mouse hover feedback.
            Repaint();
        }
        #endregion

        #region GUI Draw
        private readonly List<IndentLevel> indentLevels = new List<IndentLevel>();

        private int pinboardControlID = -1;
        private int renameFolderState = 0;
        private bool doFocusSelection = false;

        // -----------------------

        private void DrawToolbar()
        {
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Button to create a new folder.
                if (GUILayout.Button(createFolderGUI, EditorStyles.toolbarButton, GUILayout.Width(CreateButtonWidth)))
                {
                    foreach (PinObject _object in Pinboard.PinObjects)
                        _object.IsSelected = false;

                    PinObject _folder = new PinObject();
                    ArrayUtility.Add(ref Pinboard.PinObjects, _folder);

                    Select(Pinboard.PinObjects.Length - 1, true);
                    renameFolderState = 2;
                }

                // Search filter.
                string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter, GUILayout.MinWidth(50f));
                if (_searchFilter != searchFilter)
                {
                    searchFilter = _searchFilter;
                    FilterPinboard();
                }

                // Sorting options.
                using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    EnhancedEditorGUILayout.ToolbarSortOptions(ref selectedSortOption, ref doSortAscending, sortOptionsGUI, GUILayout.Width(SortOptionsWidth));
                    if (_changeCheck.changed)
                    {
                        SortPinboard();
                    }
                }
            }
        }

        private void DrawPinObjects(Rect _origin)
        {
            // Multi-selection keys.
            pinboardControlID = EnhancedEditorGUIUtility.GetControlID(916, FocusType.Keyboard);
            if (GUIUtility.keyboardControl == pinboardControlID)
            {
                EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(Pinboard.PinObjects, IsSelected, CanBeSelected, Select, lastSelectedPinObject);
            }

            int _index = 0;
            bool _useIndent = string.IsNullOrEmpty(searchFilter);

            for (int _i = 0; _i < Pinboard.PinObjects.Length; _i++)
            {
                PinObject _object = Pinboard.PinObjects[_i];

                // Skip non visible objects.
                if (!_object.IsVisible)
                    continue;

                // Position calculs.
                Rect _position = new Rect(EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing))
                {
                    height = EditorGUIUtility.singleLineHeight
                };

                Rect _temp = new Rect(_position)
                {
                    x = 0f,
                    width = position.width
                };

                Rect _linePos = new Rect(_temp);
                Rect _dragPosition = new Rect(_temp);
                if (_object.IsFolder)
                {
                    _dragPosition.yMin += SeparatorHeight;
                    _dragPosition.yMax -= SeparatorHeight;
                }
                else
                {
                    _dragPosition.y -= SeparatorHeight;
                }

                // Background color.
                bool _isHover = false;
                if (!_object.IsSelected)
                {
                    // Only display hover feedback when the user is not performing a drag and drop operation
                    // or when selecting a folder and no separator is being drawn.
                    Event _event = Event.current;
                    _isHover = _temp.Contains(_event.mousePosition) &&
                               ((DragAndDrop.visualMode != DragAndDropVisualMode.Copy) || (_object.IsFolder && _dragPosition.Contains(_event.mousePosition)));
                }
                else if (doFocusSelection && (_i == lastSelectedPinObject) && (Event.current.type == EventType.Repaint))
                {
                    // Scroll focus.
                    Vector2 _areaSize = new Vector2(position.width, position.height - _origin.y);
                    scroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(scroll, _position, _areaSize);

                    doFocusSelection = false;
                }

                if (_isHover)
                {
                    EditorGUI.DrawRect(_temp, highlightColor);
                }
                else
                {
                    EnhancedEditorGUI.BackgroundLine(_temp, _object.IsSelected, _index);
                }

                _index++;

                // Get and set indent level.
                int _indent = _useIndent
                            ? _object.Indent
                            : 0;

                EditorGUI.indentLevel = _indent * 2;
                _position = EditorGUI.IndentedRect(_position);
                EditorGUI.indentLevel = 0;

                // Indent visual representation.
                if ((_indent != 0) && (Event.current.type == EventType.Repaint))
                {
                    // Horizontal indent line.
                    _temp = new Rect()
                    {
                        x = _position.x - 23f,
                        y = _position.y + 8f,
                        width = 21f,
                        height = 2f
                    };

                    EditorGUI.DrawRect(_temp, indentColor);

                    // Indent update.
                    int _indentIndex = indentLevels.FindIndex(i => i.Indent == _indent);
                    if (_indentIndex == -1)
                    {
                        indentLevels.Add(new IndentLevel(_indent, _position.position));
                    }
                    else if (indentLevels[_indentIndex].IsDrawn)
                    {
                        indentLevels[_indentIndex] = new IndentLevel(_indent, _position.position);
                    }
                    else
                    {
                        Vector2 _startPos = indentLevels[_indentIndex].StartPosition;
                        indentLevels[_indentIndex] = new IndentLevel(_indent, _startPos, _position.y);
                    }

                    // Next indent.
                    int _nextIndex = _i + 1;
                    while ((_nextIndex < Pinboard.PinObjects.Length) && !Pinboard.PinObjects[_nextIndex].IsVisible)
                        _nextIndex++;

                    int _nextIndent = (_nextIndex != Pinboard.PinObjects.Length)
                                    ? Pinboard.PinObjects[_nextIndex].Indent
                                    : 0;

                    if (_nextIndent < _indent)
                    {
                        for (int _j = 0; _j < indentLevels.Count; _j++)
                        {
                            IndentLevel _indentLevel = indentLevels[_j];
                            if ((_indentLevel.Indent <= _nextIndent) || _indentLevel.IsDrawn)
                                continue;

                            // Vertical indent line.
                            _temp = new Rect()
                            {
                                x = _indentLevel.StartPosition.x - 23f,
                                y = _indentLevel.StartPosition.y,
                                width = 2f,
                                yMax = _indentLevel.YMax + 8f + 2f
                            };

                            EditorGUI.DrawRect(_temp, indentColor);
                            indentLevels[_j] = new IndentLevel(_indentLevel.Indent);
                        }
                    }
                }

                if (_object.IsFolder)
                {
                    // Separator operation.
                    Rect _separator = new Rect(_position)
                    {
                        x = _position.x,
                        y = _position.y - SeparatorHeight,
                        height = SeparatorHeight * 2f
                    };

                    EventOperations(_position, _separator, null, _i, _object.Indent);

                    // Foldout.
                    _temp = new Rect(_position)
                    {
                        width = EnhancedEditorGUIUtility.FoldoutWidth
                    };

                    bool _foldout = EditorGUI.Foldout(_temp, _object.Foldout, GUIContent.none);
                    if (_object.Foldout != _foldout)
                    {
                        _object.Foldout = _foldout;

                        // Only filter when not using a search filter.
                        if (_useIndent)
                            FilterPinboard();
                    }

                    _temp.x += _temp.width;
                    _temp.xMax = _position.xMax;

                    // Folder name.
                    if (_object.IsSelected && (renameFolderState > 1))
                    {
                        // Rename folder.
                        GUI.SetNextControlName(RenameControlName);
                        _object.FolderName = EditorGUI.TextField(_temp, _object.FolderName);

                        if (renameFolderState == 2)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                EditorGUI.FocusTextInControl(RenameControlName);
                                renameFolderState = 3;
                            }
                        }
                        else if (!EditorGUIUtility.editingTextField)
                        {
                            ExitRenameMode();
                        }
                    }
                    else
                    {
                        // Display folder name.
                        GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_object.FolderName);
                        EditorGUI.LabelField(_temp, _label);
                    }
                }
                else
                {
                    // Remove null entries.
                    if (!_object.Asset)
                    {
                        ArrayUtility.RemoveAt(ref Pinboard.PinObjects, _i);
                        _i--;

                        continue;
                    }

                    // Icon.
                    string _path = AssetDatabase.GetAssetPath(_object.Asset);
                    _temp = new Rect(_position)
                    {
                        width = EnhancedEditorGUIUtility.IconWidth
                    };

                    GUI.DrawTexture(_temp, AssetDatabase.GetCachedIcon(_path), ScaleMode.ScaleToFit);

                    // Name.
                    GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_object.Asset.name);

                    _temp.x += _temp.width + 5f;
                    _temp.width = EditorStyles.label.CalcSize(_label).x;

                    EditorGUI.LabelField(_temp, _label);

                    // Path.
                    if ((_temp.xMax + 20) < _position.xMax)
                    {
                        _temp.x = Mathf.Max(_temp.xMax + 20, _position.xMax - (_linePos.width * (1f - AssetPathWidthCoef)));
                        _temp.xMax = _position.xMax;

                        _label = EnhancedEditorGUIUtility.GetLabelGUI(_path, $"Path: {_path}");

                        if (_temp.width < EditorStyles.label.CalcSize(_label).x)
                        {
                            EditorGUI.LabelField(_temp, "...");
                            _temp.xMin += 12f;
                        }

                        using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(EditorStyles.label, TextAnchor.MiddleRight))
                        {
                            EditorGUI.LabelField(_temp, _label);
                        }
                    }
                }

                // Special event operations.
                EventOperations(_position, _dragPosition, _object, _i, _object.Indent);
                EnhancedEditorGUIUtility.MultiSelectionClick(_linePos, Pinboard.PinObjects, _i, IsSelected, Select);
            }
        }

        // -----------------------

        private bool IsSelected(int _index)
        {
            PinObject _object = Pinboard.PinObjects[_index];
            return _object.IsSelected;
        }

        private bool CanBeSelected(int _index)
        {
            PinObject _object = Pinboard.PinObjects[_index];
            return _object.IsVisible;
        }

        private void Select(int _index, bool _isSelected)
        {
            PinObject _object = Pinboard.PinObjects[_index];
            _object.IsSelected = _isSelected;

            // Selection update.
            if (_isSelected)
            {
                lastSelectedPinObject = _index;
                doFocusSelection = true;

                GUIUtility.keyboardControl = pinboardControlID;
            }
            else if (!Array.Exists(Pinboard.PinObjects, (s) => s.IsSelected))
            {
                lastSelectedPinObject = -1;
            }
        }
        #endregion

        #region Utility
        private void EventOperations(Rect _position, Rect _dragPosition, PinObject _object, int _index, int _indent)
        {
            Event _event = Event.current;
            Rect _mousePos = ((DragAndDrop.visualMode == DragAndDropVisualMode.Copy) || _event.type == EventType.DragPerform)
                           ? _dragPosition
                           : _position;

            _mousePos.xMin = 0f;
            if (!_mousePos.Contains(_event.mousePosition))
                return;

            // Drag and drop mode.
            if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
            {
                // Separator indicating a drag and drop destination.
                if ((_object == null) || !_object.IsFolder)
                {
                    Rect _temp = new Rect()
                    {
                        x = _position.x,
                        xMax = position.width,
                        y = _dragPosition.y + SeparatorHeight,
                        height = 2f
                    };

                    EditorGUI.DrawRect(_temp, separatorColor);
                }

                return;
            }

            // Special events.
            if (_event.type == EventType.DragPerform)
            {
                // Get destination index and indent.
                int _moveIndex = _index;
                if ((_index == -1) || ((_indent == 0) && ((_object == null) || !_object.IsFolder)))
                {
                    _index = 0;
                    _moveIndex = Pinboard.PinObjects.Length;
                }
                else
                {
                    // Get destination folder parent.
                    if ((_object == null) || !_object.IsFolder)
                    {
                        _object = Pinboard.PinObjects[_index];
                        _indent--;

                        while (!_object.IsFolder || (_object.Indent != _indent))
                        {
                            _index--;
                            _object = Pinboard.PinObjects[_index];
                        }
                    }

                    // Get folder and asset destination indexes.
                    _indent = _object.Indent + 1;
                    _moveIndex = _index
                               = _index + 1;

                    int _folderIndent = _object.Indent;
                    for (int _i = _moveIndex; _i < Pinboard.PinObjects.Length; _i++)
                    {
                        PinObject _pinObject = Pinboard.PinObjects[_i];
                        if (_pinObject.Indent < _indent)
                            break;

                        _moveIndex++;
                    }
                }

                // Pin Objects.
                if (DragAndDrop.GetGenericData(PinObjectDataType) is PinObject[] _pinObjects)
                {
                    // If moving a folder into one of its child folder,
                    // move this child and all its content on top of it.
                    if (IsMovingInChild(_index, out int _parentIndex, out int _childIndex, _pinObjects))
                    {
                        PinObject _parent = _pinObjects[_parentIndex];
                        _object = _pinObjects[_childIndex];

                        _moveIndex = Array.IndexOf(Pinboard.PinObjects, _parent);
                        _indent = _object.Indent + 1;

                        ArrayUtility.RemoveAt(ref _pinObjects, _childIndex);
                        ArrayUtility.Move(Pinboard.PinObjects, _object, _moveIndex);

                        int _indentDifference = _object.Indent - _parent.Indent;
                        _index = _moveIndex
                               = _moveIndex + 1;

                        for (int _i = _childIndex; _i < _pinObjects.Length;)
                        {
                            PinObject _temp = _pinObjects[_i];
                            if (_temp.Indent < _indent)
                                break;

                            ArrayUtility.RemoveAt(ref _pinObjects, _i);
                            ArrayUtility.Move(Pinboard.PinObjects, _temp, _moveIndex);
                            _temp.Indent -= _indentDifference;

                            _moveIndex++;
                        }

                        _object.Indent -= _indentDifference;
                        _indent -= _indentDifference;
                    }

                    // Move all objects.
                    List<int> _indentHelper = new List<int>();
                    List<int> _childHelper = new List<int>();
                    int _childCounter = 0;

                    for (int _i = 0; _i < _pinObjects.Length; _i++)
                    {
                        PinObject _pinObject = _pinObjects[_i];

                        int _indexOf = Array.IndexOf(Pinboard.PinObjects, _pinObject);
                        int _objectIndent = _pinObject.Indent;

                        // Indent helper update.
                        while (_indentHelper.Last(out int _lastIndent) && (_objectIndent <= _lastIndent))
                        {
                            if (_indentHelper.Count == 1)
                            {
                                _childCounter = 0;
                            }
                            else
                            {
                                _childCounter += _childHelper.Last() + 1;
                            }

                            _indentHelper.RemoveLast();
                            _childHelper.RemoveLast();
                        }

                        // Selected folders go on top.
                        if (_pinObject.IsFolder && _pinObject.IsSelected)
                        {
                            _indentHelper.Add(_pinObject.Indent);
                            _childHelper.Add(0);

                            int _destIndex = (_indexOf < _index)
                                           ? (_index - 1)
                                           : _index;

                            ArrayUtility.Move(Pinboard.PinObjects, _pinObject, _destIndex);
                            _pinObject.Indent = _indent;

                            if (_indexOf >= _index)
                                _index++;
                        }
                        else if (_indentHelper.Last(out int _lastIndent) && !_pinObject.IsSelected)
                        {
                            // Unselected children go below selected folder.
                            _childHelper[_childHelper.Count - 1] = _childHelper.Last() + 1;

                            int _destIndex = (_indexOf < _index)
                                           ? (_index - 1)
                                           : _index;

                            ArrayUtility.Move(Pinboard.PinObjects, _pinObject, _destIndex - _childCounter);
                            _pinObject.Indent = _indent + (_pinObject.Indent - _lastIndent);

                            if (_indexOf >= _index)
                                _index++;
                        }
                        else
                        {
                            // Selected assets go on folder bottom.
                            int _destIndex;
                            if (_indexOf < _moveIndex)
                            {
                                _destIndex = _moveIndex - 1;

                                if (_indexOf < _index)
                                    _index--;
                            }
                            else
                            {
                                _destIndex = _moveIndex;
                            }

                            ArrayUtility.Move(Pinboard.PinObjects, _pinObject, _destIndex);
                            _pinObject.Indent = _indent;
                        }

                        // Destination index increment.
                        if (_indexOf >= _moveIndex)
                            _moveIndex++;
                    }
                }

                // Assets.
                Object[] _objects = DragAndDrop.objectReferences;
                if (_objects != null)
                {
                    foreach (Object _asset in _objects)
                    {
                        if (Array.Exists(Pinboard.PinObjects, o => !o.IsFolder && (o.Asset == _asset)) || !AssetDatabase.Contains(_asset))
                            continue;

                        PinObject _pinObject = new PinObject(_asset, _indent);
                        ArrayUtility.Insert(ref Pinboard.PinObjects, _moveIndex, _pinObject);
                    }
                }

                _event.Use();

                DragAndDrop.AcceptDrag();
                SortPinboard();
            }
            else if ((_event.type == EventType.MouseDown) && (_object != null))
            {
                if (!_event.shift && !_event.control && _object.IsSelected && _object.IsFolder && (_event.button == 0) && (_event.clickCount == 2) && (renameFolderState == 0))
                {
                    // Enter rename folder mode.
                    renameFolderState = 1;
                }
                else if (!_object.IsFolder && (_event.clickCount == 2))
                {
                    // Ping object.
                    PingObject(_object);
                }
                else if ((renameFolderState != 0) && !_object.IsSelected)
                    ExitRenameMode();
            }
            else if (EnhancedEditorGUIUtility.ContextClick(_mousePos) && Array.Exists(Pinboard.PinObjects, o => o.IsSelected))
            {
                contextMenu.ShowAsContext();
            }
            else if ((_event.type == EventType.MouseUp) && (renameFolderState == 1))
            {
                renameFolderState = 2;
            }
            else if ((_event.type == EventType.MouseDrag) && (_event.button == 0) && (renameFolderState < 2))
            {
                // Stop renaming.
                ExitRenameMode();

                // When drag begin, set drag and drop data on selected objects.
                List<PinObject> _objects = new List<PinObject>();
                int _selectIndent = 99999;

                foreach (var _pinObject in Pinboard.PinObjects)
                {
                    if (_pinObject.Indent > _selectIndent)
                    {
                        _objects.Add(_pinObject);
                    }
                    else if (_pinObject.IsSelected)
                    {
                        _selectIndent = _pinObject.Indent;
                        _objects.Add(_pinObject);
                    }
                    else
                    {
                        _selectIndent = 99999;
                    }
                }

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(PinObjectDataType, _objects.ToArray());
                DragAndDrop.StartDrag(StartDragTitle);

                _event.Use();
            }
        }

        /// <summary>
        /// Checks if the folder at the given index is moving into one of its child folder.
        /// </summary>
        /// <param name="_index">The index of the folder to check (in the pin objects list).</param>
        /// <param name="_parentIndex">Outputs the index of the new parent folder.</param>
        /// <param name="_childIndex">Outputs the new index of the moved folder.</param>
        /// <param name="_pinObjects">The list of all the pinned items.</param>
        /// <returns>returns true if the foldder is moving into one of its child folder, otherwise false.</returns>
        private bool IsMovingInChild(int _index, out int _parentIndex, out int _childIndex, PinObject[] _pinObjects)
        {
            _parentIndex = -1;
            _childIndex = -1;

            if (_index == Pinboard.PinObjects.Length)
                return false;

            List<PinObject> _childFolders = new List<PinObject>();
            PinObject _indexObject = Pinboard.PinObjects[_index];

            if (_indexObject.IsFolder && (_index > 0))
            {
                _indexObject = Pinboard.PinObjects[_index - 1];
            }

            for (int _i = 0; _i < _pinObjects.Length; _i++)
            {
                PinObject _parent = _pinObjects[_i];
                if (!_parent.IsFolder || !_parent.IsSelected)
                    continue;

                // Moving a folder into itself ; cancel operation.
                if (_parent == _indexObject)
                {
                    CancelParentDrag(_i, _pinObjects);
                    return false;
                }

                // Reset folder counter.
                _childFolders.Clear();
                _childFolders.Add(_parent);

                for (int _j = _i + 1; _j < _pinObjects.Length; _j++)
                {
                    PinObject _child = _pinObjects[_j];
                    if (_child.Indent <= _parent.Indent)
                        break;

                    // Ignore non-parent folders.
                    while (_childFolders.Last().Indent >= _child.Indent)
                        _childFolders.RemoveLast();

                    if (_child.IsFolder)
                        _childFolders.Add(_child);

                    if (_child == _indexObject)
                    {
                        // Moving a folder into itself ; cancel operation.
                        var _last = _childFolders.Last();
                        if (_last == _parent)
                        {
                            CancelParentDrag(_i, _pinObjects);
                            return false;
                        }

                        // Get informations.
                        _parentIndex = _i;
                        _childIndex = Array.IndexOf(_pinObjects, _last);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Cancels the parent drag operation.
        /// </summary>
        /// <param name="_index">The index of the dragged item in the Pin Objects list.</param>
        /// <param name="_pinObjects">The list of all the pinned items.</param>
        private void CancelParentDrag(int _index, PinObject[] _pinObjects)
        {
            PinObject _parent = _pinObjects[_index];
            ArrayUtility.RemoveAt(ref _pinObjects, _index);

            for (int _i = _index; _i < _pinObjects.Length; _i++)
            {
                PinObject _child = _pinObjects[_i];
                if (_child.Indent <= _parent.Indent)
                    break;

                if (!_child.IsSelected)
                {
                    ArrayUtility.RemoveAt(ref _pinObjects, _i);
                    _i--;
                }
            }
        }

        private void ExitRenameMode()
        {
            if (renameFolderState > 1)
                SortPinboard();

            renameFolderState = 0;
        }

        private void PingObject(PinObject _object)
        {
            EditorGUIUtility.PingObject(_object.Asset);
            Selection.activeObject = _object.Asset;
        }

        private void RemoveSelectedObjects()
        {
            int _selectIndent = 99999;
            for (int _i = 0; _i < Pinboard.PinObjects.Length; _i++)
            {
                PinObject _object = Pinboard.PinObjects[_i];
                if (_object.Indent > _selectIndent)
                {
                    RemoveObject();
                }
                else if (_object.IsSelected)
                {
                    _selectIndent = _object.Indent;
                    RemoveObject();
                }
                else
                {
                    _selectIndent = 99999;
                }

                // ----- Local Methods ----- \\

                void RemoveObject()
                {
                    ArrayUtility.RemoveAt(ref Pinboard.PinObjects, _i);
                    _i--;
                }
            }
        }

        private void FilterPinboard()
        {
            // When not using a search filter, hide objects in folded folders.
            if (string.IsNullOrEmpty(searchFilter))
            {
                int _hiddenIndent = 999999;

                foreach (PinObject _object in Pinboard.PinObjects)
                {
                    if (_object.Indent <= _hiddenIndent)
                    {
                        _object.IsVisible = true;
                        _hiddenIndent = (_object.IsFolder && !_object.Foldout)
                                      ? _object.Indent
                                      : 999999;
                    }
                    else
                        _object.IsVisible = false;
                }

                return;
            }

            string _searchFilter = searchFilter.ToLower();
            foreach (PinObject _object in Pinboard.PinObjects)
            {
                bool _isVisible = _object.IsFolder
                                ? _object.FolderName.ToLower().Contains(_searchFilter)
                                : _object.Asset.name.ToLower().Contains(_searchFilter);

                _object.IsVisible = _isVisible;
            }
        }

        private void SortPinboard()
        {
            PinObjectComparer.DoSortAscending = doSortAscending;
            IComparer<PinObject> _comparer;

            switch (selectedSortOption)
            {
                // Type.
                case 0:
                    _comparer = TypeComparer.Comparer;
                    break;

                // Name.
                case 1:
                    _comparer = NameComparer.Comparer;
                    break;

                // Path.
                case 2:
                    _comparer = PathComparer.Comparer;
                    break;

                default:
                    return;
            }

            List<SortLevel> _order = new List<SortLevel>() { new SortLevel(0, 0) };
            int _indent = 0;

            for (int _i = 0; _i < Pinboard.PinObjects.Length;)
            {
                PinObject _temp = Pinboard.PinObjects[_i];

                // New indent level.
                if (_temp.Indent > _indent)
                {
                    _indent = _temp.Indent;
                    _order.Add(new SortLevel(_i, _indent));
                }

                // New folder.
                if (_temp.IsFolder)
                {
                    var _folder = new SortFolder(_temp);
                    _order[_order.Count - 1].Folders.Add(_folder);
                }

                // Child count update.
                for (int _j = 0; _j < _order.Count - 1; _j++)
                {
                    SortFolder _folder = _order[_j].Folders.Last();
                    _folder.ChildCount++;

                    _order[_j].Folders[_order[_j].Folders.Count - 1] = _folder;
                }

                _i++;

                // Sort all ended indent level assets and folders.
                while ((_i == Pinboard.PinObjects.Length) || (Pinboard.PinObjects[_i].Indent < _order.Last().Indent))
                {
                    SortLevel _last = _order.Last();
                    _order.RemoveLast();
                    _indent--;

                    int _objectIndex = _last.Index;
                    _last.Folders.Sort((a, b) => a.Object.FolderName.CompareTo(b.Object.FolderName));

                    for (int _j = _last.Folders.Count; _j-- > 0;)
                    {
                        var _ord = _last.Folders[_j];
                        int _index = Array.IndexOf(Pinboard.PinObjects, _ord.Object) + _ord.ChildCount;

                        for (int _k = _ord.ChildCount + 1; _k-- > 0;)
                        {
                            ArrayUtility.Move(Pinboard.PinObjects, _index, _last.Index);
                            _objectIndex++;
                        }
                    }

                    int _length = _i - _objectIndex;
                    Array.Sort(Pinboard.PinObjects, _objectIndex, _length, _comparer);

                    // Break.
                    if (_order.Count == 0)
                        break;
                }
            }

            // Save changes.
            EditorUtility.SetDirty(Pinboard);
        }
        #endregion
    }
}
