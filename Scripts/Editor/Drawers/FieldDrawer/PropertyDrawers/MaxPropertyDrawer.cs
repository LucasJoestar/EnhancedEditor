// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="MaxAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(MaxAttribute))]
    public class MaxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property)
        {
            MaxAttribute _attribute = Attribute as MaxAttribute;
            float _maxValue;

            // Get maximum allowed value and ceil property value.
            if (_attribute.MaxMember == null)
            {
                _maxValue = _attribute.MaxValue;
            }
            else if (!_attribute.MaxMember.Value.GetValue(_property, out _maxValue))
                return;

            EnhancedEditorUtility.CeilSerializedPropertyValue(_property, _maxValue);
        }
        #endregion
    }
}
