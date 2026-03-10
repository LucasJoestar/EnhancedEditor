// ===== Enhanced Editor - https://github.com/TetsuoYoshima/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================= //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using Random = UnityEngine.Random;

namespace EnhancedEditor {
    /// <summary>
    /// Contains multiple random utility methods.
    /// </summary>
    #pragma warning disable
    public static class RandomUtility {
        /// <summary>
        /// Disposable scope used to perform predictable random operations within.
        /// <br/> Set a temporary seed on creation, and restore both previous seed and state when disposed.
        /// </summary>
        public readonly struct PredictableScope : IDisposable {
            #region Content
            private readonly int seed;
            private readonly Random.State state;

            // -------------------------------------------
            // Constructor
            // -------------------------------------------

            /// <param name="_hint">Hint used for generating a temporary seed within this scope.</param>
            /// <inheritdoc cref="PredictableScope"/>
            public PredictableScope(int _hint) {
                seed  = currentSeed;
                state = Random.state;

                SetRandomSeed(GetRandomSeed(_hint));
            }

            // -------------------------------------------
            // Disposable
            // -------------------------------------------

            void IDisposable.Dispose() {
                SetRandomSeed (seed);
                SetRandomState(state);
            }
            #endregion
        }

        /// <summary>
        /// <see cref="Random"/>-related global state wrapper.
        /// </summary>
        [Serializable]
        public struct State {
            #region Content
            [SerializeField] public List<Pair<int, Random.State>> TempStates; // Previous values: seed as first, state as second.
            [SerializeField] public int GameSeed;

            // -------------------------------------------
            // Constructor
            // -------------------------------------------

            internal State(int _gameSeed, int _currentSeed, List<Pair<int, Random.State>> _states) {
                TempStates = new List<Pair<int, Random.State>>(_states) {
                    new Pair<int, Random.State>(_currentSeed, Random.state)
                };

                GameSeed = _gameSeed;
            }
            #endregion
        }

        #region Operations
        // -------------------------------------------
        // Orientation Offset
        // -------------------------------------------

        /// <param name="_transform">Reference <see cref="Transform"/> used for both origin position and forward orientation.</param>
        /// <returns>Random position with applied offset.</returns>
        /// <inheritdoc cref="RandomOrientationOffset(Quaternion, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomOrientationOffset(Transform _transform, float _radius = 1f) {
            return _transform.position + RandomOrientationOffset(_transform.rotation, _radius);
        }

        /// <summary>
        /// Generates a random offset position from a given rotation on its X & Z axises.
        /// </summary>
        /// <param name="_rotation">Reference rotation used to get a random orientation on its horizontal and forward axises.</param>
        /// <param name="_radius">Radius used to get a random offset.</param>
        /// <returns>Random offset based on the given radius and rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomOrientationOffset(Quaternion _rotation, float _radius = 1f) {
            if (_radius == 0f) {
                return Vector3.zero;
            }

