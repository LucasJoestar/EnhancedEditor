// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for classes with the attribute <see cref="NonEditableAttribute"/> (inherit from <see cref="UnityObjectDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(NonEditableAttribute))]
    public class NonEditableObjectDrawer : UnityObjectDrawer
    {
        #region Drawer Content
        public override bool OnInspectorGUI()
        {
            var _attribute = Attribute as NonEditableAttribute;

            GUILayout.Space(100f);
            EditorGUILayout.LabelField(_attribute.Label, EnhancedEditorStyles.BigWordWrappedCenteredLabel);

            GUILayout.Space(50f);
            return true;
        }
        #endregion
    }
}
