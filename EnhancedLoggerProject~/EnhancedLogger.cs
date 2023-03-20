// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  This class aims to replace and use the Unity standard logger.
//  Based on the Debug & Logger original classes:
//
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Debug/Debug.bindings.cs
//  https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Logging/Logger.cs
//
// ============================================================================ //

using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// Enhanced <see cref="ILogger"/> class,
    /// aiming the replace the original Unity logger used for debug.
    /// </summary>
    public class EnhancedLogger : ILogger {
        #region Global Members
        private static readonly FieldInfo defaultLoggerField = typeof(Debug).GetField("s_Logger", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly ILogger defaultLogger = defaultLoggerField.GetValue(null) as ILogger;

        public static readonly EnhancedLogger Logger = new EnhancedLogger();

        // -----------------------

        public ILogHandler logHandler {
            get {
                return defaultLogger.logHandler;
            }
            set {
                defaultLogger.logHandler = value;
            }
        }

        public bool logEnabled {
            get {
                return defaultLogger.logEnabled;
            }
            set {
                defaultLogger.logEnabled = value;
            }
        }

        public LogType filterLogType {
            get {
                return defaultLogger.filterLogType;
            }
            set {
                defaultLogger.filterLogType = value;
            }
        }

        // -----------------------

        /// <summary>
        /// Prevents from creating new instances.
        /// </summary>
        private EnhancedLogger() { }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes and enables the enhanced logger.
        /// </summary>
        public static void Initialize() {
            defaultLoggerField.SetValue(null, Logger);
            EnableLogger(true);
        }
        #endregion

        #region Logger
        // --- Generic --- \\

        void ILogger.Log(LogType _logType, object _message) {
            defaultLogger.Log(_logType, _message);
        }

        void ILogger.Log(LogType _logType, object _message, Object _context) {
            RegisterContextObject(ref _message, _context);
            defaultLogger.Log(_logType, _message, _context);
        }

        void ILogger.Log(LogType _logType, string _tag, object _message) {
            defaultLogger.Log(_logType, _tag, _message);
        }

        void ILogger.Log(LogType _logType, string _tag, object _message, Object _context) {
            RegisterContextObject(ref _message, _context);
            defaultLogger.Log(_logType, _tag, _message, _context);
        }

        // --- Log --- \\

        void ILogger.Log(object _message) {
            defaultLogger.Log(_message);
        }

        void ILogger.Log(string _tag, object _message) {
            defaultLogger.Log(_tag, _message);
        }

        void ILogger.Log(string _tag, object _message, Object _context) {
            RegisterContextObject(ref _message, _context);
            defaultLogger.Log(_tag, _message, _context);
        }

        // --- Log Warning --- \\

        void ILogger.LogWarning(string _tag, object _message) {
            defaultLogger.LogWarning(_tag, _message);
        }

        void ILogger.LogWarning(string _tag, object _message, Object _context) {
            RegisterContextObject(ref _message, _context);
            defaultLogger.LogWarning(_tag, _message, _context);
        }

        // --- Log Error --- \\

        void ILogger.LogError(string _tag, object _message) {
            defaultLogger.LogError(_tag, _message);
        }

        void ILogger.LogError(string _tag, object _message, Object _context) {
            RegisterContextObject(ref _message, _context);
            defaultLogger.LogError(_tag, _message, _context);
        }

        // --- Log Exception --- \\

        void ILogger.LogException(Exception _exception) {
            defaultLogger.LogException(_exception);
        }

        void ILogHandler.LogException(Exception _exception, Object _context) {
            defaultLogger.LogException(_exception, _context);
        }

        // --- Log Format --- \\

        void ILogger.LogFormat(LogType _logType, string _format, params object[] _args) {
            defaultLogger.LogFormat(_logType, _format, _args);
        }

        void ILogHandler.LogFormat(LogType _logType, Object _context, string _format, params object[] _args) {
            if (_args.Length != 0) {
                RegisterContextObject(ref _args[0], _context);
            }

            defaultLogger.LogFormat(_logType, _context, _format, _args);
        }
        #endregion

        #region Utility
        private const string NullMessage = "Null";
        private const string TextBeforeContext = " (id:";
        private const string TextAfterContext = ")";

        // -----------------------

        /// <inheritdoc cref="ILogger.IsLogTypeAllowed(LogType)"/>
        public bool IsLogTypeAllowed(LogType _logType) {
            if (logEnabled) {
                if (_logType == LogType.Exception) {
                    return true;
                }

                if (filterLogType != LogType.Exception) {
                    return _logType <= filterLogType;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the instance id of the context object associated with a specific log.
        /// </summary>
        /// <param name="_log">The log to get the associated context object instance id.</param>
        /// <param name="_instanceID">The instance id of this log context object (-1 if none).</param>
        /// <returns>True if this log is associated with a context object that could be found, false otherwise.</returns>
        public static bool GetLogContextInstanceID(ref string _log, out int _instanceID) {
            int _contextIndex = _log.IndexOf(TextBeforeContext);

            if (_contextIndex != -1) {
                int _contextStartIndex = _contextIndex + TextBeforeContext.Length;
                string _contextString = _log.Substring(_contextStartIndex);

                int _contextEndIndex = _contextString.IndexOf(TextAfterContext);
                _log = _log.Substring(0, _contextIndex);

                if ((_contextEndIndex != -1) && int.TryParse(_contextString.Substring(0, _contextEndIndex), out _instanceID)) {
                    return true;
                }
            }

            _instanceID = -1;
            return false;
        }

        private static void RegisterContextObject(ref object _message, Object _context) {
            string _string = GetString(_message);

            if (!_string.Contains(TextBeforeContext) && (_context != null)) {
                _message = $"{_string}{TextBeforeContext}{_context.GetInstanceID()}{TextAfterContext}";
            }
        }

        private static string GetString(object _message) {
            if (_message == null) {
                return NullMessage;
            }

            if (_message is IFormattable _formattable) {
                return _formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return _message.ToString();
        }

        // -----------------------

        /// <summary>
        /// Enabled/Disabled the <see cref="UnityObjectLogger"/> logs.
        /// </summary>
        /// <param name="_enabled">Whether to enable or disable logs.</param>
        public static void EnableLogger(bool _enabled) {
            UnityObjectLogger.Enabled = _enabled;
        }

        /// <summary>
        /// Get the default <see cref="string"/> format used to log messages for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="_type">The object <see cref="Type"/> this log comes from.</param>
        /// <param name="_color">Color used to identify this log.</param>
        /// <returns>This <see cref="Type"/> associated log format.</returns>
        public static string GetMessageFormat(Type _type, Color _color) {
            return $"<b><size=13><color=#{ColorUtility.ToHtmlStringRGBA(_color)}>{_type.Name}</color></size></b>  " +
                   $"<size=10><color=#{ColorUtility.ToHtmlStringRGBA(new Color(.5f, .5f, .5f, 1f))}>▶</color></size>  " +
                   $"{{0}}";
        }
        #endregion
    }
}
