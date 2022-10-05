// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple utility methods for usage during both editor and runtime.
    /// </summary>
    public static class EnhancedUtility {
        #region Reflection
        private const BindingFlags CopyObjectFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // -----------------------

        /// <summary>
        /// Copies the value of all fields and properties of a source object into a target object.
        /// <br/> (Both objects should inherit from a common base class).
        /// </summary>
        /// <param name="_source">The source object to copy values from.</param>
        /// <param name="_target">The destination object to copy values into.</param>
        /// <param name="_doFullCopy">If true, even the members with the <see cref="PreventCopyAttribute"/> will be copied.</param>
        /// <returns>The modified destination object.</returns>
        public static object CopyObjectContent(object _source, object _target, bool _doFullCopy = false) {
            // Copy all available field values.
            FieldInfo[] _sourceFields = _source.GetType().GetFields(CopyObjectFlags);
            FieldInfo[] _fields = _target.GetType().GetFields(CopyObjectFlags);

            for (int _i = 0; _i < _fields.Length; _i++) {
                FieldInfo _to = _fields[_i];

                if (!_doFullCopy && _to.IsDefined(typeof(PreventCopyAttribute), true)) {
                    continue;
                }

                for (int _j = 0; _j < _sourceFields.Length; _j++) {
                    FieldInfo _from = _sourceFields[_j];

                    if ((_from.Name == _to.Name) && _to.FieldType.IsAssignableFrom(_from.FieldType)) {
                        try {
                            _to.SetValue(_target, _from.GetValue(_source));
                        } catch (Exception) { }
                        break;
                    }
                }
            }

            // And do the same for the properties.
            PropertyInfo[] _sourceProperties = _source.GetType().GetProperties(CopyObjectFlags);
            PropertyInfo[] _properties = _target.GetType().GetProperties(CopyObjectFlags);

            for (int _i = 0; _i < _properties.Length; _i++) {
                PropertyInfo _to = _properties[_i];

                if (!_to.CanWrite || !_to.CanRead || (!_doFullCopy && _to.IsDefined(typeof(PreventCopyAttribute), true))) {
                    continue;
                }

                for (int _j = 0; _j < _sourceProperties.Length; _j++) {
                    PropertyInfo _from = _sourceProperties[_i];

                    if (_from.CanRead && (_from.Name == _to.Name) && _to.PropertyType.IsAssignableFrom(_from.PropertyType)) {
                        try {
                            _to.SetValue(_target, _from.GetValue(_source));
                        } catch (Exception) { }
                        break;
                    }
                }
            }

            return _target;
        }
        #endregion

        #region Generation
        /// <summary>
        /// Generates a new int GUID.
        /// </summary>
        /// <returns>The generated GUID.</returns>
        public static int GenerateGUID() {
            return Guid.NewGuid().GetHashCode();
        }
        #endregion
    }
}
