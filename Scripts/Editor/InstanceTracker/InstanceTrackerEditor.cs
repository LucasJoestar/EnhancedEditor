// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="InstanceTracker"/> editor.
    /// </summary>
	[CustomEditor(typeof(InstanceTracker)), CanEditMultipleObjects]
	public class InstanceTrackerEditor : UnityObjectEditor
    {
        #region Styles
        private static class Styles
        {
            public static readonly GUIStyle HeaderStyle = new GUIStyle(EditorStyles.label)
                                                                {
                                                                    fontSize = 16,
                                                                    fixedHeight = 22f
                                                                };
        }
        #endregion

        #region Editor Content
        private static readonly string multiObjectEditingMessage = $"{typeof(InstanceTracker).Name} does not support multi-object editing. " +
                                                                   $"Please select only one object at a time to preview.";

        private static readonly GUIContent headerGUI = new GUIContent();

        private InstanceTracker tracker = null;

        // -----------------------

        public override bool UseDefaultMargins() => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Multi-editing.
            if (targets.Length > 1)
            {
                headerGUI.text = headerGUI.tooltip
                               = $"Multiple {typeof(InstanceTracker).Name} Preview";

                return;
            }

            // Initialization.
            tracker = target as InstanceTracker;
            if (!tracker.Initialize())
                return;

            tracker.editor = this;
            headerGUI.text = headerGUI.tooltip
                           = tracker.targetType.Name;

            trackTabsGUI = new GUIContent[] { };
            selectedTrackTab = -1;
            selectedTrack = null;
            selectedInstance = null;

            // Callbacks.
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;

            // Get open scenes instances.
            foreach (var _track in tracker.tracks)
            {
                _track.IsLoaded = false;
            }

            for (int _i = 0; _i < SceneManager.sceneCount; _i++)
            {
                Scene _scene = SceneManager.GetSceneAt(_i);
                OnSceneOpened(_scene);
            }

            for (int _i = tracker.tracks.Length; _i-- > 0;)
            {
                var _track = tracker.tracks[_i];
                if (!_track.OnEnabled())
                {
                    tracker.RemoveTrack(_track);
                }
            }

            // Sort the tracks on enabled as they can be renamed by users.
            Array.Sort(tracker.tracks, (a, b) => a.SceneName.CompareTo(b.SceneName));
        }

        public override void OnInspectorGUI()
        {
            using (var _indentScope = new EditorGUI.IndentLevelScope())
            {
                GUILayout.Space(5f);
                
                // Header.
                EditorGUILayout.LabelField(headerGUI, Styles.HeaderStyle);
                GUILayout.Space(7f);

                // Multi editing is not allowed.
                if (targets.Length > 1)
                {
                    EditorGUILayout.HelpBox(multiObjectEditingMessage, UnityEditor.MessageType.Warning);
                    return;
                }

                // Tracker.
                DrawUnloadedTracks();
                DrawLoadedTracker();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if ((tracker != null) && (tracker.editor == this))
                tracker.editor = null;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }
        #endregion

        #region GUI Draw
        private const float UnloadedAreaHeight = 150f;
        private const float ToolbarHeight = 22f;
        private const float OpenSceneButtonWidth = 80f;

        private const string NoOpenedTrackMessage = "There is no \"{0}\" instance in the open scene(s).";
        private const string SelectTrackMessage = "Select a scene to look for \"{0}\" instances:";

        private static readonly GUIContent instancePreviewGUI = new GUIContent("Preview", "Shows / Hides a preview of all instances in the selected scene.");
        private static readonly GUIContent openSceneGUI = new GUIContent("Open Scene", "Opens the selected scene.");
        private static readonly GUIContent openSceneSingleGUI = new GUIContent("Open Single");
        private static readonly GUIContent openSceneAdditiveGUI = new GUIContent("Open Additive");
        private static readonly GUIContent selectObjectGUI = new GUIContent("Select", "Select this object.");

        private static readonly EditorColor sectionColor = new EditorColor(new Color(.65f, .65f, .65f), SuperColor.DarkGrey.Get());
        private static readonly EditorColor peerColor = new EditorColor(new Color(.8f, .8f, .8f), new Color(.25f, .25f, .25f));
        private static readonly EditorColor selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private static readonly Color openButtonColor = SuperColor.Green.Get();

        private GUIContent[] trackTabsGUI = new GUIContent[] { };
        private int selectedTrackTab = -1;

        private int trackSelectionControlID = -1;
        private int instanceSelectionControlID = -1;

        private string searchFilter = string.Empty;
        private bool isInstancePreview = false;

        [NonSerialized] private InstanceTracker.SceneTrack selectedUnloadedTrack = null;
        [NonSerialized] private InstanceTracker.SceneTrack selectedTrack = null;
        [NonSerialized] private Component selectedInstance = null;

        private bool doFocusUnloadedTrack = false;

        private Vector2 unloadedTracksScroll = new Vector2();
        private Vector2 trackTabsScroll = new Vector2();

        // -----------------------

        private void DrawUnloadedTracks()
        {
            Rect _position;
            trackSelectionControlID = EnhancedEditorGUIUtility.GetControlID(173, FocusType.Keyboard);

            // As the horizontal toolbar scope doesn't accept any indent, use a zero indent scope.
            using (var _zeroIndentScope = EnhancedEditorGUI.ZeroIndentScope())
            using (var _scope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.singleLineHeight);

                using (var _sectionScope = new EditorGUILayout.VerticalScope(GUILayout.Height(UnloadedAreaHeight)))
                {
                    // Background.
                    _position = _sectionScope.rect;
                    EditorGUI.DrawRect(_position, sectionColor);

                    _position.y -= 1f;
                    _position.height += 2f;

                    GUI.Label(_position, GUIContent.none, EditorStyles.helpBox);

                    // Toolbar.
                    using (var _toolbarScope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        // Draw an empty button all over the toolbar to draw its bounds.
                        {
                            Rect _toolbarPosition = _toolbarScope.rect;
                            _toolbarPosition.xMin += 1f;

                            GUI.Label(_toolbarPosition, GUIContent.none, EditorStyles.toolbarButton);
                        }

                        // Search filter.
                        string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter);
                        if (_searchFilter != searchFilter)
                        {
                            searchFilter = _searchFilter;
                            _searchFilter = _searchFilter.ToLower();

                            foreach (var _track in tracker.tracks)
                            {
                                _track.UpdateVisibility(_searchFilter);
                            }
                        }
                    }

                    // Non-loaded scene tracks.
                    using (var _scroll = new GUILayout.ScrollViewScope(unloadedTracksScroll))
                    {
                        unloadedTracksScroll = _scroll.scrollPosition;

                        int _index = 0;
                        for (int _i = 0; _i < tracker.tracks.Length; _i++)
                        {
                            var _track = tracker.tracks[_i];
                            if (!_track.IsVisible)
                                continue;

                            // Visible track.
                            _position = EditorGUILayout.GetControlRect();
                            _position.xMin -= 2f;
                            _position.xMax += 2f;
                            _position.height += 2f;

                            bool _isSelected = selectedUnloadedTrack == _track;

                            EnhancedEditorGUI.BackgroundLine(_position, _isSelected, _index, selectedColor, peerColor);
                            Rect _temp = new Rect(_position)
                            {
                                xMin = _position.x + 10f
                            };

                            EditorGUI.LabelField(_temp, _track.SceneName);

                            GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_track.Instances.Length.ToString());
                            _temp.xMin = _temp.xMax - EditorStyles.label.CalcSize(_label).x - 5f;

                            EditorGUI.LabelField(_temp, _label);

                            // Scroll focus.
                            if (_isSelected && doFocusUnloadedTrack && (Event.current.type == EventType.Repaint))
                            {
                                Vector2 _areaSize = new Vector2(_position.x, UnloadedAreaHeight - 20f);
                                unloadedTracksScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(unloadedTracksScroll, _position, _areaSize);

                                doFocusUnloadedTrack = false;
                                Repaint();
                            }

                            // Track selection.
                            if (_position.Event(out Event _event) == EventType.MouseDown)
                            {
                                SelectUnloadedTrack(_track);
                                if (_event.clickCount == 2)
                                    OpenSelectedScene();

                                _event.Use();
                            }

                            // Key selection.
                            if (_isSelected && (GUIUtility.keyboardControl == trackSelectionControlID))
                            {
                                int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                                switch (_switch)
                                {
                                    case -1:
                                        for (int _j = _i; _j-- > 0;)
                                        {
                                            var _selectedTrack = tracker.tracks[_j];
                                            if (_selectedTrack.IsVisible)
                                            {
                                                SelectUnloadedTrack(_selectedTrack);
                                                break;
                                            }
                                        }
                                        break;

                                    case 1:
                                        for (int _j = _i + 1; _j < tracker.tracks.Length; _j++)
                                        {
                                            var _selectedTrack = tracker.tracks[_j];
                                            if (_selectedTrack.IsVisible)
                                            {
                                                SelectUnloadedTrack(_selectedTrack);
                                                break;
                                            }
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }

                            // Context click.
                            if (EnhancedEditorGUIUtility.ContextClick(_position))
                            {
                                GenericMenu _menu = new GenericMenu();
                                _menu.AddItem(openSceneSingleGUI, false, () =>
                                {
                                    if (!SceneHandlerWindow.OpenSceneFromGUID(selectedUnloadedTrack.SceneGUID, OpenSceneMode.Single))
                                        RemoveSelectedUnloadedTrack();
                                });

                                _menu.AddItem(openSceneAdditiveGUI, false, () =>
                                {
                                    if (!SceneHandlerWindow.OpenSceneFromGUID(selectedUnloadedTrack.SceneGUID, OpenSceneMode.Additive))
                                        RemoveSelectedUnloadedTrack();
                                });

                                _menu.ShowAsContext();
                            }

                            _index++;
                        }

                        GUILayout.Space(5f);
                    }

                    // Unselect on empty space click.
                    if (EnhancedEditorGUIUtility.MouseDown(_sectionScope.rect))
                        selectedUnloadedTrack = null;
                }

                GUILayout.Space(10f);
            }

            GUILayout.Space(5f);

            // Selected track operations.
            bool _enabled = selectedUnloadedTrack != null;
            _position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, 20f));
            _position.xMax -= OpenSceneButtonWidth + 10f;

            if (_enabled)
            {
                isInstancePreview = EditorGUI.Foldout(_position, isInstancePreview, instancePreviewGUI, true);
            }

            _position.xMin = _position.xMax;
            _position.width = OpenSceneButtonWidth;

            using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(_enabled))
            using (var _colorScope = EnhancedGUI.GUIColor.Scope(openButtonColor))
            {
                if (GUI.Button(_position, openSceneGUI))
                {
                    OpenSelectedScene();
                }
            }

            // Selected track preview.
            if (isInstancePreview && _enabled)
            {
                using (var _indentScope = new EditorGUI.IndentLevelScope())
                {
                    for (int _i = 0; _i < selectedUnloadedTrack.Instances.Length; _i++)
                    {
                        string _instance = selectedUnloadedTrack.Instances[_i];
                        _position = EditorGUILayout.GetControlRect();

                        Rect _temp = new Rect(_position)
                        {
                            x = 0f,
                            width = EditorGUIUtility.currentViewWidth
                        };

                        EnhancedEditorGUI.BackgroundLine(_temp, false, _i);
                        EditorGUI.LabelField(_position, _instance);
                    }
                }
            }
        }

        private void DrawLoadedTracker()
        {
            GUILayout.Space(10f);

            // No track to select instances.
            if (selectedTrack == null)
            {
                EditorGUILayout.HelpBox(string.Format(NoOpenedTrackMessage, tracker.targetType.Name), UnityEditor.MessageType.Warning);
                return;
            }

            // Scene selection.
            EditorGUILayout.HelpBox(string.Format(SelectTrackMessage, tracker.targetType.Name), UnityEditor.MessageType.Info);
            GUILayout.Space(5f);

            using (var _scope = new GUILayout.ScrollViewScope(trackTabsScroll))
            {
                trackTabsScroll = _scope.scrollPosition;

                int _selectedTrackTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTrackTab, trackTabsGUI, GUILayout.Height(ToolbarHeight));
                if (_selectedTrackTab != selectedTrackTab)
                {
                    SelectTrack(_selectedTrackTab);
                }
            }

            GUILayout.Space(2f);
            instanceSelectionControlID = EnhancedEditorGUIUtility.GetControlID(173, FocusType.Keyboard);

            // Draw all scene instances.
            Event _event;
            int _index = 0;

            for (int _i = 0; _i < selectedTrack.SceneInstances.Count; _i++)
            {
                Component _instance = selectedTrack.SceneInstances[_i];
                if (_instance == null)
                    continue;

                Rect _position = EditorGUILayout.GetControlRect(true, 20f);
                Rect _temp = new Rect(_position);
                bool _isSelected = selectedInstance == _instance;

                _position.x = 0f;
                _position.width = EditorGUIUtility.currentViewWidth;

                EnhancedEditorGUI.BackgroundLine(_position, _isSelected, _index);
                EditorGUI.LabelField(_temp, _instance.name);

                _index++;

                // Select button.
                _temp.xMin = _temp.xMax - 50f;
                _temp.y += 1f;
                _temp.height -= 2f;

                if (GUI.Button(_temp, selectObjectGUI))
                {
                    EditorGUIUtility.PingObject(_instance);
                    Selection.activeObject = _instance;
                }

                // Selection.
                if (_position.Event(out _event) == EventType.MouseDown)
                {
                    SelectInstance(_instance);
                    switch (_event.clickCount)
                    {
                        case 1:
                            EditorGUIUtility.PingObject(_instance);
                            break;

                        case 2:
                            Selection.activeObject = _instance;
                            break;
                    }

                    _event.Use();
                }

                // Keyboard focus.
                if (_isSelected && (GUIUtility.keyboardControl == instanceSelectionControlID))
                {
                    int _switch = EnhancedEditorGUIUtility.VerticalKeys();
                    if (_switch != 0)
                    {
                        int _selectedIndex = Mathf.Clamp(_i + _switch, 0, selectedTrack.SceneInstances.Count - 1);
                        SelectInstance(selectedTrack.SceneInstances[_selectedIndex]);
                    }
                    else if ((_event.type == EventType.KeyDown) && (_event.keyCode == KeyCode.Return))
                    {
                        EditorGUIUtility.PingObject(_instance);
                        _event.Use();
                    }
                }
            }

            // Unselect instance on empty space click.
            _event = Event.current;
            if (_event.type == EventType.MouseDown)
            {
                selectedInstance = null;
                _event.Use();
            }
        }
        #endregion

        #region Scene Callback
        private void OnSceneOpened(Scene _scene, OpenSceneMode _mode = OpenSceneMode.Single)
        {
            if (!_scene.isLoaded || !_scene.IsValid() || string.IsNullOrEmpty(_scene.path))
                return;

            // Load instances.
            string _guid = AssetDatabase.AssetPathToGUID(_scene.path);
            foreach (var _track in tracker.tracks)
            {
                if (_track.Load(_scene, _guid, tracker.targetType))
                {
                    OnNewTrack(_track);
                    break;
                }
            }
        }

        private void OnSceneClosed(Scene _scene)
        {
            if (!_scene.IsValid() || string.IsNullOrEmpty(_scene.path))
                return;

            // Close scene.
            string _guid = AssetDatabase.AssetPathToGUID(_scene.path);
            foreach (var _track in tracker.tracks)
            {
                if (_track.Close(_guid))
                {
                    OnRemoveTrack(_track);
                    _track.UpdateVisibility(searchFilter.ToLower());

                    break;
                }
            }
        }
        #endregion

        #region Tracker Callback
        internal void OnNewTrack(InstanceTracker.SceneTrack _track)
        {
            GUIContent _tabGUI = new GUIContent(_track.SceneName, _track.SceneName);
            ArrayUtility.Add(ref trackTabsGUI, _tabGUI);

            SelectTrack(Mathf.Max(0, selectedTrackTab));
        }

        internal void OnRemoveTrack(InstanceTracker.SceneTrack _track)
        {
            int _index = Array.FindIndex(trackTabsGUI, t => t.text == _track.SceneName);
            if (_index > -1)
            {
                ArrayUtility.RemoveAt(ref trackTabsGUI, _index);
                SelectTrack(Mathf.Min(trackTabsGUI.Length - 1, selectedTrackTab));
            }
        }
        #endregion

        #region Utility
        private void OpenSelectedScene()
        {
            if (!SceneHandlerWindow.OpenSceneFromGUID(selectedUnloadedTrack.SceneGUID))
            {
                RemoveSelectedUnloadedTrack();
            }
        }

        private void RemoveSelectedUnloadedTrack()
        {
            tracker.RemoveTrack(selectedUnloadedTrack);
            selectedUnloadedTrack = null;
        }

        private void SelectUnloadedTrack(InstanceTracker.SceneTrack _track)
        {
            selectedUnloadedTrack = _track;
            doFocusUnloadedTrack = true;

            GUIUtility.keyboardControl = trackSelectionControlID;
        }

        private void SelectTrack(int _selectedTrackTab)
        {
            int _index = _selectedTrackTab;
            selectedTrackTab = _selectedTrackTab;

            foreach (var _track in tracker.tracks)
            {
                if (_track.IsLoaded)
                {
                    if (_index == 0)
                    {
                        selectedTrack = _track;
                        return;
                    }

                    _index--;
                }
            }

            selectedTrack = null;
        }

        private void SelectInstance(Component _component)
        {
            selectedInstance = _component;
            GUIUtility.keyboardControl = instanceSelectionControlID;
        }
        #endregion
    }
}
