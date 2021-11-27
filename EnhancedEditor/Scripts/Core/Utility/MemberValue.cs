// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;

namespace EnhancedEditor
{
    /// <summary>
    /// A <see cref="MemberValue{T}"/> allows you to easily get / set an object member value.
    /// <br/>
    /// This member can either be a field, a property or a method.
    /// </summary>
    /// <typeparam name="T">Member object type.
    /// <para/>
    /// The specified member must be convertible to this type:
    /// <br/>for instance, you can use an <see cref="int"/> member for a <see cref="float"/> <see cref="MemberValue{T}"/>.</typeparam>
	public struct MemberValue<T>
    {
        #region Global Members
        /// <summary>
        /// Name of the associated object member.
        /// </summary>
        public string Name;

        /// <summary>
        /// Member exact type (equals to <see cref="typeof(T)"/>).
        /// </summary>
        internal Type type;

        // -----------------------

        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <inheritdoc cref="MemberValue{T}"/>
        public MemberValue(string _name)
        {
            Name = _name;
            type = typeof(T);
        }
        #endregion

        #region Operators
        public static implicit operator MemberValue<T>(string _memberName)
        {
            return new MemberValue<T>(_memberName);
        }
        #endregion

        #region Getter / Setter
        private static readonly object[] setMethodValueParameters = new object[] { null };
        private static readonly BindingFlags getMemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
                                                            | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.InvokeMethod;

        // -----------------------

        /// <summary>
        /// Get this member value from a specific object instance.
        /// </summary>
        /// <param name="_object">Object instance to get member value from.</param>
        /// <param name="_objectType">Instance object type.</param>
        /// <param name="_value">Member value.</param>
        /// <returns>True if this member value was successfully retrieved, false otherwise.</returns>
        public bool GetValue(object _object, Type _objectType, out T _value)
        {
            MemberInfo[] _members = _objectType.GetMember(Name, getMemberFlags);
            object _objectValue;

            for (int _i = 0; _i < _members.Length; _i++)
            {
                MemberInfo _member = _members[_i];
                switch (_member.MemberType)
                {
                    // Field.
                    case MemberTypes.Field:
                        FieldInfo _field = (FieldInfo)_member;
                        _objectValue = _field.GetValue(_object);

                        if (CastValue(_objectValue, type, out _value))
                        {
                            return true;
                        }
                        break;

                    // Method.
                    case MemberTypes.Method:
                        MethodInfo _method = (MethodInfo)_member;
                        if (_method.GetParameters().Length == 0)
                        {
                            _objectValue = _method.Invoke(_object, null);
                            if (CastValue(_objectValue, type, out _value))
                            {
                                return true;
                            }
                        }
                        break;

                    // Property.
                    case MemberTypes.Property:
                        PropertyInfo _property = (PropertyInfo)_member;
                        _objectValue = _property.GetValue(_object);

                        if (CastValue(_objectValue, type, out _value))
                        {
                            return true;
                        }
                        break;

                    default:
                        break;
                }
            }

            _value = default;
            return false;

            // ----- Local Method ----- \\

            bool CastValue(object _object, Type _type, out T _value)
            {
                if (_object is T)
                {
                    _value = (T)_object;
                    return true;
                }
                try
                {
                    _value = (T)Convert.ChangeType(_object, _type);
                    return true;
                }
                catch (InvalidCastException)
                {
                    _value = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Set this member value in a specific object instance.
        /// </summary>
        /// <param name="_object">Object instance to set member value.</param>
        /// <param name="_objectType">Instance object type.</param>
        /// <param name="_value">New member value.</param>
        /// <returns>True if this member value was successfully set, false otherwise.</returns>
        public bool SetValue(object _object, Type _objectType, T _value)
        {
            MemberInfo[] _members = _objectType.GetMember(Name, getMemberFlags);
            object _convertedValue;

            for (int _i = 0; _i < _members.Length; _i++)
            {
                MemberInfo _member = _members[_i];
                switch (_member.MemberType)
                {
                    // Field.
                    case MemberTypes.Field:
                        FieldInfo _field = (FieldInfo)_member;
                        if (CastValue(_value, type, _field.FieldType, out _convertedValue))
                        {
                            _field.SetValue(_object, _convertedValue);
                            return true;
                        }
                        break;

                    // Method.
                    case MemberTypes.Method:
                        MethodInfo _method = (MethodInfo)_member;
                        var _parameters = _method.GetParameters();

                        if (_parameters.Length == 1 && CastValue(_value, type, _parameters[0].ParameterType, out _convertedValue))
                        {
                            setMethodValueParameters[0] = _convertedValue;
                            _method.Invoke(_object, setMethodValueParameters);

                            return true;
                        }
                        break;

                    // Property.
                    case MemberTypes.Property:
                        PropertyInfo _property = (PropertyInfo)_member;
                        if (_property.CanWrite && CastValue(_value, type, _property.PropertyType, out _convertedValue))
                        {
                            _property.SetValue(_object, _convertedValue);
                            return true;
                        }
                        break;

                    default:
                        break;
                }
            }

            return false;

            // ----- Local Method ----- \\

            bool CastValue(T _object, Type _objectType, Type _targetType, out object _value)
            {
                if (_objectType == _targetType)
                {
                    _value = _object;
                    return true;
                }

                try
                {
                    _value = Convert.ChangeType(_object, _targetType);
                    return true;
                }
                catch (InvalidCastException)
                {
                    _value = default;
                    return false;
                }
            }
        }
        #endregion
    }
}
