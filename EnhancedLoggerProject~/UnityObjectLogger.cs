// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Diagnostics;
using UnityEngine;

using Debug     = UnityEngine.Debug;
using Object    = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// Interface to inherit any object to customize its log messages format.
    /// <br/> Use <see cref="EnhancedLogger.GetMessageFormat(Type, Color)"/> to get a default format.
    /// </summary>
    public interface IMessageLogger {
        #region Content
        /// <summary>
        /// Get the <see cref="string"/> format to use for this object log message.
        /// </summary>
        /// <param name="_type">The <see cref="LogType"/> to get the associated format.</param>
        /// <returns>The format of this object log message.</returns>
        string GetLogMessageFormat(LogType _type);
        #endregion
    }

    /// <summary>
    /// <see cref="Object"/>-related extensions class, used to dynamically
    /// log a message to the console with the associated object instance as context.
    /// </summary>
    public static class UnityObjectLogger {
        #region Global Members
        /// <summary>
        /// Indicates whether logs are enabled or not.
        /// </summary>
        public static bool Enabled = false;
        #endregion

        // --- Logs --- \\

        #region Log
        /// <summary>
        /// Logs a message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void Log(this Object _object, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.Log(_message, _object);
        }

        /// <summary>
        /// Logs a formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogFormat(this Object _object, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_object, LogType.Log, _format, _message);
        }

        /// <summary>
        /// Logs a special formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogMessage<T>(this T _object, object _message) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Log, _message);
        }

        /// <summary>
        /// Logs a special formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_context"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_context']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogMessage<T>(this T _object, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Log, _message, _context);
        }
        #endregion

        #region Warning
        /// <summary>
        /// Logs a warning message to the Unity Console
        /// from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogWarning(this Object _object, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.LogWarning(_message, _object);
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity Console
        /// from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogWarningFormat(this Object _object, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_object, LogType.Warning, _format, _message);
        }

        /// <summary>
        /// Logs a special formatted warning message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogWarningMessage<T>(this T _object, object _message) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Warning, _message);
        }

        /// <summary>
        /// Logs a special formatted warning message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_context"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_context']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogWarningMessage<T>(this T _object, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Warning, _message, _context);
        }
        #endregion

        #region Error
        /// <summary>
        /// Logs an error message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogError(this Object _object, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.LogError(_message, _object);
        }

        /// <summary>
        /// Logs a formatted error message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogErrorFormat(this Object _object, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_object, LogType.Error, _format, _message);
        }

        /// <summary>
        /// Logs a special formatted error message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogErrorMessage<T>(this T _object, object _message) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Error, _message);
        }

        /// <summary>
        /// Logs a special formatted error warning message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_context"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_context']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogErrorMessage<T>(this T _object, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Error, _message, _context);
        }
        #endregion

        #region Assertion
        /// <summary>
        /// Logs an assertion message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogAssertion(this Object _object, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.LogAssertion(_message, _object);
        }

        /// <summary>
        /// Logs a formatted assertion message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogAssertionFormat(this Object _object, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_object, LogType.Assert, _format, _message);
        }

        /// <summary>
        /// Logs a special formatted assertion message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogAssertionMessage<T>(this T _object, object _message) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Assert, _message);
        }

        /// <summary>
        /// Logs a special formatted assertion warning message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_context"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_context']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogAssertionMessage<T>(this T _object, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogMessage(_object, LogType.Assert, _message, _context);
        }

        // -----------------------

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        [Conditional("ENHANCED_LOGS")]
        public static void Assert(this Object _object, bool _condition) {
            if (!Enabled) {
                return;
            }

            Debug.Assert(_condition, _object);
        }

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        [Conditional("ENHANCED_LOGS")]
        public static void Assert(this Object _object, bool _condition, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.Assert(_condition, _message, _object);
        }

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        [Conditional("ENHANCED_LOGS")]
        public static void Assert(this Object _object, bool _condition, string _message) {
            if (!Enabled) {
                return;
            }

            Debug.Assert(_condition, _message, _object);
        }

        /// <summary>
        /// Assert a condition and logs a formated error message from this object to the Unity console on failure.
        /// </summary>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        [Conditional("ENHANCED_LOGS")]
        public static void AssertFormat(this Object _object, bool _condition, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            Debug.AssertFormat(_condition, _object, _format, _message);
        }
        #endregion

        #region Exception
        /// <summary>
        /// Logs an exception message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_exception">Runtime exception to display.</param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogException(this Object _object, Exception _exception) {
            if (!Enabled) {
                return;
            }

            Debug.LogException(_exception, _object);
        }
        #endregion

        #region Generic
        /// <summary>
        /// Logs a message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_type"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_type']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void Log(this Object _object, LogType _type, object _message) {
            if (!Enabled) {
                return;
            }

            Debug.unityLogger.Log(_type, _message, _object);
        }

        /// <summary>
        /// Logs a formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_type"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_type']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogFormat(this Object _object, LogType _type, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            Debug.unityLogger.LogFormat(_type, _object, _format, _message);
        }

        /// <summary>
        /// Logs a special formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_type"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_type']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogMessage<T>(this T _object, LogType _type, object _message) {
            if (!Enabled) {
                return;
            }

            Object _context = (_object is Object _unityObject)
                            ? _unityObject
                            : null;

            DoLogFormat(_context, _type, GetMessageFormat(_object, _type), _message);
        }

        /// <summary>
        /// Logs a special formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_type"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_type']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_context"><inheritdoc cref="Doc(Object, LogType)" path="/param[@name='_context']"/></param>
        [Conditional("ENHANCED_LOGS")]
        public static void LogMessage<T>(this T _object, LogType _type, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_context, _type, GetMessageFormat(_object, _type), _message);
        }
        #endregion

        #region Internal
        /// <inheritdoc cref="LogFormat(Object, LogType, string, object[])"/>
        private static void DoLogFormat(this Object _object, LogType _type, string _format, params object[] _message) {
            if (!Enabled) {
                return;
            }

            Debug.unityLogger.LogFormat(_type, _object, _format, _message);
        }

        /// <inheritdoc cref="LogMessage{T}(T, LogType, object)"/>
        private static void DoLogMessage<T>(this T _object, LogType _type, object _message) {
            if (!Enabled) {
                return;
            }

            Object _context = (_object is Object _unityObject)
                            ? _unityObject
                            : null;

            DoLogFormat(_context, _type, GetMessageFormat(_object, _type), _message);
        }

        /// <inheritdoc cref="LogMessage{T}(T, LogType, object, Object)"/>
        private static void DoLogMessage<T>(this T _object, LogType _type, object _message, Object _context) {
            if (!Enabled) {
                return;
            }

            DoLogFormat(_context, _type, GetMessageFormat(_object, _type), _message);
        }

        // -----------------------

        private static string GetMessageFormat<T>(T _object, LogType _type) {
            if (_object is IMessageLogger _logger) {
                return _logger.GetLogMessageFormat(_type);
            }

            return EnhancedLogger.GetMessageFormat(_object.GetType(), Color.white);
        }
        #endregion

        // --- Documentation --- \\

        #region Documentation
        /// <summary>
        /// Documentation method.
        /// </summary>
        /// <param name="_object">Object to which the message applies.</param>
        /// <param name="_message">String or object to be converted to string representation to display.</param>
        /// <param name="_format">A composite format string..</param>
        private static void Doc(this Object _object, string _message, string _format) { }

        /// <summary>
        /// Assert a condition and logs an error message from this object to the Unity console on failure.
        /// </summary>
        /// <param name="_condition">Condition you expect to be true.</param>
        /// <inheritdoc cref="Doc(Object, string, string)"/>
        private static void Doc(this Object _object, bool _condition, string _message, string _format) { }

        /// <summary>
        /// Documentation method.
        /// </summary>
        /// <param name="_context">Context object of the message.</param>
        /// <param name="_type">Type of the message to log.</param>
        private static void Doc(this Object _context, LogType _type) { }
        #endregion
    }
}
