// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="ScriptableObjectOptionAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ScriptableObjectOptionAttribute))]
    public sealed class ScriptableObjectOptionPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override bool OnGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height) {
            var _attribute = Attribute as ScriptableObjectOptionAttribute;

            EnhancedEditorGUI.ScriptableObjectContentField(_position, _property, _attribute.Mode, out float _extraHeight, _attribute.DrawField);
            _height = _extraHeight;

            if (_attribute.DrawField || (_property.objectReferenceValue == null)) {
                _height += _position.height;
            } else {
                _height -= EditorGUIUtility.standardVerticalSpacing;
            }

            return true;
        }
        #endregion
    }
}
