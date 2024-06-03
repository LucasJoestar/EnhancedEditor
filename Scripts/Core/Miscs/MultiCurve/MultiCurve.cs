// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Utility class used to group and edit multiple <see cref="AnimationCurve"/> all together.
    /// </summary>
    [Serializable]
    public sealed class MultiCurve {
        #region Curve
        /// <summary>
        /// <see cref="MultiCurve"/>-related single curve wrapper.
        /// </summary>
        [Serializable]
        #pragma warning disable
        public sealed class Curve {
            public AnimationCurve AnimationCurve = new AnimationCurve();
            public string Label = string.Empty;
            public Color Color  = SuperColor.HarvestGold.Get();

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            /// <inheritdoc cref="Curve(AnimationCurve, string, Color)"/>
            public Curve(string _label, Color _color) : this(AnimationCurve.Linear(0f, 0f, 1f, 1f), _label, _color) { }

            /// <param name="_label"><inheritdoc cref="Label" path="/summary"/></param>
            /// <param name="_color"><inheritdoc cref="Color" path="/summary"/></param>
            /// <param name="_curve"><inheritdoc cref="AnimationCurve" path="/summary"/></param>
            /// <inheritdoc cref="Curve"/>
            public Curve(AnimationCurve _curve, string _label, Color _color) {
                Label = _label;
                Color = _color;
                AnimationCurve = _curve;
            }
        }
        #endregion

        #region Global Members
        /// <summary>
        /// Editor-purpose unique id of this curve.
        /// </summary>
        internal int id = EnhancedUtility.GenerateGUID();

        /// <summary>
        /// All <see cref="Curve"/> wrapped in this object.
        /// </summary>
        public List<Curve> Curves = new List<Curve>();

        /// <summary>
        /// Editing range of those curves.
        /// </summary>
        public Vector2 Range = new Vector2(0f, 1f);

        /// <summary>
        /// Field size when drawing those curves.
        /// </summary>
        public Vector2 FieldSize = new Vector2(1f, 1f);

        /// <summary>
        /// Whether to display or not the header above those curves.
        /// </summary>
        public bool DisplayHeader = true;

        /// <summary>
        /// Whether to display or not the legend below those curves.
        /// </summary>
        public bool DisplayLegend = true;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="MultiCurve(IList{Curve}, Vector2, bool, bool, float, float)"/>
        public MultiCurve(IList<Curve> _curves) {
            Curves.Clear();
            Curves.AddRange(_curves);
        }

        /// <inheritdoc cref="MultiCurve(IList{Curve}, Vector2, bool, bool, float, float)"/>
        public MultiCurve(params Curve[] _curves) : this((IList<Curve>)_curves) { }

        /// <param name="_curves"><inheritdoc cref="Curves" path="/summary"/></param>
        /// <param name="_displayHeader"><inheritdoc cref="DisplayHeader" path="/summary"/></param>
        /// <param name="_displayLegend"><inheritdoc cref="DisplayLegend" path="/summary"/></param>
        /// <inheritdoc cref="MultiCurve"/>
        public MultiCurve(IList<Curve> _curves, Vector2 _range, bool _displayHeader, bool _displayLegend = true, float _width = 1f, float _height = 1f) : this(_curves) {
            Range = _range;
            DisplayHeader = _displayHeader;
            DisplayLegend = _displayLegend;

            FieldSize = new Vector2(_width, _height);
        }
        #endregion

        #region Operator
        public AnimationCurve this[int _index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Curves[_index].AnimationCurve;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Curves[_index].AnimationCurve = value;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Evaluates a curve at the give index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(int index, float percent) {
            return Curves[index].AnimationCurve.Evaluate(percent);
        }
        #endregion
    }
}
