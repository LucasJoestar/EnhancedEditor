// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing all <see cref="TagData"/> in the project.
    /// </summary>
    [NonEditable("This data is sensitive and should not be manipulated manually.")]
    public class FlagDatabase : ScriptableObject {
        #region Global Members
        private static FlagDatabase database = null;

        #if UNITY_EDITOR
        /// <summary>
        /// Editor internal getter for the <see cref="FlagDatabase"/> instance.
        /// <para/>
        /// As it needs to be set manually at runtime, it uses an internal getter when in editor mode
        /// to be safely able to load it from the database, even if the user deletes it.
        /// </summary>
        internal static Func<FlagDatabase> EditorFlagDatabaseGetter = null;
        #endif

        /// <summary>
        /// You have to set this reference at runtime to be properly able to use it.
        /// <br/>
        /// There are a variety of ways to assign its value:
        /// <list type="bullet">
        /// <item>by <see cref="ScriptableObject"/> reference</item>
        /// <item>using <see cref="Resources.Load(string)"/></item>
        /// <item><see cref="AssetBundle"/></item>
        /// <item>... or any other way you'd like.</item>
        /// </list><para/>
        /// </summary>
        public static FlagDatabase Database {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (EditorFlagDatabaseGetter != null)) {
                    return EditorFlagDatabaseGetter();
                }

                if (database == null) {
                    Debug.LogError($"Unassigned {typeof(FlagDatabase).Name} reference!\nYou must manually set this database " +
                                   $"reference on game start to be able to properly use it.");

                    database = CreateInstance<FlagDatabase>();
                }
                #endif

                return database;
            }
            set {
                database = value;
            }
        }

        // -------------------------------------------
        // Database Content
        // -------------------------------------------

        [SerializeField] internal FlagHolder[] holders = new FlagHolder[0];

        /// <summary>
        /// All <see cref="FlagHolder"/> defined in the project.
        /// </summary>
        public FlagHolder[] Holders {
            get { return holders; }
        }

        /// <summary>
        /// Total amount of <see cref="FlagHolder"/> in the project.
        /// </summary>
        public int Count {
            get { return holders.Length; }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Resets all in-game flags to a FALSE value.
        /// </summary>
        public void ResetFlags() {

            foreach (FlagHolder _holder in holders) {
                _holder.ResetFlags();
            }
        }
        #endregion

        #region Utility
        /// <param name="_name"><inheritdoc cref="SetFlag(string, string, bool" path="/param[@name='_flagName']"/></param>
        /// <inheritdoc cref="SetFlag(string, string, bool)"/>
        public bool SetFlag(string _name, bool _value) {
            if (FindFlag(_name, out Flag _flag)) {
                _flag.Value = _value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the first matching <see cref="Flag"/> in the database and set its value.
        /// </summary>
        /// <param name="_flagName">Name of the flag to find.</param>
        /// <param name="_holderName">Name of the <see cref="FlagHolder"/> containing the flag.</param>
        /// <param name="_value">Value to assign to the flag.</param>
        /// <returns>True if a matching <see cref="Flag"/> could be found, false otherwise.</returns>
        public bool SetFlag(string _flagName, string _holderName, bool _value) {
            if (FindFlag(_flagName, _holderName, out Flag _flag)) {
                _flag.Value = _value;
                return true;
            }

            return false;
        }

        /// <param name="_name"><inheritdoc cref="FindFlag(string, string, out Flag, out FlagHolder" path="/param[@name='_flagName']"/></param>
        /// <inheritdoc cref="FindFlag(string, string, out Flag)"/>
        public bool FindFlag(string _name, out Flag _flag) {
            foreach (FlagHolder _holder in holders) {

                if (_holder.FindFlag(_name, out _flag)) {
                    return true;
                }
            }

            _flag = null;
            return false;
        }

        /// <summary>
        /// Finds the first matching <see cref="Flag"/> in the database.
        /// </summary>
        /// <param name="_flagName">Name of the flag to find.</param>
        /// <param name="_holderName">Name of the <see cref="FlagHolder"/> containing the flag.</param>
        /// <param name="_flag">Found matching flag (null if none).</param>
        /// <param name="_flag"><see cref="FlagHolder"/> of the matching flag (null if none).</param>
        /// <returns>True if a matching <see cref="Flag"/> could be found, false otherwise.</returns>
        public bool FindFlag(string _flagName, string _holderName, out Flag _flag) {
            if (FindHolder(_holderName, out FlagHolder _holder) && _holder.FindFlag(_flagName, out _flag)) {
                return true;
            }

            _flag = null;
            return false;
        }

        /// <summary>
        /// Finds the first <see cref="FlagHolder"/> in the database matching a given name.
        /// </summary>
        /// <param name="_name">Name of the <see cref="FlagHolder"/> to find.</param>
        /// <param name="_holder"><see cref="FlagHolder"/> with the given name (null if none).</param>
        /// <returns>True if a <see cref="FlagHolder"/> with the given name could be successfully found, false otherwise.</returns>
        public bool FindHolder(string _name, out FlagHolder _holder) {
            foreach (FlagHolder _temp in holders) {

                if (_temp.name == _name) {
                    _holder = _temp;
                    return true;
                }
            }

            _holder = null;
            return false;
        }

        /// <summary>
        /// Get the <see cref="FlagHolder"/> at the given index.
        /// <br/> Use <see cref="Count"/> to get the total amount of <see cref="FlagHolder"/> in the database.
        /// </summary>
        public FlagHolder GetHolder(int _index) {
            return holders[_index];
        }
        #endregion
    }
}
