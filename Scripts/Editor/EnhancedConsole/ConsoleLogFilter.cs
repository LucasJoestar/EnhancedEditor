// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

using LogColumnType = EnhancedEditor.Editor.EnhancedConsoleWindow.LogColumnType;
using LogEntry      = EnhancedEditor.Editor.EnhancedConsoleWindow.OriginalLogEntry;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Enum used to reference and load an icon for a <see cref="ConsoleLogFilter"/>.
    /// </summary>
    public enum ConsoleLogFilterIcon {
        None = 0,
        Apple,
        Banana,
        Blackberry,
        Blueberry,
        Eggplant,
        Grape,
        Grenade,
        Grey,
        Lemon,
        Peach,
        Pumpkin,
        Radish,
        Strawberry,
        Water,
        Watermelon
    }

    // ===== Base ===== \\

    /// <summary>
    /// <see cref="EnhancedConsoleWindow"/>-related base log filter class.
    /// </summary>
    [Serializable]
    public abstract class ConsoleLogFilter {
        #region Global Members
        [SerializeField, HideInInspector] public string Name    = "New Filter";
        [SerializeField, HideInInspector] public bool Enabled   = true;
        [SerializeField, HideInInspector] public bool IsVisible = true;

        [Space(5f)]

        [Tooltip("Color used for this filter logs background")]
        [Enhanced, Duo(nameof(UseColor), EnhancedEditorGUIUtility.IconWidth)]
        public Color Color = new Color(.15f, .6f, .15f, .7f);

        [Tooltip("Color used for this filter logs text")]
        [Enhanced, Duo(nameof(UseTextColor), EnhancedEditorGUIUtility.IconWidth)]
        public Color TextColor = Color.white;

        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseColor     = false;
        [SerializeField, HideInInspector, DisplayName("Enabled")] public bool UseTextColor = false;

        [Space(10f)]

        [Tooltip("This filter displayed icon")]
        public Texture Icon = null;

        [SerializeField, Enhanced, DisplayName("Quick Loader"), ValidationMember(nameof(FilterIcon))]
        protected ConsoleLogFilterIcon filterIcon = ConsoleLogFilterIcon.None;

        [Space(10f)]

        [Tooltip("When enabled, this filter icon remains displayed in the console toolbar even if no associated log is found")]
        [Enhanced, DisplayName("Pin Filter")] public bool DoPinFilter = true;

        // -----------------------

        [NonSerialized] public int DisplayedCount = 0;

        public ConsoleLogFilterIcon FilterIcon {
            get { return filterIcon; }
            set {
                filterIcon = value;
                if (value != ConsoleLogFilterIcon.None) {
                    Icon = value.Get();
                }
            }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Get if this filter matches a specific <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="_entry">The entry to check.</param>
        /// <returns>True if this filter matches this entry, false otherwise.</returns>
        internal abstract bool Match(LogEntry _entry);
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// <see cref="EnhancedConsoleWindow"/>-related default log filter class.
    /// </summary>
    [Serializable]
    public sealed class DefaultConsoleLogFilter : ConsoleLogFilter {
        /// <summary>
        /// Defines all default filter types.
        /// </summary>
        public enum Type {
            Log         = 0,
            Warning     = 1,
            Error       = 2,
            Assert      = 3,
            Exception   = 4,
            External    = 5,
            Compilation = 6,
        }

        #region Global Members
        [SerializeField, HideInInspector] public Type LogType = Type.Log;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="DefaultConsoleLogFilter"/>
        public DefaultConsoleLogFilter(Type _type) {
            LogType = _type;
        }
        #endregion

        #region Behaviour
        internal override bool Match(LogEntry _entry) {
            if (!Enabled) {
                return false;
            }

            switch (LogType) {
                case Type.Log:
                    return _entry.Type == FlagLogType.Log;

                case Type.Warning:
                    return _entry.Type == FlagLogType.Warning;

                case Type.Error:
                    return _entry.Type == FlagLogType.Error;

                case Type.Assert:
                    return _entry.Type == FlagLogType.Assert;

                case Type.Exception:
                    return _entry.Type == FlagLogType.Exception;

                case Type.External:
                    return (_entry.LineNumber == -1)
                        && ((_entry.Type == FlagLogType.Log) || (_entry.Type == FlagLogType.Warning));

                case Type.Compilation:
                    bool _isError = (_entry.Type != FlagLogType.Log) && (_entry.Type != FlagLogType.Warning);
                    return _isError && string.IsNullOrEmpty(_entry.StackTrace);

                default:
                    break;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EnhancedConsoleWindow"/>-related custom log filter class,
    /// </summary>
    [Serializable]
    public sealed class CustomConsoleLogFilter : ConsoleLogFilter {
        /// <summary>
        /// <see cref="CustomConsoleLogFilter"/> keyword wrapper class.
        /// </summary>
        [Serializable]
        public sealed class Keyword {
            #region Global Members
            [Tooltip("The keyword to search for")]
            [Enhanced, DisplayName("Keyword")] public string Value = "Keyword";

            [Space(5f)]

            [Tooltip("The types of log to search in"), Enhanced, DisplayName("Search in Log(s)")]
            public FlagLogType Logs = ~FlagLogType.None;

            [Tooltip("The log columns to search in"), Enhanced, DisplayName("Search in Column(s)")]
            public LogColumnType Columns = LogColumnType.Log;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            /// <inheritdoc cref="Keyword(string, FlagLogType, LogColumnType)"/>
            public Keyword() : this("Keyword", ~FlagLogType.None, LogColumnType.Log) { }

            /// <param name="_keyword"><inheritdoc cref="Keyword" path="/summary"/></param>
            /// <param name="_logs"><inheritdoc cref="Logs" path="/summary"/></param>
            /// <param name="_columns"><inheritdoc cref="Columns" path="/summary"/></param>
            /// <inheritdoc cref="Keyword"/>
            public Keyword(string _keyword, FlagLogType _logs, LogColumnType _columns) {
                Value   = _keyword;
                Logs    = _logs;
                Columns = _columns;
            }
            #endregion
        }

        #region Global Members
        [Space(10f)]

        public BlockArray<Keyword> Keywords = new BlockArray<Keyword>() { new Keyword() };
        #endregion

        #region Behaviour
        private static readonly LogColumnType[] columnValues = (LogColumnType[])Enum.GetValues(typeof(LogColumnType));

        // -----------------------

        internal override bool Match(LogEntry _entry) {
            if (!Enabled) {
                return false;
            }

            ref Keyword[] _span = ref Keywords.Array;
            for (int i = _span.Length; i-- > 0;) {

                Keyword _keyword = _span[i];
                if (!_keyword.Logs.HasFlagUnsafe(_entry.Type)) {
                    continue;
                }

                string _value = _keyword.Value.ToLower();
                for (int j = columnValues.Length; j-- > 0;) {

                    if (Match(_entry, _keyword, _value, columnValues[j])) {
                        return true;
                    }
                }
            }

            return false;

            // ----- Local Method ----- \\

            static bool Match(LogEntry _entry, Keyword _keyword, string _value, LogColumnType _column) {

                if (!_keyword.Columns.HasFlagUnsafe(_column)) {
                    return false;
                }

                string _search = string.Empty;

                switch (_column) {
                    case LogColumnType.Type:
                        _search = _entry.Type.ToString();
                        break;

                    case LogColumnType.Namespace:
                        _search = _entry.Namespace;
                        break;

                    case LogColumnType.Class:
                        _search = _entry.Class;
                        break;

                    case LogColumnType.Method:
                        _search = _entry.Method;
                        break;

                    case LogColumnType.Log:
                        _search = _entry.LogString;
                        break;

                    case LogColumnType.Object:
                        _search = _entry.ContextObjectName.ToString();
                        break;

                    case LogColumnType.File:
                        _search = _entry.File;
                        break;

                    case LogColumnType.Frame:
                        _search = _entry.Frame;
                        break;

                    case LogColumnType.Timestamp:
                        _search = _entry.Timestamp;
                        break;

                    case LogColumnType.Icon:
                    default:
                        break;
                }

                return !string.IsNullOrEmpty(_search) && _search.ToLower().Contains(_value);
            }
        }
        #endregion
    }

    // ===== Extensions ===== \\

    /// <summary>
    /// Contains multiple <see cref="ConsoleLogFilterIcon"/>-related extension methods.
    /// </summary>
    public static class ConsoleLogFilterIconExtensions {
        #region Content
        /// <summary>
        /// Get this <see cref="ConsoleLogFilterIcon"/> associated <see cref="Texture"/> icon .
        /// </summary>
        /// <param name="_icon">The icon to load the associated <see cref="Texture"/>.</param>
        /// <returns>The loaded <see cref="Texture"/> associated with this icon (null if none).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture Get(this ConsoleLogFilterIcon _icon) {
            if (_icon == ConsoleLogFilterIcon.None) {
                return null;
            }

            string _path = $"Console/LogFilter_{_icon}";
            return Resources.Load<Texture>(_path);
        }
        #endregion
    }
}
