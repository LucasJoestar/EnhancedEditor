// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

#if ENABLE_INPUT_SYSTEM && CHRONOS_INPUTS
using System;
using UnityEngine.InputSystem;
#endif

namespace EnhancedEditor
{
    /// <summary>
    /// Manipulates the global game time scale, with the support of custom inputs at runtime.
    /// </summary>
    #if ENABLE_INPUT_SYSTEM
    [ScriptingDefineSymbol("CHRONOS_INPUTS", "Chronos Runtime Inputs")]
    #endif
    public static class Chronos
    {
        #region Global Members
        internal const string IncreaseInputKey = "Chronos_IncreaseInput";
        internal const string ResetInputKey = "Chronos_ResetInput";
        internal const string DecreaseInputKey = "Chronos_DecreaseInput";

        private static readonly float[] chronosValues = new float[]
                                                            {
                                                                .1f, .2f, .25f, .5f, .75f,
                                                                1f,
                                                                2f, 4f, 8f, 16f, 32f
                                                            };

        // -----------------------

        [RuntimeInitializeOnLoadMethod]
        private static void Initialization()
        {
            #if ENABLE_INPUT_SYSTEM && CHRONOS_INPUTS
            CreateInput(IncreaseInputKey, Keyboard.current.numpadPlusKey, Increase);
            CreateInput(ResetInputKey, Keyboard.current.numpadMultiplyKey, Reset);
            CreateInput(DecreaseInputKey, Keyboard.current.numpadMinusKey, Decrease);

            // ----- Local Method ----- \\

            void CreateInput(string _prefsKey, InputControl _defaultControl, Action _onPerformed)
            {
                string _value = PlayerPrefs.GetString(_prefsKey, string.Empty);
                InputAction _input;

                if (string.IsNullOrEmpty(_value))
                {
                    _input = new InputAction();
                    _input.AddBinding(_defaultControl);
                }
                else
                {
                    _input = JsonUtility.FromJson<InputAction>(_value);
                }

                _input.performed += (InputAction.CallbackContext _c) =>
                {
                    if (Application.isFocused)
                    {
                        _onPerformed();
                    }
                };

                _input.Enable();
            }
            #endif
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Increases the game time scale.
        /// </summary>
        public static void Increase()
        {
            int _index = Mathf.Min(GetChronosValueIndex() + 1, chronosValues.Length - 1);
            Time.timeScale = chronosValues[_index];
        }

        /// <summary>
        /// Resets the game time scale to 1.
        /// </summary>
        public static void Reset()
        {
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Decreases the game time scale.
        /// </summary>
        public static void Decrease()
        {
            int _index = Mathf.Max(GetChronosValueIndex() - 1, 0);
            Time.timeScale = chronosValues[_index];
        }

        // -----------------------

        private static int GetChronosValueIndex()
        {
            float _scale = Time.timeScale;
            float _nearest = Mathf.Abs(_scale - chronosValues[0]);

            for (int _i = 1; _i < chronosValues.Length; _i++)
            {
                float _value = chronosValues[_i];
                float _difference = Mathf.Abs(_scale - _value);

                if (_difference > _nearest)
                    return _i - 1;

                _nearest = _difference;
            }

            return chronosValues.Length - 1;
        }
        #endregion
    }
}
