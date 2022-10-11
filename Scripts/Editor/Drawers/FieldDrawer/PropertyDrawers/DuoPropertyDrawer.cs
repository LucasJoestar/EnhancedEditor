// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="DuoAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(DuoAttribute))]
    public class DuoPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            DuoAttribute _attribute = Attribute as DuoAttribute;
            EnhancedEditorGUI.DuoField(_position, _property, _label, _attribute.FieldName, _attribute.FieldWidth, out float _extraHeight);

            _height = _position.height + _extraHeight;
            return true;
        }
        #endregion
    }
}
