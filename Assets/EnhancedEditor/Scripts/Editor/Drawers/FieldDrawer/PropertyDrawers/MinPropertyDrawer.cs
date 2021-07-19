// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="MinAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(MinAttribute))]
    public class MinPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property)
        {
            MinAttribute _attribute = (MinAttribute)Attribute;
            EnhancedEditorGUIUtility.FloorValue(_property, _attribute.MinValue);
        }
        #endregion
    }
}
