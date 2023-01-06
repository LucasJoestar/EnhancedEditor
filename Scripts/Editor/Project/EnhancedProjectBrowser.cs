// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Static class used to draw over the project window for better layout and icons.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedProjectBrowser {
        #region Item Infos
        private class ItemInfos {
            public const string PackageDisplayNameHeader = "\"displayName\"";
            public const char PackageDisplayNameSeparator = '\"';

            public HierarchyProperty Property = null;
            public readonly string PackageName = string.Empty;
            public readonly string GUID = string.Empty;
            public readonly bool IsAssetsFolder = false;
            public readonly bool IsPackageSubfolder = false;

            public string ParentFolderGUID = string.Empty;
            public bool IsFolder = false;
            public bool HasSubfolders = false;

            private int drawCount = -1;
            private int count = 0;

            public virtual string Name {
                get {
                    if (IsPackageSubfolder) {
                        return PackageName;
                    }

                    return Property.name;
                }
            }

            public virtual int InstanceID {
                get { return Property.instanceID; }
            }

            public virtual bool HasChildren {
                get { return Property.hasChildren; }
            }

            public virtual Texture Icon {
                get { return Property.icon; }
            }

            // -----------------------

            public ItemInfos(string _guid) {
                string _path = AssetDatabase.GUIDToAssetPath(_guid);

                GUID = _guid;
                GetParentFolder();

                Property = new HierarchyProperty(HierarchyType.Assets, _path, true);
                IsAssetsFolder = AssetDatabase.GUIDToAssetPath(Property.guid) == "Assets";

                HasSubfolders = AssetDatabase.GetSubFolders(_path).Length != 0;
                IsFolder = Property.isFolder;

                // Package folder.
                if (IsFolder) {
                    TextAsset _textAsset = AssetDatabase.LoadAssetAtPath($"{_path}/package.json", typeof(TextAsset)) as TextAsset;

                    if (_textAsset != null) {
                        string _text = _textAsset.text;
                        int _index = _text.IndexOf(PackageDisplayNameHeader);

                        if (_index != -1) {
                            _index += PackageDisplayNameHeader.Length;

                            int _startIndex = _text.IndexOf(PackageDisplayNameSeparator, _index, _text.Length - _index) + 1;
                            int _endIndex   = _text.IndexOf(PackageDisplayNameSeparator, _startIndex, _text.Length - _startIndex);

                            IsPackageSubfolder = true;
                            PackageName = _text.Substring(_startIndex, _endIndex - _startIndex);
                        }
                    }
                }
            }

            protected ItemInfos() { }

            // -----------------------

            public void OnDraw() {
                if (IsFolder) {
                    return;
                }

                switch (Event.current.type) {
                    // Get children property.
                    case EventType.Repaint:
                        if (drawCount != 0) {
                            Property.Next(expandedProjectWindowItems);
                        }

                        drawCount++;
                        break;

                    // Reset property.
                    case EventType.Layout:
                        if (drawCount != 0) {
                            if (drawCount > 1) {
                                Property.Reset();
                            }

                            count = drawCount;
                        }

                        drawCount = 0;
                        break;

                    default:
                        return;
                }
            }

            public void GetParentFolder() {
                if (IsAssetsFolder || string.IsNullOrEmpty(GUID)) {
                    return;
                }

                string _path = AssetDatabase.GUIDToAssetPath(GUID);
                string _folderPath = ProjectWindowUtil.GetContainingFolder(_path);

                if (!string.IsNullOrEmpty(_folderPath)) {
                    ParentFolderGUID = AssetDatabase.AssetPathToGUID(_folderPath);
                }
            }

            public virtual bool IsExpanded(bool _isTreeView) {
                return (count > 1) || (_isTreeView && HasChildren && ArrayUtility.Contains(expandedProjectWindowItems, Property.instanceID));
            }
        }

        private class PackageItemInfos : ItemInfos {
            public override string Name {
                get { return "Packages"; }
            }

            public override int InstanceID {
                get { return 0; }
            }

            public override bool HasChildren {
                get { return true; }
            }

            public override Texture Icon {
                get { return null; }
            }

            private bool isExpanded = false;

            // -----------------------

            public PackageItemInfos() {
                IsFolder = true;
                HasSubfolders = true;
            }

            // -----------------------

            public override bool IsExpanded(bool _isTreeView) {
                bool _isExpanded = isExpanded;
                isExpanded = false;

                return _isExpanded;
            }

            public void SetExpanded() {
                isExpanded = true;
            }
        }
        #endregion

        #region Global Members
        private static readonly Dictionary<string, ItemInfos> itemInfos = new Dictionary<string, ItemInfos>();
        private static readonly PackageItemInfos packageFolderItemInfo  = new PackageItemInfos();

        private static int[] expandedProjectWindowItems = null;
        private static string[] selectedObjects         = null;

        // -----------------------

        static EnhancedProjectBrowser() {
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update += RefreshProjectState;
            
            RefreshProjectState();
        }
        #endregion

        #region Editor GUI
        private static readonly HashSet<string> selectedTreeViewItems = new HashSet<string>();

        private static readonly List<Rect> indentPositions  = new List<Rect>() { Rect.zero };
        private static readonly Color dragPreviewColor      = new Color(1f, 1f, 1f, .1f);
        private static readonly Color selectionColor        = new Color(1f, 1f, 1f, .12f);

        private static Rect assetsPosition = Rect.zero;

        // -----------------------

        private static void OnProjectItemGUI(string _guid, Rect _position) {
            // Activation.
            EnhancedProjectBrowserEnhancedSettings _settings = EnhancedProjectBrowserEnhancedSettings.Settings;
            if (!_settings.Enabled) {
                return;
            }

            ItemInfos _item;

            // Ignore empty items (like favorites).
            if (string.IsNullOrEmpty(_guid)) {
                if ((_position.x != assetsPosition.x) || (_position.y < assetsPosition.y)) {
                    return;
                }

                // Detect package folder item using the assets folder position.
                _item = packageFolderItemInfo;
            } else if (!itemInfos.TryGetValue(_guid, out _item)) {
                // Item registration.
                _item = new ItemInfos(_guid);
                itemInfos.Add(_guid, _item);
            }

            if (_item.IsAssetsFolder) {
                assetsPosition = _position;
            }

            // Item callback.
            _item.OnDraw();

            // Position.
            var _positionInfos = GetItemPosition(ref _position);
            bool _isTreeView = _positionInfos._isTreeView;
            bool _isSmall = _positionInfos._isSmall;

            // Expanded state.
            bool _isExpanded = _item.IsExpanded(_isTreeView);

            if (_item.IsPackageSubfolder && _isTreeView) {
                packageFolderItemInfo.SetExpanded();
            }

            // Get non tree view items parent.
            if (!_isTreeView) {
                selectedTreeViewItems.Add(_item.ParentFolderGUID);
            }
            
            if (_isSmall && (!EditorGUIUtility.editingTextField || !IsSelected(_guid))) {
                Rect _full = new Rect(0f, _position.y, Screen.width, _position.height);

                // Line background.
                bool _isOdd = (int)(_position.y / _position.height) % 2 == 0;
                Color _backgroundColor = _isOdd ? EnhancedEditorGUIUtility.GUIPeerLineColor : EnhancedEditorGUIUtility.GUIThemeBackgroundColor;

                EditorGUI.DrawRect(_full, _backgroundColor);

                // Feedback background.
                if (IsSelected(_guid) && (!_isTreeView || selectedTreeViewItems.Contains(_item.GUID))) {
                    _backgroundColor = EnhancedEditorGUIUtility.GUISelectedColor;
                    EditorGUI.DrawRect(_full, _backgroundColor);
                } else if ((DragAndDrop.visualMode == DragAndDropVisualMode.Move) && _full.Contains(Event.current.mousePosition)) {
                    EditorGUI.DrawRect(_full, dragPreviewColor);
                } else if (_isTreeView && selectedTreeViewItems.Contains(_item.GUID)) {
                    EditorGUI.DrawRect(_full, selectionColor);
                }

                // Item name.
                bool _isRoot = _position.x == assetsPosition.x;

                if (_isTreeView) {
                    _full.x = _position.xMax + (_isRoot ? 2f : 1f);
                    _full.y -= 1f;
                } else {
                    _full.x = _position.xMax + 1f;
                }

                GUIStyle _labelStyle = _isRoot ? EditorStyles.boldLabel : EditorStyles.label;
                EditorGUI.LabelField(_full, EnhancedEditorGUIUtility.GetLabelGUI(_item.Name), _labelStyle);

                // Indent position.
                Rect _indentPosition = new Rect(){
                    x = _position.x - 20f,
                    y = _position.y + (_position.height / 2f),
                    width = 20f,
                    height = 1f
                };

                // Item foldout.
                if ((_isTreeView && _item.HasSubfolders) || (!_item.IsFolder && _item.HasChildren)) {
                    _full.x = _position.x - 14f;
                    _full.width = 15f;
                    _full.y += 1f;

                    EditorGUI.Foldout(_full, _isExpanded, GUIContent.none);

                    _indentPosition.width -= 12f;
                }

                // Indent dotted lines.
                if (_isTreeView && (Event.current.type == EventType.Repaint)) {
                    // Ignore root folders.
                    if (_position.x > assetsPosition.x) {
                        EnhancedEditorGUI.HorizontalDottedLine(_indentPosition, 1f, 1f);

                        while (_position.x < indentPositions.Last().x) {
                            indentPositions.RemoveLast();
                        }

                        // Vertical line.
                        if (!indentPositions.Last(out Rect _lastIndentPosition) || (_lastIndentPosition.y >= _position.y)) {
                            _lastIndentPosition = new Rect(_position.x, 0f, 1f, 1f);
                        }

                        _indentPosition = new Rect() {
                            x = _indentPosition.x - 2f,
                            y = _lastIndentPosition.y + 8f,
                            yMax = _indentPosition.y + 2f,
                            width = 1f
                        };

                        if (_position.x != _lastIndentPosition.x) {
                            _indentPosition.yMin += 8f;
                        }

                        EnhancedEditorGUI.VerticalDottedLine(_indentPosition, 1f, 1f);

                        // Only keep the last position for the same indent value.
                        if (_position.x == _lastIndentPosition.x) {
                            indentPositions.RemoveLast();
                        }
                    }

                    indentPositions.Add(_position);
                }
            } else {
                // Draw over base large icons.
                EditorGUI.DrawRect(_position, EnhancedEditorGUIUtility.GUIPeerLineColor);
            }

            // Get icon informations.
            Texture _icon = _item.Icon;
            Color _color = Color.white;
            
            if (_item.IsFolder) {
                _icon = _isExpanded ? _settings.DefaultOpenFolderIcon
                      : (_item.HasChildren ? _settings.DefaultFolderIcon
                      : _settings.DefaultEmptyFolderIcon);

                _color = _settings.FolderColor.Get();
            }

            // Draw icon.
            using (var _scope = EnhancedGUI.GUIColor.Scope(_color)) {
                GUI.DrawTexture(_position, _icon);
            }
        }
        #endregion

        #region Utility
        private static (bool _isSmall, bool _isTreeView) GetItemPosition(ref Rect _position) {
            // Position indent is 14 pixels width.
            // Tree view starts with a x position of 16, against 14 for the other project column.
            bool _isTreeView = (_position.x - 16f) % 14f == 0f;
            bool _isSmall = _position.width > _position.height;

            if (_isSmall) {
                _position.width = _position.height;

                if (!_isTreeView) {
                    _position.x += 3f;
                }
            } else {
                _position.height = _position.width;
            }

            return (_isSmall, _isTreeView);
        }

        private static bool IsSelected(string _guid) {
            return ArrayUtility.Contains(selectedObjects, _guid);
        }

        // -----------------------

        private static void OnProjectChanged() {
            itemInfos.Clear();

            foreach (var _item in itemInfos) {
                _item.Value.GetParentFolder();
            }
        }

        private static void RefreshProjectState() {
            expandedProjectWindowItems = InternalEditorUtility.expandedProjectWindowItems;
            selectedObjects = Selection.assetGUIDs;

            // Tree view selection folder update.
            selectedTreeViewItems.Clear();

            string _activeFolder = EnhancedEditorGUIUtility.GetActiveFolderPath();
            if (!string.IsNullOrEmpty(_activeFolder)) {
                selectedTreeViewItems.Add(AssetDatabase.AssetPathToGUID(_activeFolder));
            }
        }
        #endregion
    }
}
