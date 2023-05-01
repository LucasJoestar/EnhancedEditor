// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;

namespace EnhancedEditor {
    /// <summary>
    /// Inherit from this interface to receive callbacks when a <see cref="Flag"/> value is changed.
    /// <para/> Unregister or unregister this object using
    /// <see cref="FlagUtility.RegisterFlagCallback(IFlagCallback)"/> and <see cref="FlagUtility.UnregisterFlagCallback(IFlagCallback)"/>.
    /// </summary>
    public interface IFlagCallback {
        #region Content
        /// <summary>
        /// Called when any <see cref="Flag"/> value changes.
        /// </summary>
        /// <param name="_flag">The <see cref="Flag"/> whose value was changed.</param>
        /// <param name="_value">New value of this flag.</param>
        void OnFlagChanged(Flag _flag, bool _value);
        #endregion
    }

    /// <summary>
    /// Contains multiple <see cref="Flag"/>-related utility methods.
    /// </summary>
    public static class FlagUtility {
        #region Content
        private static readonly List<IFlagCallback> callbacks = new List<IFlagCallback>();

        // -----------------------

        /// <inheritdoc cref="IFlagCallback.OnFlagChanged(Flag, bool)"/>
        internal static void OnFlagChanged(Flag _flag, bool _value) {

            for (int i = callbacks.Count; i-- > 0;) {
                callbacks[i].OnFlagChanged(_flag, _value);
            }
        }

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Registers a new flag callback.
        /// </summary>
        /// <param name="_callback"><see cref="IFlagCallback"/> to register.</param>
        public static void RegisterFlagCallback(IFlagCallback _callback) {
            callbacks.Add(_callback);
        }

        /// <summary>
        /// Unregisters a specific flag callback.
        /// </summary>
        /// <param name="_callback"><see cref="IFlagCallback"/> to unregister.</param>
        public static void UnregisterFlagCallback(IFlagCallback _callback) {
            callbacks.RemoveInstance(_callback);
        }
        #endregion
    }
}
