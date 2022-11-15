// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="Flag"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(Flag), true)]
    public class FlagPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.FlagField(_position, _property, GUIContent.none);
            return _position.height;
        }
        #endregion
    }
}