            return RandomOnCircle(_radius, _rotation);
        }

        // -------------------------------------------
        // Orientation
        // -------------------------------------------

        /// <param name="_transform">Reference <see cref="Transform"/> to use for base rotation..</param>
        /// <inheritdoc cref="RandomOrientation(Quaternion)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomOrientation(Transform _transform) {
            return RandomOrientation(_transform.rotation);
        }

        /// <summary>
        /// Generates a random orientation on the X & Z axises based on a given rotation.
        /// </summary>
        /// <param name="_rotation">Reference rotation used to get a random orientation on its horizontal and forward axises.</param>
        /// <returns>Random rotation on the X & Z axises based on the given rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomOrientation(Quaternion _rotation) {
            return RandomOnCircle(1f, _rotation);
        }

        // -------------------------------------------
        // Miscs
        // -------------------------------------------

        /// <param name="_rotation">3D rotation of this disc.</param>
        /// <inheritdoc cref="RandomOnCircle(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomOnCircle(float _offset, Quaternion _rotation) {
            return RandomOnCircle(_offset).ToVector3Depth().Rotate(_rotation);
        }

        /// <summary>
        /// Generates a random direction on the surface of a disc with given radius.
        /// </summary>
        /// <param name="_radius">Radius of the circle on which to get a random direction.</param>
        /// <returns>Random direction on the surface of this disc.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RandomOnCircle(float _radius = 1f) {
            return new Vector2(RandomValueNonNull(), RandomValueNonNull()).normalized * _radius;
        }

        /// <summary>
        /// Generates a random value between -1 and 1 guaranteed to not be equal to 0.
        /// </summary>
        /// <returns>Random value between -1 and 1 different than 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomValueNonNull() {
            float _value;
            do {
                _value = Random.Range(-1f, 1f);
            } while (Mathf.Approximately(_value, 0f));

            return _value;
        }
        #endregion

        #region Predictable
        /// <summary>
        /// A predictable version of <see cref="Random.Range(float, float)"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.Range(float, float)"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static float PredictableRange(int _hint, float _minInclusive, float _maxInclusive) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.Range(_minInclusive, _maxInclusive);
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.Range(int, int)"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.Range(int, int)"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static int PredictableRange(int _hint, int _minInclusive, int _maxExclusive) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.Range(_minInclusive, _maxExclusive);
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.value"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.value"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static float PredictableValue(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.value;
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.insideUnitCircle"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.insideUnitCircle"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static Vector2 PredictableInsideUnitCircle(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.insideUnitCircle;
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.insideUnitSphere"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.insideUnitSphere"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static Vector3 PredictableInsideUnitSphere(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.insideUnitSphere;
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.onUnitSphere"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.onUnitSphere"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static Vector3 PredictableOnUnitSphere(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.onUnitSphere;
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.rotation"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.rotation"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static Quaternion PredictableRotation(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.rotation;
            }
        }

        /// <summary>
        /// A predictable version of <see cref="Random.rotationUniform"/>, using a hint to generate a predictable value.
        /// <inheritdoc cref="Doc"/>
        /// <inheritdoc cref="Random.rotationUniform"/>
        /// </summary>
        /// <inheritdoc cref="Doc"/>
        public static Quaternion PredictableRotationUniform(int _hint) {
            using (var _scope = new PredictableScope(_hint)) {
                return Random.rotationUniform;
            }
        }

        // -------------------------------------------
        // Documentation
        // -------------------------------------------

        /// <summary>
        /// <br/> The value will always be the same when using the given hint with the same seed.
        /// <para/>
        /// See:<br/>
        /// </summary>
        /// <param name="_hint">Seed hint used to generate this random value.</param>
        /// <returns>Predictable generated random value.</returns>
        private static int Doc(int _hint) => 0;
        #endregion

        #region Seed
        private static List<Pair<int, Random.State>> tempStates = new List<Pair<int, Random.State>>(); // Previous values: seed as first, state as second.
        private static int currentSeed = 0;
        private static int gameSeed    = 0;

        public static Action<int> OnSetGameSeed = null;

        // -----------------------

        /// <summary>
        /// Set a temporary seed used for predictable random operations, based on the current game seed and a given hint.
        /// <br/> Use <see cref="EndTempSeed"/> to revert to the previous seed state.
        /// <para/>
        /// All random operations performed within in the same order will always produce the same result.
        /// </summary>
        /// <param name="_hint">Hint used for generating this temporary seed.</param>
        public static void BeginTempSeed(int _hint) {
            ref int _currentSeed = ref currentSeed;
            int _seed = GetRandomSeed(_hint);

            // Same seed - ignore.
            if (_currentSeed == _seed) {
                tempStates.Add(new Pair<int, Random.State>(0, default));
                return;
            }

            // New temporary seed.
            tempStates.Add(new Pair<int, Random.State>(_currentSeed, Random.state));
            SetRandomSeed(_seed);
        }

        /// <summary>
        /// Reverts to the previous seed state before the last <see cref="BeginTempSeed"/> call.
        /// </summary>
        public static void EndTempSeed() {
            ref var _span = ref tempStates;
            int _index = _span.Count - 1;

            if (_index == -1)
                return;

            var _pair = _span[_index];
            _span.RemoveAt(_index);

            // Same seed - ignore.
            if (_pair.First == 0)
                return;

            // Restore seed and state.
            SetRandomSeed (_pair.First);
            SetRandomState(_pair.Second);
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Set the global seed of the game.
        /// </summary>
        public static void SetGameSeed(int _seed) {
            gameSeed = _seed;
            SetRandomSeed(_seed);

            OnSetGameSeed?.Invoke(_seed);
        }

        /// <summary>
        /// Get a temporary seed to use for predictable random operation(s).
        /// <br/> Uses the current game seed as base.
        /// </summary>
        /// <param name="_hint">Hint used to get a tempoary seed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetRandomSeed(int _hint) {
            return (int)((gameSeed + _hint) / 2L);
        }

        /// <summary>
        /// Set the current <see cref="Random"/> seed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetRandomSeed(int _seed) {
            currentSeed = _seed;
            Random.InitState(_seed);
        }

        /// <summary>
        /// Set the current <see cref="Random.State"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetRandomState(Random.State _state) {
            Random.state = _state;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the current <see cref="Random"/> state of the game.
        /// </summary>
        public static State GetState() {
            return new State(gameSeed, currentSeed, tempStates);
        }

        /// <summary>
        /// Set the current <see cref="Random"/> state of the game.
        /// </summary>
        public static void SetState(State _state) {
            // Game seed.
            SetGameSeed(_state.GameSeed);

            // Temporary seeds and states.
            ref var _span = ref tempStates;
            _span.ReplaceBy(_state.TempStates);

            int _count = tempStates.Count;
            if (_count != 0) { // Last is current.

                var _pair = _span[_count - 1];
                _span.RemoveAt(_count - 1);

                SetRandomSeed (_pair.First);
                SetRandomState(_pair.Second);
            }
        }
        #endregion
    }
}
