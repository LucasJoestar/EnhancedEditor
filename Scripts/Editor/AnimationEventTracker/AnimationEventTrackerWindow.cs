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
    /// Editor window used to track down all animation events in the project,
    /// and find among them corrupted ones which are referencing obsolete methods.
    /// </summary>
    public class AnimationEventTrackerWindow : EditorWindow
    {
        #region Event Method Wrapper
        private enum EventMethodParameterType
        {
            None,
            Int,
            Enum,
            Float,
            String,
            Object
        }

        private class EventMethodWrapper
        {
            public MethodInfo MethodInfo = null;
            public EventMethodParameterType Type = EventMethodParameterType.None;

            public Type ObjectType = null;
            public string[] EnumNames = null;

            public GUIContent Label = null;
            public GUIContent MethodLabel = null;

            // -----------------------

            public EventMethodWrapper(MethodInfo _method, string _label, EventMethodParameterType _type)
            {
                MethodInfo = _method;
                Type = _type;

                Label = new GUIContent(_label);
                MethodLabel = new GUIContent($"<color=green>{_label.Replace('/', '.')}</color>");

                if (_type != EventMethodParameterType.None)
                {
                    MethodLabel.text = MethodLabel.text.Replace("(", "(</color><color=olive>").Replace(")", ":   ");

                    switch (_type)
                    {
                        case EventMethodParameterType.Enum:
                            Type _parameterType = _method.GetParameters()[0].ParameterType;
                            _parameterType.GetEnumNames();
                            break;

                        case EventMethodParameterType.Object:
                            ObjectType = _method.GetParameters()[0].ParameterType;
                            break;
                    }
                }
            }
        }
        #endregion

        #region Corrupted Animator
        [Serializable]
        private class CorruptedAnimator
        {
            public Animator Animator = null;
            public string[] CorruptedClips = new string[] { };
            public bool Foldout = false;

            // -----------------------

            public CorruptedAnimator() { }

            public CorruptedAnimator(Animator _animator, string[] _corruptedClips)
            {
                Animator = _animator;
                CorruptedClips = _corruptedClips;
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="AnimationEventTrackerWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="AnimationEventTrackerWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Animation Event Tracker", false, 101)]
        public static AnimationEventTrackerWindow GetWindow()
        {
            AnimationEventTrackerWindow _window = GetWindow<AnimationEventTrackerWindow>("Animation Event Tracker");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string UndoRecordTitle = "Animation Tracker Change";

        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh all selected animator clips and events.");
        private readonly GUIContent[] tabsGUI = new GUIContent[]
                                                    {
                                                        new GUIContent("Animator Tracker", "Track down all animation events within a specific animator."),
                                                        new GUIContent("Scene(s) Corrupted Events", "Track down all corrupted events from the open scene(s) animators."),
                                                        new GUIContent("Database Corrupted Events", "Track down all corrupted events from the asset database.")
                                                    };

        private readonly Color buttonColor = SuperColor.Green.Get();

        [SerializeField] private int selectedTab = 0;

        private Vector2 scroll = new Vector2();

        // -----------------------

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoOperation;
            Undo.undoRedoPerformed += OnUndoRedoOperation;

            titleContent.image = EditorGUIUtility.IconContent("DotFrameDotted").image;

            InitializeAnimatorTracker();
            InitializeCorruptedEventTracker();
        }

        private void OnGUI()
        {
            Undo.RecordObject(this, UndoRecordTitle);

            // Toolbar.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    RefreshAnimatorInfos();
                }

                GUILayout.FlexibleSpace();
            }

            // Content.
            using (var _scroll = new GUILayout.ScrollViewScope(scroll))
            {
                scroll = _scroll.scrollPosition;
                GUILayout.Space(5f);

                // Tab selection. Use a minimum width value to avoid undesired shift on small window size.
                selectedTab = EnhancedEditorGUILayout.CenteredToolbar(selectedTab, tabsGUI, GUILayout.Height(25f), GUILayout.MinWidth(50f));

                GUILayout.Space(5f);

                switch (selectedTab)
                {
                    case 0:
                        DrawAnimatorTracker();
                        break;

                    case 1:
                        DrawSceneTracker();
                        break;

                    case 2:
                        DrawDatabaseTracker();
                        break;
                }

                GUILayout.Space(10f);
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoOperation;
        }

        // -----------------------

        private void OnUndoRedoOperation()
        {
            // Refresh the window content on undo / redo operation, as clip and method infos might require a reload.
            RefreshAnimatorInfos(selectedClip);
            Repaint();
        }
        #endregion

        #region Animator Tracker
        private const float SelectButtonWidth = 60f;
        private const float SwitchButtonWidth = 75f;
        private const float SetEventButtonWidth = 100f;
        private const float EventHeight = 42f;

        private const string SelectedClipKey = "AnimationEventTrackerClip";
        private const string NoAnimatorMessage = "Please select an animator to edit.";
        private const string NoClipMessage = "Selected animator doesn't have any clip.";
        private const string MissingEventMessageFormat = "Missing events found on {0} clip(s)!";

        private readonly GUIContent headerGUI = new GUIContent("Animator:", "Assign an animator to track down events.");
        private readonly GUIContent autoSelectGUI = new GUIContent("Auto-Select Clip", "When enabled, the selected clip will also be automatically selected in the animation window.");
        private readonly GUIContent animatorGUI = new GUIContent(string.Empty, "Currently tracking animator.");
        private readonly GUIContent clipGUI = new GUIContent("Animation Clip", "Animation to track events.");

        private readonly GUIContent previousGUI = new GUIContent("PREVIOUS", "Switch to previous clip.");
        private readonly GUIContent nextGUI = new GUIContent("NEXT", "Switch to next clip.");

        private readonly GUIContent eventCountGUI = new GUIContent("Event Count:", "How many event this clip countains.");
        private readonly GUIContent setEventGUI = new GUIContent("Set Event", "Set this animation event function.");

        private readonly GUIContent copyGUI = new GUIContent("Copy", "Copy this event method.");
        private readonly GUIContent pasteGUI = new GUIContent("Paste", "Paste the last copied event method.");

        private readonly GUIContent missingEventGUI = new GUIContent();
        private readonly GUIContent validEventGUI = new GUIContent();

        private readonly Color switchClipColor = SuperColor.Cyan.Get();
        private readonly Color separatorColor = SuperColor.Grey.Get();

        [SerializeField] private CorruptedAnimator animator = new CorruptedAnimator();

        [SerializeField] private bool autoSelectClip = false;
        [SerializeField] private int selectedClip = 0;
        [SerializeField] private int selectedEvent = -1;

        private EventMethodWrapper[] methods = new EventMethodWrapper[] { };
        private AnimationEvent[] events = new AnimationEvent[] { };
        private AnimationClip[] clips = new AnimationClip[] { };
        private GUIContent[] clipsGUI = new GUIContent[] { };

        private string eventMethodBuffer = string.Empty;

        // -----------------------

        private void InitializeAnimatorTracker()
        {
            missingEventGUI.image = EditorGUIUtility.FindTexture("console.erroricon@2x");
            validEventGUI.image = EditorGUIUtility.FindTexture("d_FilterSelectedOnly");

            selectedClip = SessionState.GetInt(SelectedClipKey, 0);
            RefreshAnimatorInfos(selectedClip);
        }

        private void DrawAnimatorTracker()
        {
            bool _drawEvents;

            // Left some space on each window sides.
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);

                using (var _verticalScope = new GUILayout.VerticalScope(GUILayout.Width(position.width - 15f)))
                {
                    _drawEvents = DrawAnimatorOverview();
                }

                GUILayout.Space(5f);
            }

            if (_drawEvents)
            {
                GUILayout.Space(5f);
                DrawAnimatorClipEvents();
            }
        }

        private bool DrawAnimatorOverview()
        {
            // Header.
            GUILayout.Space(5f);
            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI, EditorStyles.boldLabel);
            GUILayout.Space(5f);

            // Animator reference.
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                Animator _animator = EnhancedEditorGUILayout.PickerField(animator.Animator, typeof(Animator), typeof(Animator)) as Animator;

                if (_changeCheck.changed && (_animator != animator.Animator))
                {
                    animator.Animator = _animator;
                    RefreshAnimatorInfos();
                }
            }

            autoSelectClip = EditorGUILayout.Toggle(autoSelectGUI, autoSelectClip);

            // No animator, nothing to see.
            if (!animator.Animator)
            {
                GUILayout.Space(5f);
                EditorGUILayout.HelpBox(NoAnimatorMessage, UnityEditor.MessageType.Info);
                return false;
            }

            // Animator name.
            EnhancedEditorGUILayout.Section(animatorGUI);

            // No clip, nothing to see.
            if (clips.Length == 0)
            {
                EditorGUILayout.HelpBox(NoClipMessage, UnityEditor.MessageType.Info);
                return false;
            }

            // Informations about corrupted clips.
            if (animator.CorruptedClips.Length > 0)
            {
                DrawCorruptedClipInfos(animator);
                GUILayout.Space(5f);
            }

            // Clip selection.
            Rect _position = EditorGUILayout.GetControlRect();
            _position.xMax -= SelectButtonWidth + 10f;

            int _selectedClip = EditorGUI.Popup(_position, clipGUI, selectedClip, clipsGUI);
            if (_selectedClip != selectedClip)
            {
                SetClip(_selectedClip);
            }

            // Animation window clip selection.
            _position.Set(_position.xMax + 10f, _position.y - 2f, SelectButtonWidth, _position.height + 4f);

            using (var _scope = EnhancedGUI.GUIColor.Scope(buttonColor))
            {
                if (GUI.Button(_position, selectGUI))
                {
                    SelectAnimationClip();
                }
            }

            // Quick switches.
            GUILayout.Space(5f);
            using (var _scope = EnhancedGUI.GUIColor.Scope(switchClipColor))
            {
                _position = EditorGUILayout.GetControlRect(true, 25f);
                _position.xMin = _position.xMax - ((SwitchButtonWidth * 2f) + 5f);
                _position.width = SwitchButtonWidth;

                // Previous.
                using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(selectedClip > 0))
                {
                    if (GUI.Button(_position, previousGUI))
                    {
                        SetClip(selectedClip - 1);
                    }
                }

                _position.x += SwitchButtonWidth + 5f;

                // Next.
                using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(selectedClip < (clips.Length - 1)))
                {
                    if (GUI.Button(_position, nextGUI))
                    {
                        SetClip(selectedClip + 1);
                    }
                }
            }

            // Event header (drawn from here to use spacing on both window sides).
            GUILayout.Space(10f);

            EnhancedEditorGUILayout.HorizontalLine(separatorColor, GUILayout.Width(position.width * .5f), GUILayout.Height(2f));
            GUILayout.Space(10f);

            _position = EditorGUILayout.GetControlRect();
            EditorGUI.PrefixLabel(_position, eventCountGUI, EditorStyles.boldLabel);

            _position.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.SelectableLabel(_position, events.Length.ToString(), EditorStyles.numberField);

            return true;
        }

        private void DrawAnimatorClipEvents()
        {
            // Draw each events on an indented level.
            using (var _scope = new EditorGUI.IndentLevelScope())
            {
                for (int _i = 0; _i < events.Length; _i++)
                {
                    Rect _position = new Rect(EditorGUILayout.GetControlRect(false, EventHeight))
                    {
                        x = 0f,
                        width = position.width
                    };

                    // Background color.
                    bool _isSelected = selectedEvent == _i;
                    EnhancedEditorGUI.BackgroundLine(_position, _isSelected, _i);

                    _position.y += 2f;
                    _position.height -= 2f;

                    // Draw event.
                    AnimationEvent _animationEvent = events[_i];
                    DrawEventEditor(_position, _animationEvent);

                    // Select event on click.
                    if (EnhancedEditorGUIUtility.MouseDown(_position))
                    {
                        selectedEvent = _i;
                    }
                }

                // Unselect on empty space click.
                Event _event = Event.current;
                if (_event.type == EventType.MouseDown)
                {
                    selectedEvent = -1;
                    _event.Use();
                }
            }
        }

        private void DrawEventEditor(Rect _position, AnimationEvent _animationEvent)
        {
            EventMethodWrapper _wrapper = Array.Find(methods, (e) => e.MethodInfo.Name == _animationEvent.functionName);
            GUIStyle _style = EnhancedEditorStyles.BoldRichText;

            Rect _temp = new Rect(_position)
            {
                xMax = _position.xMax - 100f,
                height = 28f
            };

            bool _isCorrupted = _wrapper == null;
            if (_isCorrupted)
            {
                // Missing event informations.
                EditorGUI.LabelField(_temp, missingEventGUI);

                _temp.x += 35f;
                _temp.width = _position.width - 150f;
                _temp.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(_temp, $"<color=brown>Unassiged Event, missing method:   </color>" +
                                            $"<color=purple>{_animationEvent.functionName} ( )</color>",
                                            _style);
            }
            else
            {
                // Valid event display.
                using (var _scope = EnhancedGUI.GUIColor.Scope(Color.green))
                {
                    EditorGUI.LabelField(_temp, validEventGUI);
                }

                _temp.x += 35f;
                _temp.width = _position.width - 150f;
                _temp.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(_temp, _wrapper.MethodLabel, _style);
                if (_wrapper.Type != EventMethodParameterType.None)
                {
                    EditorGUI.LabelField(_temp, _wrapper.MethodLabel, _style);

                    float _width = _style.CalcSize(_wrapper.MethodLabel).x;
                    _temp.xMin += _width;
                    _temp.xMax -= 15f;

                    using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        // Event parameter field.
                        switch (_wrapper.Type)
                        {
                            case EventMethodParameterType.Int:
                                _animationEvent.intParameter = EditorGUI.DelayedIntField(_temp, _animationEvent.intParameter);
                                break;

                            case EventMethodParameterType.Enum:
                                _animationEvent.intParameter = EditorGUI.Popup(_temp, _animationEvent.intParameter, _wrapper.EnumNames);
                                break;

                            case EventMethodParameterType.Float:
                                _animationEvent.floatParameter = EditorGUI.DelayedFloatField(_temp, _animationEvent.floatParameter);
                                break;

                            case EventMethodParameterType.String:
                                _animationEvent.stringParameter = EditorGUI.DelayedTextField(_temp, _animationEvent.stringParameter);
                                break;

                            case EventMethodParameterType.Object:
                                _animationEvent.objectReferenceParameter = EditorGUI.ObjectField(_temp, _animationEvent.objectReferenceParameter, _wrapper.ObjectType, false);
                                break;
                        }

                        // Save on any change.
                        if (_changeCheck.changed)
                        {
                            SaveEvents();
                        }
                    }

                    _temp.xMin = _temp.xMax - 10f;
                    _temp.xMax += 15f;

                    EditorGUI.LabelField(_temp, "<color=green>)</color>", _style);
                }
            }

            // Event time on clip infos.
            using (EnhancedGUI.GUIStyleAlignment.Scope(_style, TextAnchor.MiddleRight))
            {
                _temp.Set(_position.x, _temp.yMax, _position.width - 120f, 20f);
                EditorGUI.LabelField(_temp, $"{((_animationEvent.time / clips[selectedClip].length) * 100):0.##} % " +
                                            $"<color=teal>[{_animationEvent.time:0.##} / {clips[selectedClip].length:0.##} seconds]</color>",
                                            _style);
            }

            // Copy / paste event method on context click.
            if (EnhancedEditorGUIUtility.ContextClick(_position))
            {
                ShowContextMenu(_animationEvent, !_isCorrupted);
            }

            // Set event button.
            using (var _scope = EnhancedGUI.GUIColor.Scope(buttonColor))
            {
                _temp.Set(_position.xMax - (SetEventButtonWidth + 10f), _position.y + 3f, SetEventButtonWidth, _position.height - 8f);

                if (GUI.Button(_temp, setEventGUI))
                {
                    GenericMenu _menu = new GenericMenu();
                    foreach (EventMethodWrapper _method in methods)
                    {
                        bool _isOn = _method.MethodInfo.Name == _animationEvent.functionName;
                        _menu.AddItem(new GUIContent(_method.Label), _isOn, () =>
                        {
                            SetEvent(_animationEvent, _method.MethodInfo.Name);
                        });
                    }

                    _menu.DropDown(_temp);
                }
            }
        }

        private void ShowContextMenu(AnimationEvent _event, bool _canCopy)
        {
            // Copy option.
            GenericMenu _menu = new GenericMenu();
            if (_canCopy)
            {
                _menu.AddItem(copyGUI, false, CopyToBuffer);
            }
            else
            {
                _menu.AddDisabledItem(copyGUI, false);
            }

            // Paste option.
            if (!string.IsNullOrEmpty(eventMethodBuffer))
            {
                _menu.AddItem(pasteGUI, false, PasteFromBuffer);
            }
            else
            {
                _menu.AddDisabledItem(pasteGUI, false);
            }

            _menu.ShowAsContext();

            // ----- Local Methods ----- \\

            void CopyToBuffer()
            {
                eventMethodBuffer = _event.functionName;
            }

            void PasteFromBuffer()
            {
                SetEvent(_event, eventMethodBuffer);
            }
        }
        #endregion

        #region Corrupted Event Tracker
        private const float TrackerButtonWidth = 175f;
        private const float TrackerButtonHeight = 25f;

        private const string ProgressBarTitle = "Tracking corrupted animation events";
        private const string ProgressBarInfos = "Tracking assets with animators containing corrupted clips," +
                                                "with non assigned / obsolete animation events.\n" +
                                                "This can take up to a few minutes...";

        public const string NoCorruptedEventMessage = "No corrupted event could be found. Everything is perfect!";

        private readonly GUIContent sceneTrackerGUI = new GUIContent("Track in Open Scene(s)", "Track down all corrupted animator clip events in the open scene(s).");
        private readonly GUIContent databaseTrackerGUI = new GUIContent("Track in Asset Database", "Track down all corrupted animator clip events in the asset database.");
        private readonly GUIContent selectGUI = new GUIContent("Select", "Select this object.");

        private readonly Color trackButtonColor = SuperColor.Crimson.Get();

        [SerializeField] private List<CorruptedAnimator> corruptedSceneAnimators = new List<CorruptedAnimator>();
        [SerializeField] private List<CorruptedAnimator> corruptedDatabaseAnimators = new List<CorruptedAnimator>();

        [SerializeField] private bool hasCheckedSceneAnimators = false;
        [SerializeField] private bool hasCheckedDatabaseAnimators = false;

        // -----------------------

        private void InitializeCorruptedEventTracker()
        {
            corruptedSceneAnimators.Clear();
            corruptedDatabaseAnimators.Clear();

            hasCheckedSceneAnimators = false;
            hasCheckedDatabaseAnimators = false;
        }

        private void DrawSceneTracker()
        {
            DrawTracker(sceneTrackerGUI, corruptedSceneAnimators, ref hasCheckedSceneAnimators, GetSceneAnimators);

            // ----- Local Method ----- \\

            Animator[] GetSceneAnimators()
            {
                Animator[] _animators = FindObjectsOfType<Animator>();
                return _animators;
            }
        }

        private void DrawDatabaseTracker()
        {
            DrawTracker(databaseTrackerGUI, corruptedDatabaseAnimators, ref hasCheckedDatabaseAnimators, GetDatabaseAnimators);

            // ----- Local Method ----- \\

            Animator[] GetDatabaseAnimators()
            {
                GameObject[] _objects = EnhancedEditorUtility.LoadAssets<GameObject>();
                List<Animator> _animators = new List<Animator>();

                foreach (GameObject _object in _objects)
                {
                    _animators.AddRange(_object.GetComponentsInChildren<Animator>());
                }

                return _animators.ToArray();
            }
        }

        private void DrawTracker(GUIContent _label, List<CorruptedAnimator> _corruptedAnimators, ref bool _hasTrackedAnimators, Func<Animator[]> _animatorSeeker)
        {
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (var _colorScope = EnhancedGUI.GUIColor.Scope(trackButtonColor))
                {
                    // Find corrupted animators.
                    if (GUILayout.Button(_label, GUILayout.Width(TrackerButtonWidth), GUILayout.Height(TrackerButtonHeight)))
                    {
                        EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarInfos, 1f);

                        Animator[] _animators = _animatorSeeker();
                        _corruptedAnimators.Clear();

                        foreach (Animator _animator in _animators)
                        {
                            if (AnalyzeAnimator(_animator, out var _corruptedClips))
                            {
                                CorruptedAnimator _corruptedAnimator = new CorruptedAnimator(_animator, _corruptedClips);
                                _corruptedAnimators.Add(_corruptedAnimator);
                            }
                        }

                        EditorUtility.ClearProgressBar();
                        _hasTrackedAnimators = true;
                    }
                }

                GUILayout.FlexibleSpace();
            }

            // No corrupted event, nothing to see.
            if (_hasTrackedAnimators && (_corruptedAnimators.Count == 0))
            {
                EditorGUILayout.HelpBox(NoCorruptedEventMessage, UnityEditor.MessageType.Info);
                return;
            }

            // Draw informations about each corrupted animator.
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);

                using (var _verticalScope = new GUILayout.VerticalScope(GUILayout.Width(position.width - 15f)))
                {
                    GUILayout.Space(5f);

                    for (int _i = 0; _i < _corruptedAnimators.Count; _i++)
                    {
                        GUILayout.Space(5f);

                        CorruptedAnimator _corruptedAnimator = _corruptedAnimators[_i];
                        Animator _animator = _corruptedAnimator.Animator;

                        Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 3f);
                        EnhancedEditorGUI.UnderlinedLabel(_position, EnhancedEditorGUIUtility.GetLabelGUI($"{_animator.name}:"), EditorStyles.boldLabel);

                        // Object selection.
                        using (var _colorScope = EnhancedGUI.GUIColor.Scope(buttonColor))
                        {
                            _position.xMin = _position.xMax - 75f;
                            if (GUI.Button(_position, selectGUI))
                            {
                                EditorGUIUtility.PingObject(_animator);

                                selectedTab = 0;
                                animator.Animator = _animator;

                                RefreshAnimatorInfos();
                                SetClip(0);
                            }
                        }

                        // Corrupted clip.
                        GUILayout.Space(5f);
                        DrawCorruptedClipInfos(_corruptedAnimator);
                    }
                }

                GUILayout.Space(5f);
            }
        }
        #endregion

        #region Utility
        private readonly BindingFlags getMethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // -----------------------

        private void RefreshAnimatorInfos(int _clipIndex = 0)
        {
            AnalyzeAnimator();
            clipsGUI = Array.ConvertAll(clips, (c) =>
            {
                GUIContent _content = new GUIContent(c.name.Replace('_', '/'));
                return _content;
            });

            if (_clipIndex >= clips.Length)
                _clipIndex = 0;

            SetClip(_clipIndex);
        }

        private void SetEvent(AnimationEvent _event, string _method)
        {
            _event.functionName = _method;
            SaveEvents();

            AnalyzeAnimator();
            UpdateAnimators(corruptedSceneAnimators);
            UpdateAnimators(corruptedDatabaseAnimators);

            // ----- Local Method ----- \\

            void UpdateAnimators(List<CorruptedAnimator> _animators)
            {
                int _index = _animators.FindIndex((a) => (a.Animator == animator.Animator) && (a.Animator.name == animator.Animator.name));
                if (_index > -1)
                {
                    CorruptedAnimator _animator = _animators[_index];
                    if (!AnalyzeAnimator(_animator.Animator, out _animator.CorruptedClips))
                    {
                        _animators.RemoveAt(_index);
                    }
                }
            }
        }

        private void SetClip(int _index)
        {
            SessionState.SetInt(SelectedClipKey, _index);

            selectedClip = _index;
            events = (_index < clips.Length)
                    ? clips[selectedClip].events
                    : new AnimationEvent[] { };

            if (autoSelectClip)
                SelectAnimationClip();
        }

        private void SelectAnimationClip()
        {
            if (clips.Length == 0)
                return;

            Animator _animator = animator.Animator;

            Selection.activeObject = _animator;
            Selection.activeGameObject = _animator.gameObject;
            Selection.activeTransform = _animator.transform;

            // Open the associated prefab when using an asset.
            if (PrefabUtility.IsPartOfPrefabAsset(_animator))
            {
                AssetDatabase.OpenAsset(_animator);
            }

            #if UNITY_2020_2_OR_NEWER
            AnimationWindow _window = GetWindow<AnimationWindow>(string.Empty, false);
            _window.animationClip = clips[selectedClip];
            #else
            Type _type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnimationWindow");

            var _window = GetWindow(_type, false, string.Empty);
            var _state = _type.GetProperty("state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_window);
            var _activeClip = _state.GetType().GetProperty("activeAnimationClip", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            _activeClip.SetValue(_state, clips[selectedClip]);
            #endif
        }

        private void SaveEvents()
        {
            AnimationClip _clip = clips[selectedClip];

            Undo.RecordObject(_clip, "Animation Event Change");
            AnimationUtility.SetAnimationEvents(_clip, events);
        }

        // -----------------------

        private void DrawCorruptedClipInfos(CorruptedAnimator _animator)
        {
            // Short infos box.
            EditorGUILayout.HelpBox(string.Format(MissingEventMessageFormat, _animator.CorruptedClips.Length), UnityEditor.MessageType.Error);

            Rect _position = GUILayoutUtility.GetLastRect();
            if (EnhancedEditorGUIUtility.MainMouseUp(_position))
            {
                _animator.Foldout = !_animator.Foldout;
            }

            // Draw detailed informations on corrupted clips when unfolded.
            if (_animator.Foldout)
            {
                using (var _scope = new EditorGUI.IndentLevelScope())
                {
                    string _missingFormat = "<color=brown>{0}</color>";
                    string _label = string.Format(_missingFormat, _animator.CorruptedClips[0]);

                    for (int _i = 1; _i < _animator.CorruptedClips.Length; _i++)
                    {
                        _label += string.Format($" ; {_missingFormat}", _animator.CorruptedClips[_i]);
                    }

                    EditorGUILayout.LabelField(EnhancedEditorGUIUtility.GetLabelGUI(_label), EnhancedEditorStyles.BoldWordWrappedRichText);
                }
            }
        }

        private void AnalyzeAnimator()
        {
            var _methods = AnalyzeAnimator(animator.Animator, out animator.CorruptedClips, out clips, true);
            methods = _methods.ToArray();

            Array.Sort(methods, (a, b) => a.Label.text.CompareTo(b.Label.text));
        }

        private bool AnalyzeAnimator(Animator _animator, out string[] _corruptedClips)
        {
            AnalyzeAnimator(_animator, out _corruptedClips, out _);
            return _corruptedClips.Length > 0;
        }

        private List<EventMethodWrapper> AnalyzeAnimator(Animator _animator, out string[] _corruptedClips, out AnimationClip[] _clips, bool _getWrappers = false)
        {
            if (!_animator)
            {
                _corruptedClips = new string[] { };
                _clips = new AnimationClip[] { };

                return new List<EventMethodWrapper>();
            }

            // Get all animation clips without duplicate.
            animatorGUI.text = _animator.name;

            _clips = (_animator.runtimeAnimatorController != null)
                    ? _animator.runtimeAnimatorController.animationClips
                    : new AnimationClip[] { };

            for (int _i = _clips.Length - 1; _i > -1; _i--)
            {
                for (int _j = 0; _j < _i; _j++)
                {
                    if (_clips[_j].name == _clips[_i].name)
                    {
                        ArrayUtility.RemoveAt(ref _clips, _i);
                    }
                }
            }

            Array.Sort(_clips, (a, b) => a.name.CompareTo(b.name));

            // Get all methods from this animator GameObject components.
            List<MethodInfo> _methods = new List<MethodInfo>();
            List<EventMethodWrapper> _wrappers = new List<EventMethodWrapper>();

            Component[] _components = _animator.GetComponents<Component>();

            foreach (Component _component in _components)
            {
                Type _type;
                try
                {
                    _type = _component.GetType();
                }
                catch (NullReferenceException)
                {
                    continue;
                }

                MethodInfo[] _methodInfos = _type.GetMethods(getMethodBindingFlags);
                foreach (MethodInfo _methodInfo in _methodInfos)
                {
                    // Only get non-internal methods from custom scripts, and public void ones among other components.
                    if ((_methodInfo.DeclaringType.IsSubclassOf(typeof(MonoBehaviour)) && !_methodInfo.IsAssembly)
                        || (_methodInfo.IsPublic && !_methodInfo.IsSpecialName && (_methodInfo.ReturnType == typeof(void)) && !_type.IsSubclassOf(typeof(MonoBehaviour))))
                    {
                        ParameterInfo[] _parameters = _methodInfo.GetParameters();

                        // Discard overloads and methods with more than one argument.
                        if ((_parameters.Length > 1) || Array.Exists(_methodInfos, (m) => (m != _methodInfo) && (m.Name == _methodInfo.Name) && (m.GetParameters().Length < 2)))
                        {
                            continue;
                        }

                        if (_parameters.Length == 0)
                        {
                            // Non parameter method.
                            _methods.Add(_methodInfo);

                            if (_getWrappers)
                            {
                                string _label = $"{_type.Name}/{_methodInfo.Name} ( )";

                                EventMethodWrapper _method = new EventMethodWrapper(_methodInfo, _label, EventMethodParameterType.None);
                                _wrappers.Add(_method);
                            }
                        }
                        else
                        {
                            // One parameter method.
                            Type _parameterType = _parameters[0].ParameterType;
                            EventMethodParameterType _eventType = GetEventType(_parameterType);

                            if (_eventType > 0)
                            {
                                _methods.Add(_methodInfo);

                                if (_getWrappers)
                                {
                                    string _label = $"{_type.Name}/{_methodInfo.Name} ({_parameterType.Name})";

                                    EventMethodWrapper _method = new EventMethodWrapper(_methodInfo, _label, _eventType);
                                    _wrappers.Add(_method);
                                }
                            }
                        }
                    }
                }
            }

            // Get corrupted clips.
            List<string> _corruptedClipsTemp = new List<string>();
            foreach (AnimationClip _clip in _clips)
            {
                foreach (AnimationEvent _event in _clip.events)
                {
                    if (!_methods.Exists((m) => m.Name == _event.functionName))
                    {
                        _corruptedClipsTemp.Add(_clip.name);
                        break;
                    }
                }
            }

            _corruptedClips = _corruptedClipsTemp.ToArray();
            return _wrappers;
        }

        private EventMethodParameterType GetEventType(Type _parameterType)
        {
            if (_parameterType == typeof(int))
            {
                return EventMethodParameterType.Int;
            }
            else if (_parameterType.IsSubclassOf(typeof(Enum)))
            {
                return EventMethodParameterType.Enum;
            }
            else if (_parameterType == typeof(float))
            {
                return EventMethodParameterType.Float;
            }
            else if (_parameterType == typeof(string))
            {
                return EventMethodParameterType.String;
            }
            else if (_parameterType.IsSubclassOf(typeof(Object)))
            {
                return EventMethodParameterType.Object;
            }

            return 0;
        }
        #endregion
    }
}
