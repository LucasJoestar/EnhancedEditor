// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="FlagValue"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(FlagValue), true)]
    public class FlagValuePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.FlagValueField(_position, _property, _label);
            return _position.height;
        }
        #endregion
    }
}
