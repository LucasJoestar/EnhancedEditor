// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Reflection;

using EnumCollection = System.Collections.Generic.List<EnhancedEditor.Pair<int, string>>;

namespace EnhancedEditor {
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

                if (_pair.First == _enumValue) {
                    _name = _pair.Second;
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
                    FieldInfo _field = _enumType.GetField(_enumName);

                    if (_field == null) {
                        continue;
                    }

                    // Use DisplayNameAttribute value if found.
                    if (_field.IsDefined(typeof(DisplayNameAttribute), true)) {

                        DisplayNameAttribute _attribute = _field.GetCustomAttribute<DisplayNameAttribute>(true);
                        _enumName = _attribute.Label.text;
                    }

                    SeparatorPosition _separator = SeparatorPosition.None;

                    // Separator.
                    if (_field.IsDefined(typeof(SeparatorAttribute), true)) {
                        SeparatorAttribute _attribute = _field.GetCustomAttribute<SeparatorAttribute>(true);
                        _separator = _attribute.Position;
                    }

                    AddSeparator(SeparatorPosition.Top);

                    _enumNames.Add(new Pair<int, string>(_value.ToInt(), _enumName));

                    AddSeparator(SeparatorPosition.Bottom);

                    // ----- Local Method ----- \\

                    void AddSeparator(SeparatorPosition _position) {

                        if (_separator.HasFlag(_position)) {
                            _enumNames.Add(new Pair<int, string>(-1, string.Empty));
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
