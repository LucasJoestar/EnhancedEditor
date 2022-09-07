// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> wrapper class for multiple <see cref="Flag"/>.
    /// </summary>
    [ResetOnExitPlayMode]
    [CreateAssetMenu(fileName = "FH_NewFlagHplder", menuName = "Enhanced Editor/Flag Holder", order = 200)]
    public class FlagHolder : ScriptableObject {
        #region Global Members
        /// <summary>
        /// All flags contained in this object.
        /// </summary>
        public Flag[] Flags = new Flag[] { new Flag() };
        #endregion

        #region Utility
        /// <summary>
        /// Retrieves a flag from this holder.
        /// </summary>
        /// <param name="_flagGuid">The guid of the <see cref="Flag"/> to retrieve.</param>
        /// <param name="_flag">The retrieved <see cref="Flag"/> (null if not found).</param>
        /// <returns>True if the corresponding is found, false otherwise.</returns>
        public bool RetrieveFlag(int _flagGuid, out Flag _flag) {
            foreach (Flag _f in Flags) {
                if (_f == _flagGuid) {
                    _flag = _f;
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
                ArrayUtility.Add(ref Flags, new Flag() { Name = "\'New Flag\'" });
            } else {
                // Check that the last added flag guid is unique.
                int _lastIndex = Flags.Length - 1;
                Flag _last = Flags[_lastIndex];

                for (int i = 0; i < _lastIndex; i++) {
                    if (_last.guid == Flags[i].guid) {
                        _last.guid = Flag.GenerateGuid();
                        i = 0;
                    }
                }

                Sort();
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
