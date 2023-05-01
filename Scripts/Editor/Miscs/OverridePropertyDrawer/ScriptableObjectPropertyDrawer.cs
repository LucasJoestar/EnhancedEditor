// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="ScriptableObject"/> drawer, allowing to quickly create and duplicate instances,
    /// <br/> and drawing its script content within a folder.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ScriptableObjectPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {

            _position.height = EditorGUIUtility.singleLineHeight;

            EnhancedEditorGUI.ScriptableObjectContentField(_position, _property, _label, out float _extraHeight, true);
            return _position.height + _extraHeight;
        }
        #endregion
    }
}
