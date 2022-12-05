// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="ValidationMemberAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(ValidationMemberAttribute))]
    public class ValidationMemberPropertyDrawer : EnhancedPropertyDrawer {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property) {
            ValidationMemberAttribute _attribute = Attribute as ValidationMemberAttribute;
            if (!_attribute.Mode.IsActive())
                return;

            EnhancedEditorGUI.SetValidationMemberValue(_property, _attribute.ValidationMember);
        }
        #endregion
    }
}
