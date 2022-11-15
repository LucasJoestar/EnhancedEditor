// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="FlagReferenceGroup"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(FlagReferenceGroup), true)]
    public class FlagReferenceGroupPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            EnhancedEditorGUI.FlagReferenceGroupField(_position, _property, _label);
            return _position.height;
        }
        #endregion
    }
}
