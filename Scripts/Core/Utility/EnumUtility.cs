// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//  All code for flag enums with no allocation come from here:
//  https://forum.unity.com/threads/c-hasaflag-method-extension-how-to-not-create-garbage-allocation.616924/
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

using EnumCollection = System.Collections.Generic.List<EnhancedEditor.EnumValueInfo>;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="Enum"/> value info wrapper.
    /// </summary>
    public struct EnumValueInfo {
        #region Content
        /// <summary>
        /// <see cref="int"/> value of this enum.
        /// </summary>
        public int Value;

        /// <summary>
        /// Name of this enum value.
        /// </summary>
        public string Name;

        /// <summary>
        /// Tooltip of this enum value.
        /// </summary>
        public string Tooltip;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="EnumValueInfo"/>
        public EnumValueInfo(int _value, string _name, string _tooltip) {
            Value   = _value;
            Name    = _name;
            Tooltip = _tooltip;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="Enum"/>-related utility class.
    /// </summary>
    public static class EnumUtility {
        #region Flag
        /// <summary>
        /// Get if a specific flag is contained within an enum value.
        /// </summary>
        /// <typeparam name="TEnum">This <see cref="Enum"/> type.</typeparam>
        /// <param name="_enum">Enum value to check.</param>
        /// <param name="_flag">Flag to check.</param>
        /// <returns>True of this flag is contained within this enum, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag<TEnum>(TEnum _enum, TEnum _flag) where TEnum : unmanaged, Enum {

            unsafe {
                switch (sizeof(TEnum)) {

                    case 1: {
                        var _value = (*(byte*)(&_enum) & *(byte*)(&_flag));
                        return _value > 0;
                    }
                    case 2: {
                        var _value = (*(ushort*)(&_enum) & *(ushort*)(&_flag));
                        return _value > 0;
                    }
                    case 4: {
                        var _value = (*(uint*)(&_enum) & *(uint*)(&_flag));
                        return _value > 0;
                    }
                    case 8: {
                        var _value = (*(ulong*)(&_enum) & *(ulong*)(&_flag));
                        return _value > 0;
                    }

                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
            }
        }

        /// <summary>
        /// Adds a flag to a specific enum value.
        /// </summary>
        /// <param name="_enum">Enum to add a flag to.</param>
        /// <param name="_flag">Flag to add.</param>
        /// <returns>This new enum value.</returns>
        /// <inheritdoc cref="HasFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum AddFlag<TEnum>(TEnum _enum, TEnum _flag) where TEnum : unmanaged, Enum {

            unsafe {
                switch (sizeof(TEnum)) {

                    case 1: {
                        var r = *(byte*)(&_enum) | *(byte*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 2: {
                        var r = *(ushort*)(&_enum) | *(ushort*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 4: {
                        var r = *(uint*)(&_enum) | *(uint*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 8: {
                        var r = *(ulong*)(&_enum) | *(ulong*)(&_flag);
                        return *(TEnum*)&r;
                    }

                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
            }
        }

        /// <summary>
        /// Removes a flag from this enum value.
        /// </summary>
        /// <param name="_enum">Enum to remove the flag from.</param>
        /// <param name="_flag">Flag to remove.</param>
        /// <inheritdoc cref="AddFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum RemoveFlag<TEnum>(TEnum _enum, TEnum _flag) where TEnum : unmanaged, Enum {

            unsafe {
                switch (sizeof(TEnum)) {

                    case 1: {
                        var r = *(byte*)(&_enum) & ~*(byte*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 2: {
                        var r = *(ushort*)(&_enum) & ~*(ushort*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 4: {
                        var r = *(uint*)(&_enum) & ~*(uint*)(&_flag);
                        return *(TEnum*)&r;
                    }
                    case 8: {
                        var r = *(ulong*)(&_enum) & ~*(ulong*)(&_flag);
                        return *(TEnum*)&r;
                    }

                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
            }

        }

        /// <returns></returns>
        /// <inheritdoc cref="AddFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFlagRef<TEnum>(ref TEnum _enum, TEnum _flag) where TEnum : unmanaged, Enum {

            unsafe {
                fixed (TEnum* _value = &_enum) {
                    switch (sizeof(TEnum)) {

                        case 1: {
                            var r = *(byte*)(_value) | *(byte*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 2: {
                            var r = *(ushort*)(_value) | *(ushort*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 4: {
                            var r = *(uint*)(_value) | *(uint*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 8: {
                            var r = *(ulong*)(_value) | *(ulong*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }

                        default:
                            throw new Exception("Size does not match a known Enum backing type.");
                    }
                }
            }
        }

        /// <returns></returns>
        /// <inheritdoc cref="RemoveFlag{TEnum}(TEnum, TEnum)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFlagRef<TEnum>(ref TEnum _enum, TEnum _flag) where TEnum : unmanaged, Enum {

            unsafe {
                fixed (TEnum* _value = &_enum) {
                    switch (sizeof(TEnum)) {

                        case 1: {
                            var r = *(byte*)(_value) & ~*(byte*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 2: {
                            var r = *(ushort*)(_value) & ~*(ushort*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 4: {
                            var r = *(uint*)(_value) & ~*(uint*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }
                        case 8: {
                            var r = *(ulong*)(_value) & ~*(ulong*)(&_flag);
                            *_value = *(TEnum*)&r;
                            return;
                        }

                        default:
                            throw new Exception("Size does not match a known Enum backing type.");
                    }
                }
            }
        }
        #endregion

        #region Name
        // Use a list as it behaves as a reference, to avoid allocation using the out parameter.
        private static readonly Dictionary<Type, EnumCollection> enumNames = new Dictionary<Type, EnumCollection>();

        // -----------------------

        /// <param name="_value">Enum value to get the associated name.</param>
        /// <inheritdoc cref="GetName(Type, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(Enum _value) {
            return GetName(_value.GetType(), _value.ToInt());
        }

        /// <summary>
        /// Get a friendly displayed name of a specific enum value.
        /// <br/> Uses the value of the <see cref="DisplayNameAttribute"/> if one.
        /// </summary>
        /// <param name="_enumType">Enum type of this value.</param>
        /// <param name="_enumValue">Enum value to get the associated name.</param>
        /// <returns>Friendly displayed name of this enum value.</returns>
        public static string GetName(Type _enumType, int _enumValue) {

            // Register enum and its value names.
            EnumCollection _enumNames = GetNames(_enumType);
            string _name = string.Empty;

            // Get enum name.
            foreach (var _pair in _enumNames) {

                if (_pair.Value == _enumValue) {
                    _name = _pair.Name;
                    break;
                }
            }

            return _name;
        }

        /// <summary>
        /// Get all friendly display names of a specific <see cref="Enum"/> type.
        /// </summary>
        /// <param name="_enumType">Enum to get the associated names.</param>
        /// <returns>Friendly display names of the given enum.</returns>
        public static EnumCollection GetNames(Type _enumType) {

            // Register enum.
            if (!enumNames.TryGetValue(_enumType, out EnumCollection _enumNames)) {

                var _values = Enum.GetValues(_enumType);
                _enumNames = new EnumCollection();

                for (int i = 0; i < _values.Length; i++) {

                    Enum _value = (Enum)_values.GetValue(i);

                    string _enumName = _value.ToString();
                    string _enumTooltip = string.Empty;

                    FieldInfo _field = _enumType.GetField(_enumName);

                    if (_field == null) {
                        continue;
                    }

                    // Use DisplayNameAttribute value if found.
                    if (_field.IsDefined(typeof(DisplayNameAttribute), true)) {

                        DisplayNameAttribute _attribute = _field.GetCustomAttribute<DisplayNameAttribute>(true);
                        _enumName = _attribute.Label.text;
                    }

                    // Tooltip.
                    if (_field.IsDefined(typeof(TooltipAttribute), true)) {
                        TooltipAttribute _attribute = _field.GetCustomAttribute<TooltipAttribute>(true);
                        _enumTooltip = _attribute.tooltip;
                    }

                    SeparatorPosition _separator = SeparatorPosition.None;

                    // Separator.
                    if (_field.IsDefined(typeof(SeparatorAttribute), true)) {
                        SeparatorAttribute _attribute = _field.GetCustomAttribute<SeparatorAttribute>(true);
                        _separator = _attribute.Position;
                    }

                    AddSeparator(SeparatorPosition.Top);

                    _enumNames.Add(new EnumValueInfo(_value.ToInt(), _enumName, _enumTooltip));

                    AddSeparator(SeparatorPosition.Bottom);

                    // ----- Local Method ----- \\

                    void AddSeparator(SeparatorPosition _position) {

                        if (_separator.HasFlagUnsafe(_position)) {
                            _enumNames.Add(new EnumValueInfo(-1, string.Empty, string.Empty));
                        }
                    }
                }

                enumNames.Add(_enumType, _enumNames);
            }

            return _enumNames;
        }
        #endregion
    }
}
