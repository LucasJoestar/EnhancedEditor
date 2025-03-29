// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="MultiCurve"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(MultiCurve), true)]
    public sealed class MultiCurvePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            _position.height = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.MultiCurveField(_position, _property, _label, out float _extraHeight);
            return _position.height + _extraHeight;
        }
        #endregion
    }
}
