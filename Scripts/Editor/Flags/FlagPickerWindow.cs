// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Flag picker editor window, used to manage <see cref="FlagReference"/>, <see cref="FlagValue"/>,
    /// <see cref="FlagReferenceGroup"/> and <see cref="FlagValueGroup"/> referenced <see cref="Flag"/> objects.
    /// </summary>
    public class FlagPickerWindow : EditorWindow {
        #region Picker Mode
        public enum Mode {
            Single,
            ReferenceGroup,
            ValueGroup
        }
        #endregion

        #region Flag Holder
        [Serializable]
        private struct FlagWrapper {
            public FlagHolder Holder;
            public Flag Flag;
            public bool IsVisible;
            public bool IsSelected;
            public bool IsSelectable;

            // -----------------------

            public FlagWrapper(Flag _flag, FlagHolder _holder) {
                Flag = _flag;
                Holder = _holder;

                IsVisible = true;
                IsSelected = false;
                IsSelectable = true;
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Creates and shows a new <see cref="Flag"/> picker window to edit a <see cref="FlagGroup{T}"/>.
        /// </summary>
        /// <param name="_group">The <see cref="FlagGroup{T}"/> to edit.</param>
        /// <inheritdoc cref="GetWindow(int, Flag, FlagHolder, Action{Flag, FlagHolder})"/>
        public static FlagPickerWindow GetWindow(int _controlID, FlagGroup _group) {
            controlID = _controlID;
            group = _group;
            mode = (_group is FlagValueGroup) ? Mode.ValueGroup : Mode.ReferenceGroup;

            FlagPickerWindow _window = DoGetWindow();
            return _window;
        }

        /// <summary>
        /// Creates and shows a new <see cref="Flag"/> picker window to edit a specific <see cref="Flag"/> reference.
        /// </summary>
        /// <param name="_controlID">Picker associated control id. Use the same id to get the selected flag with <see cref="GetSelectedFlag(int, out Flag, out FlagHolder)"/>.</param>
        /// <param name="_onSelectFlag">Event called when the user selects a new flag.</param>
        /// <returns><see cref="FlagPickerWindow{T}"/> instance on screen.</returns>
        public static FlagPickerWindow GetWindow(int _controlID, Flag _flag, FlagHolder _holder, Action<Flag, FlagHolder> _onSelectFlag = null) {
            controlID = _controlID;
            selectedFlag = new FlagWrapper(_flag, _holder);
            mode = Mode.Single;

            FlagPickerWindow _window = DoGetWindow();
            _window.onSelectFlag = _onSelectFlag;

            return _window;
        }

        private static FlagPickerWindow DoGetWindow() {
            hasChanged = false;

            FlagPickerWindow _window = GetWindow<FlagPickerWindow>(true, "Flag Selection Picker", true);

            _window.minSize = new Vector2(275f, 250f);
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string DataKey = "FlagPickerWindow";

        private const float RefreshButtonWidth = 60f;
        private const int FlagIndent = 2;

        private static int controlID = 0;
        private static bool hasChanged = false;
        private static FlagWrapper selectedFlag = default;
        private static FlagGroup group = null;
        private static Mode mode = Mode.Single;

        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh loaded flags.");

        [SerializeField] private FlagWrapper[] flags = new FlagWrapper[] { };
        private List<int> selectedGroupFlags = new List<int>();

        private bool UseGroup => mode != Mode.Single;

        private Action<Flag, FlagHolder> onSelectFlag = null;

        private string searchFilter = string.Empty;
        private bool doFocusSearchField = true;

        private Vector2 pickerScroll = new Vector2();
        private Vector2 groupScroll = new Vector2();

        private bool doFocusPickerElement = false;
        private bool doFocusGroupElement = false;

        private int lastSelectedFlagIndex = -1;

        // -----------------------

        private void OnEnable() {
            // Load values.
            string _json = EditorPrefs.GetString(DataKey, string.Empty);
            if (!string.IsNullOrEmpty(_json)) {
                JsonUtility.FromJsonOverwrite(_json, this);
            }

            RefreshFlags();
        }

        private void OnGUI() {
            // Toolbar rect.
            Rect _toolbarRect;
            using (var _scope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                Rect _temp = new Rect(_scope.rect) {
                    xMin = _scope.rect.xMax - RefreshButtonWidth
                };

                _toolbarRect = new Rect(EditorGUILayout.GetControlRect()) {
                    xMax = _temp.xMin - 3f
                };

                // Refresh button.
                if (GUI.Button(_temp, refreshGUI, EditorStyles.toolbarButton)) {
                    RefreshFlags();
                }
            }

            // Inspector.
            DrawInspector();

            string _controlID = EnhancedEditorGUIUtility.GetControlID(FocusType.Keyboard, _toolbarRect).ToString();

            // Finally, draw the search field to leave keyboard inputs for selecting flags.
            string _searchFilter = EnhancedEditorGUI.ToolbarSearchField(_controlID, _toolbarRect, searchFilter);
            if (_searchFilter != searchFilter) {
                searchFilter = _searchFilter;

                FilterFlags();
            }

            // Focus this search field once it has been drawn.
            if (doFocusSearchField) {
                GUI.FocusControl(_controlID);
                doFocusSearchField = false;
            }
        }

        private void OnLostFocus() {
            Close();
        }

        private void OnDisable() {
            // Save values.
            string _json = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(DataKey, _json);
        }
        #endregion

        #region GUI Draw
        private const float SectionBorder = 7f;
        private const float TransferButtonWidth = 28f;
        private const float TransferButtonHeight = 25f;

        private const string EmptyGroupMessage = "Add new flags by selecting them from the right and clicking on the \'<<\' button.";

        private readonly GUIContent pickerHeaderGUI = new GUIContent("Project Flags", "All available flags in the project.");
        private readonly GUIContent groupHeaderGUI = new GUIContent("Group Flags", "All flags registered in the selected group.");

        private readonly GUIContent addToGroupGUI = new GUIContent("<<", "Add the selected flags into the group.");
        private readonly GUIContent removeFromGroupGUI = new GUIContent(">>", "Remove the selected flags from the group.");

        private readonly GUIContent addToGroupContextGUI = new GUIContent("Add Selection to Group", "Add the selected flags into the group.");
        private readonly GUIContent removeFromGroupContextGUI = new GUIContent("Remove Selection from Group", "Remove the selected flags from the group.");

        private readonly EditorColor sectionColor = new EditorColor(new Color(.65f, .65f, .65f), SuperColor.DarkGrey.Get());
        private readonly EditorColor peerColor = new EditorColor(new Color(.8f, .8f, .8f), new Color(.25f, .25f, .25f));
        private readonly EditorColor selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;

        private int flagsControlID = -1;
        private int groupControlID = -1;

        // -----------------------

        private void DrawInspector() {
            GUILayout.Space(SectionBorder);

            using (var _horizontal = new GUILayout.HorizontalScope()) {
                GUILayout.Space(SectionBorder);
                GUILayoutOption _width = UseGroup
                                       ? GUILayout.Width(((position.width - TransferButtonWidth) / 2f) - (SectionBorder + 3f))
                                       : GUILayout.Width(position.width - (SectionBorder * 2f));

                // Group content.
                if (UseGroup) {
                    groupControlID = EnhancedEditorGUIUtility.GetControlID(719, FocusType.Keyboard);

                    using (var _vertical = new EditorGUILayout.VerticalScope(_width, GUILayout.ExpandHeight(true))) {
                        // Section background.
                        Rect _position = _vertical.rect;
                        DrawSection(_position);

                        // Toolbar header.
                        using (var _toolbar = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                            GUILayout.Label(groupHeaderGUI, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
                            _position.yMin = _toolbar.rect.yMax;
                        }

                        // Section content.
                        using (var _scroll = new GUILayout.ScrollViewScope(groupScroll)) {
                            groupScroll = _scroll.scrollPosition;
                            DrawGroup(_position);

                            GUILayout.Space(10f);
                        }

                        // Unselect on empty space click.
                        if (EnhancedEditorGUIUtility.DeselectionClick(_position)) {
                            selectedGroupFlags.Clear();
                        }

                        // Multi-selection keys.
                        if (GUIUtility.keyboardControl == groupControlID) {
                            int _index = (selectedGroupFlags.Count == 0) ? -1 : selectedGroupFlags.Last();
                            EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(group.Array, IsGroupFlagSelected, CanSelectGroupFlag, OnSelectGroupFlag, _index);
                        }

                        // Context click menu.
                        if ((selectedGroupFlags.Count != 0) && EnhancedEditorGUIUtility.ContextClick(_position)) {
                            GenericMenu _menu = new GenericMenu();

                            _menu.AddItem(removeFromGroupContextGUI, false, RemoveSelectionFromGroup);
                            _menu.ShowAsContext();
                        }
                    }

                    // Transfer buttons.
                    using (var _scope = new GUILayout.VerticalScope()) {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(addToGroupGUI, GUILayout.Width(TransferButtonWidth), GUILayout.Height(TransferButtonHeight))) {
                            AddSelectionToGroup();
                        }

                        GUILayout.Space(12f);

                        if (GUILayout.Button(removeFromGroupGUI, GUILayout.Width(TransferButtonWidth), GUILayout.Height(TransferButtonHeight))) {
                            RemoveSelectionFromGroup();
                        }

                        GUILayout.Space(50f);
                        GUILayout.FlexibleSpace();
                    }
                }

                // Picker.
                flagsControlID = EnhancedEditorGUIUtility.GetControlID(720, FocusType.Keyboard);

                using (var _vertical = new EditorGUILayout.VerticalScope(_width, GUILayout.ExpandHeight(true))) {
                    // Section background.
                    Rect _position = _vertical.rect;
                    DrawSection(_position);

                    // Toolbar header.
                    using (var _toolbar = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                        GUILayout.Label(pickerHeaderGUI, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
                        _position.yMin = _toolbar.rect.yMax;
                    }

                    // Section content.
                    using (var _scroll = new GUILayout.ScrollViewScope(pickerScroll)) {
                        pickerScroll = _scroll.scrollPosition;
                        DrawFlags(_position);

                        GUILayout.Space(10f);
                    }

                    // Unselect on empty space click.
                    if (EnhancedEditorGUIUtility.DeselectionClick(_position)) {
                        for (int i = 0; i < flags.Length; i++) {
                            flags[i].IsSelected = false;
                        }

                        lastSelectedFlagIndex = -1;
                        hasChanged = false;
                    }

                    if (UseGroup) {
                        // Multi-selection keys.
                        if (GUIUtility.keyboardControl == flagsControlID) {
                            EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(flags, IsFlagSelected, CanSelectFlag, OnSelectFlag, lastSelectedFlagIndex);
                        }

                        // Context click menu.
                        if ((lastSelectedFlagIndex != -1) && EnhancedEditorGUIUtility.ContextClick(_position)) {
                            GenericMenu _menu = new GenericMenu();

                            _menu.AddItem(addToGroupContextGUI, false, AddSelectionToGroup);
                            _menu.ShowAsContext();
                        }
                    } else if ((lastSelectedFlagIndex != -1) && (GUIUtility.keyboardControl == flagsControlID)) {
                        // Selection keys.
                        int _switch = EnhancedEditorGUIUtility.VerticalKeys();

                        if (_switch != 0) {
                            int _index = lastSelectedFlagIndex;

                            while (true) {
                                _index += _switch;

                                if ((_index == -1) || (_index == flags.Length))
                                    break;

                                if (flags[_index].IsVisible) {
                                    SelectFlag(_index);
                                    break;
                                }
                            }
                        }
                    }
                }

                GUILayout.Space(SectionBorder);
            }

            GUILayout.Space(SectionBorder);
        }

        private void DrawFlags(Rect _area) {
            FlagHolder _holder = null;
            int _peerIndex = 0;

            EditorGUI.indentLevel = FlagIndent;

            for (int i = 0; i < flags.Length; i++) {
                FlagWrapper _flag = flags[i];
                if (!_flag.IsVisible || !_flag.IsSelectable)
                    continue;

                // Holder header.
                if (_holder != _flag.Holder) {
                    _holder = _flag.Holder;

                    EditorGUI.indentLevel -= FlagIndent;

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(_holder.name, EditorStyles.boldLabel);

                    EditorGUI.indentLevel += FlagIndent;
                }

                Rect _position = GetFlagPosition();

                // Flag field.
                using (var _scope = new EditorGUI.DisabledGroupScope(!_flag.IsSelectable)) {
                    EnhancedEditorGUI.BackgroundLine(_position, _flag.IsSelected, _peerIndex++, selectedColor, peerColor);
                    EditorGUI.LabelField(_position, _flag.Flag.Name);
                }

                // Scroll focus.
                if ((i == lastSelectedFlagIndex) && doFocusPickerElement && (Event.current.type == EventType.Repaint)) {
                    Vector2 _areaSize = new Vector2(0f, _area.height);
                    pickerScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(pickerScroll, _position, _areaSize);

                    doFocusPickerElement = false;
                    Repaint();
                }

                // Select on click.
                if (UseGroup) {
                    EnhancedEditorGUIUtility.MultiSelectionClick(_position, flags, i, IsFlagSelected, CanSelectFlag, OnSelectFlag);
                } else if (EnhancedEditorGUIUtility.MouseDown(_position)) {
                    SelectFlag(i);
                    if (Event.current.clickCount > 1) {
                        Close();
                    }
                }
            }

            EditorGUI.indentLevel -= FlagIndent;
        }

        private void DrawGroup(Rect _area) {
            // Empty group message.
            if ((group == null) || group.Count == 0) {
                GUILayout.Space(5f);
                EditorGUILayout.HelpBox(EmptyGroupMessage, UnityEditor.MessageType.Info, true);

                return;
            }

            switch (mode) {
                // Reference group.
                case Mode.ReferenceGroup:
                    FlagReferenceGroup _refGroup = group as FlagReferenceGroup;
                    DrawFlag((position, index) => EditorGUI.LabelField(position, _refGroup.Flags[index].Flag.Name));
                    break;

                // Value group.
                case Mode.ValueGroup:
                    FlagValueGroup _valGroup = group as FlagValueGroup;

                    DrawFlag((position, index) => {
                        EnhancedEditorGUI.GetFlagRects(position, out Rect _labelPosition, out Rect _valuePosition);

                        EditorGUI.LabelField(_labelPosition, _valGroup.Flags[index].Flag.Name);

                        bool _value = _valGroup.Flags[index].Value;
                        using (var _scope = new EditorGUI.ChangeCheckScope()) {
                            _value = EnhancedEditorGUI.BoolPopupField(_valuePosition, _value);

                            if (_scope.changed) {
                                _valGroup.Flags[index].Value = _value;

                                hasChanged = true;
                                InternalEditorUtility.RepaintAllViews();
                            }
                        }
                    });
                    break;
            }

            // ----- Local Method ----- \\

            void DrawFlag(Action<Rect, int> _onDrawFlag) {
                int _peerIndex = 0;

                for (int i = 0; i < group.Count; i++) {
                    Rect _position = GetFlagPosition();

                    // Flag field.
                    EnhancedEditorGUI.BackgroundLine(_position, IsGroupFlagSelected(i), _peerIndex++, selectedColor, peerColor);

                    Rect _temp = new Rect(_position){
                        x = _position.x + 2f,
                        width = _position.width - 4f
                    };

                    _onDrawFlag(_temp, i);

                    // Scroll focus.
                    if ((i == lastSelectedFlagIndex) && doFocusGroupElement && (Event.current.type == EventType.Repaint)) {
                        Vector2 _areaSize = new Vector2(0f, _area.height);
                        groupScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(groupScroll, _position, _areaSize);

                        doFocusGroupElement = false;
                        Repaint();
                    }

                    // Multi selection.
                    EnhancedEditorGUIUtility.MultiSelectionClick(_position, group.Array, i, IsGroupFlagSelected, CanSelectGroupFlag, OnSelectGroupFlag);
                }
            }
        }

        // -----------------------

        private void DrawSection(Rect _position) {
            EditorGUI.DrawRect(_position, sectionColor);

            _position.y -= 1f;
            _position.height += 2f;

            GUI.Label(_position, GUIContent.none, EditorStyles.helpBox);
        }

        private Rect GetFlagPosition() {
            Rect _position = EditorGUILayout.GetControlRect();
            _position.xMin -= 2f;
            _position.xMax += 2f;
            _position.height += 2f;

            return _position;
        }
        #endregion

        #region Flag Selection
        private bool CanSelectFlag(int _index) {
            ref FlagWrapper _flag = ref flags[_index];
            return _flag.IsVisible && _flag.IsSelectable;
        }

        private bool IsFlagSelected(int _index) {
            return flags[_index].IsSelected;
        }

        private void OnSelectFlag(int _index, bool _isSelected) {
            ref FlagWrapper _wrapper = ref flags[_index];

            if (!_wrapper.IsVisible || !_wrapper.IsSelectable) {
                return;
            }

            _wrapper.IsSelected = _isSelected;

            if (_isSelected) {
                lastSelectedFlagIndex = _index;
                doFocusPickerElement = true;

                GUIUtility.keyboardControl = flagsControlID;
            } else if (!Array.Exists(flags, (f) => f.IsSelected)) {
                lastSelectedFlagIndex = -1;
            }
        }

        private void SelectFlag(int _index) {
            flags[_index].IsSelected = true;

            if (lastSelectedFlagIndex != -1) {
                flags[lastSelectedFlagIndex].IsSelected = false;
            }

            lastSelectedFlagIndex = _index;
            doFocusPickerElement = true;

            selectedFlag = flags[_index];
            hasChanged = true;
            onSelectFlag?.Invoke(selectedFlag.Flag, selectedFlag.Holder);

            GUIUtility.keyboardControl = flagsControlID;
            InternalEditorUtility.RepaintAllViews();
        }

        // -----------------------

        private bool CanSelectGroupFlag(int _index) {
            return true;
        }

        private bool IsGroupFlagSelected(int _index) {
            return selectedGroupFlags.Contains(_index);
        }

        private void OnSelectGroupFlag(int _index, bool _isSelected) {
            if (_isSelected) {
                int _elementIndex = selectedGroupFlags.IndexOf(_index);
                if (_elementIndex == -1) {
                    selectedGroupFlags.Add(_index);
                }

                doFocusGroupElement = true;
                GUIUtility.keyboardControl = groupControlID;
            } else {
                selectedGroupFlags.Remove(_index);
            }
        }

        // -----------------------

        private void AddSelectionToGroup() {
            for (int i = 0; i < flags.Length; i++) {
                ref FlagWrapper _flag = ref flags[i];

                if (_flag.IsSelected && _flag.IsVisible && _flag.IsSelectable) {
                    if (!group.ContainFlag(_flag.Flag, out _)) {
                        group.AddFlag(_flag.Flag);
                    }

                    _flag.IsSelected = false;
                    _flag.IsSelectable = false;
                }
            }

            hasChanged = true;
            InternalEditorUtility.RepaintAllViews();
        }

        private void RemoveSelectionFromGroup() {
            if (selectedGroupFlags.Count == 0) {
                return;
            }

            selectedGroupFlags.Sort();
            for (int i = selectedGroupFlags.Count; i-- > 0;) {
                group.RemoveFlagAt(selectedGroupFlags[i]);
            }

            selectedGroupFlags.Clear();

            for (int i = 0; i < flags.Length; i++) {
                Flag _flag = flags[i].Flag;
                flags[i].IsSelectable = !group.ContainFlag(_flag, out _);
            }

            hasChanged = true;
            InternalEditorUtility.RepaintAllViews();
        }
        #endregion

        #region Get Selected Flag
        /// <summary>
        /// Get the selected flag by the user for a specific control id.
        /// </summary>
        /// <param name="_controlID">Control id to get the selected flag for.</param>
        /// <param name="_flag">Selected flag.</param>
        /// <returns>True if the user selected a new flag, false otherwise.</returns>
        public static bool GetSelectedFlag(int _controlID, out Flag _flag) {
            if (hasChanged && (_controlID == controlID)) {
                _flag = selectedFlag.Flag;

                hasChanged = false;
                GUI.changed = true;

                return true;
            }

            _flag = null;
            return false;
        }

        /// <summary>
        /// Get the flag group selection made by the user for a specific control id.
        /// </summary>
        /// <param name="_controlID">Control id to get the flag group selection for.</param>
        /// <param name="_group">The flag group selection.</param>
        /// <returns>True if the user selection has changed, false otherwise.</returns>
        public static bool GetSelectedFlagGroup(int _controlID, out FlagGroup _group) {
            if (hasChanged && (_controlID == controlID)) {
                _group = group;

                hasChanged = false;
                GUI.changed = true;

                return true;
            }

            _group = null;
            return false;
        }
        #endregion

        #region Utility
        private void RefreshFlags() {
            // Get all flags.
            FlagHolder[] _assets = EnhancedEditorUtility.LoadAssets<FlagHolder>();
            int _count = 0;

            foreach (FlagHolder _asset in _assets) {
                _count += _asset.Flags.Length;
            }

            if (flags.Length != _count) {
                flags = new FlagWrapper[_count];
            }

            _count = 0;

            foreach (FlagHolder _asset in _assets) {
                foreach (Flag _flag in _asset.Flags) {
                    if (_flag != flags[_count].Flag) {
                        flags[_count] = new FlagWrapper(_flag, _asset);
                    }

                    ref FlagWrapper _wrapper = ref flags[_count];

                    // Cannot select flag if already in group.
                    _wrapper.IsSelectable = UseGroup
                                          ? !group.ContainFlag(_flag, out _)
                                          : true;

                    if (!UseGroup && (selectedFlag.Flag == _flag)) {
                        lastSelectedFlagIndex = _count;
                        _wrapper.IsSelected = true;
                    }

                    _count++;
                }
            }
        }

        private void FilterFlags() {
            string _searchFilter = searchFilter.ToLower();

            for (int i = 0; i < flags.Length; i++) {
                ref FlagWrapper _flag = ref flags[i];
                _flag.IsVisible = $"{_flag.Holder.name.ToLower()} {_flag.Flag.Name.ToLower()}".Contains(_searchFilter);
            }
        }
        #endregion
    }
}
