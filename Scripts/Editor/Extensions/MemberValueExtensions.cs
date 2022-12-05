// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Contains multiple <see cref="MemberValue{T}"/>-related extension methods.
    /// </summary>
	public static class MemberValueExtensions {
        #region Content
        /// <summary>
        /// Call this member from a specific <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_member"><see cref="MemberValue{T}"/> to call.</param>
        /// <param name="_serializedObject">The <see cref="SerializedObject"/> of this member.</param>
        /// <inheritdoc cref="MemberValue{T}.Call(object, Type)"/>
        public static bool Call<T>(this MemberValue<T> _member, SerializedObject _serializedObject) {
            object _object = _serializedObject.targetObject;
            Type _objectType = _object.GetType();

            return _member.Call(_object, _objectType);
        }

        /// <param name="_serializedObject">The <see cref="SerializedObject"/> of this member.</param>
        /// <inheritdoc cref="GetValue{T}(MemberValue{T}, SerializedProperty, out T)"/>
        public static bool GetValue<T>(this MemberValue<T> _member, SerializedObject _serializedObject, out T _value) {
            object _object = _serializedObject.targetObject;
            Type _objectType = _object.GetType();

            return _member.GetValue(_object, _objectType, out _value);
        }

        /// <summary>
        /// Get the value of this member from a specific <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_member"><see cref="MemberValue{T}"/> to get value.</param>
        /// <param name="_serializedProperty">The <see cref="SerializedProperty"/> of the variable stored in the same object instance as this member.</param>
        /// <inheritdoc cref="MemberValue{T}.GetValue(object, Type, out T)"/>
        public static bool GetValue<T>(this MemberValue<T> _member, SerializedProperty _serializedProperty, out T _value) {
            object _object = _serializedProperty.serializedObject.targetObject;
            Type _objectType = _object.GetType();

            return _member.GetValue(_object, _objectType, out _value) || (GetNestedField(_serializedProperty, ref _objectType, ref _object) && _member.GetValue(_object, _objectType, out _value));
        }

        /// <summary>
        /// Set the value of this member in each target object of a <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="_member"><see cref="MemberValue{T}"/> to set value.</param>
        /// <param name="_serializedProperty">The <see cref="SerializedProperty"/> of the variable stored in the same object instance as this member.</param>
        /// <returns>True if this member value was successfully set in each target object, false otherwise.</returns>
        /// <inheritdoc cref="MemberValue{T}.SetValue(object, Type, T)"/>
        public static bool SetValue<T>(this MemberValue<T> _member, SerializedProperty _serializedProperty, T _value) {
            bool _success = true;

            foreach (Object _object in _serializedProperty.serializedObject.targetObjects) {
                Type _type = _object.GetType();
                object _instance = _object;

                if (!GetNestedField(_serializedProperty, ref _type, ref _instance) || !_member.SetValue(_instance, _type, _value)) {
                    _success = false;
                }
            }

            return _success;
        }

        // -----------------------

        public static bool GetNestedField(SerializedProperty _property, ref Type _type, ref object _object) {
            return EnhancedEditorUtility.FindSerializedPropertyField(_property, 1, out _, out _type, out _object);
        }
        #endregion
    }
}
