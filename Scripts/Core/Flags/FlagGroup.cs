// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor {
    /// <summary>
    /// Base class to inherit managing <see cref="Flag"/> classes.
    /// </summary>
    public abstract class FlagGroup : IEnumerable<Flag> {
        #region Global Members
        /// <summary>
        /// The total amount of flags in this group.
        /// </summary>
        public int Count => Array.Length;

        /// <summary>
        /// All elements contained in this group.
        /// </summary>
        public abstract Array Array { get; }
        #endregion

        #region Operator
        public abstract Flag this[int _index] { get; }
        #endregion

        #region IEnumerable
        public IEnumerator<Flag> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region Management
        /// <summary>
        /// Adds a <see cref="Flag"/> into this group.
        /// </summary>
        /// <param name="_flag">The <see cref="Flag"/> to add to the group.</param>
        public abstract void AddFlag(Flag _flag);

        /// <summary>
        /// Removes a <see cref="Flag"/> from this group.
        /// </summary>
        /// <param name="_flag">The <see cref="Flag"/> to remove from the group.</param>
        public abstract void RemoveFlag(Flag _flag);

        /// <summary>
        /// Removes the <see cref="Flag"/> at the specified index from this group.
        /// </summary>
        /// <param name="_index">The index of the <see cref="Flag"/> to from the group.</param>
        public abstract void RemoveFlagAt(int _index);

        /// <summary>
        /// Does this group contain a specific flag?
        /// </summary>
        /// <param name="_flag">The <see cref="Flag"/> to check.</param>
        /// <param name="_index">Index of the flag in the group (-1 if not in group).</param>
        /// <returns>True if this group contains the flag, false otherwise.</returns>
        public abstract bool ContainFlag(Flag _flag, out int _index);
        #endregion
    }

    /// <summary>
    /// Group holder for multiple <see cref="FlagReference"/>.
    /// </summary>
    [Serializable]
    public class FlagReferenceGroup : FlagGroup {
        #region Global Members
        /// <summary>
        /// All <see cref="FlagReference"/> in this group.
        /// </summary>
        public FlagReference[] Flags = new FlagReference[] { };

        public override Array Array { get { return Flags; } }

        // -----------------------

        /// <inheritdoc cref="FlagReferenceGroup"/>
        public FlagReferenceGroup() { }

        /// <param name="_flags"><inheritdoc cref="Flags" path="/summary"/></param>
        /// <inheritdoc cref="FlagReferenceGroup"/>
        public FlagReferenceGroup(FlagReference[] _flags) {
            Flags = _flags;
        }
        #endregion

        #region Operator
        public override Flag this[int _index] { get { return Flags[_index].Flag; } }

        public override string ToString() {
            StringBuilder _builder = new StringBuilder(string.Empty);
            foreach (FlagReference _flag in Flags) {
                _builder.Append($"{_flag.Flag.Name}; ");
            }

            return _builder.ToString();
        }
        #endregion

        #region Management
        public override void AddFlag(Flag _flag) {
            ArrayUtility.Add(ref Flags, new FlagReference(_flag));
        }

        public override void RemoveFlag(Flag _flag) {
            if (ContainFlag(_flag, out int _index)) {
                RemoveFlagAt(_index);
            }
        }

        public override void RemoveFlagAt(int _index) {
            ArrayUtility.RemoveAt(ref Flags, _index);
        }

        public override bool ContainFlag(Flag _flag, out int _index) {
            for (int i = 0; i < Flags.Length; i++) {
                if (Flags[i].Flag.guid == _flag.guid) {
                    _index = i;
                    return true;
                }
            }

            _index = -1;
            return false;
        }
        #endregion
    }

    /// <summary>
    /// Group holder for multiple <see cref="FlagValue"/>.
    /// </summary>
    [Serializable]
    public class FlagValueGroup : FlagGroup {
        #region Global Members
        /// <summary>
        /// All <see cref="FlagValue"/> in this group.
        /// </summary>
        public FlagValue[] Flags = new FlagValue[] { };

        public override Array Array { get { return Flags; } }

        /// <summary>
        /// Are all the flags in this group valid?
        /// </summary>
        /// <returns>True if all flags are valid, false otherwise.</returns>
        public bool Valid {
            get {
                foreach (FlagValue _flag in Flags) {
                    if (!_flag) {
                        return false;
                    }
                }

                return true;
            }
        }

        // -----------------------

        /// <inheritdoc cref="FlagValueGroup"/>
        public FlagValueGroup() { }

        /// <param name="_flags"><inheritdoc cref="Flags" path="/summary"/></param>
        /// <inheritdoc cref="FlagValueGroup"/>
        public FlagValueGroup(FlagValue[] _flags) {
            Flags = _flags;
        }
        #endregion

        #region Operator
        public override Flag this[int _index] { get { return Flags[_index].Flag; } }

        public static implicit operator bool(FlagValueGroup _group) {
            return _group.Valid;
        }

        public override string ToString() {
            StringBuilder _builder = new StringBuilder(string.Empty);
            foreach (FlagValue _flag in Flags) {
                _builder.Append($"{_flag}; ");
            }

            return _builder.ToString();
        }
        #endregion

        #region Management
        public void AddFlag(Flag _flag, bool _requiredValue) {
            ArrayUtility.Add(ref Flags, new FlagValue(_flag, _requiredValue));
        }

        public override void AddFlag(Flag _flag) {
            ArrayUtility.Add(ref Flags, new FlagValue(_flag));
        }

        public override void RemoveFlag(Flag _flag) {
            if (ContainFlag(_flag, out int _index)) {
                RemoveFlagAt(_index);
            }
        }

        public override void RemoveFlagAt(int _index) {
            ArrayUtility.RemoveAt(ref Flags, _index);
        }

        public override bool ContainFlag(Flag _flag, out int _index) {
            for (int i = 0; i < Flags.Length; i++) {
                if (Flags[i].Flag == _flag) {
                    _index = i;
                    return true;
                }
            }

            _index = -1;
            return false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Set the value of all flags in this group.
        /// </summary>
        public void SetValues() {
            foreach (FlagValue _flag in Flags) {
                _flag.SetFlag();
            }
        }

        /// <summary>
        /// Set the value of all flags in this group.
        /// </summary>
        /// <param name="_valid">Whether to set or unset these flags.</param>
        public void SetValues(bool _valid) {
            foreach (FlagValue _flag in Flags) {

                bool _value = _flag.Value;
                if (!_valid) {
                    _value = !_value;
                }

                _flag.SetValue(_value);
            }
        }
        #endregion
    }
}
