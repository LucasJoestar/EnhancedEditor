// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;
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
            Value = _value;
            Name = _name;
            Tooltip = _tooltip;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="Enum"/>-related utility class.
    /// </summary>
    public static class EnumUtility {
        #region Name
        // Use a list as it behaves as a reference, to avoid allocation using the out parameter.
        private static readonly Dictionary<Type, EnumCollection> enumNames = new Dictionary<Type, EnumCollection>();

        // -----------------------

        /// <param name="_value">Enum value to get the associated name.</param>
        /// <inheritdoc cref="GetName(Type, int)"/>
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

                        if (_separator.HasFlag(_position)) {
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
