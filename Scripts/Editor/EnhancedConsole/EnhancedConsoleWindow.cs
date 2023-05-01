// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Editor window used as an enhanced version of the native console, with various improvements and utilities.
    /// <para/>
    /// These include a preview of the user scripts from the stack, customizable filters,
    /// <br/> colors and textures, and many more.
    /// </summary>
	public class EnhancedConsoleWindow : EditorWindow, IHasCustomMenu {
        // ----- Classes & Enums ----- \\

        #region Styles
        internal static class Styles {
            public static readonly GUIStyle CountBadgeStyle = new GUIStyle("CN CountBadge");

            public static readonly GUIStyle LogColumnStyle = new GUIStyle("ControlLabel") {
                alignment   = TextAnchor.MiddleLeft,
                richText    = true,
                padding     = new RectOffset(2, 2, -1, 1),
            };

            public static readonly GUIStyle LogStyle = new GUIStyle("label") {
                alignment   = TextAnchor.MiddleLeft,
                richText    = true,
                wordWrap    = false,
                
                // Use a brighter text color on all styles to make it clearly visible with a colored background.
                normal  = new GUIStyleState() { textColor = new Color(.9f, .9f, .9f) },
                hover   = new GUIStyleState() { textColor = new Color(.9f, .9f, .9f) },
                active  = new GUIStyleState() { textColor = new Color(.9f, .9f, .9f) },
                focused = new GUIStyleState() { textColor = new Color(.9f, .9f, .9f) },
            };

            public static readonly GUIStyle LogWordWrapStyle = new GUIStyle(LogStyle) {
                alignment   = TextAnchor.UpperLeft,
                wordWrap    = true,
                padding     = new RectOffset(2, 2, 2, 2),
            };

            public static readonly GUIStyle StackHeaderStyle = new GUIStyle("label") {
                alignment   = TextAnchor.UpperLeft,
                richText    = true,
                wordWrap    = true,
                padding     = new RectOffset(2, 2, 3, 3),
                normal      = new GUIStyle("ControlLabel").normal,
            };

            public static readonly GUIStyle StackPreviewStyle = new GUIStyle("label") {
                alignment   = TextAnchor.UpperLeft,
                richText    = true,
                normal      = new GUIStyle("ControlLabel").normal,
            };

            public static readonly GUIStyle StackContentStyle = new GUIStyle("label") {
                alignment   = TextAnchor.UpperLeft,
                richText    = true,
                wordWrap    = true,
                fixedHeight = 0,
                padding     = new RectOffset(2, 2, 2, 2),
                normal      = new GUIStyle("ControlLabel").normal,
            };

            public static readonly GUIContent DropdownGUI = EditorGUIUtility.IconContent("dropdown", "Shows more options.");
            public static readonly GUIContent RefreshGUI = EditorGUIUtility.IconContent("Refresh", "Refreshes the console logs and filters.");
            public static readonly GUIContent ImportGUI = EditorGUIUtility.IconContent("FolderOpened Icon", "Import console logs from a file.");
            public static readonly GUIContent ExportGUI = EditorGUIUtility.IconContent("SaveAs", "Export this console logs into a file.");
        }
        #endregion

        #region Build Preprocessor
        /// <summary>
        /// Implements the interface on another class to avoid Unity creating a new instance of the console window.
        /// </summary>
        private class BuildPreprocessor : IPreprocessBuildWithReport {
            int IOrderedCallback.callbackOrder {
                get { return -1; }
            }

            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) {

                if (HasOpenInstances<EnhancedConsoleWindow>()) {
                    GetWindow().OnPreprocessBuild(_report);
                }
            }
        }
        #endregion

        #region Log Wrappers
        [Serializable]
        internal abstract class LogEntry {
            public abstract bool IsDuplicate { get; }

            public bool IsSelected = false;
            public bool IsVisible = true;

            public string Timestamp = string.Empty;
            public string Frame = string.Empty;

            // -----------------------

            public LogEntry() {
                IsSelected = false;
                IsVisible = true;
            }

            public LogEntry(LogWrapper _log) : this() {
                Timestamp = _log.Timestamp;
                Frame = _log.Frame;
            }

            // -----------------------

            public abstract bool AddDuplicateLog(LogWrapper _log, int _instanceID, int _index);
            public abstract void SetOriginalLog(int _index);
            public abstract OriginalLogEntry GetEntry(List<LogEntry> _entries);
            public abstract string GetTime(List<LogEntry> _entries, bool _collapse);
            public abstract void OnDeleteLog(int _index);
        }

        [Serializable]
        internal class OriginalLogEntry : LogEntry {
            public override bool IsDuplicate {
                get { return false; }
            }

            [SerializeReference] public ConsoleLogFilter Filter = null;

            public string LogString = string.Empty;
            public string StackTrace = string.Empty;
            public FlagLogType Type = FlagLogType.Log;
            public int ContextInstanceID = -1;
            public string ContextObjectName = string.Empty;

            public string File = string.Empty;
            public string Namespace = string.Empty;
            public string Class = string.Empty;
            public string Method = string.Empty;
            public int LineNumber = -1;
            public int ColumnNumber = -1;

            public int DuplicateCount = 0;
            public int LastDuplicateIndex = -1;

            public float Height = 0f;

            // -----------------------

            public OriginalLogEntry(LogWrapper _log, int _instanceID) : base(_log) {
                LogString = _log.Log;
                StackTrace = _log.StackTrace;
                Type = _log.Type.ToFlag();
                ContextInstanceID = _instanceID;

                if ((_instanceID != -1) && EnhancedUtility.FindObjectFromInstanceID(_instanceID, out Object _object)) {
                    ContextObjectName = _object.name;
                }

                LogStackUtility.FillLogEntry(this);
            }

            public OriginalLogEntry(OriginalLogEntry _original, LogEntry _duplicate) {
                IsSelected = _duplicate.IsSelected;
                IsVisible = _duplicate.IsVisible;
                Timestamp = _duplicate.Timestamp;
                Frame = _duplicate.Frame;

                Filter = _original.Filter;
                LogString = _original.LogString;
                StackTrace = _original.StackTrace;
                Type = _original.Type;
                File = _original.File;
                Namespace = _original.Namespace;
                Class = _original.Class;
                Method = _original.Method;
                LineNumber = _original.LineNumber;
                ContextInstanceID = _original.ContextInstanceID;
                ContextObjectName = _original.ContextObjectName;
                DuplicateCount = _original.DuplicateCount - 1;
                LastDuplicateIndex = (DuplicateCount != 0) ? _original.LastDuplicateIndex : -1;
                Height = _original.Height;
            }

            // -----------------------

            public override OriginalLogEntry GetEntry(List<LogEntry> _entries) {
                return this;
            }

            public override bool AddDuplicateLog(LogWrapper _log, int _instanceID, int _index) {
                if ((LogString == _log.Log) && (_log.StackTrace == StackTrace) && (ContextInstanceID == _instanceID)) {
                    DuplicateCount++;
                    LastDuplicateIndex = _index;

                    return true;
                }

                return false;
            }

            public override void SetOriginalLog(int _index) { }

            public override string GetTime(List<LogEntry> _entries, bool _collapse) {
                return (_collapse && (LastDuplicateIndex != -1) && (LastDuplicateIndex < _entries.Count)) ? _entries[LastDuplicateIndex].Timestamp : Timestamp;
            }

            public override void OnDeleteLog(int _index) {
                if (LastDuplicateIndex > _index) {
                    LastDuplicateIndex--;
                }
            }
        }

        [Serializable]
        internal class DuplicateLogEntry : LogEntry {
            public override bool IsDuplicate {
                get { return true; }
            }

            public int OriginalIndex = -1;

            // -----------------------

            public DuplicateLogEntry(LogWrapper _log, int _originalIndex) : base(_log) {
                OriginalIndex = _originalIndex;
            }

            // -----------------------

            public override OriginalLogEntry GetEntry(List<LogEntry> _entries) {
                return _entries[OriginalIndex].GetEntry(_entries);
            }

            public override bool AddDuplicateLog(LogWrapper _log, int _instanceID, int _index) {
                return false;
            }

            public override void SetOriginalLog(int _index) {
                OriginalIndex = _index;
            }

            public override string GetTime(List<LogEntry> _entries, bool _collapse) {
                return Timestamp;
            }

            public override void OnDeleteLog(int _index) {
                if (OriginalIndex > _index) {
                    OriginalIndex--;
                }
            }
        }

        [Serializable]
        internal struct LogWrapper {
            public string Log;
            public string StackTrace;
            public LogType Type;

            public bool IsNative;

            public string Timestamp;
            public string Frame;

            // -----------------------

            public LogWrapper(string _log, string _stackTrace, LogType _type, bool _isNative) {
                Log = _log;
                StackTrace = _stackTrace;
                Type = _type;

                IsNative = _isNative;

                Timestamp = DateTime.Now.ToLongTimeString();

                // Unity might throw an exception when this is called during serialization.
                try {
                    Frame = EditorApplication.isPlaying ? Time.frameCount.ToString() : string.Empty;
                } catch (UnityException) {
                    Frame = string.Empty;
                }
            }
        }

        // -----------------------

        [Serializable]
        internal class LogStack {
            public OriginalLogEntry Entry = null;
            public string Header = string.Empty;
            public List<LogStackCall> Calls = new List<LogStackCall>();

            // -----------------------

            public void Setup(OriginalLogEntry _entry) {
                Entry = _entry;
                Header = $"{$"[{_entry.Type}]".Bold()}   {_entry.LogString}";

                LogStackUtility.FillStackCalls(_entry.StackTrace, Calls);
            }
        }

        [Serializable]
        internal class LogStackCall {
            public GUIContent Header = GUIContent.none;
            public string FilePath = string.Empty;

            public int LineNumber = -1;
            public List<LogStackLine> Lines = new List<LogStackLine>();

            // -----------------------

            public LogStackCall(string _header) {
                Header = new GUIContent(_header, _header.Trim());
            }

            public LogStackCall(string _header, string _filePath, int _lineNumber) {
                Header = new GUIContent(_header, _filePath);
                FilePath = _filePath;
                LineNumber = _lineNumber;
            }
        }

        [Serializable]
        internal struct LogStackLine {
            public string Line;
            public int Number;

            // -----------------------

            public LogStackLine(string _line, int _number) {
                Line = string.Format(LogStackUtility.LineFormat, _number, _line);
                Number = _number;
            }
        }

        // -----------------------

        [Serializable]
        internal class LogsWrapper {
            [SerializeReference] public List<LogEntry> Logs = new List<LogEntry>();
        }

        /// <summary>
        /// Log stack wrappers-related static class utility, used to centralize all operations at the same place.
        /// </summary>
        private static class LogStackUtility {
            public const StringComparison Comparison = StringComparison.Ordinal;
            public const int DefaultTrimIndex = 999;

            public const string TextBeforeFilePath  = " (at ";
            public const string TextBeforeLineIndex = ":";
            public const string TextAfterFilePath   = ")";
            public const string PathSeparator       = ".";
            public const string TextBeforeMethodArg = "(";
            public const string ArgumentSeparator   = ", ";
            public const char NoURLText             = '<';

            public const string CallMemberFormat    = "{0} ({1})";
            public const string HeaderFormat        = "-  <b>{0}</b>          {1}:{2}";
            public const string LineFormat          = "    {0}:     {1}";
            public const string SingleLineFormat    = "    {0}";

            public static readonly char[] SeparatorToReplace        = new char[] { ':', '/' };
            public static readonly char[] ArgumentSeparatorAsArray  = new char[] { ',' };
            public static readonly char[] PathSeparatorAsArray      = new char[] { '.' };

            // -----------------------

            /// <summary>
            /// Fills a specific <see cref="OriginalLogEntry"/> detailed informations.
            /// </summary>
            /// <param name="_entry"></param>
            public static void FillLogEntry(OriginalLogEntry _entry) {
                // Compiltation logs.
                if (string.IsNullOrEmpty(_entry.StackTrace)) {
                    // Format is: "Assets/.../File.cs(l,i): error" where l is the line number, and i the column number.
                    string _log = _entry.LogString;
                    int _endFileIndex = _log.IndexOf(':');

                    if (_endFileIndex != -1) {
                        int _startLineIndex = _log.LastIndexOf('(', _endFileIndex);
                        int _endLineIndex = _log.LastIndexOf(',', _endFileIndex);

                        if ((_startLineIndex != -1) && (_endLineIndex != -1)) {
                            string _file = _log.Substring(0, _startLineIndex);
                            _startLineIndex++;

                            int _length = Mathf.Max(0, _endLineIndex - _startLineIndex);

                            if (!int.TryParse(_log.Substring(_startLineIndex, _length), out int _lineNumber)) {
                                _lineNumber = -1;
                            }

                            if (!int.TryParse(_log.Substring(_endLineIndex + 1, _endFileIndex - (_endLineIndex + 2)), out int _columnNumber)) {
                                _columnNumber = -1;
                            }

                            _entry.File = _file;
                            _entry.LineNumber = _lineNumber;
                            _entry.ColumnNumber = _columnNumber;
                        }
                    }

                    return;
                }

                string[] _lines = _entry.StackTrace.Split('\n');

                foreach (string _line in _lines) {
                    if (!string.IsNullOrEmpty(_line) && (GetStackInfos(_line, out string _, out string _filePath, out int _lineNumber,
                                                                       out string _namespace, out string _class, out string _method) == 0)) {
                        // Get detailed informations.
                        if (EnhancedConsoleEnhancedSettings.Settings.IgnoreCall(_namespace, _class, _method)) {
                            continue;
                        }

                        _entry.Namespace = _namespace;
                        _entry.Class = _class;
                        _entry.Method = _method;

                        _entry.File = _filePath;
                        _entry.LineNumber = _lineNumber;

                        return;
                    }
                }
            }

            /// <summary>
            /// Fills a stack call list with a specific stack string.
            /// </summary>
            public static void FillStackCalls(string _stack, List<LogStackCall> _calls) {
                string[] _lines = _stack.Split('\n');
                _calls.Clear();

                foreach (string _line in _lines) {
                    if (!string.IsNullOrEmpty(_line) && GetStackCall(_line, out LogStackCall _call)) {
                        _calls.Add(_call);
                    }
                }
            }

            /// <summary>
            /// Get and fill a new stack call instance from a specific line.
            /// </summary>
            public static bool GetStackCall(string _line, out LogStackCall _call) {
                switch (GetStackInfos(_line, out string _callMember, out string _filePath, out int _lineNumber, out string _namespace, out string _class, out string _method)) {
                    // Success.
                    case 0:
                        _call = new LogStackCall(string.Format(HeaderFormat, _callMember, _filePath, _lineNumber), _filePath, _lineNumber);
                        FillStackCallLines(_call);
                        break;

                    // Standard.
                    case 1:
                        _callMember = GetCallMember(_line, out _namespace, out _class, out _method);
                        _call = new LogStackCall(string.Format(SingleLineFormat, _callMember));
                        break;

                    // Default.
                    default:
                    case 2:
                        _call = new LogStackCall(string.Format(SingleLineFormat, _line));
                        return true;
                }

                return !EnhancedConsoleEnhancedSettings.Settings.IgnoreCall(_namespace, _class, _method);
            }

            /// <summary>
            /// Get informations on a specific stack line.
            /// </summary>
            public static int GetStackInfos(string _line, out string _callMember, out string _filePath, out int _lineNumber, out string _namespace, out string _class, out string _method) {
                _namespace = _class
                           = _method
                           = string.Empty;

                // Get the _index of the file url.
                int _endCallIndex = _line.IndexOf(TextBeforeFilePath, Comparison);
                if (_endCallIndex == -1) {
                    _callMember = string.Empty;
                    _filePath = string.Empty;
                    _lineNumber = -1;

                    return 1;
                }

                int _filePathIndex = _endCallIndex + TextBeforeFilePath.Length;

                // Sometimes no url is given, just an id between <>, and there is no way to retrieve the file the call is from.
                if (_line[_filePathIndex] == NoURLText) {
                    _callMember = string.Empty;
                    _filePath = string.Empty;
                    _lineNumber = -1;

                    return 1;
                }

                // Get the call line number using LastIndex, because the url can contain the same characters (like with: "C:/My (Secret) Documents/Project")
                _filePath = _line.Substring(_filePathIndex);

                int _startLineIndex = _filePath.LastIndexOf(TextBeforeLineIndex, Comparison);
                int _endLineIndex = _filePath.LastIndexOf(TextAfterFilePath, Comparison);

                if ((_startLineIndex == -1) || (_endLineIndex == -1) || !int.TryParse(_filePath.Substring(_startLineIndex + 1, _endLineIndex - (_startLineIndex + 1)), out _lineNumber)) {
                    _callMember = string.Empty;
                    _filePath = string.Empty;
                    _lineNumber = -1;

                    return 2;
                }

                _filePath = _filePath.Substring(0, _startLineIndex);

                // Get the calling member as header.
                string _callPath = _line.Substring(0, _endCallIndex);
                _callMember = GetCallMember(_callPath, out _namespace, out _class, out _method);

                return 0;
            }

            /// <summary>
            /// Fills a stack call lines from its associated file.
            /// </summary>
            /// <param name="_call"></param>
            public static void FillStackCallLines(LogStackCall _call) {
                // File content preview.
                string _fullPath = $"{EnhancedEditorUtility.GetProjectPath()}{_call.FilePath}";

                if (!File.Exists(_fullPath)) {
                    return;
                }

                string[] _lines = File.ReadAllLines(_fullPath);
                int _lineNumber = Mathf.Min(_call.LineNumber, _lines.Length);
                int _index = _lineNumber - 1;

                // Get the call line first.
                List<Pair<string, int>> _tempLines = new List<Pair<string, int>> {
                    new Pair<string, int>(_lines[_index], _lineNumber)
                };

                // Then get previous and next non-empty lines.
                int _lineCount = GetWindow().stackCallLineCount - 1;

                if (_lineCount != 0) {
                    int _count = _lineCount / 2;

                    while (_index-- > 0) {
                        if (GetLine(0)) {
                            break;
                        }
                    }

                    _index = _lineNumber - 1;
                    _count = _lineCount / 2;

                    while (++_index < _lines.Length) {
                        if (GetLine(_tempLines.Count)) {
                            break;
                        }
                    }

                    // ----- Local Method ----- \\

                    bool GetLine(int _insertIndex) {
                        string _line = _lines[_index];

                        if (!string.IsNullOrEmpty(_line.Trim())) {
                            _tempLines.Insert(_insertIndex, new Pair<string, int>(_line, _index + 1));

                            _count--;
                            if (_count == 0) {
                                return true;
                            }
                        }

                        return false;
                    }
                }

                // Remove white spaces before the line content.
                int _startIndex = DefaultTrimIndex;
                foreach (var _pair in _tempLines) {
                    string _content = _pair.First;

                    for (int i = 0; i < _content.Length; i++) {
                        char _char = _content[i];
                        if (!char.IsWhiteSpace(_char)) {
                            _startIndex = Mathf.Min(_startIndex, i);
                            break;
                        }
                    }
                }

                if (_startIndex == DefaultTrimIndex) {
                    _startIndex = 0;
                }

                for (int i = 0; i < _tempLines.Count; i++) {
                    var _pair = _tempLines[i];

                    string _line = _pair.First;
                    _call.Lines.Add(new LogStackLine(string.IsNullOrEmpty(_line) ? _line : _line.Substring(_startIndex), _pair.Second));
                }
            }

            // -----------------------

            private static string GetCallMember(string _line, out string _namespace, out string _class, out string _method) {
                int _endCallIndex = _line.IndexOf(TextBeforeFilePath, Comparison);
                if (_endCallIndex != -1) {
                    _line = _line.Substring(0, _endCallIndex);
                }

                int _startArgIndex = _line.LastIndexOf(TextBeforeMethodArg, Comparison);

                if (_startArgIndex != -1) {
                    // The member path may or may not have a space between the end of the method name and the parenthesis, so trim the string.
                    string _call = GetClassMember(_line.Substring(0, _startArgIndex).Trim(), out _namespace, out _class, out _method);
                    string _argumentContent = string.Empty;

                    string[] _arguments = _line.Substring(_startArgIndex + 1, _line.Length - (_startArgIndex + 2)).Split(ArgumentSeparatorAsArray, StringSplitOptions.RemoveEmptyEntries);
                    _argumentContent = string.Join(ArgumentSeparator, Array.ConvertAll(_arguments, a => GetClassMember(a, out _, out _, out _)));

                    return string.Format(CallMemberFormat, _call, _argumentContent);
                }

                return GetClassMember(_line, out _namespace, out _class, out _method);
            }

            private static string GetClassMember(string _path, out string _namespace, out string _class, out string _method) {
                foreach (char _separator in SeparatorToReplace) {
                    string[] _parts = _path.Split(_separator);

                    for (int i = 1; i < _parts.Length; i++) {
                        string[] _split = _parts[i].Split(PathSeparatorAsArray);
                        if (_split.Length != 0) {
                            _parts[i] = _split[_split.Length - 1];
                        }
                    }

                    _path = string.Join(PathSeparator, _parts);
                }

                string[] _elements = _path.Split(PathSeparatorAsArray, StringSplitOptions.RemoveEmptyEntries);
                int _count = _elements.Length;

                _namespace = string.Empty;
                _class = string.Empty;
                _method = string.Empty;

                if (_count > 1) {
                    _namespace = string.Join(PathSeparator, _elements, 0, _count - 2);
                    _class = _elements[_count - 2];
                }

                if (_count > 0) {
                    _method = _elements[_count - 1];
                }

                return string.IsNullOrEmpty(_class) ? _method : string.Join(PathSeparator, _class, _method);
            }
        }
        #endregion

        #region Log Column
        [Serializable]
        private class LogColumn {
            public LogColumnType Column = 0;
            public ColumnVisibility Visibility = 0;

            public float Size = 50f;
            private Vector2 currentSize = Vector2.zero;

            public GUIContent Label = new GUIContent();
            public ColumnResizeType ResizeType = ColumnResizeType.Resizable;
            public float MinSize = 20f;

            public bool IsStatic {
                get { return ResizeType == ColumnResizeType.Static; }
            }

            public bool IsVisible {
                get { return Visibility != ColumnVisibility.Hidden; }
            }

            // -----------------------

            public LogColumn(LogColumnType _column, ColumnVisibility _visibility, float _size, bool _isStaticSize = true) : this(_column, _visibility, _size) {
                ResizeType = _isStaticSize ? ColumnResizeType.Static : ColumnResizeType.DynamicSize;
                MinSize = _size;
            }

            public LogColumn(LogColumnType _column, ColumnVisibility _visibility, float _size, float _minSize) : this(_column, _visibility, _size) {
                ResizeType = ColumnResizeType.Resizable;
                MinSize = _minSize;
            }

            private LogColumn(LogColumnType _column, ColumnVisibility _visibility, float _size) {
                Column = _column;
                Visibility = _visibility;
                Size = _size;

                switch (_column) {
                    case LogColumnType.Icon:
                        Label = new GUIContent(string.Empty, "Filter Icon");
                        break;

                    case LogColumnType.Type:
                        Label = new GUIContent("Type", "The type of these logs");
                        break;

                    case LogColumnType.Namespace:
                        Label = new GUIContent("Namespace", "The namespace of the class responsible for these logs");
                        break;

                    case LogColumnType.Class:
                        Label = new GUIContent("Class", "Name of the class responsible for these logs");
                        break;

                    case LogColumnType.Method:
                        Label = new GUIContent("Method", "Name of the method responsible for these logs");
                        break;

                    case LogColumnType.Log:
                        Label = new GUIContent("Log", "Logs message content");
                        break;

                    case LogColumnType.Object:
                        Label = new GUIContent("Object", "The context object used for these logs");
                        break;

                    case LogColumnType.File:
                        Label = new GUIContent("File", "Name of the file responsible for theese logs");
                        break;

                    case LogColumnType.Frame:
                        Label = new GUIContent("Frame", "Application time frame these logs were received on the console");
                        break;

                    case LogColumnType.Timestamp:
                        Label = new GUIContent("Time", "Time of the day these logs were received on the console");
                        break;

                    default:
                        Label = new GUIContent("???", "Unknown Column Type");
                        break;
                }
            }

            // -----------------------

            public float ReduceSize(float _delta) {
                // Non-resizable column management.
                if (ResizeType == ColumnResizeType.Static) {
                    return 0f;
                }

                float _margin = Size - MinSize;
                _delta = Mathf.Clamp(_margin - _delta, 0f, _delta);

                Size -= _delta;
                return _delta;
            }

            public Rect GetRect() {
                Rect _position;

                if (ResizeType == ColumnResizeType.DynamicSize) {
                    _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, Styles.LogColumnStyle, GUILayout.MinWidth(MinSize), GUILayout.ExpandWidth(true));

                    if (Event.current.type == EventType.Repaint) {
                        Size = _position.width;
                    }
                } else {
                    _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, Styles.LogColumnStyle, GUILayout.MinWidth(MinSize), GUILayout.MaxWidth(Size));
                }

                if (Event.current.type == EventType.Repaint) {
                    currentSize.Set(_position.x, _position.width);
                }

                return _position;
            }

            public Rect GetRect(Rect _position) {
                _position.x = currentSize.x;
                _position.width = currentSize.y;

                return _position;
            }

            public void InvertVisibility() {
                if (Visibility == ColumnVisibility.AlwaysVisible) {
                    return;
                }

                Visibility = IsVisible ? ColumnVisibility.Hidden : ColumnVisibility.Visible;
            }
        }
        #endregion

        #region Enum
        [Flags]
        public enum Flags {
            // Settings.
            Collapse = 1 << 0,
            ErrorPause = 1 << 1,
            ClearOnPlay = 1 << 2,
            ClearOnBuild = 1 << 3,
            ClearOnCompile = 1 << 4,

            // Debug.
            LogButton = 1 << 30,
            Disabled = 1 << 31
        }

        [Flags]
        public enum LogColumnType {
            Icon = 1 << 0,
            Type = 1 << 1,

            // Log file.
            Namespace = 1 << 3,
            Class = 1 << 4,
            Method = 1 << 6,
            Log = 1 << 10,
            Object = 1 << 15,
            File = 1 << 18,

            // Additional infos.
            Frame = 1 << 21,
            Timestamp = 1 << 22,
        }

        public enum ColumnVisibility {
            Hidden = 0,
            Visible = 1,
            AlwaysVisible = 2
        }

        public enum ColumnResizeType {
            Static,
            Resizable,
            DynamicSize,
        }
        #endregion

        // ----- Editor Window ----- \\

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="EnhancedConsoleWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="EnhancedConsoleWindow"/> instance on screen.</returns>
        [MenuItem(InternalUtility.MenuItemPath + "Enhanced Console", false, 40)]
        public static EnhancedConsoleWindow GetWindow() {
            return GetWindow(true);
        }

        /// <inheritdoc cref="GetWindow"/>
        public static EnhancedConsoleWindow GetWindow(bool _focus) {
            EnhancedConsoleWindow _window = GetWindow<EnhancedConsoleWindow>("Enhanced Console", _focus);
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string CompilationKey     = "EnhancedConsoleCompilation";
        private const string EditorPrefKey      = "EnhancedConsoleKey";
        private const string SessionStateKey    = "EnhancedConsoleKey";
        private const string UndoRecordTitle    = "Enhanced Console Change";

        private const int DefaultLogCapacity = 50;

        private static readonly GUIContent openPreferencesGUI   = new GUIContent("Open Preferences", "Opens the enhanced console preferences window");
        private static readonly List<LogWrapper> pendingLogs    = new List<LogWrapper>();

        [SerializeReference] private List<LogEntry> logs = new List<LogEntry>(DefaultLogCapacity);

        [SerializeField] private LogColumn[] logColumns = new LogColumn[] {
            new LogColumn(LogColumnType.Icon,       ColumnVisibility.AlwaysVisible, 25f),
            new LogColumn(LogColumnType.Type,       ColumnVisibility.Visible, 50f, 25f),

            new LogColumn(LogColumnType.Namespace,  ColumnVisibility.Hidden, 70f, 25f),
            new LogColumn(LogColumnType.Class,      ColumnVisibility.Visible, 70f, 25f),
            new LogColumn(LogColumnType.Method,     ColumnVisibility.Visible, 70f, 25f),
            new LogColumn(LogColumnType.Log,        ColumnVisibility.AlwaysVisible, 100f, false),
            new LogColumn(LogColumnType.Object,     ColumnVisibility.Hidden, 50f, 25f),
            new LogColumn(LogColumnType.File,       ColumnVisibility.Hidden, 70f, 25f),

            new LogColumn(LogColumnType.Frame,      ColumnVisibility.Hidden, 40f, 25f),
            new LogColumn(LogColumnType.Timestamp,  ColumnVisibility.Visible, 40f, 25f),
        };

        [SerializeField] private Flags flags            = Flags.ClearOnPlay;
        [SerializeField] private string searchFilter    = string.Empty;

        private Vector2 logScroll               = new Vector2();
        private Vector2 stackTraceHeaderScroll  = new Vector2();
        private Vector2 stackTraceContentScroll = new Vector2();

        // -----------------------

        private void OnEnable() {
            // Load content.
            string _json = EditorPrefs.GetString(EditorPrefKey, string.Empty);
            if (!string.IsNullOrEmpty(_json)) {
                EditorJsonUtility.FromJsonOverwrite(_json, this);
            }

            // Clear the console when launching the editor.
            if (!SessionState.GetBool(SessionStateKey, false)) {
                SessionState.SetBool(SessionStateKey, true);
                Clear();
            }

            titleContent.image = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image;

            // Callback.
            RegisterCallbacks();

            // Clear on compile.
            // Only do this when enabled to avoid focusing the editor.
            if (HasFlag(Flags.ClearOnCompile) && SessionState.GetBool(CompilationKey, false)) {
                SessionState.SetBool(CompilationKey, false);
                Clear();
            }

            // Delete compilation logs on enable.
            for (int i = logs.Count; i-- > 0;) {
                if (string.IsNullOrEmpty(logs[i].GetEntry(logs).StackTrace)) {
                    DeleteLog(i);
                }
            }

            RefreshFilters();
            GetNativeConsoleLogs();
        }

        private void OnGUI() {
            // Update pending logs.
            UpdateLogs();
            requireRepaint = false;

            // Toolbar.
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
                DrawToolbar();
            }

            // Logs.
            DrawLogs();

            // Stack trace.
            Rect _area;
            using (var _scope = new EditorGUILayout.VerticalScope(GUILayout.Height(stackTraceHeight))) {
                _area = _scope.rect;
                DrawStackTrace();
            }

            // Splitter handler.
            // Drawn outside of the scope to avoid being clamped within.
            Rect _separator = new Rect(_area) {
                x = 0f,
                y = _area.y - 2f,
                width = position.width,
                height = 2f
            };

            EditorGUI.DrawRect(_separator, EnhancedEditorGUIUtility.GUISplitterColor);

            // The area position may be wrong on certain layout event, so only set the height when changed.
            using (var _scope = new EditorGUI.ChangeCheckScope()) {
                float _height = EnhancedEditorGUIUtility.VerticalSplitterHandle(_area, MinControlHeight, position.height - (MinControlHeight + ToolbarHeight));

                if (_scope.changed) {
                    stackTraceHeight = _height;
                }
            }
        }

        private void OnDisable() {
            UnregisterCallbacks();

            // Save content.
            string _json = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(EditorPrefKey, _json);
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu _menu) {
            _menu.AddItem(openPreferencesGUI, false, () => EnhancedConsoleEnhancedSettings.OpenUserSettings());
        }
        #endregion

        #region Callback
        private const int MaxPendingLog = 9000;
        private static bool requireRepaint = true;
        private static bool hasUnpersistError = false;

        // -----------------------

        private void OnPreprocessBuild(BuildReport _) {
            // Clears the console before starting to build.
            if (HasFlag(Flags.ClearOnBuild)) {
                Clear();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange _state) {
            RegisterCallbacks();

            // When entering edit mode, OnEnable is not called again, but every serializereference is lost.
            if (_state == PlayModeStateChange.EnteredEditMode) {
                RefreshFilters();
            }

            // Clear the console when entering play mode.
            if (HasFlag(Flags.ClearOnPlay) && (_state == PlayModeStateChange.ExitingEditMode)) {
                Clear();
            }

            GetNativeConsoleLogs();
            Repaint();
        }

        private void OnUpdate() {
            if (requireRepaint) {
                Repaint();
            }
        }

        private static void OnLogMessageReceived(string _log, string _stackTrace, LogType _type) {
            RegisterLog(_log, _stackTrace, _type);
        }

        [DidReloadScripts]
        private static void OnAfterCompilation() {
            SessionState.SetBool(CompilationKey, true);
        }

        // -----------------------

        private static void RegisterLog(string _log, string _stackTrace, LogType _type, bool _isNative = false) {
            // Used for debug purpose.
            if (!EnhancedConsoleEnhancedSettings.Enabled) {
                return;
            }

            pendingLogs.Add(new LogWrapper(_log.TrimEnd(), _stackTrace.TrimEnd(), _type, _isNative));

            while (pendingLogs.Count > MaxPendingLog) {
                pendingLogs.RemoveAt(0);
            }

            // Dialog box when object(s) failed to unpersist.
            if ((_type == LogType.Error) && !hasUnpersistError && _log.Contains("Failed to unpersist")) {

                hasUnpersistError = true;
                EditorApplication.delayCall += OnUnpersistError;
            }

            // Repaint.
            requireRepaint = true;
        }

        private static void OnUnpersistError() {

            if (EditorUtility.DisplayDialog("Failed to Unpersist Object(s)", "One or more object(s) failed to unpersist and have been destroyed in the open scene(s).\n" +
                                            "Would you like to reload them?\n\nAny unsaved modification will be lost.", "Yes please", "No thanks")) {

                // Cache.
                List<string> _scenePaths = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    _scenePaths.Add(SceneManager.GetSceneAt(i).path);
                }

                string _activeScenePath = SceneManager.GetActiveScene().path;

                // Load.
                EditorSceneManager.OpenScene(_scenePaths[0], OpenSceneMode.Single);
                for (int i = 0; i < _scenePaths.Count; i++) {
                    EditorSceneManager.OpenScene(_scenePaths[i], OpenSceneMode.Additive);
                }

                // Set active scene.
                SceneManager.SetActiveScene(SceneManager.GetSceneByPath(_activeScenePath));
            }

            hasUnpersistError = false;
        }
        #endregion

        #region GUI Draw
        private const int LogEntryMaxLine = 10;
        private const float ToolbarHeight = 21f;
        private const float MinControlHeight = 60f;
        private const float StackLineHeight = 16f;
        private const float StackContentOffset = 0f;
        private const float StackPreviewSpacing = 5f;
        private const float StackHeaderSpacing = 5f;

        private const int StackHeaderMaxLine = 4;

        private const string LogFileExtension = "txt";
        private const string LogSizeFormat = "Log Size/{0} Lines";
        private const string StackLineCountFormat = "Stack Preview/{0} Lines";
        private const string LogColumnFormat = "Column/{0}";
        private const string NoEntryString = " -";

        private readonly int[] stackCallLineAmount = new int[] { 1, 3, 5, 7, 9 };

        private readonly GUIContent clearGUI = new GUIContent("Clear", "Clears all entries in the console.");
        private readonly GUIContent collapseGUI = new GUIContent("Collapse", "Collapses all identical entries.");
        private readonly GUIContent errorPauseGUI = new GUIContent("Error Pause", "Toggles pause in Play Mode on error.");

        private readonly GUIContent clearOnPlayGUI = new GUIContent("Clear on Play", "Automatically clears the console when entering play mode.");
        private readonly GUIContent clearOnBuildGUI = new GUIContent("Clear on Build", "Automatically clears the console when launching a new build.");
        private readonly GUIContent clearOnCompileGUI = new GUIContent("Clear on Compile", "Automatically clears the console when compiling scripts.");
        private readonly GUIContent openPlayerLogGUI = new GUIContent("Open Player Log", "Opens the player log file.");
        private readonly GUIContent openEditorLogGUI = new GUIContent("Open Editor Log", "Opens the editor log file.");

        private readonly GUIContent logButtonsGUI = new GUIContent("Log Buttons", "Toggles this console debug log buttons activation.");
        private readonly GUIContent enabledGUI = new GUIContent("Enabled", "Toggles this console activation.");

        private readonly GUIContent copyLogContentGUI = new GUIContent("Copy Log Content", "Copies this log content to the clipboard.");
        private readonly GUIContent copyLogStackTraceGUI = new GUIContent("Copy Log Stack Trace", "Copies this log stack trace to the clipboard.");

        [SerializeField] private int logLineCount = 2;
        [SerializeField] private int stackCallLineCount = 4;
        [SerializeField] private float stackTraceHeight = MinControlHeight;

        private LogStack selectedLogStackTrace = new LogStack();
        private int previousLogWidth = 0;

        private bool doFocusSelection = false;
        private int selectedLogIndex = -1;

        // -----------------------

        private void DrawToolbar() {
            GUIStyle _buttonStyle = EditorStyles.toolbarButton;

            // Main controls.
            if (GUILayout.Button(clearGUI, _buttonStyle, GUILayout.Width(40f))) {
                Clear();
            }

            bool _wasCollapse = HasFlag(Flags.Collapse);
            bool _wasErrorPause = HasFlag(Flags.ErrorPause);

            bool _collapse = GUILayout.Toggle(_wasCollapse, collapseGUI, _buttonStyle, GUILayout.Width(60f));
            bool _errorPause =  GUILayout.Toggle(_wasErrorPause, errorPauseGUI, _buttonStyle, GUILayout.Width(80f));

            if (_wasCollapse != _collapse) {
                InvertFlag(Flags.Collapse);
            }

            if (_wasErrorPause != _errorPause) {
                InvertFlag(Flags.ErrorPause);
            }

            // Additional options.
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, _buttonStyle, GUILayout.Width(16f));
            if (EnhancedEditorGUI.IconButton(_position, Styles.DropdownGUI, _buttonStyle)) {
                // Record object before any change.
                Undo.RecordObject(this, UndoRecordTitle);
                GenericMenu _menu = new GenericMenu();

                _menu.AddItem(clearOnPlayGUI, HasFlag(Flags.ClearOnPlay), () => InvertFlag(Flags.ClearOnPlay));
                _menu.AddItem(clearOnBuildGUI, HasFlag(Flags.ClearOnBuild), () => InvertFlag(Flags.ClearOnBuild));
                _menu.AddItem(clearOnCompileGUI, HasFlag(Flags.ClearOnCompile), () => InvertFlag(Flags.ClearOnCompile));

                _menu.AddSeparator(string.Empty);

                _menu.AddItem(openPlayerLogGUI, false, InternalEditorUtility.OpenPlayerConsole);
                _menu.AddItem(openEditorLogGUI, false, InternalEditorUtility.OpenEditorConsole);

                _menu.AddSeparator(string.Empty);

                // Size of the log entries.
                for (int i = 1; i < LogEntryMaxLine + 1; i++) {
                    int _lineCount = i;
                    _menu.AddItem(new GUIContent(string.Format(LogSizeFormat, _lineCount)), logLineCount == _lineCount, () => logLineCount = _lineCount);
                }

                // Change the stack trace preview line count.
                foreach (int _amount in stackCallLineAmount) {
                    _menu.AddItem(new GUIContent(string.Format(StackLineCountFormat, _amount)), stackCallLineCount == _amount, () => {
                        stackCallLineCount = _amount;
                        RefreshStackTrace();
                    });
                }

                // Columns.
                _menu.AddSeparator(string.Empty);

                foreach (LogColumn _column in logColumns) {
                    if (_column.Visibility == ColumnVisibility.AlwaysVisible) {
                        continue;
                    }

                    _menu.AddItem(new GUIContent(string.Format(LogColumnFormat, _column.Label.text)), _column.IsVisible, _column.InvertVisibility);
                }

                _menu.AddSeparator(string.Empty);

                _menu.AddItem(logButtonsGUI, HasFlag(Flags.LogButton), () => InvertFlag(Flags.LogButton));
                _menu.AddItem(enabledGUI, !HasFlag(Flags.Disabled), () => {

                    InvertFlag(Flags.Disabled);
                    EnhancedConsoleEnhancedSettings.Enabled = !HasFlag(Flags.Disabled);
                });

                _position.y += EditorGUIUtility.standardVerticalSpacing;
                _menu.DropDown(_position);
            }

            // Refresh.
            _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, _buttonStyle, GUILayout.Width(22f));
            if (EnhancedEditorGUI.IconButton(_position, Styles.RefreshGUI, _buttonStyle)) {
                RefreshFilters();
            }

            GUILayout.Space(3f);

            // Search filter.
            string _searchFilter = EnhancedEditorGUILayout.ToolbarSearchField(searchFilter, GUILayout.MinWidth(50f), GUILayout.ExpandWidth(true));
            if (_searchFilter != searchFilter) {
                // Record object state.
                Undo.RecordObject(this, UndoRecordTitle);
                searchFilter = _searchFilter;

                // Filter.
                FilterLogs();
            }

            GUILayout.Space(3f);

            // Import / export logs.
            if (GUILayout.Button(Styles.ImportGUI, _buttonStyle, GUILayout.Width(25f))) {
                string _path = EditorUtility.OpenFilePanel("Import Logs", string.Empty, LogFileExtension);
                if (!string.IsNullOrEmpty(_path)) {

                    try {
                        string _json = File.ReadAllText(_path);
                        LogsWrapper _logs = JsonUtility.FromJson<LogsWrapper>(_json);

                        logs = _logs.Logs;
                        RefreshFilters();
                    } catch (Exception e) when ((e is ArgumentException) || (e is IOException)) {
                        EditorUtility.DisplayDialog("Log Import", "Could not load any console entry from the selected log file.\n\n" +
                                                    "Please select another file and try again.", "Ok");
                    }
                }
            }

            if (GUILayout.Button(Styles.ExportGUI, _buttonStyle, GUILayout.Width(25f))) {
                string _path = EditorUtility.SaveFilePanel("Export Logs", string.Empty, "ConsoleLogs", LogFileExtension);
                if (!string.IsNullOrEmpty(_path)) {
                    File.WriteAllText(_path, EditorJsonUtility.ToJson(new LogsWrapper() { Logs = logs }, true));
                }
            }

            // Debug.
            if (HasFlag(Flags.LogButton)) {
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.infoicon.sml"), _buttonStyle, GUILayout.Width(25f))) {
                    Debug.Log("<b>This</b> is a <color=red><i>RED</i></color> test log. <color=red>\u2764</color>".Size(16));
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.warnicon.sml"), _buttonStyle, GUILayout.Width(25f))) {
                    Debug.LogWarning("This is a warning log.");
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.erroricon.sml"), _buttonStyle, GUILayout.Width(25f))) {
                    Debug.LogError("This is an error log.");
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.erroricon.sml@2x"), _buttonStyle, GUILayout.Width(25f))) {
                    Debug.LogAssertion("This is an assert log");
                }

                using (var _scope = EnhancedGUI.GUIContentColor.Scope(SuperColor.HarvestGold.Get())) {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("console.erroricon.sml"), _buttonStyle, GUILayout.Width(25f))) {
                        Debug.LogException(new ArgumentException("This is an exception log"));
                    }
                }
            }

            GUILayout.FlexibleSpace();

            // Filters visibility and count.
            EnhancedConsoleEnhancedSettings _preferences = EnhancedConsoleEnhancedSettings.Settings;
            for (int i = 0; i < _preferences.FilterCount; i++) {
                ConsoleLogFilter _filter = _preferences.GetFilterAt(i);

                if (!_filter.Enabled || (!_filter.DoPinFilter && (_filter.DisplayedCount == 0))) {
                    continue;
                }

                string _count = _filter.DisplayedCount.ToString();
                GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_filter.Icon, $"{_filter.Name} [{_count}]"); {
                    _label.text = _count;
                }

                float _width = _buttonStyle.CalcSize(_label).x;
                bool _isVisible = GUILayout.Toggle(_filter.IsVisible, _label, _buttonStyle, GUILayout.Width(_width));

                // Set visible.
                if (_isVisible != _filter.IsVisible) {
                    _filter.IsVisible = _isVisible;

                    EnhancedEditorUserSettings.Instance.Save();
                    FilterLogs();
                }
            }
        }

        private void DrawLogs() {
            if (Event.current.type == EventType.Repaint) {
                EnhancedConsoleEnhancedSettings.Settings.ResetFiltersDisplayedCount();
            }

            // Columns toolbar.
            {
                using (var _horizontalScope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                    // Background.
                    Rect _temp = new Rect(_horizontalScope.rect) {
                        x = 0,
                        width = position.width
                    };

                    EditorGUI.DrawRect(_temp, EnhancedEditorGUIUtility.GUIBarColor);

                    // Columns.
                    for (int i = 0; i < logColumns.Length; i++) {
                        LogColumn _column = logColumns[i];
                        if (!_column.IsVisible) {
                            continue;
                        }

                        Rect _position = _column.GetRect();
                        EditorGUI.LabelField(_position, _column.Label, Styles.LogColumnStyle);

                        // Separator.
                        Rect _splitter = new Rect(_position); {
                            _splitter.xMin = _position.xMax - 1f;
                            _splitter.yMin -= EditorGUIUtility.standardVerticalSpacing;
                        }

                        EditorGUI.DrawRect(_splitter, EnhancedEditorGUIUtility.GUISplitterColor);

                        // Left side resize handle, only if both this column and the previous one are resizable.
                        if (!_column.IsStatic && GetPreviousColumn(i, out LogColumn _previous)) {
                            using (var _changeScope = new EditorGUI.ChangeCheckScope()) {
                                // Handle without min/max size.
                                float _size = EnhancedEditorGUIUtility.HorizontalSplitterHandle(_position, 0f, position.width);

                                if (_changeScope.changed) {
                                    // Only calculate the maximum size when changed.
                                    float _maxSize = _horizontalScope.rect.width;

                                    foreach (LogColumn _other in logColumns) {
                                        if (_other != _column) {
                                            _maxSize -= _other.MinSize + 5f; // 5 for the margins and spacing.
                                        }
                                    }

                                    _size = Mathf.Clamp(_size, _column.MinSize, _maxSize);

                                    // Reduce the previous column, and then the selected one.
                                    float _delta = _size - _column.Size;
                                    _delta = _previous.ReduceSize(_delta);

                                    _column.Size += _delta;
                                }
                            }
                        }

                        // ----- Local Method ----- \\

                        bool GetPreviousColumn(int _index, out LogColumn _previous) {
                            while (_index-- != 0) {
                                _previous = logColumns[_index];

                                if (_previous.IsVisible && !_previous.IsStatic) {
                                    return true;
                                }
                            }

                            _previous = null;
                            return false;
                        }
                    }

                    // Leave some space on the right side for the scroller.
                    GUILayout.Space(12f);

                    // Separator.
                    _temp.y = _temp.yMax - 2f;
                    _temp.height = 2f;
                    EditorGUI.DrawRect(_temp, EnhancedEditorGUIUtility.GUISplitterColor);
                }
            }

            // Logs.
            using (var _scope = new EditorGUILayout.VerticalScope())
            using (var _scrollScope = new GUILayout.ScrollViewScope(logScroll)) {
                logScroll = _scrollScope.scrollPosition;

                float _height = EnhancedEditorGUIUtility.CalculLineHeight(Styles.LogWordWrapStyle, logLineCount);
                bool _collapse = HasFlag(Flags.Collapse);

                // As the area scope starts with a zero position, and the scroll position to it.
                int _count = 0;
                Rect _area = new Rect(logScroll, _scope.rect.size);
                Rect _position = EditorGUILayout.GetControlRect(true, 0f); {
                    _position.x = 0f;
                    _position.width += 7f;
                }

                LogColumn _logColumn = Array.Find(logColumns, (c) => c.Column == LogColumnType.Log);
                int _logWidth = (int)_logColumn.GetRect(position).width;
                bool _updateHeight = _logWidth != previousLogWidth;

                float _origin = _position.y;

                for (int i = 0; i < logs.Count; i++) {
                    if (DrawLog(ref _position, i, _area, _collapse, _count, _height, _logColumn, _updateHeight)) {
                        _count++;
                    }
                }

                _height = EnhancedEditorGUI.ManageDynamicControlHeight(9524, _position.yMax - _origin);
                previousLogWidth = _logWidth;

                EditorGUILayout.GetControlRect(true, _height + 5f);

                // Delete selected logs.
                switch (EnhancedEditorGUIUtility.ValidateCommand(out Event _event)) {
                    case ValidateCommand.Delete:
                    case ValidateCommand.SoftDelete:
                        for (int i = logs.Count; i-- > 0;) {
                            if (logs[i].IsSelected) {
                                DeleteLog(i);
                            }
                        }
                        break;
                }

                // Multi-selection keys.
                EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(logs, IsLogSelected, CanSelectLog, SelectLog, selectedLogIndex);

                // Unselect on empty space click.
                _area.yMax -= EnhancedEditorGUIUtility.ResizeHandlerExtent;

                if (EnhancedEditorGUIUtility.MouseDown(_area)) {
                    foreach (LogEntry _log in logs) {
                        _log.IsSelected = false;
                    }

                    SelectLog(-1);
                }
            }
        }

        private bool DrawLog(ref Rect _position, int _index, Rect _area, bool _collapse, int _count, float _height, LogColumn _logColumn, bool _updateHeight) {
            LogEntry _log = logs[_index];

            // Hide duplicates on collapse.
            if (_collapse && _log.IsDuplicate) {
                return false;
            }

            OriginalLogEntry _entry = _log.GetEntry(logs);
            ConsoleLogFilter _filter = _entry.Filter;

            if (Event.current.type == EventType.Repaint) {
                _filter.DisplayedCount++;
            }

            if (!_log.IsVisible || !_filter.IsVisible) {
                return false;
            }

            _position.y += _position.height;
            _position.height = EditorGUIUtility.singleLineHeight;

            // Optimization for non-used events.
            switch (Event.current.type) {
                case EventType.Layout:
                    return true;

                default:
                    break;
            }

            // Clamp the log height depending on its log string.
            // Only recalculate the height on column size change.
            if (_logColumn.IsVisible) {
                if ((_updateHeight || (_entry.Height == 0f)) && !_entry.IsDuplicate) {
                    float _logHeight = Styles.LogWordWrapStyle.CalcHeight(EnhancedEditorGUIUtility.GetLabelGUI(_entry.LogString), _logColumn.GetRect(position).width);
                    _entry.Height = Mathf.Min(_logHeight, _height);
                }

                _height = _entry.Height;
                if (_height > _position.height) {
                    _position.height = _height;
                }
            }

            // Ignore out-of-position logs.
            if (((_position.yMax < _area.y) || (_position.y > _area.yMax)) && (!doFocusSelection || (selectedLogIndex != _index))) {
                return true;
            }

            EnhancedEditorGUI.BackgroundLine(_position, _log.IsSelected, _count);

            if (!_log.IsSelected && _filter.UseColor) {
                EditorGUI.DrawRect(_position, _filter.Color);
            }

            // Separator.
            Rect _separator = new Rect(_position); {
                _separator.yMin = _separator.yMax - 1f;
            }

            EditorGUI.DrawRect(_separator, new Color(.2f, .2f, .2f, .2f));

            // Draw each column content.
            using (var _scope = EnhancedGUI.GUIContentColor.Scope(_filter.UseTextColor ? _filter.TextColor : GUI.contentColor)) {
                for (int i = 0; i < logColumns.Length; i++) {
                    LogColumn _column = logColumns[i];
                    if (!_column.IsVisible) {
                        continue;
                    }

                    Rect _columnPosition = _column.GetRect(_position);
                    GUIContent _label = GUIContent.none;
                    GUIStyle _style = Styles.LogStyle;

                    switch (_column.Column) {
                        case LogColumnType.Icon:
                            _label = EnhancedEditorGUIUtility.GetLabelGUI(_filter.Icon, _filter.Name);
                            break;

                        case LogColumnType.Type:
                            SetLabel(_entry.Type.ToString());
                            break;

                        case LogColumnType.Namespace:
                            SetLabel(_entry.Namespace);
                            break;

                        case LogColumnType.Class:
                            SetLabel(_entry.Class);
                            break;

                        case LogColumnType.Method:
                            SetLabel(_entry.Method);
                            break;

                        case LogColumnType.Log:
                            _style = Styles.LogWordWrapStyle;
                            SetLabel(_entry.LogString);
                            break;

                        case LogColumnType.Object:
                            SetValidLabel(_entry.ContextObjectName, !string.IsNullOrEmpty(_entry.ContextObjectName));
                            break;

                        case LogColumnType.File:
                            SetLabel(_entry.File);
                            break;

                        case LogColumnType.Frame:
                            SetValidLabel(_log.Frame, !string.IsNullOrEmpty(_entry.Frame));
                            break;

                        case LogColumnType.Timestamp:
                            SetLabel(_log.Timestamp);
                            break;

                        default:
                            break;
                    }

                    EditorGUI.LabelField(_columnPosition, _label, _style);

                    // ----- Local Method ----- \\

                    void SetValidLabel(string _content, bool _isValidEntry) {
                        SetLabel(_isValidEntry ? _content : NoEntryString);
                    }

                    void SetLabel(string _content) {
                        _label = EnhancedEditorGUIUtility.GetLabelGUI(_content, _content);
                    }
                }
            }

            // Collapse count.
            if (_collapse) {
                GUIStyle _style = Styles.CountBadgeStyle;
                GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI((_entry.DuplicateCount + 1).ToString());

                float _width = _style.CalcSize(_label).x;

                Rect _temp = new Rect(_position) {
                    x = _position.xMax - (_width + 7f),
                    width = _width,
                    y = _position.y + ((_position.height - EditorGUIUtility.singleLineHeight) / 2f) + 1f,
                    height = EditorGUIUtility.singleLineHeight
                };

                EditorGUI.LabelField(_temp, _label, _style);
            }

            // Open on double click.
            if (!string.IsNullOrEmpty(_entry.File) && EnhancedEditorGUIUtility.DoubleClick(_position)) {
                if (_entry.ColumnNumber == -1) {
                    InternalEditorUtility.TryOpenErrorFileFromConsole(_entry.File, _entry.LineNumber);
                } else {
                    InternalEditorUtility.TryOpenErrorFileFromConsole(_entry.File, _entry.LineNumber, _entry.ColumnNumber);
                }
            }

            // Select on click.
            Rect _selection = new Rect(_position); {
                _selection.yMax = Mathf.Min(_selection.yMax, _area.yMax - EnhancedEditorGUIUtility.ResizeHandlerExtent);
            }

            // Ping context object on click.
            if ((_entry.ContextInstanceID != -1) && (_position.Event(out Event _event) == EventType.MouseDown) && (_event.button == 0) && (_event.modifiers == EventModifiers.None)) {
                EditorGUIUtility.PingObject(_entry.ContextInstanceID);
            }

            EnhancedEditorGUIUtility.MultiSelectionClick(_selection, logs, _index, IsLogSelected, CanSelectLog, SelectLog);

            // Context menu.
            if (EnhancedEditorGUIUtility.ContextClick(_selection)) {
                GenericMenu _menu = new GenericMenu();
                _menu.AddItem(copyLogContentGUI, false, CopySelectedLogContent);
                _menu.AddItem(copyLogStackTraceGUI, false, CopySelectedLogStackTrace);

                _menu.ShowAsContext();
            }

            // Focus.
            if (doFocusSelection && (selectedLogIndex == _index) && (Event.current.type == EventType.Repaint)) {
                logScroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(logScroll, _position, _area.size);
                doFocusSelection = false;

                Repaint();
            }

            return true;
        }

        private void DrawStackTrace() {
            // Request an empty layout to properly draw this area.
            if (selectedLogIndex == -1) {
                GUILayout.Space(10f);
                return;
            }

            LogStack _stack = selectedLogStackTrace;

            // Header.
            {
                GUIStyle _style = Styles.StackHeaderStyle;

                string _header = _stack.Header;
                GUIContent _headerGUI = EnhancedEditorGUIUtility.GetLabelGUI(_header);

                Rect _position = EnhancedEditorGUIUtility.GetControlRect();
                float _height = EnhancedEditorGUI.ManageDynamicControlHeight(_headerGUI, _style.CalcHeight(_headerGUI, _position.width));
                float _scrollHeight = Mathf.Min(_height, EnhancedEditorGUIUtility.CalculLineHeight(_style, StackHeaderMaxLine)) + StackHeaderSpacing;

                using (var _scrollScope = new GUILayout.ScrollViewScope(stackTraceHeaderScroll, GUILayout.Height(_scrollHeight)))
                using (var _horizontalScope = new EditorGUILayout.HorizontalScope(GUILayout.Height(_height))) {
                    // Scroll.
                    stackTraceHeaderScroll = _scrollScope.scrollPosition;

                    // Background.
                    Rect _temp = new Rect(_horizontalScope.rect);
                    EditorGUI.DrawRect(_temp, EnhancedEditorGUIUtility.GUIBarColor);

                    // Label.
                    _position = new Rect(EditorGUILayout.GetControlRect(true, _height)) {
                        x = _position.x,
                        width = _position.width,
                    };

                    EditorGUI.SelectableLabel(_position, _header, _style);
                }
            }

            // Separator.
            // Draw a bit over the header content to leave some space between labels on top of the separator.
            Rect _separatorPosition = GUILayoutUtility.GetRect(position.width, 1f); {
                _separatorPosition.yMin -= StackHeaderSpacing + 1f;
            }

            EditorGUI.DrawRect(_separatorPosition, EnhancedEditorGUIUtility.GUIBarColor);

            _separatorPosition.yMin += StackHeaderSpacing;
            _separatorPosition.height = 1f;

            EditorGUI.DrawRect(_separatorPosition, EnhancedEditorGUIUtility.GUISplitterColor);

            // Stack content.
            using (var _scrollScope = new GUILayout.ScrollViewScope(stackTraceContentScroll)) {
                stackTraceContentScroll = _scrollScope.scrollPosition;

                for (int i = 0; i < _stack.Calls.Count; i++) {

                    // Use a vertical scope for each calls to properly draw a background line.
                    using (var _scope = new EditorGUILayout.VerticalScope()) {
                        LogStackCall _call = _stack.Calls[i];

                        Rect _full = new Rect(_scope.rect) {
                            x = 0f,
                            width = position.width
                        };

                        EnhancedEditorGUI.BackgroundLine(_full, false, i + 1);

                        if (_call.Lines.Count != 0) {
                            GUIStyle _style = Styles.StackPreviewStyle;

                            GUILayout.Space(StackPreviewSpacing);
                            EditorGUILayout.LabelField(_call.Header, _style);
                            GUILayout.Space(2f);

                            foreach (LogStackLine _line in _call.Lines) {
                                Rect _position = EditorGUILayout.GetControlRect(true, StackLineHeight);

                                // Call line background.
                                if (_line.Number == _call.LineNumber) {
                                    _full = new Rect(_position) {
                                        x = 0f,
                                        width = position.width
                                    };

                                    EnhancedEditorGUI.BackgroundLine(_full, true, 0);
                                }

                                EditorGUI.LabelField(_position, _line.Line, _style);

                                if (EnhancedEditorGUIUtility.DoubleClick(_position)) {
                                    InternalEditorUtility.OpenFileAtLineExternal(_call.FilePath, _line.Number);
                                }
                            }

                            GUILayout.Space(StackPreviewSpacing);
                        } else {
                            GUIStyle _style = Styles.StackContentStyle;
                            GUIContent _header = _call.Header;

                            float _height = _style.CalcHeight(_header, EnhancedEditorGUIUtility.GetControlRect().width - StackContentOffset);
                            Rect _position = EditorGUILayout.GetControlRect(true, EnhancedEditorGUI.ManageDynamicControlHeight(_header, _height)); {
                                _position.xMin += StackContentOffset;
                            }

                            EditorGUI.SelectableLabel(_position, _header.text, Styles.StackContentStyle);
                        }
                    }
                }

                GUILayout.Space(2f);
            }
        }

        // -----------------------

        // Utility delegates.
        private bool IsLogSelected(int _index) {
            return logs[_index].IsSelected;
        }

        private bool CanSelectLog(int _index) {
            // Do not select duplicate collapsed logs.
            return logs[_index].IsVisible && (!HasFlag(Flags.Collapse) || !logs[_index].IsDuplicate);
        }

        private void SelectLog(int _index, bool _isSelected) {
            logs[_index].IsSelected = _isSelected;

            if (_isSelected) {
                SelectLog(_index);
                doFocusSelection = true;
            } else {
                SelectLog(logs.FindIndex(l => l.IsSelected));
            }
        }

        // -----------------------

        private void CopySelectedLogContent() {
            if (selectedLogIndex == -1) {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = GetEntry(selectedLogIndex).LogString;
        }

        private void CopySelectedLogStackTrace() {
            if (selectedLogIndex == -1) {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = GetEntry(selectedLogIndex).StackTrace;
        }
        #endregion

        #region Flags
        private bool HasFlag(Flags _flag) {
            return flags.HasFlag(_flag);
        }

        private void SetFlag(Flags _flag, bool _enabled) {
            if (_enabled) {
                flags |= _flag;
            } else {
                flags &= ~_flag;
            }
        }

        private void InvertFlag(Flags _flag) {
            SetFlag(_flag, !HasFlag(_flag));
        }
        #endregion

        #region Native Console
        // -------------------------------------------
        // Log Flags
        // -------------------------------------------

        /// <summary>
        /// Unity native console flags enum.
        /// <br/> See GitHub for more informations.
        /// <para/>
        /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/LogEntries.bindings.cs
        /// </summary>
        [Flags]
        internal enum LogMessageFlags : int {
            kNoLogMessageFlags = 0,

            kError  = 1 << 0, // Message describes an error.
            kAssert = 1 << 1, // Message describes an assertion failure.
            kLog    = 1 << 2, // Message is a general log message.
            kFatal  = 1 << 4, // Message describes a fatal error, and that the program should now exit.

            kAssetImportError   = 1 << 6, // Message describes an error generated during asset importing.
            kAssetImportWarning = 1 << 7, // Message describes a warning generated during asset importing.

            kScriptingError     = 1 << 8, // Message describes an error produced by script code.
            kScriptingWarning   = 1 << 9, // Message describes a warning produced by script code.
            kScriptingLog       = 1 << 10, // Message describes a general log message produced by script code.

            kScriptCompileError     = 1 << 11, // Message describes an error produced by the script compiler.
            kScriptCompileWarning   = 1 << 12, // Message describes a warning produced by the script compiler.

            kStickyLog              = 1 << 13, // Message is 'sticky' and should not be removed when the user manually clears the console window.
            kMayIgnoreLineNumber    = 1 << 14, // The scripting runtime should skip annotating the log callstack with file and line information.
            kReportBug              = 1 << 15, // When used with kFatal, indicates that the log system should launch the bug reporter.

            // The message before this one should be displayed at the bottom of Unity's main window, unless there are no messages before this one.
            kDisplayPreviousErrorInStatusBar = 1 << 16,

            kScriptingException         = 1 << 17, // Message describes an exception produced by script code.
            kDontExtractStacktrace      = 1 << 18, // Stacktrace extraction should be skipped for this message.
            kScriptingAssertion         = 1 << 21, // The message describes an assertion failure in script code.
            kStacktraceIsPostprocessed  = 1 << 22, // The stacktrace has already been postprocessed and does not need further processing.
            kIsCalledFromManaged        = 1 << 23, // The message is being called from managed code.

            // Utilities.
            Log        = kScriptingLog          | kLog,
            Warning    = kScriptingWarning      | kAssetImportWarning |  kScriptCompileWarning,
            Error      = kScriptingError        | kError | kAssetImportError | kScriptCompileError,
            Assert     = kScriptingAssertion    | kAssert,
            Exception  = kScriptingException,
        }

        // -------------------------------------------
        // Behaviour 
        // -------------------------------------------

        private const int MaxNativeLogCount = 20;

        private const BindingFlags StaticFlags      = BindingFlags.Static | BindingFlags.Public;
        private const BindingFlags InstanceFlags    = BindingFlags.Instance | BindingFlags.Public;

        private static readonly Type logEntriesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
        private static readonly Type logEntryType   = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntry");

        private static readonly MethodInfo startGetEntriesMethod    = logEntriesType.GetMethod("StartGettingEntries", StaticFlags);
        private static readonly MethodInfo endGetEntriesMethod      = logEntriesType.GetMethod("EndGettingEntries", StaticFlags);

        private static readonly MethodInfo getEntryMethod           = logEntriesType.GetMethod("GetEntryInternal", StaticFlags);
        private static readonly MethodInfo clearMethod              = logEntriesType.GetMethod("Clear", StaticFlags);

        private static readonly FieldInfo messageField              = logEntryType.GetField("message", InstanceFlags);
        private static readonly FieldInfo modeField                 = logEntryType.GetField("mode", InstanceFlags);
        private static readonly FieldInfo instanceIDIField          = logEntryType.GetField("instanceID", InstanceFlags);
        private static readonly FieldInfo callstackStartField       = logEntryType.GetField("callstackTextStartUTF16", InstanceFlags);

        private static readonly object[] getEntryParams = new object[2];

        // -----------------------

        private void GetNativeConsoleLogs() {
            try {
                // Create a log object to be override be the GetEntry out parameter.
                var _logObject = Activator.CreateInstance(logEntryType);
                int _count = Mathf.Min(MaxNativeLogCount, (int)startGetEntriesMethod.Invoke(null, null));

                for (int i = 0; i < _count; i++) {
                    // Setup params.
                    getEntryParams[0] = i;
                    getEntryParams[1] = _logObject;

                    if (!(bool)getEntryMethod.Invoke(null, getEntryParams)) {
                        continue;
                    }

                    // Get log informations.
                    string _log = (string)messageField.GetValue(_logObject);
                    int _type = (int)modeField.GetValue(_logObject);
                    int _callstackStart = (int)callstackStartField.GetValue(_logObject);

                    string _stackTrace = _log.Substring(_callstackStart + 1).TrimEnd();
                    _log = _log.Substring(0, _callstackStart).TrimEnd();

                    RegisterLog(_log, _stackTrace, GetLogType((LogMessageFlags)_type), true);
                }

                // Notifies that we are stopping getting entries.
                endGetEntriesMethod.Invoke(null, null);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
        
        private void ClearNativeConsole() {
            clearMethod.Invoke(null, null);
        }

        // -------------------------------------------
        // Utility 
        // -------------------------------------------
        
        private LogType GetLogType(LogMessageFlags _flags) {
            if (HasFlag(LogMessageFlags.Exception)) {
                return LogType.Exception;
            }

            if (HasFlag(LogMessageFlags.Error)) {
                return LogType.Error;
            }

            if (HasFlag(LogMessageFlags.Assert)) {
                return LogType.Assert;
            }

            if (HasFlag(LogMessageFlags.Warning)) {
                return LogType.Warning;
            }

            return LogType.Log;

            // ----- Local Methods ----- \\

            bool HasFlag(LogMessageFlags _type) {
                return (_flags & _type) != 0;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Updates pending logs.
        /// </summary>
        private void UpdateLogs() {
            // Disabled debug.
            if (HasFlag(Flags.Disabled)) {
                pendingLogs.Clear();
                return;
            }

            if (pendingLogs.Count == 0) {
                return;
            }

            for (int i = 0; i < pendingLogs.Count; i++) {
                LogWrapper _log = pendingLogs[i];

                // Get entry.
                EnhancedLogger.GetLogContextInstanceID(ref _log.Log, out int _instanceID);

                // Ignore duplicate logs.
                if (_log.IsNative && logs.Exists(l => l.IsDuplicate || (l.GetEntry(logs).StackTrace == _log.StackTrace))) {
                    continue;
                }

                LogEntry _entry = GetLogEntry(_log, _instanceID);
                logs.Add(_entry);

                SetLogFilter(_entry, EnhancedConsoleEnhancedSettings.Settings);
                FilterLog(_entry);

                // Editor pause on error.
                if (HasFlag(Flags.ErrorPause) && Application.isPlaying) {
                    switch (_log.Type) {
                        case LogType.Error:
                        case LogType.Assert:
                        case LogType.Exception:
                            Debug.Break();
                            break;

                        default:
                            break;
                    }
                }
            }

            // Save content.
            string _json = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(EditorPrefKey, _json);

            pendingLogs.Clear();
            Repaint();

            // ----- Local Method ----- \\

            LogEntry GetLogEntry(LogWrapper _log, int _instanceID) {
                int _count = logs.Count;

                for (int i = 0; i < _count; i++) {
                    // Duplicate entry.
                    if (logs[i].AddDuplicateLog(_log, _instanceID, _count)) {
                        return new DuplicateLogEntry(_log, i);
                    }
                }

                // New original entry.
                return new OriginalLogEntry(_log, _instanceID);
            }
        }

        /// <summary>
        /// Refreshes all log filters.
        /// </summary>
        internal void RefreshFilters() {
            EnhancedConsoleEnhancedSettings _preferences = EnhancedConsoleEnhancedSettings.Settings;

            foreach (LogEntry _log in logs) {
                SetLogFilter(_log, _preferences);
            }

            FilterLogs();
            Repaint();
        }

        /// <summary>
        /// Sorts the log columns in the preferences order.
        /// </summary>
        internal void SortColumns(EnhancedConsoleEnhancedSettings _preferences) {
            Array.Sort(logColumns, (a, b) => {
                int _aIndex = Array.IndexOf(_preferences.Columns.Array, a.Column);
                int _bIndex = Array.IndexOf(_preferences.Columns.Array, b.Column);

                return _aIndex.CompareTo(_bIndex);
            });
        }

        /// <summary>
        /// Get the full log entry of the log at a specific index.
        /// </summary>
        private OriginalLogEntry GetEntry(int _index) {
            return logs[_index].GetEntry(logs);
        }

        /// <summary>
        /// Selects the log at a specific _index.
        /// </summary>
        /// <param name="_index">Index of the log to select.</param>
        private void SelectLog(int _index) {
            selectedLogIndex = _index;

            if ((selectedLogIndex != -1) && GetEntry(selectedLogIndex) != selectedLogStackTrace.Entry) {
                RefreshStackTrace();
            }
        }

        /// <summary>
        /// Refreshes the selected log stack trace content.
        /// </summary>
        private void RefreshStackTrace() {
            if (selectedLogIndex != -1) {
                selectedLogStackTrace.Setup(GetEntry(selectedLogIndex));
            }
        }

        /// <summary>
        /// CustomFilters a specific log.
        /// </summary>
        private void FilterLog(LogEntry _log) {
            string _filter = searchFilter;
            var _entry = _log.GetEntry(logs);

            _log.IsVisible = string.IsNullOrEmpty(_filter) || _entry.LogString.ToLower().Contains(_filter.ToLower());
        }

        /// <summary>
        /// CustomFilters all logs.
        /// </summary>
        private void FilterLogs() {
            string _filter = searchFilter.ToLower();

            if (string.IsNullOrEmpty(_filter)) {
                foreach (LogEntry _log in logs) {
                    _log.IsVisible = true;
                }
            } else {
                foreach (LogEntry _log in logs) {
                    var _entry = _log.GetEntry(logs);
                    _log.IsVisible = _entry.LogString.ToLower().Contains(_filter);
                }
            }

            // Focus on change.
            doFocusSelection = true;
        }

        /// <summary>
        /// Get and associate the best <see cref="ConsoleLogFilter"/> for a specific <see cref="LogEntry"/>.
        /// </summary>
        private void SetLogFilter(LogEntry _entry, EnhancedConsoleEnhancedSettings _preferences) {
            if (_entry.IsDuplicate) {
                return;
            }

            OriginalLogEntry _logEntry = _entry.GetEntry(logs);
            _logEntry.Filter = _preferences.GetBestFilter(_logEntry);
        }

        /// <summary>
        /// Deletes the log at a specific index.
        /// </summary>
        private void DeleteLog(int _index) {
            LogEntry _entry = logs[_index];
            OriginalLogEntry _origin = _entry.GetEntry(logs);

            bool _deleteDupplicate = HasFlag(Flags.Collapse);
            int _lastIndex = _origin.LastDuplicateIndex;

            if (!_entry.IsDuplicate) {
                // Change the original entry when deleting it.
                if (_origin.DuplicateCount != 0) {
                    int _originalIndex = -1;

                    for (int i = _index + 1; i <= _lastIndex; i++) {
                        LogEntry _log = logs[i];

                        // Duplicate update.
                        if (_log.IsDuplicate && (_log.GetEntry(logs) == _origin)) {
                            if (_deleteDupplicate) {
                                Delete(i);

                                i--;
                                _lastIndex--;

                                continue;
                            }

                            if (_originalIndex == -1) {
                                logs[i] = new OriginalLogEntry(_origin, _log);
                                _originalIndex = i;
                            } else {
                                _log.SetOriginalLog(_originalIndex);
                            }
                        }
                    }
                }
            } else {
                // Update duplicate informations.
                _origin.DuplicateCount--;

                if (_lastIndex == _index) {
                    for (int i = _index; i-- > 0;) {
                        LogEntry _log = logs[i];

                        if (_log == _origin) {
                            _origin.LastDuplicateIndex = -1;
                            break;
                        }

                        if (_log.GetEntry(logs) == _origin) {
                            _origin.LastDuplicateIndex = i;
                            break;
                        }
                    }
                }
            }

            Delete(_index);
            SelectLog(-1);

            Repaint();

            // ----- Local Methods ----- \\

            void Delete(int _index) {
                logs.RemoveAt(_index);

                // Delete callback.
                for (int i = 0; i < logs.Count; i++) {
                    logs[i].OnDeleteLog(_index);
                }
            }
        }

        /// <summary>
        /// Registers this window callbacks.
        /// </summary>
        private void RegisterCallbacks() {
            UnregisterCallbacks();

            // The console might be already registered (uses a static method),
            // so unregister it before as a security.
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update               += OnUpdate;
        }

        /// <summary>
        /// Unregisters this window callbacks.
        /// </summary>
        private void UnregisterCallbacks() {
            // The console window might be destroyed when maximazing another window,
            // so keep registered on logs independently.
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update               -= OnUpdate;
        }

        /// <summary>
        /// Clears the console content and reset its capacity back to default.
        /// </summary>
        private void Clear() {
            logs.Clear();
            logs.Capacity = DefaultLogCapacity;

            SelectLog(-1);

            if (!HasFlag(Flags.Disabled)) {
                ClearNativeConsole();
            }

            GetNativeConsoleLogs();
        }
        #endregion
    }
}
