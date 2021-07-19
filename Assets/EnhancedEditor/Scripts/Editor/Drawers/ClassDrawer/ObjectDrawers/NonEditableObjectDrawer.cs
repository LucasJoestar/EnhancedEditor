// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="UnityObjectDrawer"/>) for classes with attribute <see cref="NonEditableAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(NonEditableAttribute))]
    public class NonEditableObjectDrawer : UnityObjectDrawer
    {
        #region Drawer Content
        private GUIContent labelGUI = GUIContent.none;
        private bool isInitialized = false;

        // -----------------------

        public override bool OnInspectorGUI()
        {
            if (!isInitialized)
            {
                labelGUI = (Attribute as NonEditableAttribute).Label;
                isInitialized = true;
            }

            Rect _position = EditorGUILayout.GetControlRect(true, Screen.height * .5f);
            EditorGUI.LabelField(_position, labelGUI, EnhancedEditorStyles.BigWordWrappedCenteredLabel);

            return false;
        }
        #endregion
    }
}
