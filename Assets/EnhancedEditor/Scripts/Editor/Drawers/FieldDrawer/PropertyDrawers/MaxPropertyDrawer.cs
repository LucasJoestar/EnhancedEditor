// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="MaxAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(MaxAttribute))]
    public class MaxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property)
        {
            MaxAttribute _attribute = (MaxAttribute)Attribute;
            EnhancedEditorGUIUtility.CeilValue(_property, _attribute.MaxValue);
        }
        #endregion
    }
}
