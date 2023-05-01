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
    /// <see cref="EnhancedEditor.Flag"/> reference wrapper.
    /// <br/> Use this to reference a flag object in the inspector.
    /// </summary>
    [Serializable]
    public class FlagReference {
        #region Global Members
        [SerializeField] internal FlagHolder holder = null;
        [SerializeField] internal int guid = 0;

        [NonSerialized] private Flag flag = null;

        /// <summary>
        /// The referenced <see cref="EnhancedEditor.Flag"/>.
        /// </summary>
        public Flag Flag {
            get {
                #if UNITY_EDITOR
                if ((holder == null) && (guid != 0)) {
                    holder = null;
                    guid = 0;

                    Debug.LogWarning($"The flag reference with the guid \'{guid}\' is missing its holder and cannot be deserialize!");
                    return null;
                }
                #endif

                if ((flag == null) && (guid != 0) && !holder.FindFlag(guid, out flag)) {
                    holder.LogWarning($"The Flag with the guid \'{guid}\' could not be found!");
                }

                return flag;
            }
        }

        // -----------------------

        /// <param name="_guid">The guid of the flag to reference.</param>
        /// <inheritdoc cref="FlagReference(Flag, FlagHolder)"/>
        internal FlagReference(int _guid, FlagHolder _holder) {
            if (_holder.FindFlag(_guid, out Flag _flag)) {
                SetFlag(_flag);
            }
        }

        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        /// <param name="_holder"><inheritdoc cref="SetFlag(Flag, FlagHolder)" path="/param[@name='_holder']"/></param>
        /// <inheritdoc cref="FlagReference"/>
        public FlagReference(Flag _flag) {
            SetFlag(_flag);
        }

        /// <inheritdoc cref="FlagReference"/>
        public FlagReference() { }
        #endregion

        #region Operator
        public static implicit operator Flag(FlagReference _flag) {
            return _flag.Flag;
        }

        public static implicit operator bool(FlagReference _flag) {
            return _flag.Flag;
        }

        public static implicit operator int(FlagReference _flag) {
            return _flag.guid;
        }

        public override string ToString() {
            return Flag.ToString();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Set this flag reference.
        /// </summary>
        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        public void SetFlag(Flag _flag) {
            flag = _flag;
            holder = _flag.holder;

            guid = _flag.guid;
        }

        /// <summary>
        /// Get the value of this flag.
        /// </summary>
        /// <returns>The value of this flag.</returns>
        public bool GetValue() {
            return Flag.Value;
        }

        /// <summary>
        /// Set the value of this flag.
        /// </summary>
        /// <param name="_value">New value of this flag.</param>
        public void SetValue(bool _value) {
            Flag.Value = _value;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="Flag"/> reference wrapper,
    /// associated with a required value to be considered as valid.
    /// </summary>
    [Serializable]
    public class FlagValue : FlagReference {
        #region Global Members
        /// <summary>
        /// The value to be used to validate or set this flag.
        /// </summary>
        public bool Value = true;

        // -----------------------

        /// <param name="_guid">The guid of the flag to reference.</param>
        /// <param name="_holder"><inheritdoc cref="FlagReference(Flag, FlagHolder)" path="/param[@name='_holder']"/></param>
        /// <inheritdoc cref="FlagValue(Flag, FlagHolder, bool)"/>
        internal FlagValue(int _guid, FlagHolder _holder, bool _value = true) : base(_guid, _holder) {
            Value = _value;
        }

        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        /// <param name="_value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <inheritdoc cref="FlagValue"/>
        public FlagValue(Flag _flag, bool _value = true) : base(_flag) {
            Value = _value;
        }

        /// <inheritdoc cref="FlagValue"/>
        public FlagValue() { }
        #endregion

        #region Operator
        public static implicit operator bool(FlagValue _flag) {
            return _flag.IsValid();
        }

        public override string ToString() {
            return $"{Flag.Name}={Value}";
        }
        #endregion

        #region Utility
        /// <summary>
        /// Is this <see cref="Flag"/> value the same as this object <see cref="Value"/>?
        /// </summary>
        /// <returns>True if this flag is valid, false otherwise.</returns>
        public bool IsValid() {
            return Flag == Value;
        }

        /// <summary>
        /// Set this <see cref="Flag"/> value to this object <see cref="Value"/>.
        /// </summary>
        public void SetFlag() {
            Flag.Value = Value;
        }
        #endregion
    }

    /// <summary>
    /// Global <see cref="bool"/> value reflecting a specific state of the world.
    /// <para/>
    /// Conceived to be stored within a <see cref="FlagHolder"/> object,
    /// it can be referenced using a <see cref="FlagReference"/> instance.
    /// </summary>
    [Serializable]
    public class Flag {
        #region Global Members
        [SerializeField] internal string name   = "\'New Flag\'";
        [SerializeField] internal int guid      = EnhancedUtility.GenerateGUID();

        [SerializeField] internal FlagHolder holder = null;
        [SerializeField] internal bool value        = false;

        /// <summary>
        /// Unique GUID of this flag.
        /// </summary>
        public int GUID {
            get { return guid; }
        }

        /// <summary>
        /// <see cref="FlagHolder"/> containing this flag.
        /// </summary>
        public FlagHolder Holder {
            get { return holder; }
        }

        /// <summary>
        /// Value of this flag (enabled or disabled).
        /// </summary>
        public bool Value {
            get { return value; }
            set {
                if (this.value == value) {
                    return;
                }

                this.value = value;
                FlagUtility.OnFlagChanged(this, value);

                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return;
                }
                #endif

                #if DEVELOPMENT
                Debug.Log($"Flag \'{Name.Bold().Color(SuperColor.Turquoise)}\' set to {value.ToString().ToUpper().Bold()}");
                #endif
            }
        }

        /// <summary>
        /// Name of this flag.
        /// </summary>
        public string Name {
            get { return name; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new flags in other assemblies.
        /// </summary>
        internal protected Flag() { }
        #endregion

        #region Operator
        public static implicit operator bool(Flag _flag) {
            return _flag.value;
        }

        public static implicit operator int(Flag _flag) {
            return _flag.guid;
        }

        public override string ToString() {
            return value.ToString();
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Enables this flag.
        /// </summary>
        public void Enable() {
            Value = true;
        }

        /// <summary>
        /// Disables this flag.
        /// </summary>
        public void Disable() {
            Value = false;
        }

        /// <summary>
        /// Inverts this flag value.
        /// </summary>
        public void Invert() {
            Value = !value;
        }
        #endregion
    }
}
