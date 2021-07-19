// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="PropertyFieldAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(PropertyFieldAttribute))]
    public class PropertyFieldPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property)
        {
            PropertyFieldAttribute _attribute = (PropertyFieldAttribute)Attribute;
            if (!_attribute.Mode.IsActive())
                return;

            string _propertyName = string.IsNullOrEmpty(_attribute.PropertyName) ?
                                    (char.ToUpper(_property.name[0]) + _property.name.Substring(1)) :
                                    _attribute.PropertyName;

            EnhancedEditorGUIUtility.SetPropertyValue(_property, _propertyName);
        }
        #endregion
    }
}
