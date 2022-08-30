// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="FlagBooleanAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(FlagBooleanAttribute))]
    public class FlagBooleanPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            EnhancedEditorGUI.BoolPopupField(_position, _property, _label);

            _height = _position.height;
            return true;
        }
        #endregion
    }
}
