// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="Object"/>-related extensions class, used to dynamically
    /// log a message to the console with the associated object instance as context.
    /// </summary>
    public static class UnityObjectLogger {
        // --- Logs --- \\

        #region Log
        /// <summary>
        /// Logs a message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        public static void Log(this Object _object, object _message) {
            Debug.Log(_message, _object);
        }

        /// <summary>
        /// Logs a formatted message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        public static void LogFormat(this Object _object, string _format, params object[] _message) {
            Debug.LogFormat(_object, _format, _message);
        }
        #endregion

        #region Warning
        /// <summary>
        /// Logs a warning message to the Unity Console
        /// from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        public static void LogWarning(this Object _object, object _message) {
            Debug.LogWarning(_message, _object);
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity Console
        /// from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        public static void LogWarningFormat(this Object _object, string _format, params object[] _message) {
            Debug.LogWarningFormat(_object, _format, _message);
        }
        #endregion

        #region Error
        /// <summary>
        /// Logs an error message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        public static void LogError(this Object _object, object _message) {
            Debug.LogError(_message, _object);
        }

        /// <summary>
        /// Logs a formatted error message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        public static void LogErrorFormat(this Object _object, string _format, params object[] _message) {
            Debug.LogErrorFormat(_object, _format, _message);
        }
        #endregion

        #region Assertion
        /// <summary>
        /// Logs an assertion message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        public static void LogAssertion(this Object _object, object _message) {
            Debug.LogAssertion(_message, _object);
        }

        /// <summary>
        /// Logs a formatted assertion message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        public static void LogAssertionFormat(this Object _object, string _format, params object[] _message) {
            Debug.LogAssertionFormat(_object, _format, _message);
        }

        // -----------------------

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        public static void Assert(this Object _object, bool _condition) {
            Debug.Assert(_condition, _object);
        }

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        public static void Assert(this Object _object, bool _condition, object _message) {
            Debug.Assert(_condition, _message, _object);
        }

        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        public static void Assert(this Object _object, bool _condition, string _message) {
            Debug.Assert(_condition, _message, _object);
        }

        /// <summary>
        /// Assert a condition and logs a formated error message from this object to the Unity console on failure.
        /// </summary>
        /// <param name="_message"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_message']"/></param>
        /// <param name="_format"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_format']"/></param>
        /// <inheritdoc cref="Doc(Object, bool, string, string)"/>
        public static void AssertFormat(this Object _object, bool _condition, string _format, params object[] _message) {
            Debug.AssertFormat(_condition, _object, _format, _message);
        }
        #endregion

        #region Exception
        /// <summary>
        /// Logs an exception message to the Unity Console from this object.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="Doc(Object, string, string)" path="/param[@name='_object']"/></param>
        /// <param name="_exception">Runtime exception to display.</param>
        public static void LogException(this Object _object, Exception _exception) {
            Debug.LogException(_exception, _object);
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
        #endregion
    }
}
