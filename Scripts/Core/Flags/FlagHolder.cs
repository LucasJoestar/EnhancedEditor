// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> wrapper class for multiple <see cref="Flag"/>.
    /// </summary>
    [ResetOnExitPlayMode]
    [CreateAssetMenu(fileName = "FLG_FlagHolder", menuName = InternalUtility.MenuPath + "Flag Holder", order = InternalUtility.MenuOrder)]
    public sealed class FlagHolder : ScriptableObject {
        #region Global Members
        [Section("Flag Holder")]

        [Tooltip("When marked as 'persistent', a flag holder will not automatically be reset when quitting the current game")]
        [SerializeField] private bool isPersistent = false;

        [Space(10f)]

        /// <summary>
        /// All flags contained in this object.
        /// </summary>
        public Flag[] Flags = new Flag[] { new Flag() };

        // -----------------------

        /// <summary>
        /// Total amount of <see cref="Flag"/> in this holder.
        /// </summary>
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Flags.Length; }
        }

        /// <summary>
        /// "When marked as 'persistent', a flag holder will not automatically be reset when quitting the current game.
        /// </summary>
        public bool IsPersistent {
            get { return isPersistent; }
        }
        #endregion

        #region Operator
        public Flag this[int _index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Flags[_index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Flags[_index] = value; }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Resets all flags in this holder to a FALSE value.
        /// </summary>
        /// <param name="_resetPersistent">If true, also resets game flags marked as 'persistent'.</param>
        [ContextMenu("Reset Flags", false, 10)]
        public void ResetFlags(bool _resetPersistent = false) {

            if (!_resetPersistent && isPersistent)
                return;

            int _length = Flags.Length;

            for (int i = 0; i < _length; i++) {
                Flag _flag = Flags[i];

                _flag.value = false;
                FlagUtility.OnFlagChanged(_flag, _flag.value);
            }

            this.LogMessage("Reset Flags");
        }
        #endregion

        #region Utility
        /// <param name="_guid">Guid of the <see cref="Flag"/> to find.</param>
        /// <inheritdoc cref="FindFlag(string, out Flag)"/>
        public bool FindFlag(int _guid, out Flag _flag) {
            int _length = Flags.Length;

            for (int i = 0; i < _length; i++) {
                Flag _temp = Flags[i];

                if (_temp == _guid) {
                    _flag = _temp;
                    return true;
                }
            }

            _flag = null;
            return false;
        }

        /// <summary>
        /// Finds a specific <see cref="Flag"/> from this holder.
        /// </summary>
        /// <param name="_name">Name of the <see cref="Flag"/> to find.</param>
        /// <param name="_flag">The retrieved <see cref="Flag"/> (null if none).</param>
        /// <returns>True if the corresponding flag could be found, false otherwise.</returns>
        public bool FindFlag(string _name, out Flag _flag) {
            int _length = Flags.Length;

            for (int i = 0; i < _length; i++) {
                Flag _temp = Flags[i];

                if (_temp.Name == _name) {
                    _flag = _temp;
                    return true;
                }
            }

            _flag = null;
            return false;
        }
        #endregion

        #region Editor
        #if UNITY_EDITOR
        private void OnValidate() {
            if (Flags.Length == 0) {
                ArrayUtility.Add(ref Flags, new Flag() { name = "\'New Flag\'" });
            } else {
                // Check that the last added flag guid is unique.
                int _lastIndex = Flags.Length - 1;
                Flag _last = Flags[_lastIndex];

                for (int i = 0; i < _lastIndex; i++) {
                    if (_last.guid == Flags[i].guid) {
                        _last.guid = EnhancedUtility.GenerateGUID();
                        i = 0;
                    }
                }

                Sort();
            }

            // Set holder reference.
            for (int i = 0; i < Flags.Length; i++) {
                Flag _flag = Flags[i];
                _flag.holder = this;

                // Update.
                if (Application.isPlaying) {
                    FlagUtility.OnFlagChanged(_flag, _flag.value);
                }
            }
        }

        [ContextMenu("Sort by Name", false, 10)]
        private void Sort() {
            Array.Sort(Flags, (a, b) => a.Name.CompareTo(b.Name));
            EditorUtility.SetDirty(this);
        }
        #endif
        #endregion
    }
}
