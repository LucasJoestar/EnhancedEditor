// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="ScriptableObject"/> drawer, with the ability to easily create, clone and edit instances.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ScriptableObjectPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const float SingleButtonWidth = 70f;
        private const float DoubleButtonWidth = 50f;

        private static readonly GUIContent newGUI = new GUIContent("Create", "Creates a new instance of this object.");
        private static readonly GUIContent cloneGUI = new GUIContent("Clone", "Creates a new instance of this object and copy the values of the assigned object into it.");

        // -----------------------

        internal protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            ScriptableObject _object = _property.objectReferenceValue as ScriptableObject;
            if (_object == null) {
                Rect _temp = new Rect(_position) {
                    xMin = _position.xMax - SingleButtonWidth
                };

                _position.xMax = _temp.xMin;

                if (GUI.Button(_temp, newGUI)) {

                }
            } else {
                Rect _temp = new Rect(_position) {
                    xMin = _position.xMax - (DoubleButtonWidth * 2f),
                    width = DoubleButtonWidth
                };

                _position.xMax = _temp.xMin;

                if (GUI.Button(_temp, newGUI)) {

                }

                _temp.x += _temp.width;

                if (GUI.Button(_temp, cloneGUI)) {

                }
            }

            using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                bool _foldout = EditorGUI.Foldout(_position, _property.isExpanded, GUIContent.none);

                if (_changeCheck.changed) {
                    _property.isExpanded = _foldout;
                }
            }

            // Draw base property field.
            EditorGUI.PropertyField(_position, _property, _label);
            return _position.height;
        }
        #endregion
    }
}
