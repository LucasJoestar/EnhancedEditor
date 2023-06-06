// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;
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
    public class FlagHolder : ScriptableObject, IEnumerable<Flag> {
        #region Global Members
        /// <summary>
        /// All flags contained in this object.
        /// </summary>
        public Flag[] Flags = new Flag[] { new Flag() };

        /// <summary>
        /// Total amount of <see cref="Flag"/> in this holder.
        /// </summary>
        public int Count {
            get { return Flags.Length; }
        }
        #endregion

        #region Operator
        public virtual Flag this[int _index] {
            get { return Flags[_index]; }
            set { Flags[_index] = value; }
        }
        #endregion

        #region IEnumerable
        public IEnumerator<Flag> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return Flags[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Resets all flags in this holder to a FALSE value.
        /// </summary>
        [ContextMenu("Reset Flags", false, 10)]
        public void ResetFlags() {

            foreach (Flag _flag in Flags) {

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
            foreach (Flag _temp in Flags) {
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
            foreach (Flag _temp in Flags) {
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
            foreach (Flag _flag in Flags) {
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
