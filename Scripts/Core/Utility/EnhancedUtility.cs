// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple utility methods for usage during both editor and runtime.
    /// </summary>
    public static class EnhancedUtility {
        #region Reflection
        private const BindingFlags CopyObjectFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly MethodInfo findObjectFromInstanceIDMethod = typeof(Object).GetMethod("FindObjectFromInstanceID", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly object[] findObjectFromInstanceIDParameters = new object[1];

        // -----------------------

        /// <summary>
        /// Finds the loaded <see cref="Object"/> with a specific instance id.
        /// </summary>
        /// <param name="_instanceID">The id to get the associated <see cref="Object"/>.</param>
        /// <param name="_object">The object with the given instance id.</param>
        /// <returns>True if the object with this instance id could be found, false otherwise.</returns>
        public static bool FindObjectFromInstanceID(int _instanceID, out Object _object) {
            findObjectFromInstanceIDParameters[0] = _instanceID;

            _object = findObjectFromInstanceIDMethod.Invoke(null, findObjectFromInstanceIDParameters) as Object;
            return _object != null;
        }

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

        #region Type
        /// <summary>
        /// Checks if an object is compatible with a given type, and cast it if it does.
        /// </summary>
        /// <typeparam name="T">The type of the object to check.</typeparam>
        /// <typeparam name="U">The compatibility type to get the object value.</typeparam>
        /// <param name="_object">The object to check.</param>
        /// <param name="_value">The casted object value (if succeeded)</param>
        /// <returns>True if the object is compatible and was successfully casted, false otherwise.</returns>
        public static bool IsType<T, U>(T _object, out U _value) {
            if (_object is U _type) {
                _value = _type;
                return true;
            }

            _value = default;
            return false;
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

        /// <summary>
        /// Get a persistent hash code for a given <see cref="string"/>.
        /// </summary>
        /// <param name="_string"><see cref="string"/> to get a hash code for.</param>
        /// <returns>The hash code for the given <see cref="string"/>.</returns>
        public static int GetStableHashCode(string _string) {
            // Same as GetHashCode, but without any random operation.
            unchecked {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; (i < _string.Length) && (_string[i] != '\0'); i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ _string[i];
                    if ((i == _string.Length - 1) || (_string[i + 1] == '\0')) {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ _string[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        /// <summary>
        /// Get a persistent hash code for a given <see cref="string"/>, as a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="_string"><see cref="string"/> to get a hash code for.</param>
        /// <returns>The hash code for the given <see cref="string"/>.</returns>
        public static ulong GetLongStableHashCode(string _string) {
            ulong hashCode = 0;

            if (!string.IsNullOrEmpty(_string)) {
                // Unicode Encode Covering all characterset.
                byte[] byteContents = Encoding.Unicode.GetBytes(_string);
                SHA256 hash = new SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);

                // 32Byte hashText separate
                //
                // hashCodeStart = 0~7  8Byte
                // hashCodeMedium = 8~23  8Byte
                // hashCodeEnd = 24~31  8Byte

                ulong hashCodeStart = BitConverter.ToUInt64(hashText, 0);
                ulong hashCodeMedium = BitConverter.ToUInt64(hashText, 8);
                ulong hashCodeEnd = BitConverter.ToUInt64(hashText, 24);

                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }

            return hashCode;
        }
        #endregion

        #region Directory
        private const string MyGamesFolder = "My Games";

        // -----------------------

        /// <summary>
        /// Get the directory path to My Games folder.
        /// <br/> Works only on Windows. Returns <see cref="Application.persistentDataPath"/> otherwise.
        /// </summary>
        /// <param name="_autoCreate">If true, automatically creates this directory if it does not exist.</param>
        /// <returns>My Games folder path on Windows, <see cref="Application.persistentDataPath"/> otherwise.</returns>
        public static string GetMyGamesDirectoryPath(bool _autoCreate = true) {

            string _path;

            #if UNITY_STANDALONE_WIN
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MyGamesFolder, Application.productName);
            #else
            savedGamesPath = Application.persistentDataPath + "/";
            #endif

            if (_autoCreate && !Directory.Exists(_path)) {
                Directory.CreateDirectory(_path);
            }

            return _path;
        }
        #endregion
    }
}
