// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

#if INPUT_SYSTEM_PACKAGE && ENABLE_INPUT_SYSTEM && CHRONOS_INPUTS
using System;
using UnityEngine.InputSystem;
#endif

namespace EnhancedEditor
{
    /// <summary>
    /// Manipulates the global game time scale with the support of keyboard shortcuts.
    /// </summary>
    #if INPUT_SYSTEM_PACKAGE && ENABLE_INPUT_SYSTEM
    [ScriptingDefineSymbol("CHRONOS_INPUTS", "Chronos Input Shortcuts [Keyboard Numpad - * +]")]
    #endif
    public static class Chronos
    {
        #region Global Members
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
            #if INPUT_SYSTEM_PACKAGE && ENABLE_INPUT_SYSTEM && CHRONOS_INPUTS
            CreateInput(Keyboard.current.numpadMinusKey, Decrease);
            CreateInput(Keyboard.current.numpadMultiplyKey, Reset);
            CreateInput(Keyboard.current.numpadPlusKey, Increase);

            // ----- Local Method ----- \\

            void CreateInput(InputControl _defaultControl, Action _onPerformed)
            {
                InputAction _input = new InputAction();
                _input.AddBinding(_defaultControl);

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
        /// Decreases the game time scale.
        /// </summary>
        public static void Decrease()
        {
            int _index = Mathf.Max(GetChronosValueIndex() - 1, 0);
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
        /// Increases the game time scale.
        /// </summary>
        public static void Increase()
        {
            int _index = Mathf.Min(GetChronosValueIndex() + 1, chronosValues.Length - 1);
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
