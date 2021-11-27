// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple <see cref="MemberValue{T}"/>-related extension methods.
    /// </summary>
	public static class MemberValueExtensions
    {
        #region Content
        /// <summary>
        /// Get the value of this member from a specific <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_member"><see cref="MemberValue{T}"/> to get value.</param>
        /// <param name="_serializedObject"><see cref="SerializedObject"/> to get member value from.</param>
        /// <inheritdoc cref="MemberValue{T}.GetValue(object, Type, out T)"/>
        public static bool GetValue<T>(this MemberValue<T> _member, SerializedObject _serializedObject, out T _value)
        {
            Object _object = _serializedObject.targetObject;
            Type _objectType = _object.GetType();

            return _member.GetValue(_object, _objectType, out _value);
        }

        /// <summary>
        /// Set the value of this member in each target object of a <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_member"><see cref="MemberValue{T}"/> to set value.</param>
        /// <param name="_serializedObject"><see cref="SerializedObject"/> to set member value.</param>
        /// <returns>True if this member value was successfully set in each target object, false otherwise.</returns>
        /// <inheritdoc cref="MemberValue{T}.SetValue(object, Type, T)"/>
        public static bool SetValue<T>(this MemberValue<T> _member, SerializedObject _serializedObject, T _value)
        {
            bool _success = true;
            Type _objectType = _serializedObject.targetObject.GetType();

            foreach (Object _object in _serializedObject.targetObjects)
            {
                if (!_member.SetValue(_object, _objectType, _value))
                {
                    _success = false;
                }
            }

            return _success;
        }
        #endregion
    }
}
