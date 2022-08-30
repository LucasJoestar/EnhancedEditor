// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="FlagReference"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(FlagReference), true)]
    public class FlagReferencePropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.FlagReferenceField(_position, _property, _label);
            return _position.height;
        }
        #endregion
    }
}
