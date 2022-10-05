// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

#if UNITY_EDITOR || !STRIP_FLAG_NAME
#define FLAG_NAME
#endif

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
        /// The referenced <see cref="Flags.Flag"/>.
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

                if ((flag == null) && (guid != 0) && !holder.RetrieveFlag(guid, out flag)) {
                    holder.LogWarning($"The Flag with the guid \'{guid}\' could not be found!");
                }

                return flag;
            }
        }

        // -----------------------

        /// <param name="_guid">The guid of the flag to reference.</param>
        /// <inheritdoc cref="FlagReference(Flag, FlagHolder)"/>
        internal FlagReference(int _guid, FlagHolder _holder) {
            if (_holder.RetrieveFlag(_guid, out Flag _flag)) {
                SerializeFlag(_flag, _holder);
            }
        }

        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        /// <param name="_holder"><inheritdoc cref="SerializeFlag(Flag, FlagHolder)" path="/param[@name='_holder']"/></param>
        /// <inheritdoc cref="FlagReference"/>
        public FlagReference(Flag _flag, FlagHolder _holder) {
            SerializeFlag(_flag, _holder);
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
        /// Serializes this flag reference.
        /// </summary>
        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        /// <param name="_holder">The <see cref="FlagHolder"/> containing this flag.</param>
        public void SerializeFlag(Flag _flag, FlagHolder _holder) {
            flag = _flag;
            holder = _holder;

            guid = _flag.guid;
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
        /// <inheritdoc cref="FlagValue(Flag, FlagHolder, bool)"/>
        internal FlagValue(int _guid, FlagHolder _holder, bool _value = true) : base(_guid, _holder) {
            Value = _value;
        }

        /// <param name="_flag"><inheritdoc cref="Flag" path="/summary"/></param>
        /// <param name="_holder"><inheritdoc cref="FlagReference(Flag, FlagHolder)" path="/param[@name='_holder']"/></param>
        /// <param name="_value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <inheritdoc cref="FlagValue"/>
        public FlagValue(Flag _flag, FlagHolder _holder, bool _value = true) : base(_flag, _holder) {
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
            #if FLAG_NAME
            return $"{Flag.Name}={Value}";
            #else
            return IsValid().ToString();
            #endif
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
    [ScriptingDefineSymbol("STRIP_FLAG_NAME", "Strip Game Flag Name (Build Only)")]
    public class Flag {
        #region Global Members
        #if FLAG_NAME
        public string Name = "\'New Flag\'";
        #endif

        [SerializeField] internal int guid = EnhancedUtility.GenerateGUID();
        [SerializeField] internal bool value = false;

        /// <summary>
        /// Called everytime this flag value is changed.
        /// </summary>
        public Action<bool> OnValueChanged = null;

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
                OnValueChanged?.Invoke(value);
            }
        }
        #endregion

        #region Operators
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
