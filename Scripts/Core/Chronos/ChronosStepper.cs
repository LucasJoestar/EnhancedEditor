// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Utility class used to dynamically increase and decrease the game global choronos, according to predefined steps.
    /// </summary>
    public static class ChronosStepper {
        #region Behaviour
        /// <summary>
        /// Delegate used to set the game global chronos factor.
        /// <br/> Override this callback to implement a different behaviour.
        /// </summary>
        public static Action<float> OnSetChronos = (c) => Time.timeScale = c;

        /// <summary>
        /// All values used as steps when increasing/decreasing the game chronos.
        /// </summary>
        public static float[] StepChronos = new float[] {
            .1f, .2f, .25f, .5f, .75f,
            1f,
            2f, 4f, 8f, 16f, 32f
        };

        // -------------------------------------------
        // Steps
        // -------------------------------------------

        /// <summary>
        /// Increases the game time scale.
        /// </summary>
        public static void Increase() {
            int _index = Mathf.Min(GetChronosValueIndex() + 1, StepChronos.Length - 1);
            SetChronos(StepChronos[_index]);
        }

        /// <summary>
        /// Resets the game time scale to 1.
        /// </summary>
        public static void Reset() {
            SetChronos(1f);
        }

        /// <summary>
        /// Decreases the game time scale.
        /// </summary>
        public static void Decrease() {
            int _index = Mathf.Max(GetChronosValueIndex() - 1, 0);
            SetChronos(StepChronos[_index]);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static void SetChronos(float _chronos) {
            OnSetChronos?.Invoke(_chronos);
        }

        private static int GetChronosValueIndex() {
            float _scale = Time.timeScale;
            float _nearest = Mathf.Abs(_scale - StepChronos[0]);

            for (int _i = 1; _i < StepChronos.Length; _i++) {
                float _value = StepChronos[_i];
                float _difference = Mathf.Abs(_scale - _value);

                if (_difference > _nearest)
                    return _i - 1;

                _nearest = _difference;
            }

            return StepChronos.Length - 1;
        }
        #endregion
    }
}
