// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="PopupAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(PopupAttribute))]
    public class PopupPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            PopupAttribute _attribute = Attribute as PopupAttribute;
            EnhancedEditorGUI.PopupField(_position, _property, _label, _attribute.OptionMember);

            _height = _position.height;
            return true;
        }
        #endregion
    }
}
