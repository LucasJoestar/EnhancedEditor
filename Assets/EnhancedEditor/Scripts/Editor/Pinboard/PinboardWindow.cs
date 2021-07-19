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
using Folder = EnhancedEditor.Editor.Pinboard.Folder;
using Asset = EnhancedEditor.Editor.Pinboard.Asset;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// The pinboard window can be used to store and give quick access to your favorite assets and folders in project.
    /// </summary>
	public class PinboardWindow : EditorWindow
    {
        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="PinboardWindow"/> currently on screen.
        /// <para/>
        /// Creates and shows a new instance if there is none.
        /// </summary>
        [MenuItem("Enhanced Editor/Pinboard", false, 20)]
        public static PinboardWindow GetWindow()
        {
            PinboardWindow _window = GetWindow<PinboardWindow>("Pinboard");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const float FoldoutWidth = 15f;
        private const float IconWidth = 20f;
        private const float AssetPathWidth = .35f;
        private const float AssetPathShortCoef = .15f;
        private const float SeparatorHeight = 3f;
        private const float Space = 10f;

        private const string RenameControlName = "RenameFolder";
        private const string UndoName = "Pinboard Update";

        private const string FolderDataType = "Folder";
        private const string AssetDataType = "Asset";
        private const string DragTitle = "Dragging PinObject(s)";

        private readonly GUIContent createFolderGUI = new GUIContent(" Create Folder", "Create a new folder at the root of the pinboard.");
        private readonly GUIContent sortAscendingGUI = new GUIContent("↑", "Sort in ascending order.");
        private readonly GUIContent sortDescendingGUI = new GUIContent("↓", "Sort in descending order.");
        private readonly GUIContent[] sortOptionsGUI = new GUIContent[]
                                                            {
                                                                new GUIContent("Sort by type", "Sort assets by their type."),
                                                                new GUIContent("Sort by name", "Sort assets by their name."),
                                                                new GUIContent("Sort by path", "Sort assets by their path.")
                                                            };

        private readonly GUIContent pathGUI = new GUIContent();

        private readonly Color oddColor = EnhancedEditorGUIUtility.GUIOddColor;
        private readonly Color selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private readonly Color highlightColor = new Color(.5f, .5f, .5f, .25f);
        private readonly Color separatorColor = Color.blue;
        private readonly Color indentColor = Color.grey;

        private readonly AutoManagedResource<Pinboard> resource = new AutoManagedResource<Pinboard>();

        /// <summary>
        /// Pinboard related <see cref="ScriptableObject"/>, used to store all datas.
        /// </summary>
        public Pinboard Pinboard => resource.GetResource();

        private List<PinObject> selectedObjects = new List<PinObject>();
        private List<PinObject> filteredObjects = new List<PinObject>();
        private GenericMenu contextMenu = null;
        private int renameFolderState = 0;
        private int renameFocusCount = 0;

        private string searchFilter = string.Empty;
        private bool useSearchFilter = false;
        
        private int selectedSortOption = 0;
        private bool doSortAscending = true;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            createFolderGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");

            contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove", "Remove these element(s) from the pinboard."), false, RemoveSelection);
        }

        private void OnGUI()
        {
            Undo.RecordObject(Pinboard, UndoName);
            DrawToolbar();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Use the height specified when getting control as the padding at the bottom of the window.
            Rect _position = EditorGUILayout.GetControlRect(true, Space);
            _position.xMax += _position.xMin;
            _position.height = EditorGUIUtility.singleLineHeight;

            // Draw each folders.
            int _index = 0;
            Rect _finalPosition = DrawFolder(_position, Pinboard.PinnedAssets, ref _index);

            _finalPosition.x = 0f;
            _finalPosition.y -= SeparatorHeight;
            _finalPosition.yMax = position.yMax;

            // Empty space operations.
            EventOperations(_finalPosition, null, Pinboard.PinnedAssets, false);
            EditorGUILayout.EndScrollView();

            // Indicate that a drag and drop operation is available inside the window.
            Event _event = Event.current;
            if ((_event.type == EventType.DragUpdated) && (_event.mousePosition.y > _position.y))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                _event.Use();
            }

            // Constantly repaint to correctly display mouse hover feedback.
            Repaint();
        }
        #endregion

        #region GUI Drawers
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUIStyle _buttonStyle = EnhancedEditorStyles.LeftAlignedToolbarButton;

            // Create folder button.
            if (GUILayout.Button(createFolderGUI, _buttonStyle, GUILayout.Width(125f)))
            {
                Folder _newFolder = new Folder();
                ArrayExtensions.Add(ref Pinboard.PinnedAssets.Folders, _newFolder);

                selectedObjects.Clear();
                selectedObjects.Add(_newFolder);

                EnterRenameMode(3);
            }

            // Search pattern.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter);
            if (_searchFilter != searchFilter)
            {
                searchFilter = _searchFilter;
                FilterObjects();
            }

            // Sorting options.
            if (EnhancedEditorGUILayout.ToolbarSortOptions(ref selectedSortOption, ref doSortAscending, sortOptionsGUI, GUILayout.Width(130f)))
            {
                SortObjects();
            }

           /* int _sortOption = EditorGUILayout.Popup(selectedSortOption, sortOptionsGUI, EditorStyles.toolbarDropDown, GUILayout.Width(110f));
            if (_sortOption != selectedSortOption)
            {
                selectedSortOption = _sortOption;
                SortObjects();
            }
            
            if (GUILayout.Button(doSortAscending ? sortAscendingGUI : sortDescendingGUI, EditorStyles.toolbarButton, GUILayout.Width(20f)))
            {
                doSortAscending = !doSortAscending;
                SortObjects();
            }*/

            EditorGUILayout.EndHorizontal();
        }

        private Rect DrawFolder(Rect _position, Folder _folder, ref int _index)
        {
            // Subfolders.
            float _xOrigin = _position.x;
            float _yOrigin = _position.y;
            float _yIndent = _position.y;

            for (int _i = 0; _i < _folder.Folders.Length; _i++)
            {
                Folder _subfolder = _folder.Folders[_i];

                // Only draw filtered folders.
                if (useSearchFilter && !filteredObjects.Contains(_subfolder))
                {
                    if (_subfolder.Foldout)
                        _position = DrawFolder(_position, _subfolder, ref _index);

                    continue;
                }

                // Separator operations.
                Rect _separator = new Rect(_position.x, _position.y - SeparatorHeight, _position.width, SeparatorHeight * 2f);
                EventOperations(_separator, null, _folder, false);

                // Background.
                DrawBackground(_position, _subfolder, _index, true);

                // Foldout.
                Rect _drawPosition = new Rect(_position.x, _position.y, FoldoutWidth, _position.height);
                _subfolder.Foldout = EditorGUI.Foldout(_drawPosition, _subfolder.Foldout, GUIContent.none);

                _drawPosition.x = _drawPosition.xMax;
                _drawPosition.xMax = _position.xMax;

                // Editable folder name.
                if (selectedObjects.Contains(_subfolder) && (renameFolderState == 2))
                {
                    GUI.SetNextControlName(RenameControlName);
                    _subfolder.Name = EditorGUI.TextField(_drawPosition, _subfolder.Name);

                    // Use multiple focus as not all text is selected on first one.
                    if (renameFocusCount > 0)
                    {
                        EditorGUI.FocusTextInControl(RenameControlName);
                        renameFocusCount--;
                        
                    }
                    else if (!EditorGUIUtility.editingTextField)
                    {
                        ExitRenameMode();
                    }
                }
                else
                {
                    EditorGUI.LabelField(_drawPosition, _subfolder.Name);
                }

                // Reserve layout for scroller.
                GUILayoutUtility.GetRect(_drawPosition.xMin + EditorStyles.label.CalcSize(new GUIContent(_subfolder.Name)).x + Space, _drawPosition.height);

                // Perform folder operations after label draw to use dedicated events.
                EventOperations(_position, _subfolder, _subfolder, true);

                // Indent horizontal visualization.
                _yIndent = DrawHorizontalIndent(_drawPosition, _xOrigin);

                // Draw subfolder content.
                _position.xMin += IconWidth + Space;
                _position.y += EditorGUIUtility.singleLineHeight;
                _index++;
                
                if (_subfolder.Foldout)
                {
                    _position = DrawFolder(_position, _subfolder, ref _index);
                }
                
                _position.xMin = _xOrigin;
            }

            // Root objects.
            for (int _i = 0; _i < _folder.Assets.Length; _i++)
            {
                Asset _asset = _folder.Assets[_i];

                // Remove null entries.
                if (_asset.Object == null)
                {
                    ArrayUtility.Remove(ref _folder.Assets, _asset);
                    continue;
                }

                // Only draw filtered assets.
                if (useSearchFilter && !filteredObjects.Contains(_asset))
                    continue;

                // Background.
                DrawBackground(_position, _asset, _index, false);

                // Asset operations.
                Event _event = Event.current;
                Rect _eventPosition = new Rect(_position.x, _position.y - SeparatorHeight, _position.width, _position.height);
                bool _click = EventOperations(_eventPosition, _asset, _folder, false);

                // Ping object on double click.
                if (_click && (_event.clickCount == 2))
                {
                    Object _object = _asset.Object;
                    Selection.activeObject = _object;
                    EditorGUIUtility.PingObject(_object);

                    _event.clickCount = 0;
                }

                // Type icon.
                string _path = AssetDatabase.GetAssetPath(_asset.Object);
                Rect _drawPosition = new Rect(_position.x, _position.y, IconWidth, _position.height);

                GUI.DrawTexture(_drawPosition, AssetDatabase.GetCachedIcon(_path), ScaleMode.ScaleToFit);

                // Name.
                _drawPosition.x = _drawPosition.xMax + Space;
                _drawPosition.xMax = _position.xMax;

                GUIContent _name = new GUIContent(_asset.Object.name);
                EditorGUI.LabelField(_drawPosition, _name);

                // Reserve layout for scroller.
                float _width = _drawPosition.xMin + EditorStyles.label.CalcSize(_name).x + Space;
                GUILayoutUtility.GetRect(_width, _drawPosition.height);

                // Path.
                _drawPosition.x = Mathf.Max(_width + Space, _position.xMax * (1f - AssetPathWidth));
                _drawPosition.xMax = _position.xMax;

                if (_drawPosition.width > 0f)
                {
                    // Shorten the path if it is too long to be fully displayed.
                    string _shortPath = _path;
                    if (_path.Length > (_drawPosition.width * AssetPathShortCoef))
                        _shortPath = $"...{_path.Remove(0, (int)(_path.Length - (_drawPosition.width * AssetPathShortCoef)))}";

                    pathGUI.text = _shortPath;
                    pathGUI.tooltip = $"Path: {_path}";
                    EditorGUI.LabelField(_drawPosition, pathGUI);
                }

                // Indent horizontal visualization.
                _yIndent = DrawHorizontalIndent(_drawPosition, _xOrigin);

                // Increment position.
                _position.y += _position.height;
                _index++;
            }

            // Indent vertical visualization.
            DrawVerticalIndent(_xOrigin, _yOrigin, _yIndent);

            return _position;
        }

        private bool DrawBackground(Rect _position, PinObject _object, int _index, bool _isFolder)
        {
            _position.xMin = 0f;

            // Only draw hover feedback when the user is not performing a drag and drop or when selecting a folder and no separator is drawn.
            Event _event = Event.current;
            bool _isHover = _position.Contains(_event.mousePosition) &&
                            ((DragAndDrop.visualMode != DragAndDropVisualMode.Copy) ||
                            (_isFolder && (_event.mousePosition.y >= (_position.y + SeparatorHeight)) && (_event.mousePosition.y < (_position.yMax - SeparatorHeight))));

            // Background color.
            if (selectedObjects.Contains(_object))
            {
                EditorGUI.DrawRect(_position, selectedColor);
            }
            else if (_isHover)
            {
                EditorGUI.DrawRect(_position, highlightColor);
            }
            else if ((_index % 2) == 0)
            {
                EditorGUI.DrawRect(_position, oddColor);
            }

            return _isHover;
        }

        private float DrawHorizontalIndent(Rect _position, float _xOrigin)
        {
            _position.x = _xOrigin - IconWidth - 4f;
            _position.y += 7.5f;
            _position.width = IconWidth;
            _position.height = 2f;

            EditorGUI.DrawRect(_position, indentColor);
            return _position.y;
        }

        private void DrawVerticalIndent(float _xOrigin, float _yOrigin, float _yMax)
        {
            Rect _position = new Rect(_xOrigin - IconWidth - 4f, _yOrigin + 2f, 2f, _yMax - _yOrigin);

            if (_position.height > 0f)
                EditorGUI.DrawRect(_position, indentColor);
        }
        #endregion

        #region Utility
        private bool EventOperations(Rect _position, PinObject _object, Folder _destination, bool _isFolder)
        {
            Event _event = Event.current;
            Rect _mouseRect = new Rect(0f, _position.y, _position.xMax, _position.height);

            if (!_mouseRect.Contains(_event.mousePosition))
                return false;

            // Drag and drop mode.
            if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
            {
                // Separator indicating a drag and drop destination.
                if (!_isFolder)
                {
                    _position.xMin -= 2f;
                    _position.y += SeparatorHeight - 1f;
                    _position.height = 1f;

                    EditorGUI.DrawRect(_position, separatorColor);
                }

                return false;
            }

            // Special events.
            if (_event.type == EventType.DragPerform)
            {
                // Folders.
                if (DragAndDrop.GetGenericData(FolderDataType) is Folder[] _folders)
                {
                    // Perform an inverse loop do avoid duplicating objects in folders.
                    for (int _i = _folders.Length; _i-- > 0;)
                    {
                        Folder _data = _folders[_i];

                        // Prevent from moving a folder into itself.
                        if (_data == _destination)
                        {
                            ArrayExtensions.Remove(ref _folders, _data);
                        }
                        else if (FindParent(Pinboard.PinnedAssets, _data, out Folder _parent))
                        {
                            // When moving parent folder into one of its children.
                            if (FindParent(_data, _destination, out Folder _childParent))
                            {
                                ArrayExtensions.Remove(ref _childParent.Folders, _destination);
                                ArrayExtensions.Add(ref _parent.Folders, _destination);
                            }

                            ArrayExtensions.Remove(ref _parent.Folders, _data);
                        }
                    }

                    ArrayExtensions.Add(ref _destination.Folders, _folders);
                }

                // Assets.
                if (DragAndDrop.GetGenericData(AssetDataType) is Asset[] _assets)
                {
                    foreach (Asset _data in _assets)
                    {
                        if (FindParent(Pinboard.PinnedAssets, _data, out Folder _parent))
                        {
                            ArrayExtensions.Remove(ref _parent.Assets, _data);
                        }
                    }

                    ArrayExtensions.Add(ref _destination.Assets, _assets);

                    // ----- Local Method ----- //
                    bool FindParent(Folder _folder, Asset _data, out Folder _parent)
                    {
                        foreach (Asset _asset in _folder.Assets)
                        {
                            if (_asset == _data)
                            {
                                _parent = _folder;
                                return true;
                            }
                        }

                        foreach (Folder _subfolder in _folder.Folders)
                        {
                            if (FindParent(_subfolder, _data, out _parent))
                                return true;
                        }

                        _parent = null;
                        return false;
                    }
                }

                // Objects.
                Object[] _objects = DragAndDrop.objectReferences;
                if (_objects != null)
                {
                    RemoveObsoleteObjects(Pinboard.PinnedAssets, ref _objects);
                    foreach (Object _data in _objects)
                    {
                        Asset _asset = new Asset(_data);
                        ArrayExtensions.Add(ref _destination.Assets, _asset);
                    }

                    // ----- Local Method ----- //
                    void RemoveObsoleteObjects(Folder _folder, ref Object[] _objects)
                    {
                        foreach (var _subfolder in _folder.Folders)
                        {
                            RemoveObsoleteObjects(_subfolder, ref _objects);
                        }

                        for (int _i = _objects.Length; _i-- > 0;)
                        {
                            Object _object = _objects[_i];
                            if (Array.Exists(_folder.Assets, (a) => a.Object == _object))
                            {
                                ArrayExtensions.RemoveAt(ref _objects, _i);
                            }
                        }
                    }
                }

                DragAndDrop.AcceptDrag();
                SortObjects(_destination);
                SetPinboardDirty();

                _event.Use();
            }
            else if (_event.type == EventType.MouseDown)
            {
                ExitRenameMode();

                // Select / unselect objects.
                if (_event.control)
                {
                    // Do not unselect on empty click when control is pressed.
                    if (_object == null)
                        return true;

                    if (!selectedObjects.Contains(_object))
                    {
                        selectedObjects.Add(_object);
                    }
                    else if (_event.button == 0)
                    {
                        selectedObjects.Remove(_object);
                    }
                }
                else if (_event.shift)
                {
                    // Do not unselect either when shift is pressed.
                    if (_object == null)
                        return true;

                    if (selectedObjects.Count > 0)
                    {
                        PinObject _a = _object;
                        PinObject _b = selectedObjects[selectedObjects.Count - 1];
                        bool _doSelect = false;

                        SelectIntermediateObjects(Pinboard.PinnedAssets);

                        // ----- Local Methods ----- //
                        bool SelectIntermediateObjects(Folder _folder)
                        {
                            for (int _i = 0; _i < _folder.Folders.Length; _i++)
                            {
                                Folder _subfolder = _folder.Folders[_i];
                                if (SelectObject(_subfolder))
                                    return true;

                                if (_subfolder.Foldout && SelectIntermediateObjects(_subfolder))
                                    return true;
                            }

                            for (int _i = 0; _i < _folder.Assets.Length; _i++)
                            {
                                Asset _asset = _folder.Assets[_i];
                                if (SelectObject(_asset))
                                    return true;
                            }

                            return false;
                        }

                        bool SelectObject(PinObject _object)
                        {
                            // When both objects have been selected, stop selection.
                            bool _found = (_object == _a) || (_object == _b);
                            bool _end = _found && _doSelect;

                            if (_found && !_doSelect)
                                _doSelect = true;

                            // Select object if not already selected.
                            if (_doSelect && !selectedObjects.Contains(_object))
                                selectedObjects.Add(_object);

                            return _end;
                        }
                    }
                    else
                    {
                        selectedObjects.Add(_object);
                    }
                }
                else if (_object != null)
                {
                    // Rename folder on left click if already selected.
                    if (selectedObjects.Contains(_object) && (_event.button == 0))
                    {
                        renameFolderState = 1;
                    }
                    else if ((_event.button == 0) || !selectedObjects.Contains(_object))
                    {
                        // Keep multiple selection on context click.
                        selectedObjects.Clear();
                        selectedObjects.Add(_object);
                    }
                }
                else
                {
                    selectedObjects.Clear();
                }

                _event.Use();
                return true;
            }
            else if ((_event.type == EventType.ContextClick) && (selectedObjects.Count > 0))
            {
                // Display context menu on context click if at least one object is selected.
                contextMenu.ShowAsContext();
                _event.Use();
            }
            else if (_event.type == EventType.MouseUp)
            {
                if (renameFolderState == 1)
                {
                    if (selectedObjects.Count > 1)
                    {
                        // Adjust selection to one object.
                        selectedObjects.Clear();
                        selectedObjects.Add(_object);
                    }
                    else if (_isFolder)
                    {
                        // Rename folder on mouse up after second click.
                        EnterRenameMode();
                    }
                    else
                    {
                        ExitRenameMode();
                    }
                    
                    _event.Use();
                }
            }
            else if ((_event.type == EventType.MouseDrag) && (_event.button == 0) && (selectedObjects.Count > 0))
            {
                // When drag begin, set drag and drop data on selected objects (and stop renaming operation).
                ExitRenameMode();

                List<Folder> _folders = new List<Folder>();
                List<Asset> _assets = new List<Asset>();

                foreach (var selection in selectedObjects)
                {
                    if (selection is Folder _folder)
                    {
                        _folders.Add(_folder);
                    }
                    else if (selection is Asset _asset)
                    {
                        _assets.Add(_asset);
                    }
                }

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(FolderDataType, _folders.ToArray());
                DragAndDrop.SetGenericData(AssetDataType, _assets.ToArray());

                DragAndDrop.StartDrag(DragTitle);

                _event.Use();
            }
            else if ((_event.type == EventType.KeyDown) && (_event.keyCode == KeyCode.Delete))
            {
                // Remove selection on delete key down.
                RemoveSelection();
            }

            return false;
        }

        private void EnterRenameMode(int _focusCount = 2)
        {
            renameFolderState = 2;
            renameFocusCount = _focusCount;
        }

        private void ExitRenameMode()
        {
            if ((renameFolderState == 2) && (selectedObjects.Count == 1) && (selectedObjects[0] is Folder _folder)
                && FindParent(Pinboard.PinnedAssets, _folder, out Folder _parent))
            {
                SortObjects(_parent);
                SetPinboardDirty();
            }

            renameFolderState = 0;
        }

        private bool FindParent(Folder _searchFolder, Folder _child, out Folder _parent)
        {
            foreach (Folder _subfolder in _searchFolder.Folders)
            {
                if (_subfolder == _child)
                {
                    _parent = _searchFolder;
                    return true;
                }

                if (FindParent(_subfolder, _child, out _parent))
                    return true;
            }

            _parent = null;
            return false;
        }

        private void SortObjects()
        {
            SortFolder(Pinboard.PinnedAssets);
            SetPinboardDirty();

            // ----- Local Method ----- //
            void SortFolder(Folder _folder)
            {
                SortObjects(_folder);
                foreach (Folder _subfolder in _folder.Folders)
                {
                    SortFolder(_subfolder);
                }
            }
        }

        private void SortObjects(Folder _folder)
        {
            // Always sort folders the same way: by their name.
            Array.Sort(_folder.Folders, (a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });

            switch (selectedSortOption)
            {
                // Type.
                case 0:
                    Array.Sort(_folder.Assets, (a, b) =>
                    {
                        return a.Type.CompareTo(b.Type);
                    });
                    break;

                // Name.
                case 1:
                    Array.Sort(_folder.Assets, (a, b) =>
                    {
                        return a.Object.name.CompareTo(b.Object.name);
                    });
                    break;

                // Path.
                case 2:
                    Array.Sort(_folder.Assets, (a, b) =>
                    {
                        return AssetDatabase.GetAssetPath(a.Object).CompareTo(AssetDatabase.GetAssetPath(b.Object));
                    });
                    break;

                default:
                    break;
            }

            if (!doSortAscending)
                Array.Reverse(_folder.Assets);
        }

        private void FilterObjects()
        {
            filteredObjects.Clear();
            if (searchFilter == string.Empty)
            {
                useSearchFilter = false;
                return;
            }

            string _filter = searchFilter.ToLower();
            FilterFolder(Pinboard.PinnedAssets);
            useSearchFilter = true;

            // ----- Local Method ----- //
            bool FilterFolder(Folder _folder)
            {
                // Used to unfold path to filtered objects.
                bool _foldout = false;

                for (int _i = 0; _i < _folder.Folders.Length; _i++)
                {
                    Folder _subfolder = _folder.Folders[_i];
                    if (_subfolder.Name.ToLower().Contains(_filter))
                    {
                        filteredObjects.Add(_subfolder);
                        _foldout = true;
                    }

                    if (FilterFolder(_subfolder))
                    {
                        _subfolder.Foldout = true;
                        _foldout = true;
                    }
                }

                for (int _i = 0; _i < _folder.Assets.Length; _i++)
                {
                    Asset _asset = _folder.Assets[_i];
                    if (_asset.Object.name.ToLower().Contains(_filter))
                    {
                        filteredObjects.Add(_asset);
                        _foldout = true;
                    }
                }

                return _foldout;
            }
        }

        private void RemoveSelection()
        {
            CleanFolder(Pinboard.PinnedAssets);
            SetPinboardDirty();

            // ----- Local Method ----- //
            void CleanFolder(Folder _folder)
            {
                for (int _i = _folder.Folders.Length; _i-- > 0;)
                {
                    Folder _subfolder = _folder.Folders[_i];
                    int _index = selectedObjects.IndexOf(_subfolder);
                    if (_index > -1)
                    {
                        ArrayUtility.RemoveAt(ref _folder.Folders, _i);
                        selectedObjects.RemoveAt(_index);
                    }
                    else
                    {
                        CleanFolder(_subfolder);
                    }
                }

                for (int _i = _folder.Assets.Length; _i-- > 0;)
                {
                    Asset _asset = _folder.Assets[_i];
                    int _index = selectedObjects.IndexOf(_asset);
                    if (_index > -1)
                    {
                        ArrayUtility.RemoveAt(ref _folder.Assets, _i);
                        selectedObjects.RemoveAt(_index);
                    }
                }
            }
        }

        private void SetPinboardDirty()
        {
            EditorUtility.SetDirty(Pinboard);
        }
        #endregion
    }
}
