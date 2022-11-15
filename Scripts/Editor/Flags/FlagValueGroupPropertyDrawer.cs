// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="FlagValueGroup"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(FlagValueGroup), true)]
    public class FlagValueGroupPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.FlagValueGroupField(_position, _property, _label);
            return _position.height;
        }
        #endregion
    }
}
