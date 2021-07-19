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

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Window used to track down all project animation events,
    /// and find corrupted ones referencing obsolete methods.
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
            public MethodInfo MethodInfo;
            public EventMethodParameterType Type;

            public GUIContent Label;
            public GUIContent MethodLabel;

            // -----------------------

            public EventMethodWrapper(MethodInfo _method, string _label, EventMethodParameterType _type)
            {
                MethodInfo = _method;
                Label = new GUIContent(_label);
                Type = _type;

                MethodLabel = new GUIContent($"<color=#009900ff>{_label.Replace('/', '.')}</color>");
                if (_type != EventMethodParameterType.None)
                {
                    MethodLabel.text = MethodLabel.text.Replace("(", "(</color><color=olive>").Replace(")", ":   ");
                }
            }
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="AnimationEventTrackerWindow"/> currently on screen.
        /// <para/>
        /// Creates and shows a new instance if there is none.
        /// </summary>
        [MenuItem("Enhanced Editor/Animation Event Tracker", false, 101)]
        public static AnimationEventTrackerWindow GetWindow()
        {
            AnimationEventTrackerWindow _window = GetWindow<AnimationEventTrackerWindow>("Animation Event Tracker");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private readonly GUIContent refreshGUI = new GUIContent("Refresh", "Refresh selected animator clips and events.");
        private readonly GUIContent[] tabs = new GUIContent[]
                                                    {
                                                        new GUIContent("Event Tracker", "Track all animation events within an animator."),
                                                        new GUIContent("Scene(s) Corrupted Events", "Track down all corrupted events from open scene(s) animators."),
                                                        new GUIContent("Asset Corrupted Events", "Track down all corrupted events from asset database.")
                                                    };

        private readonly Color buttonColor = SuperColor.Green.Get();

        private Vector2 scroll = new Vector2();
        private int selectedTab = 0;

        // -----------------------

        private void OnEnable()
        {
            InitializeEventEditor();
            InitializeTracker();
        }

        private void OnGUI()
        {
            // Toolbar.
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(refreshGUI, EditorStyles.toolbarButton, GUILayout.Width(60f)))
            {
                RefreshAnimatorInfos(selectedClip);
            }
            
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Tab selection.
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Use minimum width value to avoid undesired shift on small window size.
            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(25f), GUILayout.MinWidth(50f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            
            switch (selectedTab)
            {
                case 0:
                    DrawEventTracker();
                    break;

                case 1:
                    DrawSceneTracker();
                    break;

                case 2:
                    DrawDatabaseTracker();
                    break;
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Event Tracker
        // -------------------------------------------
        // Event Tracker
        // -------------------------------------------

        private const string SelectedClipKey = "SelectedClip_Key";

        private const string NoAnimatorMessage = "Please select an animator to edit.";
        private const string NoClipMessage = "Selected animator doesn't have any clip.";

        private readonly GUIContent headerGUI = new GUIContent("Animator:", "Assign an animator to track.");
        private readonly GUIContent clipGUI = new GUIContent("Animation Clip", "Animation to track events.");
        private readonly GUIContent autoSelectGUI = new GUIContent("Clip auto-selection", "When enabled, selected clip is also automatically selected in animation window.");

        private readonly GUIContent previousGUI = new GUIContent("PREVIOUS", "Switch to previous clip.");
        private readonly GUIContent nextGUI = new GUIContent("NEXT", "Switch to next clip.");

        private readonly GUIContent eventCountGUI = new GUIContent("Event Count:", "How many event this clip countains.");
        private readonly GUIContent editEventGUI = new GUIContent("Edit Event", "Edit this animation event value.");

        private readonly GUIContent copyGUI = new GUIContent("Copy", "Copy this event method.");
        private readonly GUIContent pasteGUI = new GUIContent("Paste", "Paste last copied event method.");

        private readonly Color oddColor = EnhancedEditorGUIUtility.GUIOddColor;
        private readonly Color selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
        private readonly Color switchClipColor = SuperColor.Aquamarine.Get();

        private GUIContent missingEventGUI = null;
        private GUIContent validEventGUI = null;

        private Animator animator = null;
        private EventMethodWrapper[] methods = new EventMethodWrapper[] { };

        private AnimationClip[] clips = new AnimationClip[] { };
        private GUIContent[] clipNames = new GUIContent[] { };
        private int selectedClip = 0;
        private bool autoSelectAnimation = false;

        private AnimationEvent[] events = new AnimationEvent[] { };
        private int selectedEvent = -1;
        
        private string[] corruptedClips = new string[] { };
        private bool areCorruptedClipsUnfolded = false;

        private string eventMethodBuffer = string.Empty;

        // -----------------------

        private void InitializeEventEditor()
        {
            selectedClip = SessionState.GetInt(SelectedClipKey, 0);

            missingEventGUI = EditorGUIUtility.IconContent("d_console.erroricon@2x");
            validEventGUI = EditorGUIUtility.IconContent("d_FilterSelectedOnly");

            RefreshAnimatorInfos(selectedClip);
        }

        private void DrawEventTracker()
        {
            // Left some space on each window sides.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5f);

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 10f));
            EditorGUILayout.Space(5f);

            bool _doDrawEvents = DrawAnimatorOverview();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();

            if (_doDrawEvents)
            {
                EditorGUILayout.Space(5f);
                DrawAnimatorClipEvents();
            }
        }

        private bool DrawAnimatorOverview()
        {
            EnhancedEditorGUILayout.UnderlinedLabel(headerGUI, EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);

            // Animator reference.
            EditorGUI.BeginChangeCheck();
            Animator _animator = EditorGUILayout.ObjectField(animator, typeof(Animator), true) as Animator;

            if (EditorGUI.EndChangeCheck())
            {
                animator = _animator;

                RefreshAnimatorInfos();
                SetClip(0);
            }

            autoSelectAnimation = EditorGUILayout.Toggle(autoSelectGUI, autoSelectAnimation);

            // No animator, nothing to see.
            if (animator == null)
            {
                EditorGUILayout.Space(5f);
                EditorGUILayout.HelpBox(NoAnimatorMessage, UnityEditor.MessageType.Info);
                return false;
            }

            // Animator name.
            EnhancedEditorGUILayout.Section(new GUIContent(animator.name), SectionAttribute.DefaultLineWidth);

            if (clips.Length == 0)
            {
                EditorGUILayout.HelpBox(NoClipMessage, UnityEditor.MessageType.Info);
                return false;
            }

            // Informations on corrupted clips.
            if (corruptedClips.Length > 0)
            {
                areCorruptedClipsUnfolded = DrawCorruptedClipInfos(corruptedClips, areCorruptedClipsUnfolded);
                EditorGUILayout.Space(5f);
            }

            // Clip selection.
            Rect _position = EditorGUILayout.GetControlRect();
            _position.xMax -= 70f;

            int _selectedClip = EditorGUI.Popup(_position, clipGUI, selectedClip, clipNames);
            if (_selectedClip != selectedClip)
            {
                SetClip(_selectedClip);
            }

            // Animation window clip selection.
            _position.Set(_position.xMax + 10f, _position.y - 2f, 60f, _position.height + 4f);

            EnhancedEditorGUIUtility.PushGUIColor(buttonColor);
            if (GUI.Button(_position, selectGUI))
            {
                SelectAnimationClip();
            }

            EnhancedEditorGUIUtility.PopGUIColor();

            // Quick switchs.
            EditorGUILayout.Space(5f);
            EnhancedEditorGUIUtility.PushGUIColor(switchClipColor);

            _position = EditorGUILayout.GetControlRect(true, 25f);
            _position.xMin = _position.xMax - 155f;
            _position.width = 75f;

            if (GUI.Button(_position, previousGUI))
            {
                SetClip(Mathf.Max(0, selectedClip - 1));
            }

            _position.x += 80f;
            if (GUI.Button(_position, nextGUI))
            {
                SetClip(Mathf.Min(clips.Length - 1, selectedClip + 1));
            }

            EnhancedEditorGUIUtility.PopGUIColor();

            // Event header (drawn from here to use spacing on both window sides).
            EditorGUILayout.Space(10f);

            _position = EditorGUILayout.GetControlRect(false, 2f);
            _position.xMax *= .5f;

            EnhancedEditorGUI.HorizontalLine(_position, 1f, SuperColor.Grey.Get());
            EditorGUILayout.Space(10f);

            _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(_position, eventCountGUI, EditorStyles.boldLabel);

            _position.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.IntField(_position, events.Length);

            return true;
        }

        private void DrawAnimatorClipEvents()
        {
            EditorGUI.indentLevel++;
            Event _current;

            for (int _i = 0; _i < events.Length; _i++)
            {
                AnimationEvent _event = events[_i];

                Rect _position = EditorGUILayout.GetControlRect(false, 42f);
                _position.x -= 2f;
                _position.width += 4f;

                // Background color.
                if (selectedEvent == _i)
                {
                    EditorGUI.DrawRect(_position, selectedColor);
                }
                else if (_i % 2 == 0)
                {
                    EditorGUI.DrawRect(_position, oddColor);
                }

                _position.y += 2f;
                _position.height -= 2f;
                
                // Draw event.
                DrawEventEditor(_position, _event);

                // Select event on click.
                _current = Event.current;
                if ((_current.type == EventType.MouseDown) && _position.Contains(_current.mousePosition))
                {
                    selectedEvent = _i;
                    _current.Use();
                }
            }

            // Deselect on empty space click.
            _current = Event.current;
            if (_current.type == EventType.MouseDown)
            {
                selectedEvent = -1;
                _current.Use();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawEventEditor(Rect _position, AnimationEvent _event)
        {
            EventMethodWrapper _wrapper = Array.Find(methods, (e) => e.MethodInfo.Name == _event.functionName);
            EditorGUI.BeginChangeCheck();

            Rect _rect = _position;
            _rect.xMax -= 100f;
            _rect.height = 28f;

            if (_wrapper == null)
            {
                // Missing event informations.
                EditorGUI.LabelField(_rect, missingEventGUI);

                _rect.x += 35f;
                _rect.width = _position.width - 150f;
                _rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(_rect, $"<color=red>Unassiged Event, missing method:   </color>" +
                                            $"<color=orange>{_event.functionName} ( )</color>",
                                            EnhancedEditorStyles.BoldRichText);
            }
            else
            {
                // Valid event display.
                EnhancedEditorGUIUtility.PushGUIColor(Color.green);
                EditorGUI.LabelField(_rect, validEventGUI);
                EnhancedEditorGUIUtility.PopGUIColor();

                GUIStyle _style = EnhancedEditorStyles.BoldRichText;

                _rect.x += 35f;
                _rect.width = _position.width - 150f;
                _rect.height = EditorGUIUtility.singleLineHeight;

                if (_wrapper.Type == EventMethodParameterType.None)
                {
                    EditorGUI.LabelField(_rect, _wrapper.MethodLabel, _style);
                }
                else
                {
                    EditorGUI.LabelField(_rect, _wrapper.MethodLabel, _style);

                    float _width = _style.CalcSize(_wrapper.MethodLabel).x;
                    _rect.xMin += _width;
                    _rect.xMax -= 15f;

                    // Event parameter field.
                    switch (_wrapper.Type)
                    {
                        case EventMethodParameterType.Int:
                            _event.intParameter = EditorGUI.DelayedIntField(_rect, _event.intParameter);
                            break;

                        case EventMethodParameterType.Enum:
                            Type _type = _wrapper.MethodInfo.GetParameters()[0].ParameterType;
                            _event.intParameter = EditorGUI.Popup(_rect, _event.intParameter, _type.GetEnumNames());
                            break;

                        case EventMethodParameterType.Float:
                            _event.floatParameter = EditorGUI.DelayedFloatField(_rect, _event.floatParameter);
                            break;

                        case EventMethodParameterType.String:
                            _event.stringParameter = EditorGUI.DelayedTextField(_rect, _event.stringParameter);
                            break;

                        case EventMethodParameterType.Object:
                            _type = _wrapper.MethodInfo.GetParameters()[0].ParameterType;
                            _event.objectReferenceParameter = EditorGUI.ObjectField(_rect, _event.objectReferenceParameter, _type, true);
                            break;
                    }

                    _rect.xMin = _rect.xMax - 10f;
                    _rect.xMax += 15f;

                    EditorGUI.LabelField(_rect, "<color=#009900ff>)</color>", EnhancedEditorStyles.BoldRichText);
                }
            }

            // Event time on clip infos.
            EnhancedEditorGUIUtility.PushAnchor(EnhancedEditorStyles.BoldRichText, TextAnchor.MiddleRight);

            _rect.Set(_position.x, _rect.yMax, _position.width - 120f, 20f);
            EditorGUI.LabelField(_rect, $"{((_event.time / clips[selectedClip].length) * 100):0.##} % " +
                                        $"<color=teal>[{_event.time:0.##} / {clips[selectedClip].length:0.##} seconds]</color>",
                                        EnhancedEditorStyles.BoldRichText);

            EnhancedEditorGUIUtility.PopAnchor(EnhancedEditorStyles.BoldRichText);

            // Save if needed
            if (EditorGUI.EndChangeCheck())
            {
                Save();
            }

            // Copy / paste event method on context click.
            Event _current = Event.current;
            if ((_current.type == EventType.ContextClick) && _position.Contains(_current.mousePosition))
            {
                ShowContextMenu(_event, _wrapper != null);
                _current.Use();
            }

            // Edit event button.
            EnhancedEditorGUIUtility.PushGUIColor(buttonColor);
            _rect.Set(_position.xMax - 110f, _position.y + 3f, 100f, _position.height - 8f);

            if (GUI.Button(_rect, editEventGUI))
            {
                GenericMenu _menu = new GenericMenu();
                foreach (EventMethodWrapper _method in methods)
                {
                    bool _isOn = _method.MethodInfo.Name == _event.functionName;
                    _menu.AddItem(new GUIContent(_method.Label), _isOn, () =>
                    {
                        SetEvent(_event, _method.MethodInfo.Name);
                    });
                }

                _menu.DropDown(_rect);
            }

            EnhancedEditorGUIUtility.PopGUIColor();
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

            // ----- Local Methods ----- //

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

        #region Corrupted Clip Tracker
        // -------------------------------------------
        // Corrupted Clip Tracker
        // -------------------------------------------

        private const float trackerButtonWidth = 150f;
        private const float trackerButtonHeight = 25f;

        private const string ProgressBarTitle = "Tracking corrupted animation events";
        private const string ProgressBarInfos = "Tracking assets with animators containing corrupted clips," +
                                                   "with non assigned / obsolete animation events.\n" +
                                                    "This can take up to a few minutes...";

        private readonly GUIContent sceneTrackerGUI = new GUIContent("Track in Open Scene(s)", "Track down corrupted animator clip events in open scene(s).");
        private readonly GUIContent assetTrackerGUI = new GUIContent("Track in Asset Database", "Track down corrupted animator clip events in asset database.");

        private readonly GUIContent selectGUI = new GUIContent("Select", "Select this asset.");

        private readonly Color trackButtonColor = SuperColor.Crimson.Get();

        private List<Animator> _corruptedSceneAnimators = new List<Animator>();
        private List<bool> _corruptedSceneAnimatorFoldout = new List<bool>();
        private List<string[]> _corruptedSceneAnimatorEvents = new List<string[]>();

        private List<Animator> _corruptedAssetAnimators = new List<Animator>();
        private List<bool> _corruptedAssetAnimatorFoldout = new List<bool>();
        private List<string[]> _corruptedAssetAnimatorEvents = new List<string[]>();

        // -----------------------

        private void InitializeTracker()
        {
            _corruptedSceneAnimators.Clear();
            _corruptedSceneAnimatorFoldout.Clear();
            _corruptedSceneAnimatorEvents.Clear();

            _corruptedAssetAnimators.Clear();
            _corruptedAssetAnimatorFoldout.Clear();
            _corruptedAssetAnimatorEvents.Clear();
        }

        private void DrawSceneTracker()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EnhancedEditorGUIUtility.PushGUIColor(trackButtonColor);

            // Find corrupted animators in open scene(s).
            if (GUILayout.Button(sceneTrackerGUI, GUILayout.Width(trackerButtonWidth), GUILayout.Height(trackerButtonHeight)))
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarInfos, 1f);

                _corruptedSceneAnimators.Clear();
                _corruptedSceneAnimatorFoldout.Clear();
                _corruptedSceneAnimatorEvents.Clear();

                Animator[] _animators = FindObjectsOfType<Animator>();
                foreach (Animator _animator in _animators)
                {
                    if (AnalyzeAnimator(_animator, out _, out _, out var _missingEvents))
                    {
                        _corruptedSceneAnimators.Add(_animator);
                        _corruptedSceneAnimatorFoldout.Add(false);
                        _corruptedSceneAnimatorEvents.Add(_missingEvents);
                    }
                }

                EditorUtility.ClearProgressBar();
            }

            EnhancedEditorGUIUtility.PopGUIColor();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Draw corrupted assets.
            DrawTracker(_corruptedSceneAnimators, _corruptedSceneAnimatorFoldout, _corruptedSceneAnimatorEvents);
        }

        private void DrawDatabaseTracker()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EnhancedEditorGUIUtility.PushGUIColor(trackButtonColor);

            // Find corrupted animators in asset database.
            if (GUILayout.Button(assetTrackerGUI, GUILayout.Width(trackerButtonWidth), GUILayout.Height(trackerButtonHeight)))
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarInfos, 1f);

                _corruptedAssetAnimators.Clear();
                _corruptedAssetAnimatorFoldout.Clear();
                _corruptedAssetAnimatorEvents.Clear();

                GameObject[] _objects = EnhancedEditorUtility.LoadAssets<GameObject>();

                foreach (GameObject _object in _objects)
                {
                    foreach (Animator _animator in _object.GetComponentsInChildren<Animator>())
                    {
                        if (AnalyzeAnimator(_animator, out _, out _, out var _missingEvents))
                        {
                            _corruptedAssetAnimators.Add(_animator);
                            _corruptedAssetAnimatorFoldout.Add(false);
                            _corruptedAssetAnimatorEvents.Add(_missingEvents);
                        }
                    }                   
                }

                EditorUtility.ClearProgressBar();
            }

            EnhancedEditorGUIUtility.PopGUIColor();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Draw corrupted assets.
            DrawTracker(_corruptedAssetAnimators, _corruptedAssetAnimatorFoldout, _corruptedAssetAnimatorEvents);
        }

        private void DrawTracker(List<Animator> _animators, List<bool> _foldouts, List<string[]> _corruptedEvents)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 10f));

            EditorGUILayout.Space(5f);

            // Informations about each corrupted animator.
            for (int _i = 0; _i < _animators.Count; _i++)
            {
                Animator _animator = _animators[_i];

                EditorGUILayout.Space(5f);

                Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 3f);
                EnhancedEditorGUI.UnderlinedLabel(_position, new GUIContent($"{_animator.name}:"), EditorStyles.boldLabel);

                // Object selection.
                EnhancedEditorGUIUtility.PushGUIColor(buttonColor);
                _position.xMin = _position.xMax - 75f;
                
                if (GUI.Button(_position, selectGUI))
                {
                    Selection.activeObject = _animator;
                    EditorGUIUtility.PingObject(_animator);
                }

                EnhancedEditorGUIUtility.PopGUIColor();

                // Corrupted clip.
                _foldouts[_i] = DrawCorruptedClipInfos(_corruptedEvents[_i], _foldouts[_i]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Utility
        private void RefreshAnimatorInfos(int _clipIndex = 0)
        {
            AnalyzeAnimator(animator, out clips, out methods, out corruptedClips);
            clipNames = Array.ConvertAll(clips, (c) => new GUIContent(c.name.Replace('_', '/')));

            SetClip((_clipIndex < clips.Length) ? _clipIndex : 0);
        }

        private void SetEvent(AnimationEvent _event, string _method)
        {
            _event.functionName = _method;
            Save();
        }

        private void SetClip(int _index)
        {
            if (_index == selectedClip)
                return;

            SessionState.SetInt(SelectedClipKey, _index);

            selectedClip = _index;
            events = (_index < clips.Length) ? clips[selectedClip].events : new AnimationEvent[] { };

            if (autoSelectAnimation)
                SelectAnimationClip();
        }

        private void SelectAnimationClip()
        {
            if (clips.Length == 0)
                return;

            AnimationWindow _window = GetWindow<AnimationWindow>();
            Selection.activeObject = animator;

            _window.animationClip = clips[selectedClip];
        }

        private void Save()
        {
            AnimationClip _clip = clips[selectedClip];

            Undo.RecordObject(_clip, "change animation event");
            AnimationUtility.SetAnimationEvents(_clip, events);

            RefreshAnimatorInfos(selectedClip);
        }

        // -----------------------

        private bool DrawCorruptedClipInfos(string[] _corruptedClips, bool _foldout)
        {
            // Short infos box.
            Rect _position = EditorGUILayout.GetControlRect(true, EnhancedEditorGUIUtility.DefaultHelpBoxHeight);
            if (GUI.Button(_position, GUIContent.none, EditorStyles.label))
                _foldout = !_foldout;

            EditorGUI.HelpBox(_position, $"Missing events found on {_corruptedClips.Length} clips!", UnityEditor.MessageType.Error);

            // Draw detailed informations on corrupted clips when unfolded.
            if (_foldout)
            {
                EditorGUI.indentLevel++;
                string _missingFormat = "<color=red>{0}</color>";

                string _label = string.Format(_missingFormat, _corruptedClips[0]);
                for (int _i = 1; _i < _corruptedClips.Length; _i++)
                {
                    _label += string.Format($" ; {_missingFormat}", _corruptedClips[_i]);
                }

                EditorGUILayout.LabelField(_label, EnhancedEditorStyles.WordWrappedRichText);
                EditorGUI.indentLevel--;
            }

            return _foldout;
        }

        private bool AnalyzeAnimator(Animator _animator, out AnimationClip[] _clips, out EventMethodWrapper[] _methods, out string[] _corruptedClips)
        {
            if (_animator == null)
            {
                _clips = new AnimationClip[] { };
                _methods = new EventMethodWrapper[] { };
                _corruptedClips = new string[] { };

                return false;
            }

            // Get all animation clips without duplicates.
            _clips = _animator.runtimeAnimatorController.animationClips;
            for (int _i = _clips.Length - 1; _i > - 1; _i--)
            {
                for (int _j = 0; _j < _i; _j++)
                {
                    if (_clips[_j].name == _clips[_i].name)
                    {
                        UnityEditor.ArrayUtility.RemoveAt(ref _clips, _i);
                    }
                }
            }

            Array.Sort(_clips, (a, b) => a.name.CompareTo(b.name));

            // Get all object component methods.
            List<EventMethodWrapper> methodsTemp = new List<EventMethodWrapper>();

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

                MethodInfo[] _methodInfos = _type.GetMethods();
                foreach (MethodInfo _methodInfo in _methodInfos)
                {
                    // Only get fitting method:
                    // must be public, void, and have one parameter maximum.
                    if (_methodInfo.IsPublic && (_methodInfo.ReturnType == typeof(void)))
                    {
                        ParameterInfo[] _parameters = _methodInfo.GetParameters();
                        if (_parameters.Length == 0)
                        {
                            string _label = $"{_type.Name}/{_methodInfo.Name} ( )";

                            EventMethodWrapper _method = new EventMethodWrapper(_methodInfo, _label, EventMethodParameterType.None);
                            methodsTemp.Add(_method);

                            continue;
                        }

                        Type _parameterType = _parameters[0].ParameterType;
                        EventMethodParameterType _eventType = GetEventType(_parameterType);

                        if ((_parameters.Length == 1) && (_eventType > 0))
                        {
                            string _label = $"{_type.Name}/{_methodInfo.Name} ({_parameterType.Name})";

                            EventMethodWrapper _method = new EventMethodWrapper(_methodInfo, _label, _eventType);
                            methodsTemp.Add(_method);
                        }
                    }
                }
            }

            _methods = methodsTemp.ToArray();
            Array.Sort(_methods, (a, b) => a.Label.text.CompareTo(b.Label.text));

            // Get corrupted clips.
            List<string> _corruptedClipsTemp = new List<string>();
            foreach (AnimationClip _clip in _clips)
            {
                foreach (AnimationEvent _event in _clip.events)
                {
                    if (Array.Find(_methods, (m) => m.MethodInfo.Name == _event.functionName) == null)
                    {
                        _corruptedClipsTemp.Add(_clip.name);
                        break;
                    }
                }
            }

            _corruptedClips = _corruptedClipsTemp.ToArray();
            return _corruptedClips.Length > 0;
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
            else if (_parameterType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return EventMethodParameterType.Object;
            }

            return 0;
        }
        #endregion
    }
}
