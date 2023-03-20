// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="AxisConstraints"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(AxisConstraints), true)]
    public class AxisConstraintsPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private static readonly Pair<AxisConstraints, GUIContent>[] constraints = new Pair<AxisConstraints, GUIContent>[] {
            new Pair<AxisConstraints, GUIContent>(AxisConstraints.X, new GUIContent("X")),
            new Pair<AxisConstraints, GUIContent>(AxisConstraints.Y, new GUIContent("Y")),
            new Pair<AxisConstraints, GUIContent>(AxisConstraints.Z, new GUIContent("Z"))
        };

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            AxisConstraints _value = (AxisConstraints)_property.intValue;

            _position = EditorGUI.PrefixLabel(_position, _label);

            using (var _scope = EnhancedEditorGUI.ZeroIndentScope()) {
                foreach (var _constraint in constraints) {
                    DrawConstraint(_constraint);
                }
            }

            _property.intValue = (int)_value;
            return _height;

            // ----- Local Method ----- \\

            void DrawConstraint(Pair<AxisConstraints, GUIContent> _constraint) {
                AxisConstraints _axis = _constraint.First;
                GUIContent _label = _constraint.Second;

                bool _enabled = (_value & _axis) == _axis;
                _position.width = EnhancedEditorGUIUtility.FoldoutWidth;

                using (var _scope = new EditorGUI.ChangeCheckScope()) {
                    _enabled = EditorGUI.Toggle(_position, _enabled);

                    if (_scope.changed) {
                        _value = _enabled
                               ? (_value | _axis)
                               : (_value & ~_axis);
                    }
                }

                _position.x += _position.width + 2f;
                _position.width = EditorStyles.label.CalcSize(_label).x;

                EditorGUI.LabelField(_position, _label);

                _position.x += _position.width + 3f;
            }
        }
        #endregion
    }
}
